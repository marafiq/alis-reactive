// Error Display — Single responsibility: DOM error manipulation
//
// Two destinations: inline (next to field) and summary (aggregated div).
// Orchestrator decides WHERE to route — this module only executes display.
// Vendor-agnostic: never touches ej2_instances or vendor-specific APIs.

import type { ValidationField } from "../types";

const ERR_CLASS = "alis-has-error";

// ── Inline errors (next to enriched, visible fields) ────

export function showInline(formId: string, field: ValidationField, message: string): void {
  if (field.fieldId) {
    const el = document.getElementById(field.fieldId);
    if (el) el.classList.add(ERR_CLASS);
  }

  const span = findErrorSpan(formId, field.fieldName);
  if (span) {
    span.textContent = message;
    span.removeAttribute("hidden");
    span.style.display = "";
  }
}

export function clearInline(formId: string, field: ValidationField): void {
  const span = findErrorSpan(formId, field.fieldName);
  if (span) {
    span.textContent = "";
    span.setAttribute("hidden", "");
    span.style.display = "none";
  }
  if (field.fieldId) {
    const el = document.getElementById(field.fieldId);
    if (el) el.classList.remove(ERR_CLASS);
  }
}

export function clearAllInline(formId: string, fields: ValidationField[]): void {
  for (const f of fields) clearInline(formId, f);
}

// ── Summary errors (for unenriched / hidden fields) ─────

export function addToSummary(summaryEl: HTMLElement, fieldName: string, message: string): void {
  const item = document.createElement("div");
  item.setAttribute("data-valmsg-summary-for", fieldName);
  item.textContent = message;
  summaryEl.appendChild(item);
}

export function clearSummary(summaryEl: HTMLElement): void {
  summaryEl.innerHTML = "";
}

export function showSummaryDiv(summaryEl: HTMLElement): void {
  summaryEl.removeAttribute("hidden");
}

export function hideSummaryDiv(summaryEl: HTMLElement): void {
  summaryEl.setAttribute("hidden", "");
}

export function findSummaryElement(planId?: string): HTMLElement | null {
  if (planId) {
    return document.querySelector(`[data-alis-validation-summary="${planId}"]`);
  }
  return document.querySelector("[data-alis-validation-summary]");
}

// ── Server error inline display ─────────────────────────

export function showServerErrorInline(formId: string, fieldName: string, message: string, fields: ValidationField[]): void {
  const span = findErrorSpan(formId, fieldName);
  if (span) {
    span.textContent = message;
    span.removeAttribute("hidden");
    span.style.display = "";
  }

  const field = fields.find(f => f.fieldName === fieldName);
  if (field?.fieldId) {
    const el = document.getElementById(field.fieldId);
    if (el) el.classList.add(ERR_CLASS);
  }
}

// ── Shared ──────────────────────────────────────────────

function findErrorSpan(containerId: string, fieldName: string): HTMLElement | null {
  const container = document.getElementById(containerId);
  if (!container) return null;
  return container.querySelector(`span[data-valmsg-for="${fieldName}"]`);
}
