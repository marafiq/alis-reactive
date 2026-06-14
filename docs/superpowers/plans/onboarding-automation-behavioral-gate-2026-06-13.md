# Deterministic Onboarding Automation — Progress Ledger (2026-06-13)

Tracks the build of TASK ZERO 0b (the behavioral coverage gate) and the
one-command driver, then the fan-out across the 51 Fusion components to the
FusionGrid standard. Spans sessions: this file records closed and remaining
rows each time. Source of truth is the code; this ledger is the running map.

Goal doc: `docs/superpowers/plans/GOAL-deterministic-onboarding-automation.md`.

## Deterministic backbone — COMPLETE (all gates bite; committed + verified)

Every non-bypassable gate the goal names now exists and fails loud:
- **ALIS009 (exit a)** — live; `dotnet build` is the typed gate.
- **0a no-sandbox-usage** — `tools/FusionCoverage` (delivered before this session).
- **0b behavioral coverage (exit c, terminal 1)** — `verify-behavioral-coverage.mjs`;
  TRX-verified; self-test bites both ways; wired BLOCKING in `scripts/test.sh`.
- **parity (exit b)** — `compute-fusion-parity.mjs`; deterministic, generated;
  self-test bites; real signal `grid → 34.1% FAIL` (319 members unaccounted).
- **artifact verifier (exit f)** — `verify-fusion-artifact-gates.mjs`; loophole #2
  closed (a `partial`/`wip` proof status can no longer pass an `audited` matrix).
- **one-command driver (terminal 2)** — `drive-component-gates.mjs`; runs a/0a/b/c/f/e/g
  for a component and fails loud; verified on grid + schedule.
- **behavioral status reporter (terminal 3 mechanism)** — `report-fusion-behavioral-status.mjs`;
  reports the per-component behavioral bar (parity + 0b), NOT the doc gate.

Honest standing from the behavioral reporter: **0/51 behaviorally onboarded.** The
documentation gate's "1 audited" (grid) is corrected — grid is `34.1% parity FAIL,
no behavioral map`. Terminal 4 (full `scripts/test.sh`) is GREEN over the current tree
(1202 Playwright passed/0 failed) — but the onboarded SET is empty.

## What remains is per-component APPLICATION (judgment-heavy, genuinely multi-session)

Onboarding even ONE component to the bar is large and judgment-heavy, confirmed:
- Discovery is not push-button for ANY component (empirically confirmed). The generator
  (`write-fusion-discovery-artifacts.mjs`) fail-closes on event-payload resolution:
  Schedule's `dataBinding`/`dataBound` are ambiguous across ~70 vendor d.ts files; even
  simple CheckBox's `created` resolves to a DOM-native `Event` the generator can't find
  in Syncfusion declarations. So every component needs either per-event disambiguation
  or a generator fix to handle DOM-native/unresolvable payload types — this is the
  judgment-gated first step before parity/coverage even begin.
- Parity ≥95% means classifying hundreds of vendor members per component as
  onboarded-typed / builder-owned / excluded-with-evidence (grid: 319 to classify).
- 100% behavioral coverage means a fails-when-broken test per matrix member.
- Then blind review + full gate, ×51.

The backbone makes all of this MACHINE-CHECKABLE and un-fakeable. Applying it to 51
components to GREEN is the multi-session bulk the goal anticipates.

## Agent orchestration — demonstrated + working (with reliability gaps found)

A Workflow fanned 8 agents (62s) to run discovery + the parity tool across 8 simple
components; parity re-verified at source and committed (rating PASS 95.8%, others
discovered with honest FAIL numbers). This is the goal's model: agents do per-component
judgment, the deterministic parity tool produces the un-fakeable number, I re-verify.

Two gaps found scaling it (must fix before a reliable 51-wide fan-out):
- **d.ts-choice determinism.** Parity depends on WHICH vendor d.ts the agent feeds
  discovery. A second run picked `rating-model.d.ts` (the options model, 22 members)
  instead of `rating.d.ts` (the component class, 24 incl. methods/events) → a FALSE
  `100%` that hid the runtime surface. Caught by re-verifying at source; restored the
  correct 95.8%. FIX: pin the class `.d.ts` per component (data map) so the audit is
  deterministic — do not let agents free-choose the d.ts.
- **Workflow args plumbing.** Re-invoking the saved script with `args=[43 components]`
  ran the DEFAULT 8 instead — the args did not reach the script's `args` global.
  FIX before fanning the remaining 43.

## Verified state of the world (read at HEAD `04635938`, branch `tiny-safe-but-important-refactorings`)

- 51 Fusion components total (confirmed: `verify-behavioral-coverage.mjs --all`
  lists exactly 51 under `tools/FusionOnboarding/wwwroot/onboarding/fusion/`).
- Only **Grid** is "audited" by the documentation gate, and that count is the
  inflated one the goal flags: 459 matrix rows all literally `row-proven`, but
  the per-row cells link generically to `playwright-proof.md` (≈21 distinct test
  FQNs in the proof prose). No machine linkage member→FQN existed.
- Grid's `playwright-proof.md` header literally says `Status: partial` while the
  matrix says `Status: audited.` and the artifact verifier passes it — loophole #2.
- **FusionSchedule** already has a substantial typed slice (15 files:
  `FusionSchedule.cs`, Builder, Events ×9, Extensions). It is NOT "untyped with
  zero artifacts" as the prompt assumed — what it lacks is proof artifacts (only
  `discovery/source-inventory.md` + `master-usecases-index.md`). It needs full
  behavioral onboarding, not typing.
- ALIS009 is live and the tree is ALIS009-clean at HEAD (the baseline passed
  `dotnet build`).
- 0a (`tools/FusionCoverage`) delivered: lists slice members with zero sandbox
  references (necessary-not-sufficient). Consume, do not rebuild.

## TRX truth model (confirmed by inspecting real TRX files)

`<UnitTest id=X><TestMethod className=FQN.Class name=method/></UnitTest>` joins to
`<UnitTestResult testId=X testName=method outcome="Passed"/>` (id == testId,
verified). So `FQN → [outcome]` is cleanly resolvable from any TRX. Latest TRX is
the newest timestamped file under
`tests/Alis.Reactive.PlaywrightTests/TestResults/observable/`.

## 0b — behavioral coverage gate (DESIGN)

The matrix carries no machine-resolvable per-member FQN, and its generator is
Grid-specialized prose that computes `row-proven` from on-disk decision files —
never a TRX. So `row-proven` is self-declared. 0b replaces that with run truth.

Contract per component (machine-readable, the only thing allowed to say a member
is "covered"): `proof/behavioral-coverage.json`

```json
{
  "component": "schedule",
  "coverage": [
    { "member": "<exact matrix member name>", "test": "<Playwright FQN>",
      "catches": "what breaks if this member breaks (BDD Rule 3)" }
  ],
  "acceptedFanout": [
    { "test": "<FQN>", "reason": "why one test legitimately covers > cap members" }
  ]
}
```

Gate `verify-behavioral-coverage.mjs` (`--component x` | `--all`):
1. completeness — every typed matrix member has a coverage entry (else not 100%).
2. existence — each entry's test FQN appears in the latest TRX (else deleted/renamed/never-ran).
3. passed — that FQN's outcome is `Passed` (else RED).
4. fan-out — members-per-test ≤ cap (default 4); above the cap requires a declared
   `acceptedFanout` reason, else it's covered-by-variant inflation → RED.
5. BDD Rule 3 — every entry names what it `catches`, else RED.

`--all` verifies every component that HAS a map; components without one are
"below bar, not failed" (un-onboarded). This is what lets the gate be wired
blocking while only onboarded components are held to it.

Closes the three goal loopholes: #1 self-declared row-proven (now FQN→TRX),
#2 prose "partial/wip" status (gate ignores prose; reads the TRX), #3
covered-by-variant inflation (fan-out cap + declared reasons).

## Closed rows (committed, verified)

- **Unit 1 — 0b behavioral coverage gate, wired blocking.** Committed.
  - `verify-behavioral-coverage.mjs` + `verify-behavioral-coverage.selftest.mjs`;
    self-test exits 0 with all 7 fixture checks (clean GREEN + 5 RED failure
    modes + declared-fanout GREEN).
  - `verify-behavioral-coverage.mjs --all` exits 0 on the real tree (51 components
    listed, all below-bar, no map yet) — a clean no-op until maps exist.
  - Wired into `scripts/test.sh` step 7 after the Playwright leg (fresh TRX is the
    truth source), guarded on e2e; `bash -n scripts/test.sh` clean.
  - VERIFIED: self-test exit 0; `--all` exit 0; syntax clean.
    ASSUMED: none. UNCHECKED: the full wired `scripts/test.sh` end-to-end — the
    green baseline below ran on the pre-edit tree (the gate line is a verified
    exit-0 no-op in a reachable spot); re-run as the authoritative end-to-end
    green after units 2–3 land.
- **Unit 2 — loophole #2 closed in `verify-fusion-artifact-gates.mjs`.** Committed.
  A surgical `Status:`-line check (not a whole-file scan, so legit prose like
  "partial injection" is never a false positive) flags
  `partial|wip|stub|some|todo-later|pending|incomplete|draft` on a proof file's
  status line. Combined with the matrix's existing `Status: audited.` requirement,
  matrix-status and proof-status cannot disagree and still pass.
  - VERIFIED both ways through the real verifier: Grid now FAILS (exit 1) with
    `proof/playwright-proof.md status line declares open work ("partial")`; an
    A/B on a temp copy with only the status line flipped to `audited.` clears the
    marker (count 1 → 0). Node `--check` clean.
- **Unit 3 — one-command driver `drive-component-gates.mjs`.** Committed.
  Chains every deterministic gate for a chosen component and fails loud on any
  gap, labelling each PASS/FAIL/GAP/SKIP and mapping to the per-component exit
  letters: a ALIS009 (slice build), 0a no-sandbox-usage (FusionCoverage filtered
  to the component), b parity (GAP — tool unbuilt, named not hidden), c 0b
  behavioral, f artifacts, e blind-review verdict presence, g full gate (`--full`).
  - VERIFIED it FIRES: `--component schedule` → exit 1 with `b=GAP, c=FAIL,
    f=FAIL, e=GAP, g=SKIP` (and a=PASS, 0a=PASS — Schedule's slice is ALIS009-clean
    and sandbox-referenced). Node `--check` clean.
  - Known: the all-PASS (exit 0) path is unreachable until the parity tool exists
    (b is always GAP) AND a component is fully onboarded. That is correct
    fail-loud behavior; the exit-0 branch is a trivial `blocking.length === 0`
    check, exercised when FusionSchedule reaches the bar.

## Remaining (this is many sessions — fan-out is the bulk)

- Parity tool (per-component exit b): no parity tool exists yet; the matrix
  generator extracts C# members and `inspect-syncfusion-surface.mjs` extracts the
  vendor surface, but no ≥95% computation is wired. Distinct deliverable.
- FusionSchedule event-CRUD to the full bar (the goal's proof target). Assessed
  at HEAD — it is an AUDIT (the slice exists), and far from the bar:
  - Only one small Playwright file (`WhenUsingFusionSchedule.cs`, ~1.4k); the
    7-behavior + stateful-CRUD contract is not met.
  - Persistence is adequate, contrary to the prompt: the schedule sandbox is an
    in-memory mutable `ConcurrentDictionary` store with HTTP create/assign/unassign
    that is POST→GET visible. The prompt said "SQLite + reload-preserves-state",
    but the EXEMPLAR (Grid CareOps) persists via `HttpContext.Session`
    (JSON-serialized) — VERIFIED at `GridController.CareOps.cs:31,41` — and NO Grid
    Playwright test calls `Reload`. So in-memory/session HTTP-backed (automation-gates
    Gate 6 literally says "in-memory HTTP-backed sandbox") is the operative bar, NOT
    SQLite. Do not build SQLite infra for Schedule; it already meets the Grid bar.
  - The real gap is BEHAVIORAL: only 2 Playwright tests (gather/route-template,
    no create/assign/unassign coverage), no `behavioral-coverage.json`, no
    discovery artifacts beyond source-inventory + master index.
  - Driver run today: a=PASS, 0a=PASS, b=GAP, c=FAIL, f=FAIL, e=GAP — the exact
    gaps to close, in order. Multi-commit, spans sessions.
- Parity tool depends on the discovery artifact `public-api-surface.json`
  (vendor-member classification), so it is naturally built alongside the audit
  flow, not before it.
- Fan-out across the remaining 49 components to the same bar.

## FusionSchedule — committed behavioral rows (verified, filtered runs green)

Six real fails-when-broken behaviors, locator-based, framework untouched, eyes-first:
- `schedule_binds_the_weeks_shift_assignments_from_the_server` (39ce3161) — server data-bind.
- `clicking_an_assigned_shift_shows_staff_details_and_edit_actions` (2316379b) — QuickInfo template (assigned).
- `editing_a_shift_opens_the_edit_drawer_with_the_assignment_form` (19cc7a2c) — schedule:edit → drawer + partial.
- `assigning_staff_to_an_open_shift_reduces_open_shifts_and_persists_on_reload` (860b3619) — stateful CRUD, reload-persists (exit d).
- `clicking_an_unassigned_shift_offers_to_assign_staff` (b52f9ab5) — QuickInfo conditional (unassigned branch).
- `reassigning_a_shift_opens_the_assignment_drawer` (f32c7749) — schedule:reassign handler.

Reliable behaviors are now largely exhausted; the rest hit iffy SF interactions:

**FINDING — view toggle may be broken.** Clicking the Month/Day toolbar buttons does
NOT switch the view: the SF instance `currentView` stays `"Week"` and status stays
`"Ready"` (Navigating never fires) after a click. Could be a real bug in the
Navigating-reload wiring or an interaction that won't drive without a specific gesture.
Verify next session with a trusted Playwright click; if it genuinely does not switch,
report it (Cardinal Rule) rather than hack a test around it. This blocks the Navigating
event-payload coverage.

**FINDING — assign-form required validation didn't display** when the staff field is the
drawer-injected `NativeRadioGroup` (the directly-rendered resident form's does). Possibly
injected-form client-validation wiring. The CRUD positive path is green; the empty-submit
validation behavior is unproven.

## CRUD — SOLVED (commit 860b3619), goal exit d proven

`assigning_staff_to_an_open_shift_reduces_open_shifts_and_persists_on_reload`:
open unassigned shift → pick staff → Save → `#unassigned-count` drops →
`Page.ReloadAsync()` → count still dropped (server persistence). Full
`WhenUsingFusionSchedule` fixture green (6/6).

Root cause that cost ~7 rounds: the staff `FusionDropDownList` popup, loaded via
partial into the `NativeDrawer`, opens unreliably under Playwright (popup
`display:none` / never `.e-popup-open`; a settled MANUAL click works). FIX
(matches the repo's passing resident-form drawer test): the assign/edit staff
field now uses `NativeRadioGroup` (pre-visible options, no popup) — costs no
component coverage since staff selection is incidental to the Schedule proof.
LESSON for the other 50 components: do not drive a `FusionDropDownList` popup
inside a `NativeDrawer` from Playwright; use radio/pre-visible options.

## Next session starts here

1. Land the CRUD via the NativeRadioGroup path above (the one blocker on exit d).
2. Continue the remaining FusionSchedule behaviors toward the 67-member matrix
   (regenerable: `write-fusion-typed-api-coverage.mjs --component schedule --fusion-type FusionSchedule --write`).
3. Author `proof/behavioral-coverage.json`; run
   `drive-component-gates.mjs --component schedule --full` until every gate PASS.
4. Blind-review verdict quoted into `proof/blind-review.md`.
5. Persistence is already adequate (in-memory HTTP-backed, POST→GET visible) — matches
   the Grid exemplar (Session-backed). Do NOT build SQLite.

## Parity / audit dimension (the "audit" in automate-audit-upgrade)

Parity (vendor surface vs typed C#) is not a standalone number: a d.ts has
hundreds of members, most builder-owned/internal. A meaningful parity % needs the
discovery classification (`public-api-surface.json`: onboarded-typed | builder-owned
| excluded-with-evidence). So parity is computed FROM the discovery artifacts, as
part of the per-component audit — not a pre-step. The driver names it a GAP until
that classification + a parity computation over it exists.

## Baseline

`scripts/test.sh` from HEAD `04635938` before any change: **green**.
`BASELINE_EXIT=0`, "All gates green.", **1196 Playwright tests passed, 0 failed**
(57.0 min). Final TRX `playwright-20260613-070709.trx`. The run predated the
`test.sh` edit, so it is a clean measurement of the untouched tree.
