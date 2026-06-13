# Typed DSL Enforcement + Deterministic Onboarding — 2026-06-13

The framework's vision: developers author Syncfusion/Native UI through a fully
**typed** C# DSL — never raw JS, never untyped escape hatches in `.cshtml`.
Onboarding (turning a vendor component's JS API into a typed Fusion slice with
~95% parity) is the **final piece**: if it doesn't land, devs fall back to JS
escape hatches in views, which is anti-vision and would sink the framework.

## Landed this session

- **ALIS009 typed-DSL analyzer** (`Alis.Reactive.Analyzers/TypedDsl/UntypedComponentApiAnalyzer.cs`):
  blocks a public method in a Fusion/Native component slice from exposing a bare
  `object`/`object[]`, a member/method/action selector `string`, or a plan wire
  type. Symbol-level (accurate; multi-line params can't hide; `Expression<Func<T,object?>>`
  is correctly NOT flagged). Wired as a blocking `Analyzer` reference on
  Alis.Reactive.Fusion + Alis.Reactive.Native — defects fail the build there.
  This is the structural enforcement of "typed-only" that C# has no native
  feature for. Commit `53944403`.
- **Exemption mechanism** (`Alis.Reactive/Components/TypedDslExemptionAttribute.cs`):
  `[TypedDslExemption(reason)]` — a small, greppable, reason-bearing escape set
  for legit cases (vendor MVC-builder slots typed as object). Never silent.
- **Audit result** (the deterministic sweep over all slices): exactly ONE finding —
  `FusionGridFieldValidation.Field` returns `object?` to feed EJ2
  `GridColumn.ValidationRules` (a vendor builder slot typed object). Native clean.
  Ruled legit builder-boundary, exempted with owed refinements.
- **FusionSchedule event-CRUD typed** (commit `e6a5e17d`): AddEvent/SaveEvent/
  DeleteEvent/OpenEditor now take `ResponseBody<TResponse>` + path (mirrors
  SetDataSource) + a `ScheduleAction` enum (mirrors EJ2 `CurrentAction`),
  replacing raw `ValueExpression` + `string action`. ValueExpression stays internal.

## FIGURE OUT (owner flagged "a bit behind on the reasons")

- **Phantom types vs `object` for vendor-builder bridges.** `FusionGridValidation.Field`
  returns `object?` only because EJ2 `GridColumn.ValidationRules` is `object`. A
  typed phantom/marker (`FusionGridColumnValidationRules`, still an `object`, still
  assignable to the vendor slot) would make the public API typed. Question to
  settle: should every vendor-`object`-slot bridge return a phantom type instead
  of `object`? Articulate WHY `object` in a typed API is bad (parity, refactor
  safety, discoverability) so the rule is principled, not cargo-culted.

## OWED (not done — compile/typed is NOT done; 100% Playwright is the bar)

- **FusionSchedule event-CRUD onboarding**: the 4 typed methods are unproven —
  zero onboarding artifacts (verify-fusion-artifact-gates.mjs fails), no sandbox
  CRUD view, no Playwright. Needs HTTP/SQLite-backed sandbox + Playwright proving
  each method's real EJ2 behavior (browser is truth; it validates/falsifies the
  ResponseBody-bound design). Task #9.
- **Inline-grid bulk-edit validation**: `FusionGridValidation.Field` (and the EJ2
  column validationRules path) is never exercised by Playwright — a real coverage
  gap that let the untyped surface hide. Add the validator-in-inline-grid slice.
- **FusionGridValidation phantom-type refinement** (after the figure-out above).

## NEXT SESSION — the high-priority missing piece

**Deterministic, automated onboarding / audit / upgrade of Syncfusion components
into typed APIs (~95% JS parity).** Owner has asked for this for weeks; it has
not landed because it's been a manual, LLM-followed skill (error-prone — this
session shows even careful work stops at "compile", parks things wrongly, skips
the artifact chain) rather than deterministic tooling. The skill is written; the
verifier (`verify-fusion-artifact-gates.mjs`) WORKS; the probe/trace/HTTP-SQLite
chain is partially aspirational. To make it land, lean on structural enforcement,
not discipline:
1. ALIS009 (done) — untyped public API can't compile.
2. Make the artifact-gate verifier a BLOCKING gate (a component with public typed
   API but missing artifacts/coverage fails the build), so onboarding can't be
   skipped — FusionSchedule shipped with zero artifacts because nothing blocked it.
3. Deterministic parity: extract the EJ2 JS surface (d.ts) + the implemented C#
   typed surface (Roslyn) → compute coverage %; below-bar or un-onboarded public
   JS members = unaudited = fail.
4. Complete the probe/trace tooling (behavioral proof side).

## Recurring failure to stop (mine, this session; and systemic)
Stopping at "it compiles / it's committed." The bar is **100% Playwright coverage
of every onboarded member** — project Coverage Completeness Gate + onboarding
skill + owner, demanded repeatedly. Compile proves shape; only the browser proves
the typed API drives the vendor behavior.
