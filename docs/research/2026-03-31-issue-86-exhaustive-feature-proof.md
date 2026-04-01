# Issue #86 Exhaustive Feature Proof

## Purpose

This file is the hard proof packet for the issue #86 design thread.

It is not a migration note and not a cleaned-up version of the current draft
schema. It exists to answer one question only:

> What is actually proven by the real DSL, the real runtime, and the real test
> inventory, and what must the final end-state schema therefore express?

## Proof Standard

A feature row counts as proven only when all of the following are true:

1. There is a real code seam that implements it today.
2. There is fresh executable evidence from this pass, or a direct descriptor /
   serializer proof where runtime execution is not the right surface.
3. The row maps to a concrete schema responsibility instead of relying on
   hidden runtime invention.

The rows below therefore distinguish:

- runtime proof
- DSL / serializer proof
- schema obligation
- lifecycle truth

## Fresh Proof Runs

The following commands were executed in this worktree on **March 31, 2026**,
with targeted and broader TS reruns on **April 1, 2026** after the array/object
walking and same-trigger ordering proofs were added.

```bash
npm test -- \
  Alis.Reactive.SandboxApp/Scripts/__tests__/when-triggering-on-dom-ready.test.ts \
  Alis.Reactive.SandboxApp/Scripts/__tests__/when-dom-ready-chains-into-custom-event.test.ts \
  Alis.Reactive.SandboxApp/Scripts/__tests__/when-triggering-on-component-event.test.ts \
  Alis.Reactive.SandboxApp/Scripts/__tests__/when-dispatching-a-custom-event-with-payload.test.ts \
  Alis.Reactive.SandboxApp/Scripts/__tests__/when-proving-custom-event-trigger-algebra.test.ts \
  Alis.Reactive.SandboxApp/Scripts/__tests__/when-reading-component-value.test.ts \
  Alis.Reactive.SandboxApp/Scripts/__tests__/when-evaluating-guards.test.ts \
  Alis.Reactive.SandboxApp/Scripts/__tests__/when-gathering-form-values.test.ts \
  Alis.Reactive.SandboxApp/Scripts/__tests__/when-executing-http-verbs-and-error-routing.test.ts \
  Alis.Reactive.SandboxApp/Scripts/__tests__/when-chaining-http-requests.test.ts \
  Alis.Reactive.SandboxApp/Scripts/__tests__/when-executing-parallel-requests.test.ts \
  Alis.Reactive.SandboxApp/Scripts/__tests__/when-validating-form-fields.test.ts \
  Alis.Reactive.SandboxApp/Scripts/__tests__/when-validating-array-components.test.ts \
  Alis.Reactive.SandboxApp/Scripts/__tests__/when-enriching-after-merge.test.ts \
  Alis.Reactive.SandboxApp/Scripts/__tests__/when-removing-event-listeners.test.ts \
  Alis.Reactive.SandboxApp/Scripts/__tests__/when-consuming-command-values.test.ts \
  Alis.Reactive.SandboxApp/Scripts/__tests__/when-triggering-on-server-push.test.ts \
  Alis.Reactive.SandboxApp/Scripts/__tests__/when-triggering-on-signalr.test.ts \
  Alis.Reactive.SandboxApp/Scripts/__tests__/when-proving-server-push-pipeline-semantics.test.ts \
  Alis.Reactive.SandboxApp/Scripts/__tests__/when-proving-signalr-pipeline-semantics.test.ts \
  Alis.Reactive.SandboxApp/Scripts/__tests__/when-executing-multiple-entries-same-trigger.test.ts \
  Alis.Reactive.SandboxApp/Scripts/__tests__/when-executing-reactions-end-to-end.test.ts \
  Alis.Reactive.SandboxApp/Scripts/__tests__/when-http-handlers-contain-nested-reactions.test.ts \
  Alis.Reactive.SandboxApp/Scripts/__tests__/when-using-conditions-in-response-handlers.test.ts \
  Alis.Reactive.SandboxApp/Scripts/__tests__/when-proving-response-and-component-algebra.test.ts \
  Alis.Reactive.SandboxApp/Scripts/__tests__/when-proving-canonical-proof-surfaces.test.ts

dotnet test tests/Alis.Reactive.UnitTests/Alis.Reactive.UnitTests.csproj \
  --filter "FullyQualifiedName~WhenTriggeringOnDomReady|FullyQualifiedName~WhenTriggeringOnCustomEvent|FullyQualifiedName~WhenTriggeringOnServerPush|FullyQualifiedName~WhenTriggeringOnSignalR|FullyQualifiedName~WhenRequestingFromServer|FullyQualifiedName~WhenGatheringRegisteredComponents|FullyQualifiedName~WhenBuildingHttpPipelines|FullyQualifiedName~WhenConditionReadsComponent|FullyQualifiedName~WhenUsingConditionsInEveryDslSurface|FullyQualifiedName~WhenEnrichingValidationAtRenderTime|FullyQualifiedName~WhenRegisteringComponents|FullyQualifiedName~WhenGeneratingUniqueIds"

dotnet test tests/Alis.Reactive.FluentValidator.UnitTests/Alis.Reactive.FluentValidator.UnitTests.csproj \
  --filter "FullyQualifiedName~WhenExtractingAllRuleTypes|FullyQualifiedName~WhenExtractingConditionalRules|FullyQualifiedName~WhenExtractingEqualToRules|FullyQualifiedName~WhenExtractingNestedValidators|FullyQualifiedName~WhenExtractingDateOnlyRules|FullyQualifiedName~WhenExtractingComparisonRules|FullyQualifiedName~WhenExtractingLengthRules|FullyQualifiedName~WhenExtractingRangeRule"

dotnet test tests/Alis.Reactive.Native.UnitTests/Alis.Reactive.Native.UnitTests.csproj \
  --filter "FullyQualifiedName~WhenDescribingNativeCheckListEvents|FullyQualifiedName~WhenDescribingNativeDropDownEvents|FullyQualifiedName~WhenDescribingNativeRadioGroupEvents|FullyQualifiedName~WhenDescribingNativeHiddenFieldEvents"

dotnet test tests/Alis.Reactive.Fusion.UnitTests/Alis.Reactive.Fusion.UnitTests.csproj \
  --filter "FullyQualifiedName~WhenDescribingFusionNumericTextBoxEvents|FullyQualifiedName~WhenDescribingFusionDropDownListEvents|FullyQualifiedName~WhenDescribingFusionFileUploadEvents|FullyQualifiedName~WhenDescribingFusionDateRangePickerEvents"

dotnet test tests/Alis.Reactive.Native.UnitTests/Alis.Reactive.Native.UnitTests.csproj \
  --filter "FullyQualifiedName~WhenSerializingANativeActionLink"

dotnet test tests/Alis.Reactive.Analyzers.Tests/Alis.Reactive.Analyzers.Tests.csproj \
  --filter "FullyQualifiedName~WhenEnforcingNativeActionLinkSingleRequest"
```

Fresh results:

- TS runtime sweep: **22 files, 368 tests passed**
- canonical proof-surface harness: **1 file, 4 tests passed**
- focused proof-algebra sweep including the canonical harness: **5 files, 23 tests passed**
- focused trigger/order sweep after same-trigger chaining fix: **5 files, 29 tests passed**
- broader active TS runtime sweep after ordering fix: **78 files, 1,149 tests passed**
- core reactive C# sweep: **80 tests passed**
- FluentValidation extraction sweep: **23 tests passed**
- native descriptor sweep: **16 tests passed**
- fusion descriptor sweep: **16 tests passed**
- native action-link serializer sweep: **9 tests passed**
- native action-link analyzer sweep: **9 tests passed**

One stale suite still fails to parse and remains explicitly non-authoritative
for issue #86:

- [when-proving-end-state-schema.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-proving-end-state-schema.test.ts)
  via
  [end-state-plan-fixtures.ts](../../Alis.Reactive.SandboxApp/Scripts/architecture-proof/end-state-plan-fixtures.ts)

Total fresh passing tests used in this proof packet: **1,331**

## Cross-Cutting Runtime Truths Proven By Code

### 1. Deterministic bound ids are real

The framework already generates deterministic ids from property expressions:

- [IdGenerator.cs](../../Alis.Reactive/IdGenerator.cs)
- [ExpressionPathHelper.cs](../../Alis.Reactive/ExpressionPathHelper.cs)
- [WhenGeneratingUniqueIds.cs](../../tests/Alis.Reactive.UnitTests/Schema/WhenGeneratingUniqueIds.cs)

This matters because validation, `IncludeAll`, and binding-value gathering need
a canonical component join.

### 2. Binding participation and `.Reactive(...)` triggers are not the same object

The current system already separates:

- field registration:
  - [ReactivePlan.cs](../../Alis.Reactive/ReactivePlan.cs)
  - [ComponentRegistration.cs](../../Alis.Reactive/ComponentRegistration.cs)
- event-trigger wiring:
  - [ComponentEventTrigger.cs](../../Alis.Reactive/Descriptors/Triggers/ComponentEventTrigger.cs)
  - [trigger.ts](../../Alis.Reactive.SandboxApp/Scripts/execution/trigger.ts)

They may share the same generated id, but they do not share responsibility.

### 3. Explicit non-input and app-level component refs are already first-class

The C# DSL already distinguishes:

- `IComponent` surface refs
- `IInputComponent` refs with `ReadExpr`
- `IAppLevelComponent` refs with `DefaultId`

Proof:

- [IComponent.cs](../../Alis.Reactive/IComponent.cs)
- [ComponentRef.cs](../../Alis.Reactive/ComponentRef.cs)
- [PipelineBuilder.cs](../../Alis.Reactive/Builders/PipelineBuilder.cs)
- [FusionTab.cs](../../Alis.Reactive.Fusion/Components/FusionTab/FusionTab.cs)
- [FusionTabExtensions.cs](../../Alis.Reactive.Fusion/Components/FusionTab/FusionTabExtensions.cs)
- [FusionAccordion.cs](../../Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordion.cs)
- [FusionToast.cs](../../Alis.Reactive.Fusion/AppLevel/FusionToast/FusionToast.cs)
- [FusionToastExtensions.cs](../../Alis.Reactive.Fusion/AppLevel/FusionToast/FusionToastExtensions.cs)

This forces the schema to keep component surface identity and optional binding
participation separate, without inventing a second top-level registry family.

### 3a. A controlled native/fusion proof harness now exists

The issue #86 work no longer depends only on scattered real widgets to prove the
runtime algebra. There is now one controlled native surface and one controlled
fusion surface with the same API:

- [proof-surfaces.ts](../../Alis.Reactive.SandboxApp/Scripts/components/lab/proof-surfaces.ts)
- [when-proving-canonical-proof-surfaces.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-proving-canonical-proof-surfaces.test.ts)

That harness proves the shared algebra can be exercised cleanly across both
vendors for:

- nested member walking
- command-driven property writes
- command-driven method calls with and without args
- component-event-driven downstream reads and conditions

### 4. The runtime is root-first

The real runtime mechanics are still:

1. resolve a root object
2. execute access steps from that root
3. obtain a raw JS value
4. shape if needed
5. consume

Proof:

- [component.ts](../../Alis.Reactive.SandboxApp/Scripts/resolution/component.ts)
- [walk.ts](../../Alis.Reactive.SandboxApp/Scripts/core/walk.ts)
- [coerce.ts](../../Alis.Reactive.SandboxApp/Scripts/core/coerce.ts)
- [values.ts](../../Alis.Reactive.SandboxApp/Scripts/execution/values.ts)

### 5. Request is a true DSL unit

The public DSL already exposes a request as one complete unit with stages:

- `Gather`
- `AsJson` / `AsFormData`
- `WhileLoading`
- `Validate`
- `Response`
- `Response.OnSuccess`
- `Response.OnError`
- `Response.Chained`
- `Parallel.OnAllSettled`

Proof:

- [HttpRequestBuilder.cs](../../Alis.Reactive/Builders/Requests/HttpRequestBuilder.cs)
- [ResponseBuilder.cs](../../Alis.Reactive/Builders/Requests/ResponseBuilder.cs)
- [ParallelBuilder.cs](../../Alis.Reactive/Builders/Requests/ParallelBuilder.cs)
- [http.ts](../../Alis.Reactive.SandboxApp/Scripts/execution/http.ts)

### 5a. Outer pipeline order is real

The public DSL and builders already prove mixed outer ordering like:

- `When -> Request -> When`
- `Request -> When -> Request`
- `When -> Parallel -> When`

Proof:

- [PipelineBuilder.cs](../../Alis.Reactive/Builders/PipelineBuilder.cs)
- [WhenMixingConditionsWithHttp.cs](../../tests/Alis.Reactive.UnitTests/Architecture/WhenMixingConditionsWithHttp.cs)
- [WhenUsingConditionsInsideResponseHandlers.cs](../../tests/Alis.Reactive.UnitTests/Architecture/WhenUsingConditionsInsideResponseHandlers.cs)

This forces `Pipeline` to preserve ordered steps instead of collapsing to one
structural stage.

### 5b. Trigger kind does not change pipeline semantics

`DomReady`, `CustomEvent`, `ServerPush`, and `SignalR` all feed the same
`PipelineBuilder<TModel>` path in C#.

Proof:

- [TriggerBuilder.cs](../../Alis.Reactive/Builders/TriggerBuilder.cs)
  - `DomReady(Action<PipelineBuilder<TModel>>)`
  - `CustomEvent(..., Action<PipelineBuilder<TModel>>)`
  - `ServerPush(..., Action<PipelineBuilder<TModel>>)`
  - `SignalR(..., Action<PipelineBuilder<TModel>>)`
  - all of them call `AddEntryWithContexts(...)`
  - `AddEntryWithContexts(...)` calls `pb.BuildReactions()`
- [WhenTriggeringOnServerPush.cs](../../tests/Alis.Reactive.UnitTests/Triggers/WhenTriggeringOnServerPush.cs)
- [WhenTriggeringOnSignalR.cs](../../tests/Alis.Reactive.UnitTests/Triggers/WhenTriggeringOnSignalR.cs)
- [server-push.ts](../../Alis.Reactive.SandboxApp/Scripts/execution/server-push.ts)
- [signalr.ts](../../Alis.Reactive.SandboxApp/Scripts/execution/signalr.ts)

So:

- trigger kind chooses attachment surface and carried payload root
- pipeline semantics after attachment stay universal
- SSE and SignalR are not second-class behavioral lanes

Fresh direct runtime proof now exists too:

- [when-proving-server-push-pipeline-semantics.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-proving-server-push-pipeline-semantics.test.ts)
- [when-proving-signalr-pipeline-semantics.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-proving-signalr-pipeline-semantics.test.ts)

Those two tests prove more than attachment:

- condition -> request -> condition ordering is preserved for one trigger
  occurrence
- nested `onSuccess` conditions still work in the same chain
- component reads/writes/calls still use the same schema/runtime semantics
  inside SSE and SignalR pipelines

### 6. Validation is rules plus a join to live components

Proof:

- [ValidationResolver.cs](../../Alis.Reactive/Resolvers/ValidationResolver.cs)
- [orchestrator.ts](../../Alis.Reactive.SandboxApp/Scripts/validation/orchestrator.ts)
- [WhenEnrichingValidationAtRenderTime.cs](../../tests/Alis.Reactive.UnitTests/ValidationEnrichment/WhenEnrichingValidationAtRenderTime.cs)
- [when-enriching-after-merge.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-enriching-after-merge.test.ts)

The emitted validation contract should therefore stay target-and-rules only,
with runtime lookup happening through the canonical component registry and
optional `binding`.

## Exhaustive Feature Matrix

| Feature / workflow | Fresh runtime proof | Fresh DSL / serializer proof | Schema obligation forced by the proof | Self-sufficient at wire time? | Lazy resolution allowed? | Verdict |
|---|---|---|---|---|---|---|
| dom-ready | [when-triggering-on-dom-ready.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-triggering-on-dom-ready.test.ts), [when-dom-ready-chains-into-custom-event.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-dom-ready-chains-into-custom-event.test.ts) | [WhenTriggeringOnDomReady.cs](../../tests/Alis.Reactive.UnitTests/Triggers/WhenTriggeringOnDomReady.cs) | `Reaction.on = domReady` | yes | no | proven |
| custom event with payload | [when-dispatching-a-custom-event-with-payload.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-dispatching-a-custom-event-with-payload.test.ts), [when-dom-ready-chains-into-custom-event.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-dom-ready-chains-into-custom-event.test.ts), [when-proving-custom-event-trigger-algebra.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-proving-custom-event-trigger-algebra.test.ts) | [WhenTriggeringOnCustomEvent.cs](../../tests/Alis.Reactive.UnitTests/Triggers/WhenTriggeringOnCustomEvent.cs) | explicit document-level trigger plus explicit trigger payload root | yes | no | proven |
| native readable component event | [when-triggering-on-component-event.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-triggering-on-component-event.test.ts) | [WhenDescribingNativeDropDownEvents.cs](../../tests/Alis.Reactive.Native.UnitTests/Components/NativeDropDown/WhenDescribingNativeDropDownEvents.cs), [WhenDescribingNativeCheckListEvents.cs](../../tests/Alis.Reactive.Native.UnitTests/Components/NativeCheckList/WhenDescribingNativeCheckListEvents.cs) | component trigger must carry target identity and explicit payload/value semantics | yes | no | proven |
| native non-readable component event | [when-triggering-on-component-event.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-triggering-on-component-event.test.ts) | current non-readable path is visible in [trigger.ts](../../Alis.Reactive.SandboxApp/Scripts/execution/trigger.ts) and native button extensions in [NativeButtonReactiveExtensions.cs](../../Alis.Reactive.Native/Components/NativeButton/NativeButtonReactiveExtensions.cs) | trigger payload must support `none` explicitly | yes | no | proven |
| fusion callback input event | [when-triggering-on-component-event.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-triggering-on-component-event.test.ts) | [WhenDescribingFusionNumericTextBoxEvents.cs](../../tests/Alis.Reactive.Fusion.UnitTests/Components/FusionNumericTextBox/WhenDescribingFusionNumericTextBoxEvents.cs), [WhenDescribingFusionDropDownListEvents.cs](../../tests/Alis.Reactive.Fusion.UnitTests/Components/FusionDropDownList/WhenDescribingFusionDropDownListEvents.cs) | trigger payload must support host callback args | yes | no | proven |
| fusion callback widget event | runtime widget-shape behavior is covered by the fusion callback path in [when-triggering-on-component-event.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-triggering-on-component-event.test.ts) | [FusionTabEvents.cs](../../Alis.Reactive.Fusion/Components/FusionTab/FusionTabEvents.cs) proves typed `selected` callback args for a non-input widget | same component-trigger family must support non-input fusion callback payloads | yes | no | proven |
| conditions | [when-evaluating-guards.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-evaluating-guards.test.ts), [when-using-conditions-in-response-handlers.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-using-conditions-in-response-handlers.test.ts) | [WhenUsingConditionsInEveryDslSurface.cs](../../tests/Alis.Reactive.UnitTests/Conditions/WhenUsingConditionsInEveryDslSurface.cs) | first-class `When` / guard tree over the same value language | yes | mixed | proven |
| confirm guards | [when-evaluating-guards.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-evaluating-guards.test.ts) | [WhenUsingConditionsInEveryDslSurface.cs](../../tests/Alis.Reactive.UnitTests/Conditions/WhenUsingConditionsInEveryDslSurface.cs) | confirm is a normal guard, not a side API | yes | no | proven |
| dispatch across scope | [when-executing-reactions-end-to-end.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-executing-reactions-end-to-end.test.ts), [when-dispatching-a-custom-event-with-payload.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-dispatching-a-custom-event-with-payload.test.ts) | [WhenDispatchingAnEvent.cs](../../tests/Alis.Reactive.UnitTests/Commands/WhenDispatchingAnEvent.cs) | explicit `dispatch` command is the only scope-crossing lane | yes | no | proven |
| component reads outside events | [when-reading-component-value.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-reading-component-value.test.ts), [when-evaluating-guards.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-evaluating-guards.test.ts), [when-proving-response-and-component-algebra.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-proving-response-and-component-algebra.test.ts) | [WhenConditionReadsComponent.cs](../../tests/Alis.Reactive.UnitTests/Conditions/WhenConditionReadsComponent.cs) | one shared root/access-step read model must support explicit component roots, including array/object member walking | no | no | proven |
| request gather from literal | [when-gathering-form-values.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-gathering-form-values.test.ts) | [WhenBuildingHttpPipelines.cs](../../tests/Alis.Reactive.UnitTests/Http/WhenBuildingHttpPipelines.cs), [WhenRequestingFromServer.cs](../../tests/Alis.Reactive.UnitTests/Requests/WhenRequestingFromServer.cs) | `Request.gather[]` must accept literal value items | no | no | proven |
| request gather from trigger payload | [when-gathering-form-values.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-gathering-form-values.test.ts) | current trigger/path lowering is visible in [TypedEventDescriptor.cs](../../Alis.Reactive/TypedEventDescriptor.cs) and [ExpressionPathHelper.cs](../../Alis.Reactive/ExpressionPathHelper.cs) | `Request.gather[]` must accept values read from trigger scope | no | no | proven |
| request gather from response payload | [when-chaining-http-requests.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-chaining-http-requests.test.ts) | [ResponseBody.cs](../../Alis.Reactive/ResponseBody.cs), [WhenBuildingHttpPipelines.cs](../../tests/Alis.Reactive.UnitTests/Http/WhenBuildingHttpPipelines.cs) | `Request.response.chained` must keep response scope readable | no | no | proven |
| request gather from component | [when-gathering-form-values.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-gathering-form-values.test.ts) | [WhenRequestingFromServer.cs](../../tests/Alis.Reactive.UnitTests/Requests/WhenRequestingFromServer.cs) | same gather family must accept component-root reads | no | no | proven |
| request unit snapshot immutability | [when-executing-http-end-to-end.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-executing-http-end-to-end.test.ts) | [http.ts](../../Alis.Reactive.SandboxApp/Scripts/execution/http.ts) | gather resolves once into a frozen transport snapshot; later source mutation must not rewrite that request unit | yes | no | proven |
| IncludeAll | [when-gathering-form-values.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-gathering-form-values.test.ts) | [WhenGatheringRegisteredComponents.cs](../../tests/Alis.Reactive.UnitTests/Requests/WhenGatheringRegisteredComponents.cs), [WhenRegisteringComponents.cs](../../tests/Alis.Reactive.UnitTests/ValidationEnrichment/WhenRegisteringComponents.cs) | `Request.gather.includeAll` must walk the component registry and keep only components with `binding`, not invent a second value lane | no | yes | proven |
| GET query sink | [when-executing-http-verbs-and-error-routing.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-executing-http-verbs-and-error-routing.test.ts) | [WhenBuildingHttpPipelines.cs](../../tests/Alis.Reactive.UnitTests/Http/WhenBuildingHttpPipelines.cs) | `Request.method = GET` plus request sink/body mode | no | no | proven |
| JSON body sink | [when-gathering-form-values.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-gathering-form-values.test.ts), [when-executing-http-verbs-and-error-routing.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-executing-http-verbs-and-error-routing.test.ts) | [HttpRequestBuilder.cs](../../Alis.Reactive/Builders/Requests/HttpRequestBuilder.cs) | `Request.as = json` stays part of the request unit | no | no | proven |
| form-data sink | [when-gathering-form-values.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-gathering-form-values.test.ts) | [WhenDescribingFusionFileUploadEvents.cs](../../tests/Alis.Reactive.Fusion.UnitTests/Components/FusionFileUpload/WhenDescribingFusionFileUploadEvents.cs), [HttpRequestBuilder.cs](../../Alis.Reactive/Builders/Requests/HttpRequestBuilder.cs) | `Request.as = formData` stays part of the request unit | no | no | proven |
| success and error handlers | [when-executing-http-verbs-and-error-routing.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-executing-http-verbs-and-error-routing.test.ts), [when-http-handlers-contain-nested-reactions.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-http-handlers-contain-nested-reactions.test.ts), [when-using-conditions-in-response-handlers.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-using-conditions-in-response-handlers.test.ts), [when-proving-response-and-component-algebra.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-proving-response-and-component-algebra.test.ts) | [ResponseBuilder.cs](../../Alis.Reactive/Builders/Requests/ResponseBuilder.cs), [WhenBuildingHttpPipelines.cs](../../tests/Alis.Reactive.UnitTests/Http/WhenBuildingHttpPipelines.cs) | `Request.response.onSuccess[]` and `Request.response.onError[]` must exist as request stages, with response payload readable as a carried root through the same access-step language | no | no | proven |
| chained requests | [when-chaining-http-requests.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-chaining-http-requests.test.ts) | [ResponseBuilder.cs](../../Alis.Reactive/Builders/Requests/ResponseBuilder.cs), [WhenRequestingFromServer.cs](../../tests/Alis.Reactive.UnitTests/Requests/WhenRequestingFromServer.cs) | `Request.response.chained` must remain a first-class request stage | no | no | proven |
| parallel requests | [when-executing-parallel-requests.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-executing-parallel-requests.test.ts) | [ParallelBuilder.cs](../../Alis.Reactive/Builders/Requests/ParallelBuilder.cs), [WhenBuildingHttpPipelines.cs](../../tests/Alis.Reactive.UnitTests/Http/WhenBuildingHttpPipelines.cs) | separate `Parallel` unit with `onAllSettled` | no | no | proven |
| validation | [when-validating-form-fields.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-validating-form-fields.test.ts), [when-validating-array-components.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-validating-array-components.test.ts) | [ValidationResolver.cs](../../Alis.Reactive/Resolvers/ValidationResolver.cs), [WhenExtractingAllRuleTypes.cs](../../tests/Alis.Reactive.FluentValidator.UnitTests/WhenExtractingAllRuleTypes.cs), [WhenExtractingConditionalRules.cs](../../tests/Alis.Reactive.FluentValidator.UnitTests/WhenExtractingConditionalRules.cs) | one validation family: targets + rules + conditions, joined to live components through `binding` | no | yes | proven |
| partial validation lifecycle | [when-enriching-after-merge.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-enriching-after-merge.test.ts) | [WhenEnrichingValidationAtRenderTime.cs](../../tests/Alis.Reactive.UnitTests/ValidationEnrichment/WhenEnrichingValidationAtRenderTime.cs) | validation lookup must remain lazy against merged component registry, filtered to `binding` participants | no | yes | proven |
| partial component-event reload lifecycle | [when-removing-event-listeners.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-removing-event-listeners.test.ts), [when-enriching-after-merge.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-enriching-after-merge.test.ts) | [merge-plan.ts](../../Alis.Reactive.SandboxApp/Scripts/lifecycle/merge-plan.ts) is the operative runtime seam | component event triggers must be self-sufficient at wire time | yes | no | proven |
| multiple plans with clean merge isolation | [when-managing-plan-lifecycle.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-managing-plan-lifecycle.test.ts), [when-merging-plans.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-merging-plans.test.ts) | [merge-plan.ts](../../Alis.Reactive.SandboxApp/Scripts/lifecycle/merge-plan.ts) | fragment ownership must be scoped by `planId + sourceId`; merges/removals in one plan must not tear down another plan | yes | yes | proven |
| payload mutation helpers | [when-consuming-command-values.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-consuming-command-values.test.ts), [when-executing-reactions-end-to-end.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-executing-reactions-end-to-end.test.ts), [when-proving-custom-event-trigger-algebra.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-proving-custom-event-trigger-algebra.test.ts) | current command lanes are visible in [commands.ts](../../Alis.Reactive.SandboxApp/Scripts/execution/commands.ts) and [WhenSerializingUnifiedCommandValues.cs](../../tests/Alis.Reactive.UnitTests/Commands/WhenSerializingUnifiedCommandValues.cs) | payload mutation targets must be explicit; response stays a readable root in the base algebra | yes | no | proven |
| SSE | [when-triggering-on-server-push.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-triggering-on-server-push.test.ts) | [WhenTriggeringOnServerPush.cs](../../tests/Alis.Reactive.UnitTests/Triggers/WhenTriggeringOnServerPush.cs), [server-push.ts](../../Alis.Reactive.SandboxApp/Scripts/execution/server-push.ts) | explicit SSE trigger with carried host message payload | yes | no | proven |
| SignalR | [when-triggering-on-signalr.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-triggering-on-signalr.test.ts) | [WhenTriggeringOnSignalR.cs](../../tests/Alis.Reactive.UnitTests/Triggers/WhenTriggeringOnSignalR.cs), [signalr.ts](../../Alis.Reactive.SandboxApp/Scripts/execution/signalr.ts) | explicit SignalR trigger with carried host callback payload | yes | no | proven |
| native action-link as a constrained projection | runtime browser plan is not the right surface; proof is serializer + analyzer | [WhenSerializingANativeActionLink.cs](../../tests/Alis.Reactive.Native.UnitTests/Components/NativeActionLink/WhenSerializingANativeActionLink.cs), [WhenEnforcingNativeActionLinkSingleRequest.cs](../../tests/Alis.Reactive.Analyzers.Tests/NativeActionLink/WhenEnforcingNativeActionLinkSingleRequest.cs) | constrained projection over the same guard + request + response core, not a second top-level reactive family | yes | no | proven |

## What This Proof Forces The Schema To Admit

### 1. There must be a component registry with optional binding participation

`IncludeAll`, validation enrichment, and lazy reads after partial merge all prove
the runtime needs one canonical component registry carrying:

- deterministic field identity
- vendor/root identity
- optional semantic binding name
- canonical semantic value access when the component participates in binding

This is not the same concern as `.Reactive(...)`.

### 2. There must be a self-sufficient trigger contract

Component triggers, custom events, SSE, and SignalR all prove that a reaction
must carry its own attachment and payload rules. Trigger payload shape cannot be
invented later in the runtime.

### 3. There must be one value-access family

Request gather, guard inputs, dispatch payloads, response reads, payload
mutation, and validation reads all prove the same value law:

1. get a root
2. walk a path
3. obtain a raw JS value
4. shape if needed
5. consume

The final schema should therefore use one shared value/read language, not
different request-value, command-value, and validation-value DTO families.

### 4. Request must stay a first-class unit

The public DSL and runtime already prove the stable request stages:

- `gather`
- `as`
- `whileLoading`
- `validate`
- `response.onSuccess`
- `response.onError`
- `response.chained`

`parallel` is its own unit with `onAllSettled`.

### 5. Validation must stay pure

FluentValidation extraction plus runtime enrichment proves the emitted
validation contract should stay:

- target identity
- rules
- peer references
- conditions

It should not carry copied vendor/path/runtime scoping metadata.

## Stale Artifacts That Must Not Be Treated As Authority

These files are not safe sources of truth for the issue #86 end-state design:

- [when-proving-end-state-schema.test.ts](../../Alis.Reactive.SandboxApp/Scripts/__tests__/when-proving-end-state-schema.test.ts)
- [end-state-plan-fixtures.ts](../../Alis.Reactive.SandboxApp/Scripts/architecture-proof/end-state-plan-fixtures.ts)
- [end-state-plan-types.ts](../../Alis.Reactive.SandboxApp/Scripts/architecture-proof/end-state-plan-types.ts)

Why:

- they still encode draft-era nouns like `entries`, `reaction.kind = http`,
  `componentType`, `readExpr`, and `elementShape`
- they still assume old payload and mutation shapes that the live serializer no
  longer emits

The same warning applies to some verify-based native/fusion wiring snapshot
tests. In this pass, those suites failed because their `.verified.txt` files
still expect older flattened mutation JSON, while the current serializer emits
the value under `mutation.value`.

That is a stale proof-artifact problem, not a supported-feature gap.

## Final Conclusion

Every required feature/workflow in the issue #86 list is proven by at least one
real code seam and at least one fresh executable proof surface in this pass.

What is *not* proven is the old draft end-state schema harness. That harness is
stale and should be replaced, not defended.

The final schema therefore needs to be designed from the runtime truths above,
not by patching the current proof fixtures forward.
