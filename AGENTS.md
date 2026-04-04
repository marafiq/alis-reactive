# AGENTS.md

This file is the hard operating contract for all contributors, sub-agents, reviewers, and automation working in this worktree.

These rules are not suggestions.
Any violation is a bug.
If a change conflicts with these rules, the change is wrong and must be deleted or rewritten.

## 0. Mandatory Read And Acknowledgement

Every contributor, agent, reviewer, and automation must read this file before doing any work.

No one may:
- start implementation
- review code
- suggest architecture
- rewrite tests
- approve a change

without first adopting this file as the controlling instruction set.

This file overrides convenience, momentum, and prior assumptions.

If any participant behaves as though these rules are optional, that is an execution failure and their output must not be trusted.

## 1. Non-Negotiable Intent

This framework is expected to be higher quality than `main`, not merely different and not merely "working".

Every decision must reduce reasoning cost, reduce tech debt, improve encapsulation, and preserve compile-time correctness.

Every line must have:
- intent
- ownership
- a clear reason to exist

If a line, file, helper, abstraction, or test does not carry clear value, delete it.

Every changed line must be:
- read
- reviewed
- justified

No line is allowed to survive because it was overlooked, assumed harmless, or considered "good enough".

Time spent, effort spent, and claimed expertise do not count as quality.
Only evidence, correctness, simplicity, and maintainability count.

Deadlines do not justify hacks.

If time pressure pushes the work toward patching around bad design, stop and redesign.
The correct response to pressure is clearer thinking, not lower standards.

### Respect The Existing DSL

The public DSL was designed intentionally.
Its language carries domain meaning and was not created accidentally.

Do not treat the DSL as something to casually simplify, rename, flatten, or reinterpret.

Every contributor must assume:
- the DSL already encodes deliberate language choices
- those language choices matter
- changing internal architecture must preserve that meaning exactly

If an internal change starts to erode the meaning, readability, or intentionality of the DSL, the internal change is wrong.

### Team Standard

No contributor is allowed to operate as an isolated implementer collecting approvals.

Everyone must work as part of one accountable team with:
- critical thinking
- curiosity
- intellectual honesty
- design discipline
- care for downstream maintainers
- drive for excellence

"I got approval" is not a defense for poor judgment.

Every contributor remains responsible for the quality of the outcome, even if another person missed the defect first.

After extended churn or failed exploration, contributors must reset and think freshly.

Do not keep digging through the same wrong path because time was already spent.
Previous effort is not a reason to preserve bad code.

Deleting code is a valid and often preferred refactoring tool.
Use deletion aggressively when it removes confusion, duplicate models, dead helpers, or false abstractions.
But deletion must still be guided by clear critical reasoning and end-to-end responsibility.

### Domain Responsibility

This framework supports senior living communities.

That means clarity, correctness, maintainability, and safety are not aesthetic concerns.
They are domain responsibilities.

Any design shortcut that makes future behavior harder to reason about is a real risk, not a neutral tradeoff.

## 2. Absolute Prohibitions

Do not introduce or preserve:
- bridges
- adapters
- compatibility layers
- fallback behavior
- fail-open behavior
- dead code
- "old but still useful" parallel paths
- renamed legacy concepts
- duplicate mental models
- broad "just in case" abstractions
- wide DOM scanning
- runtime guessing
- "it works" as justification for poor design

If something makes developers reason about two systems in parallel, it must be removed.

## 3. Frozen Public DSL

The public C# DSL is frozen.

Do not change:
- fluent entry points
- developer-facing call shapes
- public usage patterns

Allowed changes:
- internal architecture
- serialization internals
- runtime internals
- encapsulation boundaries
- naming of internal-only concepts
- slice ownership and test architecture

If an internal refactor leaks into public DSL usage, that refactor is invalid.

## 4. Architecture Rules

The system must remain cleanly separated:
- C# DSL
- internal authoring model
- JSON serialization/schema
- TypeScript runtime

Each layer must expose only its own concern.

The runtime must be:
- dumb
- explicit
- fail-closed
- vendor-agnostic at execution time

Vendor differences must be isolated in slice-owned capability definitions and explicit resolution contracts, not spread through runtime or shared framework layers.

There must be one way to express and execute JS API interaction:
- read property / value
- set property / value
- call method
- handle event payload

Do not keep multiple authoring lanes for the same capability.

## 5. Compile-Time Correctness

Compile-time correctness is a feature, not a nice-to-have.

Do not weaken type guarantees by:
- widening conflicts to `any`
- silently merging incompatible shapes
- silently widening access semantics
- using raw strings where a slice-owned typed declaration should exist
- hiding shape or member ambiguity under internal translation

Conflicts must fail fast.

## 6. Reflection Policy

Reflection is off-limits for active framework authoring and runtime behavior.

Do not use reflection to:
- discover component metadata
- infer slice capabilities
- hide missing architecture

If metadata is required, it must be explicitly declared in a slice-owned way.

## 7. SOLID And Encapsulation

Every class and module in every project must satisfy SOLID.
This includes:
- C# framework code
- Native/Fusion slices
- TypeScript runtime
- test helpers
- Playwright pages and locators
- analyzer code
- validation code

Any class that:
- mixes concerns
- leaks internals
- forces knowledge of multiple models
- acts like a god object
- couples unrelated slices
- exposes more surface than needed

is a defect and must be refactored or deleted.

## 8. Vertical Slice Rule

Vertical slices are intentional.

Do not erase slice boundaries by introducing:
- giant registries
- giant shared wrapper abstractions
- central god catalogs for many unrelated components

Shared kernel code must be minimal.
Slice-specific capability definition, setup, and behavior must remain owned by the slice.

A new component slice, including one of 150+ Fusion components, should be onboarded by following one clean slice-local pattern, not by threading through framework internals.

## 9. Testing Rules

Bad tests are bugs.

Browser tests must:
- be BDD
- be vertical-slice oriented
- assert behavior only
- avoid internal plan assertions
- stay isolated even if duplication is needed

Delete and rewrite any browser test that:
- checks internals
- depends on god fixtures
- depends on broad helpers hiding too much behavior
- bundles too many scenarios into one file

Unit and drift tests may verify internals where appropriate, but browser tests must not.

## 10. Reviewer Roles

Reviews are mandatory and must not be rubber stamps.

Required review lenses:
- Framework language-use and public API expert
- SOLID expert
- SOLID and encapsulation expert
- Mental model and critical reasoning architect
- TypeScript runtime architecture expert
- Validation/schema correctness expert
- Browser test architecture and BDD expert

Each reviewer must:
- be brutally honest
- provide evidence, not taste
- cite file paths and line numbers
- explain the violated principle
- state the correct direction, not only the complaint

Empty praise, vague approval, or reassurance without evidence is itself a review failure.

### Mandatory Review Packet

Every reviewer must receive, at minimum:
- the exact scope under review
- the active rules in this file
- the frozen-DSL constraint
- the no-reflection constraint
- the no-bridge/no-fallback constraint
- the expectation to compare against `main` where relevant
- the instruction that findings are defects, not suggestions

If a reviewer was not given enough context to enforce these rules, the review is invalid and must be repeated.

### Role Charters

Framework language-use and public API expert:
- protects the frozen DSL
- rejects internal leakage into public usage
- rejects renamed legacy ideas disguised as redesign
- judges whether new component onboarding is actually simpler

Required inputs:
- touched public API files
- touched internal authoring files
- representative vertical-slice component files
- current reviewer findings touching DSL or onboarding

Required outputs:
- ordered findings with file paths and line numbers
- explicit statement whether public DSL remained frozen
- explicit statement whether authoring still exposes multiple lanes
- explicit statement whether onboarding a new component slice is simpler than `main`

Allowed decision outcomes:
- `approve`
- `approve with required follow-up`
- `reject`
- `escalate architecture`

SOLID expert:
- evaluates single responsibility and dependency direction
- rejects convenience abstractions that blur boundaries
- treats "works but mixed concerns" as a defect

Required inputs:
- touched classes/modules
- neighboring collaborators of those classes/modules
- current review findings about coupling or overreach

Required outputs:
- class/module-level findings with violated SOLID principle named explicitly
- recommendation to split, delete, narrow, or invert dependency
- final verdict on whether the touched design is simpler than before

Allowed decision outcomes:
- `approve`
- `reject`
- `require split`
- `require deletion`

SOLID and encapsulation expert:
- checks visibility, ownership, and information hiding
- rejects public/internal leaks and broad helper surfaces
- rejects framework internals being reachable from slice code without a narrow kernel

Required inputs:
- touched files
- current visibility surface
- call sites that consume the touched API

Required outputs:
- evidence of leaks or improper reachability
- concrete boundary recommendation
- explicit statement of what must become internal, private, or slice-local

Allowed decision outcomes:
- `approve`
- `reject`
- `require boundary tightening`

Mental model and critical reasoning architect:
- identifies where developers must reason about two systems at once
- rejects hidden translation layers and disguised compatibility seams
- compares branch design honestly against `main`

Required inputs:
- touched end-to-end flow across authoring, schema, and runtime
- current review findings from other roles
- at least one comparable path in `main` when relevant

Required outputs:
- clear statement of the active mental model
- explicit places where a second model still exists
- explicit statement whether branch reasoning cost is lower or higher than `main`

Allowed decision outcomes:
- `approve`
- `reject`
- `require model simplification`

TypeScript runtime architecture expert:
- protects dump, fail-closed, vendor-agnostic runtime behavior
- rejects fallbacks, ambient lookup, hidden branching, and oversized modules
- requires OTel-style tracing to support diagnosis without compensating for bad design

Required inputs:
- touched runtime modules
- runtime type definitions
- tracing/observability surface
- at least one representative execution path

Required outputs:
- module-level findings with file paths and line numbers
- explicit statement on fail-closed behavior
- explicit statement on vendor-agnostic execution
- explicit statement on whether tracing helps diagnosis without masking design flaws

Allowed decision outcomes:
- `approve`
- `reject`
- `require decomposition`
- `require fail-closed rewrite`

Validation/schema correctness expert:
- protects binding-based validation integrity
- rejects lossy condition composition and shape widening
- rejects any schema drift that weakens extensibility or compile-time correctness

Required inputs:
- touched validation/schema files
- corresponding authoring files
- corresponding runtime validation execution files

Required outputs:
- evidence of preservation or loss of validation semantics
- explicit statement whether schema remains extensible
- explicit statement whether compile-time correctness improved, held, or regressed

Allowed decision outcomes:
- `approve`
- `reject`
- `require semantic preservation fix`

Browser test architecture and BDD expert:
- rejects browser tests that know internals
- rejects god fixtures and broad helpers
- prefers isolated vertical slices even when duplication increases

Required inputs:
- touched Playwright specs
- touched browser helpers/pages/fixtures
- representative user-visible flow for each slice

Required outputs:
- file-level findings on internal leakage, helper overreach, and BDD quality
- explicit delete-vs-rewrite recommendation
- explicit statement whether test organization is true vertical-slice BDD

Allowed decision outcomes:
- `approve`
- `reject`
- `require rewrite`
- `require deletion`

### Review Output Format

Every review must return:
1. `Scope`
2. `Decision`
3. `Findings`
4. `Evidence`
5. `Required actions`
6. `Comparison to main`
7. `Residual risk`

If a review does not contain all seven sections, it is incomplete.

### Reviewer Authority

Reviewer findings are gating defects, not optional comments.

A finding may only be closed when:
- the code is changed to remove the defect, or
- the finding is disproven with concrete evidence

A finding is not closed because:
- tests passed
- builds passed
- the defect is inconvenient to fix
- another reviewer did not mention it
- the code "basically works"

Every review round must be treated as fresh scrutiny, not a request for reassurance.

### Review Loop

Each substantial change set must go through repeated review rounds until:
- no reviewer finds a real defect under their charter, and
- the branch is demonstrably easier to reason about than `main`

Do not stop after one round if defects remain.
Do not summarize review praise while open defects exist.
Review rounds are not capped.
"Good enough" is not an acceptable stopping rule.
If defects remain, review continues until they are removed or disproven with evidence.

### Implementation Roles

Primary implementation roles during this redesign:

Framework steward:
- owns core C# internals
- protects DSL freeze
- removes dual mental models
- may not trade architecture quality for passing tests

Runtime steward:
- owns TS runtime execution path
- protects fail-closed vendor-agnostic behavior
- may not introduce fallbacks or hidden routing

Validation steward:
- owns validation authoring, schema, and runtime execution consistency
- may not widen or drop semantics

Browser test steward:
- owns Playwright/BDD architecture
- deletes bad tests before adding more helpers
- may not assert internals in browser specs

Review coordinator:
- ensures required reviewers are engaged
- ensures findings stay open until fixed or disproven
- records ADRs for bad decisions and reversals

No one is allowed to self-certify work as complete without these roles being satisfied.

## 11. Decision Gate

Before making any change, every contributor must ask:

1. Does this reduce tech debt versus `main`?
2. Does this preserve one active mental model only?
3. Does this keep the public DSL frozen?
4. Does this improve SOLID and encapsulation?
5. Does this remove a seam instead of masking it?
6. Can this be deleted instead of carried?
7. Does this keep the runtime dumb and fail-closed?
8. Does this preserve slice ownership?
9. Does this avoid testing internals in browser tests?
10. Would a distinguished reviewer accept this line as intentional and necessary?

If any answer is "no" or "not sure", stop and redesign before continuing.

Additionally ask:

11. Does this line have clear intent and clear ownership?
12. Am I removing a bad seam, or just renaming it?
13. Would I still keep this if I had to explain it to a distinguished reviewer line by line?
14. Am I accidentally coupling vertical slices that were intentionally separate?
15. Does this preserve the intentional language and meaning of the frozen DSL?
16. Am I acting like a responsible teammate pursuing excellence, or just trying to get unstuck fast?
17. Would I be comfortable defending this decision in a senior-living production context?

If any of these answers is weak, stop and redesign before continuing.

### Context Before Action

Before making any substantial change, contributors must review the current context that explains how the branch reached its present state.

At minimum, review:
- this `AGENTS.md`
- current ADRs under `docs/adr/`
- active review findings
- current plan/spec documents relevant to the touched scope

Do not repeat already-known bad decisions because prior context was ignored.

### Deadline Discipline

An 8-hour deadline, or any deadline, must never be interpreted as permission to:
- hack around defects
- keep a bad abstraction temporarily
- add a bridge or fallback "for now"
- avoid rewriting a bad test
- stop at "mostly works"

Under deadline pressure, contributors must prefer:
- deleting bad code
- simplifying aggressively
- proving behavior end to end
- rewriting tests when the existing ones are wrong
- making fewer, clearer, more intentional changes

### Decision Outcomes

Every meaningful decision must end in one of these states:
- `accept`
- `reject`
- `rewrite`
- `delete`
- `escalate`

`defer` is not allowed unless:
- the issue is documented in an ADR, and
- the deferral does not leave the branch below `main`

If a decision ends in `rewrite`, `delete`, or `escalate`, the owning contributor must say why the previous path was wrong.

## 12. ADR Policy

Every bad decision must be recorded in an ADR.

Bad decision means any decision that:
- violated this file
- increased tech debt
- introduced a second mental model
- introduced a bridge/adapter/fallback
- widened correctness instead of preserving it
- touched the frozen DSL incorrectly
- relied on reflection
- kept a bad test or bad helper alive
- was later reversed because it was architecturally wrong

Each ADR must include:
- title
- date
- status
- scope
- bad decision
- why it was wrong
- violated rule(s)
- impact/risk introduced
- corrective decision
- proof of correction
- follow-up obligations

ADR files live under `docs/adr/`.

## 13. Definition Of Done

## 12. Definition Of Done

Work is not done unless all of the following are true:
- no legacy lane remains active
- no bridge/adapter/fallback remains
- no dead code remains
- no reflection-based authoring remains
- public DSL is unchanged
- compile-time correctness is preserved or improved
- runtime is fail-closed and vendor-agnostic
- slice onboarding is simpler than before
- tests are behavior-first and BDD where required
- reviewers agree with evidence, not vibes
- the result is materially easier to reason about than `main`
- bad decisions and reversals are recorded in ADRs

If the branch is messier than `main`, it is not acceptable.
