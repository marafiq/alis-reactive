# Trigger — Implementation Spec

> Module spec for the **Trigger** concept-slice. Grounded in the actual public DSL
> source, not the prior atlas. Open this file, read the surface + fixtures, type
> the obvious body. Every type/method below maps to a row in
> [`04-matrix-triggers-reactions-conditions.md`](../04-matrix-triggers-reactions-conditions.md)
> (Trigger band). Names come from [`03-naming.md`](../03-naming.md); ownership from
> [`02-micro-modules.md`](../02-micro-modules.md).
>
> **Source read as the requirement** (do not infer from tests/docs):
> - `Alis.Reactive/Razor/Extensions/HtmlExtensions.cs:53` (`Html.On`)
> - `Alis.Reactive/Builders/TriggerBuilder.cs` (the fluent surface)
> - `Alis.Reactive/PlanModel/StartsWhen.cs` (the trigger node family + filters)
> - `Alis.Reactive/PlanModel/Behavior.cs`, `BehaviorGraph.cs` (the edge + graph)
> - `Alis.Reactive/PlanModel/PlanBuildContext.cs:105,112` (`WireComponentEvent`, `AddBehavior`)
> - `Alis.Reactive/PlanModel/PlanTerms.cs:169,219,155,430` (`EventName`, `RequestUrl`, `MemberName`, `PayloadContract`)
> - `Alis.Reactive/ComponentOnboarding/ComponentEventOnboarding.cs` (the component-event wire path)
> - `Alis.Reactive.Assets/runtime/execution/trigger.ts` (`wireBehavior` → renamed `wireTrigger`)
> - `Alis.Reactive.Assets/runtime/execution/server-push.ts`, `signalr.ts`, `domain/execution-context.ts`
> - `Alis.Reactive.Assets/runtime/types/plan.ts:420-468`, `types/context.ts` (the wire shapes)

---

## 1. Responsibility, Ownership, Dependencies

**Responsibility (one sentence).** Trigger owns *when a behavior starts* — it lowers
each `Html.On(...)` authoring call to one `Behavior` (a `StartsWhen` trigger node
paired with its `ReactionGraph`) and, at runtime, attaches exactly one browser
listener per `StartsWhen` kind that feeds the originating payload into one
`ExecutionContext`.

**What it owns** (`→` C# author/plan side · `⇒` TS runtime side):

| Side | Owns | Source today |
|---|---|---|
| `→` | `Html.On` entry + `TriggerBuilder<TModel>` (the fluent surface) | `HtmlExtensions.On`, `Builders/TriggerBuilder.cs` |
| `→` | `StartsWhen` node family — **made symmetric** (public sealed + explicit `Kind`): `PageReadyTrigger`, `DocumentEventTrigger`, `ComponentEventTrigger`, `ServerPushTrigger`, `SignalRTrigger` | `PlanModel/StartsWhen.cs` |
| `→` | `ServerPushEventFilter` family: `AnyServerPushEvent`, `NamedServerPushEvent` | `PlanModel/StartsWhen.cs:111-152` |
| `→` | `Behavior` (one trigger→reaction edge) + `BehaviorGraph` (all edges in a plan) | `PlanModel/Behavior.cs`, `BehaviorGraph.cs` |
| `⇒` | `wireTrigger` (today `wireBehavior`) — the `switch (trigger.kind)` listener dispatcher | `execution/trigger.ts` |
| `⇒` | `wireServerPush`, `wireSignalR` — the two remote-trigger openers | `execution/server-push.ts`, `signalr.ts` |
| `⇒` | ONE `ExecutionContext` carrying the trigger payload into the reaction | `domain/execution-context.ts` |

**What it depends on** (from the module-dependency graph; arrows point *down*):

- **Reaction** — `TriggerBuilder` builds a `ReactionGraph` (via `PipelineBuilder.BuildReaction()`)
  for every behavior; `wireTrigger` calls `executeReaction`. Trigger never executes a
  reaction itself; it only delivers the payload and routes the call.
- **Component** — `component-event` resolves the `BrowserObject` by id, reads its event
  channel from `BrowserObjectContract`, and wires via `ComponentDriver`
  (`wireFusionEvent`/`wireNativeEvent`). The component-event edge is authored through the
  Component slice's `.Reactive()` extensions → `ComponentEventOnboarding.Wire` →
  `PlanBuildContext.WireComponentEvent`; Trigger only owns the resulting `StartsWhen.ComponentEvent`
  node and its runtime listener.
- **Kind** (kernel) — every `StartsWhen` / `ServerPushEventFilter` variant carries `Kind`
  (the discriminator) written by the one `PlanNodeDiscriminator`; `assertNever` proves the
  `wireTrigger` switch is exhaustive.

**What it does NOT own.** It does not own the `PayloadContract` value object (a shared
term in `PlanTerms.cs`, consumed by triggers and dispatch alike), the `ReactionGraph`
(Reaction), the `BrowserObjectContract`/`ComponentDriver` (Component), or the
`PlanDocument`/`AddBehavior` plumbing (Plan). It *uses* `PlanBuildContext.AddBehavior`
to append; it does not own the document.

**Dissolves (from the baseline):** the `Behavior`/`StartsWhen` internal-class /
public-prop asymmetry (every `StartsWhen` subclass becomes `public sealed` with an
explicit `Kind`, flowing through the one serialization path like every other node);
and the raw-vs-rich `ExecContext` double threading (one `ExecutionContext` is built once
and passed down — `wireTrigger` stops unwrapping `.raw` at the seam, see §6 note).

---

## 2. Public Surface

> XML-doc-style intent on each member is enough that a dev types the body with **no**
> design decision left. Visibility follows the frozen API rule: `Html.On` is `public`,
> `TriggerBuilder<TModel>` is `public sealed` with an `internal` constructor; the
> `StartsWhen` family is `internal` plan-model (its `Kind` is the contract surface,
> reflected into `plan.ts` by the Kind kernel).

### 2.1 Author entry — `Html.On` (`Razor/Extensions/HtmlExtensions.cs`)

```csharp
/// <summary>Configures one or more triggers that start reactive behaviors on this plan.</summary>
/// <remarks>Triggers chain: <c>t.DomReady(...).CustomEvent(...).SignalR(...)</c>; each call adds an independent behavior.</remarks>
/// <typeparam name="TModel">The view model type.</typeparam>
/// <param name="html">The Razor HTML helper (extension receiver).</param>
/// <param name="plan">The plan the behaviors are appended to.</param>
/// <param name="trigger">Fluent configuration callback over <see cref="TriggerBuilder{TModel}"/>.</param>
public static void On<TModel>(this IHtmlHelper<TModel> html, ReactivePlan<TModel> plan,
    Action<TriggerBuilder<TModel>> trigger) where TModel : class;
```

### 2.2 Author surface — `TriggerBuilder<TModel>` (`Builders/TriggerBuilder.cs`)

Each method returns `this` for chaining. Each builds a `PipelineBuilder<TModel>`, runs
the developer's `pipeline` callback into it, then appends one `Behavior`. The `<TPayload>`
overloads instantiate a phantom `new TPayload()` (used only for typed payload-path reads;
it carries no value) and stamp `PayloadContract.ForPayload(typeof(TPayload))`.

```csharp
public sealed class TriggerBuilder<TModel> where TModel : class
{
    internal TriggerBuilder(ReactivePlan<TModel> plan, PlanBuildContext context);

    /// <summary>Fires once when the page is ready. No payload.</summary>
    public TriggerBuilder<TModel> DomReady(Action<PipelineBuilder<TModel>> pipeline);

    /// <summary>Fires every time the named custom event is dispatched (e.g. by <c>p.Dispatch("name")</c>).</summary>
    public TriggerBuilder<TModel> CustomEvent(string eventName, Action<PipelineBuilder<TModel>> pipeline);

    /// <summary>Fires on the named custom event with a typed payload for typed payload-path reads.</summary>
    public TriggerBuilder<TModel> CustomEvent<TPayload>(string eventName,
        Action<TPayload, PipelineBuilder<TModel>> pipeline) where TPayload : new();

    /// <summary>Fires on every Server-Sent Event from the SSE endpoint.</summary>
    public TriggerBuilder<TModel> ServerPush(string url, Action<PipelineBuilder<TModel>> pipeline);

    /// <summary>Fires only on the named SSE event type.</summary>
    public TriggerBuilder<TModel> ServerPush(string url, string eventType, Action<PipelineBuilder<TModel>> pipeline);

    /// <summary>Fires on the named SSE event type with a typed payload.</summary>
    public TriggerBuilder<TModel> ServerPush<TPayload>(string url, string eventType,
        Action<TPayload, PipelineBuilder<TModel>> pipeline) where TPayload : new();

    /// <summary>Fires when the named SignalR hub method is invoked.</summary>
    public TriggerBuilder<TModel> SignalR(string hubUrl, string methodName, Action<PipelineBuilder<TModel>> pipeline);

    /// <summary>Fires on the named SignalR hub method with a typed payload.</summary>
    public TriggerBuilder<TModel> SignalR<TPayload>(string hubUrl, string methodName,
        Action<TPayload, PipelineBuilder<TModel>> pipeline) where TPayload : new();
}
```

> **No `ComponentEvent(...)` method on `TriggerBuilder`.** Component-event triggers are
> authored through the Component slice's `.Reactive(plan, evt => …, (args, p) => …)`
> extensions, not on `TriggerBuilder`. That path routes through
> `ComponentEventOnboarding.Wire` → `PlanBuildContext.WireComponentEvent` →
> `StartsWhen.ComponentEvent(id, eventName)`. Trigger owns the resulting node and its
> runtime listener; it does not add an authoring verb here. (Confirmed: no
> `ComponentEvent` method exists on `TriggerBuilder.cs`.)

### 2.3 Plan-model node family — `StartsWhen` (`PlanModel/StartsWhen.cs`)

`StartsWhen` is the abstract base with `internal` factory methods. Each concrete trigger
is `sealed`, carries an explicit `Kind`, and stores only value objects. **Symmetry fix:**
in the redesign every subclass is `public sealed` (today they are `internal`) so they flow
through the one `PlanNodeDiscriminator` like every other node — but the constructors stay
`internal` and instances are created only via the factory methods (frozen API rule).

```csharp
[JsonConverter(typeof(WriteOnlyPolymorphicConverter<StartsWhen>))] // → replaced by PlanNodeDiscriminator (Kind kernel)
internal abstract class StartsWhen
{
    private protected StartsWhen();

    internal static StartsWhen PageReady();
    internal static StartsWhen DocumentEvent(string eventName);                              // payload Untyped
    internal static StartsWhen DocumentEvent(string eventName, PayloadContract payloadType);
    internal static StartsWhen ComponentEvent(string component, string eventName);
    internal static StartsWhen ServerPush(string url);                                       // filter AnyEvent, Untyped
    internal static StartsWhen ServerPush(string url, string eventName);                     // filter NamedEvent, Untyped
    internal static StartsWhen ServerPush(string url, string eventName, PayloadContract payloadType);
    internal static StartsWhen SignalR(string hubUrl, string method);                        // payload Untyped
    internal static StartsWhen SignalR(string hubUrl, string method, PayloadContract payloadType);
}

internal sealed class PageReadyTrigger : StartsWhen { public string Kind => "page-ready"; }

internal sealed class DocumentEventTrigger : StartsWhen
{
    public string Kind => "document-event";
    public string Event { get; }                 // EventName.Value
    public PayloadContract PayloadType { get; }   // never null — see invariants
    internal DocumentEventTrigger(string eventName, PayloadContract payloadType);
}

internal sealed class ComponentEventTrigger : StartsWhen
{
    public string Kind => "component-event";
    public string Component { get; }             // ComponentKey.Value
    public string Event { get; }                 // EventName.Value
    internal ComponentKey ComponentKey { get; }  // for BehaviorGraph event-metadata registration
    internal EventName EventName { get; }
    internal ComponentEventTrigger(string component, string eventName);
}

internal sealed class ServerPushTrigger : StartsWhen
{
    public string Kind => "server-push";
    public string Url { get; }                   // RequestUrl.Value
    public ServerPushEventFilter EventFilter { get; }  // never null
    internal ServerPushTrigger(string url, ServerPushEventFilter filter);
}

internal sealed class SignalRTrigger : StartsWhen
{
    public string Kind => "signalr";
    public string HubUrl { get; }                // RequestUrl.Value
    public string Method { get; }                // MemberName.Value
    public PayloadContract PayloadType { get; }  // never null
    internal SignalRTrigger(string hubUrl, string method, PayloadContract payloadType);
}
```

### 2.4 The SSE event filter — `ServerPushEventFilter` (`PlanModel/StartsWhen.cs`)

```csharp
[JsonConverter(typeof(WriteOnlyPolymorphicConverter<ServerPushEventFilter>))] // → PlanNodeDiscriminator
public abstract class ServerPushEventFilter
{
    private protected ServerPushEventFilter(PayloadContract payloadType); // payloadType never null
    public abstract string Kind { get; }
    public PayloadContract PayloadType { get; }

    internal static ServerPushEventFilter AnyEvent();                                // Untyped
    internal static ServerPushEventFilter NamedEvent(string eventName);              // Untyped
    internal static ServerPushEventFilter NamedEvent(string eventName, PayloadContract payloadType);
}

internal sealed class AnyServerPushEvent  : ServerPushEventFilter { public override string Kind => "any"; }
internal sealed class NamedServerPushEvent: ServerPushEventFilter { public override string Kind => "named"; public string Event { get; } }
```

### 2.5 The edge + graph — `Behavior`, `BehaviorGraph` (`PlanModel/Behavior.cs`, `BehaviorGraph.cs`)

```csharp
internal sealed class Behavior
{
    public StartsWhen StartsWhen { get; }   // never null
    public ReactionGraph Reaction { get; }  // never null
    internal static Behavior On(StartsWhen trigger, ReactionGraph reaction);
}

internal sealed class BehaviorGraph
{
    internal BehaviorGraph(ComponentObjects components);     // → BrowserObjects in the redesign
    internal IReadOnlyList<Behavior> Snapshot();             // defensive copy
    internal void Add(Behavior behavior);                    // registers component-event metadata, then appends
}
```

`BehaviorGraph.Add` does ONE side-effect beyond appending: if the trigger is a
`ComponentEventTrigger`, it declares the event channel on the component repository so the
runtime can resolve it (`_components.DeclareEvent(componentEvent.ComponentKey,
ObjectEventContract.ForComponentEvent(componentEvent.EventName))`). This is *not*
validation — it is execution bookkeeping (remembering the event channel for runtime wiring).

### 2.6 TS runtime — `wireTrigger` + ExecutionContext (`execution/trigger.ts`)

```ts
// renamed wireBehavior → wireTrigger to match 03-naming.md
export function wireTrigger(
  trigger: StartsWhen,
  reaction: ReactionGraph,
  plan: PlanDocument,
  signal?: AbortSignal,
): void;                       // attaches exactly one listener per trigger.kind

export function wireServerPush(trigger: ServerPushTrigger, reaction: ReactionGraph, plan: PlanDocument, signal?: AbortSignal): void;
export function wireSignalR(trigger: SignalRTrigger,    reaction: ReactionGraph, plan: PlanDocument, signal?: AbortSignal): void;
```

The wire shapes these read are **generated** by the Kind kernel into `types/plan.ts`
(`StartsWhen`, `PageReadyTrigger`, `DocumentEventTrigger`, `ComponentEventTrigger`,
`ServerPushTrigger`, `SignalRTrigger`, `ServerPushEventFilter`, `PayloadContract`) — never
hand-authored here.

---

## 3. Input → Output Contract

**Author input → plan output.** `Html.On(plan, t => t.<Kind>(args, pipeline))`. Each
`TriggerBuilder` method runs `pipeline` into a fresh `PipelineBuilder<TModel>`, then calls
`AddBehaviors(StartsWhen.<Kind>(args), pb)` → `_context.AddBehavior(Behavior.On(trigger,
pb.BuildReaction()))`. Output: one `Behavior` (one `StartsWhen` + one `ReactionGraph`)
appended to the plan's `BehaviorGraph`, in author order. Chaining N triggers appends N
independent behaviors (`behaviors: [Behavior_1 … Behavior_N]`).

**Plan output → wire JSON** (camelCase, exact shapes from `plan.ts:420-468`):

| Trigger | Wire JSON |
|---|---|
| `DomReady` | `{ "kind": "page-ready" }` |
| `CustomEvent("ready")` | `{ "kind":"document-event","event":"ready","payloadType":{"kind":"untyped"} }` |
| `CustomEvent<T>("ready")` | `{ "kind":"document-event","event":"ready","payloadType":{"kind":"typed","type":"<T.FullName>"} }` |
| component-event (via `.Reactive`) | `{ "kind":"component-event","component":"<id>","event":"<eventName>" }` |
| `ServerPush("/sse")` | `{ "kind":"server-push","url":"/sse","eventFilter":{"kind":"any","payloadType":{"kind":"untyped"}} }` |
| `ServerPush("/sse","tick")` | `{ …,"eventFilter":{"kind":"named","event":"tick","payloadType":{"kind":"untyped"}} }` |
| `ServerPush<V>("/sse","tick")` | `{ …,"eventFilter":{"kind":"named","event":"tick","payloadType":{"kind":"typed","type":"<V.FullName>"}} }` |
| `SignalR("/hub","OnTick")` | `{ "kind":"signalr","hubUrl":"/hub","method":"OnTick","payloadType":{"kind":"untyped"} }` |
| `SignalR<V>("/hub","OnTick")` | `{ …,"payloadType":{"kind":"typed","type":"<V.FullName>"} }` |

**Wire JSON → browser behavior** (the runtime contract `wireTrigger` must achieve):

| `trigger.kind` | Listener + lane | Payload into `ExecutionContext` |
|---|---|---|
| `page-ready` | if `document.readyState==="loading"` run on `DOMContentLoaded`, else run immediately. **SYNC**. | `ExecutionContext.empty()` (no payload) |
| `document-event` | `document.addEventListener(event, …, opts)` every dispatch. **SYNC**. | `ExecutionContext.event(detail ?? event)` |
| `component-event` | resolve `BrowserObject` by `component`, read channel from `BrowserObjectContract`, `ComponentDriver`/`wireEvent`. **SYNC** (so SF `args.cancel` is visible). | `ExecutionContext.event(eventData)` |
| `server-push` | `wireServerPush` opens/pools an `EventSource` per url, `addEventListener(eventName)` (`"message"` for `any`, else `filter.event`), AbortSignal-scoped. **ASYNC opener** (remote trigger). | `ExecutionContext.event(JSON.parse(e.data))` |
| `signalr` | `wireSignalR` opens/pools a hub connection per `hubUrl`, `connection.on(method)`, AbortSignal-scoped. **ASYNC opener** (remote trigger). | `ExecutionContext.event(invocationPayload)` |

**Invariants** (value-object constructors enforce them; `null` is unrepresentable by
construction, NOT guarded by exceptions on the hot path):

1. **A `Behavior` always has both a trigger and a reaction.** `Behavior.On` is the only
   constructor path; both args are required by the signature. (The current
   `?? throw ArgumentNullException` in `Behavior` is a *factory-boundary* guard against
   caller misuse — keep it; it is a real authoring edge, not generated-plan defense.)
2. **`PayloadType` is never null** — every factory supplies either `PayloadContract.Untyped`
   (the good default) or `ForPayload(typeof(T))`. There is no nullable payload field and no
   `[JsonIgnore(WhenWritingNull)]`; "no typed payload" is the real `Untyped` variant, not a
   sentinel null. (`02`/`05` design rule: magic sentinels become variants.)
3. **`ServerPushTrigger.EventFilter` is never null** — `ServerPush(url)` defaults to
   `AnyEvent()`; the filter is a real `Any`/`Named` variant, never absent.
4. **String values are validated by their value objects at construction:** `EventName.Of`,
   `RequestUrl.Of`, `MemberName.Of`, `ComponentKey.Of` (all `PlanString`-derived) reject
   null/invalid at the authoring boundary — so the node can never carry a malformed event
   name or url. The trigger node stores the value object, exposes `.Value` on the public
   `Kind`-bearing property.
5. **Author order is wire order.** `BehaviorGraph` appends; `Snapshot()` preserves order.
   Multiple triggers never implicitly share or reorder.
6. **One listener per behavior per kind.** `wireTrigger` attaches exactly one listener and
   routes on the carried `trigger.kind` via `switch` + `assertNever` — no fallback branch,
   no `instanceof` re-detection.

**Good defaults** (the choice when the developer says nothing): `payloadType: untyped`;
SSE filter `any` → channel `"message"`; `page-ready` fires once with empty context;
component-event id comes from the model expression or the builder's explicit element id.

---

## 4. File Layout

The slice is one cohesive folder per side. Paths mirror the current tree (Trigger is an
existing slice being cleaned, not invented); the renames are `wireBehavior → wireTrigger`
and the `StartsWhen` symmetry fix.

```
Alis.Reactive/                                  (C# author + plan side)
  Razor/Extensions/HtmlExtensions.cs            Html.On entry            [edit: keep]
  Builders/TriggerBuilder.cs                    TriggerBuilder<TModel>   [edit: keep]
  PlanModel/StartsWhen.cs                        StartsWhen family +
                                                 ServerPushEventFilter    [edit: symmetry]
  PlanModel/Behavior.cs                          Behavior edge            [edit: keep]
  PlanModel/BehaviorGraph.cs                     BehaviorGraph            [edit: keep]
  PlanModel/PlanBuildContext.cs (:105,:112)      WireComponentEvent /
                                                 AddBehavior sink         [edit: keep]
  ComponentOnboarding/ComponentEventOnboarding.cs  component-event wire   [edit: keep — Component-owned, Trigger-consumed]
  PlanModel/PlanTerms.cs (:155,:169,:219,:430)   EventName/MemberName/
                                                 RequestUrl/PayloadContract  [shared kernel terms — do NOT move]

Alis.Reactive.Assets/runtime/                   (TS runtime side)
  execution/trigger.ts                           wireTrigger dispatcher   [edit: rename wireBehavior]
  execution/server-push.ts                       wireServerPush (SSE)     [edit: keep]
  execution/signalr.ts                           wireSignalR (hub)        [edit: keep]
  domain/execution-context.ts                    ExecutionContext         [edit: keep — Trigger consumes]
  types/plan.ts (:420-468)                        StartsWhen wire union    [GENERATED — never hand-edit]
```

> **PlanTerms.cs is a shared-kernel home, not a Trigger file.** `PayloadContract`,
> `EventName`, `RequestUrl`, `MemberName` are consumed by several slices. Trigger uses them;
> it does not own or relocate them.

---

## 5. Compile-Ready Skeleton

> The type/method declarations a dev fills in. Bodies are `// TODO` referencing the §6
> fixture that proves them. These mirror the existing source exactly; the only deltas are
> the `wireBehavior → wireTrigger` rename and the `StartsWhen` public-sealed symmetry.

### 5.1 `Builders/TriggerBuilder.cs`

```csharp
public sealed class TriggerBuilder<TModel> where TModel : class
{
    private readonly PlanBuildContext _context;

    internal TriggerBuilder(ReactivePlan<TModel> plan, PlanBuildContext context)
    {
        _context = context; // plan is the receiver shape; context is the sink
    }

    public TriggerBuilder<TModel> DomReady(Action<PipelineBuilder<TModel>> pipeline)
    {
        // TODO [F-DomReady]: new PipelineBuilder, run pipeline, AddBehaviors(StartsWhen.PageReady(), pb), return this
    }

    public TriggerBuilder<TModel> CustomEvent(string eventName, Action<PipelineBuilder<TModel>> pipeline)
    {
        // TODO [F-CustomEventUntyped]: AddBehaviors(StartsWhen.DocumentEvent(eventName), pb)
    }

    public TriggerBuilder<TModel> CustomEvent<TPayload>(string eventName,
        Action<TPayload, PipelineBuilder<TModel>> pipeline) where TPayload : new()
    {
        // TODO [F-CustomEventTyped]: run pipeline(new TPayload(), pb);
        //   AddBehaviors(StartsWhen.DocumentEvent(eventName, PayloadContract.ForPayload(typeof(TPayload))), pb)
    }

    public TriggerBuilder<TModel> ServerPush(string url, Action<PipelineBuilder<TModel>> pipeline)
    {
        // TODO [F-ServerPushAny]: AddBehaviors(StartsWhen.ServerPush(url), pb)
    }

    public TriggerBuilder<TModel> ServerPush(string url, string eventType, Action<PipelineBuilder<TModel>> pipeline)
    {
        // TODO [F-ServerPushNamed]: AddBehaviors(StartsWhen.ServerPush(url, eventType), pb)
    }

    public TriggerBuilder<TModel> ServerPush<TPayload>(string url, string eventType,
        Action<TPayload, PipelineBuilder<TModel>> pipeline) where TPayload : new()
    {
        // TODO [F-ServerPushNamedTyped]: AddBehaviors(StartsWhen.ServerPush(url, eventType, PayloadContract.ForPayload(typeof(TPayload))), pb)
    }

    public TriggerBuilder<TModel> SignalR(string hubUrl, string methodName,
        Action<PipelineBuilder<TModel>> pipeline)
    {
        // TODO [F-SignalRUntyped]: AddBehaviors(StartsWhen.SignalR(hubUrl, methodName), pb)
    }

    public TriggerBuilder<TModel> SignalR<TPayload>(string hubUrl, string methodName,
        Action<TPayload, PipelineBuilder<TModel>> pipeline) where TPayload : new()
    {
        // TODO [F-SignalRTyped]: AddBehaviors(StartsWhen.SignalR(hubUrl, methodName, PayloadContract.ForPayload(typeof(TPayload))), pb)
    }

    private void AddBehaviors(StartsWhen trigger, PipelineBuilder<TModel> pb)
    {
        // TODO [F-MultipleTriggers]: _context.AddBehavior(Behavior.On(trigger, pb.BuildReaction()));
    }
}
```

### 5.2 `PlanModel/StartsWhen.cs` (node family — symmetry-fixed)

```csharp
[JsonConverter(typeof(WriteOnlyPolymorphicConverter<StartsWhen>))]
internal abstract class StartsWhen
{
    private protected StartsWhen() { }

    internal static StartsWhen PageReady() => /* TODO [F-PageReadyNode]: new PageReadyTrigger() */;
    internal static StartsWhen DocumentEvent(string eventName) =>
        /* TODO [F-DocEventUntypedNode]: new DocumentEventTrigger(eventName, PayloadContract.Untyped) */;
    internal static StartsWhen DocumentEvent(string eventName, PayloadContract payloadType) =>
        /* TODO [F-DocEventTypedNode]: new DocumentEventTrigger(eventName, payloadType) */;
    internal static StartsWhen ComponentEvent(string component, string eventName) =>
        /* TODO [F-ComponentEventNode]: new ComponentEventTrigger(component, eventName) */;
    internal static StartsWhen ServerPush(string url) =>
        /* TODO [F-ServerPushAnyNode]: new ServerPushTrigger(url, ServerPushEventFilter.AnyEvent()) */;
    internal static StartsWhen ServerPush(string url, string eventName) =>
        /* TODO [F-ServerPushNamedNode]: new ServerPushTrigger(url, ServerPushEventFilter.NamedEvent(eventName)) */;
    internal static StartsWhen ServerPush(string url, string eventName, PayloadContract payloadType) =>
        /* TODO [F-ServerPushNamedTypedNode]: NamedEvent(eventName, payloadType) */;
    internal static StartsWhen SignalR(string hubUrl, string method) =>
        /* TODO [F-SignalRUntypedNode]: new SignalRTrigger(hubUrl, method, PayloadContract.Untyped) */;
    internal static StartsWhen SignalR(string hubUrl, string method, PayloadContract payloadType) =>
        /* TODO [F-SignalRTypedNode]: new SignalRTrigger(hubUrl, method, payloadType) */;
}

internal sealed class PageReadyTrigger : StartsWhen { public string Kind => "page-ready"; }

internal sealed class DocumentEventTrigger : StartsWhen
{
    private readonly EventName _event;
    private readonly PayloadContract _payloadType;
    public string Kind => "document-event";
    public string Event => _event.Value;
    public PayloadContract PayloadType => _payloadType;
    internal DocumentEventTrigger(string eventName, PayloadContract payloadType)
    {
        // TODO: _event = EventName.Of(eventName); _payloadType = payloadType (required by signature; Untyped is the no-payload variant)
    }
}

internal sealed class ComponentEventTrigger : StartsWhen
{
    private readonly ComponentKey _component;
    private readonly EventName _event;
    public string Kind => "component-event";
    public string Component => _component.Value;
    public string Event => _event.Value;
    internal ComponentKey ComponentKey => _component;
    internal EventName EventName => _event;
    internal ComponentEventTrigger(string component, string eventName)
    {
        // TODO: _component = ComponentKey.Of(component); _event = EventName.Of(eventName);
    }
}

internal sealed class ServerPushTrigger : StartsWhen
{
    private readonly RequestUrl _url;
    private readonly ServerPushEventFilter _filter;
    public string Kind => "server-push";
    public string Url => _url.Value;
    public ServerPushEventFilter EventFilter => _filter;
    internal ServerPushTrigger(string url, ServerPushEventFilter filter)
    {
        // TODO: _url = RequestUrl.Of(url); _filter = filter (Any is the unnamed-default variant)
    }
}

internal sealed class SignalRTrigger : StartsWhen
{
    private readonly RequestUrl _hubUrl;
    private readonly MemberName _method;
    private readonly PayloadContract _payloadType;
    public string Kind => "signalr";
    public string HubUrl => _hubUrl.Value;
    public string Method => _method.Value;
    public PayloadContract PayloadType => _payloadType;
    internal SignalRTrigger(string hubUrl, string method, PayloadContract payloadType)
    {
        // TODO: _hubUrl = RequestUrl.Of(hubUrl); _method = MemberName.Of(method); _payloadType = payloadType
    }
}

[JsonConverter(typeof(WriteOnlyPolymorphicConverter<ServerPushEventFilter>))]
public abstract class ServerPushEventFilter
{
    private readonly PayloadContract _payloadType;
    private protected ServerPushEventFilter(PayloadContract payloadType)
    {
        // TODO: _payloadType = payloadType (Untyped default)
    }
    public abstract string Kind { get; }
    public PayloadContract PayloadType => _payloadType;
    internal static ServerPushEventFilter AnyEvent() => /* TODO [F-FilterAny]: new AnyServerPushEvent(PayloadContract.Untyped) */;
    internal static ServerPushEventFilter NamedEvent(string eventName) => /* TODO [F-FilterNamed]: new NamedServerPushEvent(eventName, PayloadContract.Untyped) */;
    internal static ServerPushEventFilter NamedEvent(string eventName, PayloadContract payloadType) => /* TODO [F-FilterNamedTyped] */;
}

internal sealed class AnyServerPushEvent : ServerPushEventFilter
{
    internal AnyServerPushEvent(PayloadContract payloadType) : base(payloadType) { }
    public override string Kind => "any";
}

internal sealed class NamedServerPushEvent : ServerPushEventFilter
{
    private readonly EventName _event;
    internal NamedServerPushEvent(string eventName, PayloadContract payloadType) : base(payloadType)
    {
        // TODO: _event = EventName.Of(eventName);
    }
    public override string Kind => "named";
    public string Event => _event.Value;
}
```

### 5.3 `PlanModel/Behavior.cs` + `BehaviorGraph.cs`

```csharp
internal sealed class Behavior
{
    public StartsWhen StartsWhen { get; }
    public ReactionGraph Reaction { get; }
    private Behavior(StartsWhen startsWhen, ReactionGraph reaction)
    {
        // TODO [F-MultipleTriggers]: assign both (factory-boundary null guard against caller misuse is allowed here)
    }
    internal static Behavior On(StartsWhen trigger, ReactionGraph reaction) => new Behavior(trigger, reaction);
}

internal sealed class BehaviorGraph
{
    private readonly ComponentObjects _components;          // → BrowserObjects
    private readonly List<Behavior> _behaviors = new();
    internal BehaviorGraph(ComponentObjects components) { /* TODO */ }
    internal IReadOnlyList<Behavior> Snapshot() => /* TODO [F-MultipleTriggers]: new List<Behavior>(_behaviors) */;
    internal void Add(Behavior behavior)
    {
        // TODO [F-ComponentEventMetadata]: if StartsWhen is ComponentEventTrigger, _components.DeclareEvent(key, ObjectEventContract.ForComponentEvent(name)); then _behaviors.Add(behavior)
    }
}
```

### 5.4 `execution/trigger.ts` (rename `wireBehavior → wireTrigger`)

```ts
import type { PlanDocument, ReactionGraph, StartsWhen } from "../types";
import { wireEvent } from "../resolution/resolver";              // → ComponentDriver in the redesign
import { RuntimePlan } from "../domain/runtime-plan";             // → RuntimeComponents
import { catchAsyncReactionFailure, executeReaction } from "./execute";
import { wireServerPush } from "./server-push";
import { wireSignalR } from "./signalr";
import { assertNever } from "../core/assert-never";
import { scope } from "../core/trace";
import { ExecutionContext } from "../domain/execution-context";
import { componentEventChannel } from "../domain/component-event-contract";

const log = scope("trigger");

function runReaction(reaction: ReactionGraph, plan: PlanDocument, context: ExecutionContext, source: string): void {
  // TODO [F-RuntimePageReady, F-RuntimeDocEvent, F-RuntimeComponentEvent]:
  //   try { catchAsyncReactionFailure(executeReaction(reaction, plan, context.raw), err => log.error(...)); }
  //   catch (err) { log.error("reaction.failed", { source, sync: true, error: String(err) }); }
}

export function wireTrigger(
  trigger: StartsWhen,
  reaction: ReactionGraph,
  plan: PlanDocument,
  signal?: AbortSignal,
): void {
  const opts = listenerOptions(signal);
  switch (trigger.kind) {
    case "page-ready":
      // TODO [F-RuntimePageReady]: readyState==="loading" ? DOMContentLoaded listener : run now; ExecutionContext.empty()
      break;
    case "document-event": {
      // TODO [F-RuntimeDocEvent]: document.addEventListener(trigger.event, e => runReaction(..., ExecutionContext.event(documentEventPayload(e)), ...), opts)
      break;
    }
    case "component-event": {
      // TODO [F-RuntimeComponentEvent]: resolve component, componentEventChannel(component, trigger.event),
      //   wireEvent(plan, trigger.component, channel, eventData => runReaction(..., ExecutionContext.event(eventData), ...), opts)
      break;
    }
    case "server-push":
      // TODO [F-RuntimeServerPush]: wireServerPush(trigger, reaction, plan, signal)
      break;
    case "signalr":
      // TODO [F-RuntimeSignalR]: wireSignalR(trigger, reaction, plan, signal)
      break;
    default:
      assertNever(trigger, "trigger kind"); // Kind kernel: exhaustiveness proof
  }
}

function listenerOptions(signal: AbortSignal | undefined): AddEventListenerOptions | undefined {
  // TODO: signal === undefined ? undefined : { signal }
}

function documentEventPayload(event: Event): unknown {
  // TODO [F-RuntimeDocEvent]: const detail = (event as CustomEvent).detail; return (detail != null) ? detail : event;
}
```

> **Lane/threading note (`02`/`03` cleanup).** The current seam passes `context.raw`
> (`ExecContext | undefined`) into `executeReaction`. In the redesign the single
> `ExecutionContext` is threaded *as the object*, not unwrapped to `.raw` and re-wrapped
> downstream — the "raw-vs-rich double threading" the module owns is removed. Keep
> `executeReaction`'s signature change in lock-step with the Reaction module; do not invent
> a second context type here.

---

## 6. Acceptance Fixtures (matrix cases this module must satisfy)

These are the named Trigger-band rows from
[`04-matrix-triggers-reactions-conditions.md`](../04-matrix-triggers-reactions-conditions.md)
("Trigger band — `Trigger` module"). Each is the proof for the `// TODO` markers above.
A row is satisfied by: one C# domain test (DSL call → expected `StartsWhen` node + wire
JSON), and one TS runtime test (`wireTrigger` attaches the listener and feeds the right
`ExecutionContext`), plus a Playwright slice for browser-visible kinds.

| Fixture name | Matrix row | Proves | C# `// TODO` | TS `// TODO` |
|---|---|---|---|---|
| **PageReady** | `t.DomReady(...)` → `{ "kind":"page-ready" }` | fire-once-on-load, empty context, SYNC | F-DomReady, F-PageReadyNode | F-RuntimePageReady |
| **CustomEvent (untyped)** | `t.CustomEvent("ready",...)` → `document-event` + `payloadType:untyped` | every dispatch runs with `event(detail)`, default untyped | F-CustomEventUntyped, F-DocEventUntypedNode | F-RuntimeDocEvent |
| **CustomEvent (typed)** | `t.CustomEvent<OrderReady>("ready",...)` → `payloadType:{typed,type}` | phantom `e` carries shape only; typed payload-path reads enabled | F-CustomEventTyped, F-DocEventTypedNode | F-RuntimeDocEvent |
| **ComponentEvent** | `.Reactive(plan, evt=>evt.Changed, …)` → `component-event` | id resolved, channel from contract, SYNC (`args.cancel` visible) | F-ComponentEventNode, F-ComponentEventMetadata | F-RuntimeComponentEvent |
| **ServerPush (any)** | `t.ServerPush("/sse",...)` → `eventFilter:{any,untyped}` | EventSource per url, channel `"message"`, ASYNC opener, abort-scoped | F-ServerPushAny, F-ServerPushAnyNode, F-FilterAny | F-RuntimeServerPush |
| **ServerPush (named)** | `t.ServerPush("/sse","tick",...)` → `eventFilter:{named,event,untyped}` | only `tick` SSE events fire the reaction | F-ServerPushNamed, F-ServerPushNamedNode, F-FilterNamed | F-RuntimeServerPush |
| **ServerPush (named, typed)** | `t.ServerPush<Vitals>("/sse","tick",...)` → `eventFilter:{named,event,typed}` | typed payload contract on the filter | F-ServerPushNamedTyped, F-ServerPushNamedTypedNode, F-FilterNamedTyped | F-RuntimeServerPush |
| **SignalR** | `t.SignalR("/hub","OnTick",...)` → `signalr` + `payloadType:untyped` | hub connection per url, `connection.on(method)`, ASYNC opener, abort-scoped | F-SignalRUntyped, F-SignalRUntypedNode | F-RuntimeSignalR |
| **SignalR (typed)** | `t.SignalR<Vitals>("/hub","OnTick",...)` → `payloadType:typed` | typed contract | F-SignalRTyped, F-SignalRTypedNode | F-RuntimeSignalR |
| **Multiple triggers** | `t.DomReady(…).CustomEvent("x",…)` → `behaviors:[B1,B2]` | each chained call appends an independent `Behavior`; author order = wire order | F-MultipleTriggers | (covered by the per-kind runtime cases) |

**Parameterization that the fixtures collapse.** Per the matrix, a Trigger row =
`(TriggerKind × PayloadContract)`: 5 kinds × `{untyped, typed}` (page-ready has no payload
axis; component-event has no typed authoring overload). The payload axis only toggles the
presence/shape of `payloadType` — it is never a separate template. A generator that emits
the C# `StartsWhen.<Kind>` factory + `TriggerBuilder` overload, the wire interface
(generated by Kind into `plan.ts`), and the `wireTrigger` switch arm, fixed by the kind,
satisfies every row mechanically.

**Out of Trigger scope (recorded, not owned).** The matrix's three open edges
(`set`×`plugin`, `dom`-vs-`component` source, `payload` scope `local`) belong to the
Reaction/Value/Request bands; none touches the Trigger surface. The
`ExecContext.local`/`element` scopes are read by `ExecutionContext.resolvePayload` but are
populated downstream (Value/Request), not by any Trigger listener — Trigger only ever
produces the `event` scope (`ExecutionContext.event(...)`).
