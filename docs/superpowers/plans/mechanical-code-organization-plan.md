# Mechanical Code Organization Plan

Pass goal for this plan-only pass:

```text
Close matrix row: N/A, repository organization only -> navigable module map -> runtime behavior unchanged
```

This is not a rewrite plan. It intentionally ignores `docs/design/redesign/` and
does not propose logical code changes, runtime behavior changes, DSL changes,
route changes, or feature changes. The only allowed implementation work from
this plan is mechanical movement of files, import updates, project-file updates
if needed, and matching test or doc path updates.

## Current Branch Evidence

- Current local branch: `cleanbreakbutrc1`.
- Local checkout is ahead of `origin/cleanbreakbutrc1` by 7 commits.
- Latest meaningful plan/runtime code commit on the PR branch:
  `71add3e1e1f0a5ffbbfde23d841a206179445a3f` -
  `Plugin module: ReactivePlugin->Plugin + remove arity ladder + collapse read/call builders into Arg-based surface`.
- The 7 local commits above the PR tip are docs, archive, and hook guidance
  changes, not production plan/runtime code changes.

Open PR branch comparison:

| PR | Head branch | Base branch | Commits over base | Note |
| --- | --- | --- | ---: | --- |
| #131 | `cleanbreakbutrc1` | `complete-dsl-with-array` | 9 | Selected latest stacked branch. |
| #130 | `complete-dsl-with-array` | `release/1.0.0-rc1` | 416 | RC1 base underneath #131. |
| #129 | `feature/reactive-array-operations-dsl` | `feature/blazor-static-ssr` | 18 | Older array DSL branch. |
| #128 | `all-sf-components-onboarded` | `feature/fusion-component-onboarding-skill` | 40 | Draft Fusion onboarding branch. |
| #126 | `refactor/plan-model-smells` | `release/1.0.0-preview1` | 336 | Older plan/runtime refactor branch. |

Interpreted as the current release stack, `cleanbreakbutrc1` has the most
updated plan/runtime code: #130 plus the 9 additional #131 commits.

## Current Organization Facts

Core C# package:

- `Alis.Reactive` has 114 C# source files.
- Major existing groups: `PlanModel` 35 files, `Builders` 35 files,
  `Validation` 17 files.
- 21 files are at project root or in small folders. These mix public DSL entry
  points, component registration, input binding, identity, path helpers, and
  response/event concepts.

Runtime TypeScript:

- `Alis.Reactive.Assets/runtime` has 80 TypeScript files.
- Major existing groups: `__tests__` 29 files, `execution` 10, `domain` 9,
  `core` 7, `validation` 6, `lifecycle` 5.
- `core` currently mixes shared utility code with runtime concepts such as
  value evaluation and plugin catalog ownership.

Fusion package and sandbox:

- `Alis.Reactive.Fusion/Components/*` already uses `Fusion*` folder names for
  every source component slice. Do not rename or collapse those slices.
- Fusion vertical slices are intentional. A slice may grow beyond one page,
  model, or Playwright file. `Grid` is the correct example of a larger vertical
  slice, not a smell by itself.
- Sandbox naming is less explicit. Under
  `Alis.Reactive.SandboxApp/Areas/Sandbox/{Controllers,Models,Views}/Components/Fusion`,
  most Syncfusion slice folders/classes drop the `Fusion` prefix because the
  parent folder already says `Fusion`.
- Playwright Fusion tests are mixed: some use `WhenUsingFusion*`, while many
  behavior tests use names like `WhenDateSelected`, `WhenBindingArrayToGrid`,
  or `WhenFilteringWithChips`.

## Organization Principles

1. Preserve behavior first. Moves must not change generated plan JSON, runtime
   execution, MVC routes, page-visible behavior, public DSL names, or public
   component API.
2. Preserve Fusion vertical slices. Do not flatten source slices into generic
   folders such as `Events`, `Builders`, or `Extensions` across components.
3. Make the main architecture flow visible:
   `Public DSL -> Plan Domain -> Generated TS Contract -> Runtime Executor`.
4. Prefer folder moves with namespace stability for the first implementation
   pass. Namespace cleanup is optional and should be a separate explicit pass.
5. Follow naming conventions that make sense for the codebase. Naming alignment
   is still mechanical when it preserves public API, route URLs, generated plan
   terms, runtime behavior, and test intent.
6. Do not move generated or build output. Exclude `bin`, `obj`, `dist`,
   `TestResults`, and Playwright traces.
7. Each implementation commit should be one mechanical move boundary with a
   passing build or focused test gate.

## Naming Convention Rules

Naming cleanup is allowed only as a mechanical move pass. It must not smuggle in
new domain concepts.

Rules:

- C# public DSL/API names stay stable unless the pass is explicitly scoped as a
  public rename and all call sites/tests prove compatibility or intentional API
  change. This organization plan does not include public API renames.
- Folder names should describe the thing a developer is looking for:
  `Dsl`, `Components`, `PlanModel`, `Requests`, `Reactions`, `Values`,
  `Conditions`, `Validation`, `BrowserObjects`, `Infrastructure`.
- Runtime TypeScript folders should name executor responsibilities:
  `execution/requests`, `execution/reactions`, `execution/triggers`,
  `execution/partials`, `execution/realtime`, `value`, `validation`, `plugins`,
  `shared`.
- Fusion source package slices keep the `Fusion*` prefix because the source
  package exposes Syncfusion component types and already follows that convention.
- Sandbox folders may omit `Fusion` only when the parent path already carries
  `Components/Fusion` and route stability matters. If this feels too implicit,
  write a route-preserving naming plan before changing paths.
- Playwright test class names should prefer the behavior under test, but folder
  paths must carry the component/vendor context. For example,
  `Components/Fusion/Grid/WhenUsingFusionGridBilling.cs` is fine for a broad
  grid board, while `Components/Fusion/InPlaceEditor/WhenQuickEditCommitsDate.cs`
  would be clearer than leaving many InPlaceEditor tests in the Fusion root.
- Any naming pass must include a before/after table and a proof that routes,
  generated `plan.ts`, and test discovery are unchanged.

## Proposed Mechanical Move Boundaries

### 1. Core C# Root Scatter

Goal: make the public authoring surface and component contracts easier to find
without changing type names or namespaces.

Move root-level files into stable folders:

| Current files | Proposed folder | Reason |
| --- | --- | --- |
| `ReactivePlan.cs`, `Plugin.cs`, `TypedEvent.cs`, `ResponseBody.cs` | `Dsl/` | Public authoring entry points live together. |
| `ComponentRef.cs`, `ComponentMember.cs`, `ComponentRegistration.cs`, `RegisteredComponentIdentity.cs` | `Components/Contracts/` | Browser object and component join-key concepts live together. |
| `RegisteredInputBinding.cs`, `RegisteredInputComponents.cs`, `InputComponentRegistrationProfile.cs`, `ClientValidationRuleBinder.cs` | `Components/InputBinding/` | Input registration and validation binding stop being root-level background noise. |
| `ExpressionPathHelper.cs`, `IdGenerator.cs` | `Dsl/ExpressionPaths/` | Expression capture and generated element ids are part of the public authoring-to-plan bridge, not anonymous infrastructure. |

Keep namespaces stable in this pass. This limits the change to file moves and
project/source-map updates.

Commit boundary:

```text
Core root scatter -> Dsl/Components folders -> build unchanged
```

Proof:

- `dotnet build`
- `npm run typecheck` only if generated plan or TS imports are touched, which
  this pass should not require.

### 2. Core PlanModel Subfolders

Goal: make `PlanModel` mirror the rich C# plan domain vocabulary instead of one
large bucket.

Proposed folder-only grouping:

| Proposed folder | Files |
| --- | --- |
| `PlanModel/Document/` | `PlanDocument`, `PlanBuildContext`, `PlanSerializer`, `PlanJsonWriter` |
| `PlanModel/Reactions/` | `Behavior`, `BehaviorGraph`, `ReactionGraph`, `StartsWhen` |
| `PlanModel/Requests/` | `RequestPlan`, `RequestInput`, `GatherRequestInput`, `RegisteredInputValueRead` |
| `PlanModel/Values/` | `ValueExpression`, `Source`, `Path`, `Shape`, `ShapeContractCompatibility` |
| `PlanModel/Conditions/` | `ConditionGraph`, `CompareOp`, `CompareOperator`, `MinimumTextLength` |
| `PlanModel/BrowserObjects/` | `BrowserObject`, `BrowserObjects`, `BrowserObjectId`, `BrowserObjectContract`, `BrowserObjectContracts`, `PluginContract` |
| `PlanModel/Validation/` | existing validation plan nodes plus `ValidationJob` |
| `PlanModel/Contract/` | `PlanContractGenerator`, `ContractDriftGate`, `PlanTerms` |

Open question before implementation: `PlanTerms` may remain in `PlanModel/`
if it is too cross-cutting to place under `Contract/`. That should be decided
from references, not preference.

Commit boundary:

```text
PlanModel bucket -> domain subfolders -> generated contract unchanged
```

Proof:

- `dotnet build`
- `npm run generate:plan-types`
- `git diff -- Alis.Reactive.Assets/runtime/types/plan.ts` must be empty.

### 3. Runtime TS Module Map

Goal: make the dumb runtime executor easier to navigate by separating execution
lanes, shared utilities, and value evaluation.

Mechanical grouping:

| Current area | Proposed area | Reason |
| --- | --- | --- |
| `core/evaluate.ts` | `value/evaluate.ts` | Value evaluation is runtime value execution, not generic core utility. |
| `core/plugin-catalog.ts` | `plugins/catalog.ts` | Plugin lookup is a plugin browser boundary. |
| `core/shape-convert.ts`, `core/assert-never.ts`, `core/trace.ts`, `core/wire-format.ts` | `shared/` | Utility/support code becomes explicit. |
| `core/url-template.ts` | `execution/requests/url-template.ts` | URL templating belongs to request execution. |
| `execution/http.ts`, `http-fetch.ts`, `gather.ts`, `request-payload-writer.ts`, `retry-indicator.ts` | `execution/requests/` | Request execution files stay together. |
| `execution/execute.ts` | `execution/reactions/execute.ts` | Reaction graph execution gets a clear home. |
| `execution/trigger.ts` | `execution/triggers/trigger.ts` | Trigger wiring is separate from reaction execution. |
| `execution/inject.ts` | `execution/partials/inject.ts` | Partial injection has its own browser boundary. |
| `execution/server-push.ts`, `signalr.ts` | `execution/realtime/` | Remote trigger infrastructure is grouped. |

Keep exported function names stable. Update imports only.

Commit boundary:

```text
Runtime folders -> executor lane folders -> vitest and typecheck unchanged
```

Proof:

- `npm run test -w Alis.Reactive.Assets`
- `npm run typecheck -w Alis.Reactive.Assets`

### 4. Runtime Vitest Mirror

Goal: make tests follow the runtime module map after the runtime files move.

Mechanical grouping:

| Current tests | Proposed tests |
| --- | --- |
| `__tests__/array/*` | `__tests__/value/array/*` |
| `__tests__/gather/*`, `http.test.ts` | `__tests__/execution/requests/*` |
| `execute.test.ts` | `__tests__/execution/reactions/*` |
| `inject.test.ts`, plan lifecycle slot tests | Keep under `__tests__/plan-lifecycle/` unless partial execution files move enough to justify `__tests__/execution/partials/`. |
| `plugin-catalog.test.ts` | `__tests__/plugins/catalog.test.ts` |

Commit boundary:

```text
Vitest paths -> runtime module mirror -> same 192 runtime tests
```

Proof:

- `npm run test -w Alis.Reactive.Assets`

### 5. Sandbox and Playwright Slice Alignment

Goal: make page-visible examples and tests easier to navigate without
collapsing vertical slices or changing routes.

Rules:

- Keep Fusion source package vertical slices exactly as vertical slices.
- Preserve existing MVC routes and Razor view resolution.
- Do not rename controller classes or view folders if that changes URLs.
- Prefer adding subfolders for large test slices over flattening them.
- Grid can remain a multi-page, multi-model, multi-test slice.

Recommended mechanical moves:

| Area | Move |
| --- | --- |
| Playwright Fusion root | Move multi-file behavior sets into slice folders, for example `Components/Fusion/InPlaceEditor/*`, `Components/Fusion/AutoComplete/*`, `Components/Fusion/DatePicker/*`. |
| Playwright Grid | Keep `Components/Fusion/Grid/*`; it is already the right pattern for a larger vertical slice. |
| Sandbox models | Keep folder-per-slice. Do not force every model namespace to match folders in the first move pass. |
| Sandbox views | Preserve route/view folder names unless an explicit route-preserving migration is planned. |

Fusion-prefix observation:

- Library source component slices use `Fusion*` consistently.
- Sandbox slice folders and controllers mostly omit the `Fusion` prefix.
- Test names are intentionally mixed between component names and behavior names.
- Do not "fix" this by renaming routes. If prefix consistency is desired, do a
  separate route-preserving naming plan that proves URLs and Playwright locators
  remain stable.

Commit boundary:

```text
Playwright Fusion tests -> slice folders -> routes unchanged, tests unchanged
```

Proof:

- `scripts/playwright.sh --filter "FullyQualifiedName~Components.Fusion"`
- Full `scripts/test.sh` before merging the whole mechanical plan.

## Suggested Execution Order

1. Core C# root scatter.
2. Core `PlanModel` subfolders.
3. Runtime TS module map.
4. Runtime vitest mirror.
5. Playwright Fusion test slice alignment.
6. Optional sandbox naming inventory document only. Do not route-rename in this
   plan.

This order keeps the highest-navigation-value, lowest-risk C# moves first, then
runtime imports, then tests.

## Blind Review Format

Use this exact format for a mid-level engineer review of the plan:

```text
Task lens:
I am a mid-level engineer trying to <find/change/debug/onboard something specific>.

Expected path:
The paths or folders I expected to inspect first.

Actual concern:
What in the proposed organization would slow me down, mislead me, or hide the next step.

Severity:
Blocking | Should fix | Optional | Non-actionable

Evidence:
Concrete current or proposed path names from this plan or repository.

Suggested mechanical adjustment:
A file/folder move, naming rule, or plan wording change. No behavior changes.

Why this is not just generic preference:
Reasoning tied to the task lens.
```

Reviewer guardrails:

- Feedback must name the task they were trying to do or the thing they were
  looking for.
- Generic folder preferences are allowed only when grounded in the task lens and
  repository evidence.
- Do not recommend collapsing Fusion vertical slices.
- Do not claim behavior changes from this plan.
- Do not use rewrite docs as authority.
- Findings without evidence paths are non-actionable.

## Mid-Level Engineer Review

This review is constrained to the plan above. It is not an implementation
review, and it does not claim behavior changed.

### Finding 1

Task lens:
I am a mid-level engineer trying to add a new expression-backed DSL source and
need to find how lambda member paths become runtime-readable paths.

Expected path:
`Alis.Reactive/Builders/*`, then a nearby path/expression folder.

Actual concern:
The original plan moved `ExpressionPathHelper.cs` to `Infrastructure/`, but the
file is used by DSL builders, request gather, dispatch payload building,
validation field token construction, and `IdGenerator`. A generic
infrastructure folder would hide the authoring-to-plan bridge.

Severity:
Should fix.

Evidence:
`Alis.Reactive/ExpressionPathHelper.cs`,
`Alis.Reactive/Builders/ElementBuilder.cs`,
`Alis.Reactive/Builders/Requests/GatherBuilder.cs`,
`Alis.Reactive/Validation/ClientValidationFieldToken.cs`,
`Alis.Reactive/IdGenerator.cs`.

Suggested mechanical adjustment:
Move `ExpressionPathHelper.cs` and `IdGenerator.cs` to
`Alis.Reactive/Dsl/ExpressionPaths/` instead of `Infrastructure/`.

Why this is not just generic preference:
The task is about tracing a real DSL capture path. The folder should expose that
bridge directly.

### Finding 2

Task lens:
I am a mid-level engineer trying to debug a generated TypeScript contract drift
failure.

Expected path:
`Alis.Reactive/PlanModel/Contract/` for generator and drift gate, then the
specific plan-domain folder for the term that drifted.

Actual concern:
Putting `PlanTerms.cs` under `PlanModel/Contract/` may imply that all plan
vocabulary is contract-generation-only. `PlanTerms` is cross-cutting enough
that the first mechanical move should not bury it until references prove a
better home.

Severity:
Should fix.

Evidence:
Proposed `PlanModel/Contract/` row includes `PlanContractGenerator`,
`ContractDriftGate`, and `PlanTerms`; the plan already marks `PlanTerms` as an
open question.

Suggested mechanical adjustment:
Keep `PlanTerms.cs` in `PlanModel/` for the first move pass, or split it only in
a separate naming/vocabulary pass with a before/after table.

Why this is not just generic preference:
The task starts from a contract drift failure, but the drifted term may belong
to request, validation, condition, value, or browser-object vocabulary. A
contract-only folder can mislead the search.

### Finding 3

Task lens:
I am a mid-level engineer onboarding or extending a Syncfusion component and
trying to find all browser tests for that component.

Expected path:
`tests/Alis.Reactive.PlaywrightTests/Components/Fusion/<Component>/`.

Actual concern:
The plan correctly preserves Fusion vertical slices, but many Fusion Playwright
files still sit directly under `Components/Fusion` with behavior names such as
`WhenDateSelected` or `WhenBindingArrayToGrid`. Behavior-first names are useful,
but the folder path should carry the component context.

Severity:
Should fix.

Evidence:
`tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/` already exists as
a larger slice, while files such as
`tests/Alis.Reactive.PlaywrightTests/Components/Fusion/WhenDateSelected.cs`,
`tests/Alis.Reactive.PlaywrightTests/Components/Fusion/WhenBindingArrayToGrid.cs`,
and `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/WhenFilteringWithChips.cs`
remain at the Fusion root.

Suggested mechanical adjustment:
Move behavior-named Fusion tests into component slice folders while preserving
class names where they remain clear. Example:
`Components/Fusion/DatePicker/WhenDateSelected.cs` and
`Components/Fusion/ChipList/WhenFilteringWithChips.cs`.

Why this is not just generic preference:
The task is finding complete coverage for a component. Component context in the
folder path is the navigation key, and behavior names can stay inside the slice.

### Finding 4

Task lens:
I am a mid-level engineer debugging request-input execution in the TypeScript
runtime.

Expected path:
`Alis.Reactive.Assets/runtime/execution/requests/`.

Actual concern:
The runtime move groups request files well, but `retry-indicator.ts` may not
belong under request execution if it is a generic browser feedback helper. The
plan should require checking actual imports before moving it with HTTP/gather.

Severity:
Optional.

Evidence:
Proposed runtime row groups `execution/http.ts`, `http-fetch.ts`, `gather.ts`,
`request-payload-writer.ts`, and `retry-indicator.ts` together.

Suggested mechanical adjustment:
During implementation, classify `retry-indicator.ts` from imports and call
sites. If only HTTP uses it, keep it under `execution/requests/`; otherwise move
it under `shared/browser/` or leave it in `execution/`.

Why this is not just generic preference:
The task is tracing request execution. If retry UI is shared beyond requests,
placing it under requests would create a false ownership signal.

### Finding 5

Task lens:
I am a mid-level engineer trying to understand whether `Fusion` prefixing is a
source-code requirement or just sandbox context.

Expected path:
Compare `Alis.Reactive.Fusion/Components/*` with sandbox and Playwright paths.

Actual concern:
The plan now states the important distinction: library source component slices
already use `Fusion*`, while sandbox folders/controllers often omit `Fusion`
under a parent `Components/Fusion` context. This is enough for the first
mechanical plan. Renaming sandbox routes for prefix symmetry would be higher
risk and not mechanically necessary.

Severity:
Non-actionable.

Evidence:
`Alis.Reactive.Fusion/Components/FusionGrid`,
`Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid`,
`Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/Components/Fusion/GridController.cs`,
`tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid`.

Suggested mechanical adjustment:
Keep the current plan wording. If prefix normalization is desired later, write a
separate route-preserving naming plan.

Why this is not just generic preference:
The task distinguishes package source naming from sandbox route naming. The
current plan protects both contexts.

## Gate To Use Before Any Implementation

Before implementing any mechanical move:

```bash
scripts/test.sh
```

For individual move commits, use the narrower proof listed in that section, then
run the full gate before merging the plan as a whole.
