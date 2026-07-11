# GOAL — Deterministic Onboarding Automation

Build the final piece: a deterministic pipeline to onboard/audit/upgrade
Syncfusion (and Native) components into a **fully typed** C# DSL at **~95% JS
parity**. If it doesn't land, devs fall back to JS escape hatches in `.cshtml` —
anti-vision. It hasn't landed because it's been manual discipline, not
structural enforcement (agents stop at "compile", skip artifacts — FusionSchedule
shipped untyped with zero artifacts because nothing blocked it).

Make the BUILD the gate, not the agent.

**Before acting, read the code to build real context** — do not act from this
prompt or memory. Read: the root `CLAUDE.md`; the target component slice under
`Alis.Reactive.Fusion/Components/` (or Native); the DSL/runtime source it
touches; any existing artifacts under `tools/FusionOnboarding/.../{component}/`;
ALIS009 + `[TypedDslExemption]`; an exemplar fully-onboarded slice (FusionGrid).
This work continues on the current RC3 branch (not a fresh `main`); build context
from the current tree as it stands.

## TASK ZERO (build the deterministic coverage signals BEFORE onboarding any component)
The automation cannot honestly onboard anything until coverage is machine-known.
Build these first — they are the spine every later phase leans on:
0a. **Coverage "no sandbox usage" signal — DELIVERED; consume it, do NOT rebuild.**
    Built as `tools/FusionCoverage` (run:
    `dotnet run --project tools/FusionCoverage -- Alis.Reactive.SandboxApp/bin/Debug/net10.0`).
    It reads compiled IL — the public Fusion/Native slice surface from their dlls vs the
    SandboxApp dll's MemberReference table (views compile into the dll, so their calls land
    there) — and lists every public component-slice member with zero sandbox references.
    Chosen over an in-build analyzer because the analyzer cannot see Razor-generated view
    trees (the ALIS010 prototype false-positived on `NativeButton.Reactive`/`CssClass`; 1,482
    warnings). Overload-precise (param-type keys) and adversarially verified false-positive-free
    (`NativeButton.Reactive` not flagged; `NativeButtonBuilder.CssClass` used vs
    `NativeCheckListBuilder.CssClass` unused correctly separated). NECESSARY condition
    (referenced in a view), not sufficiency — behavioral proof is 0b below.
0b. **Behavioral coverage gate.** Parses the matrix → resolves each member's test
    FQN against the suite → confirms `Outcome="Passed"` in the latest TRX by exit
    code; wired blocking into `scripts/test.sh`. This is sufficiency.
Together with ALIS009 (typed, Error): typed → used (warn) → proven-passing (block).

## The automation must
1. Extract vendor JS surface (`d.ts`/`.js`) + implemented C# surface (Roslyn) →
   compute **parity**; un-onboarded public members below bar = FAIL.
2. Make `verify-fusion-artifact-gates.mjs` a **blocking** gate in `scripts/test.sh`.
3. Refuse "done" unless every exit condition below holds, failing loud on any gap.

## EXIT CONDITIONS (binary — a component is onboarded only when ALL hold)
1. Source-grounded probe + browser trace committed — `onboard-fusion-component/SKILL.md` workflow.
2. **Typed only**: ALIS009 clean (no `object`/selector-string/wire type in public slice API). Exceptions only via `[TypedDslExemption(reason)]`. Plugin name is the lone string, at the plugin boundary.
3. **Parity met** (≥95%): every public vendor member onboarded-typed | builder-owned | excluded-with-evidence. Generated, not asserted.
4. **100% behavior coverage — MACHINE-VERIFIED, not self-declared**: every public member maps to a Playwright test whose FQN **exists in the suite AND passed in the current TRX**. A gate parses the matrix → resolves each test FQN → confirms green. A `row-proven` string + markdown link is NOT proof (today's verifier checks only files-exist + the literal cell — close this; see loopholes). (Root `CLAUDE.md` → Coverage Completeness Gate.)
5. **7-behavior contract** + **stateful HTTP/SQLite CRUD proof** where applicable — `automation-gates.md` Gates 5–7 (reload-preserves-state mandatory for grids/schedulers/lists).
6. **BDD-valid + blind-reviewed** — every test obeys `.claude/memory/bdd-principles.md` (5 Rules, framework-primitives-only, nested vertical slice, No-Hack rules); a blind reviewer agent passes them.
7. **Artifacts complete**: `verify-fusion-artifact-gates.mjs --component {x}` passes.
8. **Full gate green** on rebuilt assets: typecheck (plan.ts no drift) → vitest → `dotnet build` both TFMs (ALIS009 active) → `scripts/playwright.sh`. Observed, not inferred.

## Non-negotiable (REJECT — this is "half work")
Untyped public API (ALIS009) · compiles-but-no-sandbox-usage · one assertion ≠ 100% ·
static-array example for a data component · any missing artifact · a test that passes
when the feature is broken · `page.evaluate()`/mocking/`Thread.Sleep`/`[Retry]`/weak asserts.
Full reject table: `automation-gates.md` → "Done Means Done".

## Close these loopholes FIRST (found in the current verifier + Grid exemplar)
The verifier (`verify-fusion-artifact-gates.mjs`) gates on DOCUMENTATION, not
behavior — so the matrix can lie. Before onboarding the other 50 components,
make the gate behavioral or every new component inherits the same theater:
1. **`row-proven` is self-declared.** The gate must resolve each row's test FQN
   against the Playwright project AND the latest TRX (exists + passed). Run it.
2. **"partial" slips the denylist.** Grid's `playwright-proof.md` says
   `Status: partial` while the matrix says `audited`, and it passes. Add
   `partial|some|todo-later|wip|stub` to the open-marker denylist and cross-check
   matrix-status == proof-status == TRX truth.
3. **"covered-by-variant" inflation.** 459 rows "proven" by ~31 tests; one variant
   test claims dozens of members. Bound it: a member proven-by-variant must name
   the variant test, and that test must actually exercise the member (the skill's
   variant-sensitive rule). Flag fan-out beyond a threshold for review.
Grid is genuinely onboarded (31 real passing tests) — it is the EXEMPLAR to
replicate; but its 459/459 count is not trustworthy until the gate is behavioral.

## Orchestration (you may run a Workflow or a loop — fully automated, agents prompt each other)
Scale: 50 components at static-discovery -> audited, to Grid's standard. Shape it
as a per-component pipeline, looped or fanned-out, with DETERMINISTIC gates as the
non-bypassable backbone and JUDGMENT gates as agent steps:
- deterministic (machine, cannot be argued): ALIS009 typed gate · the behavioral
  coverage verifier above · parity computation · full `scripts/test.sh` green.
- judgment (agent, then blind-reviewed): primitive mapping · which members to
  accept/exclude · test design.
Agent roles that prompt each other per component: discovery (probe+trace) ->
mapping (primitive map + name decisions) -> implementation (typed slice) ->
test author (sandbox + Playwright, 7-behavior, HTTP/SQLite) -> blind reviewer ->
verify gate. A phase may not start until the prior phase's committed artifact
exists and the deterministic gate for it is green. Terminal exit: the status
reporter shows 51/51 audited with the behavioral coverage gate passing — not the
documentation gate.

## Reference files (read these FIRST; do not re-derive)
- **Authoritative project law: `CLAUDE.md` (root) / `AGENTS.md`** — architecture, the 14 rules, the Must/Never lists, the Coverage Completeness Gate, the Pass Protocol. It overrides any drifted guidance below.
- Process: `.claude/skills/onboard-fusion-component/SKILL.md`
- Done-criteria / 7 gates: `.../onboard-fusion-component/references/automation-gates.md`
- BDD rules: `.claude/memory/bdd-principles.md`
- Typed-DSL enforcement: `Alis.Reactive.Analyzers/TypedDsl/UntypedComponentApiAnalyzer.cs` (ALIS009), `Alis.Reactive/Components/TypedDslExemptionAttribute.cs`
- Context + owed work: `docs/superpowers/plans/typed-dsl-and-deterministic-onboarding-2026-06-13.md`

## Done means
A single command drives a chosen component through the exit conditions and fails
loud on any gap; the verifier is wired blocking into the full gate; proven
end-to-end on one real stateful component (FusionSchedule event-CRUD, task #9) to
a green, blind-reviewed, artifact-complete close.
