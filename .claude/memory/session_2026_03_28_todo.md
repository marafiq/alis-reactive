---
name: session_2026_03_28_todo
description: Persistent todo list — updated 2026-03-28 evening session. 6 done, 1 partial, 5 open.
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
- Archive preserves historical plans, reviews, and specs

## Partially Done

**Task 4. Review all skills** — 4 of 8 reviewed
- [x] onboard-fusion-component: 12 fixes (6 review + 6 A/B). FusionComponent base, ComponentRegistration, sandbox paths, references/ extracted. Score: ~9/10
- [x] validation-rules: 10 fixes (6 review + 4 A/B). DateTime.Today warning, nullable gt, case sensitivity, verification steps. Score: ~9/10
- [x] bdd-testing: 13 fixes (8 review + 5 A/B). ComponentScope, 13 components, SelectDate guidance, popup surfaces, journey granularity. Score: ~9/10
- [x] solid-ts-audit: 11 fixes (7 review + 4 A/B). Module map, dispatcher exception, sync/async coupling, non-null assertion. Score: ~9/10
- [ ] reactive-dsl — NOT REVIEWED
- [ ] http-pipeline — NOT REVIEWED
- [ ] conditions-dsl — NOT REVIEWED
- [ ] modern-csharp — NOT REVIEWED (1,272 lines, needs condensing)

Common remaining gap: Rule 10 (A/B test log not persisted as references/ file)

## Open — Future Sessions

**Task 7. docs-site drift** — needs own session + plan
- 5 pages reference deleted IReactivePlan
- 3 pages wrong API name
- Test counts 30-54% stale
- Scope: content accuracy + code examples + sandbox verification

**Task 8. SonarQube CRITICALs** — code change (Layer 3)
- 3 complexity hotspots: conditions.ts 24, commands.ts 17, rule-engine.ts 26
- Plan: extraction refactors to get under complexity 15

**Task 9. Vendor isolation leaks** — code change (Layer 3)
- trigger.ts:45, live-clear.ts:44 still have vendor string checks
- Plan: move vendor logic into component.ts exports

**Task 10. Schema drift detection tool** — code change (Layer 1→2)
- Automated C# descriptor → schema validation
- No approach decided yet

**Task 11. TS-to-schema validation** — code change (Layer 2→3)
- Automated schema → TS type conformance
- No approach decided yet
