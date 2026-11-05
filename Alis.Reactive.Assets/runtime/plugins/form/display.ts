import { invalidClassOf } from "../model";

export function showInline(componentDomId: string, message: string): void {
  const componentElement = document.getElementById(componentDomId);
  if (componentElement) componentElement.classList.add(invalidClassOf(componentDomId));

  const errorSpan = findErrorSpan(componentDomId);
  if (errorSpan) {
    errorSpan.textContent = message;
    errorSpan.removeAttribute("hidden");
    delete errorSpan.dataset.alisFromServer;
  }
}

export function showServerInline(componentDomId: string, message: string): void {
  showInline(componentDomId, message);
  const errorSpan = findErrorSpan(componentDomId);
  if (errorSpan) errorSpan.dataset.alisFromServer = "";
}

export function showsServerMessage(componentDomId: string): boolean {
  return findErrorSpan(componentDomId)?.dataset.alisFromServer !== undefined;
}

export function clearInline(componentDomId: string): void {
  const errorSpan = findErrorSpan(componentDomId);
  if (errorSpan) {
    delete errorSpan.dataset.alisFromServer;
    errorSpan.textContent = "";
    errorSpan.setAttribute("hidden", "");
  }
  const componentElement = document.getElementById(componentDomId);
  if (componentElement) componentElement.classList.remove(invalidClassOf(componentDomId));
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
