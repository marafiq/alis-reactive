// Error Display — Single responsibility: DOM error manipulation
// V3: uses component IDs directly (not ValidationField objects).
// Error spans found by predictable ID: {componentDomId}_error (O(1) lookup).
// Summary found by predictable ID: {planId_sanitized}_validation_summary.
// No fallbacks. No querySelector scanning. ID-aware only.

import type { Plan, ContainerScope } from "../types";
import { resolveElement } from "../resolution/resolver";

const ERR_CLASS = "alis-has-error";

// -- Inline errors (next to visible fields) --

export function showInline(containerId: string, componentDomId: string, message: string): void {
  // componentDomId is a pre-resolved DOM ID (comp.id), not a component key.
  // Callers resolve via the shared resolver before calling this function.
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
  // componentDomId is a pre-resolved DOM ID (comp.id), not a component key.
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

// Validation summary is generated HTML ({planId}_validation_summary), NOT a plan component.
// getElementById is correct — summary elements are not registered in plan.components.
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

  // Error spans are generated HTML ({componentDomId}_error), NOT plan components.
  // getElementById is correct here — see findErrorSpan.
  const span = findErrorSpan(comp.id);
  if (span) {
    span.textContent = message;
    span.removeAttribute("hidden");
    span.style.display = "";
  }

  try {
    const el = resolveElement(plan, componentKey);
    el.classList.add(ERR_CLASS);
  } catch {
    // Element not in DOM — skip CSS class
  }
}

// -- Error span lookup -- ID only, no scanning --
// Error spans ({componentDomId}_error) are generated HTML from Html.Field(),
// NOT plan components. getElementById is correct here.

function findErrorSpan(componentDomId: string): HTMLElement | null {
  return document.getElementById(componentDomId + "_error");
}
