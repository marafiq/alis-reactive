// Validation Orchestrator — Fail-Closed, V3 ContainerScope
//
// Uses SHARED evaluateValue for ALL component value reads.
// No parallel read path — same concept as pipeline and gather.

import type {
  Plan, ValidationContainerComponent, ComponentValidation,
  ValidationRule, ValueProducer,
  Condition,
  ValidationRuleActivation as PlanValidationRuleActivation,
  ValidationRuleOperand as PlanValidationRuleOperand,
} from "../types";
import type { ExecContext } from "../types";
import { RuntimePlan, RuntimeResolutionError, type RuntimeComponent } from "../domain/runtime-plan";
import { evaluateCondition } from "../conditions/conditions";
import { evaluateValue } from "../core/evaluate";
import { scope } from "../core/trace";
import { toString } from "../core/shape-convert";
import type { ResolvedPeerValue } from "./rule-engine";
import { noPeerValue, peerValue, ruleFails } from "./rule-engine";
import {
  showInline, clearInline,
  addToSummary, removeSummaryEntry, clearSummary, showSummaryDiv, hideSummaryDiv, findSummaryElement,
  showServerErrorInline,
} from "./error-display";
import { ExecutionContext } from "../domain/execution-context";
import { ObjectRecord } from "../domain/object-record";
import { assertNever } from "../core/assert-never";

const log = scope("validation");

abstract class ValidationSummary {
  static forPlan(planId: string): ValidationSummary {
    const element = findSummaryElement(planId);
    if (element === null) return MissingValidationSummary.instance;

    return new RenderedValidationSummary(element);
  }

  abstract add(componentKey: string, message: string): boolean;

  abstract remove(componentKey: string): void;

  abstract hasEntry(componentKey: string): boolean;

  abstract showWhen(hasErrors: boolean): void;

  abstract clearAndHide(): void;
}

class RenderedValidationSummary extends ValidationSummary {
  constructor(private readonly element: HTMLElement) {
    super();
  }

  add(componentKey: string, message: string): boolean {
    addToSummary(this.element, componentKey, message);
    return true;
  }

  remove(componentKey: string): void {
    removeSummaryEntry(this.element, componentKey);
  }

  hasEntry(componentKey: string): boolean {
    return this.element.querySelector(`[data-valmsg-summary-for="${componentKey}"]`) !== null;
  }

  showWhen(hasErrors: boolean): void {
    if (hasErrors) showSummaryDiv(this.element);
  }

  clearAndHide(): void {
    clearSummary(this.element);
    hideSummaryDiv(this.element);
  }
}

class MissingValidationSummary extends ValidationSummary {
  static readonly instance = new MissingValidationSummary();

  add(): boolean {
    return false;
  }

  remove(): void {
    return;
  }

  hasEntry(): boolean {
    return false;
  }

  showWhen(): void {
    return;
  }

  clearAndHide(): void {
    return;
  }
}

interface ValidationSurface {
  readonly plan: Plan;
  readonly runtime: RuntimePlan;
  readonly containerId: string;
  readonly containerScope: ValidationContainerComponent;
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

export function validateContainer(plan: Plan, containerKey: string, ctx?: ExecContext): boolean {
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
    if (!isResolutionError(e)) throw e;
    if (containerScope.validationRules.length > 0) {
      log.warn("form.missing", { id: containerId });
      return false;
    }
    return true;
  }

  const summary = ValidationSummary.forPlan(plan.planId);
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
      summaryHasErrors = summaryHasErrors || surface.summary.hasEntry(cv.component);
    }
  }

  surface.summary.showWhen(summaryHasErrors);

  log.debug("validated", { id: containerId, valid });
  return valid;
}

export function showServerErrors(plan: Plan, containerKey: string, data: unknown): void {
  const runtime = RuntimePlan.from(plan);
  const containerComp = runtime.components.find(containerKey);
  const containerScope = containerComp?.containerScope;
  if (!containerComp || !containerScope) return;

  const containerId = containerComp.id;
  const summary = ValidationSummary.forPlan(plan.planId);
  const surface: ValidationSurface = {
    plan,
    runtime,
    containerId,
    containerScope,
    summary,
    context: ExecutionContext.absent(),
  };

  clearContainerErrors(surface);

  const errors = ServerValidationErrors.from(data);
  if (errors.wrongShape) log.warn("server-errors.wrong-shape");
  if (!errors.found) return;

  let summaryHasErrors = false;

  for (const error of errors.fields) {
    const addedToSummary = placeServerError(error, surface);
    if (addedToSummary) summaryHasErrors = true;
  }

  surface.summary.showWhen(summaryHasErrors);
  log.debug("server-errors.shown", { id: containerId, fieldCount: errors.fields.length });
}

/** Place a single server error on its component or into the summary. Returns true if any summary errors added. */
function placeServerError(
  error: ServerValidationError,
  surface: ValidationSurface,
): boolean {
  const msg = ServerValidationErrorMessage.from(error.messages);
  return ServerErrorPlacementTarget
    .for(error.name, surface)
    .place(msg, surface);
}

abstract class ServerErrorPlacementTarget {
  static for(serverFieldName: string, surface: ValidationSurface): ServerErrorPlacementTarget {
    const validation = findComponentValidationByName(surface, serverFieldName);
    if (validation === undefined) return new SummaryServerErrorTarget(serverFieldName);

    const component = surface.runtime.components.find(validation.component);
    if (component === undefined) return new SummaryServerErrorTarget(serverFieldName);

    const element = component.tryElement();
    if (element === undefined) return new SummaryServerErrorTarget(serverFieldName);

    const inlineMessageSlotCanRender = canRenderInlineValidationMessage(component.id);
    if (!inlineMessageSlotCanRender) return new SummaryServerErrorTarget(serverFieldName);

    return new InlineServerErrorTarget(component.id, element);
  }

  abstract place(message: string, surface: ValidationSurface): boolean;
}

class InlineServerErrorTarget extends ServerErrorPlacementTarget {
  constructor(
    private readonly componentDomId: string,
    private readonly element: HTMLElement,
  ) {
    super();
  }

  place(message: string): boolean {
    showServerErrorInline(this.componentDomId, message, this.element);
    return false;
  }
}

class SummaryServerErrorTarget extends ServerErrorPlacementTarget {
  constructor(private readonly serverFieldName: string) {
    super();
  }

  place(message: string, surface: ValidationSurface): boolean {
    return surface.summary.add(this.serverFieldName, message);
  }
}

/**
 * Re-validate a single component within its container.
 * Called on blur/change by live-clear to give immediate field-level feedback.
 */
export function revalidateField(plan: Plan, containerKey: string, componentKey: string): void {
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
    if (!isResolutionError(e)) throw e;
    return;
  }

  const summary = ValidationSummary.forPlan(plan.planId);
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

export function clearContainerValidation(plan: Plan, containerKey: string): void {
  const runtime = RuntimePlan.from(plan);
  const containerComp = runtime.components.find(containerKey);
  const containerScope = containerComp?.containerScope;
  if (!containerComp || !containerScope) return;

  const containerId = containerComp.id;
  const summary = ValidationSummary.forPlan(plan.planId);
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
  if (!comp) return handleMissingComponent(cv, surface);

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

/** When the component is not found in the plan, check if all rules are conditionally skipped. */
function handleMissingComponent(
  cv: ComponentValidation,
  surface: ValidationSurface,
): boolean {
  log.trace("component.not-found", { component: cv.component });
  if (allRulesConditionallySkipped(cv.rules, surface)) return true;
  const message = firstRuleMessage(cv);
  if (message !== undefined) {
    surface.summary.add(cv.component, message);
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
    if (!isResolutionError(e)) throw e;
    if (allRulesConditionallySkipped(cv.rules, surface)) return { done: true, result: true };
    const message = firstRuleMessage(cv);
    if (message !== undefined) {
      surface.summary.add(cv.component, message);
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
    const runtimeRule = RuntimeValidationRule.from(rule);
    if (!runtimeRule.isActive(surface)) continue;

    const otherValue = runtimeRule.resolvePeerValue(surface.plan);

    if (ruleFails({ rule, value: field.value, peerValue: otherValue })) {
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
    surface.summary.add(component, rule.message);
  } else {
    showInline(field.componentDomId, rule.message);
    surface.summary.remove(component);
  }
}

/** Only suppress errors from component/element resolution — not contract bugs. */
function isResolutionError(e: unknown): boolean {
  return RuntimeResolutionError.is(e);
}

// -- Helpers --

function allRulesConditionallySkipped(rules: ValidationRule[], surface: ValidationSurface): boolean {
  if (rules.length === 0) return true;
  for (const rule of rules) {
    if (!RuntimeValidationRule.from(rule).isConditionallySkipped(surface)) return false;
  }
  return true;
}

class RuntimeValidationRule {
  private constructor(private readonly rule: ValidationRule) {}

  static from(rule: ValidationRule): RuntimeValidationRule {
    return new RuntimeValidationRule(rule);
  }

  isActive(surface: ValidationSurface): boolean {
    return ValidationActivation.from(this.rule.execution.activation).isActive(surface);
  }

  isConditionallySkipped(surface: ValidationSurface): boolean {
    return ValidationActivation.from(this.rule.execution.activation).isSkipped(surface);
  }

  resolvePeerValue(plan: Plan): ResolvedPeerValue {
    return ValidationPeerOperand.from(this.rule.execution.otherValue).resolve(plan);
  }
}

abstract class ValidationActivation {
  static from(activation: PlanValidationRuleActivation): ValidationActivation {
    switch (activation.kind) {
      case "always": return AlwaysValidationActivation.instance;
      case "when": return new ConditionalValidationActivation(activation.condition);
      default: return assertNever(activation, "validation rule activation");
    }
  }

  abstract isActive(surface: ValidationSurface): boolean;

  abstract isSkipped(surface: ValidationSurface): boolean;
}

class AlwaysValidationActivation extends ValidationActivation {
  static readonly instance = new AlwaysValidationActivation();

  isActive(): boolean {
    return true;
  }

  isSkipped(): boolean {
    return false;
  }
}

class ConditionalValidationActivation extends ValidationActivation {
  constructor(private readonly condition: Condition) {
    super();
  }

  isActive(surface: ValidationSurface): boolean {
    return evaluateCondition(this.condition, surface.plan, surface.context.raw);
  }

  isSkipped(surface: ValidationSurface): boolean {
    return !evaluateCondition(this.condition, surface.plan, surface.context.raw);
  }
}

abstract class ValidationPeerOperand {
  static from(operand: PlanValidationRuleOperand): ValidationPeerOperand {
    switch (operand.kind) {
      case "none": return MissingValidationPeerOperand.instance;
      case "value": return new PresentValidationPeerOperand(operand.value);
      default: return assertNever(operand, "validation peer operand");
    }
  }

  abstract resolve(plan: Plan): ResolvedPeerValue;
}

class MissingValidationPeerOperand extends ValidationPeerOperand {
  static readonly instance = new MissingValidationPeerOperand();

  resolve(): ResolvedPeerValue {
    return noPeerValue();
  }
}

class PresentValidationPeerOperand extends ValidationPeerOperand {
  constructor(private readonly value: ValueProducer) {
    super();
  }

  resolve(plan: Plan): ResolvedPeerValue {
    return peerValue(evaluateValue(this.value, plan));
  }
}

function clearContainerErrors(
  surface: ValidationSurface,
): void {
  for (const cv of surface.containerScope.validationRules) {
    const comp = surface.runtime.components.find(cv.component);
    if (comp) clearInline(comp.id);
  }
  surface.summary.clearAndHide();
}

function findComponentValidationByName(surface: ValidationSurface, name: string): ComponentValidation | undefined {
  // Plan-driven: each ComponentValidation carries serverFieldName set at C# build time.
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

class ServerValidationErrors {
  private constructor(
    readonly fields: ServerValidationError[],
    private readonly payloadShape: ServerValidationPayloadShape,
  ) {}

  static from(data: unknown): ServerValidationErrors {
    const payload = ObjectRecord.tryFrom(data);
    if (payload === undefined) return ServerValidationErrors.notPresent();

    const errors = ObjectRecord.tryFrom(payload.get("errors"));
    if (errors === undefined) return ServerValidationErrors.wrongShape();

    return ServerValidationErrors.withFields(
      errors.entries().map(([name, messages]) => ({ name, messages })),
    );
  }

  private static notPresent(): ServerValidationErrors {
    return new ServerValidationErrors([], "not-validation-payload");
  }

  private static wrongShape(): ServerValidationErrors {
    return new ServerValidationErrors([], "wrong-shape");
  }

  private static withFields(fields: ServerValidationError[]): ServerValidationErrors {
    return new ServerValidationErrors(fields, "field-errors");
  }

  get wrongShape(): boolean {
    return this.payloadShape === "wrong-shape";
  }

  get found(): boolean {
    return this.fields.length > 0;
  }
}

type ServerValidationPayloadShape =
  | "not-validation-payload"
  | "wrong-shape"
  | "field-errors";

class ServerValidationErrorMessage {
  static from(messages: unknown): string {
    if (Array.isArray(messages)) return messages.join(", ");

    const message = toString(messages);
    if (message.ok) return message.value;

    return "";
  }
}
