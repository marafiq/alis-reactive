---
name: event-args-mutation-design
description: Design decisions for mutate-event command, EventGather, and typed extensions on args — how to onboard vertical slices when SF events have callable methods (e.g., updateData on filtering)
type: project
---

## MutateEventCommand — Calling Methods / Setting Props on Event Args

**Problem:** Some SF events expose methods/props on the event callback parameter (`e.updateData(data)`, `e.preventDefaultAction = true`). These must be called/set via the plan, not hacked in the runtime.

**Why:** Experiment verified: only `e.updateData()` works for async server-filtered data. `dataSource` alone, `dataSource + dataBind`, `dataSource + showPopup` all fail because SF's filtering lifecycle completes before the async HTTP response arrives. `preventDefaultAction` suppresses the "No records found" flash during async fetch.

**How to apply:** When onboarding a new SF event that has callable methods or settable props on its event args.

### Plan JSON — Two Mutation Kinds

```json
// call: e.updateData(data)
{ "kind": "mutate-event", "mutation": { "kind": "call", "method": "updateData", "args": [...] } }

// set-prop: e.preventDefaultAction = true
{ "kind": "mutate-event", "mutation": { "kind": "set-prop", "prop": "preventDefaultAction" }, "value": true }
```

Same `Mutation` algebra as `mutate-element` — both `set-prop` and `call` supported. Target is `ctx.evt` instead of a DOM element.

### Runtime (commands.ts)

`mutate-event` handler switches on `mutation.kind`: `set-prop` does `ctx.evt[prop] = val`, `call` does `ctx.evt[method](...args)`. Args resolved via `resolveMethodArg()` (same as element.ts). The `ctx.evt` for Fusion vendor events IS the raw SF event object — the reference survives through async HTTP pipelines.

### EventGather — Sending Event Args Values to Server

`FromEvent(args, x => x.Text, "MedicationType")` resolves the typed text from event args at runtime (not from component value). Plan JSON: `{ "kind": "event", "param": "MedicationType", "path": "evt.text" }`. Runtime walks `ctx.evt` using `walk()`. Required `evt` parameter to be threaded through `resolveGather()` → `http.ts`.

### C# DSL Pattern — Extension on Args Type

Extensions go **directly on the args class**, not on a separate builder. The args type IS the compile-time constraint:

```csharp
// In FusionAutoCompleteOnFiltering.cs — alongside the args class
public static void PreventDefault<TModel>(
    this FusionAutoCompleteFilteringArgs args,
    PipelineBuilder<TModel> pipeline) { ... }

public static void UpdateData<TModel, TResponse>(
    this FusionAutoCompleteFilteringArgs args,
    PipelineBuilder<TModel> pipeline,
    ResponseBody<TResponse> source,
    Expression<Func<TResponse, object?>> path) { ... }
```

**Why `pipeline` parameter is required:** `args` is a phantom created by the event descriptor — shared across the entire reactive lambda. It doesn't know which pipeline builder to emit into (`p` vs `s`). Unlike `ComponentRef` which is created per-context (`p.Component()` vs `s.Component()`), `args` comes from the outer scope.

### Usage in View

```csharp
.Reactive(plan, evt => evt.Filtering, (args, p) =>
{
    args.PreventDefault(p);
    p.Get("/url")
     .Gather(g => g.FromEvent(args, x => x.Text, "ParamName"))
     .Response(r => r.OnSuccess<TResponse>((json, s) =>
     {
         args.UpdateData(s, json, j => j.Items);
         s.Element("status").SetText("loaded");
     }));
})
```

### Calling SF Component Methods (separate from event args)

Any SF ej2 instance method is one line in the vertical slice extensions via `ComponentRef.Emit()`:
- `CallMutation("methodName")` → `ej2.methodName()`
- `CallMutation("methodName", args: new[] { new LiteralArg("val") })` → `ej2.methodName("val")`
- `SetPropMutation("propName")` + value → `ej2.propName = value`

Zero runtime changes. Plan carries method/prop name, runtime uses bracket notation.

### SF FilteringEventArgs.updateData Signature

`updateData(dataSource, query?, fields?)` — only `dataSource` is required. For server-side filtering (server already filtered), single arg is sufficient.

### Verified SF AutoComplete Behaviors

- `showSpinner/hideSpinner`: NO visible effect (SF spinner is a standalone utility from ej2-popups, not built into dropdown inputs)
- `refresh()`: causes focus loss mid-typing — NOT usable during filtering
- `preventDefaultAction`: suppresses "No records found" flash during async fetch — WORKS
- `updateData(data)`: the ONLY correct SF API for async server-filtered data — WORKS
- `dataSource + dataBind + showPopup`: workaround that renders results but is 3 calls vs 1 — AVOID

### Onboarding Checklist for Events with Methods

1. Args class with typed properties (e.g., `Text`) — same as any event
2. Extension method(s) on args class for each callable method/settable prop
3. Each extension emits `MutateEventCommand` with appropriate `Mutation` (call or set-prop)
4. No intermediary builders — args type IS the API surface
5. Comment in the vertical slice what was verified manually (what works, what doesn't)
6. `FromEvent()` for sending event args values as HTTP query params
