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

  function addValue(name: string, raw: unknown): void {
    const value = raw === "" ? null : raw;
    if (isGet) {
      urlParams.push(`${encodeURIComponent(name)}=${encodeURIComponent(String(raw))}`);
    } else if (formData) {
      formData.append(name, String(raw ?? ""));
    } else {
      setNested(body, name, value);
    }
    log.trace("component", { name, value });
  }

  for (const g of items) {
    switch (g.kind) {
      case "component":
        addValue(g.name, evalRead(g.componentId, g.vendor, g.readExpr));
        break;

      case "static":
        if (isGet) {
          urlParams.push(`${encodeURIComponent(g.param)}=${encodeURIComponent(String(g.value))}`);
        } else if (formData) {
          formData.append(g.param, String(g.value ?? ""));
        } else {
          body[g.param] = g.value;
        }
        log.trace("static", { param: g.param, value: g.value });
        break;

      case "all":
        if (Object.keys(components).length === 0) {
          throw new Error(
            "[alis] IncludeAll() executed but plan.components is empty. " +
            "No components registered — check that builders call plan.AddToComponentsMap().");
        }
        for (const [bindingPath, comp] of Object.entries(components)) {
          addValue(bindingPath, evalRead(comp.id, comp.vendor, comp.readExpr));
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
