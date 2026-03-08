import type { Plan, Entry } from "./types";
import { setLevel } from "./trace";
import { scope } from "./trace";
import { wireTrigger } from "./trigger";

const log = scope("boot");

export function boot(plan: Plan): void {
  log.info("booting", { entries: plan.entries.length });

  // Two-phase boot: wire all listeners first, then execute dom-ready reactions.
  // This ensures custom-event listeners exist before dom-ready dispatches into them.
  const deferred: Entry[] = [];

  for (const entry of plan.entries) {
    if (entry.trigger.kind === "dom-ready") {
      deferred.push(entry);
    } else {
      wireTrigger(entry.trigger, entry.reaction);
    }
  }

  for (const entry of deferred) {
    wireTrigger(entry.trigger, entry.reaction);
  }

  log.info("booted");
}

export const trace = { setLevel };
