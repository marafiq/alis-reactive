import type { GatherItem, ComponentEntry } from "../types";
import { evalRead } from "../resolution/component";
import { walk } from "../core/walk";
import { scope } from "../core/trace";
import { assertNever } from "../core/assert-never";
import { toString } from "../core/coerce";

const log = scope("gather");

/** Unwrap toString Result — returns empty string on Err and logs a warning. */
function serializeValue(value: unknown, name: string): string {
  const result = toString(value);
  if (!result.ok) {
    log.warn("gather serialize failed, using empty", { name, error: result.error });
    return "";
  }
  return result.value;
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

export interface GatherResult {
  urlParams: string[];
  body: Record<string, unknown> | FormData;
}

/**
 * Transport — the single place that knows how to emit a name/value pair
 * into one of three formats (GET params, FormData, JSON body).
 * Array and scalar values share this path — arrays expand into
 * repeated entries for GET/FormData, and pass through as-is for JSON.
 */
interface Transport {
  emitScalar(name: string, value: unknown): void;
  emitArray(name: string, items: unknown[]): void;
}

function createTransport(
  urlParams: string[],
  formData: FormData | null,
  body: Record<string, unknown>
): Transport {
  if (formData) {
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
  return {
    emitScalar: (name, value) => urlParams.push(
      `${encodeURIComponent(name)}=${encodeURIComponent(serializeValue(value, name))}`),
    emitArray: (name, items) => {
      if (hasFiles(items))
        throw new Error("[alis] File objects cannot be sent via GET");
      for (const item of items)
        urlParams.push(`${encodeURIComponent(name)}=${encodeURIComponent(serializeValue(item, name))}`);
    },
  };
}

function createJsonTransport(body: Record<string, unknown>): Transport {
  return {
    emitScalar: (name, value) => setNested(body, name, value === "" ? null : value),
    emitArray: (name, items) => {
      if (hasFiles(items))
        throw new Error("[alis] File objects require contentType: form-data");
      setNested(body, name, items);
    },
  };
}

function emitValue(name: string, raw: unknown, transport: Transport): void {
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
  log.trace("component", { name, value: raw });
}

function selectTransport(
  verb: string, urlParams: string[], formData: FormData | null, body: Record<string, unknown>
): Transport {
  if (verb === "GET") return createTransport(urlParams, null, body);
  if (formData) return createTransport(urlParams, formData, body);
  return createJsonTransport(body);
}

function emitAllComponents(
  components: Record<string, ComponentEntry>, transport: Transport
): void {
  if (Object.keys(components).length === 0) {
    throw new Error(
      "[alis] IncludeAll() executed but plan.components is empty. " +
      "No components registered — check that builders call plan.AddToComponentsMap().");
  }
  for (const [bindingPath, comp] of Object.entries(components)) {
    emitValue(bindingPath, evalRead(comp.id, comp.vendor, comp.readExpr), transport);
  }
}

export function resolveGather(
  items: GatherItem[],
  verb: string,
  components: Record<string, ComponentEntry>,
  contentType?: string,
  evt?: Record<string, unknown>
): GatherResult {
  const urlParams: string[] = [];
  const formData = contentType === "form-data" ? new FormData() : null;
  const body: Record<string, unknown> = {};
  const transport = selectTransport(verb, urlParams, formData, body);

  for (const g of items) {
    switch (g.kind) {
      case "component":
        emitValue(g.name, evalRead(g.componentId, g.vendor, g.readExpr), transport);
        break;

      case "static":
        emitValue(g.param, g.value, transport);
        break;

      case "event": {
        const ctx = evt ? { evt } : {};
        emitValue(g.param, walk(ctx, g.path), transport);
        break;
      }

      case "all":
        emitAllComponents(components, transport);
        break;

      default:
        assertNever(g, "gather item kind");
    }
  }

  return { urlParams, body: formData ?? body };
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
