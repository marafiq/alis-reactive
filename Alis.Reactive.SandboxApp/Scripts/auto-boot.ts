import { boot, trace } from "./boot";
import { init as initConfirm } from "./confirm";
import { initNativeActionLinks } from "./native-action-link";
import type { Plan } from "./types";
import type { TraceLevel } from "./trace";

initConfirm();
initNativeActionLinks();

const planEls = document.querySelectorAll<HTMLElement>("[data-alis-plan]");
const byPlanId = new Map<string, Plan>();

for (const el of planEls) {
  const traceLevel = el.getAttribute("data-trace") as TraceLevel | null;
  if (traceLevel) trace.setLevel(traceLevel);

  const raw: Plan = JSON.parse(el.textContent!);
  const key = raw.planId;

  if (byPlanId.has(key)) {
    const existing = byPlanId.get(key)!;
    Object.assign(existing.components, raw.components);
    existing.entries.push(...raw.entries);
  } else {
    byPlanId.set(key, raw);
  }
}

for (const plan of byPlanId.values()) {
  boot(plan);
}
