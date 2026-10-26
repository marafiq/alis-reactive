// Validation messages live in generated HTML: an inline slot {id}_error beside each field and one
// summary per plan. These helpers reach them by ID only.

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

export function summaryHasEntries(summaryElement: HTMLElement): boolean {
  return summaryElement.children.length > 0;
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

export function canRenderInline(componentDomId: string): boolean {
  const errorSpan = findErrorSpan(componentDomId);
  if (errorSpan === null) return false;
  for (let node: HTMLElement | null = errorSpan.parentElement; node; node = node.parentElement) {
    if (node.hasAttribute("hidden") || node.style.display === "none") return false;
  }
  return true;
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
