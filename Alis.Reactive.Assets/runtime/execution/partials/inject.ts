// Partial injection strips Reactive Plan scripts before DOM append so Syncfusion
// can initialize the HTML and the slot's plans can be applied separately.

import type { PlanDocument } from "../../types/index";
import { loadPartialSlot, unloadPartialSlot } from "../../lifecycle/boot";

interface SyncfusionBase {
  append(nodes: ChildNode[], target: HTMLElement, shouldClone?: boolean): void;
}

interface SyncfusionGlobal {
  readonly ej?: {
    readonly base?: SyncfusionBase;
  };
}

export function injectPartial(container: HTMLElement, html: string, slot: string): void {
  const temp = document.createElement("div");
  temp.innerHTML = html;

  const planEls = temp.querySelectorAll<HTMLElement>("[data-reactive-plan]");
  const plans: PlanDocument[] = [];
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

  if (plans.length === 0) {
    unloadPartialSlot(slot);
  } else {
    loadPartialSlot(slot, plans);
  }
}
