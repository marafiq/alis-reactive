---
name: session_next_todo
description: Handoff from 2026-03-28 late session. 3 isolated branches need rebase onto refactor/api-surface-xml-docs then merge. All based on main (51 commits behind).
type: project
---

# Session Handoff — 2026-03-28 Late Session

## What Happened

4 parallel agents dispatched in worktrees. All branched from `main` (4dec2c8)
instead of `refactor/api-surface-xml-docs` (424bdeb, 51 commits ahead).
IReactivePlan was deleted in those 51 commits. Each branch needs rebase before merge.

Current branch (`refactor/api-surface-xml-docs`) is pushed and clean.
1793 tests pass on current branch. Skill files edited directly (not in git).

---

## Branch A: "Docs-Site Drift" — `fix/docs-site-drift`

**Name to remember:** Docs-Site Drift
**Worktree:** `.claude/worktrees/agent-a66dcc24`
**Base:** `main` (4dec2c8) — 51 commits behind
**Commits:** 2
**Files:** 7 (all in `docs-site/src/content/docs/`)

### What Was Done
- [x] Test counts updated: C# 510→605, TS 944→1126, Playwright 483→742, Total→2400+
- [x] `NativeHiddenField()` → `HiddenFieldFor()` factory method name fixed (3 files)
- [x] Runtime size "2kb" → "88KB minified"
- [x] SF component count 15→18
- [x] Agent error caught: removed Fusion prefix from 4 files — REVERTED in 2nd commit

### What Was NOT Done
- [ ] IReactivePlan references NOT fixed — agent said "still exists" (wrong — deleted on current branch)
- [ ] No browser verification
- [ ] No review agent dispatched
- [ ] Test counts not verified against current branch (may have grown in 51 commits)

### To Do Next Session
- [ ] Rebase onto `refactor/api-surface-xml-docs`
- [ ] Grep `docs-site/src/` for `IReactivePlan` — fix ALL references
- [ ] Verify test counts match current branch
- [ ] `cd docs-site && npm run build` — must pass
- [ ] Review diff, merge or PR

---

## Branch B: "TS Runtime Quality" — `fix/ts-runtime-quality`

**Name to remember:** TS Runtime Quality
**Worktree:** `.claude/worktrees/agent-a8d61b9e`
**Base:** `main` (4dec2c8) — 51 commits behind
**Commits:** 1
**Files:** 6 (all in `Scripts/`)

### What Was Done
- [x] `conditions.ts` — complexity 24→~6 via `opHandlers` lookup map
- [x] `commands.ts` — complexity 17→~4 via `commandHandlers` dispatch map
- [x] `rule-engine.ts` — complexity 26→~5 via `ruleHandlers` lookup map
- [x] `trigger.ts` — vendor check moved to `component.ts` (`buildComponentEventDetail()`)
- [x] `live-clear.ts` — vendor check moved to `component.ts` (`wireLiveValidationEvents()`)
- [x] `component.ts` — 2 new exports added

### What Was NOT Done
- [ ] Tests pass on `main` (agent reported 1126/1126) — NOT verified on current branch
- [ ] No browser verification — conditions, commands, validation not manually tested
- [ ] No review agent dispatched
- [ ] SonarQube not run to confirm complexity reduction
- [ ] No PR created

### To Do Next Session
- [ ] Rebase onto `refactor/api-surface-xml-docs`
- [ ] `npm test` — all vitest must pass
- [ ] `npm run typecheck` — zero errors
- [ ] Start SandboxApp, verify in browser: conditions page, form submission, validation
- [ ] Dispatch review agent on diff
- [ ] Run SonarQube (`./scripts/sonar-analyze.sh`) to verify complexity metrics
- [ ] Create PR against `refactor/api-surface-xml-docs`
- [ ] Run Playwright tests

---

## Branch C: "Drift Detection" — `feature/drift-detection`

**Name to remember:** Drift Detection
**Worktree:** `.claude/worktrees/agent-acea39de`
**Base:** `main` (4dec2c8) — 51 commits behind
**Commits:** 1
**Files:** 3 (1 plan doc + 1 C# test + 1 TS test) — 1,792 new lines

### What Was Done
- [x] Plan document: `docs/plans/drift-detection-plan.md`
- [x] C#→Schema detection: `tests/.../Schema/WhenDetectingSchemaCompleteness.cs` (34 tests)
  - Serializes every descriptor type with all properties populated
  - Validates against `reactive-plan.schema.json`
  - Includes proof test (adds fake property, confirms rejection)
- [x] Schema→TS detection: `Scripts/__tests__/when-detecting-schema-ts-drift.test.ts` (44 tests)
  - Enum conformance, discriminated union conformance, property completeness
  - Reads actual schema file, checks TS types match

### What Was NOT Done
- [ ] Tests pass on `main` (agent reported 384 C# + 1170 TS) — NOT verified on current branch
- [ ] C# tests reference descriptor types that may have changed in 51 commits — likely compile errors
- [ ] TS tests check schema properties — schema may have changed
- [ ] No review agent dispatched
- [ ] Browser verification N/A (test infrastructure)

### To Do Next Session
- [ ] Rebase onto `refactor/api-surface-xml-docs`
- [ ] Fix compilation errors (descriptor type changes, IReactivePlan removal)
- [ ] Update tests if descriptor/schema shapes changed
- [ ] `dotnet test tests/Alis.Reactive.UnitTests` — must pass
- [ ] `npm test` — must pass
- [ ] Review plan document accuracy on current branch
- [ ] Create PR

---

## Branch D: "Skill Review" — `harness/skill-review-round-2`

**Name to remember:** Skill Review (empty)
**Worktree:** none
**Base:** `main` (4dec2c8)
**Commits:** 0 — empty branch, agent was blocked on permissions

### What Was Done (outside this branch)
- [x] Skill files edited directly at `~/.claude/skills/` (NOT in git):
  - `reactive-dsl/SKILL.md` — 7 fixes (ServerPush/SignalR triggers, TPayload `new()` constraint, namespace fix, FusionColorPicker, cross-model Component)
  - `http-pipeline/SKILL.md` — 1 fix (AsJson documentation)
- [x] All fixes verified against actual source code with file:line evidence
- [x] 2 false positives from review agent rejected (FusionColorPicker initial miss, TResponse already documented)

### What Was NOT Done
- [ ] A/B testing — not completed for any of 8 skills
- [ ] Top-10 skill audits — not run
- [ ] `conditions-dsl` — reviewed (2 minor findings), no changes made
- [ ] `modern-csharp` — NOT reviewed. 1,272 lines, promotes C# 12+ but repo uses C# 8.0. Needs complete rewrite.
- [ ] A/B test logs (`references/ab-test-log.md`) not created for any skill

### To Do Next Session
- [ ] Delete this empty branch: `git branch -d harness/skill-review-round-2`
- [ ] `modern-csharp` rewrite — dedicated session (1,272 → ~250 lines)
- [ ] A/B testing for all 8 skills
- [ ] Top-10 audits for all 8 skills

---

## Worktree Cleanup (after merges)

After each branch is merged or abandoned:
```bash
git worktree remove .claude/worktrees/agent-a66dcc24   # Docs-Site Drift
git worktree remove .claude/worktrees/agent-a8d61b9e   # TS Runtime Quality
git worktree remove .claude/worktrees/agent-acea39de   # Drift Detection
git branch -d worktree-agent-a66dcc24
git branch -d worktree-agent-a8d61b9e
git branch -d worktree-agent-acea39de
git branch -d harness/skill-review-round-2
```

---

## Recommended Merge Order

1. **Branch B (TS Runtime Quality)** — standalone TS refactors, least likely to conflict
2. **Branch A (Docs-Site Drift)** — content fixes, need IReactivePlan refs fixed first
3. **Branch C (Drift Detection)** — most likely to need adaptation after rebase

---

## Key Lesson

`isolation: "worktree"` branches from `main`, not current feature branch.
Future sessions: verify agent base commit, or avoid worktree isolation for feature branch work.
