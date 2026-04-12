// Validation Orchestrator — Fail-Closed, V3 ContainerScope
//
// Uses SHARED evaluateValue for ALL component value reads.
// No parallel read path — same concept as pipeline and gather.

import type {
  Plan, ContainerScope, ComponentValidation,
  ValidationRule,
} from "../types";
import type { ExecContext } from "../types";
import { resolveElement } from "../resolution/resolver";
import { evaluateCondition } from "../conditions/conditions";
import { evaluateValue } from "../core/evaluate";
import { scope } from "../core/trace";
import { toString } from "../core/shape-convert";
import { ruleFails } from "./rule-engine";
import {
  showInline, clearInline,
  addToSummary, removeSummaryEntry, clearSummary, showSummaryDiv, hideSummaryDiv, findSummaryElement,
  showServerErrorInline,
} from "./error-display";

const log = scope("validation");

// -- Public API --

export function validateContainer(plan: Plan, containerKey: string, ctx?: ExecContext): boolean {
  const containerComp = plan.components[containerKey];
  if (!containerComp) {
    log.warn("validate: container component not found", { containerKey });
    return false;
  }

  const containerScope = containerComp.container;
  if (!containerScope) {
    log.warn("validate: component has no container scope", { containerKey });
    return true;
  }

  const containerId = containerComp.id;
  let container: HTMLElement;
  try {
    container = resolveElement(plan, containerKey);
  } catch (e) {
    if (!isResolutionError(e)) throw e;
    if ((containerScope.validationRules?.length ?? 0) > 0) {
      log.warn("validate: form container missing, blocking", { containerId });
      return false;
    }
    return true;
  }

  const planId = plan.planId;
  const summaryEl = findSummaryElement(planId);

  clearContainerErrors(containerScope, plan, containerId, summaryEl);

  if (!containerScope.validationRules || containerScope.validationRules.length === 0) {
    return true;
  }

  let valid = true;
  let summaryHasErrors = false;

  for (const cv of containerScope.validationRules) {
    if (!evaluateComponentRules(cv, plan, containerId, container, summaryEl, ctx)) {
      valid = false;
      summaryHasErrors = summaryHasErrors || hasSummaryEntry(summaryEl, cv.component);
    }
  }

  if (summaryHasErrors && summaryEl) showSummaryDiv(summaryEl);

  log.debug("validate", { containerId, valid });
  return valid;
}

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

/**
 * Re-validate a single component within its container.
 * Called on blur/change by live-clear to give immediate field-level feedback.
 */
export function revalidateField(plan: Plan, containerKey: string, componentKey: string): void {
  const containerComp = plan.components[containerKey];
  if (!containerComp?.container?.validationRules) return;

  const cv = containerComp.container.validationRules.find(r => r.component === componentKey);
  if (!cv) return;

  const containerId = containerComp.id;

  // Clear existing error for this field
  const comp = plan.components[componentKey];
  if (comp) clearInline(containerId, comp.id);

  // Find the container element
  let container: HTMLElement;
  try {
    container = resolveElement(plan, containerKey);
  } catch (e) {
    if (!isResolutionError(e)) throw e;
    return;
  }

  const summaryEl = findSummaryElement(plan.planId);

  evaluateComponentRules(cv, plan, containerId, container, summaryEl);
}

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

  let el: HTMLElement;
  try {
    el = resolveElement(plan, cv.component);
  } catch (e) {
    if (!isResolutionError(e)) throw e;
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

  // Error spans are generated HTML elements ({componentDomId}_error), NOT plan components.
  // They are created by Html.Field() in C# and follow a predictable ID convention.
  // getElementById is correct here — error spans are not registered in plan.components.
  const errorSpan = document.getElementById(comp.id + "_error");
  const hidden = errorSpan?.parentElement ? isHidden(errorSpan.parentElement) : true;

  // Read value via the shared evaluateValue — same concept as pipeline and gather.
  // Component may not be resolved yet (partial not merged) — suppress resolution errors only.
  let value: unknown;
  try {
    value = evaluateValue(cv.value, plan);
  } catch (e) {
    if (isResolutionError(e)) { value = undefined; }
    else throw e;
  }

  for (const rule of cv.rules) {
    // Condition may reference components not yet merged — skip rule if unresolvable
    if (rule.when) {
      try {
        const condResult = evaluateCondition(rule.when, plan, ctx);
        if (!condResult) continue;
      } catch (e) {
        if (isResolutionError(e)) continue;
        throw e;
      }
    }

    // Pre-resolve otherValue if present — keeps rule-engine pure (no DOM)
    let otherValue: unknown;
    if (rule.otherValue) {
      try { otherValue = evaluateValue(rule.otherValue, plan); }
      catch (e) { if (isResolutionError(e)) otherValue = undefined; else throw e; }
    }

    if (ruleFails(rule, value, otherValue)) {
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

/** Only suppress errors from component/element resolution — not contract bugs. */
function isResolutionError(e: unknown): boolean {
  if (!(e instanceof Error)) return false;
  const msg = e.message;
  return msg.includes("component not found") || msg.includes("element not found");
}

// -- Helpers --


function allRulesConditionallySkipped(rules: ValidationRule[], plan: Plan, ctx?: ExecContext): boolean {
  if (rules.length === 0) return true;
  for (const rule of rules) {
    if (!rule.when) return false;
    try {
      const result = evaluateCondition(rule.when, plan, ctx);
      if (result) return false;
    } catch (e) {
      if (isResolutionError(e)) continue;
      throw e;
    }
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
      if (comp) clearInline(containerId, comp.id);
    }
  }
  if (summaryEl) {
    clearSummary(summaryEl);
    hideSummaryDiv(summaryEl);
  }
}

function findComponentKeyByName(containerScope: ContainerScope, name: string): string | undefined {
  // Plan-driven: each ComponentValidation carries serverFieldName set at C# build time.
  // No heuristics — the plan declares the mapping.
  return containerScope.validationRules?.find(
    cv => cv.serverFieldName === name || cv.component === name
  )?.component;
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
