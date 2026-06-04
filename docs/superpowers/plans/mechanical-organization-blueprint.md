# Mechanical Organization Blueprint

Pass goal for this mechanical organization pass:

```text
Close matrix row: repository organization only -> PlanAuthoring/PlanModel/runtime module map -> behavior unchanged
```

This document is the execution blueprint. It keeps the existing analysis document
as background context and makes the implementation decisions required to move
files without changing public DSL/API behavior, generated plan JSON, runtime
behavior, MVC routes, or page-visible behavior.

## Target Organization

```text
Alis.Reactive/
├── PlanAuthoring/                         developer-authored plan surface
│   ├── ReactivePlan.cs                    public entry point
│   ├── Pipelines/                         trigger -> pipeline -> reaction authoring
│   ├── Requests/                          request input, gather, and response authoring
│   ├── Conditions/                        condition source and branch authoring
│   ├── Arrays/                            typed array/value expression authoring
│   ├── Plugins/                           plugin contract authoring surface
│   ├── Events/                            typed event and response body authoring
│   └── ExpressionPaths/                   expression capture/id helper support
│
├── Components/                            typed component contracts and registration
│   ├── Contracts/                         component identity, references, members
│   ├── InputRegistration/                 registered input binding/profile support
│   └── Onboarding/                        component object and event onboarding
│
├── PlanModel/                             rich generated plan domain
│   ├── Document/                          document and build context
│   ├── Serialization/                     serializer and JSON writer
│   ├── ContractGeneration/                TS contract generator and drift gate
│   ├── WireTerms/                         generated/wire term constants
│   ├── Reactions/                         reaction graph and starts-when concepts
│   ├── Requests/                          request plans, inputs, gather reads
│   ├── Values/                            value expressions, sources, paths, shapes
│   ├── Conditions/                        condition graph and comparison terms
│   ├── BrowserObjects/                    browser object and plugin contracts
│   └── Validation/                        validation plan graph
│
├── Razor/                                 MVC/Razor integration
├── InputField/                            input field integration surface
├── Serialization/                         non-plan-model serialization helpers
└── Validation/                            server-side validation adapters

Alis.Reactive.Assets/runtime/
├── lifecycle/                             plan boot, apply, merge, unload
├── execution/                             dumb runtime executor
│   ├── reactions/                         reaction graph execution
│   ├── triggers/                          trigger wiring
│   ├── requests/                          HTTP, gather, payload, URL templates
│   ├── partials/                          partial injection
│   └── realtime/                          SignalR and server push
├── values/                                value and array evaluation
├── conditions/                            condition execution
├── browser-objects/                       component/plugin/browser object lookup
├── plugins/                               plugin catalog
├── validation/                            browser validation runtime
├── components/                            component runtime integration
├── events/                                native and Fusion event resolution
├── diagnostics/                           trace/debug-only helpers
├── shared/                                generic runtime support helpers
└── types/                                 generated TS contract

tests/Alis.Reactive.PlaywrightTests/
├── Components/
│   ├── Fusion/                            Syncfusion vertical slices
│   ├── Native/                            native component slices
│   └── AppLevel/                          app-level component behaviors
├── HttpPipeline/
├── Conditions/
├── Validation/
└── Patterns/
```

## Naming Decision

Use `PlanAuthoring/` for the developer-authored plan surface.

Rejected alternatives:

- `Dsl/`: rejected because DSL is a high-level description, not an internal
  repository concept used by the source.
- `Builders/`: rejected as the top-level organization because it is too narrow
  for `ReactivePlan`, `Plugin`, `TypedEvent`, `ResponseBody`, expression paths,
  and id helpers.

## C# Move Matrix

All public shipped `Alis.Reactive` namespaces remain unchanged. Test namespaces
may follow folders. Internal-only namespaces may change only when the file
contains no public type.

| Current path | Target path | Namespace rule |
| --- | --- | --- |
| `Alis.Reactive/ReactivePlan.cs` | `Alis.Reactive/PlanAuthoring/ReactivePlan.cs` | Keep |
| `Alis.Reactive/Plugin.cs` | `Alis.Reactive/PlanAuthoring/Plugins/Plugin.cs` | Keep |
| `Alis.Reactive/Builders/PluginArguments.cs` | `Alis.Reactive/PlanAuthoring/Plugins/PluginArguments.cs` | Keep |
| `Alis.Reactive/Builders/PluginMemberBuilder.cs` | `Alis.Reactive/PlanAuthoring/Plugins/PluginMemberBuilder.cs` | Keep |
| `Alis.Reactive/Builders/PluginTypeBuilder.cs` | `Alis.Reactive/PlanAuthoring/Plugins/PluginTypeBuilder.cs` | Keep |
| `Alis.Reactive/TypedEvent.cs` | `Alis.Reactive/PlanAuthoring/Events/TypedEvent.cs` | Keep |
| `Alis.Reactive/ResponseBody.cs` | `Alis.Reactive/PlanAuthoring/Events/ResponseBody.cs` | Keep |
| `Alis.Reactive/ExpressionPathHelper.cs` | `Alis.Reactive/PlanAuthoring/ExpressionPaths/ExpressionPathHelper.cs` | Keep |
| `Alis.Reactive/IdGenerator.cs` | `Alis.Reactive/PlanAuthoring/ExpressionPaths/IdGenerator.cs` | Keep |
| `Alis.Reactive/Builders/PipelineBuilder.cs` | `Alis.Reactive/PlanAuthoring/Pipelines/PipelineBuilder.cs` | Keep |
| `Alis.Reactive/Builders/PipelineBuilder.Arrays.cs` | `Alis.Reactive/PlanAuthoring/Pipelines/PipelineBuilder.Arrays.cs` | Keep |
| `Alis.Reactive/Builders/PipelineBuilder.Conditions.cs` | `Alis.Reactive/PlanAuthoring/Pipelines/PipelineBuilder.Conditions.cs` | Keep |
| `Alis.Reactive/Builders/PipelineBuilder.Http.cs` | `Alis.Reactive/PlanAuthoring/Pipelines/PipelineBuilder.Http.cs` | Keep |
| `Alis.Reactive/Builders/TriggerBuilder.cs` | `Alis.Reactive/PlanAuthoring/Pipelines/TriggerBuilder.cs` | Keep |
| `Alis.Reactive/Builders/ElementBuilder.cs` | `Alis.Reactive/PlanAuthoring/Pipelines/ElementBuilder.cs` | Keep |
| `Alis.Reactive/Builders/DispatchPayloadBuilder.cs` | `Alis.Reactive/PlanAuthoring/Pipelines/DispatchPayloadBuilder.cs` | Keep |
| `Alis.Reactive/Builders/ReactionPipelineDraft.cs` | `Alis.Reactive/PlanAuthoring/Pipelines/ReactionPipelineDraft.cs` | Keep |
| `Alis.Reactive/Builders/IReactionEmitter.cs` | `Alis.Reactive/PlanAuthoring/Pipelines/IReactionEmitter.cs` | Keep |
| `Alis.Reactive/Builders/Arrays/*` | `Alis.Reactive/PlanAuthoring/Arrays/*` | Keep |
| `Alis.Reactive/Builders/Conditions/*` | `Alis.Reactive/PlanAuthoring/Conditions/*` | Keep |
| `Alis.Reactive/Builders/Requests/*` | `Alis.Reactive/PlanAuthoring/Requests/*` | Keep |
| `Alis.Reactive/IComponent.cs` | `Alis.Reactive/Components/Contracts/IComponent.cs` | Keep |
| `Alis.Reactive/ComponentRef.cs` | `Alis.Reactive/Components/Contracts/ComponentRef.cs` | Keep |
| `Alis.Reactive/ComponentMember.cs` | `Alis.Reactive/Components/Contracts/ComponentMember.cs` | Keep |
| `Alis.Reactive/ComponentRegistration.cs` | `Alis.Reactive/Components/Contracts/ComponentRegistration.cs` | Keep |
| `Alis.Reactive/RegisteredComponentIdentity.cs` | `Alis.Reactive/Components/Contracts/RegisteredComponentIdentity.cs` | Keep |
| `Alis.Reactive/RegisteredInputBinding.cs` | `Alis.Reactive/Components/InputRegistration/RegisteredInputBinding.cs` | Keep |
| `Alis.Reactive/RegisteredInputComponents.cs` | `Alis.Reactive/Components/InputRegistration/RegisteredInputComponents.cs` | Keep |
| `Alis.Reactive/InputComponentRegistrationProfile.cs` | `Alis.Reactive/Components/InputRegistration/InputComponentRegistrationProfile.cs` | Keep |
| `Alis.Reactive/ClientValidationRuleBinder.cs` | `Alis.Reactive/Components/InputRegistration/ClientValidationRuleBinder.cs` | Keep |
| `Alis.Reactive/ComponentOnboarding/*` | `Alis.Reactive/Components/Onboarding/*` | Keep |
| `Alis.Reactive/PlanModel/PlanDocument.cs` | `Alis.Reactive/PlanModel/Document/PlanDocument.cs` | Keep |
| `Alis.Reactive/PlanModel/PlanBuildContext.cs` | `Alis.Reactive/PlanModel/Document/PlanBuildContext.cs` | Keep |
| `Alis.Reactive/PlanModel/PlanSerializer.cs` | `Alis.Reactive/PlanModel/Serialization/PlanSerializer.cs` | Keep |
| `Alis.Reactive/PlanModel/PlanJsonWriter.cs` | `Alis.Reactive/PlanModel/Serialization/PlanJsonWriter.cs` | Keep |
| `Alis.Reactive/PlanModel/PlanContractGenerator.cs` | `Alis.Reactive/PlanModel/ContractGeneration/PlanContractGenerator.cs` | Keep |
| `Alis.Reactive/PlanModel/ContractDriftGate.cs` | `Alis.Reactive/PlanModel/ContractGeneration/ContractDriftGate.cs` | Keep |
| `Alis.Reactive/PlanModel/PlanTerms.cs` | `Alis.Reactive/PlanModel/WireTerms/PlanTerms.cs` | Keep |
| `Alis.Reactive/PlanModel/Behavior*.cs` | `Alis.Reactive/PlanModel/Reactions/*` | Keep |
| `Alis.Reactive/PlanModel/ReactionGraph.cs` | `Alis.Reactive/PlanModel/Reactions/ReactionGraph.cs` | Keep |
| `Alis.Reactive/PlanModel/StartsWhen.cs` | `Alis.Reactive/PlanModel/Reactions/StartsWhen.cs` | Keep |
| `Alis.Reactive/PlanModel/RequestPlan.cs` | `Alis.Reactive/PlanModel/Requests/RequestPlan.cs` | Keep |
| `Alis.Reactive/PlanModel/RequestInput.cs` | `Alis.Reactive/PlanModel/Requests/RequestInput.cs` | Keep |
| `Alis.Reactive/PlanModel/GatherRequestInput.cs` | `Alis.Reactive/PlanModel/Requests/GatherRequestInput.cs` | Keep |
| `Alis.Reactive/PlanModel/RegisteredInputValueRead.cs` | `Alis.Reactive/PlanModel/Requests/RegisteredInputValueRead.cs` | Keep |
| `Alis.Reactive/PlanModel/ValueExpression.cs` | `Alis.Reactive/PlanModel/Values/ValueExpression.cs` | Keep |
| `Alis.Reactive/PlanModel/Source.cs` | `Alis.Reactive/PlanModel/Values/Source.cs` | Keep |
| `Alis.Reactive/PlanModel/Path.cs` | `Alis.Reactive/PlanModel/Values/Path.cs` | Keep |
| `Alis.Reactive/PlanModel/Shape*.cs` | `Alis.Reactive/PlanModel/Values/*` | Keep |
| `Alis.Reactive/PlanModel/ConditionGraph.cs` | `Alis.Reactive/PlanModel/Conditions/ConditionGraph.cs` | Keep |
| `Alis.Reactive/PlanModel/Compare*.cs` | `Alis.Reactive/PlanModel/Conditions/*` | Keep |
| `Alis.Reactive/PlanModel/MinimumTextLength.cs` | `Alis.Reactive/PlanModel/Conditions/MinimumTextLength.cs` | Keep |
| `Alis.Reactive/PlanModel/BrowserObject*.cs` | `Alis.Reactive/PlanModel/BrowserObjects/*` | Keep |
| `Alis.Reactive/PlanModel/PluginContract.cs` | `Alis.Reactive/PlanModel/BrowserObjects/PluginContract.cs` | Keep |
| `Alis.Reactive/PlanModel/ValidationJob.cs` | `Alis.Reactive/PlanModel/Validation/ValidationJob.cs` | Keep |

## Runtime Move Matrix

Runtime imports are updated directly by relative path. Do not introduce new
barrel files, path aliases, or generated contract output paths.

| Current path | Target path |
| --- | --- |
| `runtime/core/evaluate.ts` | `runtime/values/evaluate.ts` |
| `runtime/value/array-op-engine.ts` | `runtime/values/array-op-engine.ts` |
| `runtime/core/plugin-catalog.ts` | `runtime/plugins/catalog.ts` |
| `runtime/core/trace.ts` | `runtime/diagnostics/trace.ts` |
| `runtime/core/assert-never.ts` | `runtime/shared/assert-never.ts` |
| `runtime/core/shape-convert.ts` | `runtime/shared/shape-convert.ts` |
| `runtime/core/wire-format.ts` | `runtime/shared/wire-format.ts` |
| `runtime/core/url-template.ts` | `runtime/execution/requests/url-template.ts` |
| `runtime/domain/*` | `runtime/browser-objects/*` |
| `runtime/resolution/*` | `runtime/events/*` |
| `runtime/execution/http*.ts` | `runtime/execution/requests/*` |
| `runtime/execution/gather.ts` | `runtime/execution/requests/gather.ts` |
| `runtime/execution/request-payload-writer.ts` | `runtime/execution/requests/request-payload-writer.ts` |
| `runtime/execution/retry-indicator.ts` | `runtime/execution/requests/retry-indicator.ts` |
| `runtime/execution/execute.ts` | `runtime/execution/reactions/execute.ts` |
| `runtime/execution/trigger.ts` | `runtime/execution/triggers/trigger.ts` |
| `runtime/execution/inject.ts` | `runtime/execution/partials/inject.ts` |
| `runtime/execution/signalr.ts` | `runtime/execution/realtime/signalr.ts` |
| `runtime/execution/server-push.ts` | `runtime/execution/realtime/server-push.ts` |

## Runtime Test Move Matrix

| Current path | Target path |
| --- | --- |
| `runtime/__tests__/array/*` | `runtime/__tests__/values/array/*` |
| `runtime/__tests__/evaluate.test.ts` | `runtime/__tests__/values/evaluate.test.ts` |
| `runtime/__tests__/gather.test.ts` | `runtime/__tests__/execution/requests/gather.test.ts` |
| `runtime/__tests__/gather/*` | `runtime/__tests__/execution/requests/gather/*` |
| `runtime/__tests__/http.test.ts` | `runtime/__tests__/execution/requests/http.test.ts` |
| `runtime/__tests__/execute.test.ts` | `runtime/__tests__/execution/reactions/execute.test.ts` |
| `runtime/__tests__/inject.test.ts` | `runtime/__tests__/execution/partials/inject.test.ts` |
| `runtime/__tests__/realtime-trigger-lifecycle.test.ts` | `runtime/__tests__/execution/realtime/realtime-trigger-lifecycle.test.ts` |
| `runtime/__tests__/plugin-catalog.test.ts` | `runtime/__tests__/plugins/catalog.test.ts` |
| `runtime/__tests__/trigger-wiring/*` | `runtime/__tests__/execution/triggers/*` |

## Playwright Fusion Audit Source

Before moving Playwright files, produce a short inventory from these sources:

- `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/**/*`
- `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/**/*`
- `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Fusion/**/*`

Inventory columns:

```text
slice -> current path -> has Fusion prefix? -> target path
```

Decision for this pass:

- Fusion vertical slices are intentional.
- Grid may stay larger than one view/test/model.
- Playwright paths must carry `Components/Fusion/<Slice>/`.
- Sandbox files are not reorganized except for compile fixes required by moved
  production files.

## Commit Boundaries And Proof

1. `docs: add mechanical organization blueprint`
   - Include this blueprint and the unchanged analysis document if it is not yet
     tracked.
   - Proof: `git diff --check`.

2. `refactor: move plan authoring files`
   - Move only `PlanAuthoring` files.
   - Proof: `dotnet build`.

3. `refactor: move component registration files`
   - Move only `Components` files.
   - Proof: `dotnet test --filter "Component|Input|Validation"` and
     `dotnet build`.

4. `refactor: organize plan model files`
   - Move only `PlanModel` files.
   - Proof: `dotnet build`, contract generation, TS typecheck, and no generated
     `runtime/types/plan.ts` drift unless the generator path update requires it.

5. `refactor: organize runtime executor files`
   - Move runtime implementation files and update relative imports.
   - Proof: runtime asset build, `npm run typecheck`, and vitest.

6. `refactor: organize runtime tests`
   - Move vitest files to mirror runtime organization.
   - Proof: vitest.

7. `refactor: organize Playwright component tests`
   - Move Playwright tests after writing the Fusion inventory.
   - Proof: `scripts/playwright.sh --filter "FullyQualifiedName~Components.Fusion.Grid"`,
     then `scripts/playwright.sh --filter "FullyQualifiedName~Components"`.

8. Final gate
   - Proof: `scripts/test.sh`.

## Review Feedback Protocol

Any review of this blueprint or the implementation must use this format:

```text
Task attempted:
What I looked for:
Finding:
Evidence:
Suggested correction:
```

Feedback that does not state the task attempted or what was searched is treated
as generic context, not blocking implementation feedback.
