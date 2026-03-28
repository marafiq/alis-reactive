# Alis.Reactive — Session Memory

## Active Work
- **Branch**: main (master deleted), refactor/api-surface-xml-docs for XML docs + ISP work
- **Current**: XML docs tour (near-complete), conditions module docs in progress
- See [project_xml_docs_dev_audit.md](project_xml_docs_dev_audit.md) — dev perspective audit: 9/10 rating, 12/14 gaps closed
- See [project_xml_docs_tour.md](project_xml_docs_tour.md) for docs tour progress + user style guide
- ISP refactor plan removed — IReactivePlan interface was deleted, plan is moot
- See [project_reactive_reader.md](project_reactive_reader.md) for Reactive Reader v3 design (SQLite + INVEST)
- See [project_reactive_reader_v3_session.md](project_reactive_reader_v3_session.md) for v3 agent registry + INVEST session (2026-03-23)
- See [project_docs_site.md](project_docs_site.md) for docs site status
- **Audit list**: `docs/audit-remaining.md`
- See [bdd-test-session.md](bdd-test-session.md) for BDD test redesign progress
- See [project_bdd_sandbox_reorganization.md](project_bdd_sandbox_reorganization.md) for Sandbox + test reorganization initiative
- See [project_bdd_next_session.md](project_bdd_next_session.md) for 5 flaky parallel tests + vertical slice fix needed
- See [feedback_plan_render_rule.md](feedback_plan_render_rule.md) for RenderPlan MUST always be called
- See [feedback_research_before_iterate.md](feedback_research_before_iterate.md) for research-before-guessing rule
- See [feedback_never_read_secrets.md](feedback_never_read_secrets.md) for NEVER read/list user secrets
- See [validation-module-session.md](validation-module-session.md) for validation 1.0 work

## SOLID TS Audit (March 2026)
- See [solid-ts-research.md](solid-ts-research.md) for research from Uncle Bob, Fowler, TanStack, Kent Beck
- Skill at `~/.claude/skills/solid-ts-audit/SKILL.md`
- eslint + typescript-eslint enabled (0 errors, 20 any warnings)
- All modules audited — see docs/audit-remaining.md for full status
- See [ts-refactor-plan.md](ts-refactor-plan.md) for original plan

## HTTP Pipeline Hardening (DONE)
- See [http-pipeline-hardening.md](http-pipeline-hardening.md) — error handling, immutability, retry-ready design
- COMPLETED: fully async executeReaction, expanded error boundary, ResolvedFetch, Promise.all

## Validation Module Completion (IN PROGRESS)
- See [feedback_validation_completion.md](feedback_validation_completion.md) — CRITICAL: read before ANY validation work
- See [validation-roadmap.md](validation-roadmap.md) — broader roadmap
- Plan: `docs/superpowers/plans/2026-03-19-validation-completion.md`
- Conditional parity VERIFIED (all 11 existing rule types work under WhenField)
- NEXT: Execute plan — coerceAs, field, 23 rule types, 100% extraction, Playwright
- readExpr as object — separate plan needed (DateRangePicker start/end)

## Validation Deep Audit (March 2026)
- See [validation-deep-audit.md](validation-deep-audit.md) — SF live-clear FIXED, ID-based lookups FIXED, isHidden FIXED

## Event Args Mutation Design (March 2026)
- See [event-args-mutation-design.md](event-args-mutation-design.md) — mutate-event command, EventGather, typed extensions on args
- New primitives: `mutate-event` (command), `event` (gather kind), `FromEvent()` (gather builder)
- Extensions live ON the args class (e.g., `args.UpdateData(s, json, j => j.Items)`)
- `args` needs pipeline param because it's a phantom — no pipeline binding (unlike ComponentRef)
- SF `e.updateData(data)` is the ONLY correct API for async filtering — experiment proved it

## Key Architectural Decisions
- See [component-architecture.md](component-architecture.md) for vendor-agnostic design
- See [idgenerator-design.md](idgenerator-design.md) for IdGenerator + plan-driven gather
- See [vertical-slice-design.md](vertical-slice-design.md) for vertical slice redesign
- See [descriptor-restructure-plan.md](descriptor-restructure-plan.md) for SOLID+DDD descriptor restructuring
- User plans to onboard 100+ component vertical slices — architecture must scale
- Runtime is dumb executor — plan carries ALL behavior info
- **Architecture tests must use REAL components** — never fake/mock
- **Typed access must match semantic type** — DateTime for dates, decimal for numbers, bool for checkboxes

## Pending Refactors
- See [project_pipeline_consistency_refactor.md](project_pipeline_consistency_refactor.md) — pipeline arg ordering, Fusion prefix, NativeHiddenField rename

## Critical Feedback — Session Mistakes
- See [feedback_ddl_test_flakiness.md](feedback_ddl_test_flakiness.md) — SF DDL re-selection flaky via keyboard, ej2 API workaround may mask bugs
- See [feedback_validation_session_mistakes.md](feedback_validation_session_mistakes.md) — MUST READ before any validation/UI work
- See [feedback_rubber_stamping.md](feedback_rubber_stamping.md) — NEVER rubber-stamp audits, every PASS must be earned
- See [feedback_no_tech_debt.md](feedback_no_tech_debt.md) — NEVER add tech debt: no fallbacks, no string-matching, no reflection hacks
- See [feedback_audit_evidence_criteria.md](feedback_audit_evidence_criteria.md) — 9-point evidence criteria before ANY code change from audit
- See [feedback_audit_agent_layers.md](feedback_audit_agent_layers.md) — 3-layer agent pattern: module readers → 3 judges → proven findings only
- See [feedback_xml_docs_style.md](feedback_xml_docs_style.md) — XML docs voice: dev-facing, no internals, no runtime jargon
- See [feedback_bdd_no_internals.md](feedback_bdd_no_internals.md) — BDD tests should not use internals
- See [project_xml_docs_dev_audit.md](project_xml_docs_dev_audit.md) — 14 gaps from dev perspective audit, 4 blocking

## User Preferences
- Prefers sync-first design (async only when HTTP calls need it)
- Strongly values SOLID principles and vertical slice isolation
- "Plan should carry information, runtime is dumb executor of plan behaviors"
- **Don't treat existing tests like a bible** — redesign tests to match architecture
- **TypedSource<TProp> is sacred** — never break typed condition access
- **BDD tests must be DEEP** — test user workflows, not framework plumbing
- **Duplication over abstraction** — each vertical slice is self-contained
- **Senior living domain** — use realistic models (residents, facilities, care levels)
- **Keep things typed** — never use untyped dispatch/events when typed alternatives exist
- **Use .Reactive() extensions** not custom events for component event wiring
- **Quality gate for ALL modules** in a refactor — never skip BDD tests for touched modules
- **Colocation is always a good thing** — coupled modules belong together

## HARD RULES — NEVER VIOLATE
- **NEVER change public API surface** without explicit user approval + full downstream analysis — See [feedback_api_surface_frozen.md](feedback_api_surface_frozen.md)
- **NEVER change `internal` to `public`** — internal members protect the API surface deliberately
- **NEVER change DSL, TS runtime, descriptor, or plan shape** without explicit user approval
- **NEVER write raw HTML in views** — always use framework builders
- **NEVER use input components without Html.Field()** — label + validation slot mandatory
- **Builder constructors MUST be internal** — devs use Html.XxxFor() factories only
- **Fusion methods use Fusion prefix** — `.FusionDropDownList()` not `.DropDownList()`
- **One Reactive overload per component** — input components are expression-only
- **Vertical slice shape is INVIOLABLE** — 7 files per component, no exceptions
- **Tests must understand framework** — Element() only for non-input display elements
- **If test fails, keep it** — user reviews fixes before any shape changes
- **Pass ALL tests after EVERY task** — vitest + Playwright, no exceptions

## Test Suite
- Run `npm test` + `dotnet test` for current counts (counts grow each session)
- Playwright: parallel fixtures, ~75 seconds
- All must pass before commit — no exceptions

## TS Runtime Structure (After Restructure)
```
Scripts/
├── root.ts                          ← entry point
├── types/                           ← 9 domain files + barrel
├── core/                            ← walk, trace, assert-never, coerce
├── resolution/                      ← resolver, component
├── execution/                       ← execute, commands, element, trigger, inject, pipeline, http, gather
├── conditions/                      ← conditions
├── lifecycle/                       ← boot, enrichment, merge-plan, walk-reactions
├── validation/                      ← orchestrator, rule-engine, condition, error-display, live-clear, index
└── components/
    ├── native/                      ← drawer, loader, checklist, native-action-link
    ├── fusion/                      ← confirm
    └── lab/                         ← test-widget
```

## Remaining Onboarding
- Check current component list in `docs-site/src/content/docs/reference/api-reference.md` — many already onboarded
- Conditions page: raw onclick buttons still need converting to NativeButton + Dispatch

## Documentation Site (March 2026)
- See [project_docs_site.md](project_docs_site.md) for full status and page inventory
- See [feedback_docs_writing_style.md](feedback_docs_writing_style.md) — CRITICAL: read before writing any docs
- Starlight + astro-d2 at `docs-site/`
- Sandbox Todo example verified: `/Sandbox/Todo`
- Architecture page needs more polish — question-driven, progressive disclosure

## SonarQube Integration (March 2026)
- SonarQube Community on Docker at localhost:9000, token in SONAR_TOKEN env var
- `./scripts/sonar-analyze.sh` — one-command analysis, step 7 in pre-commit gate
- 15 PRs created for CRITICAL+MAJOR issues (#21-#30, #38-#42), all verified with SonarQube scans
- Local branch `integration/sonarqube-all-fixes` has all 15 merged — 2,304 tests pass
- Issues #43-#47 created for remaining MINOR TS issues (not yet fixed)
- See [project_coerce_serialization_gap.md](project_coerce_serialization_gap.md) — FOUNDATIONAL: gather has no coercion, issue #48

## Pipeline Mode Fix (March 2026)
- See [project_conditions_playwright_session.md](project_conditions_playwright_session.md) for next session plan
- Branch: fix/architecture-review-docs, PR #31
- Fixed: `SetMode()` treated HTTP as terminal mode — conditions after HTTP now work
- `FlushSegment()` handles HTTP/Parallel flushing + mode reset
- 56 C# unit tests in 3 BDD files, all pass (345 total unit tests)
- NEXT: Playwright vertical slices — dedicated controllers + views + BDD browser tests for conditions + HTTP mixing

## API Doc Generator (March 2026)
- `tools/ApiDocGenerator/` reads 3 XML doc files, generates `docs-site/src/content/docs/reference/api-reference.md`
- `npm run build:api-docs` — CI-ready, accepts configuration arg
- 758 CS1591 warnings remaining — see `docs/cs1591-xml-docs-remaining.md` for module-by-module audit
- `<GenerateDocumentationFile>true</GenerateDocumentationFile>` enabled in Core/Native/Fusion csproj
