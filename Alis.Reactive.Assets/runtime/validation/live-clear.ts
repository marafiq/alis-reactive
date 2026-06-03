// Live validation revalidates on blur/vendor change and only clears while typing.
// RuntimePlan lookup keeps fields on the same component path as execution/gather.

import type { PlanDocument } from "../types/index";
import {
  RuntimeComponentReadinessError,
  RuntimePlan,
  RuntimeResolutionError,
  type RuntimeComponent,
} from "../browser-objects/runtime-plan";
import { wireEvent } from "../events/resolver";
import { clearInline } from "./error-display";
import { revalidateField } from "./orchestrator";

interface LiveFieldWire {
  readonly plan: PlanDocument;
  readonly containerKey: string;
  readonly component: RuntimeComponent;
  readonly signal: AbortSignal | undefined;
}

interface LiveFieldEvents {
  readonly clear: () => void;
  readonly revalidate: () => void;
  readonly listenerOptions: AddEventListenerOptions | undefined;
}

export function wireLiveValidation(plan: PlanDocument, containerKey: string, signal?: AbortSignal): void {
  const runtime = RuntimePlan.from(plan);
  const containerComp = runtime.components.find(containerKey);
  const containerScope = containerComp?.containerScope;
  if (!containerComp || !containerScope) return;

  for (const cv of containerScope.validationRules) {
    const comp = runtime.components.find(cv.component);
    if (!comp) continue;
    wireField({ plan, containerKey, component: comp, signal });
  }
}

function wireField(field: LiveFieldWire): void {
  if (field.signal?.aborted === true) return;

  const el = resolveFieldElement(field.component);
  if (el === undefined) return;

  const events = liveFieldEventsFor(field);

  const domEventsWereAdded = wireFieldDomEvents(field, el, events);
  const changeEventWasAdded = wireComponentChangeEvent(field, events);

  if (domEventsWereAdded || changeEventWasAdded) {
    forgetWiredFieldOnAbort(field.component.id, field.signal, {
      domEvents: domEventsWereAdded,
      componentChangeEvent: changeEventWasAdded,
    });
  }
}

function liveFieldEventsFor(field: LiveFieldWire): LiveFieldEvents {
  return {
    clear: () => clearInline(field.component.id),
    revalidate: () => revalidateField(field.plan, field.containerKey, field.component.key),
    listenerOptions: listenerOptionsFor(field.signal),
  };
}

function resolveFieldElement(component: RuntimeComponent): HTMLElement | undefined {
  try {
    return component.element();
  } catch (e) {
    if (RuntimeResolutionError.is(e)) return undefined;
    throw e;
  }
}

function wireFieldDomEvents(
  field: LiveFieldWire,
  el: HTMLElement,
  events: LiveFieldEvents,
): boolean {
  if (fieldHasDomEvents(field.component.id)) return false;

  // DOM events are best-effort; vendor semantic change is wired separately below.
  el.addEventListener("input", events.clear, events.listenerOptions);
  el.addEventListener("blur", events.revalidate, events.listenerOptions);
  wiredFieldDomEvents.add(field.component.id);
  return true;
}

function wireComponentChangeEvent(
  field: LiveFieldWire,
  events: LiveFieldEvents,
): boolean {
  if (fieldHasComponentChangeEvent(field.component.id)) return false;

  // Semantic change must go through the vendor event adapter (DOM or modelObserver).
  try {
    wireEvent(field.plan, field.component.key, "change", () => events.revalidate(), events.listenerOptions);
  } catch (e) {
    if (componentChangeEventCanBeDeferred(e)) return false;
    throw e;
  }

  wiredFieldComponentChangeEvents.add(field.component.id);
  return true;
}

function componentChangeEventCanBeDeferred(error: unknown): boolean {
  return RuntimeResolutionError.is(error) || RuntimeComponentReadinessError.is(error);
}

/** Partial reloads unmount fields; clear wiring so remounted fields can wire again. */
export function unwireField(domId: string): void {
  forgetFieldWiring(domId);
}

export function resetLiveClearForTests(): void {
  wiredFieldDomEvents.clear();
  wiredFieldComponentChangeEvents.clear();
}

function listenerOptionsFor(signal: AbortSignal | undefined): AddEventListenerOptions | undefined {
  if (signal === undefined) return undefined;

  return { signal };
}

const wiredFieldDomEvents = new Set<string>();
const wiredFieldComponentChangeEvents = new Set<string>();

function fieldHasDomEvents(componentDomId: string): boolean {
  return wiredFieldDomEvents.has(componentDomId);
}

function fieldHasComponentChangeEvent(componentDomId: string): boolean {
  return wiredFieldComponentChangeEvents.has(componentDomId);
}

function forgetFieldWiring(componentDomId: string): void {
  wiredFieldDomEvents.delete(componentDomId);
  wiredFieldComponentChangeEvents.delete(componentDomId);
}

interface LiveFieldWireKinds {
  readonly domEvents: boolean;
  readonly componentChangeEvent: boolean;
}

function forgetWiredFieldOnAbort(
  componentDomId: string,
  signal: AbortSignal | undefined,
  kinds: LiveFieldWireKinds,
): void {
  signal?.addEventListener("abort", () => {
    if (kinds.domEvents) wiredFieldDomEvents.delete(componentDomId);
    if (kinds.componentChangeEvent) wiredFieldComponentChangeEvents.delete(componentDomId);
  }, { once: true });
}
