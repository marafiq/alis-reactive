# Reactive Reader v3 — Next Session Prompt

Continue Reactive Reader v3 at http://localhost:4500.
    Worktree: ../Alis.Reactive-reader-v3/tools/md-viewer/
    Branch: feature/reactive-reader-v3

Previous session built: agent registry, two-round reviews with evidence
scoring, INVEST scorecard UI, plan-scoped navigation. PR #5 open.

Three priorities for this session:

## 1. Accept & Apply — Agent Finding Resolution

Stories AA-001 through AA-005 are in the app (plan: "Accept & Apply").
Core feature: one-click "Accept & Apply" button on each agent finding.

- Dispatches the SAME agent that raised the finding with focused context
- Agent returns structured changes: story body updates, new follow-up
  stories (linked back), plan goal/constraint updates
- Follow-up stories go through same INVEST gate (start as draft)
- "Dismiss" button with optional reason for won't-fix
- Changes shown inline on finding card after apply
- Schema: finding_resolutions table tracks status per finding

## 2. Plan-Level "Review All" + Configurable Review Types

Current gap: reviews can only be launched per-story. Need:

- **Plan-level "Review All" button** on the plan page — dispatches
  INVEST reviews for all eligible stories (draft/ready) at once
- **Review types beyond INVEST** — the system should support:
  - INVEST pre-work review (what we have)
  - SOLID post-implementation review (after code is written)
  - Pre-merge code quality review
  - Custom review types per plan
- Each review type has its own agent set and evaluation criteria
- Think: `review_types` table with name, criteria schema, linked agents
- Plan page shows a "Reviews" section with available review types

## 3. Agent Interaction UX Polish

Dogfooding revealed: "big gap in how you can easily see who is saying
and talk." Specifically:

- Agent identity needs to be clearer (avatar/icon per agent?)
- Accept & Apply is the primary interaction (Priority 1)
- Review results need better visual hierarchy
- Plan-level review status dashboard (how many stories reviewed,
  aggregate evidence quality, outstanding blockers across plan)

## Bug to Fix

`createStory` API endpoint doesn't persist `invest_*` fields from the
POST body. The STORY_COLUMN_WHITELIST in db.mjs is likely missing them.
Had to use PUT to update INVEST fields after creation.

Start with the createStory bug fix, then implement AA-001 through AA-005,
then tackle plan-level reviews.
