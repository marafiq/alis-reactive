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

## Remaining (this is many sessions — fan-out is the bulk)

- Unit 1: `verify-behavioral-coverage.mjs` + self-test (bites both ways) + wired
  into `scripts/test.sh`. (built + self-test green + `--all` no-op green; commit pending baseline)
- Unit 2: harden `verify-fusion-artifact-gates.mjs` for loophole #2 (status
  denylist `partial|wip|stub|some|todo-later` + matrix-status == proof-status).
- Unit 3: one-command driver that chains the deterministic gates (ALIS009 via
  build, 0a coverage, 0b behavioral, artifact verifier, parity, full gate) and
  fails loud on any gap.
- Parity tool (per-component exit b): no parity tool exists yet; the matrix
  generator extracts C# members and `inspect-syncfusion-surface.mjs` extracts the
  vendor surface, but no ≥95% computation is wired. Distinct deliverable.
- FusionSchedule event-CRUD to the full bar (the goal's proof target): discovery
  probes + traces, SQLite-backed sandbox CRUD with reload-preserves-state, 7
  behaviors, full behavioral-coverage.json map, blind-reviewer verdict, all gates
  green. Multi-commit, spans sessions.
- Fan-out across the remaining 49 components to the same bar.

## Baseline

`scripts/test.sh` from HEAD `04635938` before any change: **green**.
`BASELINE_EXIT=0`, "All gates green.", **1196 Playwright tests passed, 0 failed**
(57.0 min). Final TRX `playwright-20260613-070709.trx`. The run predated the
`test.sh` edit, so it is a clean measurement of the untouched tree.
