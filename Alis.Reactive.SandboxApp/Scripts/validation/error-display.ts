// Error Display — Single responsibility: DOM error manipulation
// V3: uses component IDs directly (not ValidationField objects).
// Error spans found by predictable ID: {componentDomId}_error (O(1) lookup).
// Summary found by predictable ID: {planId_sanitized}_validation_summary.
// No fallbacks. No querySelector scanning. ID-aware only.

import type { Plan, ContainerScope } from "../types";

const ERR_CLASS = "alis-has-error";

// -- Inline errors (next to visible fields) --

export function showInline(containerId: string, componentDomId: string, message: string): void {
  const el = document.getElementById(componentDomId);
  if (el) el.classList.add(ERR_CLASS);

  const span = findErrorSpan(componentDomId);
  if (span) {
    span.textContent = message;
    span.removeAttribute("hidden");
    span.style.display = "";
  }
}

export function clearInline(containerId: string, componentDomId: string): void {
  const span = findErrorSpan(componentDomId);
  if (span) {
    span.textContent = "";
    span.setAttribute("hidden", "");
    span.style.display = "none";
  }
  const el = document.getElementById(componentDomId);
  if (el) el.classList.remove(ERR_CLASS);
}

export function clearAllInline(containerId: string, componentDomIds: string[]): void {
  for (const id of componentDomIds) clearInline(containerId, id);
}

// -- Summary errors --

export function addToSummary(summaryEl: HTMLElement, name: string, message: string): void {
  const item = document.createElement("div");
  item.dataset.valmsgSummaryFor = name;
  item.textContent = message;
  summaryEl.appendChild(item);
}

export function removeSummaryEntry(summaryEl: HTMLElement, name: string): void {
  const entry = summaryEl.querySelector(`[data-valmsg-summary-for="${name}"]`);
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
  const summaryId = planId.replace(/[.+]/g, "_") + "_validation_summary";
  return document.getElementById(summaryId);
}

// -- Server error inline display --

export function showServerErrorInline(
  containerId: string,
  componentKey: string,
  message: string,
  plan: Plan,
  containerScope: ContainerScope,
): void {
  const comp = plan.components[componentKey];
  if (!comp) return;

  const span = findErrorSpan(comp.id);
  if (span) {
    span.textContent = message;
    span.removeAttribute("hidden");
    span.style.display = "";
  }

  const el = document.getElementById(comp.id);
  if (el) el.classList.add(ERR_CLASS);
}

// -- Error span lookup -- ID only, no scanning --

function findErrorSpan(componentDomId: string): HTMLElement | null {
  return document.getElementById(componentDomId + "_error");
}
