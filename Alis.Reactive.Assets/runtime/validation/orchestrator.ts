// Validation uses the same ValueExpression resolver as execution and gather so component reads stay consistent.

import type {
  PlanDocument, ValidationContainerScope, ComponentValidation,
  ValidationRule,
  ValidationRuleActivation as PlanValidationRuleActivation,
  PeerEqualityValidationRule,
  PeerOrderedComparisonValidationRule,
} from "../types/index";
import type { ExecContext } from "../types/index";
import { RuntimePlan, RuntimeResolutionError, type RuntimeComponent } from "../browser-objects/runtime-plan";
import { evaluateCondition } from "../conditions/conditions";
import { evaluateValue } from "../values/evaluate";
import { scope } from "../diagnostics/trace";
import { toString } from "../shared/shape-convert";
import { ruleFails } from "./rule-engine";
import {
  showInline, clearInline,
  addToSummary, removeSummaryEntry, hasSummaryEntry, clearSummary, showSummaryDiv, hideSummaryDiv, findSummaryElement,
  showServerErrorInline,
} from "./error-display";
import { ExecutionContext } from "../browser-objects/execution-context";
import { objectRecordFrom } from "../browser-objects/object-record";
import { assertNever } from "../shared/assert-never";

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
  readonly planDocument: PlanDocument;
  readonly runtimePlan: RuntimePlan;
  readonly containerId: string;
  readonly containerScope: ValidationContainerScope;
  readonly summary: ValidationSummary;
  readonly context: ExecutionContext;
}

interface FieldEvaluation {
  readonly componentValidation: ComponentValidation;
  readonly componentDomId: string;
  readonly value: unknown;
  readonly inlineMessageHidden: boolean;
}

export function validateContainer(
  planDocument: PlanDocument,
  containerKey: string,
  context?: ExecContext,
): boolean {
  const runtimePlan = RuntimePlan.from(planDocument);
  const containerComponent = runtimePlan.components.find(containerKey);
  if (!containerComponent) {
    log.warn("container.not-found", { id: containerKey });
    return false;
  }

  const containerScope = containerComponent.containerScope;
  if (!containerScope) {
    log.warn("container.no-scope", { id: containerComponent.id });
    return true;
  }

  const containerId = containerComponent.id;
  let container: HTMLElement;
  try {
    container = containerComponent.element();
  } catch (error) {
    if (!RuntimeResolutionError.is(error)) throw error;
    if (containerScope.validationRules.length > 0) {
      log.warn("form.missing", { id: containerId });
      return false;
    }
    return true;
  }

  const summary = validationSummaryForPlan(planDocument.planId);
  const surface: ValidationSurface = {
    planDocument,
    runtimePlan,
    containerId,
    containerScope,
    summary,
    context: ExecutionContext.from(context),
  };

  clearContainerErrors(surface);

  if (containerScope.validationRules.length === 0) {
    return true;
  }

  let containerIsValid = true;
  let summaryReceivedErrors = false;

  for (const componentValidation of containerScope.validationRules) {
    if (!evaluateComponentRules(componentValidation, surface, container)) {
      containerIsValid = false;
      summaryReceivedErrors = summaryReceivedErrors || summaryHasError(surface.summary, componentValidation.component);
    }
  }

  showSummaryWhen(surface.summary, summaryReceivedErrors);

  log.debug("validated", { id: containerId, valid: containerIsValid });
  return containerIsValid;
}

export function showServerErrors(planDocument: PlanDocument, containerKey: string, serverPayload: unknown): void {
  const runtimePlan = RuntimePlan.from(planDocument);
  const containerComponent = runtimePlan.components.find(containerKey);
  const containerScope = containerComponent?.containerScope;
  if (!containerComponent || !containerScope) return;

  const containerId = containerComponent.id;
  const summary = validationSummaryForPlan(planDocument.planId);
  const surface: ValidationSurface = {
    planDocument,
    runtimePlan,
    containerId,
    containerScope,
    summary,
    context: ExecutionContext.absent(),
  };

  clearContainerErrors(surface);

  const errors = serverValidationErrorsFrom(serverPayload);
  if (errors.kind === "wrong-shape") log.warn("server-errors.wrong-shape");
  if (errors.kind !== "field-errors") return;
  if (errors.fields.length === 0) return;

  let summaryReceivedErrors = false;

  for (const error of errors.fields) {
    const addedToSummary = placeServerErrorOnFieldOrSummary(error, surface);
    if (addedToSummary) summaryReceivedErrors = true;
  }

  showSummaryWhen(surface.summary, summaryReceivedErrors);
  log.debug("server-errors.shown", { id: containerId, fieldCount: errors.fields.length });
}

function placeServerErrorOnFieldOrSummary(
  error: ServerValidationError,
  surface: ValidationSurface,
): boolean {
  const message = serverValidationErrorMessage(error.messages);
  const validation = findComponentValidationByName(surface, error.name);
  if (validation === undefined) return addSummaryError(surface.summary, error.name, message);

  const component = surface.runtimePlan.components.find(validation.component);
  if (component === undefined) return addSummaryError(surface.summary, error.name, message);

  const element = component.tryElement();
  if (element === undefined) return addSummaryError(surface.summary, error.name, message);

  const inlineMessageSlotCanRender = canRenderInlineValidationMessage(component.id);
  if (inlineMessageSlotCanRender) {
    showServerErrorInline(component.id, message, element);
    return false;
  }

  return addSummaryError(surface.summary, error.name, message);
}

export function revalidateField(planDocument: PlanDocument, containerKey: string, componentKey: string): void {
  const runtimePlan = RuntimePlan.from(planDocument);
  const containerComponent = runtimePlan.components.find(containerKey);
  const containerScope = containerComponent?.containerScope;
  if (!containerComponent || !containerScope) return;

  const componentValidation = containerScope.validationRules.find(
    validation => validation.component === componentKey,
  );
  if (!componentValidation) return;

  const containerId = containerComponent.id;

  const component = runtimePlan.components.find(componentKey);
  if (component) clearInline(component.id);

  let container: HTMLElement;
  try {
    container = containerComponent.element();
  } catch (error) {
    if (!RuntimeResolutionError.is(error)) throw error;
    return;
  }

  const summary = validationSummaryForPlan(planDocument.planId);
  const surface: ValidationSurface = {
    planDocument,
    runtimePlan,
    containerId,
    containerScope,
    summary,
    context: ExecutionContext.absent(),
  };

  evaluateComponentRules(componentValidation, surface, container);
}

export function clearContainerValidation(planDocument: PlanDocument, containerKey: string): void {
  const runtimePlan = RuntimePlan.from(planDocument);
  const containerComponent = runtimePlan.components.find(containerKey);
  const containerScope = containerComponent?.containerScope;
  if (!containerComponent || !containerScope) return;

  const containerId = containerComponent.id;
  const summary = validationSummaryForPlan(planDocument.planId);
  clearContainerErrors({
    planDocument,
    runtimePlan,
    containerId,
    containerScope,
    summary,
    context: ExecutionContext.absent(),
  });
}

function evaluateComponentRules(
  componentValidation: ComponentValidation,
  surface: ValidationSurface,
  container: HTMLElement,
): boolean {
  const component = surface.runtimePlan.components.find(componentValidation.component);
  if (!component) return handleInactiveValidationField(componentValidation, surface);

  const resolved = resolveFieldElement(component, componentValidation, surface);
  if (resolved.done) return resolved.result;

  if (!container.contains(resolved.element)) {
    log.trace("field.out-of-scope", { component: componentValidation.component, containerId: surface.containerId });
    return true;
  }

  const inlineMessageHidden = isErrorSpanHidden(component.id);
  const value = evaluateValue(componentValidation.value, surface.planDocument);
  const field: FieldEvaluation = {
    componentValidation,
    componentDomId: component.id,
    value,
    inlineMessageHidden,
  };

  return evaluateRulesForField(field, surface);
}

/** Unmounted partial fields fail unless every active rule is skipped. */
function handleInactiveValidationField(
  componentValidation: ComponentValidation,
  surface: ValidationSurface,
): boolean {
  log.trace("validation-field.inactive", { component: componentValidation.component });
  if (allRulesInactiveForUnmountedField(componentValidation.rules, surface)) return true;
  const message = firstRuleMessage(componentValidation);
  if (message !== undefined) {
    addSummaryError(surface.summary, componentValidation.component, message);
  }
  return false;
}

type FieldResolution = { done: false; element: HTMLElement } | { done: true; result: boolean };

/** Missing partial field elements use the same skipped-rule invariant as unregistered fields. */
function resolveFieldElement(
  component: RuntimeComponent,
  componentValidation: ComponentValidation,
  surface: ValidationSurface,
): FieldResolution {
  try {
    return { done: false, element: component.element() };
  } catch (error) {
    if (!RuntimeResolutionError.is(error)) throw error;
    if (allRulesInactiveForUnmountedField(componentValidation.rules, surface)) return { done: true, result: true };
    const message = firstRuleMessage(componentValidation);
    if (message !== undefined) {
      addSummaryError(surface.summary, componentValidation.component, message);
    }
    return { done: true, result: false };
  }
}

function firstRuleMessage(componentValidation: ComponentValidation): string | undefined {
  return componentValidation.rules[0]?.message;
}

// Error spans are generated by Html.Field(), not plan components.
function isErrorSpanHidden(componentDomId: string): boolean {
  return !canRenderInlineValidationMessage(componentDomId);
}

function canRenderInlineValidationMessage(componentDomId: string): boolean {
  const errorSpan = document.getElementById(componentDomId + "_error");
  const parent = errorSpan?.parentElement;
  const messageSlotWasNotRendered = parent === null || parent === undefined;
  if (messageSlotWasNotRendered) return false;

  return !isHidden(parent);
}

function evaluateRulesForField(
  field: FieldEvaluation,
  surface: ValidationSurface,
): boolean {
  for (const rule of field.componentValidation.rules) {
    if (!isRuleActive(rule.execution.activation, surface)) continue;

    if (failsRule(rule, field.value, surface.planDocument)) {
      reportRuleFailure(field, rule, surface);
      return false;
    }
  }
  return true;
}

function reportRuleFailure(
  field: FieldEvaluation,
  rule: ValidationRule,
  surface: ValidationSurface,
): void {
  const component = field.componentValidation.component;
  log.trace("rule.failed", { component, rule: rule.name, value: field.value, message: rule.message });
  if (field.inlineMessageHidden) {
    addSummaryError(surface.summary, component, rule.message);
  } else {
    showInline(field.componentDomId, rule.message);
    removeSummaryError(surface.summary, component);
  }
}

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
    case "when": return evaluateCondition(activation.condition, surface.planDocument, surface.context.raw);
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
        return !evaluateCondition(activation.condition, surface.planDocument, surface.context.raw);
      } catch (error) {
        if (RuntimeResolutionError.is(error)) return true;
        throw error;
      }
    default: return assertNever(activation, "validation rule activation");
  }
}

function failsRule(
  rule: ValidationRule,
  value: unknown,
  planDocument: PlanDocument,
): boolean {
  if (hasPeerTarget(rule)) {
    return ruleFails({
      rule,
      value,
      peerValue: evaluateValue(rule.execution.value, planDocument),
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
  for (const componentValidation of surface.containerScope.validationRules) {
    const component = surface.runtimePlan.components.find(componentValidation.component);
    if (component) clearInline(component.id);
  }
  clearAndHideSummary(surface.summary);
}

function findComponentValidationByName(surface: ValidationSurface, name: string): ComponentValidation | undefined {
  // Server field names are emitted into the plan at C# build time; no heuristics here.
  return surface.containerScope.validationRules.find(componentValidation =>
    matchesServerErrorName(componentValidation, name));
}

function matchesServerErrorName(componentValidation: ComponentValidation, serverFieldName: string): boolean {
  return componentValidation.serverFieldName === serverFieldName;
}

function isHidden(element: HTMLElement): boolean {
  let node: HTMLElement | null = element;
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

function serverValidationErrorsFrom(serverPayload: unknown): ServerValidationErrors {
  const payload = objectRecordFrom(serverPayload);
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
