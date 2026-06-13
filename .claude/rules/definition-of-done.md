# Definition of Done — the anti-drift contract

Drift is this repo's recurring, most expensive failure: "done" reported when work
merely compiles, is committed, or "should work" — short of the bar that was set.
This file makes "done" binary and applies to **every** kind of work. If a claim
of done cannot cite the command run and one line of its output, it is not done.
(Battle-tested by adversarial review 2026-06-13; the holes it found are closed below.)

## The universal spine (true for all work)

1. **Observed, not inferred.** Compiles / typechecks / committed / "should work"
   is necessary but NEVER sufficient. For anything user-facing, *done* means it
   was run and the outcome was seen (the gesture performed, the page changed).
2. **The set bar is met in full, or the gap is named.** "every X", "100%",
   "browser-verified", "all members" — sampling is not done; a spot-check reported
   as full coverage is the drift. State exactly what was and was not exercised.
3. **Verified / Assumed / Unchecked — as three labelled lists.** Every done-report
   ends with `VERIFIED:` (each line a command + its output), `ASSUMED:` (each with
   why it was not checked), `UNCHECKED:` (each with what would check it). An empty
   list is written `none`. Silence is not separation.
4. **A deterministic gate passed where one exists** — and "exists" means *wired
   into the gate that runs*, not "a script exists somewhere". Prefer a machine
   check over an adjective. Judgment work passes an independent review whose prompt
   could return REJECT, and the verdict is quoted. A secondhand result (subagent,
   tool, prior doc) is a lead, re-verified at the source `file:line`.
5. **No drift markers survive as "done".** Nothing labelled partial / pending /
   TODO / wip / stub / mostly / some / ~done is reported done. Tree left cleaner:
   no dead code, no new untyped/null markers, no dropped vocabulary, no schema revival.
6. **Past tense with evidence.** "Did X; ran Y; it printed Z." Never "done / works
   / green / 100%" beyond what was literally checked. If a bar was set, the number
   is the report, not an adjective — and quoted from the run, never from memory.

7. **Analyzer diagnostics are work, not noise.** Every analyzer warning/error
   (ALIS009 typed-gate, ALIS010 coverage, …) is ADDRESSED — fixed, or exempted
   with a documented reason (`[TypedDslExemption]` and the like) — never
   normalized, never accumulated as "expected output", never taken for granted.
   A warning is suppressed or deferred ONLY when the owner explicitly says to
   skip it. Treating a standing warning as acceptable is drift.

## What "100% coverage" MEANS (it cannot be hacked — BDD defines it)

Coverage is NOT a count of matrix rows linked to a proof file. By **BDD Rule 3
(fails-when-broken)**, a member is covered only when a Playwright test **FAILS if
that member's behavior breaks**. Consequences, non-negotiable:
- **One test does not cover many members.** A broad "variant" test catches only the
  one behavior it asserts; claiming it proves 20 payload members is the
  `covered-by-variant` inflation (Grid: 459 rows, ~31 tests). Each member needs its
  own fails-when-broken assertion or it is uncovered.
- **`row-proven` is a self-declared string, not proof.** Proof = a real test FQN
  that EXISTS in the suite and shows `Outcome="Passed"` in the latest parsed TRX.
  A markdown link, a substring match, or an on-disk `.trx` file is not proof.
- A test that still passes when the member is deleted does not cover it. Real
  interactions only (`bdd-principles.md`): no `page.evaluate()`, mocking, weak
  asserts, `[Ignore]`/skip, or assertions in unreached code.

## Acceptance by work type (each line binary; cite the evidence)

- **Refactor (C#/TS).** Behavior preserved (focused tests green); `dotnet build`
  net10 0/0 AND `npm test` green AND `npm run lint` clean over touched files;
  public contract unchanged unless the task required it; `plan.ts` diff checked
  (byte-identical unless plan shape was the point); the simplification is shown by
  a before/after number on the touched unit (lines, nesting depth, branch count
  strictly down, or a duplicated block removed) stated `N→M` — a refactor that
  moves no number says what was clarified and why none moved.
- **Visibility / contract tightening.** Solution builds net10 0/0; `plan.ts`
  byte-identical; vitest exits 0, zero skipped, count not decreased; full Playwright
  green; `git diff --stat` inspected and every changed file named with its reason.
- **net48 (the second TFM).** net48 0/0 is a **CI gate** (`verify-net48.yml`) — the
  macOS dev box cannot build it. "Both TFMs green" may be claimed ONLY from a green
  `verify-net48` run on this commit, else stated `UNCHECKED: net48 (pending CI)`.
  Never infer net48 from a passing net10 build.
- **Dead-code deletion.** ≥99% confidence with `file:line` proof of unreachability;
  adversarial prosecution upheld; full gate green AFTER. Never on pattern-match or
  zero-callers alone (frozen public surface = feature).
- **Rename / move.** Zero stale references repo-wide (full-output grep of the old
  name across code, `.claude`, docs, views, `plan.ts`); `plan.ts` regenerated if a
  serialized name moved; full gate green AFTER.
- **Bug fix.** Reproduced (seen); root-caused to `file:line` (cause, not symptom);
  fixed at the root layer; each boundary the fix crosses verified by its ritual
  (C#→contract = `plan.ts` regen + typecheck; contract→runtime = the vitest;
  runtime→browser = the gesture observed) and named; reported with the gesture and
  what the page showed.
- **New primitive.** All 9 steps of root Rule 3; `plan.ts` regenerated AND the
  runtime handler + `assertNever` arm for any new variant exists (typecheck-green
  after self-generation is not proof the contract is honored); behavior browser-proven.
- **Component onboarding.** BLOCKED until the behavioral coverage gate exists and is
  wired into `scripts/test.sh`: a gate that parses the matrix, resolves each member's
  test FQN, and confirms `Outcome="Passed"` in the latest TRX by exit code. Until
  then no component is "onboarded" — `scripts/test.sh` green and the documentation
  verifier do NOT satisfy this (they never run the verifier or parse a TRX). Then:
  typed-only (ALIS009 clean; exemptions only via `[TypedDslExemption]`); parity ≥95%
  generated by tool (not hand-counted); 100% coverage as defined above; 7-behavior +
  HTTP/SQLite where applicable; BDD-valid + blind-reviewer verdict quoted; artifacts
  + `verify-fusion-artifact-gates.mjs --component x` pass; full gate green.
  See `docs/superpowers/plans/GOAL-deterministic-onboarding-automation.md`.
- **Writing a view.** Opened in a browser, performed the gesture, saw the outcome;
  then a Playwright slice pins it.
- **Writing tests.** Each obeys the 5 BDD rules + framework primitives only; the
  coverage matrix lists every item in scope and maps each to a named test (not a
  doc link) that fails-when-broken or a justified exclusion; blind reviewer passed.
- **Writing docs.** The example was sandbox built-run-verified first; dev-facing
  voice; Rider/diagnostics clean on every touched file.
- **NuGet pack / release.** All six expected packages produced at the exact tagged
  SemVer (id+version asserted, not a count); net48 packs clean on Windows CI; the
  consumer-targets copy places the versioned runtime in `wwwroot/scripts/`. Six
  files on disk is not done.
- **Tooling / hook / gate-script / CI change.** Exercised by triggering it BOTH
  ways: it FIRES on the case it must catch AND passes on a clean case (attempt the
  forbidden edit and see the hook block; run the script and see the new leg run).
  Proven by its own behavior, never by code-reading.
- **Analyzer / gate.** Proven to BITE (fails on a real violation) AND green on the
  clean tree; accuracy audited (no false positives across the codebase); wired to block.
- **Audit / review.** Every item in scope listed and mapped (Coverage Completeness
  Gate); each finding re-opened at its `file:line` with the code quoted back, a
  finding whose quoted code does not support it dropped; report the counts (raised /
  confirmed / discarded).

## Spanning sessions is allowed — a hand-off is not a "done" claim

New-primitive and onboarding may span sessions. Report closed rows in past tense
with evidence AND the open rows explicitly (which of N steps remain, the next
command). The drift-marker ban (#5) governs **done reports**; an unstated partial
is the violation, a stated hand-off is the protocol.

## Relationship to other rules

This is authoritative for the question "is it done." The CLAUDE.md Post-Flight
Checklist remains the per-commit ritual; BDD acceptance lives in
`.claude/memory/bdd-principles.md`; onboarding exits in the GOAL doc. Where any
drifts from this file, this file governs the done-question.

## The one test before saying "done"

Name the command and the line of output that proves it. If a bar was set, the
number is the report. If you cannot, it is not done — say what remains, plainly.
