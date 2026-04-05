// Validation Orchestrator — Fail-Closed, V3 ContainerScope
//
// V3: validation rules live on plan.components[key].container.validationRules.
// Each ComponentValidation maps a component key to its rules.
// Uses SHARED resolver for reading component values.

import type {
  Plan, Component, ContainerScope, ComponentValidation,
  ValidationRule, Condition,
} from "../types";
import type { ExecContext } from "../types";
import {
  resolveComponent, getJsType, readProperty, resolveVendorRoot,
} from "../resolution/resolver";
import { evaluateCondition } from "../conditions/conditions";
import { scope } from "../core/trace";
import { toString, toDate, applyShape, shapeToCoercionType } from "../core/coerce";
import { ruleFails, type PeerReader } from "./rule-engine";
import {
  showInline, clearInline, clearAllInline,
  addToSummary, removeSummaryEntry, clearSummary, showSummaryDiv, hideSummaryDiv, findSummaryElement,
  showServerErrorInline,
} from "./error-display";

const log = scope("validation");

// -- Public API --

/**
 * Validate all components in a container scope. Returns true if all pass.
 * The container key identifies which component holds the ContainerScope.
 */
export function validateContainer(plan: Plan, containerKey: string, ctx?: ExecContext): boolean {
  const containerComp = plan.components[containerKey];
  if (!containerComp) {
    log.warn("validate: container component not found", { containerKey });
    return false;
  }

  const containerScope = containerComp.container;
  if (!containerScope) {
    log.warn("validate: component has no container scope", { containerKey });
    return true; // no validation rules
  }

  const containerId = containerComp.id;
  const container = document.getElementById(containerId);
  if (!container) {
    if ((containerScope.validationRules?.length ?? 0) > 0) {
      log.warn("validate: form container missing, blocking", { containerId });
      return false;
    }
    return true;
  }

  const planId = plan.planId;
  const summaryEl = findSummaryElement(planId);

  // Clear all previous errors
  clearContainerErrors(containerScope, plan, containerId, summaryEl);

  if (!containerScope.validationRules || containerScope.validationRules.length === 0) {
    return true;
  }

  const peerReader = createPeerReader(plan, containerScope);
  let valid = true;
  let summaryHasErrors = false;

  for (const cv of containerScope.validationRules) {
    if (!evaluateComponentRules(cv, plan, containerId, container, peerReader, summaryEl, ctx)) {
      valid = false;
      summaryHasErrors = summaryHasErrors || hasSummaryEntry(summaryEl, cv.component);
    }
  }

  if (summaryHasErrors && summaryEl) showSummaryDiv(summaryEl);

  log.debug("validate", { containerId, valid });
  return valid;
}

/** Show server-side validation errors from a ProblemDetails response. */
export function showServerErrors(plan: Plan, containerKey: string, data: unknown): void {
  const containerComp = plan.components[containerKey];
  if (!containerComp?.container) return;

  const containerId = containerComp.id;
  const containerScope = containerComp.container;
  const planId = plan.planId;
  const summaryEl = findSummaryElement(planId);

  clearContainerErrors(containerScope, plan, containerId, summaryEl);

  const errors = extractErrors(data);
  if (!errors) return;

  let summaryHasErrors = false;

  for (const [name, msgs] of Object.entries(errors)) {
    const msgResult = toString(msgs);
    const msg = Array.isArray(msgs) ? msgs.join(", ") : msgResult.ok ? msgResult.value : "";

    const compKey = findComponentKeyByName(containerScope, name);
    if (compKey) {
      const comp = plan.components[compKey];
      if (comp) {
        showServerErrorInline(containerId, compKey, msg, plan, containerScope);
      }
    } else if (summaryEl) {
      addToSummary(summaryEl, name, msg);
      summaryHasErrors = true;
    }
  }

  if (summaryHasErrors && summaryEl) showSummaryDiv(summaryEl);
  log.debug("showServerErrors", { containerId, fieldCount: Object.keys(errors).length });
}

/** Clear all validation errors for a container. */
export function clearContainerValidation(plan: Plan, containerKey: string): void {
  const containerComp = plan.components[containerKey];
  if (!containerComp?.container) return;

  const containerId = containerComp.id;
  const summaryEl = findSummaryElement(plan.planId);
  clearContainerErrors(containerComp.container, plan, containerId, summaryEl);
}

// -- Per-component evaluation --

function evaluateComponentRules(
  cv: ComponentValidation,
  plan: Plan,
  containerId: string,
  container: HTMLElement,
  peerReader: PeerReader,
  summaryEl: HTMLElement | null,
  ctx?: ExecContext,
): boolean {
  const comp = plan.components[cv.component];
  if (!comp) {
    log.trace("component-not-found", { component: cv.component });
    if (allRulesConditionallySkipped(cv.rules, plan, ctx)) return true;
    if (cv.rules.length > 0 && summaryEl) {
      addToSummary(summaryEl, cv.component, cv.rules[0].message);
    }
    return false;
  }

  const el = document.getElementById(comp.id);
  if (!el) {
    if (allRulesConditionallySkipped(cv.rules, plan, ctx)) return true;
    if (cv.rules.length > 0 && summaryEl) {
      addToSummary(summaryEl, cv.component, cv.rules[0].message);
    }
    return false;
  }

  if (!container.contains(el)) {
    log.trace("field outside form, skipping", { component: cv.component, containerId });
    return true;
  }

  const errorSpan = document.getElementById(comp.id + "_error");
  const hidden = errorSpan?.parentElement ? isHidden(errorSpan.parentElement) : true;

  // Read the component's default value using the shared resolver
  const jsType = getJsType(plan, cv.component);
  const root = resolveVendorRoot(el, comp.vendor);
  let value: unknown;
  if (jsType.defaultValue) {
    const dv = jsType.defaultValue;
    if (dv.kind === "property") {
      const prop = jsType.properties?.[dv.member];
      value = prop ? readProperty(root, prop) : undefined;
    } else {
      const method = jsType.methods?.[dv.member];
      value = method ? (root as any)[method.path[method.path.length - 1].kind === "property"
        ? (method.path[method.path.length - 1] as any).name
        : method.path[method.path.length - 1]]?.() : undefined;
    }
  }

  // Evaluate rules
  for (const rule of cv.rules) {
    if (rule.when) {
      const condResult = evaluateCondition(rule.when, plan, ctx);
      if (!condResult) continue; // condition false -> skip rule
    }

    if (ruleFails(rule, value, peerReader)) {
      log.trace("rule-fail", { component: cv.component, rule: rule.name, value, message: rule.message });
      if (hidden) {
        if (summaryEl) addToSummary(summaryEl, cv.component, rule.message);
      } else {
        showInline(containerId, comp.id, rule.message);
        if (summaryEl) removeSummaryEntry(summaryEl, cv.component);
      }
      return false;
    }
  }

  return true;
}

// -- Helpers --

function createPeerReader(plan: Plan, containerScope: ContainerScope): PeerReader {
  return {
    readPeer(componentKey: string): unknown {
      const comp = plan.components[componentKey];
      if (!comp) return undefined;
      const el = document.getElementById(comp.id);
      if (!el) return undefined;
      try {
        const root = resolveVendorRoot(el, comp.vendor);
        const jsType = getJsType(plan, componentKey);
        if (!jsType.defaultValue) return undefined;
        const dv = jsType.defaultValue;
        if (dv.kind === "property") {
          const prop = jsType.properties?.[dv.member];
          return prop ? readProperty(root, prop) : undefined;
        }
        return undefined;
      } catch {
        return undefined;
      }
    },
  };
}

function allRulesConditionallySkipped(rules: ValidationRule[], plan: Plan, ctx?: ExecContext): boolean {
  if (rules.length === 0) return true;
  for (const rule of rules) {
    if (!rule.when) return false; // unconditional rule -> must block
    const result = evaluateCondition(rule.when, plan, ctx);
    if (result) return false; // condition met -> must block
  }
  return true;
}

function clearContainerErrors(
  containerScope: ContainerScope,
  plan: Plan,
  containerId: string,
  summaryEl: HTMLElement | null,
): void {
  if (containerScope.validationRules) {
    for (const cv of containerScope.validationRules) {
      const comp = plan.components[cv.component];
      if (comp) {
        clearInline(containerId, comp.id);
      }
    }
  }
  if (summaryEl) {
    clearSummary(summaryEl);
    hideSummaryDiv(summaryEl);
  }
}

function findComponentKeyByName(containerScope: ContainerScope, name: string): string | undefined {
  return containerScope.validationRules?.find(cv => cv.component === name)?.component;
}

function hasSummaryEntry(summaryEl: HTMLElement | null, componentKey: string): boolean {
  if (!summaryEl) return false;
  return summaryEl.querySelector(`[data-valmsg-summary-for="${componentKey}"]`) !== null;
}

function isHidden(el: HTMLElement): boolean {
  let node: HTMLElement | null = el;
  while (node) {
    if (node.hasAttribute("hidden") || node.style?.display === "none") return true;
    node = node.parentElement;
  }
  return false;
}

function extractErrors(data: unknown): Record<string, unknown> | null {
  if (!data || typeof data !== "object") return null;
  const obj = data as Record<string, unknown>;
  if ("errors" in obj && typeof obj.errors === "object" && obj.errors !== null) {
    return obj.errors as Record<string, unknown>;
  }
  log.warn("showServerErrors: response is not ProblemDetails shape, ignoring", {});
  return null;
}
