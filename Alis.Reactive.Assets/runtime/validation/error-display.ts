// Validation error UI is generated HTML, not plan components. These helpers use
// predictable DOM IDs and dataset values instead of component lookup or selectors.

const ERR_CLASS = "alis-has-error";

export function showInline(componentDomId: string, message: string): void {
  const componentElement = document.getElementById(componentDomId);
  if (componentElement) componentElement.classList.add(ERR_CLASS);

  const errorSpan = findErrorSpan(componentDomId);
  if (errorSpan) {
    errorSpan.textContent = message;
    errorSpan.removeAttribute("hidden");
    errorSpan.style.display = "";
  }
}

export function clearInline(componentDomId: string): void {
  const errorSpan = findErrorSpan(componentDomId);
  if (errorSpan) {
    errorSpan.textContent = "";
    errorSpan.setAttribute("hidden", "");
    errorSpan.style.display = "none";
  }
  const componentElement = document.getElementById(componentDomId);
  if (componentElement) componentElement.classList.remove(ERR_CLASS);
}

export function addToSummary(summaryElement: HTMLElement, name: string, message: string): void {
  const item = document.createElement("div");
  item.dataset.valmsgSummaryFor = name;
  item.textContent = message;
  summaryElement.appendChild(item);
}

export function removeSummaryEntry(summaryElement: HTMLElement, name: string): void {
  const entry = findSummaryEntry(summaryElement, name);
  if (entry) entry.remove();
}

export function hasSummaryEntry(summaryElement: HTMLElement, name: string): boolean {
  return findSummaryEntry(summaryElement, name) !== undefined;
}

export function clearSummary(summaryElement: HTMLElement): void {
  summaryElement.innerHTML = "";
}

export function showSummaryDiv(summaryElement: HTMLElement): void {
  summaryElement.removeAttribute("hidden");
}

export function hideSummaryDiv(summaryElement: HTMLElement): void {
  summaryElement.setAttribute("hidden", "");
}

export function findSummaryElement(planId: string): HTMLElement | null {
  const summaryId = planId.replace(/[.+]/g, "_") + "_validation_summary";
  return document.getElementById(summaryId);
}

export function showServerErrorInline(
  componentDomId: string,
  message: string,
  element?: HTMLElement,
): void {
  const errorSpan = findErrorSpan(componentDomId);
  if (errorSpan) {
    errorSpan.textContent = message;
    errorSpan.removeAttribute("hidden");
    errorSpan.style.display = "";
  }

  element?.classList.add(ERR_CLASS);
}

function findErrorSpan(componentDomId: string): HTMLElement | null {
  return document.getElementById(componentDomId + "_error");
}

function findSummaryEntry(summaryElement: HTMLElement, name: string): HTMLElement | undefined {
  for (const child of summaryElement.children) {
    if (!(child instanceof HTMLElement)) continue;
    if (child.dataset.valmsgSummaryFor === name) return child;
  }

  return undefined;
}
