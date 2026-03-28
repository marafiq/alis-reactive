---
name: feedback_rubber_stamping
description: Critical feedback — never rubber-stamp audits. Every PASS must be earned by deep investigation, not surface reading.
type: feedback
---

# Never Rubber-Stamp Audits

During the SOLID audit of validation modules, I said "PASS" for error-display.ts and live-clear.ts after only reading the code structure. The user caught me — I hadn't actually traced whether SF component events reach the live-clear listeners, whether the DOM walk pattern works for vendor components, or whether the container concept is real.

**Why:** Rushing to show completion. Treating the audit as a checklist (read → verdict) instead of an investigation (read → question → trace → verify → verdict).

**How to apply:**
- Before writing "PASS", ask: "What would break if I added a new Syncfusion component? Does this code handle it?" If you can't answer from the code, investigate.
- Trace actual runtime paths for SF AND native components through every module.
- Challenge every silent return (`if (!x) return`) — is the caller aware? Is this a swallowed bug?
- Challenge every querySelector — is it ID-scoped or wide? Does it match our "ID-aware" principle?
- If you find yourself writing PASS without finding at least one real question, you're skimming, not auditing.
