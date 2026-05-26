# Reactive Plan Source Blueprint

This document is the design proof for the refactor. The public DSL is the source of truth. XML docs, schema files, and the current runtime are not design sources.

The required flow is:

```text
typed cshtml DSL -> rich Reactive Plan domain model -> generated Plan JSON and TypeScript contract -> runtime executor
```

The runtime executes the plan. It does not infer missing behavior and it does not defend against shapes that the generated domain model cannot emit.

## Source Roots

The blueprint is grounded in these source families:

| Source family | Role in the DSL |
| --- | --- |
| `Alis.Reactive/Razor/Extensions/PlanExtensions.cs` | View, same-model partial, render lifecycle |
| `Alis.Reactive/Razor/Extensions/HtmlExtensions.cs` | `Html.On` behavior attachment |
| `Alis.Reactive/Razor/Extensions/InputFieldExtensions.cs` | Model-bound input field surface |
| `Alis.Reactive/ReactivePlan.cs` | Plan artifact, plugin registration, input registration, validation projection binding |
| `Alis.Reactive/Builders/TriggerBuilder.cs` | Page-ready, custom event, server push, SignalR triggers |
| `Alis.Reactive/Builders/PipelineBuilder*.cs` | Ordered reaction graph, target selection, dispatch, value source reads, HTTP, conditions |
| `Alis.Reactive/Builders/ElementBuilder.cs` | DOM element object property/method operations |
| `Alis.Reactive/Builders/Conditions/*.cs` | Compare, all, any, not, confirm, then, else-if, else |
| `Alis.Reactive/Builders/Requests/*.cs` | Request endpoint, gather, headers, route params, response, chain, parallel |
| `Alis.Reactive/Builders/DispatchPayloadBuilder.cs` | Runtime-computed custom event payload object |
| `Alis.Reactive/ReactivePlugin.cs`, `Alis.Reactive/PlanModel/PluginContract.cs`, and `Alis.Reactive/Builders/Plugin*.cs` | Plugin object contracts, reads, calls, properties, arguments |
| `Alis.Reactive/Validation/*.cs` | Direct client validation projection DSL and projection registry |
| `Alis.Reactive.FluentValidator/*.cs` | FluentValidation client-rule projection bridge |
| `Alis.Reactive/ComponentRef.cs` and `Alis.Reactive/ComponentMember.cs` | JS object property/method contract emission |
| `Alis.Reactive/ComponentOnboarding/*.cs` and `Alis.Reactive/ComponentRegistration.cs` | Component identity, render binding, gather and validation join keys |
| `Alis.Reactive.Native/**` | Native component vertical slices, native app-level components, native gather |
| `Alis.Reactive.Fusion/**` | Fusion component vertical slices, app-level components, Syncfusion event payload commands |
| `Alis.Reactive.Fusion/Templates/*.cs` | Render-only template DSL |
| `Alis.Reactive.Assets/runtime/types/plan.ts` | Generated TS contract target, not hand-written source |
| `Alis.Reactive.Assets/runtime/domain`, `runtime/execution`, `runtime/lifecycle`, `runtime/validation` | Runtime role evidence |

Every public DSL method in these roots needs one row in the traceability tables before implementation work is treated as complete.

## Core Language

The central model is a deterministic browser object graph.

| Term | Meaning |
| --- | --- |
| `PlanArtifact` | A rendered plan document: root view, same-model partial contribution, independent partial root, or inline action-link plan. |
| `ContributionHandle` | Stable ownership handle for load/unload rollback. It is not a component id and not a type key. |
| `RuntimeJoinKey` | Component id, type key, plugin name, event name, or plan id used by runtime lookup. |
| `JsObjectContract` | Object members the runtime may read, write, call, or listen to. Applies to DOM elements, vendor components, app-level components, plugins, and event payload objects. |
| `PropertyMember` | Readable and/or writable JS object property with path, shape, and access. |
| `MethodMember` | Callable JS object method with path, argument shapes, and return shape. |
| `EventOrCallbackMember` | JS event or vendor callback channel that starts a reaction with optional payload contract. |
| `InputBinding` | Model path joined to rendered component id, value member, and value shape. |
| `ValueExpression` | Deterministic value producer: literal, object, array, URL read, payload read, property read, method read. |
| `BehaviorGraph` | Runtime executable reaction graph: sequence, set, call, dispatch, branch, request, parallel, inject, validation display. |
| `ConditionGraph` | Compare/all/any/not/confirm expression over value expressions. |
| `RequestPlan` | HTTP endpoint, route/header params, optional input payload, validation gate, lifecycle handlers, response handlers, chain. |
| `ValidationPlan` | Client-executable validation fields, rules, operands, activation conditions, display binding. |
| `PluginContract` | Browser plugin JS object contract: root/member functions, commands, properties, argument contracts. |
| `RenderContract` | Render-only options/templates. These do not become runtime behavior unless they register ids, bindings, or events. |

## DSL Grammar Matrix

### Plan Artifacts

| DSL source | Domain concept | JSON contract | Runtime role |
| --- | --- | --- | --- |
| `Html.ReactivePlan<TModel>()` | Root `PlanArtifact` for model type | `Plan.scope = root`, `planId` from model | Booted root plan registered and wired |
| `Html.ResolvePlan<TModel>()` | Same-model partial contribution | `Plan.scope = partial`, same `planId` as root model | Merge into booted plan; unload by contribution handle |
| Independent partial with its own `ReactivePlan` | Independent root artifact | Different `planId` | Boot or merge as separate plan document |
| `Html.RenderPlan(plan)` | Artifact serialization boundary | JSON script tag plus root validation summary for root plans | Boot/load consumes JSON; partial unload removes contribution |
| Native action link inline plan | Inline `PlanArtifact` carried in attributes | Serialized request/behavior fragment | Runtime executes the inline artifact when clicked |

### Triggers

| DSL source | Domain concept | JSON contract | Runtime role |
| --- | --- | --- | --- |
| `Html.On(plan, t => ...)` | Attach behavior definitions to artifact | `behaviors[]` | Trigger wiring scans behavior starts |
| `t.DomReady(...)` | Page-ready trigger | `startsWhen.kind = page-ready` | Deferred after non-page-ready listeners wire |
| `t.CustomEvent(name, ...)` | Document event trigger | `document-event` with untyped payload | `document.addEventListener` and payload context |
| `t.CustomEvent<TPayload>(name, ...)` | Typed document event trigger | `document-event` with `PayloadContract.typed` | Payload paths are typed at plan build |
| `t.ServerPush(url, ...)` | SSE trigger | `server-push` any event | Async event source listener |
| `t.ServerPush(url, eventType, ...)` | Named SSE trigger | `server-push` named event filter | Async event source listener |
| `t.ServerPush<TPayload>(...)` | Typed SSE trigger | `server-push` with typed payload | Payload paths are typed at plan build |
| `t.SignalR(hubUrl, method, ...)` | SignalR trigger | `signalr` | Async hub listener |
| `t.SignalR<TPayload>(...)` | Typed SignalR trigger | `signalr` with typed payload | Payload paths are typed at plan build |
| `builder.Reactive(plan, event, ...)` in component slices | Component event/callback trigger | `component-event` with component key and event name | Vendor event wiring resolves component object |

### Behavior Graph

| DSL source | Domain concept | JSON contract | Runtime role |
| --- | --- | --- | --- |
| Sequential calls inside pipeline | Ordered `BehaviorGraph.Sequence` | `sequence.steps[]` or single reaction | Execute in order |
| `p.Element(id)` | DOM element object target | Component object target with browser element type | Resolve `document.getElementById` as JS object |
| `SetText`, `SetHtml`, `Show`, `Hide` | Property write | `set` reaction | Sync property assignment |
| `AddClass`, `RemoveClass`, `ToggleClass` | DOM method call | `call` reaction | Sync method invocation |
| `p.Component<T>(expr)` | Model-bound component target | `component` source by deterministic id | Resolve vendor runtime object |
| `p.Component<T, TOtherModel>(expr)` | Cross-model component target | `component` source by other model id | Resolve vendor runtime object |
| `p.Component<T>(id)` | Explicit component target | `component` source by id | Resolve vendor runtime object |
| `p.Component<TApp>()` | App-level component target | Layout object component with fixed id | Resolve layout-owned object |
| Component extension `Set*` | JS property write | `set` reaction | Sync property assignment |
| Component extension method call | JS method call | `call` reaction | Sync method invocation |
| Component extension `Value` or readable property | Component property read | `ValueExpression.read(component)` | Evaluate value |
| `p.Dispatch(name)` | Event dispatch without payload | `dispatch` with none | Sync document `CustomEvent` |
| `p.Dispatch<TPayload>(name, literal)` | Event dispatch with literal payload | `dispatch` with literal data and payload contract | Sync document `CustomEvent` |
| `p.DispatchWith<TPayload>(...)` | Event dispatch with runtime-computed payload | `dispatch` with object `ValueExpression` | Evaluate values then dispatch |
| `p.ValidationErrors(containerId)` | Display validation errors | `show-validation-errors` | Client validate or server-error display |
| `p.Into(elementId)` | HTML injection from response body | `inject` | Inject string HTML and boot nested plans |

### Value Expressions

| DSL source | Domain concept | JSON contract | Runtime role |
| --- | --- | --- | --- |
| Literal strings, numbers, bools, dates, raw objects | Literal value | `ValueProducer.literal` with shape | Shape conversion |
| `p.FromUrl(name)` and `p.FromUrl<T>(name)` | URL query read | `read` from `url` | Read `URLSearchParams` |
| Event payload expressions | Payload path read | `read` from `payload:event` | Read typed event payload path |
| `ResponseBody<T>` expressions | Response payload read | `read` from `payload:success` or `payload:error` | Read typed response body path |
| `responseBody` whole body | Whole payload read | `member = responseBody` | Read payload root |
| Component readable property | JS object property read | `read` from `component` with property access | Runtime object read |
| Plugin property | JS object property read | `read` from `plugin` with property access | Runtime plugin object read |
| Plugin function | JS object method read | `read` from `plugin` with method access and args | Runtime plugin call returning value |
| Dispatch payload object | Object value | `ValueProducer.object` | Recursively evaluate fields |
| Membership/range operands | Array value | `ValueProducer.array` | Recursively evaluate items |

### Conditions

| DSL source | Domain concept | JSON contract | Runtime role |
| --- | --- | --- | --- |
| `p.When(payload, path)` | Event-payload compare start | `Condition.compare.left = payload read` | Evaluate compare |
| `p.When(responseBody, path)` | Response-body compare start | `Condition.compare.left = response read` | Evaluate compare |
| `p.When(typedSource)` | Typed source compare start | `Condition.compare.left = value expression` | Evaluate compare |
| `p.Confirm(message)` | User confirmation guard | `Condition.confirm` | Async lane because browser prompt/confirm can pause |
| `Eq`, `NotEq`, `Gt`, `Gte`, `Lt`, `Lte` | Binary comparison | `compare` with operator and right value | Compare shaped values |
| `Truthy`, `Falsy`, `IsNull`, `NotNull`, `IsEmpty`, `NotEmpty` | Unary comparison | `compare` with no right operand | Compare shaped value |
| `In`, `NotIn` | Membership comparison | `compare` with array operand | Evaluate membership |
| `Between` | Range comparison | `compare` with two-item array | Evaluate range |
| `Contains`, `StartsWith`, `EndsWith`, `Matches`, `MinLength` | Text comparison | `compare` with text/number operand | Evaluate text condition |
| `ArrayContains` | Collection item comparison | `compare` with literal item | Evaluate collection membership |
| `Eq/Gt/... (TypedSource<T>)` | Source-vs-source compare | `compare` with right value expression | Evaluate both sources |
| `And`, `Or` from sources | Condition composition | `all` or `any` | Short logical evaluation |
| Nested `And(start => ...)`, `Or(start => ...)` | Nested condition composition | Nested or flattened `all`/`any` | Logical evaluation |
| `Not()` | Negation | `not` | Logical not |
| `Then(...)` | Branch case | `branch.cases[].guard = when` | Execute first matching case |
| `ElseIf(...)` | Additional branch case | Additional `branch.cases[]` | Continue matching in order |
| `Else(...)` | Default branch case | `branch.cases[].guard = default` | Execute when no previous match |

### HTTP

| DSL source | Domain concept | JSON contract | Runtime role |
| --- | --- | --- | --- |
| `p.Get/Post/Put/Delete(url)` | Request endpoint | `Request.method`, `Request.url` | Async fetch lane |
| Inline `Post(url, gather)` and `Put(url, gather)` | Endpoint plus gather | `Request.input = gather` | Async fetch lane |
| `.Gather(...)` | Request input builder | `GatherInput` | Build request payload |
| `.AsJson()` | JSON body format | `bodyFormat = json` | Serialize JSON |
| `.AsFormData()` | Form data body format | `bodyFormat = form-data` | Serialize `FormData` |
| `.WhileLoading(...)` | Before request reaction stage | `Request.before[]` | Execute before fetch |
| `.Finally(...)` | Complete reaction stage | `Request.complete[]` | Execute after fetch settles |
| `.Validate<TValidationSource>(formId)` | Client validation gate | `Request.validation.container` | Validate before fetch; stop request on invalid |
| `.Response(r => ...)` | Response routing | `success[]`, `error[]`, `chain` | Route by fetch result |
| `r.OnSuccess(...)` | Any success handler | `success` handler any | Execute with success context |
| `r.OnSuccess<TResponse>(...)` | Typed success handler | `success` handler with typed payload contract | Typed response reads |
| `r.OnError(...)` | Any error handler | `error` handler any | Execute with error context |
| `r.OnError(status, ...)` | Status error handler | `error.match.status` | Execute matching status |
| `r.OnError<TError>(...)` | Typed error handler | `error` handler with typed payload contract | Typed error reads |
| `r.Chained(...)` | Follow-up request | `Request.chain.follow-up` | Execute next request after current response |
| `p.Parallel(...)` | Concurrent request group | `parallel.steps[]` | Async `Promise.allSettled` |
| `.OnAllSettled(...)` | Parallel completion reaction | `parallel.completion.on-settled` | Execute after all settle |

### Gather

| DSL source | Domain concept | JSON contract | Runtime role |
| --- | --- | --- | --- |
| `g.IncludeAll()` | Select all registered input bindings | `sourceSelection = all-registered-inputs` | Read all registered component values |
| `g.Static(name, value)` | Literal payload assignment | `payloadAssignments[]` | Write into payload path |
| `g.FromEvent(args, path, param)` | Event payload assignment | Payload read assignment | Write into payload path |
| `g.Header(name, value/source/event)` | Header source | `Request.headers` | Evaluate scalar header |
| `g.RouteParam(name, value/source/event)` | Route placeholder source | `Request.routeParams` | Fill URL template |
| `g.FromUrl(...)` | URL query assignment | URL read assignment | Write into payload path |
| `g.Plugin(source, param)` | Plugin read assignment | Plugin read assignment | Write into payload path |
| `g.Include<TComponent>(expr)` | Model-bound component value | Component read assignment plus component binding | Read registered input |
| `g.Include<TComponent>(id, name)` | Explicit component value | Component read assignment | Read component property/value member |
| `g.Include(typedComponentSource)` | Typed component property read | Component read assignment | Write into payload path |
| Native gather extensions | Native field groups | Same gather assignments | Runtime is vendor-agnostic |

### Validation

| DSL source | Domain concept | JSON contract | Runtime role |
| --- | --- | --- | --- |
| `ClientValidationProjectionBuilder<T>.Field(expr/token)` | Projected client field | `ComponentValidation.component/value/serverFieldName` | Resolve bound component and read value |
| `ClientValidationProjectionRegistry.Create(... For<TValidationSource,TModel>)` | Validation projection source keyed by validation source type | Validation rules bound to a validation container | Request validation resolves the client projection by source type |
| Direct rules `Required`, `Empty`, `Email`, `Url`, `CreditCard`, `AtLeastOne` | No-operand client rules | Rule with `constraint:none` | Rule engine |
| `MinLength`, `MaxLength`, `Regex` | Text/numeric constraint rules | Rule with literal constraint | Rule engine |
| `Range`, `ExclusiveRange` | Range constraint rules | Rule with range constraint | Rule engine |
| `Min`, `Max`, `GreaterThan`, `LessThan` | Ordered comparison rules | Rule with scalar constraint | Rule engine |
| `EqualTo`, `NotEqual` literal | Literal equality rules | Rule with scalar constraint | Rule engine |
| `EqualTo`, `NotEqualTo` peer field | Peer equality rules | Rule with peer read | Rule engine reads peer component |
| Projection `.When(condition, define)` | Rule activation | `activation.when` with validation condition | Rule active only if condition passes |
| Validation condition `Field(...).Truthy/Falsy/...` | Field condition compare | `ValidationCondition.compare` | Condition evaluator over current form values |
| Validation condition `And/Or/Not` | Validation condition composition | `all`, `any`, `not` | Logical evaluation |
| FluentValidation `.ProjectToClient(...)` | Explicit client projection marker | Registered projected rule | Adapter emits only client rule |
| `.ProjectToClient(...)` on async FluentValidation property validator | Server-only rule | No client projection | Runtime has no role |
| `ReactiveValidator.WhenField*` and `WhenFields` | Projectable client condition plus server predicate | Rule activation condition | Same condition also guards server rule |
| FluentValidation ordinary `When/Unless` | Server-only condition | No client projection | Adapter skips client projection under server-only condition |
| FluentValidation `WhenAsync/UnlessAsync` and async validators | Server-only validation | No client projection | Runtime never executes it |
| FluentValidation member-to-member comparison without explicit projection | Server-only comparison | No client projection | Peer client comparisons require `.ProjectToClient(...)` |
| Child validators/includes | Nested validation projection | Prefixed field paths | Runtime reads prefixed components |

### Plugin

| DSL source | Domain concept | JSON contract | Runtime role |
| --- | --- | --- | --- |
| `plan.RegisterPlugin(name, p => ...)` | String plugin contract | `JsType` plus plugin source | Resolve browser plugin by name |
| `plan.RegisterPlugin(ReactivePlugin)` | Typed plugin contract | `JsType` plus plugin source | Resolve browser plugin by name |
| `plan.RegisterPlugin<TPlugin>()` | Construct typed plugin contract | Same | Same |
| `PluginTypeBuilder.Method<T>` | Member function returning value | Method member with return shape | Runtime object call in value evaluation |
| `PluginTypeBuilder.Property<T>` | Readable property | Property member read access | Runtime object property read |
| `PluginTypeBuilder.Function<T>` | Root function returning value | Method member `$call` with root path | Runtime object call |
| `PluginTypeBuilder.Void/Command` | Command method/root command | Method member returning none | Runtime call reaction |
| `ReactivePlugin.Function/Property/Command` | Typed descriptors | Same contracts without string use by consumer | Same |
| `PluginTypeBuilder.Method<T>` and `Void/Command` without arg contract | Open argument contract | Method arguments `open` | Runtime passes evaluated args |
| `PluginArgumentTypes.Arg<T>` and typed arity overloads | Exact argument contract | Method arguments `exact` with shapes | Runtime applies argument shapes |
| `ReactivePlugin.Function/Command` descriptors | Exact argument list from descriptor | Method arguments `exact`, including empty exact list | Runtime applies argument shapes |
| `p.Plugin<T>(name/member/function)` | Plugin function read | `ValueExpression.read(plugin, method)` | Evaluate by calling plugin |
| `p.PluginProperty<T>(...)` or `p.Plugin(property)` | Plugin property read | `ValueExpression.read(plugin, property)` | Evaluate by reading plugin |
| `p.Plugin(name/member/command).Arg(...).Fire()` | Plugin command call | `call` reaction on plugin source | Sync method invocation |
| Plugin arg from response/event/typed source/literals | Argument value expressions | Method access args | Evaluate args then call |
| Plugin event arg overloads | Event payload read by expression path | Payload read arg | Runtime reads event payload; the C# args instance is not serialized |

### Component Vertical Slices

The component DSL is deliberately vertical. The rich model underneath must be shared.

| Slice source | Domain concept | Runtime effect |
| --- | --- | --- |
| `*HtmlExtensions.cs` | Render component and register controlled id/binding when model-bound | Emits HTML and input registration |
| `*Builder.cs` | Render-only options | No runtime behavior unless registration/events are added |
| `*Events.cs` and `Events/*.cs` | Event/callback descriptors and payload objects | Payload contract for typed event reads |
| `*ReactiveExtensions.cs` | Component event/callback trigger DSL | Adds `component-event` behavior |
| `*Extensions.cs` | Component property read/write and method calls | Emits JS object contract and reactions |
| `*GatherExtensions.cs` | Component value source selection | Emits gather value read |
| App-level components | Fixed layout object id | Join existing layout object, do not own model binding |
| Event arg extensions like `PreventDefault` and `UpdateData` | Event payload object property set/method call | Sync payload mutation inside event callback |
| Fusion templates | Render contract | No runtime behavior unless nested DSL registers behavior |

Component onboarding reduces to:

```text
component id + vendor + type key + object contract + optional input binding + optional event contracts
```

## Rich Domain Model Target

The C# model should be organized around the DSL language, not around serialization plumbing.

| Module | Owns | Does not own |
| --- | --- | --- |
| `Artifacts` | Root, partial contribution, independent partial root, inline artifact, contribution handle | Component lookup rules |
| `Objects` | JS object contracts, members, paths, shapes, access, method signatures | HTTP, validation, trigger policy |
| `Components` | Component identity, vendor, type key, contribution intent, input binding, event/callback contract | Vendor rendering details |
| `Values` | Literals, object/array producers, URL reads, payload reads, object reads/calls | Runtime DOM lookup |
| `Behaviors` | Sequence, set, call, dispatch, branch, request, parallel, inject, validation display | Browser APIs directly |
| `Conditions` | Compare/all/any/not/confirm, operands, shaped comparison intent | Request lifecycle |
| `Requests` | Endpoint, params, gather, body format, validation gate, response routes, chain, parallel | Component registration |
| `Validation` | Client projection, fields, rules, operands, activation, binding to components | Server-only FluentValidation behavior |
| `Plugins` | Plugin contract and invocation language | Vendor component APIs |
| `Rendering` | HTML-only options/templates and registration hooks | Executable behavior |

The generated JSON and TS contract should be emitted from these modules. No hand-written TS type should describe plan concepts independently from C#.

## JSON Contract Target

The JSON should remain a small published language:

| Domain concept | JSON discriminant |
| --- | --- |
| Plan artifact | `Plan.scope: root | partial` |
| JS object contract | `JsType.properties`, `methods`, `events` |
| Component role | `Component.contribution: owned-definition | object-target | validation-container | layout-object` |
| Input binding | `binding: registered-input` |
| Validation container | `container: validation-container` |
| Trigger | `startsWhen.kind` |
| Reaction | `reaction.kind` |
| Value expression | `ValueProducer.kind` plus read source/access |
| Condition | `Condition.kind` |
| Request input | `RequestInput.kind` |
| Request chain | `RequestChain.kind` |
| Rule activation | `ValidationRuleActivation.kind` |

The TS runtime type contract is generated from this C# domain. Runtime implementation imports generated types but does not duplicate domain rules as a second schema.

## Runtime Executors

| Executor | Responsibility | Lane |
| --- | --- | --- |
| `LifecycleExecutor` | Boot root plans, compose initial plans, load partial slots, unload partial slots | Sync orchestration with async listener cleanup |
| `TriggerExecutor` | Wire page-ready, document events, component events, SSE, SignalR | Sync for DOM/component events; async for remote event streams |
| `ObjectResolver` | Resolve component/plugin/event-payload object roots and member paths | Sync |
| `ValueEvaluator` | Evaluate literals, object/array values, URL reads, payload reads, object reads/calls | Sync unless future value source is explicitly async |
| `ConditionEvaluator` | Evaluate compare/all/any/not/confirm | Sync except confirm |
| `BehaviorExecutor` | Execute set/call/dispatch/inject/branch/sequence/show-validation | Sync until it reaches an async concept |
| `HttpExecutor` | Validate, gather, route params, headers, fetch, route response, chain | Async |
| `ValidationExecutor` | Evaluate client rules, display inline/summary/server errors, live revalidation | Sync |
| `PluginExecutor` | Resolve plugin object and execute declared property/method access | Sync |

The sync lane is required for component event payload mutations such as Syncfusion cancel/prevent-default operations. HTTP, remote triggers, parallel requests, and confirm are the selected async concepts.

## Partial Plan Matrix

| Scenario | Identity rule | Merge behavior | Unload behavior |
| --- | --- | --- | --- |
| MVC renders root and same-model partial together | Same `planId`, partial scope contribution | Compose initial plan by plan id before boot | Not applicable unless later slot owns it |
| Browser loads same-model partial into slot | Same `planId`, slot contribution handle | Merge behavior, types, components, validation rules into booted plan | Abort listeners, remove contributed behavior/types/components/rules |
| Browser loads independent partial model | Different `planId` | Create or merge independent plan document | Remove that plan if contribution leaves it empty |
| Partial references root-owned component | Component id/type/vendor must match root runtime identity | Join as object target or validation extension | Remove only contributed references/rules |
| Partial owns new component | Component key owned by contribution handle | Add component and type fragments | Remove component and any live validation wiring |
| Partial contributes layout object reference | Fixed app-level id/type/vendor | Materialize if absent, otherwise join matching root object | Remove only if materialized by that partial and no remaining partial references |
| Partial contributes object type fragment | Type key is runtime join key | Merge compatible member fragments | Recompute remaining type from root plus other partial fragments |
| Partial contributes validation rules | Container identity must match root container | Append/replace by validated component rules | Remove only contributed rule objects |

## Design Sequences

### DSL Build

```mermaid
sequenceDiagram
    participant View as cshtml DSL
    participant Domain as Reactive Plan domain
    participant Json as Plan JSON
    participant TS as Generated TS types
    View->>Domain: ReactivePlan / ResolvePlan
    View->>Domain: Html.On triggers and pipelines
    View->>Domain: component render registrations
    View->>Domain: validation projections
    Domain->>Json: serialize deterministic artifact
    Domain->>TS: generate runtime contract
```

### Component Event

```mermaid
sequenceDiagram
    participant Vendor as Component callback
    participant Trigger as TriggerExecutor
    participant Exec as BehaviorExecutor
    participant Object as ObjectResolver
    Vendor->>Trigger: event payload object
    Trigger->>Exec: execute reaction with event context
    Exec->>Object: resolve component or payload object
    Exec->>Object: set/call synchronously
```

### HTTP

```mermaid
sequenceDiagram
    participant Exec as BehaviorExecutor
    participant Validation as ValidationExecutor
    participant Gather as GatherExecutor
    participant Http as HttpExecutor
    Exec->>Validation: validate container if configured
    Validation-->>Exec: valid or invalid
    Exec->>Gather: evaluate selected sources
    Gather->>Http: body, headers, route params
    Http->>Http: fetch
    Http->>Exec: success or error response context
    Exec->>Exec: response handlers and chain
```

### Partial Load And Unload

```mermaid
sequenceDiagram
    participant Browser as Browser partial slot
    participant Life as LifecycleExecutor
    participant Plans as Applied plan state
    participant Trigger as TriggerExecutor
    Browser->>Life: load slot with plan artifacts
    Life->>Plans: merge by plan id and contribution handle
    Life->>Trigger: wire contributed behaviors with abort signal
    Browser->>Life: unload slot
    Life->>Trigger: abort contributed listeners
    Life->>Plans: remove contributed behaviors, components, types, validation rules
```

### Validation Projection

```mermaid
sequenceDiagram
    participant DSL as Validation DSL
    participant Domain as ValidationPlan
    participant Runtime as ValidationExecutor
    DSL->>Domain: fields, rules, conditions
    Domain->>Domain: bind fields to registered component ids
    Domain->>Runtime: generated validation JSON
    Runtime->>Runtime: read field components
    Runtime->>Runtime: evaluate activation and rules
    Runtime->>Runtime: display inline or summary errors
```

## Proof Bar

No implementation refactor is complete until these checks pass:

1. Every public DSL method in the source roots appears in a traceability row.
2. Every row maps to one domain concept, one JSON concept, and one runtime executor role.
3. Every component vertical slice maps to the shared component object contract: properties, methods, events/callbacks, optional input binding, optional render-only options.
4. Validation distinguishes client projection from server validation. Server-only and async FluentValidation behavior is not projected.
5. Partial merge/unmerge uses contribution handles for rollback and runtime join keys for lookup.
6. Runtime preserves sync execution for ordinary object and payload mutations.
7. Runtime async appears only on explicit async concepts: HTTP, parallel, remote triggers, confirm.
8. No generated plan schema is treated as design authority. C# rich domain model emits JSON and TS contracts.
9. Defensive runtime validation is limited to external integration failure messages: missing DOM element, missing browser plugin, missing vendor runtime, corrupted external JSON. It is not a second plan validator.
10. Tests are behavior documents by module: lifecycle, object contracts, pipeline execution, HTTP/gather, conditions, validation, plugin, component trigger wiring.

## Immediate Implementation Order

1. Close the DSL matrix with source review comments.
2. Rename and arrange plan-domain files around the module map above.
3. Replace anemic plan construction with domain concepts from this blueprint.
4. Generate `runtime/types/plan.ts` from the rich domain contract.
5. Trim runtime to the executor modules listed above.
6. Rearrange tests by behavior module and delete implementation-shaped tests.
7. Only then continue component-specific onboarding cleanup.
