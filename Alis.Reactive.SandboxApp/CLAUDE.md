# SandboxApp — Proof Bed

Root CLAUDE.md is authoritative. The sandbox exists to prove framework behavior
with eyes and Playwright; it is not a product.

## Pages

Views follow `.claude/rules/views.md`; a page is a journey slice (`.claude/rules/tests.md`, Nested
vertical slices; Grid's `Billing` is the exemplar).

- One exercise page demonstrates one primitive or one component slice, reachable
  from the sandbox navigation. A page nobody can reach proves nothing.

## Realtime

Under Playwright the app suppresses ambient broadcasts so realtime tests control
their own events. Connection-outage drill endpoints are test infrastructure:
keep them deterministic and world-scoped per page, never process-global.

## Run

`scripts/run.sh` → http://localhost:5220. Kill by port, not by process name —
a test suite may own a sibling instance.
