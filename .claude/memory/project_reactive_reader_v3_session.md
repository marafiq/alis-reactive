---
name: Reactive Reader v3 — Agent Registry + INVEST Session
description: What was built in 2026-03-23 session, what's left for next session, user's feedback and priorities
type: project
---

## What Was Built (2026-03-23)

Branch: `feature/reactive-reader-v3`
Worktree: `../Alis.Reactive-reader-v3/tools/md-viewer/`
PR: https://github.com/marafiq/alis-reactive/pull/5

### Implemented (14 commits)
1. **Schema migration** — agent_templates, plan_agents, evidence_scores, invest_assessments, modified reviews (agent_role → agent_template_id FK + prompt/rubric snapshots). Dropped votes + conflict_summaries.
2. **API endpoints** — 10+ routes for templates CRUD, plan agents CRUD, invest summary, evidence scores, invest assessments
3. **Evidence scoring module** (`evidence.mjs`) — 0-100 score (50 category + 30 INVEST + 20 structural), rubber-stamp detection, vague language penalties
4. **Conflict detection module** (`conflicts.mjs`) — verdict conflicts, INVEST disagreements, unaddressed blockers
5. **Two-round agent orchestration** — dynamic agents from DB, enhanced prompts with rubric injection, automatic challenge round with cross-visibility
6. **INVEST UI components** — InvestScorecard (leads story detail), InvestHealthBar (board cards), InvestCriterionRow, AgentFeedbackEntry, DisagreementBanner, EvidenceQualityBadge
7. **Plan agent settings UI** — add/remove/configure agents per plan
8. **WebSocket round-aware events** — round tracking, round2-starting detection
9. **Plan-scoped navigation** — `/plans/:planId/stories/:storyId`, `/plans/:planId/board`, breadcrumbs, filtered sidebar

### PR Review Fixes Applied
- WebSocket agentId vs role field name mismatch (CRITICAL)
- SQL params spreading in getReviews
- ReviewSection now groups reviews by round (Round 1 / Challenge Round)
- effective_rubric API field name mismatch
- Duplicate round 2 prevention guard
- Pretty-printed rubric JSON in settings

### Dogfooding
- Created "Accept & Apply" plan with 5 INVEST-validated stories + NAV-001 directly in the app
- User confirmed the plan is visible and stories have proper INVEST scores

## What's Next (Next Session Priorities)

### Priority 1: Accept & Apply — Agent Finding Resolution
Stories AA-001 through AA-005 are in the app. Core feature:
- One-click "Accept & Apply" button on each agent finding
- Dispatches the same agent that raised the finding with focused context
- Agent generates structured changes: story updates, new follow-up stories, plan updates
- Follow-up stories go through same INVEST gate
- "Dismiss" button with optional reason for won't-fix findings
- Changes shown inline on finding card after apply

### Priority 2: Plan-Level "Review All" + Review Types
User feedback: there's no way to dispatch reviews for all stories in a plan at once. Also, INVEST review is only one type. The system needs:
- **Plan-level "Review All" button** — dispatch INVEST reviews for all eligible stories at once
- **Review types/pipelines** — not just INVEST pre-work, but also:
  - SOLID post-implementation review (after code is written)
  - Pre-merge code quality review
  - Custom review types per plan
- Each review type has its own set of agents and criteria
- Think: configurable review pipeline per plan

### Priority 3: Agent Interaction UX
User feedback from dogfooding: "big gap in how you can easily see who is saying and talk." The review UI needs:
- Clearer agent identity (who said what)
- Ability to respond to specific agents
- Conversational thread per finding (Accept & Apply is the start, but conversation might be needed too)

## Key User Feedback
- **Dogfooding is valuable** — user wanted to use the app itself to plan features
- **Navigation was a pain point** — fixed with plan-scoped routes, but may need more polish
- **INVEST invest_* fields not passed through createStory API** — had to use PUT to update them separately. Fix the create endpoint to accept these fields.
- **Review dispatch should be plan-level, not just story-level**
- **Review types should be configurable** — INVEST is just one type of review

## Technical Notes
- `createStory` in db.mjs doesn't persist invest_* fields from the POST body — the STORY_COLUMN_WHITELIST may be missing them. Fix in next session.
- The seed data backfill for invest_assessments works — investScores added to review_json + assessments table populated during seed
- getInvestSummary had 'mixed' instead of 'contested' — fixed
