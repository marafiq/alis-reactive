// Forms — Client-side validation engine + server error display
//
// Fully self-contained: no SF FormValidator dependency.
// 11 rule types, conditional rules, live error clearing.

import type {
  ValidationDescriptor,
  ValidationField,
  ValidationRule,
  ValidationCondition,
} from "./types";
import { scope } from "./trace";

const log = scope("forms");
const ERR_CLASS = "alis-has-error";

interface FormState {
  fields: ValidationField[];
  byName: Map<string, ValidationField>;
}

const forms = new Map<string, FormState>();
const wired = new Set<string>();

// ── Register ────────────────────────────────────────────────

export function register(desc: ValidationDescriptor): void {
  const byName = new Map<string, ValidationField>();
  for (const f of desc.fields) {
    byName.set(f.fieldName, f);
  }
  forms.set(desc.formId, { fields: desc.fields, byName });
  wireLiveClearing(desc.formId);
  log.debug("register", { formId: desc.formId, fields: desc.fields.length });
}

function wireLiveClearing(formId: string): void {
  const fs = forms.get(formId);
  if (!fs) return;
  for (const f of fs.fields) {
    const key = `${formId}:${f.fieldId}`;
    if (wired.has(key)) continue;
    wired.add(key);
    const el = document.getElementById(f.fieldId);
    if (el) {
      el.addEventListener("input", () => clearField(f));
      el.addEventListener("change", () => clearField(f));
    }
  }
}

// ── Validate ────────────────────────────────────────────────

export function validate(formId: string): boolean {
  const fs = forms.get(formId);
  if (!fs) return true;
  clearErrors(formId);
  let valid = true;

  for (const f of fs.fields) {
    const el = document.getElementById(f.fieldId);
    if (!el) continue;
    // Skip hidden fields — walk up DOM checking for display:none
    if (isHidden(el)) continue;

    const value = readValue(f);
    for (const rule of f.rules) {
      if (rule.when && !evalCondition(rule.when, fs)) continue;
      if (ruleFails(rule, value, fs)) {
        showError(f, rule.message);
        valid = false;
        break; // first failing rule wins
      }
    }
  }

  log.debug("validate", { formId, valid });
  return valid;
}

// ── Value reading ───────────────────────────────────────────

function readValue(field: ValidationField): unknown {
  const el = document.getElementById(field.fieldId);
  if (!el) return null;
  if (field.vendor === "fusion" && field.readExpr) {
    try {
      return new Function("el", `return ${field.readExpr}`)(el);
    } catch {
      return null;
    }
  }
  // Native: checkbox → checked, everything else → .value
  if ((el as HTMLInputElement).type === "checkbox") {
    return (el as HTMLInputElement).checked;
  }
  return (el as HTMLInputElement).value;
}

// ── Condition evaluation ────────────────────────────────────

function evalCondition(cond: ValidationCondition, fs: FormState): boolean {
  const srcField = fs.byName.get(cond.field);
  let val: unknown;
  if (srcField) {
    val = readValue(srcField);
  } else {
    // Fall back to reading by field name as element ID
    const el = document.getElementById(cond.field) || document.getElementById(cond.field.replace(/\./g, "_"));
    if (el && (el as HTMLInputElement).type === "checkbox") {
      val = (el as HTMLInputElement).checked;
    } else {
      val = el ? (el as HTMLInputElement).value : null;
    }
  }
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

// ── Rule evaluation (11 types) ──────────────────────────────

function ruleFails(rule: ValidationRule, value: unknown, fs: FormState): boolean {
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
      try {
        return !empty && !new RegExp(String(rule.constraint)).test(str);
      } catch {
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
      const other = fs.byName.get(String(rule.constraint));
      if (!other) return false;
      return String(value ?? "") !== String(readValue(other) ?? "");
    }
    case "atLeastOne":
      return Array.isArray(value) ? value.length === 0 : empty;
    default:
      return false;
  }
}

// ── Server errors (400 ProblemDetails) ──────────────────────

export function showFieldErrors(formId: string, errorData: unknown): void {
  clearErrors(formId);
  const fs = forms.get(formId);

  let errors: Record<string, unknown> | null = null;
  if (errorData && typeof errorData === "object") {
    if ("errors" in errorData && typeof (errorData as Record<string, unknown>).errors === "object") {
      errors = (errorData as Record<string, Record<string, unknown>>).errors;
    } else {
      errors = errorData as Record<string, unknown>;
    }
  }
  if (!errors) return;

  for (const [name, msgs] of Object.entries(errors)) {
    const msg = Array.isArray(msgs) ? msgs.join(", ") : String(msgs);
    const f = fs?.byName.get(name);
    if (f) {
      showError(f, msg);
    } else {
      showServerFieldError(formId, name, msg);
    }
  }

  log.debug("showFieldErrors", { formId, fieldCount: Object.keys(errors).length });
}

function showServerFieldError(formId: string, name: string, msg: string): void {
  let el: HTMLElement | null = null;

  // First: search within the form element by name attribute
  // (handles partial views where field IDs have a prefix like partial_Address_Street)
  const form = document.getElementById(formId);
  if (form) {
    el = form.querySelector(`[name="${name}"]`) as HTMLElement | null;
  }

  // Fallback: try MVC ID convention (dots → underscores) or raw name
  if (!el) {
    const nativeId = name.replace(/\./g, "_");
    el = document.getElementById(nativeId) || document.getElementById(name);
  }

  if (el) {
    el.classList.add(ERR_CLASS);
    const span = document.getElementById(`err_${el.id}`);
    if (span) {
      span.textContent = msg;
      span.removeAttribute("hidden");
      span.style.display = "";
    }
  }
}

// ── Error display ───────────────────────────────────────────

function showError(field: ValidationField, message: string): void {
  const el = document.getElementById(field.fieldId);
  if (!el) return;
  el.classList.add(ERR_CLASS);
  const span = document.getElementById(field.errorId);
  if (span) {
    span.textContent = message;
    span.removeAttribute("hidden");
    span.style.display = "";
  }
}

export function clearErrors(formId: string): void {
  const fs = forms.get(formId);
  if (!fs) return;
  for (const f of fs.fields) {
    clearField(f);
  }
}

function clearField(field: ValidationField): void {
  const span = document.getElementById(field.errorId);
  if (span) {
    span.textContent = "";
    span.setAttribute("hidden", "");
    span.style.display = "none";
  }
  const el = document.getElementById(field.fieldId);
  if (el) el.classList.remove(ERR_CLASS);
}

// ── Helpers ─────────────────────────────────────────────────

function isHidden(el: HTMLElement): boolean {
  let node: HTMLElement | null = el;
  while (node) {
    if (node.style && node.style.display === "none") return true;
    node = node.parentElement;
  }
  return false;
}

/** Reset all internal state. Used by tests only. */
export function _reset(): void {
  forms.clear();
  wired.clear();
}
