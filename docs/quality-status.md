# Quality Status — April 2026

Single source of truth for quality work. Updated after each quality session.

## Done (since March 28 audit)

- Vendor isolation — `resolver.ts` centralizes vendor root resolution and event wiring
  (resolveVendorRoot, wireEvent). Note: `inject.ts` still has vendor-specific Syncfusion
  append logic, which is by design (injection is a separate concern from resolution).
- Docs-site drift — PR #94 (IReactivePlan replaced with ReactivePlan, new pages, navigation)
- XML docs on builders — PRs #95, #97 (Builder APIs, PlanModel contract types)
- Design system cleanup — PR #96 (unused builders removed, enums moved, TagMode fix)
- Schema drift detection infrastructure — `tests/Alis.Reactive.DriftDetection.Tests/` contains
  `DriftTestBase.cs` (AssertSchemaValid, AssertAllPropertiesPresent, AssertDefinitionPropertiesExactly)
  and `SchemaAnalyzer.cs`. **No executable tests yet** — test classes were in a lost worktree branch.
- Plugin Source vertical slice — 4 source kinds: `component`, `payload`, `url`, `plugin`
  (discriminated union at `Scripts/types/plan.ts` Source type)
- HTTP Headers, URL Templates, URL Query Source — all landed
- Skill review — 7/8 reviewed: reactive-dsl, http-pipeline, conditions-dsl,
  onboard-fusion-component, validation-rules, bdd-testing, solid-ts-audit.
  6 had fixes applied. modern-csharp not reviewed.
- GitHub issue triage — 4 stale issues closed (#36, #44, #46, #50)

## Open — Code

Tracked as GitHub issues or in `docs/issues.md`:

| Item | Location | GH Issue |
|------|----------|----------|
| SonarQube CRITICALs: evaluateCompare (19-case switch) + ruleFails (20-case switch) | `Alis.Reactive.SandboxApp/Scripts/conditions/conditions.ts`, `Scripts/validation/rule-engine.ts` | #54 |
| Invalid trace level enables all logging (unsafe `as TraceLevel` cast) | `Scripts/root.ts:29` | #73 |
| HTTP Finally stage (prevents permanent WhileLoading spinners) | Feature request | #88 |
| FlushSegment bundles commands across condition boundaries | `Alis.Reactive/Builders/PipelineBuilder.cs:184` | #90 |
| Drift detection test classes missing (infrastructure exists, tests lost) | `tests/Alis.Reactive.DriftDetection.Tests/` | — |
| ElementBuilder.Show()/Hide() return type | `Alis.Reactive/Builders/ElementBuilder.cs:161,171` | issues.md #1 |
| itemShape no fail-fast on non-array | `Scripts/conditions/conditions.ts:122-127` | issues.md #2 |
| Unknown vendor error missing element ID | `Scripts/resolution/resolver.ts:72` | issues.md #3 |
| ForTests functions exported from production modules | `Scripts/lifecycle/boot.ts:81`, `Scripts/validation/live-clear.ts:72`, `Scripts/components/native/native-action-link.ts:25` | — |
| PlanRegistry class exported (only facade functions should be public) | `Scripts/lifecycle/merge-plan.ts:18` | — |

SonarQube minor issues (#43, #45, #47) are open but may be partially resolved — verify against current code before acting.

## Open — Documentation

- CS1591 XML docs: count needs recount after recent PRs #95, #97
  - Per-module breakdown in `docs/cs1591-xml-docs-remaining.md`
- Dev experience gaps: 6 items from review
  - Details in `docs/reviews/dev-experience-review.md`

## Open — Skills & Process

- modern-csharp skill rewrite (1,272 lines, promotes C# 12+ but repo enforces C# 8.0 in libraries)
- A/B testing incomplete for all 8 skills

## Deferred

- Validation roadmap: date range, SF Grid inline edit, server-side filtering
  - Details in `.claude/memory/validation-roadmap.md`
- SonarQube pre-existing MAJOR+MINOR smells (untriaged)

## References

- Forensic master index: `.claude/memory/forensic-master-index.md`
- Dev experience review: `docs/reviews/dev-experience-review.md`
- Validation roadmap: `.claude/memory/validation-roadmap.md`
- Known issues: `docs/issues.md`
