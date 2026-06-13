# GOAL — Deterministic Onboarding Automation

Build the final piece: a deterministic pipeline to onboard/audit/upgrade
Syncfusion (and Native) components into a **fully typed** C# DSL at **~95% JS
parity**. If it doesn't land, devs fall back to JS escape hatches in `.cshtml` —
anti-vision. It hasn't landed because it's been manual discipline, not
structural enforcement (agents stop at "compile", skip artifacts — FusionSchedule
shipped untyped with zero artifacts because nothing blocked it).

Make the BUILD the gate, not the agent.

## The automation must
1. Extract vendor JS surface (`d.ts`/`.js`) + implemented C# surface (Roslyn) →
   compute **parity**; un-onboarded public members below bar = FAIL.
2. Make `verify-fusion-artifact-gates.mjs` a **blocking** gate in `scripts/test.sh`.
3. Refuse "done" unless every exit condition below holds, failing loud on any gap.

## EXIT CONDITIONS (binary — a component is onboarded only when ALL hold)
1. Source-grounded probe + browser trace committed — `onboard-fusion-component/SKILL.md` workflow.
2. **Typed only**: ALIS009 clean (no `object`/selector-string/wire type in public slice API). Exceptions only via `[TypedDslExemption(reason)]`. Plugin name is the lone string, at the plugin boundary.
3. **Parity met** (≥95%): every public vendor member onboarded-typed | builder-owned | excluded-with-evidence. Generated, not asserted.
4. **100% behavior coverage**: `proof/typed-api-coverage-matrix.md` — one row per public member, each with a passing Playwright behavior; zero unproven rows. (Root `CLAUDE.md` → Coverage Completeness Gate.)
5. **7-behavior contract** + **stateful HTTP/SQLite CRUD proof** where applicable — `automation-gates.md` Gates 5–7 (reload-preserves-state mandatory for grids/schedulers/lists).
6. **BDD-valid + blind-reviewed** — every test obeys `.claude/memory/bdd-principles.md` (5 Rules, framework-primitives-only, nested vertical slice, No-Hack rules); a blind reviewer agent passes them.
7. **Artifacts complete**: `verify-fusion-artifact-gates.mjs --component {x}` passes.
8. **Full gate green** on rebuilt assets: typecheck (plan.ts no drift) → vitest → `dotnet build` both TFMs (ALIS009 active) → `scripts/playwright.sh`. Observed, not inferred.

## Non-negotiable (REJECT — this is "half work")
Untyped public API (ALIS009) · compiles-but-no-sandbox-usage · one assertion ≠ 100% ·
static-array example for a data component · any missing artifact · a test that passes
when the feature is broken · `page.evaluate()`/mocking/`Thread.Sleep`/`[Retry]`/weak asserts.
Full reject table: `automation-gates.md` → "Done Means Done".

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
