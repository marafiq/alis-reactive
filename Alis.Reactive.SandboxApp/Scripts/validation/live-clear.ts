// Live Validation — Per-field event wiring for interactive validation
// V3: reads from ContainerScope on plan.components[containerKey].container.
// Uses SHARED resolver for vendor root resolution.

import type { Plan, Component, ContainerScope } from "../types";
import { resolveElement, resolveVendorRoot } from "../resolution/resolver";
import { clearInline } from "./error-display";
import { scope } from "../core/trace";

const log = scope("live-clear");

/** Set of componentDomIds already wired — prevents double-wiring on partial reload. */
const wiredFields = new Set<string>();

/**
 * Wire live validation for all components in a container scope.
 * containerKey identifies the component that holds the ContainerScope.
 */
export function wireLiveValidation(plan: Plan, containerKey: string): void {
  const containerComp = plan.components[containerKey];
  if (!containerComp?.container) return;

  const containerId = containerComp.id;
  const containerScope = containerComp.container;

  if (!containerScope.validationRules) return;

  for (const cv of containerScope.validationRules) {
    const comp = plan.components[cv.component];
    if (!comp) continue;
    wireField(plan, containerId, cv.component, comp);
  }
}

function wireField(plan: Plan, containerId: string, componentKey: string, comp: Component): void {
  if (wiredFields.has(comp.id)) return;

  let el: HTMLElement;
  try {
    el = resolveElement(plan, componentKey);
  } catch {
    return; // Element not in DOM yet — will be wired on merge
  }

  wiredFields.add(comp.id);

  const clearHandler = () => clearInline(containerId, comp.id);

  if (comp.vendor === "native") {
    el.addEventListener("input", clearHandler);
    el.addEventListener("blur", clearHandler);
    el.addEventListener("change", clearHandler);
  } else {
    // Fusion (and future vendors): listen on the vendor root
    try {
      const root = resolveVendorRoot(el, comp.vendor);
      (root as EventTarget).addEventListener("change", clearHandler);
    } catch {
      // Component not yet initialized — skip, will be wired on merge
    }
  }
}

/** Reset for tests — clears the wired set so tests start clean. */
export function resetLiveClearForTests(): void {
  wiredFields.clear();
}
