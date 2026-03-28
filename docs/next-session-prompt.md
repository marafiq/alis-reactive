# Next Session Prompt

Branch: `refactor/api-surface-xml-docs`. Run `git status` to see uncommitted work from last session.

## Context

Last session (2026-03-28) built the layered harness system:
- `.claude/rules/process-pipeline.md`, `process-layers.md`, `process-task-types.md` — auto-loaded every session
- `CLAUDE.md` tightened to ~160 lines
- Forensic analysis of 753 commits → 32 mistake patterns (`.claude/memory/forensic-master-index.md`)
- 5 feedback memories + session reflection saved

**Read first:**
- `.claude/memory/session_2026_03_28_todo.md` — 11 open tasks, each plan-first
- `.claude/memory/session_2026_03_28_reflection.md` — what went right/wrong, all feedback actioned

## Step 1: Commit Last Session's Work

Uncommitted changes:
- Modified: `CLAUDE.md`, `.claude/memory/MEMORY.md`
- New: `.claude/rules/process-pipeline.md`, `process-layers.md`, `process-task-types.md`
- New: 7 memory files (forensic index, 5 feedback, todo, reflection)

Review the diff, then one clean commit.

## Step 2: Review Priority Order With User

Present the 11 open tasks from `session_2026_03_28_todo.md` and review priority with user
BEFORE starting any work. Focus is **process and harness** — no code changes this phase.

### Process & Harness Tasks (the focus)

1. **API Surface hook** — enhance to catch `internal` → `public` across 3 library projects
2. **BDD test enforcement** — skill in agent prompt + post-hook for test patterns
3. **BDD public API only** — analyzer or hook to catch internal constructor usage (53 violations)
4. **Review all skills** — accuracy + effectiveness testing (6 errors in onboard-fusion, 5 gaps in validation-rules)
5. **Claude optimization findings** — reduce emphasis, add "why", questions → commands in rules files

### Docs Cleanup (still process, not code)

6. **docs/ folder** — 14 delete, 28 archive, 16+ stale refs
7. **docs-site drift** — 5 pages reference deleted IReactivePlan

## Process Reminder

The layered harness is in `.claude/rules/` (auto-loaded). Follow it:
- Identify layers the task touches. Load skills first.
- A failing test drives every boundary crossing.
- Plan before execution. Get alignment before editing.
- If touching an unexpected layer → plan is wrong, stop and rethink.
- Save feedback and learnings immediately.
- Review agents get: positive framing, evidence-based I/O, rank findings by value.
