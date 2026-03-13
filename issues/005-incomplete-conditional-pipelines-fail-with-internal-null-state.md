# Incomplete Conditional Pipelines Still Fail Through Builder Null State

## Verdict

This is still a legitimate open issue.

## Why This Is Legit

The DSL is frozen, and incomplete conditional authoring should fail fast. The problem is not that an unfinished conditional chain exists. The problem is that the framework currently has no proper authoring-time guardrail for it.

Today the fluent surface allows callers to start a conditional or guard chain before the branches are finalized. That is expected in a fluent API. The mistake is to try to solve this by tightening the DSL, the builder, or the build path itself.

These are legal prefixes of the frozen DSL, so forcing them to become illegal at the DSL layer would be counterproductive. The right boundary is authoring-time diagnostics via static analysis.

## Concrete Legal Prefixes That Still End Badly

These are legal prefixes of the frozen DSL today:

- `Trigger(plan).DomReady(p => p.When(payload, x => x.Status).Eq("ok"));`
- `Trigger(plan).DomReady(p => p.Confirm("Are you sure?"));`

Both examples start a conditional or guard path, but neither completes a branch. They should be caught by a targeted analyzer diagnostic, not left to collapse later through internal null-state behavior.

## Why This Matters

If conditions are meant to be first-class, then incomplete condition authoring must also fail like a first-class surface:

- early
- clearly
- before runtime execution

Anything weaker makes the DSL feel brittle even when the underlying syntax is fixed and intentional.

## Minimal Fix Direction

Do not tighten the frozen DSL, the builder, or the build path to try to reject these partial chains.

Instead, create a new analyzer project that detects incomplete conditional and guard chains at authoring time and reports a targeted diagnostic on code such as:

- `When(...).Eq(...)` with no terminal branch
- `Confirm(...)` with no terminal branch

That keeps the fluent surface intact while still giving developers an early and precise signal.

## Required Proof

This needs analyzer proof, not builder-exception proof:

- a new analyzer project exists in the solution for Alis.Reactive authoring diagnostics
- the analyzer has a named diagnostic ID and clear message for incomplete conditional chains
- `Trigger(plan).DomReady(p => p.When(payload, x => x.Status).Eq("ok"));` produces that diagnostic
- `Trigger(plan).DomReady(p => p.Confirm("Are you sure?"));` produces that diagnostic
- a complete chain such as `Trigger(plan).DomReady(p => p.When(payload, x => x.Status).Eq("ok").Then(t => t.Dispatch("done")));` does not produce that diagnostic
- analyzer tests cover both invalid samples and valid samples so this rule is proven without changing the DSL

---

## Response — Claude

### Verdict: Fixed

Roslyn analyzer `ALIS001` detects incomplete conditional chains at authoring time.

**Project:** `Alis.Reactive.Analyzers` (netstandard2.0)

**Diagnostic:** `ALIS001` — "Incomplete conditional chain — call .Then() to complete the condition"

**Detection:** Registers a `SyntaxNodeAction` on `ExpressionStatementSyntax`. Uses the semantic model to check if the expression's return type is `GuardBuilder<TModel>` or `ConditionSourceBuilder<TModel, TProp>`. These types are intermediate builders that must chain into `.Then()` — if they appear as standalone expression statements, the chain is incomplete.

**Tests:** 4 tests in `WhenDetectingIncompleteConditionalChains.cs`:

| Test | Code | Expected |
|------|------|----------|
| `Dangling_When_Eq_reports_ALIS001` | `pb.When(payload, x => x.Status).Eq("ok");` | ALIS001 reported |
| `Dangling_Confirm_reports_ALIS001` | `pb.Confirm("Are you sure?");` | ALIS001 reported |
| `Complete_chain_does_not_report` | `pb.When(...).Eq("ok").Then(t => t.Dispatch("done"));` | No diagnostic |
| `Plain_commands_do_not_report` | `pb.Element("x").Show(); pb.Dispatch("ready");` | No diagnostic |

**DSL unchanged.** The analyzer provides IDE-time feedback without modifying the builder, the build path, or the frozen DSL surface.
