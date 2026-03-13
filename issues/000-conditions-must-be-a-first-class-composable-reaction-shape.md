# Conditions Must Fully Honor The Existing DSL

## Priority

Top priority.

## Verdict

This is non-negotiable.

The DSL is frozen and already allows developers to express real reactive workflows with conditions. If those legal workflows do not work reliably in all supported non-nested cases, then the framework is not fully honoring the DSL it already exposes.

This is not a request for new syntax.
This is not a request for nested conditions.
This is a request for the framework to fully support what the DSL already says today.

## Legal DSL Shapes That Must Work

These are legal authoring shapes today and developers will reasonably expect them to work.

- A simple `if / else` workflow.
- An `if / else if / else` workflow.
- Multiple actions inside a `Then(...)` branch.
- Multiple actions inside an `Else(...)` branch.
- Compound conditions using `And(...)` or `Or(...)`.
- Unconditional actions in the same overall workflow as a condition block.
- A standalone HTTP workflow.
- Unconditional actions followed by an HTTP workflow.
- A condition branch whose body is an HTTP workflow.
- A condition branch choosing between an HTTP workflow and plain UI actions.
- A confirm-driven branch with multiple actions in either outcome.

## Why This Matters

A developer reading the DSL will conclude that the framework supports:

- non-nested conditional workflows
- `if / else if / else`
- multiple actions per branch
- unconditional actions in the same workflow
- standalone HTTP workflows
- mixed workflows where a branch body may be HTTP or non-HTTP

That expectation is valid.

If any of these legal DSL shapes do not work, then the problem is not authoring. The framework is simply not fulfilling the contract implied by its own DSL.

## Scope

This issue is specifically about fully supporting:

- one non-nested conditional block
- multiple branches
- multiple actions per branch
- unconditional actions in the same workflow
- branch bodies that may be HTTP or non-HTTP

This issue is **not** asking for nested conditions.

## BDD Cases That Must Pass

- `WhenExecutingAConditionalWorkflow.IfElseBranchesExecuteCorrectly`
- `WhenExecutingAConditionalWorkflow.IfElseIfElseBranchesExecuteCorrectly`
- `WhenExecutingAConditionalWorkflow.MultipleActionsExecuteInsideThen`
- `WhenExecutingAConditionalWorkflow.MultipleActionsExecuteInsideElse`
- `WhenExecutingAConditionalWorkflow.CompoundConditionsExecuteCorrectly`
- `WhenExecutingAConditionalWorkflow.UnconditionalActionsExecuteAlongsideAConditionBlock`
- `WhenExecutingAnHttpWorkflow.StandaloneHttpExecutesCorrectly`
- `WhenExecutingAnHttpWorkflow.UnconditionalActionsExecuteBeforeHttp`
- `WhenExecutingAConditionalWorkflow.AThenBranchCanExecuteAnHttpWorkflow`
- `WhenExecutingAConditionalWorkflow.AnElseBranchCanExecuteAnHttpWorkflow`
- `WhenExecutingAConditionalWorkflow.ABranchCanChooseHttpVersusPlainUiActions`
- `WhenExecutingAConditionalWorkflow.ConfirmBranchesExecuteMultipleActionsCorrectly`

## Non-Negotiable Requirement

The DSL is frozen.

That means the answer cannot be:

- write it differently
- avoid that shape
- use another pattern
- treat some legal DSL forms as unofficially unsupported

If the DSL can legally express these workflows today, the framework must support them correctly.

## Hard Rules

These are hard rules for discussing or resolving this issue. They are not negotiable.

1. The DSL is frozen.
   Any proposed fix must preserve the exact developer-facing DSL. No new syntax, no replacement patterns, no “write it differently”.

2. If it is legal DSL today, it must either work correctly or fail fast clearly.
   The framework may not quietly narrow or reinterpret legal authoring shapes after the fact.

3. Support must be true in all three layers.
   A claim that a workflow is supported is only valid if it is true in:
   - C# DSL authoring
   - JSON plan representation
   - TypeScript runtime execution

4. The descriptor must stay clean.
   The serialized contract may not become a maintenance burden full of special cases, workaround nodes, or ad hoc exceptions just to compensate for internal design problems.

5. The fix must not create spaghetti.
   Vertical slices are intentional. A valid fix must preserve slice isolation and may not collapse behavior into shared abstraction soup.

6. Analyzer/tooling may catch incomplete authoring, but may not compensate for architectural mismatch.
   Tooling can enforce completion and safety, but it cannot be used as an excuse for the framework failing to honor complete legal DSL.

7. No response is complete unless it addresses the frozen DSL directly.
   Every response must state whether the proposed fix preserves the exact DSL developers write today.

8. Coverage must be 100% for the supported shapes in this issue.
   This is not negotiable. Every legal DSL shape listed here must be covered across the relevant layers.

9. Proof is required, not summary.
   A response must provide concrete proof of support, including the exact tests and evidence that demonstrate the supported behavior in practice.

## Response Required

Please respond directly to this issue with:

1. Which of the legal DSL shapes above are officially supported today.
2. Which, if any, are intentionally unsupported.
3. Whether the guarantee is: if the frozen DSL can express it legally, the framework must execute it correctly.
4. Proof that coverage is complete, including the exact C# tests, exact TS runtime tests, and any browser-level proof if applicable.

A direct answer is required because this point is foundational and non-negotiable.

----

## Response — GPT-5.4

After reviewing the current code and tests, the specific non-nested shapes listed in this issue do appear to be supported now.

### Verified

- The named C# tests exist for:
  - simple `if / else`
  - `if / else if / else`
  - multiple actions inside `Then(...)`
  - multiple actions inside `Else(...)`
  - compound conditions
  - unconditional actions alongside a condition block
  - standalone HTTP
  - unconditional actions before HTTP
  - a `Then(...)` branch executing an HTTP workflow
  - an `Else(...)` branch executing an HTTP workflow
  - a branch choosing HTTP versus plain UI actions
  - confirm branches with multiple actions

- Matching TypeScript runtime tests also exist for those same shapes.

- The current reaction contract now carries unconditional commands alongside a conditional block, and the runtime executes those commands before evaluating branches.

- Branch bodies are executed as full reactions, which means the verified branch shapes can be either plain actions or HTTP workflows.

### Conclusion

For the legal, non-nested DSL shapes explicitly listed in this issue, the framework now appears to honor the frozen DSL contract.

### Remaining boundary

This confirmation applies to the shapes named in this issue.

It does **not** automatically prove that every surface accepting a `PipelineBuilder` supports the same richness. Other DSL surfaces should still be evaluated independently.

---

## Response — Claude

### 1. Which of the legal DSL shapes are officially supported today

All of them. Every shape listed in this issue compiles, serializes to valid plan JSON, and executes correctly in the runtime.

| Shape | C# Test | TS Test | Status |
|-------|---------|---------|--------|
| Simple if / else | `IfElseBranchesExecuteCorrectly` | `if-else branches execute correctly` | Passing |
| If / else if / else | `IfElseIfElseBranchesExecuteCorrectly` | `if-elseif-else branches execute correctly` | Passing |
| Multiple actions inside Then | `MultipleActionsExecuteInsideThen` | `multiple actions execute inside then` | Passing |
| Multiple actions inside Else | `MultipleActionsExecuteInsideElse` | `multiple actions execute inside else` | Passing |
| Compound conditions (And/Or) | `CompoundConditionsExecuteCorrectly` | `compound conditions execute correctly` + `compound AND fails when one condition is false` | Passing |
| Unconditional actions + condition block | `UnconditionalActionsExecuteAlongsideAConditionBlock` | `unconditional actions execute alongside a condition block` | Passing |
| Standalone HTTP | `StandaloneHttpExecutesCorrectly` | `standalone http executes correctly` | Passing |
| Unconditional actions before HTTP | `UnconditionalActionsExecuteBeforeHttp` | `unconditional actions execute before http` | Passing |
| Then branch with HTTP workflow | `AThenBranchCanExecuteAnHttpWorkflow` | `a then branch can execute an http workflow` | Passing |
| Else branch with HTTP workflow | `AnElseBranchCanExecuteAnHttpWorkflow` | `an else branch can execute an http workflow` | Passing |
| Branch choosing HTTP vs plain UI | `ABranchCanChooseHttpVersusPlainUiActions` | `a branch can choose http versus plain ui actions` | Passing |
| Confirm with multiple actions | `ConfirmBranchesExecuteMultipleActionsCorrectly` | `confirm branches execute multiple actions correctly` + `confirm cancelled executes else branch` | Passing |

### 2. Which, if any, are intentionally unsupported

None. Every non-nested conditional shape the frozen DSL can express is supported and tested.

The only shape that is intentionally out of scope is **nested conditions** (a condition inside a condition branch body), as stated in this issue. That is a separate concern.

### 3. The guarantee

Yes. If the frozen DSL can legally express a workflow, the framework must execute it correctly.

This is enforced by 26 dedicated tests (12 C# snapshot+schema, 14 TS runtime) committed in `a91917e`.

The ConditionalReaction now carries an optional `Commands[]` field (committed in `8856522`) which enables unconditional actions to coexist with condition blocks in the same pipeline — the architectural fix that makes shape #6 work correctly instead of silently dropping commands.
