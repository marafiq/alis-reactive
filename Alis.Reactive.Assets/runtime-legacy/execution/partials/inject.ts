// Partial injection strips Reactive Plan scripts before DOM append so Syncfusion
// can initialize the HTML and the slot's plans can be applied separately. The app hears
// alis:unmount before the old content goes and alis:mount before the new content is wired.

import type { PlanDocument } from "../../types/index";
import { loadPartialSlot, unloadPartialSlot } from "../../lifecycle/boot";
import { announceMount, announceUnmount } from "../../lifecycle/mount-events";

interface SyncfusionBase {
  append(nodes: ChildNode[], target: HTMLElement, shouldClone?: boolean): void;
}

interface SyncfusionGlobal {
  readonly ej?: {
    readonly base?: SyncfusionBase;
  };
}

export function injectPartial(container: HTMLElement, html: string, slotId: string): void {
  const fragmentHost = document.createElement("div");
  fragmentHost.innerHTML = html;

  const planElements = fragmentHost.querySelectorAll<HTMLElement>("[data-reactive-plan]");
  const slotPlans: PlanDocument[] = [];
  for (const planElement of planElements) {
    const planJson = planElement.textContent?.trim();
    if (!planJson) throw new Error("[alis] empty plan element in injected HTML");
    slotPlans.push(JSON.parse(planJson));
    planElement.remove();
  }

  announceUnmount(container);
  container.innerHTML = "";
  const ej = (globalThis as SyncfusionGlobal).ej;
  if (ej?.base?.append) {
    ej.base.append(Array.from(fragmentHost.childNodes), container, true);
  } else {
    container.append(...Array.from(fragmentHost.childNodes));
  }
  announceMount(container);

  if (slotPlans.length === 0) {
    unloadPartialSlot(slotId);
  } else {
    loadPartialSlot(slotId, slotPlans);
  }
}
