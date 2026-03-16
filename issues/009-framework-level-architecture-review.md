# Framework-Level Architecture Review

## Scope

This pass reviews the current framework architecture across C#, runtime, components, MVC seams, and proof quality.

Excluded from scope:

- `tools/`

Hard rules used for this review:

- the public DSL is frozen
- no feature reduction
- fixes should improve contract truthfulness, not shift burden back to app authors

## Verdict

The framework is directionally strong. Identified gaps were retired as not worth pursuing (partials, planId, lifecycle, proof depth).

## Current Framework-Level Issues

None. All identified issues retired (see below).

## Retired

**Plan construction / Render finalization** — With partials, `ResolveAll()` at render time is inherent. Partials are built in isolation and merged later; only at `Render()` do we have full context (merged entries, ComponentsMap, extractor). No alternative design.

**Logical plan identity** — `typeof(TModel).FullName` as planId is sufficient. Collision with full namespace is hard in practice; extra complexity not worth it.

**`WhileLoading` lifecycle** — Up to dev to remove the effects in success/error branches. Agree there should be a Finally state for each request unit (like parallel has AllSettled) for deterministic cleanup, but not worth pursuing as an active issue.

**Architecture page slice fidelity** — Page should use real component builders instead of raw `AddToComponentsMap`/`Entry`. Valid but won't pursue.

**Architecture test depth** — Tests should assert exact payloads, plan shape, contract correctness. Valid but won't pursue. LLM-assisted development doesn't reliably deliver deep contract tests when asked.

## Short Summary

The framework is not in bad shape.

The core loop is still coherent:

- C# builds a plan
- plan serializes to JSON
- runtime executes the plan

No active framework-level gaps.

## Most Important Next Moves

None.
