import { boot, trace } from "./boot";
import { init as initConfirm } from "./confirm";
import { initNativeActionLinks } from "./native-action-link";
import { composeInitialPlans } from "./merge-plan";
import type { Plan } from "./types";
import type { TraceLevel } from "./trace";

initConfirm();
initNativeActionLinks();

const planEls = document.querySelectorAll<HTMLElement>("[data-alis-plan]");
const plans: Plan[] = [];

for (const el of planEls) {
  const traceLevel = el.getAttribute("data-trace") as TraceLevel | null;
  if (traceLevel) trace.setLevel(traceLevel);

  plans.push(JSON.parse(el.textContent!));
}

for (const plan of composeInitialPlans(plans)) {
  boot(plan);
}
