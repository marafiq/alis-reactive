# Plain-English Names for the Redesign

> Source-grounded synthesis. Inputs: the unified decomposition at
> [`02-micro-modules.md`](./02-micro-modules.md), the source-verified baseline at
> [`01-connectivity-graph.md`](./01-connectivity-graph.md), the active vocabulary
> in [`reactive-plan-domain-language.md`](../../reactive-plan-domain-language.md),
> and the actual C#/TS source under `Alis.Reactive/` and
> `Alis.Reactive.Assets/runtime/`.
>
> This pass names **every module and every key concept** in plain English a .NET
> developer reads once and gets. A name that needs a sentence to explain is
> rejected here, not shipped.

---

## The Naming Test

Before a name is kept it must pass this test. If it fails, it is reworked.

> **Read it cold.** Show the name alone — no doc, no signature, no neighbour
> — to a .NET developer who has never seen this codebase. If they can say what
> it does in one breath and be right, the name passes. If they have to guess, ask,
> or get it subtly wrong, the name fails and is renamed.

This is why `ReactionPipelineDraft` beats `PipelineBuilder` (a "pipeline" of what?
"builder" of what?), why `BrowserObjectContract` beats `TypeKey` (a key for which
type?), and why `IdGenerator` survives unchanged (it generates ids — nothing to
explain).

---

## Naming Principles Applied

Seven principles, applied in order. Earlier ones win ties.

1. **Name the thing, not the pattern.** The noun is the domain concept, not the
   GoF role. `Trigger`, `Reaction`, `Condition`, `Request` — never `TriggerHandler`,
   `ReactionManager`, `ConditionService`, `RequestProcessor`. A `*Builder` suffix
   is kept only on fluent C# builders a developer actually chains (`HttpRequestBuilder`,
   `GuardBuilder`), because there the suffix *is* the thing — the type you call
   `.Get(...).Gather(...)` on. It is banned everywhere else.

2. **The name answers "what does it do," in one verb-or-noun line.** Every entry
   below carries a one-line gloss. If the gloss needs an "and" joining two unrelated
   jobs, the concept is doing two things and is split — not named cleverly to hide
   it. (`ComponentObject` did six jobs; it became `BrowserObject` + `ComponentRole`
   + `InputBinding` + a validation home.)

3. **Screaming intent over generic words.** A name announces *when* it lives and
   *who* writes it: authoring-time fluent surface, plan-model node, or runtime
   executor. This is how the two `ValidationRule` types separate into
   `ClientRule` (what the developer writes) and `ValidationRuleNode` (what the plan
   carries). Banned generic words from the domain-language Naming Guard stay banned:
   `artifact`, `contribution`, `claim`, `reject`, `fallback`, `registry`
   (unless a true lookup), `lifecycle` (unless true browser load/unload), `Manager`,
   `Helper`, `Util`, `Info`, `Data`, `Context` (unless a true ambient scope).

4. **One concept, one name, everywhere.** The same idea wears the same word across
   C#, JSON, generated TS, runtime, tests, and docs. A value is a `Value` from
   `TypedSource` to `ValueExpression` to `evaluateValue` — never "source" here and
   "producer" there. Collisions are forbidden: two types may not share a name
   (the `ValidationRule` × 2 and `RequestReaction.Request`-via-`new` collisions are
   resolved below).

5. **Names match the dependency direction.** A kernel every concept leans on gets a
   short, bare noun — `Shape`, `Kind`, `Plan`. A concept slice gets the domain verb
   the matrix row already uses — `Trigger`, `Reaction`, `Condition`. The size of the
   name tracks how foundational it is; foundational things are short because they
   are said most often.

6. **Lane and purity are in the name when they matter.** Where the sync/async lane
   or pure-vs-effect split is the whole point of a type, the name says so:
   `evaluateValue` (pure read, no name says "async" because it never is),
   `ReactionPipelineDraft` *stamps* the lane, `AsyncEffect` vs `SyncEffect` reading
   for reaction edges. A developer should not open the file to learn whether a thing
   can block.

7. **Keep good existing names; rename only to remove confusion.** A name already
   doing its job is not churned for novelty. `IdGenerator`, `PlanDocument`,
   `BrowserObjectContract`, `ConditionGraph`, `ReactionGraph`, `Shape`, `GatherBuilder`,
   `TypedSource<T>`, `evaluateValue`, `assertNever` all survive. Renames happen only
   where the old name lies, collides, or needs a paragraph (`PipelineBuilder`,
   `TypeKey`, `RuntimePlan`, the duplicate `ValidationRule`, `RequestReaction.Request`,
   `activeRuntimePlan`).

---

## Module Names

The 12 modules. Each name is a domain noun a .NET developer already owns. No name
needs a gloss to be understood — the gloss only confirms.

| Module | Says what it does |
|---|---|
| **Trigger** | When a behavior starts — page-ready, an event, a component callback, a server push. |
| **Reaction** | What runs when a trigger fires — set, call, dispatch, inject, show errors, in order. |
| **Condition** | The if/else-if/else decision over readable values; first match wins. |
| **Value** | Every value the plan can read — a literal, a component member, a URL part, a payload field. |
| **Request** | The HTTP call: gather the body, fetch, route success/error, chain, run in parallel. |
| **Component** | A browser object with an id, a vendor, a type, and a member contract. |
| **Slot** | Compose plans: join by plan id on the server, load/unload partials by slot id in the browser. |
| **Validation** | Client-side rules recorded for a field and run in the browser; the server stays the authority. |
| **Plugin** | The typed escape hatch — declare a browser object the DSL does not model, then read/call it. |
| **Shape** *(kernel)* | The structural type tag carried on every value, so the same bytes convert the same way everywhere. |
| **Kind** *(kernel)* | The one discriminator that tells C# and TS apart which node this is, and generates the TS contract from it. |
| **Plan** *(spine)* | The document the slices write into and the runtime boots from — build, freeze, serialize, discover, boot. |

Each module name *is* the coverage-matrix row's domain term. There is nothing to
translate between "the module a developer opens" and "the concept they were
thinking about."

---

## Key Concepts — Names, Glosses, and Old → New

Organized by module. `→` is the C# authoring/plan side; `⇒` is the TS runtime side.
"Old" is the current source name from `Alis.Reactive/` or
`Alis.Reactive.Assets/runtime/`; a dash means the concept is new (closes a hole).

### Shape (kernel)

| New name | Says what it does | Old name |
|---|---|---|
| `Shape` | The structural type tag on a value: scalar, object, or array of a shape. | `Shape` *(kept)* |
| `ShapeStructure` | Whether the shape is scalar, object, or array — the structure axis. | `ShapeStructure` *(kept)* |
| `ShapeContractCompatibility` | Whether one shape may flow into a contract member of another shape. | `ShapeContractCompatibility` *(kept)* |
| `ShapeConverter` ⇒ | The one runtime engine that converts a value to its declared shape. | `shape-convert.ts` *(kept file; `applyShape`/`convertByShape` stay)* |
| **shape-once rule** | The invariant: a value is shaped exactly once, on the gather egress path. | — *(replaces the 3 re-shapings: `evaluate`, gather re-derive, `wire-format.ts`/`formatForWire`)* |

### Kind (kernel)

| New name | Says what it does | Old name |
|---|---|---|
| `Kind` | The string discriminator each plan node carries so both sides agree which node it is. | the scattered `Kind` properties *(kept, unified)* |
| `PlanNodeDiscriminator` → | The one polymorphic mechanism that writes `kind` from a compile-enforced base. | `WriteOnlyPolymorphicConverter` + 11 hand `JsonConverter`s *(collapsed to one)* |
| `PlanContractGenerator` → | Reflects the C# node families and writes `plan.ts` from them. | `PlanTypeScriptContract` + `TypeScriptContractWriter` *(1,165-line hand mirror, deleted)* |
| `PlanSerializer` → | The single owner of plan-to-JSON, camelCase. | `ReactivePlanSerializer` / `PlanJsonWriter` *(unified, renamed for clarity)* |
| `ContractDriftGate` → | Build step that fails if `plan.ts` disagrees with the C# node families. | — *(new; CLAUDE.md claimed generation existed — it did not)* |
| `assertNever` ⇒ | Compile-time proof a switch handled every `Kind`. | `assert-never.ts` / `assertNever` *(kept)* |

### Value

| New name | Says what it does | Old name |
|---|---|---|
| `TypedSource<T>` → | The one typed authoring surface for any readable value. | `TypedSource<T>` *(kept; absorbs Component/Url/Plugin/Payload/Element source families)* |
| `ValueExpression` → | The flat plan node for a value: literal, read, object, array, or array-op. | `ValueExpression` *(kept name; the 590-line god-facade is split, see below)* |
| `Literal` / `Read` / `ObjectValue` / `ArrayValue` / `ArrayOp` → | The five value-node variants — what a value can be. | the same union, flattened *(was tangled with `ValueRead`→`ValueReadTarget`→`ValueReadPath` 4-type indirection)* |
| `WholePayload` → | "The entire response body," as a real node — not a sentinel string. | `responseBody` magic sentinel |
| `WholeElement` → | "The entire element value," as a real node — not a sentinel string. | `elementValue` magic sentinel |
| `FilterOp` / `MapOp` / `SumOp` / … (per-op variants) → | Each array operation as its own node, carrying only the fields it needs. | nullable predicate/projection + `[JsonIgnore]` pairs *(replaced by per-op variants)* |
| `evaluateValue` ⇒ | Reads a value from its node — pure, no IO, no DOM writes. | `evaluate.ts` / `evaluateValue` *(kept; the 300-line god-class is split)* |
| `ArrayOpEngine` ⇒ | Runs count/filter/map/sum/any/all/find/orderBy over an array value. | inline in `evaluate.ts` *(extracted to its own module)* |
| `RuntimeValue` / `RuntimeShape` / `RuntimePath` ⇒ | The runtime's view of a read value, its shape, and its JSON path. | `runtime-value.ts` / `runtime-shape.ts` / `runtime-path.ts` *(kept)* |
| **gather source closed** | A gather assignment now reads through `TypedSource` like everything else. | — *(closes the gather-source hole; no second resolver)* |

### Condition

| New name | Says what it does | Old name |
|---|---|---|
| `When` / `Confirm` → | The fluent entry to a condition, and a confirm-the-user guard. | `When` / `Confirm` *(kept)* |
| `GuardBuilder` / `BranchBuilder` / `ConditionContinuation` → | Compose guards, branch cases, and the then/else-if/else chain. | same *(kept; `Standalone.Then` made unrepresentable, not a runtime throw)* |
| `ConditionGraph` → | The deterministic predicate node: compare, all, any, not, confirm. | `ConditionGraph` *(kept)* |
| `ComparisonOperands` → | The left and right values a compare reads. | `ComparisonOperands` *(kept, collapsed to one shape)* |
| `CompareOp` → | The 21 comparison tokens — the single source of the op list. | scattered op arrays in `PlanTerms` *(unified)* |
| `CompareEngine` ⇒ | The one engine that evaluates a comparison — used by both lanes. | — *(unifies the two divergent evaluators)* |
| `evaluateCondition` ⇒ | Reads a condition graph to true/false using `CompareEngine`. | `sync-condition.ts` *(the sync core)* |
| `confirmThenEvaluate` ⇒ | The async wrapper that awaits a confirm, then delegates to `CompareEngine`. | `conditions.ts` *(the async recursion; now a thin wrapper, no divergence)* |
| **(removed)** | — | `ValueEvaluator` DI threaded through 8 functions *(removed by layered dependence on Value)* |

### Reaction

| New name | Says what it does | Old name |
|---|---|---|
| `ReactionPipelineDraft` → | Sequences sync/async/branch reaction nodes and stamps each node's lane. | `ReactionPipelineDraft` *(kept name; `PipelineBuilder` 4-partial god-builder folded in)* |
| `ElementBuilder` / `DispatchPayloadBuilder` → | Author element mutations and dispatch payloads. | `ElementBuilder` / `DispatchPayloadBuilder` *(kept, each focused)* |
| `ReactionGraph` → | The executable action node: set, call, dispatch, inject, show-validation, sequence, branch, request. | `ReactionGraph` *(kept)* |
| `RequestReaction.Request` → | A reaction node that runs an HTTP request. | `RequestReaction.Request` declared with `new` *(collision removed — the `new` keyword hack is dropped; the property is the only `Request` on the node)* |
| `ReactionLane` → | The plan-carried fact: this node runs sync or async. | — *(new; replaces `instanceof Promise` / `crossedAsyncBoundary` re-detection)* |
| `executeReaction` ⇒ | Routes a reaction node by `Kind` and its carried lane — switch + `assertNever`. | `execute.ts` / `executeReaction` *(kept; reduced to routing on the carried lane)* |

### Request

| New name | Says what it does | Old name |
|---|---|---|
| `HttpRequestBuilder` → | Author a Get/Post/Put/Delete with gather, response, chain, parallel. | `HttpRequestBuilder` *(kept)* |
| `GatherBuilder` / `Include` → | Map readable values onto route params, headers, query, or body. | `GatherBuilder` / `Include` *(kept)* |
| `ResponseBuilder` / `ParallelBuilder` → | Route success/error scopes; run requests concurrently. | `ResponseBuilder` / `ParallelBuilder` *(kept)* |
| `RequestPlan` → | The HTTP node: method, URL, gather, response routes, chain, parallel. | `RequestPlan` *(kept)* |
| `GatherAssignment` → | One `target <- value` mapping in a gather. | `GatherRequestInput` / `RequestInput` *(renamed: a gather assigns a value to a target; the old name read like an input record)* |
| `ResponseRouting` / `ResponseRoute` / `RequestChain` → | Where success/error go; the next request after success. | same *(kept)* |
| `PayloadScope` → | The scope a value reads from — event, success, error, request, dispatch. | `PayloadScope` *(kept; folded to only scopes that carry data — dead `local` scope removed)* |
| `http` ⇒ | The pipeline: gather → fetch → route → finally → chain. | `http.ts` *(kept)* |
| `gather` / `RequestPayloadWriter` / `httpFetch` ⇒ | The named stages: build the payload, write it, fetch. | `gather.ts` / `request-payload-writer.ts` / `http-fetch.ts` *(kept)* |
| `RequestPayloadWriter` ⇒ | The single owner of FormData/File body writing. | FormData/File knowledge scattered across 3 modules *(consolidated)* |

### Trigger

| New name | Says what it does | Old name |
|---|---|---|
| `Html.On` / `TriggerBuilder` → | Author when a behavior starts. | `Html.On` / `TriggerBuilder` *(kept)* |
| `StartsWhen` → | The trigger node: page-ready, document event, component event, server push, SignalR. | `StartsWhen` *(kept; made symmetric — public sealed + explicit `Kind`)* |
| `Behavior` → | One trigger-to-reaction edge. | `Behavior` *(kept; internal-class/public-props asymmetry removed)* |
| `BehaviorGraph` → | All the behaviors in a plan. | `BehaviorGraph` *(kept)* |
| `wireTrigger` ⇒ | Wires the browser listener for one `StartsWhen` kind. | `trigger.ts` *(kept)* |
| `ExecutionContext` ⇒ | The one context carrying the trigger's payload into the reaction. | `execution-context.ts` + the separate raw `ExecContext` *(unified — one context, no raw-vs-rich double threading)* |

### Component

| New name | Says what it does | Old name |
|---|---|---|
| `IdGenerator` → | Generates a component's deterministic id from model type + expression. | `IdGenerator` *(kept — the one id regime)* |
| `Html.InputField` / `InputBoundField` → | Render a model-bound input and its label/validation slot. | `Html.InputField` / `InputBoundField` *(kept)* |
| `ModelBoundInputComponentSlot` → | The render slot that ties a model property to a vendor component. | `ModelBoundInputComponentSlot` *(kept)* |
| `BrowserObject` → | A registered browser object: id, vendor, type, role, binding. | `ComponentObject` *(renamed — it is the page-object the runtime talks to, not a "component object"; god-file split: role/binding/validation extracted)* |
| `ComponentRole` → | What an entry is for: object-target, plan-input, validation-container, layout-object. | `ComponentRole` *(kept)* |
| `InputBinding` → | The model property a plan-input is bound to. | `InputBinding` / `RegisteredInputBinding` *(kept, clarified)* |
| `BrowserObjects` → | The repository of declared browser objects, with the same-vendor invariant. | `ComponentObjects` *(renamed to match `BrowserObject`)* |
| `BrowserObjectContract` → | The vendor-agnostic member contract: properties, methods, events. | `BrowserObjectContract` *(kept)* |
| `BrowserObjectId` → | The `(vendor, kind, id)` value object that names one object. | `TypeKey` *(renamed — "TypeKey" never said what it keyed; the new name says it identifies a browser object, and it stops being an opaque parsed string)* |
| `RuntimeComponents` ⇒ | The runtime's lookup of all components in the active plan. | `runtime-plan.ts` (`RuntimePlan` join) *(split out)* |
| `RuntimeObject` ⇒ | One resolved browser object — DOM element + vendor root, memoized. | `runtime-object.ts` *(kept; now memoized, not rebuilt per read)* |
| `ComponentDriver` ⇒ | The per-vendor driver — the sole place vendor knowledge lives. | `component-runtime.ts` (`ComponentRuntime`) *(renamed: it drives a vendor component; the sole vendor seam)* |
| `wireFusionEvent` / `wireNativeEvent` ⇒ | Vendor-specific event wiring — only these two files know a vendor. | `event-fusion.ts` / `event-native.ts` *(kept; stale `resolver.ts` Rule-5 claim fixed)* |

### Slot

| New name | Says what it does | Old name |
|---|---|---|
| `PlanScope` → | Whether a plan is root (SSR-merged) or partial (slot-loadable). | `PlanScope` *(kept)* |
| `SlotId` → | The browser handle for loading/unloading a partial. | `SlotId` / `PartialSlotId` *(kept)* |
| `injectPartial` ⇒ | Loads partial HTML into a slot and recomposes the active plan. | `inject.ts` *(kept)* |
| `AppliedPlans` ⇒ | The composition state: boot snapshots + loaded slots + abort controllers. | `browser-plans.ts` (`AppliedBrowserPlans`) *(renamed — drops the redundant "Browser"; it is the set of plans currently applied)* |
| `recompose` ⇒ | Builds a **new** `PlanDocument` from the snapshot plus loaded slots. | `resetPlanDocument` *(renamed — it composes, it does not "reset"; and it no longer mutates in place)* |
| `MergePolicy` | The one replace-vs-append rule, shared by the C# container merge and TS recompose. | divergent C# (replace) vs TS (append) rules *(unified)* |

### Validation

| New name | Says what it does | Old name |
|---|---|---|
| `ReactiveValidator<T>` → | Where a developer records client rules for a model. | `ReactiveValidator<T>` *(kept)* |
| `ClientRule` / `WhenField` → | One client rule; a rule that applies only when a field condition holds. | `ClientRule` / `WhenField` *(kept)* |
| `ClientValidationFieldRuleBuilder` → | Fluent surface for a field's 16 rule types. | `ClientValidationFieldRuleBuilder` *(kept)* |
| `ValidationRuleNode` → | The plan-model node carrying one validation rule. | `PlanModel`'s `ValidationRule` *(renamed — ends the two-`ValidationRule` collision: the developer writes a `ClientRule`, the plan carries a `ValidationRuleNode`)* |
| `RuleName` → | The single source of rule names — a TS union generated from C#. | three independent rule-name enumerations *(unified to one)* |
| `RuleOperand` → | The one operand model for a rule's values. | operands modeled twice *(unified; `rule-operands.ts` kept)* |
| `CollectionItemBinding` → | Binds a rule to one item in a collection field. | substring path arithmetic *(replaced by a real value object)* |
| `ValidationGraph` → | The plan's validation rules, in their own home. | buried in `ComponentObject.cs` *(extracted, flattened)* |
| `validationOrchestrator` / `ruleEngine` / `errorDisplay` / `liveClear` ⇒ | Run rules, show errors, clear on input — reusing `CompareEngine` for `WhenField`. | same files *(kept; `WhenField` now reuses Condition's `CompareEngine`)* |
| `ErrorElementNaming` | The one constant for `{id}_error` and `{planId}_validation_summary`. | ad-hoc string building in multiple places *(one shared constant)* |

### Plugin

| New name | Says what it does | Old name |
|---|---|---|
| `Plugin` → | Declare a plugin browser object: typed properties + operations. | `ReactivePlugin` + `PluginTypeBuilder` *(two parallel declaration APIs collapsed to one)* |
| `PluginMemberBuilder` → | The one args-builder-first surface to read a property or call an operation. | `PluginReadBuilder` + `PluginCallBuilder` *(two ~95%-identical builders + ~30 arity overloads collapsed)* |
| `PluginContract` → | The declared member contract for a plugin, mapped to `BrowserObjectContract`. | `PluginContract` *(kept)* |
| `PluginCatalog` ⇒ | Host-registered plugin instances; resolving an unknown one throws at the boundary. | `plugin-catalog.ts` *(kept — a true lookup at a real external edge)* |

### Plan (spine)

| New name | Says what it does | Old name |
|---|---|---|
| `PlanBuildContext` → | The authoring sink slices write into — narrow Declare/Wire verbs. | `PlanBuildContext` *(kept)* |
| `PlanDocument` → | The immutable, serializable plan for one model identity (version 3). | `PlanDocument` *(kept)* |
| `PlanId` → | The stable model-derived key that composes root + same-model partials. | `PlanId` *(kept)* |
| `Html.ReactivePlan` / `ResolvePlan` / `RenderPlan` → | Create, resolve, and serialize a plan in a view. | `PlanExtensions` *(kept)* |
| `root` ⇒ | Discovers `[data-reactive-plan]` scripts at boot. | `root.ts` *(kept)* |
| `boot` ⇒ | Wires each composed plan, passing the active plan explicitly to `executeReaction`. | `boot.ts` *(kept; no callback cycle)* |
| `ActivePlan` ⇒ | The active plan, passed explicitly down — not a hidden singleton. | `activeRuntimePlan` global *(removed; with it the 4 `reset*ForTests` functions and the boot↔browser-plans callback cycle)* |

---

## Collisions Resolved by Screaming Names

These three were the named pain in the design. Each is resolved by a name that
announces *when the type lives and who writes it*.

| Collision | Resolution |
|---|---|
| Two `ValidationRule` types (`Validation/ValidationRule.cs` + the one inside `ComponentObject.cs`) | The authoring concept stays `ClientRule` (what the developer writes); the plan-model concept becomes `ValidationRuleNode` (what the plan carries). No two types named `ValidationRule`. |
| `RequestReaction.Request` declared with `new` (`ReactionGraph.cs:314`) to shadow a base member | The base no longer exposes a colliding `Request`; the node has exactly one `Request` property, so the `new` keyword and the shadowing hack are deleted. |
| Stale `resolver.ts` "Rule 5 — sole vendor seam" claim that is no longer true | The vendor seam is named honestly: `ComponentDriver` + `wireFusionEvent`/`wireNativeEvent` are the only vendor-aware code, and the comment matches reality. |

---

## What Was Deliberately *Not* Renamed

Per principle 7, these names already pass the read-it-cold test and were left
alone to avoid churn: `Shape`, `Kind`, `IdGenerator`, `PlanDocument`, `PlanId`,
`PlanBuildContext`, `BrowserObjectContract`, `ConditionGraph`, `ReactionGraph`,
`RequestPlan`, `StartsWhen`, `Behavior`, `BehaviorGraph`, `TypedSource<T>`,
`ValueExpression`, `evaluateValue`, `assertNever`, `GatherBuilder`,
`HttpRequestBuilder`, `ComponentRole`, `ReactiveValidator<T>`, `ClientRule`,
`PluginContract`, `PluginCatalog`, `SlotId`, `PlanScope`.

A name is renamed only when it lies, collides, or needs a paragraph — not for
novelty.
