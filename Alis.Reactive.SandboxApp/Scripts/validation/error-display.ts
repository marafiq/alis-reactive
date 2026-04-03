export interface ValidationFieldView {
  binding: string;
  elementId?: string;
}

const ERR_CLASS = "alis-has-error";

export function showInline(formId: string, field: ValidationFieldView, message: string): void {
  if (field.elementId) {
    const el = document.getElementById(field.elementId);
    if (el) el.classList.add(ERR_CLASS);
  }

  const span = findErrorSpan(field);
  if (span) {
    span.textContent = message;
    span.removeAttribute("hidden");
    span.style.display = "";
  }
}

export function clearInline(formId: string, field: ValidationFieldView): void {
  const span = findErrorSpan(field);
  if (span) {
    span.textContent = "";
    span.setAttribute("hidden", "");
    span.style.display = "none";
  }

  if (field.elementId) {
    const el = document.getElementById(field.elementId);
    if (el) el.classList.remove(ERR_CLASS);
  }
}

export function clearAllInline(formId: string, fields: ValidationFieldView[]): void {
  for (const field of fields) clearInline(formId, field);
}

export function addToSummary(summaryEl: HTMLElement, binding: string, message: string): void {
  const item = document.createElement("div");
  item.dataset.valmsgSummaryFor = binding;
  item.textContent = message;
  summaryEl.appendChild(item);
}

export function removeSummaryEntry(summaryEl: HTMLElement, binding: string): void {
  const entry = summaryEl.querySelector(`[data-valmsg-summary-for="${binding}"]`);
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

export function showServerErrorInline(
  formId: string,
  binding: string,
  message: string,
  fields: ValidationFieldView[]
): void {
  const field = fields.find(item => item.binding === binding);
  const span = field ? findErrorSpan(field) : null;
  if (span) {
    span.textContent = message;
    span.removeAttribute("hidden");
    span.style.display = "";
  }

  if (field?.elementId) {
    const el = document.getElementById(field.elementId);
    if (el) el.classList.add(ERR_CLASS);
  }
}

function findErrorSpan(field: ValidationFieldView): HTMLElement | null {
  if (!field.elementId) return null;
  return document.getElementById(field.elementId + "_error");
}
