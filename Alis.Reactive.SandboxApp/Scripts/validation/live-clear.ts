import type { Plan, RequestValidation } from "../types";
import { getBindingValue, getObjectContract, resolveObjectRoot, tryGetElementIdForBinding } from "../resolution/contracts";
import { clearInline } from "./error-display";
import { revalidateField } from "./orchestrator";
import { scope } from "../core/trace";

const log = scope("live-clear");
const wiredFields = new Set<string>();

export function wireLiveValidation(plan: Plan, desc: RequestValidation): void {
  for (const field of desc.fields) {
    const elementId = tryGetElementIdForBinding(plan, field.binding);
    if (!elementId || wiredFields.has(elementId)) continue;

    const el = document.getElementById(elementId);
    if (!el) continue;

    wiredFields.add(elementId);

    const uiField = { binding: field.binding, elementId };
    const clearHandler = () => clearInline(desc.formId, uiField);
    const revalidateHandler = () => revalidateField(plan, desc, field.binding);

    const binding = plan.bindings[field.binding];
    if (!binding || binding.object === "$eventObject") continue;

    const contract = getObjectContract(plan, binding.object);
    if (contract.resolver === "native-element") {
      el.addEventListener("input", clearHandler);
      el.addEventListener("blur", revalidateHandler);
      el.addEventListener("change", revalidateHandler);
      continue;
    }

    const root = resolveObjectRoot(plan, binding.object, { plan }) as EventTarget;
    root.addEventListener("change", revalidateHandler);
  }
}

export function unwireFields(fieldIds: string[]): void {
  for (const id of fieldIds) wiredFields.delete(id);
  if (fieldIds.length > 0) log.debug("unwired", { count: fieldIds.length, fieldIds });
}

export function resetLiveClearForTests(): void {
  wiredFields.clear();
}
