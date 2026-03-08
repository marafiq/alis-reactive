import { boot, trace } from "./boot";
import type { TraceLevel } from "./trace";

const planEl = document.getElementById("alis-plan");
if (planEl) {
  const traceLevel = planEl.getAttribute("data-trace") as TraceLevel | null;
  if (traceLevel) {
    trace.setLevel(traceLevel);
  }
  boot(JSON.parse(planEl.textContent!));
}
