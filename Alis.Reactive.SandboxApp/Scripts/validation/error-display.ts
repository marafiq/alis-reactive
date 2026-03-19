// Error Display — Single responsibility: DOM error manipulation
//
// Two destinations: inline (next to field) and summary (aggregated div).
// Orchestrator decides WHERE to route — this module only executes display.
// Vendor-agnostic: never touches ej2_instances or vendor-specific APIs.
//
// Error spans are found by predictable ID: {fieldId}_error (O(1) lookup).
// Fallback to querySelector for server errors where only fieldName is available.

import type { ValidationField } from "../types";

const ERR_CLASS = "alis-has-error";

// ── Inline errors (next to enriched, visible fields) ────

export function showInline(formId: string, field: ValidationField, message: string): void {
  if (field.fieldId) {
    const el = document.getElementById(field.fieldId);
    if (el) el.classList.add(ERR_CLASS);
  }

  const span = findErrorSpanForField(field) ?? findErrorSpanByName(formId, field.fieldName);
  if (span) {
    span.textContent = message;
    span.removeAttribute("hidden");
    span.style.display = "";
  }
}

export function clearInline(formId: string, field: ValidationField): void {
  const span = findErrorSpanForField(field) ?? findErrorSpanByName(formId, field.fieldName);
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

export function removeSummaryEntry(summaryEl: HTMLElement, fieldName: string): void {
  const entry = summaryEl.querySelector(`[data-valmsg-summary-for="${fieldName}"]`);
  if (entry) entry.remove();
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
  if (!planId) return null;
  return document.querySelector(`[data-alis-validation-summary="${planId}"]`);
}

// ── Server error inline display ─────────────────────────

export function showServerErrorInline(formId: string, fieldName: string, message: string, fields: ValidationField[]): void {
  const field = fields.find(f => f.fieldName === fieldName);
  const span = (field ? findErrorSpanForField(field) : null) ?? findErrorSpanByName(formId, fieldName);
  if (span) {
    span.textContent = message;
    span.removeAttribute("hidden");
    span.style.display = "";
  }

  if (field?.fieldId) {
    const el = document.getElementById(field.fieldId);
    if (el) el.classList.add(ERR_CLASS);
  }
}

// ── Error span lookup ───────────────────────────────────

/** ID-based lookup: O(1). Convention: {fieldId}_error. */
function findErrorSpanForField(field: ValidationField): HTMLElement | null {
  if (!field.fieldId) return null;
  return document.getElementById(field.fieldId + "_error");
}

/** Fallback: querySelector scan within form container. Used when only fieldName is available. */
function findErrorSpanByName(containerId: string, fieldName: string): HTMLElement | null {
  const container = document.getElementById(containerId);
  if (!container) return null;
  return container.querySelector(`span[data-valmsg-for="${fieldName}"]`);
}
