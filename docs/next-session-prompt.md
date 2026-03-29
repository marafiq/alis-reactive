# Next Session Prompt

Branch: `refactor/api-surface-xml-docs`. 7 unpushed commits.

## Context

Last session (2026-03-28 evening) executed the process & harness priority list:
- Agent dispatch template (`agent-dispatch.md`) — 4 templates, 9-point evidence contract
- 3 hookify rules (API surface, BDD enforcement, BDD public API)
- Rules optimization (why, commands, caps, important tags)
- docs/ cleanup (78 → 24 files, -6,600 lines)
- 4 skills deep-reviewed + A/B tested + fixed (46 total fixes across onboard-fusion, validation-rules, bdd-testing, solid-ts-audit)

**Read first:**
- `.claude/memory/session_2026_03_28_todo.md` — master status: 6 done, 1 partial, 5 open
- `.claude/rules/agent-dispatch.md` — the new agent dispatch reference

## Step 1: Review Remaining Skills (Task 4 continuation)

4 skills still need the same treatment (review → A/B experiment → fix → verify):

1. **reactive-dsl** (275 lines) — core DSL, high usage
2. **http-pipeline** (235 lines) — all data flow
3. **conditions-dsl** (245 lines) — ResponseBody phantom confusion documented
4. **modern-csharp** (1,272 lines) — needs condensing, may need split

Process per skill:
1. Dispatch review agent: verify every claim against actual code
2. Dispatch A/B experiment agent: follow skill with real task, document gaps
3. Apply fixes from both rounds
4. Run top-10 audit, fix remaining failures
5. Save A/B test log as `references/ab-test-log.md` (closes Rule 10)

## Step 2: docs-site drift (Task 7) — own session

Plan the scope before starting:
- 5 pages reference deleted IReactivePlan
- 3 pages wrong API name
- Test counts 30-54% stale
- Touches content accuracy, code examples, sandbox verification

## Step 3: Code Issues (Tasks 8-11) — separate sessions

These are Layer 3 code changes, not process:
- SonarQube CRITICALs (conditions.ts, commands.ts, rule-engine.ts)
- Vendor isolation leaks (trigger.ts:45, live-clear.ts:44)
- Schema drift detection tool
- TS-to-schema validation

## Process Reminder

The layered harness is in `.claude/rules/` (4 files, auto-loaded):
- `process-pipeline.md` — pipeline overview, speed gate, wrong plan protocol
- `process-layers.md` — layer details, boundary crossings
- `process-task-types.md` — task categorization by layers
- `agent-dispatch.md` — 4 dispatch templates, evidence contract

Follow it. Load skills first. Plan before execution.
