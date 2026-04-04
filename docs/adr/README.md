# Architecture Decision Records

This directory records bad decisions, reversals, and corrective architecture decisions for this worktree.

It is not optional.

Any decision that violated [AGENTS.md](/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/.codex-worktrees/schema-capability-design/AGENTS.md), increased tech debt, or was later reversed because it was architecturally wrong must be captured here.

## When To Write An ADR

Write an ADR when a decision:
- introduced a bridge, adapter, fallback, or dual mental model
- widened correctness instead of preserving it
- touched the frozen DSL incorrectly
- relied on reflection
- preserved a bad test or bad helper
- coupled layers or slices incorrectly
- was later judged worse than `main`
- had to be undone because it violated the repo operating contract

## Required Fields

Every ADR must include:
- Title
- Date
- Status
- Scope
- Bad decision
- Why it was wrong
- Violated rule(s)
- Impact introduced
- Corrective decision
- Proof of correction
- Follow-up obligations

## Status Values

Use one of:
- `recorded`
- `rejected`
- `corrected`
- `superseded`

## Naming

Use:
- `0001-...`
- `0002-...`
- `0003-...`

with short descriptive slugs.

## Rule

If a bad decision is discovered and not written here, accountability is missing and the work is incomplete.
