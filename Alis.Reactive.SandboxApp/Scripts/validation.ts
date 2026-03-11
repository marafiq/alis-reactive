// Validation — Stateless client-side validation engine + server error display
//
// No registration, no global state. Each function receives the ValidationDescriptor.
// Error spans found by: querySelector('span[data-valmsg-for="${fieldName}"]')
// scoped to the container element. 11 rule types, conditional rules, first-failing-rule-wins.
//
// Validation rules (11 types + ValidationCondition with truthy/falsy/eq/neq)
// are completely separate from the Conditions/Guards pipeline module.

import type {
  ValidationDescriptor,
  ValidationField,
  ValidationRule,
  ValidationCondition,
} from "./types";
import { resolveRoot } from "./component";
import { scope } from "./trace";
import { walk } from "./walk";

const log = scope("validation");
const ERR_CLASS = "alis-has-error";

// ── Public API ──────────────────────────────────────────────

export function validate(desc: ValidationDescriptor): boolean {
  clearAll(desc);
  const container = document.getElementById(desc.formId);
  if (!container) return true;
  const byName = buildByName(desc);
  let valid = true;

  for (const f of desc.fields) {
    const el = document.getElementById(f.fieldId);
    if (!el) continue;

    // Only validate elements that belong to this form
    if (!container.contains(el)) continue;

    if (isHidden(el)) continue;

    const root = resolveRoot(el, f.vendor);
    const value = walk(root, f.readExpr);
    for (const rule of f.rules) {
      if (rule.when && !evalCondition(rule.when, byName)) continue;
      if (ruleFails(rule, value, byName)) {
        showError(desc.formId, f, rule.message);
        valid = false;
        break; // first failing rule wins
      }
    }
  }

  log.debug("validate", { formId: desc.formId, valid });
  return valid;
}

export function showServerErrors(desc: ValidationDescriptor, data: unknown): void {
  clearAll(desc);
  const errors = extractErrors(data);
  if (!errors) return;

  for (const [name, msgs] of Object.entries(errors)) {
    const msg = Array.isArray(msgs) ? msgs.join(", ") : String(msgs);

    const span = findErrorSpan(desc.formId, name);
    if (span) {
      span.textContent = msg;
      span.removeAttribute("hidden");
      span.style.display = "";
    }

    const field = desc.fields.find(f => f.fieldName === name);
    if (field) {
      const el = document.getElementById(field.fieldId);
      if (el) el.classList.add(ERR_CLASS);
    }
  }

  log.debug("showServerErrors", { formId: desc.formId, fieldCount: Object.keys(errors).length });
}

export function clearAll(desc: ValidationDescriptor): void {
  for (const f of desc.fields) {
    clearFieldError(desc.formId, f);
  }
}

export function wireLiveClearing(desc: ValidationDescriptor): void {
  const container = document.getElementById(desc.formId);
  if (!container || container.dataset.alisValidated) return;
  container.dataset.alisValidated = "true";

  const handler = (e: Event) => {
    const target = e.target as HTMLElement;
    const field = desc.fields.find(f => f.fieldId === target.id);
    if (field) clearFieldError(desc.formId, field);
  };

  container.addEventListener("input", handler);
  container.addEventListener("change", handler);
}

// ── Internal ────────────────────────────────────────────────

function buildByName(desc: ValidationDescriptor): Map<string, ValidationField> {
  const map = new Map<string, ValidationField>();
  for (const f of desc.fields) map.set(f.fieldName, f);
  return map;
}

function findErrorSpan(containerId: string, fieldName: string): HTMLElement | null {
  const container = document.getElementById(containerId);
  if (!container) return null;
  return container.querySelector(`span[data-valmsg-for="${fieldName}"]`);
}

function showError(containerId: string, field: ValidationField, message: string): void {
  const el = document.getElementById(field.fieldId);
  if (el) el.classList.add(ERR_CLASS);

  const span = findErrorSpan(containerId, field.fieldName);
  if (span) {
    span.textContent = message;
    span.removeAttribute("hidden");
    span.style.display = "";
  }
}

function clearFieldError(containerId: string, field: ValidationField): void {
  const span = findErrorSpan(containerId, field.fieldName);
  if (span) {
    span.textContent = "";
    span.setAttribute("hidden", "");
    span.style.display = "none";
  }
  const el = document.getElementById(field.fieldId);
  if (el) el.classList.remove(ERR_CLASS);
}

function evalCondition(cond: ValidationCondition, byName: Map<string, ValidationField>): boolean {
  const srcField = byName.get(cond.field);
  if (!srcField) return true;
  const el = document.getElementById(srcField.fieldId);
  if (!el) return true;
  const root = resolveRoot(el, srcField.vendor);
  const val = walk(root, srcField.readExpr);
  const str = val == null ? "" : String(val);
  const empty = val == null || str === "" || val === false;
  switch (cond.op) {
    case "truthy": return !empty;
    case "falsy": return empty;
    case "eq": return str === String(cond.value ?? "");
    case "neq": return str !== String(cond.value ?? "");
    default: return true;
  }
}

function ruleFails(rule: ValidationRule, value: unknown, byName: Map<string, ValidationField>): boolean {
  const str = value == null ? "" : String(value);
  const empty = value == null || str === "" || value === false;

  switch (rule.rule) {
    case "required":
      return empty;
    case "minLength":
      return !empty && str.length < Number(rule.constraint);
    case "maxLength":
      return !empty && str.length > Number(rule.constraint);
    case "email":
      return !empty && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(str);
    case "regex": {
      try { return !empty && !new RegExp(String(rule.constraint)).test(str); }
      catch {
        log.warn("invalid validation regex", { constraint: rule.constraint });
        return false;
      }
    }
    case "url":
      return !empty && !/^https?:\/\/.+/.test(str);
    case "min":
      return !empty && Number(str) < Number(rule.constraint);
    case "max":
      return !empty && Number(str) > Number(rule.constraint);
    case "range": {
      const [lo, hi] = rule.constraint as [number, number];
      const n = Number(str);
      return !empty && (n < lo || n > hi);
    }
    case "equalTo": {
      const other = byName.get(String(rule.constraint));
      if (!other) return false;
      const otherEl = document.getElementById(other.fieldId);
      if (!otherEl) return false;
      const otherRoot = resolveRoot(otherEl, other.vendor);
      return String(value ?? "") !== String(walk(otherRoot, other.readExpr) ?? "");
    }
    case "atLeastOne":
      return Array.isArray(value) ? value.length === 0 : empty;
    default:
      return false;
  }
}

/** Checks if element or any ancestor is hidden (reactive Hide or display:none) */
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
  if ("errors" in data && typeof (data as Record<string, unknown>).errors === "object") {
    return (data as Record<string, Record<string, unknown>>).errors;
  }
  return data as Record<string, unknown>;
}
