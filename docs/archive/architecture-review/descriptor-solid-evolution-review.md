# Descriptor & Builder SOLID Review — Evolution at Scale

**Question:** As Alis.Reactive grows to 100+ component vertical slices, what in the descriptor
and builder system will become painful to evolve?

**Method:** Audited all descriptor classes, all builders, all extension points, and the TS
runtime against real code. Every finding has file:line evidence.

---

## The Two Scaling Dimensions

Not everything scales the same way. The framework has two independent growth axes:

| Axis | What grows | How fast | Examples |
|------|-----------|----------|---------|
| **Components** | Vertical slices (7 files each) | Fast — 100+ planned | NativeTextBox, FusionDropDownList, etc. |
| **Infrastructure** | Triggers, reactions, HTTP features, condition operators | Slow — new primitives are rare | ServerPush, SignalR, ParallelHttp |

**This distinction is critical.** An SRP violation in a class that changes with every component
is urgent. An SRP violation in a class that changes once a quarter is tolerable.

---

## What Scales Excellently (No Action Needed)

### ComponentRef — the extension point that works

`Alis.Reactive/ComponentRef.cs:37-45`

```csharp
internal ComponentRef<TComponent, TModel> Emit(
    Mutation mutation, object? value = null, BindSource? source = null)
{
    Pipeline.AddCommand(new MutateElementCommand(
        TargetId, mutation, value, source, vendor: _instance.Vendor));
    return this;
}
```

Each component adds extension methods on `ComponentRef<MyComponent, TModel>` — the core
never changes. Component #101 costs 0 core modifications.

**Evidence:** NativeTextBoxExtensions.cs adds `.SetValue()`, `.FocusIn()`, `.Value()` — all
call `Emit()` with different Mutations. FusionAutoCompleteExtensions.cs does the same.
Zero overlap, zero coupling.

### Polymorphic descriptors — open for new types

Every descriptor hierarchy (Command, Trigger, Reaction, Mutation, Guard, BindSource, GatherItem)
uses `WriteOnlyPolymorphicConverter<T>` with a `Kind` property per subclass. Adding a new
command kind = create new sealed class with `public string Kind => "new-kind"`. No base class
modification, no registry update.

**Evidence:** `WriteOnlyPolymorphicConverter.cs:10` serializes via `value.GetType()` — runtime
reflection discovers the concrete type. `DispatchCommand.cs:8` declares `Kind => "dispatch"`.

### Vertical slices — truly self-contained

Adding component #101 requires:
- 7-10 files **created** (phantom type, builder, extensions, events, reactive, factory, tests)
- 0 files **modified** in core

**Evidence:** Traced NativeTextBox (7 files in `Alis.Reactive.Native/Components/NativeTextBox/`)
and FusionAutoComplete (7 files in `Alis.Reactive.Fusion/Components/FusionAutoComplete/`).
Neither modifies any core framework file.

### TS runtime — component-agnostic

The runtime has **zero hardcoded component names or types**. Bracket notation
(`root[prop] = val`, `root[method](val)`) handles all components uniformly.
`component.ts:resolveRoot()` is the only vendor-aware module (7 lines).

**Evidence:** `execution/element.ts:27-45` — switches on `mutation.kind` ("set-prop" vs "call"),
never on component type. `execution/commands.ts:36-92` — switches on `command.kind`, never on
component identity.

---

## What Has Real SRP Concerns (But Doesn't Scale With Components)

These classes have multiple responsibilities, but they only change when **infrastructure**
changes (new trigger types, new HTTP features) — not when new components are added.

### RequestDescriptor — 8 responsibilities, changes rarely

`Alis.Reactive/Descriptors/Requests/RequestDescriptor.cs`

| Responsibility | Properties | Changes when... |
|----------------|-----------|----------------|
| HTTP request spec | `Verb`, `Url` | New HTTP verb support |
| Request body assembly | `Gather` (List<GatherItem>) | New gather strategy (e.g., headers) |
| Request format | `ContentType` | New content type (e.g., multipart) |
| Loading UI | `WhileLoading` (List<Command>) | Loading pattern changes |
| Success handling | `OnSuccess` (List<StatusHandler>) | Response handling changes |
| Error handling | `OnError` (List<StatusHandler>) | Error handling changes |
| Request chaining | `Chained` (RequestDescriptor) | Sequential HTTP patterns |
| Validation | `Validation`, `ValidatorType` + mutations | Validation extraction changes |

**Why it's tolerable:** RequestDescriptor changes when HTTP pipeline features change, not when
components are added. Adding component #101 never touches this class. The 8 responsibilities
are co-located because they all describe ONE HTTP request — splitting them would scatter the
request's meaning across 4-5 files with no practical benefit, since they always change together
when the HTTP pipeline evolves.

**When it would become a problem:** If the framework needed to support WebSocket requests,
GraphQL subscriptions, or streaming responses — fundamentally different request shapes would
force awkward contortions into this flat structure. But that's a bridge to cross when reached.

### PipelineBuilder — god class, changes rarely

`Alis.Reactive/Builders/PipelineBuilder.cs` (+ `.Http.cs`, `.Conditions.cs`) — 321 lines total

| Responsibility | Partial file |
|----------------|-------------|
| Sequential command pipeline | Main (lines 31-81) |
| Element/Component factories | Main (lines 48-81) |
| Conditional branching | `.Conditions.cs` |
| HTTP orchestration | `.Http.cs` |
| Multi-segment reactions | Main (lines 137-187) |
| Pipeline mode management | `.Http.cs` (line 69) |

**Why it's tolerable:** PipelineBuilder changes when a new pipeline MODE is added (e.g.,
streaming, retry). It does NOT change when new components are added — components use
`.Element()` and `.Component<T>()` which are stable entry points.

The partial file pattern helps — each mode lives in its own file. At 3 modes (Sequential,
Http, Parallel) plus Conditions, it's manageable.

**When it would become a problem:** Adding mode #5 or #6 would make BuildSingleReaction()
(line 173-187) a growing switch statement. But that's rare infrastructure work, not
component-scale growth.

### TriggerBuilder — closed to new trigger types

`Alis.Reactive/Builders/TriggerBuilder.cs` (106 lines)

Each trigger type requires a new public method on TriggerBuilder:
- `DomReady()` (line 22)
- `CustomEvent()` (lines 31, 40)
- `ServerPush()` (lines 51, 60, 69)
- `SignalR()` (lines 80, 90)

**Why it's tolerable:** Components don't add triggers to TriggerBuilder. Components use
`.Reactive()` extensions (e.g., `NativeTextBoxReactiveExtensions.cs`) which wire
ComponentEventTrigger directly — they bypass TriggerBuilder entirely.

TriggerBuilder only grows when new INFRASTRUCTURE triggers are added (WebSocket, polling,
long-polling). That's maybe 1-2 new triggers per year.

**When it would become a problem:** If there were 20+ infrastructure trigger types with
3 overloads each = 60 methods. Not happening at current trajectory.

---

## What Has Real SRP Concerns AND Scales With Components

These are the areas where growth pressure from 100+ components creates actual pain.

### ComponentEventTrigger — carries too much per-event metadata

`Alis.Reactive/Descriptors/Triggers/ComponentEventTrigger.cs`

```csharp
public sealed class ComponentEventTrigger : Trigger
{
    public string ComponentId { get; }    // which component
    public string JsEvent { get; }        // what JS event
    public string Vendor { get; }         // which vendor system
    public string? BindingPath { get; }   // model binding path
    public string? ReadExpr { get; }      // how to read the value
}
```

5 properties for one trigger. Every component's `.Reactive()` extension creates one of these
by manually assembling all 5 values:

```csharp
// NativeTextBoxReactiveExtensions.cs (every component repeats this pattern)
var trigger = new ComponentEventTrigger(
    componentId, descriptor.JsEvent, Component.Vendor, bindingPath, Component.ReadExpr);
```

**Scaling pressure:** 100 components × 3 events each = 300 `.Reactive()` extensions, each
manually assembling 5 constructor args. If ComponentEventTrigger's constructor signature
changes, all 300 break.

**Potential improvement:** A factory method on ComponentEventTrigger that takes fewer params:

```csharp
// Instead of 5 args manually assembled in every Reactive extension:
ComponentEventTrigger.For<TComponent>(componentId, descriptor, bindingPath)
```

This centralizes the "how to build a component trigger" knowledge in one place.

### Reactive extension boilerplate — copy-paste at scale

Every component's `*ReactiveExtensions.cs` file repeats the same pattern:

```csharp
public static TriggerBuilder<TModel> Reactive<TModel>(
    this ComponentBuilder<TModel, TProp> builder,
    IReactivePlan<TModel> plan,
    Func<Events, TypedEventDescriptor<TArgs>> selector,
    Action<TArgs, PipelineBuilder<TModel>> configure) where TModel : class
{
    var descriptor = selector(Events.Instance);
    var componentId = builder.ElementId;
    var bindingPath = builder.BindingPath;
    var pb = new PipelineBuilder<TModel>();
    configure(new TArgs(), pb);
    var trigger = new ComponentEventTrigger(
        componentId, descriptor.JsEvent, Component.Vendor, bindingPath, Component.ReadExpr);
    foreach (var reaction in pb.BuildReactions())
        plan.AddEntry(new Entry(trigger, reaction));
    return new TriggerBuilder<TModel>(plan);
}
```

**~15 lines of boilerplate per event overload.** With 100 components × 2-3 event overloads =
200-300 near-identical methods differing only in type parameters and event selector.

**Scaling pressure:** This isn't DRY, but per CLAUDE.md rules, "duplication over abstraction"
is an explicit architectural decision for vertical slice isolation. The question is whether
the duplication will cause maintenance pain at 200+ copies.

**Potential improvement:** A shared helper that handles the plumbing:

```csharp
// In core — handles the boilerplate once
internal static TriggerBuilder<TModel> WireComponentEvent<TModel, TComponent, TArgs>(
    IReactivePlan<TModel> plan,
    string componentId, string? bindingPath,
    TypedEventDescriptor<TArgs> descriptor,
    Action<TArgs, PipelineBuilder<TModel>> configure)
    where TModel : class
    where TComponent : IComponent, new()
    where TArgs : new()
```

Each component's `.Reactive()` becomes a 3-line wrapper that calls this helper.
The duplication shrinks from 15 lines × 300 = 4500 lines to 3 lines × 300 = 900 lines.

**Trade-off:** This creates a shared dependency (the helper) that all slices use. A change to
the helper breaks all slices. But the current pattern has the SAME coupling — they all
construct ComponentEventTrigger manually. The coupling already exists; it's just hidden behind
copy-paste.

### Component registration boilerplate — repeated in every factory

Every `*HtmlExtensions.cs` manually registers the component:

```csharp
// NativeTextBoxHtmlExtensions.cs (every component repeats this)
setup.Plan.AddToComponentsMap(setup.BindingPath,
    new ComponentRegistration(setup.ElementId, Component.Vendor,
        setup.BindingPath, Component.ReadExpr, "textbox"));
```

100 components = 100 copies of this registration call with different component type strings.

**Scaling pressure:** Low — it's one line, and the `ComponentRegistration` constructor is stable.
But if the constructor signature changes, all 100 factories break.

---

## What the TS Runtime Does Right

The TS runtime has **zero scaling concerns**. It's worth documenting WHY so we don't break it.

| Module | What it does | Why it doesn't scale with components |
|--------|-------------|-------------------------------------|
| `commands.ts` | Switches on `command.kind` | New commands are infrastructure, not component-level |
| `element.ts` | Bracket notation: `root[prop]` / `root[method]()` | ALL component mutations use the same 2 patterns |
| `trigger.ts` | Switches on `trigger.kind` | Component events use `"component-event"` — one case for all |
| `component.ts` | `resolveRoot(el, vendor)` | 2 vendors (native, fusion) — not per-component |
| `gather.ts` | Reads component values via `evalRead()` | Uses `readExpr` from plan — no component knowledge |
| `resolver.ts` | Walks dot-paths via `walk()` | Pure utility — vendor/component agnostic |

**The plan carries ALL behavior.** The runtime is a dumb executor. This is the architecture's
greatest strength. Don't break it.

---

## Recommendations — Ordered by Impact

### 1. ComponentEventTrigger factory method (do when next trigger work happens)

**Problem:** 5 constructor args manually assembled in every `.Reactive()` extension.
**Fix:** Static factory `ComponentEventTrigger.For<TComponent>(componentId, descriptor, bindingPath)`.
**Impact:** Centralizes trigger assembly, reduces breakage surface from 300 locations to 1.
**Risk:** Low — internal change, no plan shape change, no runtime change.

### 2. Shared Reactive wiring helper (do when component count hits ~30)

**Problem:** ~15 lines of identical boilerplate in every `*ReactiveExtensions.cs`.
**Fix:** Internal helper method in core that handles plumbing.
**Impact:** Reduces per-component boilerplate from 15 lines to 3 lines per event overload.
**Risk:** Low — internal refactor, vertical slices become thinner, not fatter.
**Trade-off:** Creates shared dependency. But the coupling already exists — it's just copy-pasted.

### 3. Leave RequestDescriptor, PipelineBuilder, TriggerBuilder alone

**Problem:** These classes have multiple responsibilities.
**Why leave them:** They change with infrastructure, not components. The growth pressure from
100+ components doesn't hit them. Splitting them adds indirection without solving a scaling
problem.
**Revisit when:** A new pipeline mode (streaming?) or a new trigger category (20+ types?) is
needed.

### 4. Add schema-kind consistency test (do now — cheap insurance)

**Problem:** Adding a new Command subclass doesn't REQUIRE updating the JSON schema. The
`WriteOnlyPolymorphicConverter` serializes any subclass via reflection. A missing schema
definition is a silent bug.
**Fix:** A test that discovers all Command/Trigger/Reaction/etc subclasses via reflection and
asserts each has a matching schema definition in `reactive-plan.schema.json`.
**Impact:** Catches schema drift immediately.
**Risk:** Zero — test-only change.

---

## Summary

| Area | SRP Status | Scales with components? | Action |
|------|-----------|----------------------|--------|
| ComponentRef + vertical slices | Excellent | No scaling pressure | None needed |
| Polymorphic descriptors | Excellent | No scaling pressure | None needed |
| TS runtime | Excellent | No scaling pressure | None needed |
| ComponentEventTrigger | 5 constructor args | YES — 300+ call sites | Factory method |
| Reactive wiring boilerplate | 15 lines repeated | YES — 200-300 copies | Shared helper at ~30 components |
| Component registration | 1 line repeated | Minor — 100 copies | Leave for now |
| RequestDescriptor | 8 responsibilities | No — infrastructure only | Leave |
| PipelineBuilder | 11 responsibilities (3 files) | No — infrastructure only | Leave |
| TriggerBuilder | Closed to new types | No — slow growth | Leave |
| Schema-kind consistency | No enforcement | N/A | Add test |
