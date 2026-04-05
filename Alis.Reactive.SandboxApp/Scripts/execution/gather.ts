// gather.ts — Gather form values using the SHARED resolver.
// Reads component default values via plan.types[].defaultValue.
// Supports GatherInput (component fields) and ValueInput (pre-computed value).

import type { Plan, GatherInput, RequestInput, Transport } from "../types";
import type { ExecContext } from "../types";
import { readDefaultValue, getJsType } from "../resolution/resolver";
import { applyShape, toString } from "../core/coerce";
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

  for (const field of gatherInput.components) {
    const raw = readDefaultValue(plan, field.component);
    const defaultShape = getDefaultShape(plan, field.component);
    const value = applyShape(raw, defaultShape);
    emitValue(field.key, value, transport);
  }

  return { urlParams, body: formData ?? body };
}

/** Get the default value's shape from the JsType. */
function getDefaultShape(plan: Plan, componentKey: string): import("../types").Shape | undefined {
  const jsType = getJsType(plan, componentKey);
  return jsType.defaultValue?.shape;
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
