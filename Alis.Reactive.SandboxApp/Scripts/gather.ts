import type { GatherItem } from "./types";
import { scope } from "./trace";

const log = scope("gather");

export interface GatherResult {
  urlParams: string[];
  body: Record<string, unknown>;
}

export function resolveGather(items: GatherItem[], verb: string): GatherResult {
  const isGet = verb === "GET";
  const urlParams: string[] = [];
  const body: Record<string, unknown> = {};

  for (const g of items) {
    switch (g.kind) {
      case "component": {
        const el = document.getElementById(g.componentId);
        if (!el) {
          log.warn("gather target not found", { componentId: g.componentId });
          break;
        }
        const value = g.readExpr
          ? new Function("el", `return ${g.readExpr}`)(el)
          : (el as HTMLInputElement).value;
        if (isGet) {
          urlParams.push(`${encodeURIComponent(g.name)}=${encodeURIComponent(String(value))}`);
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
        fd.forEach((value, key) => {
          if (isGet) {
            urlParams.push(`${encodeURIComponent(key)}=${encodeURIComponent(String(value))}`);
          } else {
            body[key] = value;
          }
        });
        log.trace("all", { formId: g.formId });
        break;
      }

      case "static": {
        if (isGet) {
          urlParams.push(`${encodeURIComponent(g.param)}=${encodeURIComponent(String(g.value))}`);
        } else {
          body[g.param] = g.value;
        }
        log.trace("static", { param: g.param, value: g.value });
        break;
      }
    }
  }

  return { urlParams, body };
}
