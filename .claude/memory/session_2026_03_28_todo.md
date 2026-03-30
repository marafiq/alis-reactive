---
name: session_2026_03_28_todo
description: Persistent todo list — updated 2026-03-28 late session. 8 done, 1 partial, 3 open.
type: project
---

# Persistent Todo List

## Completed — Session 2026-03-28 (morning)
- [x] Forensic git analysis (9 agents, 32 patterns, commit hashes)
- [x] process-flows v1 → v2 (flat checklists → layered harness)
- [x] CLAUDE.md tightened (323 → 160 lines)
- [x] Moved process-flows to `.claude/rules/` (3 files, auto-loaded every session)
- [x] Saved forensic master index + 5 feedback memories
- [x] Expert review (4 reviewers with evidence-based output)
- [x] Removed redundant rules (ESM, Snapshots, Two-Phase Boot, BDD — enforceable elsewhere)

## Completed — Session 2026-03-28 (evening)

**Task 0. Agent dispatch template** — `agent-dispatch.md` (192 lines)
- 4 task-specific templates (implementation, audit, review, BDD)
- 9-point evidence contract embedded, layer-skill lookup table
- Replaced fragmented sections in process-pipeline.md + process-task-types.md

**Task 1. API Surface hook** — `hookify.no-public-in-libraries.local.md`
- Blocks `public` declarations across all 4 library projects
- Covers Alis.Reactive, Native, Fusion, FluentValidator

**Task 2. BDD test enforcement** — `hookify.bdd-test-enforcement.local.md`
- Warns on page.evaluate, Thread.Sleep, Task.Delay, [Retry], try/catch assertions
- References BDD Constitution and bdd-testing skill

**Task 3. BDD public API only** — `hookify.bdd-public-api-only.local.md`
- Warns on internal constructor usage in test files
- 50 existing violations tracked for gradual migration (9 files in Native + Fusion)

**Task 5. Claude optimization findings**
- 9-point evidence contract: questions → imperative commands
- Added "why" to 10 critical rules in process-layers.md (with forensic commit evidence)
- Reduced ALL CAPS, added `<important>` tags for inviolable rules

**Task 6. docs/ folder cleanup**
- 78 → 24 active files (14 deleted, 40 archived to docs/archive/, -6,600 lines)

## Completed — Session 2026-03-28 (late)

**Task 7. docs-site drift** — branch `fix/docs-site-drift` in worktree agent-a66dcc24
- 10 files changed, 17 lines fixed
- Test counts updated: C# 510→605, TS 944→1126, Playwright 483→742, Total→2400+
- `NativeHiddenField()` → `HiddenFieldFor()` factory method fixed (3 files)
- Runtime size "2kb" → "88KB minified", SF count 15→18
- Agent incorrectly removed Fusion prefix from 4 files — reverted in follow-up commit
- IReactivePlan still exists (memory was stale — "deleted" was wrong)
- Build verified: 38 pages, zero errors

**Task 8+9. TS runtime quality** — branch `fix/ts-runtime-quality` in worktree agent-a8d61b9e
- SonarQube complexity: conditions.ts 24→~6, commands.ts 17→~4, rule-engine.ts 26→~5
- All 3 used handler lookup maps (opHandlers, commandHandlers, ruleHandlers)
- Vendor leaks: trigger.ts + live-clear.ts now call component.ts exports
  - New: `buildComponentEventDetail()`, `wireLiveValidationEvents()` in component.ts
- 1126/1126 vitest pass, zero new typecheck errors

## Partially Done

**Task 4. Review all skills** — 6 of 8 reviewed + fixes applied
- [x] onboard-fusion-component: 12 fixes. Score: ~9/10
- [x] validation-rules: 10 fixes. Score: ~9/10
- [x] bdd-testing: 13 fixes. Score: ~9/10
- [x] solid-ts-audit: 11 fixes. Score: ~9/10
- [x] reactive-dsl: 7 fixes (ServerPush/SignalR, TPayload constraint, fake namespace, FusionColorPicker, cross-model Component). Score: ~8.5/10
- [x] http-pipeline: 1 fix (AsJson()). Score: ~9.5/10
- [ ] conditions-dsl — reviewed, 2 minor findings, no changes needed. Score: ~9.5/10
- [ ] modern-csharp — NOT reviewed in depth. 1,272 lines, promotes C# 12+ but repo uses C# 8.0. Needs complete rewrite.

Remaining gaps:
- A/B testing not completed (agent blocked on permissions, sub-agents not dispatched)
- Top-10 audits not run
- A/B logs not created (for any of the 8 skills)
- modern-csharp needs dedicated condensing session

## Open — Future Sessions

**Task 10. Schema drift detection tool** — agent dispatched, awaiting results
- Automated C# descriptor → schema validation
- Branch: feature/drift-detection in worktree agent-acea39de

**Task 11. TS-to-schema validation** — same agent as Task 10
- Automated schema → TS type conformance

**modern-csharp skill rewrite** — dedicated session needed
- 1,272 lines → target ~250 lines
- Remove Span/Memory, async streaming, generic patterns
- Add framework-specific patterns: sealed descriptors, internal constructors, C# 8.0 constraint
- Align with CLAUDE.md "C# 8.0 enforced in library projects"
