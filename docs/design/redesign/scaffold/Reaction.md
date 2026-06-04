# Reaction — Implementation Spec

> One of the 12 redesign micro-modules. Open this file, read the surface and the
> fixtures, type the obvious body. Every name comes from
> [`03-naming.md`](../03-naming.md); every responsibility from
> [`02-micro-modules.md`](../02-micro-modules.md); every acceptance fixture from
> [`04-matrix-triggers-reactions-conditions.md`](../04-matrix-triggers-reactions-conditions.md).
> Grounded against the actual source: `Builders/PipelineBuilder*.cs`,
> `Builders/ElementBuilder.cs`, `Builders/DispatchPayloadBuilder.cs`,
> `Builders/ReactionPipelineDraft.cs`, `Builders/IReactionEmitter.cs`,
> `PlanModel/ReactionGraph.cs`, `runtime/execution/execute.ts`,
> `runtime/execution/inject.ts`, `runtime/types/plan.ts`.

---

## 1. Responsibility

**Reaction owns *what runs when a trigger fires* — the `p.*` command surface and
the executable action graph (set, call, dispatch, inject, show-validation-errors,
sequence, branch) — and stamps each node's sync/async lane into the plan so the
runtime routes on a carried fact, never on `instanceof Promise`.**

### Owns

| Side | Artifacts |
|---|---|
| `→` author | The thin command sink `PipelineBuilder<TModel>` (the `p` parameter); the focused `ElementBuilder<TModel>` and `DispatchPayloadBuilder<TPayload,TModel>`; the `ReactionPipelineDraft<TModel>` sequencer that **stamps the `ReactionLane`**; the `IReactionEmitter` seam vendor slices emit through. |
| `→` plan node | `ReactionGraph` family: `SequenceReaction`, `BranchReaction`, `SetReaction`, `CallReaction`, `DispatchReaction` (`DispatchPayload`: `NoDispatchPayload`/`PresentDispatchPayload`), `InjectReaction`, `ShowValidationErrorsReaction`. `RequestReaction`/`ParallelReaction` are **authored** by Reaction (via `BeginHttp`/`BeginParallel`) but the node bodies are **owned by Request** (`RequestReaction.Request` carries `RequestPlan`). `BranchCase`/`BranchGuard` are **owned by Condition**; Reaction only sequences them. |
| `⇒` runtime | `executeReaction` (switch + `assertNever`) and its per-kind readers `executeSet`, `executeCall`, `executeDispatch`, `executeInject`, `executeShowValidationErrors`, `executeSequence`, `executeBranch`; lane routing on the carried `ReactionLane`. |

### Depends on (downward only — see the acyclic graph in `00-design.md` §2)

- **Value** — every `set` value, `call` arg, and dispatch field is one
  `ValueExpression`; the runtime reads them with `evaluateValue`.
- **Condition** — `branch` guards are `ConditionGraph`; the runtime reads them with
  `evaluateConditionInCurrentLane` (Condition's `CompareEngine`). Reaction never
  evaluates a condition itself.
- **Request** — `BeginHttp`/`BeginParallel` open the async lane; `RequestReaction`
  wraps a `RequestPlan` that Request builds. Runtime delegates to `executeRequest`.
- **Slot** — `executeInject` injects into a slot via `injectHtml` (Slot owns the
  recompose; Reaction only fires the inject node).
- **Component** — `set`/`call` on a `ComponentSource` resolve a `RuntimeObject`
  through the active plan (`plan.objectForSource`); `inject`/`show-validation-errors`
  resolve a container/element id.
- **Kind** (kernel) — each node carries `kind`; the runtime switch ends in
  `assertNever`. The contract is generated, not hand-mirrored.

It does **not** own: triggers (Trigger calls `pb.BuildReaction()`), the value union,
the condition graph, the HTTP node body, validation rules, or the plan document.

---

## 2. Public Surface (exact signatures, XML-doc intent)

Visibility is frozen (Rule 8): plan-node constructors are `internal`, factory
methods are `internal static`, `Kind`/payload props are `public` get-only. Builder
constructors are `internal`; developers reach builders through `Html.On`/`p`. The
matrix never asks the developer to pass a lane — `ReactionPipelineDraft` infers it.

### 2.1 Author surface — `PipelineBuilder<TModel>` (`Builders/`)

The command sink received as `p`. Mostly already shaped in source; the spec freezes
the verbs Reaction owns. (HTTP verbs `Get/Post/Put/Delete/Parallel` live in the
`PipelineBuilder.Http.cs` partial and delegate to `_draft.BeginHttp/BeginParallel` —
they are Request's authoring entry, kept here only because `p` is the sink.)

```csharp
public partial class PipelineBuilder<TModel> : IReactionEmitter where TModel : class
{
    internal PlanBuildContext Context { get; }
    internal PipelineBuilder(PlanBuildContext context);

    /// <summary>Adds a reaction step to the current pipeline (vendor/ComponentRef seam).</summary>
    void IReactionEmitter.AddStep(ReactionGraph step);
    /// <summary>Gets the plan build context for component registration.</summary>
    PlanBuildContext IReactionEmitter.BuildContext { get; }
    internal void AddStep(ReactionGraph step);   // → _draft.AddCommand(step)

    /// <summary>Dispatches a custom browser event by name with no payload.</summary>
    public PipelineBuilder<TModel> Dispatch(string eventName);

    /// <summary>Dispatches a custom browser event with a compile-time literal payload.</summary>
    public PipelineBuilder<TModel> Dispatch<TPayload>(string eventName, TPayload payload);

    /// <summary>Dispatches a custom event whose fields resolve from live sources at dispatch time.</summary>
    public PipelineBuilder<TModel> DispatchWith<TPayload>(
        string eventName, Action<DispatchPayloadBuilder<TPayload, TModel>> configure)
        where TPayload : class;

    /// <summary>Targets a DOM element by id for mutations (SetText, AddClass, Show, Hide).</summary>
    public ElementBuilder<TModel> Element(string elementId);

    /// <summary>Displays accumulated validation errors in the named container.</summary>
    public PipelineBuilder<TModel> ValidationErrors(string formId);   // → ShowValidationErrors node

    /// <summary>Injects the HTTP success body into a DOM element as HTML. Must follow a request.</summary>
    public PipelineBuilder<TModel> Into(string elementId);            // → Inject node, value fixed to ReadWholePayload(Success)

    // Branch wiring (called by Condition continuations; not authored directly):
    internal void SetConditionalBranches(List<BranchCase> branches);  // → _draft.SetConditionalBranches
    internal void FlushSegment();                                     // → _draft.FlushSegment
    internal ReactionGraph BuildReaction();                           // → _draft.BuildReaction (Trigger/Request call this)
}
```

> `Component<…>(…)` (the `ComponentRef` entry), `FromUrl`, `Plugin*`, `From*` array
> entries, and the `When/Confirm` partials are authored on `PipelineBuilder` but are
> the *authoring entry points of Component / Value / Plugin / Condition*. Reaction
> keeps the method on `p` (the one sink) but does not own their lowering. They are
> out of scope for Reaction's acceptance fixtures.

### 2.2 Author surface — `ElementBuilder<TModel>` (`Builders/ElementBuilder.cs`)

Element mutations. Every method lowers to a `set` or `call` on a **`ComponentSource`**
(never `dom` — see matrix open-question #2), declaring the element + member contract
on `Context` first.

```csharp
public class ElementBuilder<TModel> where TModel : class
{
    internal ElementBuilder(PipelineBuilder<TModel> pipeline, string elementId); // declares element on Context

    public PipelineBuilder<TModel> AddClass(string className);     // call addClass
    public PipelineBuilder<TModel> RemoveClass(string className);  // call removeClass
    public PipelineBuilder<TModel> ToggleClass(string className);  // call toggleClass
    public PipelineBuilder<TModel> SetText(string text);                                       // set text, literal
    public PipelineBuilder<TModel> SetText<TSource>(TSource source, Expression<Func<TSource,object>> path);     // set text, read(payload event)
    public PipelineBuilder<TModel> SetText<TResponse>(ResponseBody<TResponse> source, Expression<Func<TResponse,object>> path) where TResponse : class; // set text, read(payload success/error)
    public ElementBuilder<TModel> SetText<TProp>(TypedSource<TProp> source);                   // set text, source.ToValueExpression()
    public PipelineBuilder<TModel> SetHtml(string html);                                       // set html, literal
    public PipelineBuilder<TModel> SetHtml<TSource>(TSource source, Expression<Func<TSource,object>> path);     // set html, read(payload event)
    public ElementBuilder<TModel> SetHtml<TProp>(TypedSource<TProp> source);                   // set html, source.ToValueExpression()
    public PipelineBuilder<TModel> Show();   // set hidden = literal false
    public PipelineBuilder<TModel> Hide();   // set hidden = literal true

    // private Set(property, value)  -> Context.DeclareProperty(write); AddStep(ReactionGraph.Set(ComponentSource.Of(key), member, value))
    // private Call(method, arg)     -> Context.DeclareMethod(returning Shape.None); AddStep(ReactionGraph.Call(ComponentSource.Of(key), member, [arg]))
}
```

### 2.3 Author surface — `DispatchPayloadBuilder<TPayload,TModel>` (`Builders/DispatchPayloadBuilder.cs`)

Builds an `object` `ValueExpression` for `DispatchWith`. Nested paths (`x.A.B`)
become nested object nodes; conflicts (leaf-vs-parent) throw at author time; an
empty payload throws (use `Dispatch(name)` for none).

```csharp
public class DispatchPayloadBuilder<TPayload, TModel> where TPayload : class where TModel : class
{
    internal DispatchPayloadBuilder();
    public DispatchPayloadBuilder<TPayload,TModel> Set<TProp>(Expression<Func<TPayload,TProp>> field, TypedSource<TProp> source); // read source
    public DispatchPayloadBuilder<TPayload,TModel> Set(Expression<Func<TPayload,string>> field, string value); // literal
    public DispatchPayloadBuilder<TPayload,TModel> Set(Expression<Func<TPayload,int>> field, int value);       // literal
    public DispatchPayloadBuilder<TPayload,TModel> Set(Expression<Func<TPayload,bool>> field, bool value);     // literal
    internal ValueExpression Build();   // object node; throws if HasFields == false
}
```

### 2.4 Sequencer — `ReactionPipelineDraft<TModel>` (`Builders/`, `internal`)

The single accumulator that decides ordering and **stamps the lane**. The fresh-design
delta over today: `ReactionLane` is computed once at `BuildReaction`/`FlushSegment` and
carried on each block (see §2.6); the runtime no longer probes `instanceof Promise`.

```csharp
internal sealed class ReactionPipelineDraft<TModel> where TModel : class
{
    internal HttpRequestBuilder<TModel> BeginHttp(PlanBuildContext context);     // async opener -> pending request
    internal ParallelBuilder<TModel>   BeginParallel(PlanBuildContext context);  // async opener -> pending parallel
    internal void BeginBranch();                                                 // flushes pending async + branch
    internal void SetConditionalBranches(List<BranchCase> branches);             // arms a pending branch at current sync count
    internal void AddCommand(ReactionGraph reaction);                            // sync command -> _pendingSyncReactions
    internal void FlushSegment();                                                // flush async, branch, then sync
    internal ReactionGraph BuildReaction();                                      // FlushSegment(); single block -> bare, else Sequence(blocks)
}
```

Ordering invariant (already correct in source — preserve exactly): sync commands
accumulate in author order; an async opener (`BeginHttp`/`BeginParallel`) or a branch
flushes the leading sync block, appends the opener/branch node, and the trailing sync
block is flushed after. `BuildReaction` returns the bare node when there is exactly
one ordered block, else a `sequence`.

### 2.5 Plan nodes — `ReactionGraph` family (`PlanModel/ReactionGraph.cs`)

Base + factories (all `internal static`); each concrete node `sealed`, `internal`
ctor, `public string Kind => "<token>"`. Frozen exactly as source — listed so the
generator emits the matching wire interface and runtime case.

```csharp
[JsonConverter(typeof(WriteOnlyPolymorphicConverter<ReactionGraph>))]
public abstract class ReactionGraph
{
    private protected ReactionGraph();
    internal static ReactionGraph Sequence(List<ReactionGraph> steps);
    internal static ReactionGraph Branch(List<BranchCase> cases);
    internal static ReactionGraph Set(Source on, string property, ValueExpression value);
    internal static ReactionGraph Call(Source on, string method, IReadOnlyList<ValueExpression> args);
    internal static ReactionGraph Dispatch(string eventName);                                      // None
    internal static ReactionGraph Dispatch(string eventName, ValueExpression data);                // Untyped
    internal static ReactionGraph Dispatch(string eventName, ValueExpression data, PayloadContract payloadType); // Typed
    internal static ReactionGraph Inject(string slot, ValueExpression value);
    internal static ReactionGraph ShowValidationErrors(string container);
    // Request / Parallel factories are co-located but author Request's node body.
    internal static ReactionGraph Request(RequestPlan request);
    internal static ReactionGraph Parallel(List<ReactionGraph> steps, ParallelCompletion completion);
    internal static IReadOnlyList<ReactionGraph> OrderedSteps(IEnumerable<ReactionGraph> steps);
}

public sealed class SequenceReaction : ReactionGraph            // kind "sequence";  Steps
public sealed class BranchReaction   : ReactionGraph            // kind "branch";    Cases (Condition-owned BranchCase[])
public sealed class SetReaction      : ReactionGraph            // kind "set";       On (Source), Property (MemberName), Value
public sealed class CallReaction     : ReactionGraph            // kind "call";      On (Source), Method (MemberName), Args
public sealed class DispatchReaction : ReactionGraph            // kind "dispatch";  Event (EventName), PayloadForJson (DispatchPayload)
public sealed class InjectReaction   : ReactionGraph            // kind "inject";    Slot (ComponentKey), Value
public sealed class ShowValidationErrorsReaction : ReactionGraph // kind "show-validation-errors"; Container (ComponentId)
```

> **Collision resolved (`03-naming.md`).** Today `RequestReaction.Request` is
> declared `public new RequestPlan Request` to shadow a base member. In the fresh
> design the base `ReactionGraph` exposes no colliding `Request`, so the node has
> exactly one `Request` property and the `new` keyword is **deleted**. Reaction does
> not author `RequestReaction`'s body; it only sequences the node.

### 2.6 New plan-carried fact — `ReactionLane` (`PlanModel/`, fresh design)

The one genuinely new type Reaction adds. It is stamped at authoring time and read at
runtime instead of `instanceof Promise` re-detection. Two values only — value object,
no null:

```csharp
/// <summary>The plan-carried fact that a reaction node runs synchronously or
/// asynchronously. Stamped by <see cref="ReactionPipelineDraft{TModel}"/> at
/// authoring time; read by the runtime instead of re-detecting a Promise.</summary>
public sealed class ReactionLane
{
    /// <summary>The synchronous lane: void completion, same browser tick (SF args.cancel visible).</summary>
    public static ReactionLane Sync { get; }
    /// <summary>The asynchronous lane: a node reaches a request, parallel, or confirm.</summary>
    public static ReactionLane Async { get; }
    /// <summary>Gets the wire token: "sync" or "async".</summary>
    public string Value { get; }
    private ReactionLane(string value);
    /// <summary>Async wins: a sequence/branch is async if any block/case reaches the async lane.</summary>
    internal static ReactionLane Combine(ReactionLane a, ReactionLane b);
}
```

The lane is carried on `SequenceReaction` and `BranchReaction` (the composite nodes)
as `public ReactionLane Lane { get; }` so the runtime can route a sequence/branch
without probing. Leaf nodes (`set`/`call`/`dispatch`/`inject`/`show-validation-errors`)
are intrinsically `Sync`; `request`/`parallel` are intrinsically `Async`; a `branch`
is `Async` iff any guard is a `confirm`, else `Sync`. **Wire shape:** add
`lane: ReactionLane` to `SequenceReaction`/`BranchReaction` in `plan.ts` (`"sync"|"async"`).

### 2.7 Runtime surface — `executeReaction` (`runtime/execution/execute.ts`)

```ts
export type ReactionCompletion = void | Promise<void>;

/// Routes a reaction node by kind + carried lane. Sync kinds return void;
/// request/parallel/confirm-reaching branch return Promise<void>.
export function executeReaction(
  reaction: ReactionGraph, plan?: PlanDocument, ctx?: ExecContext,
): ReactionCompletion;

// Per-kind readers (module-internal):
function executeSet(r: SetReaction, plan, ctx): void;          // evaluateValue -> objectForSource.set | payload[prop]=
function executeCall(r: CallReaction, plan, ctx): void;        // args.map(evaluateValue) -> objectForSource.call | plugin | payload-method
function executeDispatch(r: DispatchReaction, plan, ctx): void;// document.dispatchEvent(new CustomEvent(event,{detail}))
function executeInject(r: InjectReaction, plan, ctx): void;    // components.element(slot); injectHtml(container, html, slot)
function executeShowValidationErrors(r, plan, ctx): void;      // server payload -> showServerErrors else validateContainer
function executeSequence(r: SequenceReaction, plan, ctx): ReactionCompletion;   // route on r.lane
function executeBranch(r: BranchReaction, plan, ctx): ReactionCompletion;       // route on r.lane; first-match
```

> **Fresh-design delta in the runtime:** `executeSequence`/`executeBranch` route on
> `reaction.lane === "sync" | "async"` (the carried fact) rather than the current
> `crossedAsyncBoundary(result)` probe. The sync path returns `void` synchronously;
> the async path is the `await`-chaining variant. `request`/`parallel` remain
> delegated to Request. `assertNever(reaction, "reaction kind")` stays as the
> exhaustiveness guard. Singleton `activeRuntimePlan` and `resetActivePlanForTests`
> are removed by Plan passing `ActivePlan` explicitly (Plan's concern; Reaction just
> accepts the `plan` argument).

---

## 3. Input → Output Contract

| In (authoring) | Out (plan node) | Invariant |
|---|---|---|
| `p.Element(id).Show()` etc. | `SetReaction{ on: ComponentSource(id), property, value }` | `On`, `Property` (`MemberName`), `Value` are non-null **by construction** — `SetReaction`'s ctor takes them as required args and `MemberName.Of` rejects null/empty at the boundary; there is no nullable field and no `[JsonIgnore(WhenWritingNull)]`. |
| `p.Element(id).AddClass(c)` etc. | `CallReaction{ on, method, args }` | `Args` is never null: empty = `Array.Empty<ValueExpression>()` (the sentinel is the empty list, not null). |
| `p.Dispatch(name)` | `DispatchReaction{ event, payload: NoDispatchPayload }` | "No payload" is the real variant `NoDispatchPayload` (`kind:"none"`), not a null `data`. |
| `p.Dispatch<T>(name, obj)` / `DispatchWith<T>` | `DispatchReaction{ event, payload: PresentDispatchPayload{ data, payloadType } }` | `DispatchWith` `Build()` throws if no field set; `payloadType` is always present (`Untyped` or `ForPayload(T)`), never null. |
| `p.Into(id)` (after a request) | `InjectReaction{ slot: id, value: ReadWholePayload(Success) }` | Value is **fixed** at lowering — no value axis. `Slot` is a `ComponentKey` (rejects null/empty). |
| `p.ValidationErrors(id)` | `ShowValidationErrorsReaction{ container: id }` | `Container` is a `ComponentId` (rejects null/empty); required, no fallback container. |
| N sync commands in author order | one `SequenceReaction{ steps[], lane: Sync }` (or the bare node if N==1) | `BuildReaction` returns the bare node when exactly one ordered block exists — no `sequence` wrapper of size 1. |
| sync block, async opener, sync block | `SequenceReaction{ steps:[seq(sync), request, seq(sync)], lane: Async }` | Ordering = author order; the async opener is the lane boundary; everything before stays `Sync`. |
| `.Then/.ElseIf/.Else` (from Condition) | `BranchReaction{ cases[], lane }` | `lane: Async` iff a guard is `confirm`, else `Sync`. `BranchCase`/`BranchGuard` are Condition-owned; Reaction sequences them via `SetConditionalBranches`. |

**Runtime contract.** Given a `ReactionGraph` + active `PlanDocument` + `ExecContext`,
`executeReaction` produces the browser effect and the matching completion: `void` for
sync kinds and a sync sequence/branch; `Promise<void>` for `request`/`parallel`/a
branch reaching `confirm`/a sequence reaching one. **Boundary errors only** (Rule 6):
`executeInject` throws if the evaluated value is not a string (real type boundary);
`executeCall`/`executeSet` on a `payload` throw on a missing/non-object payload (real
runtime boundary). No defensive validation of framework-generated node shapes.

**Null is unrepresentable by construction, not guarded.** Every Reaction node field is
required in its `internal` ctor; "absence" is a real variant (`NoDispatchPayload`) or a
real sentinel (empty `Args` list, fixed `ReadWholePayload(Success)` value). No NEW
nullable property and no NEW `[JsonIgnore(Condition = WhenWritingNull)]` is introduced
by this module (Rule 6 null-escape-hatch gate).

---

## 4. File Layout

Cohesion: one concept, one place. Reaction's files (existing paths preserved; the new
`ReactionLane` is the only addition):

```
Alis.Reactive/
├── Builders/
│   ├── PipelineBuilder.cs              (sink: Dispatch/Element/ValidationErrors/Into; AddStep; BuildReaction)
│   ├── PipelineBuilder.Conditions.cs   (When/Confirm partial — Condition entry, sequenced here)
│   ├── PipelineBuilder.Http.cs         (Get/Post/Put/Delete/Parallel partial — Request entry, sequenced here)
│   ├── PipelineBuilder.Arrays.cs       (From/FromDom partial — Value/array entry)
│   ├── ElementBuilder.cs               (element set/call mutations)
│   ├── DispatchPayloadBuilder.cs       (object payload draft for DispatchWith)
│   ├── ReactionPipelineDraft.cs        (sequencer; STAMPS ReactionLane)
│   └── IReactionEmitter.cs             (vendor/ComponentRef emit seam)
└── PlanModel/
    ├── ReactionGraph.cs                (node family + factories + BranchCase/BranchGuard JSON converters)
    └── ReactionLane.cs                 (NEW — sync/async value object)

Alis.Reactive.Assets/runtime/
├── execution/
│   ├── execute.ts                      (executeReaction switch + per-kind readers; route on carried lane)
│   └── inject.ts                       (injectHtml — used by executeInject)
└── types/
    └── plan.ts                         (GENERATED by Kind: ReactionGraph union + add `lane` to Sequence/Branch)
```

`runtime/execution/http.ts` (Request) and `runtime/conditions/conditions.ts`
(Condition) are dependencies, not Reaction-owned. `plan.ts` is generated by the
**Kind** kernel — do not hand-edit it; change the C# node, regenerate, run
`npm run typecheck`.

---

## 5. Compile-Ready Skeleton

Bodies are `// TODO` referencing the §6 fixture that pins them. The signatures and
node shapes are final — a dev fills the bodies without a design decision. (Existing
files already contain correct bodies for the un-changed surface; the skeleton below
highlights the **lane stamping** delta plus the new `ReactionLane.cs`.)

### `PlanModel/ReactionLane.cs` (NEW)

```csharp
namespace Alis.Reactive.PlanModel
{
    /// <summary>The plan-carried fact that a reaction node runs sync or async.</summary>
    public sealed class ReactionLane
    {
        /// <summary>Synchronous lane: void completion, same browser tick.</summary>
        public static ReactionLane Sync { get; } = new ReactionLane("sync");
        /// <summary>Asynchronous lane: the node reaches request, parallel, or confirm.</summary>
        public static ReactionLane Async { get; } = new ReactionLane("async");
        /// <summary>Gets the wire token, "sync" or "async".</summary>
        public string Value { get; }
        private ReactionLane(string value) => Value = value;

        /// <summary>Async wins when combining lanes of sibling blocks/cases.</summary>
        internal static ReactionLane Combine(ReactionLane a, ReactionLane b)
        {
            // TODO[fixture: ordered-sync, sync-async-sync]: Async if either is Async, else Sync.
            return null!;
        }

        /// <summary>The intrinsic lane of a leaf node by kind (set/call/dispatch/inject/show=Sync; request/parallel=Async).</summary>
        internal static ReactionLane Of(ReactionGraph node)
        {
            // TODO[fixture: single-command, sync-async-sync]: leaf kinds Sync; request/parallel Async;
            // sequence/branch carry their own already-combined Lane.
            return null!;
        }
    }
}
```

### `PlanModel/ReactionGraph.cs` (composite nodes carry the lane — delta only)

```csharp
public sealed class SequenceReaction : ReactionGraph
{
    public string Kind => "sequence";
    public IReadOnlyList<ReactionGraph> Steps => _steps;
    /// <summary>Gets the carried lane; Async if any step reaches the async lane.</summary>
    public ReactionLane Lane { get; }

    internal SequenceReaction(IEnumerable<ReactionGraph> steps)
    {
        _steps = ReactionGraph.OrderedSteps(steps);
        // TODO[fixture: ordered-sync, sync-async-sync]: Lane = fold Combine over each step's ReactionLane.Of(...).
        Lane = null!;
    }
}

public sealed class BranchReaction : ReactionGraph
{
    public string Kind => "branch";
    public IReadOnlyList<BranchCase> Cases => _cases;
    /// <summary>Gets the carried lane; Async iff a guard is a confirm.</summary>
    public ReactionLane Lane { get; }

    internal BranchReaction(IEnumerable<BranchCase> cases)
    {
        _cases = OrderedCases(cases);
        // TODO[fixture: then, confirm-then, confirm-in-composition]:
        // Lane = Async if any case.Guard is a confirm ConditionGraph, else Sync.
        Lane = null!;
    }
}
```

### `runtime/types/plan.ts` (GENERATED — shows the regenerated shape)

```ts
export type ReactionLane = "sync" | "async";

export interface SequenceReaction { kind: "sequence"; steps: ReactionGraph[]; lane: ReactionLane; }
export interface BranchReaction   { kind: "branch";   cases: BranchCase[];    lane: ReactionLane; }
// SetReaction/CallReaction/DispatchReaction/InjectReaction/ShowValidationErrorsReaction/RequestReaction/ParallelReaction unchanged.
```

### `runtime/execution/execute.ts` (route on the carried lane — delta only)

```ts
function executeSequence(r: SequenceReaction, plan: RuntimePlan, ctx: ExecutionContext): ReactionCompletion {
  // TODO[fixture: ordered-sync, sync-async-sync]:
  //   if (r.lane === "sync") { for (step of r.steps) executeReactionWith(step, plan, ctx); return; }
  //   else: run steps, awaiting at the carried async boundary (the existing slice-and-then variant).
}

function executeBranch(r: BranchReaction, plan: RuntimePlan, ctx: ExecutionContext): ReactionCompletion {
  // TODO[fixture: then, else-if, else, no-match, confirm-then]:
  //   first-match over r.cases; sync lane returns the matching reaction's completion synchronously,
  //   async lane (confirm reached) awaits the guard then routes. assertNever on guard.kind.
}
```

Un-changed readers (`executeSet`, `executeCall`, `executeDispatch`, `executeInject`,
`executeShowValidationErrors`) keep their current bodies — pinned by the §6 fixtures.

---

## 6. Acceptance Fixtures (matrix cases this module must satisfy)

From [`04-matrix-triggers-reactions-conditions.md`](../04-matrix-triggers-reactions-conditions.md),
the **Reaction band** plus the Reaction-owned rows of the Condition band. Each is one
self-contained input→output proof; named here so a dev maps each to a C# domain test
(one write path) and a TS runtime test (one read path).

### Sequencing & lane (3)

| Fixture | DSL | Asserts |
|---|---|---|
| **single-command** | `p.Element("s").Show()` | `BuildReaction` returns the bare `set` node (no `sequence` wrapper); lane `Sync`. |
| **ordered-sync** | `p.Element("a").Show(); p.Dispatch("x")` | one `SequenceReaction{ steps:[set, dispatch], lane: Sync }`; runtime runs top-to-bottom same tick. |
| **sync-async-sync** | `p.Element(..).Show(); p.Get(..); p.Element(..).Hide()` | `sequence[seq(sync), request, seq(sync)]`, lane `Async`; sync block runs, request awaited, trailing sync runs. |

### `set` reactions (6)

| Fixture | DSL | Asserts |
|---|---|---|
| **element-show-hide** | `p.Element("box").Show()` / `.Hide()` | `set{ on:component, property:"hidden", value:literal false/true }`; element visibility toggles. SYNC. |
| **set-text-literal** | `p.Element("s").SetText("hi")` | `set{ property:"text", value:literal "hi" (Shape.String) }`; `textContent="hi"`. |
| **set-text-source** | `p.Element("s").SetText(p.FromUrl("q"))` | `set{ property:"text", value:read(url,"q") }`; text = URL param via `evaluateValue`. |
| **set-html** | `p.Element("s").SetHtml(src)` | `set{ property:"html", … }`; `innerHTML`=value. |
| **component-property-write** | `p.Component<X>(m=>m.Field).Set(c=>c.Enabled, true)` | `set{ on:component(id), property:"enabled", value:literal true }`; vendor property updated. |
| **event-arg-set** | inside `.Reactive`, `p` sets `args.cancel` | `set{ on:payload(scope event), property:"cancel", value:literal true }`; SYNC mandatory (SF reads `args.cancel`). |

### `call` reactions (5)

| Fixture | DSL | Asserts |
|---|---|---|
| **element-css-class** | `p.Element("b").AddClass("on")` | `call{ on:component("b"), method:"addClass", args:[literal "on"] }`; class added. SYNC. |
| **component-method-no-arg** | `p.Component<Grid>(…).Call(g=>g.Refresh())` | `call{ method:"refresh", args:[] }`; empty args = `[]`. |
| **component-method-args** | `…Call(g=>g.SelectRow(2))` | `call{ args:[literal 2] }`; each arg one `ValueExpression`. |
| **plugin-command** | `p.Plugin("url","push").Arg(...).Fire()` | `call{ on:plugin, method:"push", args:[…] }`; plugin op runs. SYNC. |
| **event-arg-method** | inside `.Reactive`, `args.UpdateData(...)` | `call{ on:payload(event), method:"updateData", args:[…] }`; arg method invoked in-tick. |

### `dispatch` reactions (3)

| Fixture | DSL | Asserts |
|---|---|---|
| **dispatch-no-payload** | `p.Dispatch("saved")` | `dispatch{ event:"saved", payload:{kind:"none"} }`; detail `{}`. |
| **dispatch-literal-payload** | `p.Dispatch("saved", new Msg{Id=1})` | `dispatch{ payload:{kind:"value", data:literal{…}, payloadType:typed} }`; detail = literal object. |
| **dispatch-source-backed** | `p.DispatchWith<Msg>("saved", b=>b.Set(x=>x.Total, src))` | `dispatch{ payload:{kind:"value", data:object{ total:read… }, payloadType:typed} }`; detail assembled from live sources. |

### `inject` reaction (1)

| Fixture | DSL | Asserts |
|---|---|---|
| **inject-success-body** | `p.Get("/card").Into("card-host")` | `inject{ slot:"card-host", value: read(payload success, whole) }`; element `innerHTML` = HTTP success body. SYNC. Value fixed — no axis. |

### `show-validation-errors` reaction (1)

| Fixture | DSL | Asserts |
|---|---|---|
| **show-validation-errors** | `p.ValidationErrors("resident-form")` | `show-validation-errors{ container:"resident-form" }`; server errors after failed request else current client-rule results render in the container. SYNC. Container required. |

### `branch` routing — Reaction's edge (Condition owns the guard) (5)

| Fixture | DSL | Asserts |
|---|---|---|
| **then** | `p.When(s).Eq(1).Then(p2=>…)` | `branch{ cases:[{guard:when, reaction}], lane:Sync }`; reaction runs only if guard true. |
| **else-if** | `.ElseIf(s).Gt(0).Then(p3=>…)` | another conditional case appended to the **same** `cases`; first-match top-to-bottom. |
| **else** | `.Else(p4=>…)` | `BranchCase.Default` appended last; only one allowed (author-time guard). |
| **no-match** | guards all false, no `Else` | runtime logs `branch.no-match`, returns void; silent no-op. |
| **confirm-then** | `p.Confirm("Delete?").Then(p2=>…)` | `branch` whose guard is `confirm`; `lane:Async`; awaits `window.alis.confirm`, runs reaction only on accept. |

**Coverage map (24 Reaction-owned fixtures):** 3 sequencing/lane + 6 set + 5 call +
3 dispatch + 1 inject + 1 show-validation-errors + 5 branch routing. These are the
acceptance gate. (The Condition band's compare-operator families, guard composition,
and condition-source rows are **Condition's** fixtures; Reaction only sequences the
resulting `BranchCase`s — `confirm-in-composition` lane-Async is verified jointly with
Condition.)

---

## 7. Done When (per the repo Pass Protocol)

1. The 24 §6 fixtures pass as C# domain tests (one write path) and TS runtime tests in
   jsdom (one read path).
2. `ReactionLane` is stamped by `ReactionPipelineDraft`/the composite node ctors and
   read by `executeSequence`/`executeBranch`; `crossedAsyncBoundary` Promise-probing
   for routing is removed.
3. The C# node shape change (`lane` on Sequence/Branch) is regenerated into `plan.ts`
   by the **Kind** kernel; `npm run typecheck` passes; `assertNever` stays exhaustive.
4. No new nullable property and no new `[JsonIgnore(WhenWritingNull)]` (Rule 6 gate).
5. Runtime assets rebuilt before Playwright; page-visible rows (`element-show-hide`,
   `inject-success-body`, `confirm-then`) proved with Playwright against fresh assets.
6. `git status` clean; commit names the closed row(s).
```
