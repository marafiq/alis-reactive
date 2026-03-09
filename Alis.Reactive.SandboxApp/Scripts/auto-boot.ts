import { boot, trace } from "./boot";
import { init as initConfirm } from "./confirm";
import type { TraceLevel } from "./trace";

// Initialize app-level infrastructure before processing plans
initConfirm();

// Discover all plan elements: [data-alis-plan] (new) or fallback to #alis-plan (legacy)
const planEls = document.querySelectorAll<HTMLElement>("[data-alis-plan]");

if (planEls.length > 0) {
  // New: multiple plans per page
  for (const el of planEls) {
    const traceLevel = el.getAttribute("data-trace") as TraceLevel | null;
    if (traceLevel) {
      trace.setLevel(traceLevel);
    }
    boot(JSON.parse(el.textContent!));
  }
} else {
  // Legacy fallback: single #alis-plan element
  const planEl = document.getElementById("alis-plan");
  if (planEl) {
    const traceLevel = planEl.getAttribute("data-trace") as TraceLevel | null;
    if (traceLevel) {
      trace.setLevel(traceLevel);
    }
    boot(JSON.parse(planEl.textContent!));
  }
}
