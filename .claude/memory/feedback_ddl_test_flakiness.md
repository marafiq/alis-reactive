---
name: DDL test flakiness — ej2 API workaround may mask real bugs
description: SF DropDownList re-selection via keyboard ArrowDown doesn't wrap reliably. ej2 API (value + dataBind) works but may produce misleading tests since it bypasses UI interaction.
type: feedback
---

SF DropDownList Playwright tests fail on re-selections — ArrowDown keyboard navigation doesn't wrap after a prior selection. The ej2 JS API (`ej2.value = x; ej2.dataBind()`) is deterministic but bypasses real user interaction, risking misleading green tests.

**Why:** Flaky DDL test cycles waste significant debugging time (observed: 5 iterations to stabilize P2 Care Level Cascade). But the ej2 API shortcut skips the popup click → item select path that real users follow, so it won't catch popup rendering or item-click handler regressions.

**How to apply:** Use ej2 API for now to unblock test coverage. Monitor whether this masks real bugs in future sessions. If it does, consider a hybrid approach: ej2 API for condition/cascade logic tests + a separate manual-test-warning process for DDL interaction fidelity. User is open to a process where flaky component tests print a warning to perform manual browser verification after the run.
