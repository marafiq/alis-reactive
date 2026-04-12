// gather.ts — Gather values for HTTP requests using the SHARED value concept.
// Every field carries a ValueProducer — evaluated via evaluateValue().
// No parallel read path. Shape flows from plan → transport for wire formatting.

import type { Plan, GatherInput, RequestInput, Transport, Shape } from "../types";
import type { ExecContext } from "../types";
import { resolveComponent, readProperty } from "../resolution/resolver";
import { applyShape, toString } from "../core/shape-convert";
import { scope } from "../core/trace";
import { evaluateValue } from "../core/evaluate";
import { formatForWire } from "../core/wire-format";

const log = scope("gather");

export interface GatherResult {
  urlParams: string[];
  body: Record<string, unknown> | FormData;
}

/** Extracts a File from a value — handles raw File objects and wrapper objects with .rawFile. */
function toFile(item: unknown): File | null {
  if (item instanceof File) return item;
  if (item != null && typeof item === "object" && "rawFile" in item && (item as any).rawFile instanceof File)
    return (item as any).rawFile;
  return null;
}

/** Returns true if any item in the array is or wraps a File. */
function hasFiles(items: unknown[]): boolean {
  return items.some(item => toFile(item) != null);
}

/** Unwrap toString Result — returns empty string on Err and logs a warning. */
function serializeValue(value: unknown, name: string): string {
  const result = toString(value);
  if (!result.ok) {
    log.warn("gather serialize failed, using empty", { name, error: result.error });
    return "";
  }
  return result.value;
}

/** Transport strategies for emitting name/value pairs into GET, FormData, or JSON. */
interface TransportStrategy {
  emitScalar(name: string, value: unknown, shape?: Shape): void;
  emitArray(name: string, items: unknown[], itemShape?: Shape): void;
}

function createGetTransport(urlParams: string[]): TransportStrategy {
  return {
    emitScalar: (name, value, shape) => {
      const wire = formatForWire(value, shape);
      urlParams.push(`${encodeURIComponent(name)}=${encodeURIComponent(serializeValue(wire, name))}`);
    },
    emitArray: (name, items, itemShape) => {
      if (hasFiles(items)) throw new Error("[alis] File objects cannot be sent via GET");
      for (const item of items) {
        const wire = formatForWire(item, itemShape);
        urlParams.push(`${encodeURIComponent(name)}=${encodeURIComponent(serializeValue(wire, name))}`);
      }
    },
  };
}

function createFormDataTransport(formData: FormData): TransportStrategy {
  return {
    emitScalar: (name, value, shape) => {
      const wire = formatForWire(value, shape);
      formData.append(name, serializeValue(wire, name));
    },
    emitArray: (name, items, itemShape) => {
      for (const item of items) {
        const file = toFile(item);
        if (file) formData.append(name, file, file.name);
        else {
          const wire = formatForWire(item, itemShape);
          formData.append(name, serializeValue(wire, name));
        }
      }
    },
  };
}

function createJsonTransport(body: Record<string, unknown>): TransportStrategy {
  return {
    emitScalar: (name, value, shape) => {
      const wire = formatForWire(value, shape);
      setNested(body, name, wire === "" ? null : wire);
    },
    emitArray: (name, items, itemShape) => {
      if (hasFiles(items)) throw new Error("[alis] File objects require transport: form-data");
      const wireItems = itemShape
        ? items.map(v => formatForWire(v, itemShape))
        : items;
      setNested(body, name, wireItems);
    },
  };
}

function selectTransport(
  transport: Transport, method: string, urlParams: string[], formData: FormData | null, body: Record<string, unknown>,
): TransportStrategy {
  if (method === "GET") return createGetTransport(urlParams);
  if (transport === "form-data" && formData) return createFormDataTransport(formData);
  return createJsonTransport(body);
}

function emitValue(name: string, raw: unknown, shape: Shape | undefined, transport: TransportStrategy): void {
  if (typeof FileList !== "undefined" && raw instanceof FileList) {
    transport.emitArray(name, Array.from(raw), shape);
    log.trace("file", { name, count: raw.length });
    return;
  }
  if (Array.isArray(raw)) {
    const itemShape = shape?.kind === "array" ? shape.item : undefined;
    transport.emitArray(name, raw, itemShape);
  } else {
    transport.emitScalar(name, raw, shape);
  }
  log.trace("gathered", { name, value: raw });
}

/**
 * Resolve gather input into GatherResult (urlParams + body/FormData).
 */
export function resolveGather(
  input: RequestInput | undefined,
  method: string,
  plan: Plan,
  ctx?: ExecContext,
): GatherResult {
  const urlParams: string[] = [];
  const body: Record<string, unknown> = {};

  if (!input) return { urlParams, body };

  if (input.kind === "value") {
    return resolveValueInput(input, method, urlParams, body, plan, ctx);
  }

  return resolveGatherInput(input as GatherInput, method, urlParams, body, plan, ctx);
}

/** ValueInput — evaluate the value producer directly and emit as object fields or single value. */
function resolveValueInput(
  input: Extract<RequestInput, { kind: "value" }>,
  method: string, urlParams: string[], body: Record<string, unknown>,
  plan: Plan, ctx?: ExecContext,
): GatherResult {
  const value = evaluateValue(input.value, plan, ctx);
  const formData = input.transport === "form-data" ? new FormData() : null;
  const transport = selectTransport(input.transport, method, urlParams, formData, body);

  if (typeof value === "object" && value !== null && !Array.isArray(value)) {
    for (const [key, val] of Object.entries(value as Record<string, unknown>)) {
      emitValue(key, val, undefined, transport);
    }
  } else {
    emitValue("value", value, undefined, transport);
  }

  return { urlParams, body: formData ?? body };
}

/** GatherInput — each field carries a ValueProducer. Handles components, statics, and includeAll. */
function resolveGatherInput(
  gatherInput: GatherInput, method: string, urlParams: string[], body: Record<string, unknown>,
  plan: Plan, ctx?: ExecContext,
): GatherResult {
  const formData = gatherInput.transport === "form-data" ? new FormData() : null;
  const transport = selectTransport(gatherInput.transport, method, urlParams, formData, body);

  const gatheredComponents = gatherExplicitFields(gatherInput, transport, plan, ctx);
  emitStaticValues(gatherInput, transport, plan, ctx);

  if (gatherInput.includeAll) {
    gatherDynamicComponents(plan, gatheredComponents, transport);
  }

  return { urlParams, body: formData ?? body };
}

/** Gather explicit component fields — each carries a ValueProducer with shape. Returns tracked component keys. */
function gatherExplicitFields(
  gatherInput: GatherInput, transport: TransportStrategy, plan: Plan, ctx?: ExecContext,
): Set<string> {
  const gatheredComponents = new Set<string>();
  for (const field of gatherInput.components) {
    if (field.value.kind === "read" && field.value.from.kind === "component") {
      gatheredComponents.add(field.value.from.component);
    }
    const raw = evaluateValue(field.value, plan, ctx);
    const shape = "shape" in field.value ? field.value.shape : undefined;
    emitValue(field.key, raw, shape, transport);
  }
  return gatheredComponents;
}

/** Emit static/event values merged alongside component fields. */
function emitStaticValues(
  gatherInput: GatherInput, transport: TransportStrategy, plan: Plan, ctx?: ExecContext,
): void {
  if (!gatherInput.statics) return;
  const staticValues = evaluateValue(gatherInput.statics, plan, ctx);
  if (typeof staticValues !== "object" || staticValues === null || Array.isArray(staticValues)) return;
  for (const [key, val] of Object.entries(staticValues as Record<string, unknown>)) {
    emitValue(key, val, undefined, transport);
  }
}

/**
 * IncludeAll: gather dynamically-merged components from partial plan injection.
 * The C# builder expands all KNOWN components at build time. This loop catches
 * components added AFTER build time via partial plan merge.
 */
function gatherDynamicComponents(
  plan: Plan, gatheredComponents: Set<string>, transport: TransportStrategy,
): void {
  for (const [compKey, comp] of Object.entries(plan.components)) {
    if (gatheredComponents.has(compKey)) continue;
    if (!comp.valueMember || !comp.bindingPath) continue;
    if (!document.getElementById(comp.id)) continue;
    const jsType = plan.types[comp.type];
    const prop = jsType?.properties?.[comp.valueMember];
    if (!prop) continue;
    const root = resolveComponent(plan, compKey);
    const raw = readProperty(root, prop);
    const value = applyShape(raw, prop.shape);
    emitValue(comp.bindingPath, value, prop.shape, transport);
  }
}

function setNested(obj: Record<string, unknown>, key: string, value: unknown): void {
  const parts = key.split(".");
  if (parts.length === 1) {
    obj[key] = value;
    return;
  }
  let cur = obj;
  for (let i = 0; i < parts.length - 1; i++) {
    const p = parts[i];
    if (!(p in cur) || typeof cur[p] !== "object" || cur[p] === null) {
      cur[p] = {};
    }
    cur = cur[p] as Record<string, unknown>;
  }
  cur[parts[parts.length - 1]] = value;
}
