import type {
  Plan,
  RequestValidation,
  RequestValidationField,
  RequestValidationRule,
  ValueShape,
} from "../types";
import { evaluatePredicate } from "../conditions/conditions";
import { applyShape } from "../resolution/values";
import { getBindingValue, tryGetElementIdForBinding } from "../resolution/contracts";
import { scope } from "../core/trace";
import { toString } from "../core/coerce";
import {
  addToSummary,
  clearAllInline,
  clearInline,
  clearSummary,
  findSummaryElement,
  hideSummaryDiv,
  removeSummaryEntry,
  showInline,
  showServerErrorInline,
  showSummaryDiv,
  type ValidationFieldView,
} from "./error-display";

const log = scope("validation");

interface ResolvedValidationField extends ValidationFieldView {
  rules: RequestValidationRule[];
  shape?: ValueShape;
}

export function validate(plan: Plan, desc: RequestValidation): boolean {
  const fields = resolveFields(plan, desc);
  clearAllInline(desc.formId, fields);

  const summaryEl = findSummaryElement(plan.planId);
  if (summaryEl) {
    clearSummary(summaryEl);
    hideSummaryDiv(summaryEl);
  }

  const container = document.getElementById(desc.formId);
  if (!container) {
    return fields.length === 0;
  }

  let valid = true;
  let summaryHasErrors = false;

  for (const field of fields) {
    if (!evaluateField(plan, desc, field, container, summaryEl)) {
      valid = false;
      summaryHasErrors = summaryHasErrors || hasSummaryEntry(summaryEl, field.binding);
    }
  }

  if (summaryHasErrors && summaryEl) showSummaryDiv(summaryEl);
  log.debug("validate", { formId: desc.formId, valid });
  return valid;
}

export function revalidateField(plan: Plan, desc: RequestValidation, binding: string): void {
  const field = resolveField(plan, desc.fields.find(item => item.binding === binding));
  if (!field) return;

  clearInline(desc.formId, field);

  const container = document.getElementById(desc.formId);
  if (!container || !field.elementId) return;

  const el = document.getElementById(field.elementId);
  if (!el || !container.contains(el)) return;

  evaluateField(plan, desc, field, container, findSummaryElement(plan.planId));
}

export function showServerErrors(plan: Plan, desc: RequestValidation, data: unknown): void {
  const fields = resolveFields(plan, desc);
  clearAllInline(desc.formId, fields);

  const summaryEl = findSummaryElement(plan.planId);
  if (summaryEl) {
    clearSummary(summaryEl);
    hideSummaryDiv(summaryEl);
  }

  const errors = extractErrors(data);
  if (!errors) return;

  let summaryHasErrors = false;

  for (const [binding, messages] of Object.entries(errors)) {
    const message = Array.isArray(messages) ? messages.join(", ") : text(messages);
    if (findErrorSpanExists(binding, fields)) {
      showServerErrorInline(desc.formId, binding, message, fields);
    } else if (summaryEl) {
      addToSummary(summaryEl, binding, message);
      summaryHasErrors = true;
    }
  }

  if (summaryHasErrors && summaryEl) showSummaryDiv(summaryEl);
}

export function clearAll(plan: Plan, desc: RequestValidation): void {
  const fields = resolveFields(plan, desc);
  clearAllInline(desc.formId, fields);
  const summaryEl = findSummaryElement(plan.planId);
  if (summaryEl) {
    clearSummary(summaryEl);
    hideSummaryDiv(summaryEl);
  }
}

function evaluateField(
  plan: Plan,
  desc: RequestValidation,
  field: ResolvedValidationField,
  container: HTMLElement,
  summaryEl: HTMLElement | null
): boolean {
  if (!field.elementId) {
    return handleUnresolvableField(plan, field, summaryEl, true);
  }

  const el = document.getElementById(field.elementId);
  if (!el) {
    return handleUnresolvableField(plan, field, summaryEl, true);
  }

  if (!container.contains(el)) return true;

  let value: unknown;
  try {
    value = getBindingValue(plan, field.binding, { plan });
  } catch {
    return handleUnresolvableField(plan, field, summaryEl, false);
  }

  const errorSpan = document.getElementById(field.elementId + "_error");
  const hidden = errorSpan?.parentElement ? isHidden(errorSpan.parentElement) : true;

  for (const rule of field.rules) {
    const condition = evaluateRuleCondition(plan, rule);
    if (condition === "skip") continue;
    if (condition === "block") {
      if (summaryEl) addToSummary(summaryEl, field.binding, rule.message);
      return false;
    }

    if (ruleFails(plan, field, rule, value)) {
      if (hidden) {
        if (summaryEl) addToSummary(summaryEl, field.binding, rule.message);
      } else {
        showInline(desc.formId, field, rule.message);
        if (summaryEl) removeSummaryEntry(summaryEl, field.binding);
      }
      return false;
    }
  }

  return true;
}

function handleUnresolvableField(
  plan: Plan,
  field: ResolvedValidationField,
  summaryEl: HTMLElement | null,
  treatUnknownConditionAsSkip: boolean
): boolean {
  if (allRulesConditionallySkipped(plan, field, treatUnknownConditionAsSkip)) {
    return true;
  }

  if (field.rules.length > 0 && summaryEl) {
    addToSummary(summaryEl, field.binding, field.rules[0].message);
  }

  return false;
}

function allRulesConditionallySkipped(
  plan: Plan,
  field: ResolvedValidationField,
  treatUnknownConditionAsSkip: boolean
): boolean {
  if (field.rules.length === 0) return true;

  for (const rule of field.rules) {
    if (!rule.when) return false;
    const result = tryEvaluatePredicate(plan, rule.when);
    if (result === true) return false;
    if (result === undefined && !treatUnknownConditionAsSkip) return false;
  }

  return true;
}

function evaluateRuleCondition(plan: Plan, rule: RequestValidationRule): "skip" | "block" | "eval" {
  if (!rule.when) return "eval";
  const result = tryEvaluatePredicate(plan, rule.when);
  if (result === false) return "skip";
  if (result === undefined) return "block";
  return "eval";
}

function tryEvaluatePredicate(plan: Plan, predicate: NonNullable<RequestValidationRule["when"]>): boolean | undefined {
  try {
    return evaluatePredicate(predicate, { plan });
  } catch (error) {
    log.trace("predicate unresolved", { error: String(error) });
    return undefined;
  }
}

function ruleFails(
  plan: Plan,
  field: ResolvedValidationField,
  rule: RequestValidationRule,
  value: unknown
): boolean {
  const shape = rule.as ?? field.shape;
  const coerced = shape ? applyShape(value, shape) : value;
  const empty = isValidationEmpty(value, coerced, shape != null);
  const str = text(coerced);

  switch (rule.rule) {
    case "required":
      return empty;
    case "empty":
      return !empty;
    case "minLength":
      return !empty && str.length < Number(rule.constraint);
    case "maxLength":
      return !empty && str.length > Number(rule.constraint);
    case "email":
      return !empty && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(str);
    case "regex":
      try {
        return !empty && !new RegExp(text(rule.constraint)).test(str);
      } catch {
        return true;
      }
    case "url":
      return !empty && !/^https?:\/\/.+/.test(str);
    case "creditCard":
      return !empty && !luhn(str.replace(/\D/g, ""));
    case "min":
    case "max":
    case "gt":
    case "lt":
      return compareRule(plan, rule, coerced, empty, shape);
    case "range":
    case "exclusiveRange":
      return rangeRule(rule, coerced, empty, shape);
    case "equalTo":
    case "notEqual":
    case "notEqualTo":
      return equalityRule(plan, rule, coerced, empty, shape);
    case "atLeastOne":
      return Array.isArray(coerced) ? coerced.length === 0 : empty;
    default:
      return true;
  }
}

function compareRule(
  plan: Plan,
  rule: RequestValidationRule,
  value: unknown,
  empty: boolean,
  shape?: ValueShape
): boolean {
  const target = resolveOtherValue(plan, rule, shape);
  if (target === undefined) return true;
  const cmp = compareValues(value, target, shape);

  switch (rule.rule) {
    case "min":
      return !empty && (Number.isNaN(cmp) || cmp < 0);
    case "max":
      return !empty && (Number.isNaN(cmp) || cmp > 0);
    case "gt":
      return empty || Number.isNaN(cmp) || cmp <= 0;
    case "lt":
      return !empty && (Number.isNaN(cmp) || cmp >= 0);
    default:
      return true;
  }
}

function rangeRule(rule: RequestValidationRule, value: unknown, empty: boolean, shape?: ValueShape): boolean {
  if (empty) return false;
  const [lo, hi] = Array.isArray(rule.constraint) ? rule.constraint : [undefined, undefined];
  const cmpLo = compareValues(value, shape ? applyShape(lo, shape) : lo, shape);
  const cmpHi = compareValues(value, shape ? applyShape(hi, shape) : hi, shape);
  if (Number.isNaN(cmpLo) || Number.isNaN(cmpHi)) return true;
  return rule.rule === "range"
    ? cmpLo < 0 || cmpHi > 0
    : cmpLo <= 0 || cmpHi >= 0;
}

function equalityRule(
  plan: Plan,
  rule: RequestValidationRule,
  value: unknown,
  empty: boolean,
  shape?: ValueShape
): boolean {
  switch (rule.rule) {
    case "equalTo": {
      if (empty) return false;
      const target = resolveOtherValue(plan, rule, shape);
      if (target === undefined) return true;
      return !valuesEqual(value, target, shape);
    }
    case "notEqual":
      return !empty && valuesEqual(value, rule.constraint, shape);
    case "notEqualTo": {
      const target = resolveOtherValue(plan, rule, shape);
      if (target === undefined) return true;
      return !empty && valuesEqual(value, target, shape);
    }
    default:
      return true;
  }
}

function resolveOtherValue(plan: Plan, rule: RequestValidationRule, shape?: ValueShape): unknown {
  if (rule.otherBinding) {
    try {
      const value = getBindingValue(plan, rule.otherBinding, { plan });
      return shape ? applyShape(value, shape) : value;
    } catch {
      return undefined;
    }
  }

  return shape ? applyShape(rule.constraint, shape) : rule.constraint;
}

function compareValues(left: unknown, right: unknown, shape?: ValueShape): number {
  if (shape?.kind === "scalar") {
    switch (shape.type) {
      case "number":
      case "date":
        return compareNumericValues(left, right);
      case "boolean":
        return compareNumericValues(left === true, right === true);
      default:
        return Number.NaN;
    }
  }

  if (shape?.kind) {
    return Number.NaN;
  }

  return compareNumericValues(left, right);
}

function compareNumericValues(left: unknown, right: unknown): number {
  const a = Number(left);
  const b = Number(right);
  if (Number.isNaN(a) || Number.isNaN(b)) return Number.NaN;
  return a - b;
}

function valuesEqual(left: unknown, right: unknown, shape?: ValueShape): boolean {
  if (shape?.kind === "array") {
    if (!Array.isArray(left) || !Array.isArray(right)) return false;
    if (left.length !== right.length) return false;
    return left.every((item, index) => valuesEqual(item, right[index], shape.item));
  }

  if (shape?.kind === "object") {
    return objectValuesEqual(left, right, shape);
  }

  if (shape?.kind === "any") {
    return looselyEqual(left, right);
  }

  if (shape?.kind === "scalar") {
    switch (shape.type) {
      case "number":
      case "date":
        return compareValues(left, right, shape) === 0;
      case "boolean":
        return left === right;
      case "string":
      case "raw":
        return text(left) === text(right);
    }
  }

  return looselyEqual(left, right);
}

function objectValuesEqual(left: unknown, right: unknown, shape: Extract<ValueShape, { kind: "object" }>): boolean {
  if (!isPlainObject(left) || !isPlainObject(right)) return false;

  const fields = shape.fields;
  if (fields) {
    for (const [name, fieldShape] of Object.entries(fields)) {
      if (!valuesEqual(left[name], right[name], fieldShape)) return false;
    }

    if (!shape.additional) return true;
  }

  const leftKeys = Object.keys(left);
  const rightKeys = Object.keys(right);
  if (leftKeys.length !== rightKeys.length) return false;

  for (const key of leftKeys) {
    if (!(key in right)) return false;
    if (fields && key in fields) continue;
    if (!looselyEqual(left[key], right[key])) return false;
  }

  return true;
}

function looselyEqual(left: unknown, right: unknown): boolean {
  if (Array.isArray(left) || Array.isArray(right)) {
    if (!Array.isArray(left) || !Array.isArray(right)) return false;
    if (left.length !== right.length) return false;
    return left.every((item, index) => looselyEqual(item, right[index]));
  }

  if (isPlainObject(left) || isPlainObject(right)) {
    if (!isPlainObject(left) || !isPlainObject(right)) return false;
    const leftKeys = Object.keys(left);
    const rightKeys = Object.keys(right);
    if (leftKeys.length !== rightKeys.length) return false;
    return leftKeys.every(key => key in right && looselyEqual(left[key], right[key]));
  }

  return text(left) === text(right);
}

function isPlainObject(value: unknown): value is Record<string, unknown> {
  return value != null && typeof value === "object" && !Array.isArray(value);
}

function isValidationEmpty(rawValue: unknown, shapedValue: unknown, usedShape: boolean): boolean {
  if (rawValue == null || rawValue === "" || rawValue === false) return true;
  if (Array.isArray(rawValue) && rawValue.length === 0) return true;

  if (usedShape && shapedValue === undefined) return true;
  if (shapedValue == null || shapedValue === "" || shapedValue === false) return true;
  if (Array.isArray(shapedValue) && shapedValue.length === 0) return true;

  return false;
}

function resolveFields(plan: Plan, desc: RequestValidation): ResolvedValidationField[] {
  return desc.fields.map(field => resolveField(plan, field)).filter((field): field is ResolvedValidationField => field != null);
}

function resolveField(plan: Plan, field: RequestValidationField | undefined): ResolvedValidationField | undefined {
  if (!field) return undefined;

  const binding = plan.bindings[field.binding];
  return {
    binding: field.binding,
    rules: field.rules,
    elementId: tryGetElementIdForBinding(plan, field.binding),
    shape: binding?.shape,
  };
}

function findErrorSpanExists(binding: string, fields: ResolvedValidationField[]): boolean {
  const field = fields.find(item => item.binding === binding);
  return !!field?.elementId && document.getElementById(field.elementId + "_error") !== null;
}

function hasSummaryEntry(summaryEl: HTMLElement | null, binding: string): boolean {
  if (!summaryEl) return false;
  return summaryEl.querySelector(`[data-valmsg-summary-for="${binding}"]`) !== null;
}

function isHidden(el: HTMLElement): boolean {
  let node: HTMLElement | null = el;
  while (node) {
    if (node.hasAttribute("hidden") || node.style.display === "none") return true;
    node = node.parentElement;
  }
  return false;
}

function text(value: unknown): string {
  const result = toString(value);
  return result.ok ? result.value : "";
}

function extractErrors(data: unknown): Record<string, unknown> | null {
  if (!data || typeof data !== "object") return null;
  const obj = data as Record<string, unknown>;
  if ("errors" in obj && typeof obj.errors === "object" && obj.errors !== null) {
    return obj.errors as Record<string, unknown>;
  }
  return null;
}

function luhn(digits: string): boolean {
  if (digits.length < 13) return false;
  let sum = 0;
  let alt = false;
  for (let i = digits.length - 1; i >= 0; i--) {
    let n = parseInt(digits[i], 10);
    if (alt) {
      n *= 2;
      if (n > 9) n -= 9;
    }
    sum += n;
    alt = !alt;
  }
  return sum % 10 === 0;
}
