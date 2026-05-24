// inject.ts — Inject HTML into a container.
// Extracts any <script data-reactive-plan> elements and merges them.

import type { Plan } from "../types";
import { loadPartialSlot, mergePlan, unloadPartialSlot } from "../lifecycle/boot";

interface SyncfusionBase {
  append(nodes: ChildNode[], target: HTMLElement, shouldClone?: boolean): void;
}

interface SyncfusionGlobal {
  readonly ej?: {
    readonly base?: SyncfusionBase;
  };
}

/**
 * Inject HTML into a container, using ej.base.append when available (SF component init).
 * Extracts any <script data-reactive-plan> elements first and merges them into the booted plan.
 */
export function injectHtml(container: HTMLElement, html: string): void {
  const temp = document.createElement("div");
  temp.innerHTML = html;

  // Extract plan elements before injection (ej.base.append can't handle script tags with JSON)
  const planEls = temp.querySelectorAll<HTMLElement>("[data-reactive-plan]");
  const plans: Plan[] = [];
  for (const el of planEls) {
    const text = el.textContent?.trim();
    if (!text) throw new Error("[alis] empty plan element in injected HTML");
    plans.push(JSON.parse(text));
    el.remove();
  }

  container.innerHTML = "";
  const ej = (globalThis as SyncfusionGlobal).ej;
  if (ej?.base?.append) {
    ej.base.append(Array.from(temp.childNodes), container, true);
  } else {
    container.append(...Array.from(temp.childNodes));
  }

  const containerIdentifiesPartialSlot = container.id.length > 0;
  if (containerIdentifiesPartialSlot) {
    if (plans.length === 0) {
      unloadPartialSlot(container.id);
    } else {
      loadPartialSlot(container.id, plans);
    }
    return;
  }

  for (const plan of plans) {
    mergePlan(plan);
  }
}
