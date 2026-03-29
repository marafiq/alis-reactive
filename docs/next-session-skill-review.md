# Session: Skill Review — 4 Remaining Skills

Branch: create `harness/skill-review-round-2` from `refactor/api-surface-xml-docs`

## Goal

Review, A/B test, and fix the 4 remaining project skills to the same standard as the first batch (46 fixes, ~9/10 scores).

## Skills to Review (priority order)

1. **reactive-dsl** (275 lines) — core DSL for all views, highest usage
2. **http-pipeline** (235 lines) — all data flow, FromEvent vs Include confusion documented
3. **conditions-dsl** (245 lines) — ResponseBody phantom confusion, guard composition
4. **modern-csharp** (1,272 lines) — far over 500-line limit, needs condensing/split

## Process Per Skill (proven in previous session)

### Round 1: Code-Verified Review
Dispatch agent: read skill, verify every claim against actual code (file paths, method signatures, builder patterns, code examples). Report findings ranked by impact with file:line evidence.

### Round 2: A/B Experiment
Dispatch "Claude B" agent: given ONLY the skill, follow it step-by-step on a real task. Document every gap where the skill was wrong, unclear, or insufficient.

Real tasks per skill:
- **reactive-dsl**: Write a view with DomReady + CustomEvent + InputField + Dispatch + RenderPlan
- **http-pipeline**: Write a form that does POST with Gather (Include + FromEvent), chained response, WhileLoading
- **conditions-dsl**: Write conditional show/hide with component source, cross-property, guard composition
- **modern-csharp**: Write a sealed descriptor class with value object invariants, pattern matching, records

### Round 3: Fix + Top-10 Audit
Apply fixes from both rounds. Run `verify-skill-top-10-things` on each. Fix description (third-person), add verification steps if missing, save A/B test log as `references/ab-test-log.md`.

### Round 4: Persist A/B Logs (closes Rule 10 for ALL 8 skills)
Also create `references/ab-test-log.md` for the 4 skills already reviewed (onboard-fusion, validation-rules, bdd-testing, solid-ts-audit) using the experiment results from the previous session.

## Context

- Previous session reviewed 4 skills: 46 fixes, avg ~9/10. Common gap: Rule 10 (no persisted A/B log)
- Agent dispatch template: `.claude/rules/agent-dispatch.md` — use Review Agent template for Round 1
- Session todo: `.claude/memory/session_2026_03_28_todo.md` — Task 4 partial status
- Every reviewed skill had 6-13 errors. Expect similar for these 4.
