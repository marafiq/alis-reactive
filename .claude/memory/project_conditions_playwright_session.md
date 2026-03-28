---
name: conditions-playwright-session
description: Next session — create Playwright vertical slices for conditions + HTTP mixing with dedicated controllers and deep BDD tests
type: project
---

## Conditions + HTTP Mixing — Playwright BDD Vertical Slices

**Branch**: fix/architecture-review-docs (worktree at .worktrees/fix-architecture-review-docs)
**PR**: #31

**What was done (2026-03-24)**:
- Fixed foundational pipeline mode bug — `SetMode()` in PipelineBuilder.Http.cs was treating HTTP as terminal mode
- `FlushSegment()` now handles HTTP and Parallel flushing + resets mode to Sequential
- `BuildReactions()` reuses `FlushSegment()` for trailing flush (DRY)
- 56 C# unit tests across 3 focused BDD files, all passing:
  - `WhenMixingCommandsAndConditions` (19 tests) — pure command + condition segmentation
  - `WhenMixingConditionsWithHttp` (29 tests) — HTTP + conditions at pipeline level (all verbs, Chained, Parallel, Confirm, compound guards)
  - `WhenUsingConditionsInsideResponseHandlers` (8 tests) — conditions inside OnSuccess/OnError
- All C# unit tests pass: 345 + 73 Native + 103 Fusion + 73 FluentValidator
- All TS tests pass: 1092

**What needs to happen next**:

### 1. Analyze existing Playwright test organization
- Read `tests/Alis.Reactive.PlaywrightTests/` structure
- Understand how existing pages (Events, Payload, Architecture, Conditions) are organized
- Check existing condition Playwright tests in `Conditions/` folder

### 2. Create Sandbox vertical slices for conditions + HTTP mixing
Each scenario needs its own:
- **Controller action** — returns the view
- **View (.cshtml)** — uses the DSL to build the plan (conditions + HTTP)
- **Playwright test** — BDD assertions on DOM state

Suggested vertical slice tree (under `/Sandbox/Conditions/`):
```
/Sandbox/Conditions/HttpMixing/ConditionAfterHttp
/Sandbox/Conditions/HttpMixing/ConditionBeforeHttp
/Sandbox/Conditions/HttpMixing/ElseIfChainAfterHttp
/Sandbox/Conditions/HttpMixing/CompoundGuardAfterHttp
/Sandbox/Conditions/HttpMixing/ConfirmThenHttp
/Sandbox/Conditions/HttpMixing/ChainedHttpWithConditions
/Sandbox/Conditions/HttpMixing/ParallelHttpWithConditions
/Sandbox/Conditions/HttpMixing/ConditionsInsideOnSuccess
/Sandbox/Conditions/HttpMixing/ConditionsInsideOnError
/Sandbox/Conditions/HttpMixing/RealisticFormWorkflow
```

### 3. BDD principles — MUST follow
- Test classes: `When{Scenario}` naming
- Tests assert DOM state after the plan executes (not plan JSON)
- Use framework primitives — `Html.On()`, `When().Then().Else()`, `Post().Response()`, etc.
- No manual JS in views, no hacks, no workarounds
- Each test navigates to its dedicated page, triggers the event, asserts DOM mutations

### 4. Key patterns to verify in browser
- Condition after HTTP: HTTP fires, then condition evaluates independently
- ElseIf chain after HTTP: all branches produce correct DOM state
- Compound guards (And/Or/Not) with HTTP: guard composition works in real browser
- Conditions inside OnSuccess: nested pipeline conditions fire on success response
- Conditions inside OnError: nested conditions fire on error response
- Chained HTTP: first request → second request → condition evaluates
- Parallel HTTP: both fire concurrently → condition evaluates after
- Confirm guard → HTTP: dialog blocks, HTTP fires after confirm

### 5. HTTP endpoints needed
- Controllers need mock API endpoints that return predictable responses
- For OnError tests: endpoints that return 400/409/500
- For success: endpoints that return JSON payloads matching MixedPayload shape

**Why:** Conditions are foundational. C# unit tests verify plan JSON shape. Playwright verifies the runtime actually executes conditions + HTTP correctly in a real browser. Without Playwright coverage, the pipeline mode fix is only half-verified.

**How to apply:** Start with the simplest slice (ConditionAfterHttp), get one working end-to-end, then replicate the pattern for all scenarios.
