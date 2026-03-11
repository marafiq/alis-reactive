import type { GatherItem } from "./types";
import { evalRead } from "./component";
import { scope } from "./trace";

const log = scope("gather");

export interface GatherResult {
  urlParams: string[];
  body: Record<string, unknown> | FormData;
}

export function resolveGather(items: GatherItem[], verb: string, contentType?: string): GatherResult {
  const isGet = verb === "GET";
  const useFormData = contentType === "form-data";
  const urlParams: string[] = [];
  const formData = useFormData ? new FormData() : null;
  const body: Record<string, unknown> = {};

  for (const g of items) {
    switch (g.kind) {
      case "component": {
        if (!document.getElementById(g.componentId)) {
          log.warn("gather target not found", { componentId: g.componentId });
          break;
        }
        const value = evalRead(g.componentId, g.vendor, g.readExpr);
        if (isGet) {
          urlParams.push(`${encodeURIComponent(g.name)}=${encodeURIComponent(String(value))}`);
        } else if (formData) {
          formData.append(g.name, String(value ?? ""));
        } else {
          body[g.name] = value;
        }
        log.trace("component", { name: g.name, value });
        break;
      }

      case "all": {
        const form = document.getElementById(g.formId) as HTMLFormElement | null;
        if (!form) {
          log.warn("form not found", { formId: g.formId });
          break;
        }
        const fd = new FormData(form);
        if (formData) {
          // FormData mode: transfer entries directly (preserves File objects)
          fd.forEach((value, key) => {
            formData.append(key, value);
          });
        } else {
          fd.forEach((value, key) => {
            const v = isGet ? value : (value === "" ? null : value);
            if (isGet) {
              urlParams.push(`${encodeURIComponent(key)}=${encodeURIComponent(String(value))}`);
            } else {
              setNested(body, key, v);
            }
          });
        }
        log.trace("all", { formId: g.formId });
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
