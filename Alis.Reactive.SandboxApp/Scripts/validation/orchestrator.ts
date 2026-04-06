// Validation Orchestrator — Fail-Closed, V3 ContainerScope
//
// Uses SHARED resolver for ALL component value reads.
// This module NEVER touches JsType internals — readDefaultValue handles it.

import type {
  Plan, ContainerScope, ComponentValidation,
  ValidationRule,
} from "../types";
import type { ExecContext } from "../types";
import { readDefaultValue, resolveElement } from "../resolution/resolver";
import { evaluateCondition } from "../conditions/conditions";
import { scope } from "../core/trace";
import { toString } from "../core/shape-convert";
import { ruleFails, type PeerReader } from "./rule-engine";
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
  } catch {
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

  const peerReader = createPeerReader(plan);
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

  let el: HTMLElement;
  try {
    el = resolveElement(plan, cv.component);
  } catch {
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

  // ONE call to the shared resolver — no JsType internals here
  let value: unknown;
  try {
    value = readDefaultValue(plan, cv.component);
  } catch {
    value = undefined;
  }

  for (const rule of cv.rules) {
    if (rule.when) {
      const condResult = evaluateCondition(rule.when, plan, ctx);
      if (!condResult) continue;
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

function createPeerReader(plan: Plan): PeerReader {
  return {
    readPeer(componentKey: string): unknown {
      try {
        return readDefaultValue(plan, componentKey);
      } catch {
        return undefined;
      }
    },
  };
}

function allRulesConditionallySkipped(rules: ValidationRule[], plan: Plan, ctx?: ExecContext): boolean {
  if (rules.length === 0) return true;
  for (const rule of rules) {
    if (!rule.when) return false;
    const result = evaluateCondition(rule.when, plan, ctx);
    if (result) return false;
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
