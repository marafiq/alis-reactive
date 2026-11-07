import { http } from "./http";
import { page } from "./owners";
import { startWithin } from "./plans";
import { openRegistry } from "./plugins";
import { slot } from "./slots";

function boot(): void {
  openRegistry(
    new Map<string, (key: string) => unknown>([
      ["alis.http", () => http],
      ["alis.slot", slot],
    ]),
  );
  document.dispatchEvent(new CustomEvent("alis:mount", { detail: { root: document.body } }));
  startWithin(document, page);
  document.documentElement.dataset["alisBooted"] = "true";
}

function parsed(): boolean {
  const [navigation] = performance.getEntriesByType("navigation") as PerformanceNavigationTiming[];
  return document.readyState === "complete" || (navigation !== undefined && navigation.domContentLoadedEventStart > 0);
}

if (parsed()) boot();
else document.addEventListener("DOMContentLoaded", boot, { once: true });
