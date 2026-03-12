import { boot, trace } from "./boot";
import { init as initConfirm } from "./confirm";
import type { TraceLevel } from "./trace";

initConfirm();

const planEls = document.querySelectorAll<HTMLElement>("[data-alis-plan]");

if (planEls.length > 0) {
  for (const el of planEls) {
    const traceLevel = el.getAttribute("data-trace") as TraceLevel | null;
    if (traceLevel) {
      trace.setLevel(traceLevel);
    }
    boot(JSON.parse(el.textContent!));
  }
} else {
  const planEl = document.getElementById("alis-plan");
  if (planEl) {
    const traceLevel = planEl.getAttribute("data-trace") as TraceLevel | null;
    if (traceLevel) {
      trace.setLevel(traceLevel);
    }
    boot(JSON.parse(planEl.textContent!));
  }
}
