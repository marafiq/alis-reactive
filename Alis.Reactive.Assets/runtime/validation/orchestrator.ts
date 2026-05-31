// Validation Orchestrator — Fail-Closed, V3 ContainerScope
//
// Uses SHARED evaluateValue for ALL component value reads.
// No parallel read path — same concept as pipeline and gather.

import type {
  PlanDocument, ValidationContainerScope, ComponentValidation,
  ValidationRule,
  ValidationRuleActivation as PlanValidationRuleActivation,
  PeerEqualityValidationRule,
  PeerOrderedComparisonValidationRule,
} from "../types";
import type { ExecContext } from "../types";
import { RuntimePlan, RuntimeResolutionError, type RuntimeComponent } from "../domain/runtime-plan";
import { evaluateCondition } from "../conditions/conditions";
import { evaluateValue } from "../core/evaluate";
import { scope } from "../core/trace";
import { toString } from "../core/shape-convert";
import { ruleFails } from "./rule-engine";
import {
  showInline, clearInline,
  addToSummary, removeSummaryEntry, hasSummaryEntry, clearSummary, showSummaryDiv, hideSummaryDiv, findSummaryElement,
  showServerErrorInline,
} from "./error-display";
import { ExecutionContext } from "../domain/execution-context";
import { objectRecordFrom } from "../domain/object-record";
import { assertNever } from "../core/assert-never";

const log = scope("validation");

interface ValidationSummary {
  readonly element: HTMLElement | undefined;
}

function validationSummaryForPlan(planId: string): ValidationSummary {
  return { element: findSummaryElement(planId) ?? undefined };
}

function addSummaryError(summary: ValidationSummary, componentKey: string, message: string): boolean {
  if (summary.element === undefined) return false;

  addToSummary(summary.element, componentKey, message);
  return true;
}

function removeSummaryError(summary: ValidationSummary, componentKey: string): void {
  if (summary.element === undefined) return;

  removeSummaryEntry(summary.element, componentKey);
}

function summaryHasError(summary: ValidationSummary, componentKey: string): boolean {
  if (summary.element === undefined) return false;

  return hasSummaryEntry(summary.element, componentKey);
}

function showSummaryWhen(summary: ValidationSummary, hasErrors: boolean): void {
  if (summary.element === undefined) return;
  if (hasErrors) showSummaryDiv(summary.element);
}

function clearAndHideSummary(summary: ValidationSummary): void {
  if (summary.element === undefined) return;

  clearSummary(summary.element);
  hideSummaryDiv(summary.element);
}

interface ValidationSurface {
  readonly plan: PlanDocument;
  readonly runtime: RuntimePlan;
  readonly containerId: string;
  readonly containerScope: ValidationContainerScope;
  readonly summary: ValidationSummary;
  readonly context: ExecutionContext;
}

interface FieldEvaluation {
  readonly componentValidation: ComponentValidation;
  readonly componentDomId: string;
  readonly value: unknown;
  readonly hidden: boolean;
}

// -- Public API --

export function validateContainer(plan: PlanDocument, containerKey: string, ctx?: ExecContext): boolean {
  const runtime = RuntimePlan.from(plan);
  const containerComp = runtime.components.find(containerKey);
  if (!containerComp) {
    log.warn("container.not-found", { id: containerKey });
    return false;
  }

  const containerScope = containerComp.containerScope;
  if (!containerScope) {
    log.warn("container.no-scope", { id: containerComp.id });
    return true;
  }

  const containerId = containerComp.id;
  let container: HTMLElement;
  try {
    container = containerComp.element();
  } catch (e) {
    if (!RuntimeResolutionError.is(e)) throw e;
    if (containerScope.validationRules.length > 0) {
      log.warn("form.missing", { id: containerId });
      return false;
    }
    return true;
  }

  const summary = validationSummaryForPlan(plan.planId);
  const surface: ValidationSurface = {
    plan,
    runtime,
    containerId,
    containerScope,
    summary,
    context: ExecutionContext.from(ctx),
  };

  clearContainerErrors(surface);

  if (containerScope.validationRules.length === 0) {
    return true;
  }

  let valid = true;
  let summaryHasErrors = false;

  for (const cv of containerScope.validationRules) {
    if (!evaluateComponentRules(cv, surface, container)) {
      valid = false;
      summaryHasErrors = summaryHasErrors || summaryHasError(surface.summary, cv.component);
    }
  }

  showSummaryWhen(surface.summary, summaryHasErrors);

  log.debug("validated", { id: containerId, valid });
  return valid;
}

export function showServerErrors(plan: PlanDocument, containerKey: string, data: unknown): void {
  const runtime = RuntimePlan.from(plan);
  const containerComp = runtime.components.find(containerKey);
  const containerScope = containerComp?.containerScope;
  if (!containerComp || !containerScope) return;

  const containerId = containerComp.id;
  const summary = validationSummaryForPlan(plan.planId);
  const surface: ValidationSurface = {
    plan,
    runtime,
    containerId,
    containerScope,
    summary,
    context: ExecutionContext.absent(),
  };

  clearContainerErrors(surface);

  const errors = serverValidationErrorsFrom(data);
  if (errors.kind === "wrong-shape") log.warn("server-errors.wrong-shape");
  if (errors.kind !== "field-errors") return;
  if (errors.fields.length === 0) return;

  let summaryHasErrors = false;

  for (const error of errors.fields) {
    const addedToSummary = placeServerError(error, surface);
    if (addedToSummary) summaryHasErrors = true;
  }

  showSummaryWhen(surface.summary, summaryHasErrors);
  log.debug("server-errors.shown", { id: containerId, fieldCount: errors.fields.length });
}

/** Place a single server error on its component or into the summary. Returns true if any summary errors added. */
function placeServerError(
  error: ServerValidationError,
  surface: ValidationSurface,
): boolean {
  const msg = serverValidationErrorMessage(error.messages);
  const validation = findComponentValidationByName(surface, error.name);
  if (validation === undefined) return addSummaryError(surface.summary, error.name, msg);

  const component = surface.runtime.components.find(validation.component);
  if (component === undefined) return addSummaryError(surface.summary, error.name, msg);

  const element = component.tryElement();
  if (element === undefined) return addSummaryError(surface.summary, error.name, msg);

  const inlineMessageSlotCanRender = canRenderInlineValidationMessage(component.id);
  if (inlineMessageSlotCanRender) {
    showServerErrorInline(component.id, msg, element);
    return false;
  }

  return addSummaryError(surface.summary, error.name, msg);
}

/**
 * Re-validate a single component within its container.
 * Called on blur/change by live-clear to give immediate field-level feedback.
 */
export function revalidateField(plan: PlanDocument, containerKey: string, componentKey: string): void {
  const runtime = RuntimePlan.from(plan);
  const containerComp = runtime.components.find(containerKey);
  const containerScope = containerComp?.containerScope;
  if (!containerComp || !containerScope) return;

  const cv = containerScope.validationRules.find(r => r.component === componentKey);
  if (!cv) return;

  const containerId = containerComp.id;

  // Clear existing error for this field
  const comp = runtime.components.find(componentKey);
  if (comp) clearInline(comp.id);

  // Find the container element
  let container: HTMLElement;
  try {
    container = containerComp.element();
  } catch (e) {
    if (!RuntimeResolutionError.is(e)) throw e;
    return;
  }

  const summary = validationSummaryForPlan(plan.planId);
  const surface: ValidationSurface = {
    plan,
    runtime,
    containerId,
    containerScope,
    summary,
    context: ExecutionContext.absent(),
  };

  evaluateComponentRules(cv, surface, container);
}

export function clearContainerValidation(plan: PlanDocument, containerKey: string): void {
  const runtime = RuntimePlan.from(plan);
  const containerComp = runtime.components.find(containerKey);
  const containerScope = containerComp?.containerScope;
  if (!containerComp || !containerScope) return;

  const containerId = containerComp.id;
  const summary = validationSummaryForPlan(plan.planId);
  clearContainerErrors({
    plan,
    runtime,
    containerId,
    containerScope,
    summary,
    context: ExecutionContext.absent(),
  });
}

// -- Per-component evaluation --

function evaluateComponentRules(
  cv: ComponentValidation,
  surface: ValidationSurface,
  container: HTMLElement,
): boolean {
  const comp = surface.runtime.components.find(cv.component);
  if (!comp) return handleInactiveValidationField(cv, surface);

  const resolved = resolveFieldElement(comp, cv, surface);
  if (resolved.done) return resolved.result;

  if (!container.contains(resolved.element)) {
    log.trace("field.out-of-scope", { component: cv.component, containerId: surface.containerId });
    return true;
  }

  const hidden = isErrorSpanHidden(comp.id);
  const value = evaluateValue(cv.value, surface.plan);
  const field: FieldEvaluation = {
    componentValidation: cv,
    componentDomId: comp.id,
    value,
    hidden,
  };

  return evaluateRulesForField(field, surface);
}

/** An unloaded partial field is valid only when its active rules are skipped. */
function handleInactiveValidationField(
  cv: ComponentValidation,
  surface: ValidationSurface,
): boolean {
  log.trace("validation-field.inactive", { component: cv.component });
  if (allRulesInactiveForUnmountedField(cv.rules, surface)) return true;
  const message = firstRuleMessage(cv);
  if (message !== undefined) {
    addSummaryError(surface.summary, cv.component, message);
  }
  return false;
}

type FieldResolution = { done: false; element: HTMLElement } | { done: true; result: boolean };

/** Resolve the field element. On resolution error, returns early result (true if all skipped, false otherwise). */
function resolveFieldElement(
  component: RuntimeComponent,
  cv: ComponentValidation,
  surface: ValidationSurface,
): FieldResolution {
  try {
    return { done: false, element: component.element() };
  } catch (e) {
    if (!RuntimeResolutionError.is(e)) throw e;
    if (allRulesInactiveForUnmountedField(cv.rules, surface)) return { done: true, result: true };
    const message = firstRuleMessage(cv);
    if (message !== undefined) {
      addSummaryError(surface.summary, cv.component, message);
    }
    return { done: true, result: false };
  }
}

function firstRuleMessage(cv: ComponentValidation): string | undefined {
  return cv.rules[0]?.message;
}

/**
 * Error spans are generated HTML elements ({componentDomId}_error), NOT plan components.
 * They are created by Html.Field() in C# and follow a predictable ID convention.
 * getElementById is correct here — error spans are not registered in plan.components.
 */
function isErrorSpanHidden(compId: string): boolean {
  return !canRenderInlineValidationMessage(compId);
}

function canRenderInlineValidationMessage(compId: string): boolean {
  const errorSpan = document.getElementById(compId + "_error");
  const parent = errorSpan?.parentElement;
  const messageSlotWasNotRendered = parent === null || parent === undefined;
  if (messageSlotWasNotRendered) return false;

  return !isHidden(parent);
}

/** Evaluate each rule against the field value. Returns false on first failure. */
function evaluateRulesForField(
  field: FieldEvaluation,
  surface: ValidationSurface,
): boolean {
  for (const rule of field.componentValidation.rules) {
    if (!isRuleActive(rule.execution.activation, surface)) continue;

    if (failsRule(rule, field.value, surface.plan)) {
      reportRuleFailure(field, rule, surface);
      return false;
    }
  }
  return true;
}

/** Show the error inline or in the summary depending on visibility. */
function reportRuleFailure(
  field: FieldEvaluation,
  rule: ValidationRule,
  surface: ValidationSurface,
): void {
  const component = field.componentValidation.component;
  log.trace("rule.failed", { component, rule: rule.name, value: field.value, message: rule.message });
  if (field.hidden) {
    addSummaryError(surface.summary, component, rule.message);
  } else {
    showInline(field.componentDomId, rule.message);
    removeSummaryError(surface.summary, component);
  }
}

// -- Helpers --

function allRulesInactiveForUnmountedField(rules: ValidationRule[], surface: ValidationSurface): boolean {
  if (rules.length === 0) return true;
  for (const rule of rules) {
    if (!isRuleInactiveWhenFieldIsUnmounted(rule.execution.activation, surface)) return false;
  }
  return true;
}

function isRuleActive(
  activation: PlanValidationRuleActivation,
  surface: ValidationSurface,
): boolean {
  switch (activation.kind) {
    case "always": return true;
    case "when": return evaluateCondition(activation.condition, surface.plan, surface.context.raw);
    default: return assertNever(activation, "validation rule activation");
  }
}

function isRuleInactiveWhenFieldIsUnmounted(
  activation: PlanValidationRuleActivation,
  surface: ValidationSurface,
): boolean {
  switch (activation.kind) {
    case "always": return false;
    case "when":
      try {
        return !evaluateCondition(activation.condition, surface.plan, surface.context.raw);
      } catch (e) {
        if (RuntimeResolutionError.is(e)) return true;
        throw e;
      }
    default: return assertNever(activation, "validation rule activation");
  }
}

function failsRule(
  rule: ValidationRule,
  value: unknown,
  plan: PlanDocument,
): boolean {
  if (hasPeerTarget(rule)) {
    return ruleFails({
      rule,
      value,
      peerValue: evaluateValue(rule.execution.value, plan),
    });
  }

  return ruleFails({ rule, value });
}

type PeerTargetValidationRule = PeerEqualityValidationRule | PeerOrderedComparisonValidationRule;

function hasPeerTarget(rule: ValidationRule): rule is PeerTargetValidationRule {
  return rule.execution.kind === "peer";
}

function clearContainerErrors(
  surface: ValidationSurface,
): void {
  for (const cv of surface.containerScope.validationRules) {
    const comp = surface.runtime.components.find(cv.component);
    if (comp) clearInline(comp.id);
  }
  clearAndHideSummary(surface.summary);
}

function findComponentValidationByName(surface: ValidationSurface, name: string): ComponentValidation | undefined {
  // PlanDocument-driven: each ComponentValidation carries serverFieldName set at C# build time.
  // No heuristics — the plan declares the mapping.
  return surface.containerScope.validationRules.find(cv => matchesServerErrorName(cv, name));
}

function matchesServerErrorName(componentValidation: ComponentValidation, serverFieldName: string): boolean {
  return componentValidation.serverFieldName === serverFieldName;
}

function isHidden(el: HTMLElement): boolean {
  let node: HTMLElement | null = el;
  while (node) {
    const nodeIsHidden = node.hasAttribute("hidden") || node.style?.display === "none";
    if (nodeIsHidden) return true;
    node = node.parentElement;
  }
  return false;
}

interface ServerValidationError {
  readonly name: string;
  readonly messages: unknown;
}

type ServerValidationErrors =
  | { readonly kind: "not-validation-payload" }
  | { readonly kind: "wrong-shape" }
  | { readonly kind: "field-errors"; readonly fields: ServerValidationError[] };

function serverValidationErrorsFrom(data: unknown): ServerValidationErrors {
  const payload = objectRecordFrom(data);
  if (payload === undefined) return { kind: "not-validation-payload" };

  const errors = objectRecordFrom(payload["errors"]);
  if (errors === undefined) return { kind: "wrong-shape" };

  return {
    kind: "field-errors",
    fields: Object.entries(errors).map(([name, messages]) => ({ name, messages })),
  };
}

function serverValidationErrorMessage(messages: unknown): string {
  if (Array.isArray(messages)) return messages.join(", ");

  const message = toString(messages);
  if (message.ok) return message.value;

  return "";
}
