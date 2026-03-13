import type { Plan } from "./types";
import { mergePlan } from "./boot";

/**
 * Inject HTML into a container, using ej.base.append when available (SF component init).
 * Extracts any <script data-alis-plan> elements first and merges them into the booted plan.
 */
export function injectHtml(container: HTMLElement, html: string): void {
  const temp = document.createElement("div");
  temp.innerHTML = html;

  // Extract plan elements before injection (ej.base.append can't handle script tags with JSON)
  const planEls = temp.querySelectorAll<HTMLElement>("[data-alis-plan]");
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

  // Merge extracted plans into booted plans
  for (const plan of plans) {
    mergePlan(plan);
  }
}
