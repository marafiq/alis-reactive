// gather.ts — Gather form values using the SHARED resolver.
// Reads component default values via plan.types[].defaultValue.
// Supports GatherInput (component fields) and ValueInput (pre-computed value).

import type { Plan, GatherInput, RequestInput, Transport } from "../types";
import type { ExecContext } from "../types";
import { readDefaultValue, getJsType } from "../resolution/resolver";
import { applyShape, toString } from "../core/shape-convert";
import { scope } from "../core/trace";
import { evaluateValue } from "./execute";

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
  emitScalar(name: string, value: unknown): void;
  emitArray(name: string, items: unknown[]): void;
}

function createGetTransport(urlParams: string[]): TransportStrategy {
  return {
    emitScalar: (name, value) => urlParams.push(
      `${encodeURIComponent(name)}=${encodeURIComponent(serializeValue(value, name))}`),
    emitArray: (name, items) => {
      if (hasFiles(items)) throw new Error("[alis] File objects cannot be sent via GET");
      for (const item of items)
        urlParams.push(`${encodeURIComponent(name)}=${encodeURIComponent(serializeValue(item, name))}`);
    },
  };
}

function createFormDataTransport(formData: FormData): TransportStrategy {
  return {
    emitScalar: (name, value) => formData.append(name, serializeValue(value, name)),
    emitArray: (name, items) => {
      for (const item of items) {
        const file = toFile(item);
        if (file) formData.append(name, file, file.name);
        else formData.append(name, serializeValue(item, name));
      }
    },
  };
}

function createJsonTransport(body: Record<string, unknown>): TransportStrategy {
  return {
    emitScalar: (name, value) => setNested(body, name, value === "" ? null : value),
    emitArray: (name, items) => {
      if (hasFiles(items)) throw new Error("[alis] File objects require transport: form-data");
      setNested(body, name, items);
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

function emitValue(name: string, raw: unknown, transport: TransportStrategy): void {
  if (typeof FileList !== "undefined" && raw instanceof FileList) {
    transport.emitArray(name, Array.from(raw));
    log.trace("file", { name, count: raw.length });
    return;
  }
  if (Array.isArray(raw)) {
    transport.emitArray(name, raw);
  } else {
    transport.emitScalar(name, raw);
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
    // ValueInput — evaluate the value producer directly
    const value = evaluateValue(input.value, plan, ctx);
    const formData = input.transport === "form-data" ? new FormData() : null;
    const transport = selectTransport(input.transport, method, urlParams, formData, body);

    if (typeof value === "object" && value !== null && !Array.isArray(value)) {
      for (const [key, val] of Object.entries(value as Record<string, unknown>)) {
        emitValue(key, val, transport);
      }
    } else {
      // Single value — emit as "value"
      emitValue("value", value, transport);
    }

    return { urlParams, body: formData ?? body };
  }

  // GatherInput — read component default values
  const gatherInput = input as GatherInput;
  const formData = gatherInput.transport === "form-data" ? new FormData() : null;
  const transport = selectTransport(gatherInput.transport, method, urlParams, formData, body);

  const gatheredComponents = new Set<string>();
  let includeAll = false;
  for (const field of gatherInput.components) {
    // Sentinel field from C# IncludeAll() — signals dynamic inclusion
    if (field.component === "__include_all__") {
      includeAll = true;
      continue;
    }
    gatheredComponents.add(field.component);
    const raw = readDefaultValue(plan, field.component);
    const defaultShape = getDefaultShape(plan, field.component);
    let value = applyShape(raw, defaultShape);
    // Date shape produces epoch ms (number) for condition comparison,
    // but HTTP gather needs ISO strings for ASP.NET DateTime deserialization.
    // Handle direct date, nullable date, and array-of-date shapes.
    value = coerceDateForTransport(value, defaultShape);
    emitValue(field.key, value, transport);
  }

  // Emit static/event values merged alongside component fields
  if (gatherInput.statics) {
    const staticValues = evaluateValue(gatherInput.statics, plan, ctx);
    if (typeof staticValues === "object" && staticValues !== null && !Array.isArray(staticValues)) {
      for (const [key, val] of Object.entries(staticValues as Record<string, unknown>)) {
        emitValue(key, val, transport);
      }
    }
  }

  // Include dynamically-merged components (from partial plan injection).
  // Only when IncludeAll was used (sentinel present) — selective gathers
  // must NOT auto-include extra components.
  if (includeAll) {
    for (const [compKey, comp] of Object.entries(plan.components)) {
      if (gatheredComponents.has(compKey)) continue;
      const jsType = plan.types[comp.type];
      if (!jsType?.defaultValue) continue;
      const bindingPath = deriveBindingPath(compKey);
      if (!bindingPath) continue;
      // Skip components whose DOM elements are not present — they may be
      // conditionally rendered (e.g., diagnosis-specific fields in a wizard).
      if (!document.getElementById(comp.id)) continue;
      const raw = readDefaultValue(plan, compKey);
      const defaultShape = getDefaultShape(plan, compKey);
      let value = applyShape(raw, defaultShape);
      value = coerceDateForTransport(value, defaultShape);
      emitValue(bindingPath, value, transport);
    }
  }

  return { urlParams, body: formData ?? body };
}

/** Returns true if the shape (possibly wrapped in nullable) is a date shape. */
function isDateShape(shape?: import("../types").Shape): boolean {
  if (!shape) return false;
  if (shape.kind === "date") return true;
  if (shape.kind === "nullable" && shape.inner?.kind === "date") return true;
  return false;
}

/** Returns true if the shape is an array whose item shape is a date. */
function isDateArrayShape(shape?: import("../types").Shape): boolean {
  if (!shape) return false;
  if (shape.kind === "array" && shape.item?.kind === "date") return true;
  return false;
}

/** Convert epoch ms numbers to ISO strings for date-shaped values so ASP.NET can bind them. */
function coerceDateForTransport(value: unknown, shape?: import("../types").Shape): unknown {
  if (isDateShape(shape)) {
    if (typeof value === "number" && !isNaN(value)) return new Date(value).toISOString();
    return value;
  }
  if (isDateArrayShape(shape) && Array.isArray(value)) {
    return value.map(v => typeof v === "number" && !isNaN(v) ? new Date(v).toISOString() : v);
  }
  return value;
}

/** Get the default value's shape from the JsType. */
function getDefaultShape(plan: Plan, componentKey: string): import("../types").Shape | undefined {
  const jsType = getJsType(plan, componentKey);
  return jsType.defaultValue?.shape;
}

/**
 * Derive the model binding path from a component key (element ID).
 * Format: {Namespace_Type}__PropertyPath → PropertyPath with _ → .
 * Returns null if the key doesn't follow the convention.
 */
function deriveBindingPath(componentKey: string): string | null {
  const sep = componentKey.indexOf("__");
  if (sep < 0) return null;
  return componentKey.substring(sep + 2).replace(/_/g, ".");
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
