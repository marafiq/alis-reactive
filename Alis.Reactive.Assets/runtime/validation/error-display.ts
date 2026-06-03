// Validation error UI is generated HTML, not plan components. These helpers use
// predictable DOM IDs and dataset values instead of component lookup or selectors.

const ERR_CLASS = "alis-has-error";

// Inline helpers receive pre-resolved DOM IDs (comp.id), not component keys.
export function showInline(componentDomId: string, message: string): void {
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
  const el = document.getElementById(componentDomId);
  if (el) el.classList.remove(ERR_CLASS);
}

export function addToSummary(summaryEl: HTMLElement, name: string, message: string): void {
  const item = document.createElement("div");
  item.dataset.valmsgSummaryFor = name;
  item.textContent = message;
  summaryEl.appendChild(item);
}

export function removeSummaryEntry(summaryEl: HTMLElement, name: string): void {
  const entry = findSummaryEntry(summaryEl, name);
  if (entry) entry.remove();
}

export function hasSummaryEntry(summaryEl: HTMLElement, name: string): boolean {
  return findSummaryEntry(summaryEl, name) !== undefined;
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

export function showServerErrorInline(
  componentDomId: string,
  message: string,
  element?: HTMLElement,
): void {
  const span = findErrorSpan(componentDomId);
  if (span) {
    span.textContent = message;
    span.removeAttribute("hidden");
    span.style.display = "";
  }

  element?.classList.add(ERR_CLASS);
}

// Error spans ({componentDomId}_error) are generated HTML from Html.Field(),
// NOT plan components. getElementById is correct here.
function findErrorSpan(componentDomId: string): HTMLElement | null {
  return document.getElementById(componentDomId + "_error");
}

function findSummaryEntry(summaryEl: HTMLElement, name: string): HTMLElement | undefined {
  for (const child of summaryEl.children) {
    if (!(child instanceof HTMLElement)) continue;
    if (child.dataset.valmsgSummaryFor === name) return child;
  }

  return undefined;
}
