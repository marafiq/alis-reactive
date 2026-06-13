# Deterministic Onboarding Automation — Progress Ledger (2026-06-13)

Tracks the build of TASK ZERO 0b (the behavioral coverage gate) and the
one-command driver, then the fan-out across the 51 Fusion components to the
FusionGrid standard. Spans sessions: this file records closed and remaining
rows each time. Source of truth is the code; this ledger is the running map.

Goal doc: `docs/superpowers/plans/GOAL-deterministic-onboarding-automation.md`.

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
  - The sandbox is backed by `FakeScheduleData.cs` (static) — a REJECT for a
    stateful data component. The schedule sandbox must become SQLite-backed with
    create/update/delete and reload-preserves-state (automation-gates Gate 6).
  - No discovery artifacts beyond source-inventory + master index; no
    `behavioral-coverage.json`.
  - Driver run today: a=PASS, 0a=PASS, b=GAP, c=FAIL, f=FAIL, e=GAP — the exact
    gaps to close, in order. Multi-commit, spans sessions.
- Parity tool depends on the discovery artifact `public-api-surface.json`
  (vendor-member classification), so it is naturally built alongside the audit
  flow, not before it.
- Fan-out across the remaining 49 components to the same bar.

## Next session starts here

1. Drive FusionSchedule as an AUDIT (skill stages, existing C#/tests = evidence
   only): rebuild discovery artifacts, regenerate the fail-closed matrix.
2. Rebuild the schedule sandbox SQLite-backed (replace `FakeScheduleData`); prove
   create/update/delete + reload-preserves-state.
3. Write the 7-behavior + CRUD Playwright slice covering every matrix member.
4. Author `proof/behavioral-coverage.json`; run
   `drive-component-gates.mjs --component schedule --full` until every gate PASS.
5. Blind-review verdict quoted into `proof/blind-review.md`.

## Baseline

`scripts/test.sh` from HEAD `04635938` before any change: **green**.
`BASELINE_EXIT=0`, "All gates green.", **1196 Playwright tests passed, 0 failed**
(57.0 min). Final TRX `playwright-20260613-070709.trx`. The run predated the
`test.sh` edit, so it is a clean measurement of the untouched tree.
