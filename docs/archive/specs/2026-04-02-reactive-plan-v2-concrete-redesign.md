# Reactive Plan V2 Concrete Redesign

Status: accepted redesign target

This document locks the next plan model as a redesign, not an adapter, not a backward bridge,
and not a compatibility layer. The current runtime may break during the transition. That is
acceptable. The goal is to replace the current descriptor-heavy plan authoring model with a
single capability-first schema that the C# DSL can render directly.

## Decision

1. Keep the public C# DSL and typed authoring experience.
2. Replace the internal descriptor model with a new plan document model that serializes directly
to Reactive Plan V2 JSON.
3. Rewrite the runtime against V2 instead of carrying old `entries/components` assumptions.
4. Retire the current descriptor vocabulary in favor of natural, domain-specific names.

## Why The Current Model Must Be Replaced

The current system expresses the same capability in too many places:

- `ComponentRegistration`
- `BindSource`
- `ComponentGather`
- `readExpr` on triggers
- validation field enrichment
- `MutateElementCommand`
- `MutateEventCommand`
- `SetPropMutation`
- `CallMutation`

That duplication is the real design bug. Every new feature forces one more special case because
the schema has no single place to describe what an object can do, what can be read from it,
what can be written to it, and what shape that value has.

## V2 North Star

The JSON plan must describe four things cleanly:

1. What runtime objects exist.
2. What capabilities each object exposes.
3. What fields can be read in a typed way.
4. What workflows execute when subscriptions fire.

Everything else is detail.

## Final V2 Shape

```ts
interface ReactivePlanV2 {
  version: 2
  planId: string
  sourceId?: string
  contracts: Record<string, CapabilityContract>
  objects: Record<string, RuntimeObject>
  bindings: Record<string, FieldBinding>
  workflows: Workflow[]
}

interface CapabilityContract {
  kind: "component" | "element" | "event-object" | "service"
  resolver: "native-element" | "fusion-instance" | "event-object" | "context-object"
  members: Record<string, PropertyMember | MethodMember>
  events?: Record<string, EventContract>
}

interface PropertyMember {
  kind: "property"
  path: PathSegment[]
  shape: ValueShape
  access: "read" | "write" | "readwrite"
}

interface MethodMember {
  kind: "method"
  path: PathSegment[]
  args?: ValueShape[]
  returns?: ValueShape | "void"
}

interface EventContract {
  channel: string
  eventObject?: { contract: string }
  data?: Record<string, ValueExpr>
}

interface RuntimeObject {
  contract: string
  elementId?: string
}

interface FieldBinding {
  object: string
  valueMember: string
  shape: ValueShape
}

type ValueShape =
  | { kind: "scalar"; type: "string" | "number" | "boolean" | "date" | "raw" }
  | { kind: "array"; item: ValueShape }
  | { kind: "object"; fields?: Record<string, ValueShape>; additional?: boolean }
  | { kind: "any" }

type ValueExpr =
  | { kind: "literal"; value: unknown }
  | { kind: "binding"; binding: string }
  | { kind: "member"; object: string | "$eventObject"; member: string }
  | { kind: "context"; scope: "event" | "response" | "request" | "local"; path?: PathSegment[] }
  | { kind: "object"; fields: Record<string, ValueExpr> }
  | { kind: "array"; items: ValueExpr[] }
  | { kind: "convert"; value: ValueExpr; to: ValueShape }

type Predicate =
  | { kind: "compare"; left: ValueExpr; op: CompareOp; right?: ValueExpr; as?: ValueShape; itemAs?: ValueShape }
  | { kind: "all"; terms: Predicate[] }
  | { kind: "any"; terms: Predicate[] }
  | { kind: "not"; term: Predicate }
  | { kind: "confirm"; message: string }

interface Workflow {
  when: Subscription
  run: Action
}

type Subscription =
  | { kind: "dom-ready" }
  | { kind: "document-event"; name: string }
  | { kind: "object-event"; object: string; event: string }
  | { kind: "server-push"; url: string; eventType?: string }
  | { kind: "signalr"; hubUrl: string; method: string }

type Action =
  | { kind: "sequence"; steps: Action[] }
  | { kind: "branch"; cases: Array<{ when?: Predicate; run: Action }> }
  | { kind: "parallel"; steps: Action[]; onSettled?: Action }
  | { kind: "set"; target: { object: string | "$eventObject"; member: string }; value: ValueExpr }
  | { kind: "call"; target: { object: string | "$eventObject"; member: string }; args?: ValueExpr[] }
  | { kind: "dispatch"; name: string; detail?: ValueExpr }
  | { kind: "request"; request: RequestPlan }
  | { kind: "inject"; object: string; value?: ValueExpr }
  | { kind: "show-validation-errors"; formId: string }

interface RequestPlan {
  method: "GET" | "POST" | "PUT" | "DELETE"
  url: string
  input?: {
    transport: "query" | "json" | "form-data"
    value: ValueExpr | { kind: "binding-map"; include: "all" | string[] }
  }
  validation?: {
    formId: string
    fields: Array<{ binding: string; rules: ValidationRule[] }>
  }
  before?: Action[]
  onSuccess?: ResponseHandler[]
  onError?: ResponseHandler[]
  onSettled?: Action[]
  next?: RequestPlan
}

type ResponseHandler = { statusCode?: number; run: Action }

type ValidationRule = {
  rule: string
  message: string
  constraint?: unknown
  otherBinding?: string
  as?: ValueShape
  when?: Predicate
}

type PathSegment = { prop: string } | { index: number }
```

## V2 Invariants

1. `contracts` is the only place allowed to know vendor resolution and raw JS member paths.
2. `objects` is the only place allowed to name concrete runtime instances.
3. `bindings` is the only place allowed to map model fields to canonical readable values.
4. `ValueExpr` is the only read and data-construction shape.
5. `Action` is the only execution shape.
6. Validation rules point to `binding`, never enriched vendor metadata.
7. Event payload shaping lives in event contracts, not in trigger variants.
8. Coercion becomes shape-driven conversion, not ad hoc flags leaking through the plan.
9. Partial and lazy plan merging stays `planId` + `sourceId` driven.
10. The runtime stays dumb: resolve object, evaluate value, execute instruction.

## Natural Language Vocabulary

The new names should match the problem domain instead of implementation artifacts.

| Current name | Replace with | Reason |
|--------------|--------------|--------|
| `Descriptor` | `PlanModel` or `Document` | These are not descriptors. They are the actual plan contract. |
| `ComponentRegistration` | `RuntimeObject` + `FieldBinding` + `CapabilityContract` | One type is doing three jobs today. |
| `BindSource` | `ValueExpr` | It is not only binding. It is the whole read model. |
| `GatherItem` | `RequestInput` via `ValueExpr` or `binding-map` | Gather is a request concern, not a top-level domain. |
| `Entry` | `Workflow` | The real concept is subscription plus action. |
| `Trigger` | `Subscription` | More descriptive and neutral. |
| `Reaction` | `Action` | The runtime executes actions. |
| `Mutation` | member target inside `set` or `call` | Mutation split is unnecessary once target/member is explicit. |
| `ValueGuard` | `Predicate.compare` | It is a predicate over values. |
| `StatusHandler` | `ResponseHandler` | Natural HTTP vocabulary. |
| `coerceAs` | `ValueShape` and `convert` | The real problem is shape and conversion, not loose coercion hints. |

## C# Architecture After The Redesign

The public DSL stays where it is. The internal pipeline changes completely.

### Layer 1: Authoring DSL

This layer remains familiar:

- `Html.ReactivePlan()`
- `Html.ResolvePlan()`
- `Html.On(plan, t => ...)`
- component `.Reactive(...)`
- `p.Component<TComponent>(...)`
- `comp.Value()`
- `p.When(...)`
- `p.Post(...).Gather(...).Validate(...).Response(...)`

No forced DSL rewrite for users.

### Layer 2: Typed Intent Graph

This is the new internal authoring model. It preserves compile-time correctness before JSON
exists. Recommended internal names:

- `ValueIntent<T>`
- `ActionIntent`
- `PredicateIntent`
- `RequestIntent`
- `WorkflowIntent`
- `SubscriptionIntent`
- `FieldBindingRef<T>`
- `RuntimeObjectRef<TContract>`

This is where the C# DSL should live. Builders produce typed intents, not JSON descriptors.

### Layer 3: Plan Document Model

This is the exact V2 JSON model. Recommended folder and namespace:

`Alis.Reactive/PlanModel/...`

Recommended subfolders:

- `PlanModel/Contracts`
- `PlanModel/Objects`
- `PlanModel/Bindings`
- `PlanModel/Expressions`
- `PlanModel/Predicates`
- `PlanModel/Actions`
- `PlanModel/Requests`
- `PlanModel/Validation`
- `PlanModel/Workflows`
- `PlanModel/Shapes`

This layer should be strongly typed and serializable as-is. No anonymous-object flattening.

### Layer 4: Serializer

`ReactivePlan<TModel>.Render()` should serialize a `ReactivePlanV2Document` directly.
Schema validation and snapshots should validate this document, not a mixed object graph.

## How The DSL Maps To The New Internals

### Components And Elements

Each vertical slice contributes a capability contract and uses that contract when creating
runtime objects.

- HTML helper registers a `RuntimeObject`
- HTML helper registers a `FieldBinding` when the component is model-bound
- component type contributes a `CapabilityContract`
- component extension methods emit `set` and `call` actions by member name
- `Value()` returns `ValueIntent<T>` from the default readable member
- special reads like `StartDate()` and `EndDate()` are named members on the same contract

This removes the need for `vendor`, `readExpr`, and `componentType` to leak through every feature.

### Event Args

Typed event args remain compile-time surfaces in C#, but they become event-object contracts in V2.

Example:

- `FusionAutoCompleteFilteringArgs.Text` -> event-object readable member
- `args.PreventDefault(p)` -> `set` on `"$eventObject"` member `preventDefaultAction`
- `args.UpdateData(p, response, x => x.Items)` -> `call` on `"$eventObject"` member `updateData`

This eliminates the `MutateEventCommand` split entirely.

### Conditions

`When(...)`, `Then(...)`, `ElseIf(...)`, `Else(...)`, and `Confirm(...)` should build
`PredicateIntent` and `ActionIntent`, then serialize to `Predicate` and `Action.branch`.

The conditions module should stop producing a separate reaction tree model. It should be part of
the same action model as everything else.

### HTTP

HTTP should become an action, not a separate reaction family.

- `Get/Post/Put/Delete` -> `RequestIntent`
- `Gather(...)` -> request input builder that emits `ValueExpr` or `binding-map`
- `WhileLoading(...)` -> `before`
- `OnSuccess(...)` -> `ResponseHandler`
- `OnError(...)` -> `ResponseHandler`
- `Chained(...)` -> `next`
- `Parallel(...)` -> `Action.parallel` where each branch is usually a `request` action and
`onSettled` carries the current `onAllSettled` behavior

### Validation

Validation extraction stays in C#, but enrichment changes completely.

- extractor returns rules keyed by binding
- request validation stores only `formId` and binding-based rules
- no `fieldId`, `vendor`, `readExpr`, or `coerceAs` gets injected into validation fields
- runtime resolves the field binding through `bindings`

This is the correct boundary.

## Typed Conversion Support Must Improve

The redesign must make conversions explicit and typed instead of sprinkling `coerceAs` everywhere.

### Required internal services

- `TypeShapeRegistry`
- `ValueShapeFactory`
- `ValueConversionPlanner`

### Rules

1. Every `ValueIntent<T>` carries an inferred `ValueShape`.
2. Nullable types use the underlying shape.
3. Arrays and collections become `ValueShape.array`.
4. Complex DTOs can emit `ValueShape.object`.
5. Enums default to scalar string unless a contract overrides that decision.
6. Cross-shape writes emit `convert` explicitly.
7. Component contracts may declare richer readable members than their default binding member.

### What this unlocks

- Date range components can expose `value`, `startDate`, and `endDate` without custom runtime code.
- File upload can expose file arrays without fake gather branches.
- Dropdown data sources can accept typed arrays or response objects through the same expression system.
- Validation comparisons can use the same shapes as conditions and request building.

## Concrete Refactor Plan

### Phase 1: Freeze The V2 Contract

Deliverables:

- add V2 JSON schema file
- add golden sample JSON
- add schema tests for the new document types
- mark the redesign as replacement-only, no backward adapter

### Phase 2: Introduce The New Plan Model

Deliverables:

- create `PlanModel` types for contracts, objects, bindings, expressions, predicates, actions, requests, validation, and workflows
- create a `ReactivePlanDocument` root object
- update `ReactivePlan<TModel>` to hold document-building state instead of `_entries` + `_componentsMap`

Files most directly affected:

- `Alis.Reactive/ReactivePlan.cs`
- `Alis.Reactive/ComponentRegistration.cs`
- `Alis.Reactive/Descriptors/**` -> replaced

### Phase 3: Introduce Typed Intent Objects

Deliverables:

- replace `ValueExpression<T>` internals with `ValueIntent<T>`
- make `EventValueExpression<TPayload, TProp>` and `ComponentValueExpression<TProp>` thin wrappers over the same value model
- make `ResponseBody<T>` paths compile into `context: response`

Files most directly affected:

- `Alis.Reactive/Builders/Conditions/ValueExpression.cs`
- `Alis.Reactive/Builders/Conditions/ComponentValueExpression.cs`
- response body path helpers

### Phase 4: Refactor Component And Event Onboarding

Deliverables:

- every vertical slice contributes a `CapabilityContract`
- HTML extensions register `RuntimeObject` and `FieldBinding`
- component extension methods emit actions by member name instead of `Mutation`
- event types contribute event-object contracts

Files most directly affected:

- `Alis.Reactive.Native/**/HtmlExtensions.cs`
- `Alis.Reactive.Fusion/**/HtmlExtensions.cs`
- `Alis.Reactive.Native/**/Extensions.cs`
- `Alis.Reactive.Fusion/**/Extensions.cs`
- `Alis.Reactive.Native/**/ReactiveExtensions.cs`
- `Alis.Reactive.Fusion/**/ReactiveExtensions.cs`

### Phase 5: Refactor Pipeline, Conditions, And HTTP

Deliverables:

- replace `Entry + Trigger + Reaction` emission with `Workflow`
- replace `Command` emission with `ActionIntent`
- replace `BindSource` and `GatherItem` with `ValueExpr` and `binding-map`
- fold conditional branching into the action model
- keep public builder methods unchanged

Files most directly affected:

- `Alis.Reactive/Builders/TriggerBuilder.cs`
- `Alis.Reactive/Builders/PipelineBuilder.cs`
- `Alis.Reactive/Builders/PipelineBuilder.Conditions.cs`
- `Alis.Reactive/Builders/PipelineBuilder.Http.cs`
- `Alis.Reactive/Builders/Requests/GatherBuilder.cs`
- `Alis.Reactive/Builders/Requests/HttpRequestBuilder.cs`
- `Alis.Reactive/Builders/Requests/ResponseBuilder.cs`
- `Alis.Reactive/ComponentRef.cs`

### Phase 6: Refactor Validation Around Bindings

Deliverables:

- extractor output remains rule-centric but keyed by binding
- remove field enrichment with component metadata
- validation rules reference bindings only
- stamp `planId` and `sourceId` cleanly on the document model

Files most directly affected:

- `Alis.Reactive/Resolvers/ValidationResolver.cs`
- validation descriptors and extractor integration

### Phase 7: Delete Descriptor Fluff

Deliverables:

- delete `Descriptors/Commands`
- delete `Descriptors/Reactions`
- delete `Descriptors/Triggers`
- delete `Descriptors/Requests`
- delete `Descriptors/Sources`
- delete `Descriptors/Mutations`
- keep only the new plan model and typed intent graph

This is not cleanup later. It is part of the redesign definition.

### Phase 8: Rewrite The Runtime To Match V2 Exactly

Deliverables:

- runtime resolves contracts and objects
- runtime evaluates `ValueExpr`
- runtime executes `Action`
- runtime performs request handling and binding-based validation lookup
- merge logic becomes `contracts` merge plus `sourceId` ownership for objects, bindings, workflows

The runtime should not try to emulate the old model.

## Merge And Lazy-Load Semantics

These rules stay true in V2:

1. `planId` remains the merge key for same-model plans.
2. `sourceId` identifies the owning partial or lazy-loaded fragment.
3. `contracts` merge idempotently by name.
4. `objects`, `bindings`, and `workflows` are owned by `sourceId` and are replaced or removed with that source.
5. Component IDs remain stable under the existing naming strategy.
6. Different `planId` values remain isolated.

## Acceptance Criteria For The C# Refactor

The redesign is only done when all of the following are true:

1. The public DSL still reads the same in views.
2. `Render()` emits only V2 JSON.
3. No serializer path depends on legacy descriptor types.
4. No validation path depends on enriched field vendor metadata.
5. No component feature requires inventing a new schema shape when it is just another readable or writable member.
6. Component event payload capabilities use the same action model as normal components and elements.
7. Shape inference and conversion are explicit and testable.
8. The runtime executes the V2 contract directly.

## Recommended First Implementation Slice

Do this first:

1. Add the V2 plan model and serializer behind `ReactivePlan<TModel>`.
2. Onboard one native component, one fusion component, and one event-object contract end to end.
3. Prove `bindings`, `member` reads, `set`, `call`, and `request` without backward shims.
4. Then fan out the remaining vertical slices.

That slice will validate the redesign before the entire runtime rewrite lands.
