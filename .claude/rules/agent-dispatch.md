# Agent Dispatch — Strict Input/Output Criteria

Related: `process-pipeline.md` | `process-layers.md` | `process-task-types.md`

This framework serves senior living communities. Residents depend on software built with it.
Every agent prompt starts from one of the four templates below. Each template embeds the
evidence contract that prevents false alarms, wasted effort, and architecture reversals.

## Layer-Skill-Test Lookup

| Layer | Skills to Load | Test Command | Evidence Format |
|-------|---------------|--------------|-----------------|
| 1 C# | modern-csharp, bdd-testing (TDD principles) | `dotnet test tests/Alis.Reactive.UnitTests` | AssertSchemaValid() + file:line |
| 2 Schema | (contract file) | `dotnet test` (70 AssertSchemaValid calls) | Schema diff + failing test output |
| 3 TS | solid-ts-audit | `npm test` | vitest output + file:line |
| 4 Browser | bdd-testing | Playwright tests | Browser state + test name |
| 5 Docs | dotnet-xml-docs | Rider diagnostics | Sandbox-verified code example |

**Boundary triggers** (each requires a failing test as proof):
- 1→2: Failing `AssertSchemaValid()` drives schema edit
- 2→3: Failing vitest drives TS type update
- 3→4: Eyes first in browser, then Playwright
- 4→5: Working sandbox example before writing docs

## 9-Point Evidence Contract

Every finding or change satisfies all 9 before action:

| # | Criterion | What Counts |
|---|-----------|-------------|
| 1 | Describe the issue | Clear description |
| 2 | Identify root cause | Not symptom — trace to the origin |
| 3 | Cite evidence | file:line, test output, commit hash |
| 4 | Confirm no other layer handles it | Check the whole system first |
| 5 | Provide reproduction | Concrete scenario in browser or test |
| 6 | Propose fix with framework primitives | No hacks, no workarounds |
| 7 | State tangible outcome | What does fixing bring? |
| 8 | Verify framework alignment | Plan-driven, fail-fast, vertical slices |
| 9 | Confirm all tests pass after fix | No regressions |

## Template 1: Implementation Agent

```
Task: [description]
Layers: [from lookup table]
Boundary crossings: [from lookup table]

You are working on a Senior Living App Framework. Any lack of focus, guessing,
or skipped verification will cost dearly. Root yourself in pragmatic excellence.

FIRST: Load skills — [auto-map from Layer-Skill table]
THEN: Read [specific file paths for the area being changed]

Input evidence: [what proves this change is needed]
Output evidence: [what proves this change is correct]

Guardrails:
- Touching an unexpected layer means the plan is wrong — stop and return to planning.
  Why: wizard session had 3 architecture changes in 30 minutes without a plan.
- A failing test drives every boundary crossing.
  Why: 3 schema drift incidents were discovered by accident, not by process.
- Verify in browser before claiming done.
  Why: C# unit tests caught 11 bugs; Playwright caught 1. Browser catches more.
- After 2 fail rounds, stop coding and WebSearch.
  Why: 2 days of guessing vs 5 minutes of research (M9 in forensic index).
```

**Example A — New Primitive (all layers):**
```
Task: Add "toggle-class" command to reactive pipeline
Layers: 1, 2, 3, 4, 5 | Crossings: 1→2, 2→3, 3→4, 4→5
Skills: modern-csharp, dotnet-xml-docs, solid-ts-audit, bdd-testing
Read first: existing command descriptors, schema commands section, TS commands.ts
Input evidence: User request + no existing toggle-class in schema
Output evidence: VerifyJson snapshot, AssertSchemaValid passes, vitest passes, Playwright passes
```

**Example B — Single-Layer Refactor:**
```
Task: Extract CoerceResult complexity in conditions.ts
Layers: 3 | Skills: solid-ts-audit
Read first: conditions.ts (full file), related vitest
Input evidence: SonarQube CRITICAL — cyclomatic complexity 47
Output evidence: npm test passes, complexity under 15, all tests pass
```

## Template 2: Audit Agent (3-Layer Pattern)

**Layer 1 — Module Readers:** One agent per scope, reads every line in their scope.
Scopes: core+types, execution, network+HTTP, resolution+conditions,
validation, lifecycle+boot, components.
Output per file: Clean OR finding with file:line + severity + tangible outcome.

**Layer 2 — Three Judges** (after all readers complete):
- **Integration Judge** — systemic patterns ACROSS modules (what individual readers miss)
- **Non-Dogmatic Judge** — classifies: MUST FIX / SHOULD FIX / DEFER / REJECT
- **Evidence-Based Prosecutor** — proves each finding against 9-point contract above

<important>Only findings that pass all 3 judges become work items.</important>

Why: Module readers over-report in isolation. Judges filter noise. The Prosecutor prevents
false alarms from becoming wasted work (5 documented false alarms in this repo).

Each judge receives the positive framing preamble and ranks findings by value.

**Example — TS Runtime SOLID Audit:**
```
Reader scopes: 7 agents (one per scope above)
Skills for readers: solid-ts-audit
Judge input: all reader reports
Prosecutor input: reader reports + code access to verify file:line claims
Output: ranked findings, most impactful first, stop when signal drops
```

## Template 3: Review Agent

```
You are reviewing [what] for a Senior Living App Framework.
Pragmatic excellence is the standard. Stakes are real.

Input: [specific files, PR diff, or agent output to review]
Criteria: 9-point evidence contract + layer-specific harness from lookup table

Output design:
- Rank findings by value to the system (impact if not addressed).
- Most impactful first. Each finding: file:line + evidence + consequence.
- Stop when remaining findings add no meaningful value.
- Verify against actual code before accepting any finding.

Coverage completeness (MANDATORY for test suite reviews):
- List every item in the scope (schema $defs, TS exports, API members, etc.)
- Map each to the test that covers it — by name, not by assumption
- Report uncovered items as findings ranked by risk
- "All tests pass" is not a sign-off. "All items are covered or justified" is.

5 documented false alarms confirm: the code is the authority, not the review.
```

**Example — PR Review:**
```
Task: Review PR #42 — CoerceResult extraction
Layers touched by PR: 3 | Skills: solid-ts-audit
Input: git diff main...fix/coerce-extraction
Output: ranked findings with file:line evidence, most impactful first
```

**Example — Test Suite Review (with coverage gate):**
```
Task: Review drift detection test suite for completeness
Scope: all $defs in reactive-plan.schema.json (51 definitions)
Input: tests/Alis.Reactive.DriftDetection.Tests/**/*.cs
Output: coverage matrix (definition → test name), uncovered items ranked by risk
Sign-off requires: every definition mapped to a test or justified as untestable
```

## Template 4: BDD Agent

**Cascade preamble** (include in every BDD agent prompt):
> You are writing Playwright tests for Alis.Reactive — a framework serving
> senior living communities. Load skill: bdd-testing. Read the BDD Constitution
> at `memory/bdd-principles.md` before writing any test.
> Five Rules: (1) Behavior, (2) Independent, (3) Fails when broken,
> (4) Real interactions, (5) Blind reviewed.
> Cardinal Rule: framework code is read-only.

**7-behavior contract** per component: Renders, Interacts, Validates,
Conditionally Validates, Live-Clears, Gathers, Submits.

**After writing tests** → dispatch blind reviewer (full template in BDD Constitution).

**Example — Component Test Suite:**
```
Task: Write BDD tests for FusionDatePicker
Layers: 4 | Skills: bdd-testing
Read first: DatePicker sandbox page, existing DatePicker Playwright tests
Input evidence: Component onboarded, zero Playwright coverage
Output evidence: 7-behavior contract covered, blind reviewer passes all tests
```

## Escalation Path

| Round | Action |
|-------|--------|
| 1 | Re-read the code path end-to-end at the stuck layer |
| 2 | Check if another layer already handles it (9-point criterion #4) |
| After 2 | Stop coding. WebSearch for the specific issue. Save findings to temp file |
| Still stuck | Save learnings to memory. Present the problem to user: what you know, what you tried, what the options are |

## Evidence Format Examples

| Layer | Example Evidence |
|-------|-----------------|
| 1 C# | `src/Alis.Reactive/Commands/ToggleClassCommand.cs:42` — VerifyJson snapshot showing exact JSON |
| 2 Schema | `"added 'toggle-class' to commands oneOf"` — AssertSchemaValid() failing output |
| 3 TS | `Scripts/execution/commands.ts:187` — vitest: `"FAIL: toggle-class produces correct mutation"` |
| 4 Browser | `selecting_care_level_updates_billing_amount` — field shows "$2,400" after selecting "Memory Care" |
| 5 Docs | `/Sandbox/ToggleClass` verified — zero CS1591 warnings in file |

## References (read, do not duplicate)

- Layer details + boundaries + harness: `process-layers.md` (auto-loaded)
- Task types + 10-step checklist: `process-task-types.md` (auto-loaded)
- Pipeline + speed gate + wrong plan protocol: `process-pipeline.md` (auto-loaded)
- BDD principles (consolidated): `memory/bdd-principles.md`
- Quality principles (audit, review, evidence): `memory/quality-principles.md`
- Forensic mistake patterns: `memory/forensic-master-index.md`
