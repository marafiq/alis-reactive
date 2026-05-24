// Error Display — Single responsibility: DOM error manipulation
// V3: uses component IDs directly (not ValidationField objects).
// Error spans found by predictable ID: {componentDomId}_error (O(1) lookup).
// Summary found by predictable ID: {planId_sanitized}_validation_summary.
// No fallbacks. No querySelector scanning. ID-aware only.

const ERR_CLASS = "alis-has-error";

// -- Inline errors (next to visible fields) --

export function showInline(componentDomId: string, message: string): void {
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

export function clearInline(componentDomId: string): void {
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
export function findSummaryElement(planId: string): HTMLElement | null {
  const summaryId = planId.replace(/[.+]/g, "_") + "_validation_summary";
  return document.getElementById(summaryId);
}

// -- Server error inline display --

export function showServerErrorInline(
  componentDomId: string,
  message: string,
  element?: HTMLElement,
): void {
  // Error spans are generated HTML ({componentDomId}_error), NOT plan components.
  // getElementById is correct here — see findErrorSpan.
  const span = findErrorSpan(componentDomId);
  if (span) {
    span.textContent = message;
    span.removeAttribute("hidden");
    span.style.display = "";
  }

  element?.classList.add(ERR_CLASS);
}

// -- Error span lookup -- ID only, no scanning --
// Error spans ({componentDomId}_error) are generated HTML from Html.Field(),
// NOT plan components. getElementById is correct here.

function findErrorSpan(componentDomId: string): HTMLElement | null {
  return document.getElementById(componentDomId + "_error");
}
