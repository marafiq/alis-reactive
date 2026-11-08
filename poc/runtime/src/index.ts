import { page, type Owner } from "./owners";
import { plansWithin, slotsNamedBy, start } from "./plans";
import { openRegistry } from "./plugins";
import { slot, slotOwner } from "./slots";

function boot(): void {
  openRegistry(
    new Map<string, (key: string) => unknown>([
      ["alis.slot", slot],
    ]),
  );
  document.dispatchEvent(new CustomEvent("alis:mount", { detail: { root: document.body } }));
  const found = plansWithin(document);
  const slots = new Set(found.flatMap(({ plan }) => slotsNamedBy(plan)));
  for (const { plan, script } of found) start(plan, ownerAtBoot(script, slots));
  document.documentElement.dataset["alisBooted"] = "true";
}

function ownerAtBoot(script: HTMLScriptElement, slots: ReadonlySet<string>): Owner {
  for (let around = script.parentElement; around !== null; around = around.parentElement) {
    if (slots.has(around.id)) return slotOwner(around.id);
  }
  return page;
}

function parsed(): boolean {
  const [navigation] = performance.getEntriesByType("navigation") as PerformanceNavigationTiming[];
  return document.readyState === "complete" || (navigation !== undefined && navigation.domContentLoadedEventStart > 0);
}

if (parsed()) boot();
else document.addEventListener("DOMContentLoaded", boot, { once: true });
