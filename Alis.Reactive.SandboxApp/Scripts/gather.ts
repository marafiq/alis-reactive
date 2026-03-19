import type { GatherItem, ComponentEntry } from "./types";
import { evalRead } from "./component";
import { scope } from "./core/trace";

const log = scope("gather");

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
      emitScalar: (name, value) => formData.append(name, String(value ?? "")),
      emitArray: (name, items) => {
        for (const item of items) {
          const file = toFile(item);
          if (file) formData.append(name, file, file.name);
          else formData.append(name, String(item ?? ""));
        }
      },
    };
  }
  if (urlParams === body as unknown) {
    // unreachable — type guard for exhaustiveness
    throw new Error("unreachable");
  }
  return {
    emitScalar: (name, value) => urlParams.push(
      `${encodeURIComponent(name)}=${encodeURIComponent(String(value))}`),
    emitArray: (name, items) => {
      if (hasFiles(items))
        throw new Error("[alis] File objects cannot be sent via GET");
      for (const item of items)
        urlParams.push(`${encodeURIComponent(name)}=${encodeURIComponent(String(item))}`);
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

export function resolveGather(
  items: GatherItem[],
  verb: string,
  components: Record<string, ComponentEntry>,
  contentType?: string
): GatherResult {
  const urlParams: string[] = [];
  const useFormData = contentType === "form-data";
  const formData = useFormData ? new FormData() : null;
  const body: Record<string, unknown> = {};

  const transport = verb === "GET"
    ? createTransport(urlParams, null, body)
    : formData
      ? createTransport(urlParams, formData, body)
      : createJsonTransport(body);

  function emit(name: string, raw: unknown): void {
    // FileList — browser native, array-like but not Array.isArray
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

  for (const g of items) {
    switch (g.kind) {
      case "component":
        emit(g.name, evalRead(g.componentId, g.vendor, g.readExpr));
        break;

      case "static":
        emit(g.param, g.value);
        break;

      case "all":
        if (Object.keys(components).length === 0) {
          throw new Error(
            "[alis] IncludeAll() executed but plan.components is empty. " +
            "No components registered — check that builders call plan.AddToComponentsMap().");
        }
        for (const [bindingPath, comp] of Object.entries(components)) {
          emit(bindingPath, evalRead(comp.id, comp.vendor, comp.readExpr));
        }
        break;
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
