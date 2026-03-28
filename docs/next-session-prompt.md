# Next Session Prompt

Branch: `refactor/api-surface-xml-docs`. Run `git log --oneline -15` to see recent work.

## PRIMARY TASK: Build `.claude/process-flows.md`

CLAUDE.md now has a simple 4-line ASCII flow linking to `.claude/process-flows.md` (not yet created).
This file needs detailed per-task-type flows. Take your time. Fire multiple agents.

### Approach
1. Fire **Claude Code workflow experts** — research Anthropic's latest on workflows, hooks, skills orchestration
2. Fire **code analysts** — read actual code, past mistakes (`.claude/memory/feedback_*.md`), sandbox views
3. Fire **process designers** — design the per-task-type flows with pre/post checklists

### Task Types to Cover

| Task Type | Key Skills | Nuances |
|-----------|-----------|---------|
| Writing a .cshtml view | reactive-dsl, http-pipeline, conditions-dsl, validation-rules | InputField + component selection, browser verify |
| Onboarding a new component | onboard-fusion-component | 8-file vertical slice, zero TS changes |
| Fixing a bug | — | Root cause analysis, trace full path, browser first, NEVER patch |
| Adding a new primitive | — | 10-step checklist, schema alignment |
| Refactoring | solid-ts-audit | SOLID check, code smells, coupling analysis |
| Reviewing (code review) | — | Evidence-based, 9-point audit criteria |
| SOLID enforcement | solid-ts-audit | SRP/OCP/LSP/ISP/DIP per module |
| SonarQube cleanup | — | Quality gate, BDD coverage 80% on touched code |
| Rider warnings | — | CS1591, nullable, unused, etc. |
| Writing docs | technical-documentation-writing | Sandbox-verified examples, question-driven style |
| Writing tests | bdd-testing | Public API only, full user journeys, no internals |

### For Each Task Type, Define:
1. **Pre-flight checklist** — what to verify before starting
2. **Skills to load** — which skills this task needs
3. **Process steps** — the exact flow
4. **Post-flight checklist** — what to verify before marking done
5. **Common mistakes** — from feedback memories, specific to this task type
6. **Agent context template** — what to include when dispatching an agent for this task

### Read Before Starting
- `.claude/memory/MEMORY.md` — session memory index
- `.claude/memory/solid-ts-research.md` — SOLID research (Uncle Bob, Fowler, TanStack, Beck)
- `.claude/memory/feedback_validation_session_mistakes.md` — biggest session failure
- `.claude/memory/feedback_no_tech_debt.md` — no shortcuts
- `.claude/memory/feedback_rubber_stamping.md` — no lazy audits
- `docs/todo-skill-updates.md` — skill/hook todo list
- `docs/reviews/anthropic-claude-md-research.md` — Anthropic best practices
- Current CLAUDE.md — the process section and rules

### Other Pending Tasks (lower priority)
- Fix `reactive-dsl` skill (WIP — expand scope, verify examples)
- Fix `onboard-fusion-component` skill (6 errors)
- Fix `validation-rules-alis-reactive` skill (5 gaps)
- Build schema drift hook
- Create `design-system` and `technical-documentation-writing` skills
- CS1591 XML doc warnings (758 across 81 files)
- CLAUDE.md positive language rewrite for rules
- Audit all skill YAML descriptions via `/verify-skill-top-10-things`
