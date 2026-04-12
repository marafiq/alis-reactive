# Quality Status — April 2026

Single source of truth for quality work. Updated after each quality session.

## Done (since March 28 audit)

- Vendor isolation — resolver.ts centralizes all vendor dispatch
- Docs-site drift — PR #94 (IReactivePlan fixed, new pages, navigation)
- XML docs on builders — PRs #95, #97 (Builder APIs, PlanModel contract types)
- Design system cleanup — PR #96 (unused builders removed, enums moved, TagMode fix)
- Schema drift detection — `tests/Alis.Reactive.DriftDetection.Tests/`
- Plugin Source vertical slice — 4 source kinds (component, payload, URL, plugin)
- HTTP Headers, URL Templates, URL Query Source — all landed
- Skill review — 7/8 reviewed (reactive-dsl, http-pipeline, conditions-dsl,
  onboard-fusion, validation-rules, bdd-testing, solid-ts-audit). 6 had fixes applied.
- GitHub issue triage — 4 stale issues closed (#36, #44, #46, #50)

## Open — Code

Tracked as GitHub issues or in `docs/issues.md`:

| Item | Location | GH Issue |
|------|----------|----------|
| SonarQube CRITICALs: conditions.ts + rule-engine.ts | conditions.ts, rule-engine.ts | #54 |
| Invalid trace level enables all logging | root.ts:29 | #73 |
| HTTP Finally stage (prevents permanent spinners) | Feature request | #88 |
| FlushSegment command-bundling across condition boundaries | PipelineBuilder.cs:184 | #90 |
| Type assertions (4 occurrences) | Various TS files | #43 |
| replaceAll + export-from patterns | Various TS files | #45 |
| Number.parseInt + empty exports | Various TS files | #47 |
| ElementBuilder.Show()/Hide() return type | ElementBuilder.cs:161,171 | issues.md #1 |
| itemShape no fail-fast on non-array | conditions.ts:122-127 | issues.md #2 |
| Unknown vendor error missing componentId | resolver.ts:72 | issues.md #3 |
| ForTests functions in production | boot.ts, live-clear.ts, native-action-link.ts | — |
| PlanRegistry over-exported | merge-plan.ts:18 | — |

## Open — Documentation

- CS1591 XML docs: ~758 warnings across 81 files (needs recount after recent PRs)
  - Tracked per module in `docs/cs1591-xml-docs-remaining.md`
- Dev experience gaps: 6 items from review
  - Details in `docs/reviews/dev-experience-review.md`

## Open — Skills & Process

- modern-csharp skill rewrite (1,272 lines, promotes C# 12+ but repo uses C# 8.0)
- A/B testing incomplete for all 8 skills

## Deferred

- Validation roadmap: date range, SF Grid inline edit, server-side filtering
  - Details in `.claude/memory/validation-roadmap.md`
- SonarQube ~67 pre-existing MAJOR+MINOR smells (untriaged)

## References

- Forensic master index: `.claude/memory/forensic-master-index.md`
- Dev experience review: `docs/reviews/dev-experience-review.md`
- Validation roadmap: `.claude/memory/validation-roadmap.md`
- Known issues: `docs/issues.md`
