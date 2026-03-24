# Accuracy review — plan & issue docs (multi-reviewer run)

**Date:** 2025-03-24  
**Method:** Three parallel reviewers with scoped inputs; outputs merged below.

## Reviewer inputs (shared)

| Reviewer | Scope | Code/schema cross-check |
|----------|--------|-------------------------|
| **R1** | INVEST-rubric, issue-A/B/C | `Command.cs`, `RequestDescriptor.cs`, `ValidationResolver.cs`, `PipelineBuilder.cs` |
| **R2** | README master, issue-D/E/F | `reactive-plan.schema.json`, `Scripts/types/http.ts`, `Entry.cs`, `TriggerBuilder.cs`, `BuildReaction` |
| **R3** | `descriptor-design-target-state.md`, `descriptor-solid-analysis-plan.md` intro | Full schema `oneOf` spot-check, link resolution from `docs/architecture-review/` |

## Consolidated outputs

### Critical (addressed in repo or below)

1. **Broken markdown links:** Issue files used `Alis.Reactive/...` without `../../../`, which does not resolve from `descriptor-target-state-planning/`. **Fix:** prefix `../../../Alis.Reactive/...` and `../../../Alis.Reactive.SandboxApp/...` where applicable.
2. **Issue C framing:** `ValidatorType` is already `[JsonIgnore]` — not on wire today. Primary Issue C problem is **in-place mutation** during `ValidationResolver` + `StampPlanId`, not wire leakage.
3. **Issue D framing:** C# `StatusHandler` uses **constructors** that enforce commands **or** reaction; gap is **schema XOR vs TS optional fields** / discriminated union modeling — not “compiler allows anything.”
4. **Issue A scope vs analysis plan:** Analysis **IssueA** text groups `Command` + `RequestDescriptor`; target-state splits **A** (commands) vs **C** (HTTP/resolve). **Mitigation:** one-line mapping in analysis plan intro.

### Minor (documented for follow-up)

- **B-T1 / ModeGate:** Diagram label aligned to **PipelineModeGate** (or both named consistently).
- **F3:** Code uses redundant ternary — both branches return `reactions[0]`; doc text clarifies **always** first reaction.
- **README vs issue-f:** README shows **prerequisites** (F1,F2→F3); issue-f shows **tier sequence** F1→F2→F3 — both valid; note added in README.
- **D-T3:** Labeled **post-change** where `kind` does not exist on TS `StatusHandler` today.
- **Target-state / root links:** Same `../../Alis.Reactive/...` convention recommended for `descriptor-design-target-state.md` (large file — batch fix optional).

### Verified accurate (sample)

- `GuardWith` + double-guard throw matches `Command.cs`.
- `ResolveRequest` → `EnrichValidation` matches `ValidationResolver.cs`.
- Trigger 5 / Reaction 4 / Command 5 / Guard 5 / Gather 4 match schema.
- `BuildReaction` footgun and `AddEntryWithContexts` foreach match source.
- `AddCommand` public on `PipelineBuilder` confirmed.

## Consolidated verdict (post-review)

**Canonical summary:** [ISSUE-BY-ISSUE-VERDICT-2025-03-24.md](ISSUE-BY-ISSUE-VERDICT-2025-03-24.md) — priorities, **F3** vs production path, **C** nine-param ctor, **D** serialization nuance, **E** independence from **B**, encapsulation re-counts, recommended execution order.

## Sign-off

**Follow-up edits applied (same session):** Relative links `../../../Alis.Reactive/...` in issue A–F files; Issue **C** and **D** problem statements aligned to code (`JsonIgnore` on `ValidatorType`; StatusHandler ctor vs TS XOR); **B** diagram `PipelineModeGate`; **F3** redundant-ternary note; **README** `HttpReaction` class + F-tier note; **analysis plan** issue-mapping paragraph.

**2025-03-24 (second pass):** Verdict doc + per-issue highlights (F3 P0, **D** not “zero new” serialization, **C** **9** params, **E** shippable without **B**); [descriptor-encapsulation-review.md](../descriptor-encapsulation-review.md) accuracy note for class counts.

Re-run multi-reviewer pass after major doc churn.
