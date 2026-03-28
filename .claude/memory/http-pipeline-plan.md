---
name: http-pipeline-plan
description: HTTP pipeline redesign plan — user rejected current approach, says "stuck in bad design". Start fresh in new session.
type: project
---

# HTTP Pipeline Redesign — User Rejected Plan

## What Was Proposed (REJECTED)
- Make executeReaction async
- Expand error boundaries
- Status -1 for client errors
- Readonly ExecContext

## User's Feedback
"stuck in bad design" — the approach may be papering over fundamental issues instead of rethinking from first principles.

## What Needs Fresh Thinking
1. The relationship between sync executeReaction and async HTTP is fundamentally wrong. Making everything async might not be the answer — might need a different dispatch model.
2. Status -1 convention is inventing behavior — the runtime should not invent, the plan should carry error routing info.
3. The error boundary approach (multiple try/catch layers in pipeline + http) is patchy, not clean.
4. Need to think about HTTP request as a truly self-contained unit with clear lifecycle, not as "execution plus HTTP plumbing."

## Plan File Location
/Users/muhammadadnanrafiq/.claude/plans/giggly-waddling-star.md — contains the rejected plan for reference.

## Research Done (Keep)
- See memory/solid-ts-research.md for async patterns, immutability research
- See memory/http-pipeline-hardening.md for problem description
- Explore agent output: complete code trace of all HTTP files
- Test agent output: complete test inventory with gaps
- Research agent output: TanStack, Redux, Koa patterns for immutability and error boundaries

## Key Code Files
- execution/execute.ts — the sync/async split problem
- execution/pipeline.ts — orchestration
- execution/http.ts — request lifecycle
- execution/gather.ts — form data collection (pure, correct)
- types/context.ts — ExecContext definition
- types/http.ts — RequestDescriptor, StatusHandler
