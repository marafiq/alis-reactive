---
name: forensic-master-index
description: 32 mistake patterns discovered from git forensic analysis of 753 commits — grounded in commit hashes, not memory summaries
type: reference
---

# Forensic Master Index — 32 Mistake Patterns

Generated 2026-03-28 from 9 forensic agents analyzing full git history (753 commits, 40 PRs, 23 SonarQube issues).

## Category A: Speed Over Correctness

| ID | Pattern | Key Evidence |
|----|---------|-------------|
| M1 | Patch-fix cascades — commit broken, fix symptoms one at a time | Validation: 26 fix/52 total on March 16. Wizard: 10 fixes in 75 min |
| M2 | Design-by-coding — build, discover it doesn't work, rebuild | Wizard: 3 arch changes in 30 min (9a8865c→f633aa6→ee51590) |
| M3 | "Fix ALL" that doesn't fix all | Binding redirects f7b2763 "correct ALL" → 2 more needed |
| M4 | Micro-commit streaming — tiny changes as separate commits | Docs: 31 commits/4 hours. CLAUDE.md: 7 rewrites/30 min |
| M5 | Never testing in browser — 10+ commits without opening browser | Validation session feedback_validation_session_mistakes #4 |
| M6 | "All tests pass" while browser broken | Validation session feedback_validation_session_mistakes #7 |
| M7 | Writing the rule while violating it | f7296ae "add Rule 8: root cause not patch" DURING a patch cycle |

## Category B: Not Reading Code Before Changing It

| ID | Pattern | Key Evidence |
|----|---------|-------------|
| M8 | Changed behavior without understanding design | e1269a1 deleted code, dcff10a restored it 77 min later |
| M9 | Guessed for days instead of researching | feedback_research_before_iterate: 2 days vs 5 min |
| M10 | Wrote docs without reading code | runtime.mdx: 5 passes for "Decision 3" |
| M11 | Concurrent agent reverted working code | cfa8367 "restore validation enrichment reverted by concurrent agent" |

## Category C: Tests — Wrong Investments

| ID | Pattern | Key Evidence |
|----|---------|-------------|
| M12 | Playwright: 25-35 hours infrastructure, 1 bug caught | 40+ commits on tooling vs 1 confirmed catch (c49a9b9) |
| M13 | Shallow BDD tests — asserted true/false, not journeys | feedback_validation_session_mistakes #1 |
| M14 | 54 test instances using internal constructors | 10 files construct ComponentEventTrigger, Entry directly |
| M15 | Test redesign churn — tests rewritten 3x for quality | WhenValidatingFormFields.cs: 13 revisions |

## Category D: API Surface & Encapsulation

| ID | Pattern | Key Evidence |
|----|---------|-------------|
| M16 | Changed API without downstream analysis — 170+ files | feedback_api_surface_frozen: 6 commits cascade |
| M17 | 5 constructors left public that should be internal | api-surface-code-review.md: AllGuard, AnyGuard, etc. |
| M18 | Vendor checks leaked outside component.ts — STILL broken | trigger.ts:45, live-clear.ts:44 |
| M19 | TS PlanRegistry class exported unnecessarily | merge-plan.ts:13 |
| M20 | 4 "ForTests" functions in production code | boot.ts:67, merge-plan.ts:142, etc. |

## Category E: Schema & Contract Drift

| ID | Pattern | Key Evidence |
|----|---------|-------------|
| M21 | Schema drifted 3 times in 33 revisions | b5bb10b, d1fa967, 4be3e5e |
| M22 | No automated schema-descriptor comparison — TODO never built | CLAUDE.md: "Schema drift is a known risk. TODO..." |
| M23 | TS types diverge silently from schema | componentType omitted from TS, present in C# + schema |
| M24 | MutateElementCommand.value TS type too narrow | C# object?, TS string|string[] |

## Category F: Reviewer Trust & Process

| ID | Pattern | Key Evidence |
|----|---------|-------------|
| M25 | Rubber-stamped audits — PASS without tracing paths | feedback_rubber_stamping |
| M26 | 5 false alarms — correct code flagged as bugs | sync executeCommand, window.alis.confirm, etc. |
| M27 | 3-layer judge pattern defined but never deployed | No commit references it as actually run |
| M28 | Architecture docs written from reviewer output, not code | 3247ff6, 91eafb2, 06cb0b7 — 3 corrections in 10 min |

## Category G: Documentation & Organization

| ID | Pattern | Key Evidence |
|----|---------|-------------|
| M29 | docs/ folder: 78 files, no index, 50 obsolete | 14 delete, 28 archive, 16+ stale refs |
| M30 | docs-site: 5 pages reference deleted IReactivePlan | Also NativeHiddenField wrong, test counts 30-54% stale |

## Category H: SonarQube

| ID | Pattern | Key Evidence |
|----|---------|-------------|
| M31 | Quality gate FAILING — 3 new CRITICALs from CoerceResult | conditions.ts 24, commands.ts 17, rule-engine.ts 26 |
| M32 | ~67 pre-existing MAJOR+MINOR smells untriaged | SonarQube dashboard, no issues filed |

## Quantified Cost

| Metric | Value |
|--------|-------|
| Fix commits / total commits | 193/753 (25.6%) |
| Overall file revision mistake ratio | 78.87% (1,710 excess of 2,168) |
| CLAUDE.md revisions | 45 (97.7% waste) |
| Playwright infrastructure hours | 25-35 hours for 1 bug caught |
| Schema drift incidents | 3 confirmed, 4 minor discrepancies current |
| False alarm reviewer findings | 5 documented |
| Concurrent agent damage | 1 incident (111 lines restored) |
| Docs files obsolete | 50 of 78 |