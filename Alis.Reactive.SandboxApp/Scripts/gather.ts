import type { GatherItem, ComponentEntry } from "./types";
import { evalRead } from "./component";
import { scope } from "./trace";

const log = scope("gather");

export interface GatherResult {
  urlParams: string[];
  body: Record<string, unknown> | FormData;
}

export function resolveGather(
  items: GatherItem[],
  verb: string,
  components: Record<string, ComponentEntry>,
  contentType?: string
): GatherResult {
  const isGet = verb === "GET";
  const useFormData = contentType === "form-data";
  const urlParams: string[] = [];
  const formData = useFormData ? new FormData() : null;
  const body: Record<string, unknown> = {};

  for (const g of items) {
    switch (g.kind) {
      case "component": {
        const raw = evalRead(g.componentId, g.vendor, g.readExpr);
        if (raw === undefined) {
          log.warn("gather target not found", { componentId: g.componentId });
          break;
        }
        // Normalize: empty string -> null for JSON body (avoids deserialization errors on numeric types)
        const value = raw === "" ? null : raw;
        if (isGet) {
          urlParams.push(`${encodeURIComponent(g.name)}=${encodeURIComponent(String(raw))}`);
        } else if (formData) {
          formData.append(g.name, String(raw ?? ""));
        } else {
          setNested(body, g.name, value);
        }
        log.trace("component", { name: g.name, value });
        break;
      }

      case "static": {
        if (isGet) {
          urlParams.push(`${encodeURIComponent(g.param)}=${encodeURIComponent(String(g.value))}`);
        } else if (formData) {
          formData.append(g.param, String(g.value ?? ""));
        } else {
          body[g.param] = g.value;
        }
        log.trace("static", { param: g.param, value: g.value });
        break;
      }

      case "all": {
        for (const [bindingPath, comp] of Object.entries(components)) {
          const raw = evalRead(comp.id, comp.vendor, comp.readExpr);
          if (raw === undefined) {
            log.warn("gather target not found", { componentId: comp.id });
            continue;
          }
          const value = raw === "" ? null : raw;
          if (isGet) {
            urlParams.push(`${encodeURIComponent(bindingPath)}=${encodeURIComponent(String(raw))}`);
          } else if (formData) {
            formData.append(bindingPath, String(raw ?? ""));
          } else {
            setNested(body, bindingPath, value);
          }
          log.trace("component", { name: bindingPath, value });
        }
        break;
      }
    }
  }

  return { urlParams, body: formData ?? body };
}

/** Convert dotted key "Address.Street" into nested object { Address: { Street: val } } */
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
