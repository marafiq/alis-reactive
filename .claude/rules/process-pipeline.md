# Process Pipeline — Layered Harness

This framework serves senior living communities. Residents depend on software built with it.
Correctness over speed. Evidence over assumptions. Pragmatic excellence at every layer.

## The Pipeline

The canonical 5-layer model, boundaries, and per-layer harness live in root
`CLAUDE.md` (Architecture — 5 Layers, 4 Boundaries). A failing test drives
every boundary crossing. Per-layer skills:

| Layer | Skills |
|-------|--------|
| 1 C# DSL & plan domain | `tdd`, `modern-csharp`, `dotnet-xml-docs` |
| 2 generated TS contract | none — `PlanContractGenerator` owns it; see `.claude/rules/plan-contract-boundary.md` |
| 3 TS runtime executor | `solid-ts-audit` |
| 4 browser verification | `bdd-testing` |
| 5 documentation | `dotnet-xml-docs`, domain skills |

One task can touch all layers (new primitive, component onboarding). It may span sessions.
The harness tracks which layers are verified and which still need work.

## Speed Gate

Before editing any file: read it first.
Before changing plan shape: regenerate the TS contract and run typecheck.
Before committing: verify in browser.
Before accepting a review finding: trace the code path yourself.
Before dispatching an agent: specify input evidence and output evidence.
If editing a file a 2nd time this session: stop, rethink the approach.

Why: 25.6% of all commits in this repo are fixes. Each re-edit of a file is a mistake
that costs time and erodes trust. Correctness on the first pass is the standard.

## Wrong Plan Protocol

If touching an unexpected layer, the plan or task is wrong. Stop immediately,
save lessons first, return to planning. The canonical protocol (with the why)
is in root `CLAUDE.md` → Process → Wrong Plan Protocol.

## Evidence-Based Decisions

At every boundary crossing, define:

- **Input evidence**: what proves this change is needed? (failing test, user request, bug)
- **Output evidence**: what proves this change is correct? (passing test, browser verified, typecheck clean)
- **Review evidence**: was this reviewed against actual code, or accepted on trust?

Five documented false alarms from reviewers confirm: always check actual code before
accepting any finding. The code is the authority, not the review comment.

## Agent Dispatch

See `agent-dispatch.md` — the single reference for constructing all agent prompts.

## Reference

Mistake patterns with commit evidence: `.claude/memory/forensic-master-index.md`
