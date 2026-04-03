# Process Pipeline — Layered Harness

This framework serves senior living communities. Residents depend on software built with it.
Correctness over speed. Evidence over assumptions. Pragmatic excellence at every layer.

## The Pipeline

Every change flows through layers. Each has skills, thinking, and a test harness.
A failing test drives every boundary crossing.

```
Layer 1: C# Plan Authoring & Builders
   │  Skills: TDD, modern-csharp, dotnet-xml-docs
   │  Verify: Value Objects, Encapsulation, Serialization impact, SOLID
   │  Harness: VerifyJson snapshots + AssertSchemaValid
   │
   ▼  BOUNDARY: Failing AssertSchemaValid() test drives schema update
   │
Layer 2: JSON Schema (reactive-plan.schema.json)
   │  The contract. A failing test is the only reason to edit this file.
   │  Harness: 310 AssertSchemaValid() + additionalProperties: false
   │
   ▼  BOUNDARY: Schema change → failing vitest drives TS type update
   │
Layer 3: TS Types → TS Runtime
   │  Skills: solid-ts-audit
   │  Verify: SOLID, vendor isolation, ID-driven, fail-fast
   │  Harness: vitest + boot(), architecture enforcement tests
   │
   ▼  BOUNDARY: Eyes first, then Playwright. Browser is truth.
   │
Layer 4: Browser Verification
   │  Skills: bdd-testing
   │  Verify: What does the USER see? Full journeys, real interactions.
   │  Harness: Manual smoke → Playwright BDD (5 rules)
   │
   ▼  BOUNDARY: Working sandbox example before writing docs
   │
Layer 5: Documentation & Skills
   │  Skills: dotnet-xml-docs, domain skills
   │  Verify: Dev-facing language, question-driven, progressive disclosure
   │  Harness: Sandbox-verified code examples, Rider diagnostics
```

One task can touch all layers (new primitive, component onboarding). It may span sessions.
The harness tracks which layers are verified and which still need work.

## Speed Gate

Before editing any file: read it first.
Before editing schema: have a failing test requiring the change.
Before committing: verify in browser.
Before accepting a review finding: trace the code path yourself.
Before dispatching an agent: specify input evidence and output evidence.
If editing a file a 2nd time this session: stop, rethink the approach.

Why: 25.6% of all commits in this repo are fixes. Each re-edit of a file is a mistake
that costs time and erodes trust. Correctness on the first pass is the standard.

## Wrong Plan Protocol

If touching an unexpected layer, the plan or task is wrong.

1. Stop immediately.
2. Save what you learned (to memory — context loss is the real cost).
3. Return to planning. Revert commits if needed, but save lessons first.
4. Present the problem to user step by step — concise, specific, not walls of text.

Why: the wizard session had 3 architecture changes in 30 minutes because there was no plan.
The validation session took 26 fix commits in one day because the design was discovered by coding.

## Evidence-Based Decisions

At every boundary crossing, define:

- **Input evidence**: what proves this change is needed? (failing test, user request, bug)
- **Output evidence**: what proves this change is correct? (passing test, browser verified, schema conforms)
- **Review evidence**: was this reviewed against actual code, or accepted on trust?

Five documented false alarms from reviewers confirm: always check actual code before
accepting any finding. The code is the authority, not the review comment.

## Agent Dispatch

See `agent-dispatch.md` — the single reference for constructing all agent prompts.

## Reference

Mistake patterns with commit evidence: `.claude/memory/forensic-master-index.md`
