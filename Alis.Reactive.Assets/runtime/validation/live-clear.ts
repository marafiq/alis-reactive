// Live Validation — Per-field event wiring for interactive validation
// Reads container scope through RuntimePlan so component lookup and DOM resolution
// stay on the same deterministic path as execution/gather.
// On blur/change: re-validates the single field (not just clears).
// On input: clears only (typing should not show errors mid-keystroke).

import type { Plan } from "../types";
import {
  RuntimeComponentReadinessError,
  RuntimePlan,
  RuntimeResolutionError,
  type RuntimeComponent,
} from "../domain/runtime-plan";
import { wireEvent } from "../resolution/resolver";
import { clearInline } from "./error-display";
import { revalidateField } from "./orchestrator";

interface LiveFieldWire {
  readonly plan: Plan;
  readonly containerKey: string;
  readonly component: RuntimeComponent;
  readonly signal: AbortSignal | undefined;
}

interface LiveFieldEvents {
  readonly clear: () => void;
  readonly revalidate: () => void;
  readonly listenerOptions: AddEventListenerOptions | undefined;
}

/**
 * Wire live validation for all components in a container scope.
 * containerKey identifies the component that holds the ContainerScope.
 */
export function wireLiveValidation(plan: Plan, containerKey: string, signal?: AbortSignal): void {
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

  // DOM events are best-effort field events; vendor semantic change is tracked separately.
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

  // Semantic "change" goes through the vendor's event system (DOM or modelObserver).
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

/** Remove a field's wired status so it can be re-wired after partial reload. */
export function unwireField(domId: string): void {
  forgetFieldWiring(domId);
}

/** Reset for tests — clears the wired set so tests start clean. */
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
