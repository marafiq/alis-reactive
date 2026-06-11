# SandboxApp — Proof Bed

Root CLAUDE.md is authoritative. The sandbox exists to prove framework behavior
with eyes and Playwright; it is not a product.

## View rules

- The plan is the only contract. No inline `<script>`, no
  `document.addEventListener`, no `window.alis`, no manual JS in any view —
  discovery and boot are automatic.
- Every reactive view ends with `@Html.RenderPlan(plan)`. All inputs go through
  `Html.InputField(...)`; non-input elements get explicit developer-chosen IDs.
- One exercise page demonstrates one primitive or one component slice, reachable
  from the sandbox navigation. A page nobody can reach proves nothing.
- A page is a journey slice: its model, controller partial, and view nest under
  the same concern path with aligned names, and it carries only elements a real
  application page would carry — no echo spans, no debug divs
  (`memory/bdd-principles.md` → Nested Vertical Slices; Grid's `Billing` is the exemplar).
- Validators register through DI before the view renders; form scope equals
  validator scope.

## Realtime

Under Playwright the app suppresses ambient broadcasts so realtime tests control
their own events. Connection-outage drill endpoints are test infrastructure:
keep them deterministic and world-scoped per page, never process-global.

## Run

`scripts/run.sh` → http://localhost:5220. Kill by port, not by process name —
a test suite may own a sibling instance.

Every documented code example originates from a working page here. If the
syntax never ran on a sandbox page, it does not ship in docs.
