# Reactive DSL Feature Atlas

This atlas is a source-grounded map from the frozen public DSL to the rich plan
domain and runtime execution model. It is not a separate specification. When in
doubt, the source builders and real cshtml usage win.

The reason this document exists: broad refactors must start from DSL capability,
not from current helper classes or JSON shape. Each row below should pressure
test the same path:

`DSL capability -> PlanModel concept -> generated TypeScript term -> runtime execution behavior -> behavior proof`

## Primary Mental Model

The framework models deterministic interaction with browser objects.

A browser object has:

- properties that can be read or written
- methods that can be called and may return values
- event channels that carry payload objects
- callback payloads that are still payload objects, even when the vendor calls
  them callbacks rather than events

Everything else is a source scope or a reaction over those objects:

- URL query values are a source scope.
- Event args, HTTP success/error bodies, outgoing request payload, and dispatch
  payloads are payload scopes.
- JSON walking is structured `Path` traversal over a value, not string magic.
- Plugins are explicit browser objects/functions for behavior that should not
  become a first-class deterministic DSL primitive.

Runtime should execute the declared object/member intent. It should not infer
capability from live JavaScript objects except at integration boundaries where
external script loading or corrupted JSON can break the contract.

## Plan Load And Unload

Source:

- `Html.ReactivePlan<TModel>()`
- `Html.ResolvePlan<TModel>()`
- `Html.RenderPlan(plan)`
- `Html.On(plan, t => ...)`

Domain terms:

- `ReactivePlan<TModel>` owns plan identity, component registrations, behavior
  graph, plugin/type contracts, and validation work queue.
- `ReactivePlanScope.RootView` emits a root plan and validation summary.
- `ReactivePlanScope.PartialView` emits a partial plan document that
  merges into an existing browser plan.
- `PlanIdentity` is the stable model identity used by composition and slot load.

Design consequence:

Partial behavior is not "append this JSON". It is browser plan load/unload:
owned component definitions, object member declarations, validation rules,
behaviors/listeners, and references must merge on load and be released on slot
unload. There are two load paths, SSR and browser injection, but both should
use the same plan document merge language.

## Trigger Surface

Source:

- `t.DomReady(p => ...)`
- `t.CustomEvent("name", p => ...)`
- `t.CustomEvent<TPayload>("name", (payload, p) => ...)`
- `t.ServerPush(url, ...)`
- `t.ServerPush<TPayload>(url, eventType, ...)`
- `t.SignalR(hubUrl, methodName, ...)`
- component `.Reactive(...)` extension methods

Domain terms:

- `StartsWhen.PageReady`
- `StartsWhen.DocumentEvent`
- `StartsWhen.ServerPush`
- `StartsWhen.SignalR`
- `StartsWhen.ComponentEvent`
- `PayloadContract`
- `Behavior.On(trigger, reaction)`

Runtime behavior:

Triggers subscribe once per loaded behavior graph and execute a reaction graph
when the channel fires. Component `.Reactive` is not special runtime magic; it
wires a component event channel to the same behavior graph model.

## Reaction Graph Surface

Source:

- `p.Element(id).AddClass/RemoveClass/ToggleClass/SetText/SetHtml/Show/Hide`
- `p.Component<TComponent>(...).Set*/Call*`
- `p.Plugin(...).Arg(...).Fire()`
- `p.Dispatch(...)`
- `p.DispatchWith<TPayload>(...)`
- `p.When(...).Then(...).ElseIf(...).Else(...)`
- `p.Confirm(message).Then(...)`
- `p.Get/Post/Put/Delete(...)`
- `p.Parallel(...)`
- `p.ValidationErrors(containerId)`
- `p.Into(elementId)`

Domain terms:

- `Reaction.Sequence`
- `Reaction.Set`
- `Reaction.Call`
- `Reaction.Dispatch`
- `Reaction.Branch`
- `Reaction.Request`
- `Reaction.Parallel`
- `Reaction.Inject`
- `Reaction.ShowValidationErrors`
- `BranchCase`
- `ParallelCompletion`

Runtime behavior:

Most reactions execute in the immediate lane. HTTP requests, parallel requests,
partial injection, server push setup, SignalR setup, and reached confirmation
guards introduce async boundaries. Composite conditions must short-circuit in
the current lane and only become async when a reached term needs async behavior.

Design consequence:

Request stages such as `WhileLoading`, `Finally`, response handlers, chained
requests, and parallel `OnAllSettled` are reaction graph slots. They are not
"command list only" slots. If the DSL can express a branch, request, dispatch,
or nested graph there, PlanModel/runtime must preserve it.

## Value Producer Surface

Source:

- literals passed to builders
- `p.FromUrl<T>("name")`
- `p.When(args, x => x.Property)`
- `ResponseBody<T>.Read(...)` via response handler lambdas
- `p.Component<TComponent>(...).Value()` and other typed component reads
- `p.Plugin<T>(...)` and declared plugin functions/properties
- object/array payload builders such as `DispatchWith`
- event/response expressions passed to plugin and gather arguments

Domain terms:

- `ValueProducer.Literal`
- `ValueProducer.Read`
- `ValueProducer.Invoke`
- `ValueProducer.Object`
- `ValueProducer.Array`
- `Source.Component`
- `Source.Payload`
- `Source.Plugin`
- `Source.Url`
- `Path`
- `Shape`

Runtime behavior:

Evaluate the value producer against the current execution context. `Path`
traversal is deterministic JSON walking over a value scope. It is one explicit
path language across payload reads, response body reads, method arguments,
conditions, setters, gather, and validation.

## Condition Surface

Source:

- equality/order: `Eq`, `NotEq`, `Gt`, `Gte`, `Lt`, `Lte`
- presence: `Truthy`, `Falsy`, `IsNull`, `NotNull`, `IsEmpty`, `NotEmpty`
- membership/range/text: `In`, `NotIn`, `Between`, `Contains`, `StartsWith`,
  `EndsWith`, `Matches`, `MinLength`, `ArrayContains`
- source-to-source comparisons such as `Eq(TypedSource<T>)`
- composition: `And`, `Or`, `Not`
- branch flow: `Then`, `ElseIf`, `Else`
- prompt guard: `Confirm`

Domain terms:

- `Condition.Compare`
- `Condition.All`
- `Condition.Any`
- `Condition.Not`
- `Condition.Confirm`
- `ComparisonOperands`
- `BranchGuard`

Runtime behavior:

Conditions are value-producer trees with an explicit async prompt primitive.
Branches evaluate ordered cases and execute the first matching reaction; default
is the explicit else branch. Validation activation binds `FieldCondition` to a
`ValidationCondition`, the deterministic compare/all/any/not subset that cannot
carry `Confirm`.

## HTTP Surface

Source:

- `p.Get/Post/Put/Delete(url)`
- inline gather overloads such as `Post(url, g => ...)`
- `.Gather(g => ...)`
- `.AsJson()`
- `.AsFormData()`
- `.Validate<TValidationSource>(formId)`
- `.WhileLoading(...)`
- `.Finally(...)`
- `.Response(r => r.OnSuccess(...).OnError(...).Chained(...))`
- `.Parallel(a => ..., b => ...).OnAllSettled(...)`

Domain terms:

- `Request`
- `RequestEndpoint`
- `RequestInput`
- `RequestInputProjection`
- `RequestStages`
- `RequestReactionStages`
- `RequestValidation`
- `ResponseDraft`
- `ParallelCompletion`

Runtime behavior:

HTTP is the main async lane. Gather builds payload, route params, and headers
before transport. Validation can gate the request. Success/error handlers receive
payload scopes. Chained requests run after their parent response path. `Finally`
runs after request settlement.

Design consequence:

HTTP should read like a deterministic request plan with named stages. The
runtime should not reinterpret handler order or infer response shape; the plan
already declares those scopes and follow-up graphs.

## Gather Surface

Source:

- `IncludeAll()`
- vendor `Include(m => m.Field)` extensions
- `Include(TypedComponentSource<T>, paramName)`
- `Static(param, value)`
- `FromEvent(args, path, param)`
- `FromUrl(...)`
- `Header(...)`
- `RouteParam(...)`
- `Plugin(source, paramName)`

Domain terms:

- `GatherDraft`
- `RequestInputAssignment`
- `RequestInputTarget`
- `RequestPayloadTarget`
- `ValueProducer`
- `RegisteredInputAssignment`
- `GatheredComponentValue`
- `RequestScalarSlot`
- `HeaderName`
- `RouteParameterName`
- `UrlParameterName`

Runtime behavior:

Gather is a declared request input projection. It is one ordered list of authored
assignments: `target <- source`. A target is a payload path, header name, or
route parameter name. A source is any `ValueProducer`: literal, URL value,
event/response payload, component property read, plugin result, object, or
array. Scalar destinations such as headers and route parameters stay scalar.
`IncludeAll` is a selection policy over the current runtime plan, so registered
inputs from the root view and loaded partials load and unload deterministically.

Design consequence:

Gather depends on controlled component IDs. ID control is an absolute
requirement because component render, registration, validation, gather, partial
merge/unmerge, and runtime lookup all join on that ID.

There is no payload ownership accounting model. The DSL and model-bound
component IDs are the correctness mechanism. Runtime executes every assignment
in the generated plan; it does not negotiate precedence or double-check a
generated plan before execution.

## Validation Surface

Source:

- `ReactiveValidator<T>`
- `WhenField(...)`
- `WhenFieldNot(...)`
- order/presence/text/membership condition helpers
- `WhenFields(c => c.Field(...).Truthy().And(...), ...)`
- normal FluentValidation rules such as NotEmpty, Length, Email, Regex, ranges,
  comparisons, nested validators, Include
- FluentValidation `.When/.Unless/.WhenAsync/.UnlessAsync` guards outside the
  client projection language

Domain terms:

- `IClientValidationProjectionSource.ProjectClientRules`
- `ClientValidationProjections`
- `ClientValidationFieldToken`
- `ClientValidationField`
- `ValidationRule`
- `FieldCondition`
- `ValidationRuleActivation`
- `ClientConditionProjection`
- `SkippedClientRule`

Runtime behavior:

FluentValidation still executes normally. The framework projects only
client-side projectable rules. `ReactiveValidator` gives explicit client
condition intent while still applying the normal predicate. FluentValidation
guards outside that client language are skipped for client projection.

Design consequence:

Validation is a projection/binding problem, not a reflection trick. Client rules
should be explicit, named, and bound to registered component value contracts at
render. Core registry projections carry typed field tokens and field shapes;
FluentValidation is one adapter into the same projection contract. Peer-field
comparisons and conditional activation must resolve through the same value
producer and condition language as `.Reactive`.

## Browser Object Contracts

Source:

- `ComponentProperty<TValue>`
- `ComponentMethod`
- `TypedEvent<TArgs>`
- `ComponentRef<TComponent, TModel>`
- `ComponentObjectTarget`
- `ComponentEventOnboarding`
- `ModelBoundInputComponentSlot`
- `ReactivePlugin`
- `PluginTypeBuilder`

Domain terms:

- `BrowserObjectContract`
- `JsPropertyContract`
- `JsMethodContract`
- `JsEventContract`
- `MethodSignature`
- `MemberAccess`
- `InputValueContract`
- `ComponentRole`
- `PluginContract`

Runtime behavior:

The runtime resolves a declared component/element/plugin object, then applies
the declared property path, method path, event channel, or callback payload
member. Component vertical slices isolate vendor APIs, but all slices converge
into the same object/member contract.

Design consequence:

Component onboarding should be a vertical slice:

- render HTML with controlled ID
- register input value contract when model-bound
- expose typed object properties/methods/value sources
- expose typed event/callback payload descriptors
- prove gather, validation, conditions, and event handling through behavior
  tests and Playwright slices

## Component Families

Native input/object slices observed in source:

- `NativeTextBox`, `NativeTextArea`, `NativeCheckBox`, `NativeCheckList`,
  `NativeDropDown`, `NativeHiddenField`, `NativeRadioGroup`, `NativeButton`,
  `NativeActionLink`

Native app-level slices:

- `NativeDrawer`
- `NativeLoader`

Fusion input/object slices observed in source:

- `FusionAccordion`, `FusionAutoComplete`, `FusionColorPicker`,
  `FusionDatePicker`, `FusionDateRangePicker`, `FusionDateTimePicker`,
  `FusionDialog`, `FusionDropDownList`, `FusionFileUpload`, `FusionGrid`,
  `FusionInPlaceEditor`, `FusionInputMask`, `FusionMultiColumnComboBox`,
  `FusionMultiSelect`, `FusionNumericTextBox`, `FusionRichTextEditor`,
  `FusionSchedule`, `FusionSwitch`, `FusionTab`, `FusionTimePicker`,
  `FusionTooltip`

Fusion app-level slices:

- `FusionConfirm`
- `FusionToast`

Special notes:

- `NativeActionLink` carries a small plan/reaction as inline attributes. It is
  an app-level/action primitive that unlocks repeated row actions without a
  separate rendered plan block per row.
- `FusionAutoComplete` and `FusionMultiSelect` filtering callbacks mutate the
  event payload object through `preventDefaultAction` and `updateData(...)`.
  These are not exceptions to the model; they are property/method access on the
  callback payload object.
- `FusionSchedule` proves deep payload paths such as `args.Data.Id`, mixed
  drawer injection, app-level object calls, gather from event payload, and
  component reads like current view/selected date.

## Plugin Surface

Source:

- `plan.RegisterPlugin("name", p => p.Method<T>(...).Property<T>(...).Void(...))`
- `plan.RegisterPlugin(new MyPlugin())`
- `plan.RegisterPlugin<TPlugin>()`
- `p.Plugin<T>(pluginName, member)`
- `p.Plugin<T>(PluginFunction<T>)`
- `p.Plugin(PluginCommand).Arg(...).Fire()`
- `p.PluginProperty<T>(...)`

Domain terms:

- `ReactivePlugin`
- `PluginContract`
- `PluginOperation`
- `PluginFunction<T>`
- `PluginCommand`
- `PluginProperty<T>`
- `PluginOperationId`
- `PluginPropertyId`

Runtime behavior:

Plugins are named browser objects/functions. They bridge behavior difficult to
express deterministically in the core DSL, such as complex array work, URL/DOM
APIs, or vendor-specific browser logic. They should remain explicit contracts;
string-based plugin calls are compatibility surface, not the preferred rich
model.

Design direction:

The richer plugin API should favor typed descriptors and exact argument/return
contracts while preserving existing DSL compatibility. Plugin calls and reads
must still reduce to object member method/property access in PlanModel.

## Test Organization Target

Behavior tests should be arranged around these domain modules:

- plan load/unload and partial slot load/unload
- object contract and component onboarding
- value producers and path traversal
- conditions and branch execution
- request/gather/response/chained/parallel stages
- validation projection and field binding
- plugin descriptors and invocation
- runtime execution lanes
- app-level components and action link behavior

Tests that only mirror helper class internals are a smell. They should prove
DSL behavior or PlanModel invariants that developers rely on.

## Refactor Direction

The next closed surfaces should be chosen by blast radius and domain clarity:

1. Object/member contract kernel: property, method, event/callback, source,
   value read, path, shape.
2. Reaction graph execution: sequence, branch, request graph slots, sync/async
   lane boundary.
3. Partial slot load/unload: plan identity, slot load, slot unload, owned
   definitions, references, validation and listener removal.
4. Validation projection/binding: client rule projection, field conditions,
   peer fields, skipped unprojected guards.
5. Gather/request: source-to-target payload assignments, scalar slots, route/header/url binding,
   chained/parallel stages.

Delete-and-rewrite is acceptable for a surface only when this atlas identifies
the DSL capability, the replacement PlanModel vocabulary, generated TS shape,
runtime behavior, and proof path.
