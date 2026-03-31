# Dispatch Payloads Should Join The Existing Source/Value Model

## Capability Matrix

Public support means a normal fluent C# caller can reach the capability without
hand-authoring descriptors or raw JSON.

| Capability | Public fluent DSL support | Descriptor / schema support | Runtime execution support | Real sandbox / test evidence | Status | Notes / caveats |
| --- | --- | --- | --- | --- | --- | --- |
| Read event payload prop | Yes | Yes: event payload reads lower to `EventSource` paths (`Alis.Reactive/Builders/Conditions/TypedSource.cs`, `Alis.Reactive/Descriptors/Sources/BindSource.cs`) | Yes: runtime resolves event sources by walking execution context (`Alis.Reactive.SandboxApp/Scripts/resolution/resolver.ts`) | `Alis.Reactive/Builders/PipelineBuilder.Conditions.cs`; `Alis.Reactive/Builders/ElementBuilder.cs`; `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/CoreBehaviors/Payload/Index.cshtml`; `tests/Alis.Reactive.UnitTests/Commands/WhenResolvingPayloadSource.cs`; `Alis.Reactive.SandboxApp/Scripts/__tests__/when-wiring-event-with-payload.test.ts` | Supported | This already works for typed custom-event payloads and typed component-event payloads. |
| Read component prop / value | Yes | Yes: component reads are first-class via `ComponentSource` / `TypedComponentSource<T>` (`Alis.Reactive/Builders/Conditions/TypedComponentSource.cs`, `Alis.Reactive/Descriptors/Sources/BindSource.cs`) | Yes: runtime resolves vendor root, then reads `readExpr` (`Alis.Reactive.SandboxApp/Scripts/resolution/component.ts`, `Alis.Reactive.SandboxApp/Scripts/resolution/resolver.ts`) | `Alis.Reactive.Fusion/Components/FusionDropDownList/FusionDropDownListExtensions.cs`; `Alis.Reactive.Fusion/Components/TestWidgetSyncFusion/TestWidgetSyncFusionExtensions.cs`; `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/AllModulesTogether/TestWidget/Index.cshtml`; `tests/Alis.Reactive.UnitTests/Conditions/WhenComparingTwoSources.cs`; `tests/Alis.Reactive.PlaywrightTests/AllModulesTogether/CrossVendor/WhenComponentApiExercisedEndToEnd.cs` | Supported | `readExpr` is not special architecture. It is member access after vendor-agnostic root resolution. |
| Write element prop | Yes | Yes: `MutateElementCommand` + `SetPropMutation` already accept literal value or source (`Alis.Reactive/Descriptors/Commands/MutateElementCommand.cs`, `Alis.Reactive/Descriptors/Mutations/Mutation.cs`) | Yes: runtime resolves source or uses literal, then applies optional `coerce` (`Alis.Reactive.SandboxApp/Scripts/execution/element.ts`) | `Alis.Reactive/Builders/ElementBuilder.cs`; `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/CoreBehaviors/Payload/Index.cshtml`; `tests/Alis.Reactive.DriftDetection.Tests/Behavior/WhenMutatingResidentUiState.cs`; `Alis.Reactive.SandboxApp/Scripts/__tests__/when-walking-source-into-mutations.test.ts` | Supported | Element writes are the broadest public source consumer today. |
| Write component prop | Partial | Yes: component refs lower into the same generic mutation sink (`Alis.Reactive/ComponentRef.cs`, `Alis.Reactive/Descriptors/Commands/MutateElementCommand.cs`) | Yes: runtime treats component writes as vendor-rooted element mutations (`Alis.Reactive.SandboxApp/Scripts/execution/element.ts`) | `Alis.Reactive.Fusion/Components/FusionDropDownList/FusionDropDownListExtensions.cs`; `Alis.Reactive.Native/Components/NativeHiddenField/NativeHiddenFieldExtensions.cs`; `Alis.Reactive.Fusion/Components/TestWidgetSyncFusion/TestWidgetSyncFusionExtensions.cs`; `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/AllModulesTogether/TestWidget/Index.cshtml`; `tests/Alis.Reactive.PlaywrightTests/AllModulesTogether/CrossVendor/WhenComponentApiExercisedEndToEnd.cs` | Partial | Literal writes are common. Event / response-body writes exist widely. Public typed component-source writes exist only on narrower surfaces today. |
| Write event payload prop | Partial | Yes: `MutateEventCommand` supports `SetPropMutation` plus command-level source (`Alis.Reactive/Descriptors/Commands/MutateEventCommand.cs`) | Yes: runtime assigns into `ctx.evt` after optional source resolution and `coerce` (`Alis.Reactive.SandboxApp/Scripts/execution/commands.ts`) | `Alis.Reactive.Fusion/Components/FusionAutoComplete/Events/FusionAutoCompleteOnFiltering.cs`; `Alis.Reactive.Fusion/Components/FusionMultiSelect/Events/FusionMultiSelectOnFiltering.cs`; `tests/Alis.Reactive.DriftDetection.Tests/Behavior/WhenMutatingResidentUiState.cs` | Partial | Public DSL exposure is specialized, not generic. |
| Call element / component method without args | Yes | Yes: `CallMutation` models this generically (`Alis.Reactive/Descriptors/Mutations/Mutation.cs`) | Yes: runtime invokes and ignores return value (`Alis.Reactive.SandboxApp/Scripts/execution/element.ts`) | `Alis.Reactive/Builders/ElementBuilder.cs`; `Alis.Reactive.Fusion/Components/FusionDropDownList/FusionDropDownListExtensions.cs`; `Alis.Reactive.Fusion/Components/TestWidgetSyncFusion/TestWidgetSyncFusionExtensions.cs`; `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/AllModulesTogether/TestWidget/Index.cshtml`; `Alis.Reactive.SandboxApp/Scripts/__tests__/when-using-unified-call-mutations.test.ts`; `tests/Alis.Reactive.PlaywrightTests/AllModulesTogether/CrossVendor/WhenComponentApiExercisedEndToEnd.cs` | Supported | Supported through specialized helpers, not a general arbitrary-call fluent API. |
| Call element / component method with literal args | Yes | Yes: `CallMutation.Args` already supports literal args (`Alis.Reactive/Descriptors/Mutations/MethodArg.cs`) | Yes: runtime resolves literal args and invokes (`Alis.Reactive.SandboxApp/Scripts/execution/element.ts`) | `Alis.Reactive/Builders/ElementBuilder.cs`; `tests/Alis.Reactive.DriftDetection.Tests/Behavior/WhenMutatingResidentUiState.cs`; `Alis.Reactive.SandboxApp/Scripts/__tests__/when-using-unified-call-mutations.test.ts`; `Alis.Reactive.SandboxApp/Scripts/__tests__/when-walking-source-into-mutations.test.ts` | Supported | Public DSL reaches this via helpers like class-list operations and other fixed component methods. |
| Call element / component method with source args | Partial | Yes: `SourceArg` is a first-class descriptor (`Alis.Reactive/Descriptors/Mutations/MethodArg.cs`) | Yes: runtime resolves `SourceArg` from event or component sources (`Alis.Reactive.SandboxApp/Scripts/execution/element.ts`) | `Alis.Reactive.Fusion/Components/TestWidgetSyncFusion/TestWidgetSyncFusionExtensions.cs`; `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/AllModulesTogether/TestWidget/Index.cshtml`; `Alis.Reactive.SandboxApp/Scripts/__tests__/when-using-unified-call-mutations.test.ts`; `Alis.Reactive.SandboxApp/Scripts/__tests__/when-walking-source-into-mutations.test.ts` | Partial | Runtime is broader than public fluent exposure here. |
| Call event payload method without args | No | Yes: same event-mutation algebra can model it (`Alis.Reactive/Descriptors/Commands/MutateEventCommand.cs`, `Alis.Reactive/Descriptors/Mutations/Mutation.cs`) | Yes: runtime can invoke `ctx.evt[method](...)` (`Alis.Reactive.SandboxApp/Scripts/execution/commands.ts`) | Public callers found are specialized arg-bearing helpers only: `Alis.Reactive.Fusion/Components/FusionAutoComplete/Events/FusionAutoCompleteOnFiltering.cs`, `Alis.Reactive.Fusion/Components/FusionMultiSelect/Events/FusionMultiSelectOnFiltering.cs` | Descriptor / runtime only | Engine can do this, but public fluent DSL does not currently expose it directly. |
| Call event payload method with args | Partial | Yes | Yes | `Alis.Reactive.Fusion/Components/FusionAutoComplete/Events/FusionAutoCompleteOnFiltering.cs`; `Alis.Reactive.Fusion/Components/FusionMultiSelect/Events/FusionMultiSelectOnFiltering.cs`; `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/AutoComplete/Index.cshtml`; `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/MultiSelect/Index.cshtml`; `tests/Alis.Reactive.DriftDetection.Tests/Behavior/WhenMutatingResidentUiState.cs` | Partial | Public reachability exists only through specialized helpers like `UpdateData(...)`. |
| Dispatch event without payload | Yes | Yes: `DispatchCommand` models it (`Alis.Reactive/Descriptors/Commands/DispatchCommand.cs`) | Yes: runtime dispatches `CustomEvent` with `{}` detail when payload is absent (`Alis.Reactive.SandboxApp/Scripts/execution/commands.ts`) | `Alis.Reactive/Builders/PipelineBuilder.cs`; `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/CoreBehaviors/Events/Index.cshtml`; `tests/Alis.Reactive.UnitTests/Commands/WhenDispatchingAnEvent.cs`; `tests/Alis.Reactive.PlaywrightTests/CoreBehaviors/WhenEventsChainAcrossListeners.cs` | Supported | This path is already stable. |
| Dispatch event with typed payload | Yes, if payload is already a build-time literal object | Yes, but only as raw `object? Payload` on the command (`Alis.Reactive/Descriptors/Commands/DispatchCommand.cs`) | Yes: runtime forwards `cmd.payload` directly into `detail` (`Alis.Reactive.SandboxApp/Scripts/execution/commands.ts`) | `Alis.Reactive/Builders/PipelineBuilder.cs`; `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/CoreBehaviors/Events/Index.cshtml`; `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/CoreBehaviors/Payload/Index.cshtml`; `tests/Alis.Reactive.UnitTests/Commands/WhenDispatchingAnEvent.cs`; `tests/Alis.Reactive.UnitTests/Triggers/WhenTriggeringOnCustomEventWithAllSupportedTypes.cs`; `Alis.Reactive.SandboxApp/Scripts/__tests__/when-dispatching-a-custom-event-with-payload.test.ts` | Supported | Typed custom-event payload dispatch already exists. The limitation is not payload existence; it is payload composition from live sources. |
| Dispatch event with source-backed or composed payload | No | No: `DispatchCommand` has only raw `object? Payload` and no source-backed payload contract (`Alis.Reactive/Descriptors/Commands/DispatchCommand.cs`) | No: runtime only forwards `cmd.payload`; it does not resolve payload fields from sources (`Alis.Reactive.SandboxApp/Scripts/execution/commands.ts`) | `Alis.Reactive/Builders/PipelineBuilder.cs`; `Alis.Reactive/Descriptors/Commands/DispatchCommand.cs`; `Alis.Reactive.SandboxApp/Scripts/types/commands.ts`; current sandbox dispatch examples are all literal payloads in `CoreBehaviors/Events` and `CoreBehaviors/Payload` | Not supported | This is the cleanest proven place where source-driven value flow stops today. |
| Consume typed custom-event payload | Yes | Yes: `TriggerBuilder.CustomEvent<TPayload>` creates a typed event-source lane (`Alis.Reactive/Builders/TriggerBuilder.cs`) | Yes: runtime passes `CustomEvent.detail` into `ctx.evt` (`Alis.Reactive.SandboxApp/Scripts/execution/trigger.ts`) | `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/CoreBehaviors/Payload/Index.cshtml`; `tests/Alis.Reactive.UnitTests/Triggers/WhenTriggeringOnCustomEvent.cs`; `tests/Alis.Reactive.UnitTests/Triggers/WhenTriggeringOnCustomEventWithAllSupportedTypes.cs`; `tests/Alis.Reactive.PlaywrightTests/CoreBehaviors/WhenPayloadFlowsBetweenEvents.cs` | Supported | Typed custom-event consumption is already a first-class capability. |
| Use event / component values in conditions | Yes | Yes: `ValueGuard.Source` is a `BindSource`, and `When(...)` starts from event or typed component source (`Alis.Reactive/Builders/PipelineBuilder.Conditions.cs`, `Alis.Reactive/Descriptors/Guards/ValueGuard.cs`) | Yes: runtime resolves and coerces values before guard evaluation (`Alis.Reactive.SandboxApp/Scripts/conditions/conditions.ts`) | `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Conditions/HttpMixing/Index.cshtml`; `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/AllModulesTogether/TestWidget/Index.cshtml`; `tests/Alis.Reactive.UnitTests/Conditions/WhenUsingConditionsInEveryDslSurface.cs`; `tests/Alis.Reactive.PlaywrightTests/AllModulesTogether/CrossVendor/WhenComponentApiExercisedEndToEnd.cs` | Supported | This is already proof that the framework has a real typed value model. |
| Source-vs-source comparisons | Yes | Yes: `ValueGuard.RightSource` exists specifically for this (`Alis.Reactive/Builders/Conditions/ConditionSourceBuilder.cs`, `Alis.Reactive/Descriptors/Guards/ValueGuard.cs`) | Yes | `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Conditions/NumericCondition/Index.cshtml`; `tests/Alis.Reactive.UnitTests/Conditions/WhenComparingTwoSources.cs` | Supported | Already covers component-vs-component and event-vs-component comparisons. |
| Source use in method args | Partial | Yes: `SourceArg` accepts any `BindSource` | Yes | Public event / response examples: `Alis.Reactive.Fusion/Components/TestWidgetSyncFusion/TestWidgetSyncFusionExtensions.cs`, `Alis.Reactive.Fusion/Components/FusionAutoComplete/Events/FusionAutoCompleteOnFiltering.cs`; runtime component-source example: `Alis.Reactive.SandboxApp/Scripts/__tests__/when-using-unified-call-mutations.test.ts` | Partial | Engine is broader than public DSL here. |
| Source use in prop writes | Partial | Yes: command-level `source` exists on element and event mutation commands | Yes | Element: `Alis.Reactive/Builders/ElementBuilder.cs`; component: `Alis.Reactive.Fusion/Components/TestWidgetSyncFusion/TestWidgetSyncFusionExtensions.cs`, `Alis.Reactive.Native/Components/NativeHiddenField/NativeHiddenFieldExtensions.cs`; event-object specialized: `Alis.Reactive.Fusion/Components/FusionAutoComplete/Events/FusionAutoCompleteOnFiltering.cs` | Partial | Element writes are broad. Component and event-object public surfaces are more specialized. |
| Gather from event | Yes | Yes: `EventGather` is first-class (`Alis.Reactive/Builders/Requests/GatherBuilder.cs`, `Alis.Reactive/Descriptors/Requests/EventGather.cs`) | Yes: runtime walks `evt` at gather time (`Alis.Reactive.SandboxApp/Scripts/execution/gather.ts`) | `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Conditions/HttpMixing/Index.cshtml`; `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/AutoComplete/Index.cshtml`; `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/MultiSelect/Index.cshtml` | Supported | Explicit bridge from event payload into request payload already exists. |
| Gather from component | Yes | Yes: `ComponentGather` and `AllGather` are first-class (`Alis.Reactive/Builders/Requests/GatherExtensions.cs`, `Alis.Reactive/Builders/Requests/GatherBuilder.cs`, `Alis.Reactive/Descriptors/Requests/ComponentGather.cs`) | Yes: runtime reads component registrations / component descriptors (`Alis.Reactive.SandboxApp/Scripts/execution/gather.ts`) | `Alis.Reactive/ReactivePlan.cs`; `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/AllModulesTogether/TestWidget/Index.cshtml`; `tests/Alis.Reactive.UnitTests/Requests/WhenGatheringRegisteredComponents.cs`; `Alis.Reactive.SandboxApp/Scripts/__tests__/when-gathering-form-values.test.ts` | Supported | `IncludeAll()` is backed by the plan’s component registry, not ad hoc DOM scanning. |
| Capture intermediate value for later reuse | No | No dedicated descriptor exists | No execution slot exists in `ExecContext` (`Alis.Reactive.SandboxApp/Scripts/types/context.ts`) | `Alis.Reactive/Builders/PipelineBuilder.cs`; `Alis.Reactive.SandboxApp/Scripts/execution/execute.ts`; repo search found no local/store source kind or capture command | Not supported | Real missing capability, but broader than the narrow dispatch-payload boundary proven here. |
| Capture method return value for later reuse | No | No return-capture descriptor exists | No: runtime invokes methods and discards return values (`Alis.Reactive.SandboxApp/Scripts/execution/element.ts`, `Alis.Reactive.SandboxApp/Scripts/execution/commands.ts`) | Same runtime call sites; no public DSL or descriptor for capturing call results was found | Not supported | Separate expansion from source-backed dispatch payload composition. |
| Existing workaround paths | Partial | N/A | N/A | Immediate source consumers already exist in conditions, prop writes, method args, and gather; direct component-to-component sync exists in `AllModulesTogether/TestWidget/Index.cshtml` | Partial | These only help when the consumer is immediate. None of them let `Dispatch(...)` compose a payload from current sources. |

## Architecture As It Exists Today

The framework already has a coherent end-to-end model:

1. the C# fluent DSL builds a plan
2. the plan serializes to JSON through `ReactivePlan.Render()`
3. the browser runtime executes that plan against a small execution context

The most accurate architecture vocabulary is:

1. resolve root
2. access member path
3. read raw JS value
4. shape / coerce
5. consume through a small command set
6. optionally cross scope by explicit dispatch

That model is already visible in code:

- vendor-agnostic root resolution exists today (`Alis.Reactive.SandboxApp/Scripts/resolution/component.ts`)
- `readExpr` is just member access after root resolution (`Alis.Reactive/ComponentRegistration.cs`, `Alis.Reactive/Descriptors/Sources/BindSource.cs`)
- the raw read result may be primitive, object, or array (`Alis.Reactive.SandboxApp/Scripts/__tests__/when-resolving-bind-expr.test.ts`, `Alis.Reactive.SandboxApp/Scripts/__tests__/when-reading-component-value.test.ts`)
- `coerce` is already the value-shaping seam for command consumers (`Alis.Reactive/Descriptors/Mutations/Mutation.cs`, `Alis.Reactive/Descriptors/Mutations/MethodArg.cs`, `Alis.Reactive.SandboxApp/Scripts/core/coerce.ts`)

There are also two related shaping seams already present:

- registration-time `coerceAs`
  - carried on `ComponentRegistration`
  - used for validation enrichment and gather/read-side flows
- command-time `coerce`
  - carried on mutations and source-backed method args
  - used when a command consumes a raw value

These are conceptually aligned, but not yet modeled through one unified
command-value contract.

## Current Source And Event Model

The serialized source model is still very small:

- `event`
- `component`

Typed response-body access does not introduce a third source kind today. It
piggybacks on the existing event-path lane by emitting paths like
`responseBody.name` or `responseBody.address.city`, then resolving those paths
from `ExecContext`.

This is important because it shows the framework already prefers reusing shared
mechanics over inventing new source families when the same model can carry the
capability.

The same thing is true for events:

- component events and custom events are the same value-flow idea with different
  scope attachment
- component event is local
- custom event is on `document`

Current trigger behavior is not identical in payload shaping, though:

- custom events pass `CustomEvent.detail`
- fusion-style component events pass their callback payload directly
- native component events synthesize `{ [readExpr]: currentValue, event: e }`

So the architecture should treat them as one event/value-flow model with
different roots and scope, while still describing the current trigger-layer
differences accurately.

## Where Value Flow Actually Stops

The current framework already supports live values flowing directly into:

- conditions
- prop writes
- method args
- gather

So the real question is not “does the framework have values?”

The real question is:

> where does value-flow continuity stop today, and what evidence proves that boundary?

The narrowest proven answer is: `Dispatch(...)`.

The proof is direct:

- public DSL exposes only `Dispatch(string)` and `Dispatch<TPayload>(string, TPayload)` (`Alis.Reactive/Builders/PipelineBuilder.cs`)
- descriptor shape exposes only raw `object? Payload` on `DispatchCommand` (`Alis.Reactive/Descriptors/Commands/DispatchCommand.cs`)
- runtime only forwards `cmd.payload` into `new CustomEvent(..., { detail: cmd.payload ?? {} })` (`Alis.Reactive.SandboxApp/Scripts/execution/commands.ts`)

So today the framework can already:

- read a component value
- read an event payload value
- read a response-body value
- shape that value
- use it immediately in several consumers

But it cannot yet:

- express a dispatch payload field as a source
- compose a dispatch payload from current execution-context values
- explicitly promote a component / event / response value into a new custom-event payload without custom JavaScript

That is the real boundary.

## Important Scope Boundary

Component-event payload is not the same thing as global custom-event payload.

- component events are wired to one component root (`Alis.Reactive.SandboxApp/Scripts/execution/trigger.ts`)
- custom events are wired on `document` (`Alis.Reactive.SandboxApp/Scripts/execution/trigger.ts`)

So a value from component A should not implicitly become global or implicitly
become component B’s event payload.

The explicit handoff point should remain `Dispatch(...)`.

That is why the smallest strengthening change is not “add storage everywhere”.
It is “make dispatch payload construction a source consumer so the handoff can
be declared explicitly in the same source/value model the rest of the framework
already uses”.

## Precise Gap Analysis

What is already strong today:

- typed custom-event payload dispatch already exists
- typed custom-event payload consumption already exists
- source-vs-source conditions already exist
- source-driven prop writes already exist
- source-driven method args already exist at runtime and partially in public DSL
- gather already bridges event and component values into HTTP requests

What is uneven but already present:

- event-object mutation is generic in descriptor/runtime, but public fluent reachability is specialized
- component-source prop writes are public only on narrower surfaces
- component-source method args are runtime-capable but not broadly exposed

What is actually missing for the motivating scenario:

- source-backed payload composition for `Dispatch(...)`

What is also missing, but larger than this issue:

- reaction-scoped local capture
- method-return capture

Those larger capture capabilities should not be the V1 issue unless there is a
proven use case that source-driven dispatch cannot solve. The motivating
scenario described here does not require that broader jump.

## Smallest Strengthening Change

Strengthen `Dispatch(...)` so custom-event payloads can be built from the same
source/value model already used elsewhere.

The design objective is:

- keep `Dispatch(...)` as the explicit handoff from local execution context to
  global custom event
- allow payload fields to come from literals and sources
- preserve typed custom-event consumption on the listener side
- avoid introducing a general local-store subsystem as a prerequisite

The key architectural direction is:

- dispatch becomes another command consumer of shaped values
- custom-event payload becomes an explicit mapped handoff, not an implicit leak
  of local event state
- the solution should reuse the shared root/member/shaping model rather than
  adding a dispatch-only shortcut

## Non-Goals

- arbitrary JavaScript evaluation
- implicit propagation of component-event payloads to global listeners
- reaction-scoped general storage as a prerequisite
- cross-trigger or cross-entry state
- method-return capture in the first strengthening pass

## Acceptance Criteria

- public fluent DSL can express a custom-event payload whose fields come from
  existing sources
- descriptor shape models dispatch payload fields explicitly instead of relying
  only on literal `object? Payload`
- runtime resolves dispatch payload fields from current execution context at
  dispatch time
- typed custom-event consumption continues to work without regression
- proof includes unit tests for plan shape
- proof includes runtime tests for dispatch-time source resolution
- proof includes one sandbox / Playwright scenario showing explicit handoff:
  component A value -> dispatch custom event -> typed custom-event listener ->
  downstream DOM / component update

## Why Issue #86 Needs Rewrite

Issue #86 currently leans toward “reaction-scoped intermediate values” and
`store` / `local` as the primary solution.

That overstates the proven gap.

The current code shows that the framework already has a real value-flow model
and already uses sources as the core abstraction. The smallest missing boundary
is not “we cannot hold any values”. It is “dispatch payloads are still
literal-only even though other consumers already accept sources”.

That is the smaller, more truthful, and better-sequenced issue.
