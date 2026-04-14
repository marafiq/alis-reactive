// Validation Orchestrator — Fail-Closed, V3 ContainerScope
//
// Uses SHARED evaluateValue for ALL component value reads.
// No parallel read path — same concept as pipeline and gather.

import type {
  Plan, ContainerScope, ComponentValidation,
  ValidationRule, ValueProducer,
} from "../types";
import { isEvaluable } from "../types";
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
    log.warn("container.not-found", { id: containerKey });
    return false;
  }

  const containerScope = containerComp.container;
  if (!containerScope) {
    log.warn("container.no-scope", { id: containerComp.id });
    return true;
  }

  const containerId = containerComp.id;
  let container: HTMLElement;
  try {
    container = resolveElement(plan, containerKey);
  } catch (e) {
    if (!isResolutionError(e)) throw e;
    if (containerScope.validationRules.length > 0) {
      log.warn("form.missing", { id: containerId });
      return false;
    }
    return true;
  }

  const planId = plan.planId;
  const summaryEl = findSummaryElement(planId);

  clearContainerErrors(containerScope, plan, containerId, summaryEl);

  if (containerScope.validationRules.length === 0) {
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

  log.debug("validated", { id: containerId, valid });
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
    const addedToSummary = placeServerError(name, msgs, containerScope, containerId, summaryEl, plan);
    if (addedToSummary) summaryHasErrors = true;
  }

  if (summaryHasErrors && summaryEl) showSummaryDiv(summaryEl);
  log.debug("server-errors.shown", { id: containerId, fieldCount: Object.keys(errors).length });
}

/** Place a single server error on its component or into the summary. Returns true if any summary errors added. */
function placeServerError(
  name: string, msgs: unknown, containerScope: ContainerScope,
  containerId: string, summaryEl: HTMLElement | null, plan: Plan,
): boolean {
  const msgResult = toString(msgs);
  const msg = Array.isArray(msgs) ? msgs.join(", ") : msgResult.ok ? msgResult.value : "";

  const compKey = findComponentKeyByName(containerScope, name);
  if (compKey) {
    const comp = plan.components[compKey];
    if (comp) {
      showServerErrorInline(containerId, compKey, msg, plan, containerScope);
    }
    // compKey exists but component missing from plan — silently skip (not a summary item)
    return false;
  }
  // No component found by name — route to summary
  if (summaryEl) {
    addToSummary(summaryEl, name, msg);
    return true;
  }
  return false;
}

/**
 * Re-validate a single component within its container.
 * Called on blur/change by live-clear to give immediate field-level feedback.
 */
export function revalidateField(plan: Plan, containerKey: string, componentKey: string): void {
  const containerComp = plan.components[containerKey];
  if (!containerComp?.container) return;

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
  if (!comp) return handleMissingComponent(cv, plan, summaryEl, ctx);

  const resolved = resolveFieldElement(plan, cv, summaryEl, ctx);
  if (resolved.done) return resolved.result;

  if (!container.contains(resolved.el)) {
    log.trace("field.out-of-scope", { component: cv.component, containerId });
    return true;
  }

  const hidden = isErrorSpanHidden(comp.id);
  const value = readValueSafe(cv.value, plan);

  return evaluateRulesForField(cv, plan, containerId, comp.id, value, hidden, summaryEl, ctx);
}

/** When the component is not found in the plan, check if all rules are conditionally skipped. */
function handleMissingComponent(
  cv: ComponentValidation, plan: Plan, summaryEl: HTMLElement | null, ctx?: ExecContext,
): boolean {
  log.trace("component.not-found", { component: cv.component });
  if (allRulesConditionallySkipped(cv.rules, plan, ctx)) return true;
  if (cv.rules.length > 0 && summaryEl) {
    addToSummary(summaryEl, cv.component, cv.rules[0].message);
  }
  return false;
}

type FieldResolution = { done: false; el: HTMLElement } | { done: true; result: boolean };

/** Resolve the field element. On resolution error, returns early result (true if all skipped, false otherwise). */
function resolveFieldElement(
  plan: Plan, cv: ComponentValidation, summaryEl: HTMLElement | null, ctx?: ExecContext,
): FieldResolution {
  try {
    return { done: false, el: resolveElement(plan, cv.component) };
  } catch (e) {
    if (!isResolutionError(e)) throw e;
    if (allRulesConditionallySkipped(cv.rules, plan, ctx)) return { done: true, result: true };
    if (cv.rules.length > 0 && summaryEl) {
      addToSummary(summaryEl, cv.component, cv.rules[0].message);
    }
    return { done: true, result: false };
  }
}

/**
 * Error spans are generated HTML elements ({componentDomId}_error), NOT plan components.
 * They are created by Html.Field() in C# and follow a predictable ID convention.
 * getElementById is correct here — error spans are not registered in plan.components.
 */
function isErrorSpanHidden(compId: string): boolean {
  const errorSpan = document.getElementById(compId + "_error");
  return errorSpan?.parentElement ? isHidden(errorSpan.parentElement) : true;
}

/**
 * Read value via the shared evaluateValue — same concept as pipeline and gather.
 * Component may not be resolved yet (partial not merged) — suppress resolution errors only.
 */
function readValueSafe(valueProducer: ValueProducer, plan: Plan): unknown {
  try {
    return evaluateValue(valueProducer, plan);
  } catch (e) {
    if (isResolutionError(e)) return undefined;
    throw e;
  }
}

/** Evaluate each rule against the field value. Returns false on first failure. */
function evaluateRulesForField(
  cv: ComponentValidation, plan: Plan, containerId: string, compId: string,
  value: unknown, hidden: boolean, summaryEl: HTMLElement | null, ctx?: ExecContext,
): boolean {
  for (const rule of cv.rules) {
    if (!isRuleActive(rule, plan, ctx)) continue;

    const otherValue = resolveOtherValue(rule, plan);

    if (ruleFails(rule, value, otherValue)) {
      reportRuleFailure(cv.component, rule, value, containerId, compId, hidden, summaryEl);
      return false;
    }
  }
  return true;
}

/** Check if a rule's condition is met. Returns false if condition skips this rule. */
function isRuleActive(rule: ValidationRule, plan: Plan, ctx?: ExecContext): boolean {
  if (rule.when.kind === "none") return true;
  try {
    return evaluateCondition(rule.when, plan, ctx);
  } catch (e) {
    if (isResolutionError(e)) return false;
    throw e;
  }
}

/** Pre-resolve otherValue if present — keeps rule-engine pure (no DOM). */
function resolveOtherValue(rule: ValidationRule, plan: Plan): unknown {
  if (!isEvaluable(rule.otherValue)) return undefined;
  try {
    return evaluateValue(rule.otherValue, plan);
  } catch (e) {
    if (isResolutionError(e)) return undefined;
    throw e;
  }
}

/** Show the error inline or in the summary depending on visibility. */
function reportRuleFailure(
  component: string, rule: ValidationRule, value: unknown,
  containerId: string, compId: string, hidden: boolean, summaryEl: HTMLElement | null,
): void {
  log.trace("rule.failed", { component, rule: rule.name, value, message: rule.message });
  if (hidden) {
    if (summaryEl) addToSummary(summaryEl, component, rule.message);
  } else {
    showInline(containerId, compId, rule.message);
    if (summaryEl) removeSummaryEntry(summaryEl, component);
  }
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
    if (rule.when.kind === "none") return false;
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
  for (const cv of containerScope.validationRules) {
    const comp = plan.components[cv.component];
    if (comp) clearInline(containerId, comp.id);
  }
  if (summaryEl) {
    clearSummary(summaryEl);
    hideSummaryDiv(summaryEl);
  }
}

function findComponentKeyByName(containerScope: ContainerScope, name: string): string | undefined {
  // Plan-driven: each ComponentValidation carries serverFieldName set at C# build time.
  // No heuristics — the plan declares the mapping.
  return containerScope.validationRules.find(
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
  log.warn("server-errors.wrong-shape");
  return null;
}
