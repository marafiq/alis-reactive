# Alis.Reactive Framework

This project uses C# (.NET), TypeScript, Playwright for E2E tests, Syncfusion components, and a custom reactive framework. Always run the full test suite (Playwright + unit tests) after changes and report exact pass/fail counts.

Alis.Reactive is a clean-break V2 system.

C# builders capture typed browser intent and render one JSON contract:

- `version`
- `contracts`
- `objects`
- `bindings`
- `workflows`

The browser runtime is a dumb executor for that contract. There is no legacy plan, no compatibility layer, and no second schema to reason about.

## Quality Bar

- One active model only: V2.
- No adapter, bridge, fallback, shim, or parallel implementation.
- If a module makes the system harder to reason about than `main`, it is a bug.
- Any old vocabulary that teaches the removed design is a bug.
- Every class, helper, and test must satisfy SOLID and proper encapsulation.
- Tests are behavior-first vertical slices, not internal implementation probes.
- Runtime uses explicit object/member resolution only. No DOM scanning, no guessing.

## Session Start

At the beginning of every session, load the `preflight` skill and complete its protocol. Do not begin work until preflight confirms: correct branch, baseline test counts, and no unaddressed uncommitted changes. Report: branch name, 3 most relevant CLAUDE.md constraints, baseline pass counts, loaded skills.

## Build & Verify

```bash
npm run build:all
dotnet build Alis.Reactive.slnx -nologo -t:Rebuild
npm run typecheck
npm test

dotnet test tests/Alis.Reactive.UnitTests/Alis.Reactive.UnitTests.csproj -nologo
dotnet test tests/Alis.Reactive.Native.UnitTests/Alis.Reactive.Native.UnitTests.csproj -nologo
dotnet test tests/Alis.Reactive.Fusion.UnitTests/Alis.Reactive.Fusion.UnitTests.csproj -nologo
dotnet test tests/Alis.Reactive.FluentValidator.UnitTests/Alis.Reactive.FluentValidator.UnitTests.csproj -nologo
dotnet test tests/Alis.Reactive.DriftDetection.Tests/Alis.Reactive.DriftDetection.Tests.csproj -nologo
dotnet test tests/Alis.Reactive.PlaywrightTests/Alis.Reactive.PlaywrightTests.csproj -nologo --logger "console;verbosity=detailed"
```

### Playwright Run Rules

1. **Kill ALL sandbox/dotnet processes** before running Playwright. Lingering processes cause port conflicts, timeouts, and false failures.
2. **Always use** `--logger "console;verbosity=detailed"` — never run without it. Non-verbose mode hides which tests failed.
3. **Never pipe Playwright output through grep/tail/filters** — the full output must be visible so every Passed/Failed test name and error message is captured. Unit test output may use `tail` for summary extraction.
4. **Run directly** — do not use `&` backgrounding with pipes. Use Bash `run_in_background` if needed, but the command itself must write full output to the output file.
5. **Rebuild JS bundle** (`npm run build`) before running — the bundle must match the current TS source.

## Mental Model

### C# authoring

- `ReactivePlan<TModel>` owns the document being authored.
- Builders create V2-native actions, predicates, requests, bindings, and workflows.
- Components register capability contracts and runtime objects.
- Typed expressions become value expressions and shapes.

### Rendered plan

- `contracts` describe vendor-specific members and events.
- `objects` name concrete runtime instances.
- `bindings` define canonical reads for typed model fields.
- `workflows` connect subscriptions to actions.

### Browser runtime

- Resolve object.
- Resolve member.
- Evaluate value/predicate.
- Execute action.

Nothing in the runtime invents missing information.

## Architectural Rules

### 1. The plan is the contract

No inline JavaScript in views. No hidden browser behavior outside the rendered plan and explicit component boot slices.

### 2. Vendor isolation

Vendor differences live in declared capability contracts and resolvers. They do not leak into unrelated modules.

### 3. Fail fast — no fallbacks

Missing contract data is an error. Wrong shapes are errors. Unsupported paths are errors. Do not guess. If the domain model expresses it, the plan carries it — there is no reason to doubt. Never use fallback defaults (`?? "value"`, `?? Shape.Any`) for values the framework knows at build time. If a value is null at consumption time, that is a pipeline bug — trace the root cause and fix the design end-to-end. Fallbacks hide bugs.

### 4. Surgical runtime

Use explicit ids and explicit object names. Do not introduce wide DOM queries when the plan can carry the information.

### 5. Vertical slices

Each component slice owns its contracts, event payloads, HTML helpers, builder surface, tests, and docs.

### 6. Observability

Tracing follows W3C/Otel-style context propagation. Requests carry `traceparent`. Logs and spans must describe real V2 behavior, not deleted concepts.

## Working Principles

Never rush to implementation. Always prove intermediate steps work before moving to the next (load skill `incremental-verify`). When working with third-party APIs (especially Syncfusion), research the correct API surface first — do not guess at method names, casing, or template syntax (load skill `research-first` or `syncfusion-slice`). At session start, verify branch and baseline test counts (load skill `preflight`).

## Git Workflow

When working in git worktree branches, always verify the current branch and read files from the CURRENT worktree, never from main. Run `git branch` and `pwd` before planning any work. Do not rewrite CLAUDE.md, configuration files, or process documents more than once per session without user approval. Before writing to files in a shared worktree, run `git status -s` to check for concurrent modifications.

## Debugging / Problem Solving

Do not take shortcuts or use fallback patterns. Trace issues through the full chain (domain model → schema → runtime). The user expects systematic root-cause analysis, not patches.

**2-Strike Rule (CRITICAL — no hook enforcement, self-discipline required):** After 2 failed fix attempts on the same issue, STOP coding immediately. Load skill `stop-and-research`. State what you tried, what failed, and why. WebSearch or read actual source code. Present researched solution for approval before writing more code. This rule exists because 94 wrong-approach instances were logged — most from guessing instead of researching. Cost of guessing: 1-2 hours. Cost of researching: 5 minutes. There is no hook that counts failed attempts — YOU must track this yourself. If the user catches you on attempt 3+ without having researched, that is a review failure.

## Tooling / Hooks

Be aware that hookify/pretooluse hooks may block legitimate edits (especially `public` keyword, NRT changes, test files). When an edit is blocked by a hook, mention the block to the user immediately and explain why. Do not bypass the block without explicit user approval.

## Change Rules

When adding or changing a capability:

1. Model it in V2 terms first.
2. Keep the C# DSL compile-time safe.
3. Serialize directly to the V2 schema.
4. Keep the runtime dumb.
5. Add or update unit, runtime, and Playwright coverage.
6. Delete dead code immediately.

## Review Questions

Ask these every time:

- Does this reduce tech debt compared with `main`?
- Does this keep one active model only?
- Is this class or helper SOLID and properly encapsulated?
- Am I keeping code that should be deleted?
- Does this test prove user-visible behavior instead of internals?

If the answer is not clearly yes, stop and redesign.

Before any code review, load skill `code-review`. It provides the operational steps; the Review Standards section below provides the detailed criteria.

## Review After Every Fix

Every fix triggers a new review round. If a reviewer finds issues and you fix them, the fixed code MUST go through another full review cycle including Codex xhigh. No fix is considered done until reviewers sign off on the FIXED version, not the original. This applies to plan changes, code changes, hook changes, skill changes, and CLAUDE.md changes. Shipping un-reviewed fixes is how bugs reach residents.

**No conditional sign-offs.** A reviewer says SIGN-OFF or BLOCK — nothing in between. "Conditional sign-off" means the reviewer found problems but didn't want to say BLOCK. That is a BLOCK. Fix the findings, re-review, get a clean SIGN-OFF. The only exception: if you categorically reject a finding with evidence that it is wrong (false positive, out of scope, or based on stale assumptions), document the rejection with file:line proof and move on. Rejections require the same rigor as findings — "I disagree" is not a rejection.

## Bug Reporting

When reporting bugs or issues, apply high scrutiny — do not report false positives. Before claiming something is a bug:
1. Write a failing test that reproduces it
2. Check if existing tests already cover this scenario (and pass)
3. Trace the code path end-to-end to confirm the behavior is wrong, not just unexpected
4. If you can't reproduce it with a test, it's not a confirmed bug — present it as "possible issue, needs investigation"

The user rejected 9 out of 9 reported bugs in one session because they were all false positives. 5 documented false alarms exist in this repo. The code is the authority, not your assumption.

## Review Standards — No Rubber Stamping

This framework serves senior living communities. Residents depend on it.
Every review must earn its verdict with evidence from actual code. Surface-level
grep checks and blanket SIGN-OFFs are not reviews — they are rubber stamps.

A reviewer MUST:

1. **READ the actual code**, not just search for keywords. Trace logic paths
   end-to-end: C# build → plan JSON → TS types → runtime execution.
2. **Run the flows**, not assume they work. If a change touches gather, trace
   a real gather through evaluateValue → transport → wire format.
3. **Verify truth alignment** across all layers. If the domain model says
   `ValueProducer`, the schema must say `$ref ValueProducer`, the TS type
   must say `value: ValueProducer`, and the runtime must call `evaluateValue`.
   Check each layer — do not assume alignment from one layer.
4. **Find what is NOT there**, not just validate what is. Missing error context
   in throw messages, missing shape propagation, missing XML docs on public
   members — absence is a finding.
5. **Question shared concepts**. If a module uses `applyShape`, verify the shape
   flows from the plan, not from a hardcoded guess. If a module catches errors,
   verify it only catches the specific error type, not swallowing everything.
6. **Check vocabulary**. Dead concepts (defaultValue, coercion, PeerReader) must
   not survive in variable names, comments, or error messages. The codebase
   vocabulary must match the current design, not the previous one.
7. **Report with file:line evidence**. Every PASS and every BLOCK must cite the
   exact code that proves the claim. "Looks correct" is not evidence.

A review that produces only PASS/SIGN-OFF without tracing at least one
end-to-end flow is incomplete. Send it back.

## Review Process — Team Ownership

Every plan and every implementation requires **3 independent sign-offs** before
it can be committed. Reviewers OWN the outcome — if implementation reveals a
gap the plan should have caught, the reviewer who signed off missed it.

### Gate 1: Plan Review (before implementation)

1. Write the plan with exact file paths, code changes, test tables, sandbox sections.
2. Fire 3 reviewers in parallel:
   - **Codex xhigh** — lead expert reviewer, highest bar, catches architectural gaps
   - **Code reviewer** — traces cross-layer alignment, verifies every file reference
   - **Independent 3rd reviewer** — fresh eyes, questions assumptions
3. Every BLOCK finding must be fixed and re-reviewed. Changes requested → changes approved.
4. Implementation starts ONLY when all 3 say SIGN-OFF. No exceptions.

### Gate 2: Post-Implementation Review (before commit)

1. After implementation, fire Codex xhigh + code reviewer on the actual diff.
2. They verify the committed code matches the signed-off plan.
3. Every finding must be fixed and re-reviewed.
4. Commit and push ONLY after post-implementation SIGN-OFF.

### Gate 3: Full Test Suite (before push)

1. All C# unit tests pass (dotnet test — all projects).
2. TypeScript typecheck clean (npm run typecheck).
3. Bundle builds (npm run build).
4. Full Playwright suite passes (789+ tests, --logger detailed, no filters).
5. Transient network flakes (ERR_NETWORK_CHANGED) are noted, not counted as failures.

### Reviewer Accountability — No Passengers

This framework serves senior living communities. Residents depend on software built
with it. Every reviewer is the last line of defense — not a rubber stamp.

**The standard for every reviewer (not just Codex xhigh):**

- Your SIGN-OFF means you OWN the outcome. If the implementation breaks,
  YOUR review failed. You are not a secondary checker — you are a co-owner.
- A SIGN-OFF with zero findings is suspicious. If you found nothing, you
  probably didn't look hard enough. The codebase has patterns that repeat —
  if a prior review found shape drops, null corruption, or trace bugs, those
  same patterns WILL appear in new code. Hunt for them.
- Do NOT just confirm what the plan says. Read the ACTUAL CODE the plan
  references. Run grep. Trace the flow. Find what the plan MISSED.
- Every PASS must have evidence that proves correctness, not absence of failure.
  "I checked and it looks right" is not evidence. "File:line shows X which
  matches Y in the schema" is evidence.
- Every BLOCK must have a concrete consequence: "If not fixed, X will happen
  at runtime." Theoretical concerns without consequences are DEFER, not BLOCK.
- If another reviewer already found 5 issues and you found 0, something is wrong
  with YOUR review, not with the code.

**Stakes reminder (include in every prompt):**

> This framework serves senior living communities. Residents — real people in
> care facilities — depend on software built with it. A silent bug in value
> resolution means wrong medication schedules displayed. A fallback that hides
> an error means a care alert that never fires. Quality is not optional.
> Your SIGN-OFF is your professional commitment that this code is correct.

### Architectural Checks (every review MUST verify these)

Reviewers MUST check these for every plan and implementation. These are not
optional — they are the framework's architectural invariants.

**1. DDD / Domain Model integrity:**
- Value objects have invariants enforced by constructors (not setters).
- Constructors are `internal` — devs use factory methods or builder APIs.
- Domain types carry meaning, not just data. Shape is a type contract, not a string.

**2. DSL surface protection:**
- `internal` members stay `internal`. Never promote to `public` without explicit approval.
- Builder constructors are `internal` — devs use `Html.XxxFor()` or `p.FromUrl()` factories.
- TypedSource subclass constructors are `internal` — devs get them from component extensions.
- Plan model classes (Request, Component, JsType) have `internal` constructors and `internal set`.
- If a reviewer sees a `public` constructor on a plan model class, that is a BLOCK finding.

**3. Plan carries ALL information — runtime is a dumb executor:**
- Every value the runtime needs is in the plan JSON. No runtime guessing.
- If a builder reads a CLR type (typeof(TProp)), that information becomes Shape in the plan.
- If a builder knows a component ID, that goes into the plan. Not discovered at runtime.
- Check: does the runtime make any decision that the plan should have made?

**4. No fallbacks (Rule 3):**
- No `?? "value"` for framework-known values.
- No `?? Shape.Any` — if shape is null, trace why and fix the source.
- No `log.warn` + continue for errors — throw with context.
- No `try/catch` that swallows and returns defaults.
- If a reviewer sees `??` or a warning-as-error-handling, verify it's truly needed.

**5. No vocabulary drift:**
- No `defaultValue`, `coercion`, `PeerReader` in new code.
- Variable names match the V2 design (ValueProducer, Shape, evaluateValue).
- Error messages describe the current architecture, not the old one.

### Lessons Encoded (from Headers + URL Templates reviews)

- Shape MUST flow through every ValueProducer creation path via Shape.FromClrType.
- String-destination values (headers, route params) require Shape.IsScalar guard.
- Null literal strings must throw ArgumentNullException at build time.
- Every builder overload gets its own unit test with AssertSchemaValid.
- Sandbox sections must have exact element IDs, button names, DSL code.
- vitest required for new TS runtime functions.
- Fail-fast: throws, not warnings + fallbacks (CLAUDE.md Rule 3).
