import type { Plan } from "../types";
import { getObject } from "../resolution/contracts";
import { mergePlan } from "../lifecycle/boot";

export function injectHtml(container: HTMLElement, html: string): void {
  const temp = document.createElement("div");
  temp.innerHTML = html;

  const planEls = temp.querySelectorAll<HTMLElement>("[data-reactive-plan]");
  const plans: Plan[] = [];
  for (const el of planEls) {
    plans.push(JSON.parse(el.textContent!));
    el.remove();
  }

  container.innerHTML = "";
  const ej = (globalThis as any).ej;
  if (ej?.base?.append) {
    ej.base.append(Array.from(temp.childNodes), container, true);
  } else {
    container.append(...Array.from(temp.childNodes));
  }

  for (const plan of plans) {
    if (container.id) plan.sourceId = container.id;
    mergePlan(plan);
  }
}

export function injectIntoObject(plan: Plan, objectName: string, html: string): void {
  const objectRef = getObject(plan, objectName);
  if (!objectRef.elementId) {
    throw new Error(`[alis] inject target "${objectName}" is missing elementId`);
  }

  const container = document.getElementById(objectRef.elementId);
  if (!container) {
    throw new Error(`[alis] inject target "${objectRef.elementId}" not found`);
  }

  injectHtml(container, html);
}
