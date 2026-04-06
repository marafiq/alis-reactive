// resolver.ts — the ONE shared resolution module.
// Every module uses this. No other resolution path exists.
//
// Resolution flow:
//   1. Look up component in plan.components[key] -> get id, vendor, type
//   2. Use vendor to resolve JS root: native -> getElementById, fusion -> ej2_instances[0]
//   3. Look up JsType in plan.types[component.type] -> get properties/methods/events
//   4. Read property: walk the property's path on the root
//   5. Set property: walk the property's path and assign
//   6. Call method: walk the method's path and call

import type {
  Plan, JsType, Property, Method, Source,
  PayloadSource, Path, Vendor,
} from "../types";
import type { ExecContext } from "../types";
import { walkPath, walkPathParent } from "../core/walk";
import { scope } from "../core/trace";
import { assertNever } from "../core/assert-never";
import { wire as wireNative } from "./event-native";
import { wire as wireFusion } from "./event-fusion";

const log = scope("resolver");

// ── Source resolution ──────────────────────────────────────

/** Resolve a Source to a JS object root. */
export function resolveSource(plan: Plan, source: Source, ctx?: ExecContext): unknown {
  switch (source.kind) {
    case "component":
      return resolveComponent(plan, source.component);
    case "payload":
      return resolvePayload(source, ctx);
    default:
      assertNever(source, "source kind");
  }
}

/** Resolve a component's raw DOM element (not vendor root). For inject, validation, error display. */
export function resolveElement(plan: Plan, componentKey: string): HTMLElement {
  const comp = plan.components[componentKey];
  if (!comp) throw new Error(`[alis] component not found: ${componentKey}`);
  const el = document.getElementById(comp.id);
  if (!el) throw new Error(`[alis] element not found: ${comp.id}`);
  return el;
}

/** Resolve component key -> JS object root (vendor-dispatched). */
export function resolveComponent(plan: Plan, componentKey: string): unknown {
  const el = resolveElement(plan, componentKey);
  return resolveVendorRoot(el, plan.components[componentKey]!.vendor);
}

/** Resolve vendor-specific root from a DOM element. */
export function resolveVendorRoot(el: HTMLElement, vendor: Vendor): unknown {
  switch (vendor) {
    case "native":
      return el;
    case "fusion": {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any -- ej2_instances is a Syncfusion runtime property
      const root = (el as any).ej2_instances?.[0];
      if (!root) throw new Error(`[alis] no vendor root for "${el.id}" (vendor: fusion) — is the component initialized?`);
      return root;
    }
    default: {
      const _: never = vendor;
      throw new Error(`[alis] unknown vendor: "${_}"`);
    }
  }
}

/** Resolve payload from execution context. */
function resolvePayload(source: PayloadSource, ctx?: ExecContext): unknown {
  if (!ctx) throw new Error("[alis] payload source requires execution context");
  switch (source.scope) {
    case "event":    return ctx.event;
    case "success":  return ctx.response;
    case "error":    return ctx.response;
    case "request":  return ctx.request;
    case "dispatch": return ctx.event;
    case "local":    return ctx.local;
    default: {
      const _: never = source.scope;
      throw new Error(`[alis] unknown payload scope: "${_}"`);
    }
  }
}

// ── JsType lookup ──────────────────────────────────────────

/** Get JsType for a component by key. */
export function getJsType(plan: Plan, componentKey: string): JsType {
  const comp = plan.components[componentKey];
  if (!comp) throw new Error(`[alis] component not found: ${componentKey}`);
  const jsType = plan.types[comp.type];
  if (!jsType) throw new Error(`[alis] type not found: ${comp.type}`);
  return jsType;
}

/** Get JsType for a Source (component sources only). */
export function getJsTypeForSource(plan: Plan, source: Source): JsType {
  if (source.kind !== "component") {
    throw new Error("[alis] getJsTypeForSource only supports component sources");
  }
  return getJsType(plan, source.component);
}

// ── Property operations ────────────────────────────────────

/** Read a property value from a resolved root using JsType path. */
export function readProperty(root: unknown, property: Property): unknown {
  return walkPath(root, property.path);
}

/** Set a property value on a resolved root using JsType path. */
export function setProperty(root: unknown, property: Property, value: unknown): void {
  const { owner, key } = walkPathParent(root, property.path);
  if (owner == null) throw new Error(`[alis] cannot set property — parent is null`);
  owner[key] = value;
}

// ── Method operations ──────────────────────────────────────

/** Call a method on a resolved root using JsType path. */
export function callMethod(root: unknown, method: Method, args: unknown[]): unknown {
  const { fn, owner } = resolveCallable(root, method.path);
  return fn.apply(owner, args);
}

/** Resolve a callable function and its owner from a path. */
function resolveCallable(root: unknown, path: Path): { fn: Function; owner: unknown } {
  if (path.length === 0) throw new Error("[alis] resolveCallable: empty path");

  // Walk to the parent of the last segment
  let owner: any = root;
  for (let i = 0; i < path.length - 1; i++) {
    const seg = path[i];
    if (owner == null) throw new Error(`[alis] resolveCallable: null at segment ${i}`);
    switch (seg.kind) {
      case "property":
        owner = owner[seg.name];
        break;
      case "index":
        owner = owner[seg.index];
        break;
    }
  }

  const last = path[path.length - 1];
  const key = last.kind === "property" ? last.name : last.index;
  const fn = owner[key];

  if (typeof fn !== "function") {
    throw new Error(`[alis] resolveCallable: "${String(key)}" is not a function`);
  }

  return { fn, owner };
}

// ── Default value ──────────────────────────────────────────

/** Read the component's default value using its JsType's defaultValue definition. */
export function readDefaultValue(plan: Plan, componentKey: string): unknown {
  const jsType = getJsType(plan, componentKey);
  if (!jsType.defaultValue) {
    throw new Error(`[alis] no defaultValue on type for ${componentKey}`);
  }

  const root = resolveComponent(plan, componentKey);
  const dv = jsType.defaultValue;

  if (dv.kind === "property") {
    const prop = jsType.properties?.[dv.member];
    if (!prop) throw new Error(`[alis] defaultValue property not found: ${dv.member}`);
    return readProperty(root, prop);
  } else {
    const method = jsType.methods?.[dv.member];
    if (!method) throw new Error(`[alis] defaultValue method not found: ${dv.member}`);
    return callMethod(root, method, []);
  }
}

// ── Event wiring ──────────────────────────────────────────

/** Wire an event listener on a component — dispatches to vendor-specific module. */
export function wireEvent(
  plan: Plan,
  componentKey: string,
  channel: string,
  handler: (data: unknown) => void,
  opts?: AddEventListenerOptions,
): void {
  const comp = plan.components[componentKey];
  if (!comp) throw new Error(`[alis] wireEvent: component not found: ${componentKey}`);
  const root = resolveComponent(plan, componentKey);

  switch (comp.vendor) {
    case "native":  wireNative(root, channel, handler, opts); break;
    case "fusion":  wireFusion(root, channel, handler, opts); break;
    default: assertNever(comp.vendor, "vendor");
  }
}

log.debug("loaded");
