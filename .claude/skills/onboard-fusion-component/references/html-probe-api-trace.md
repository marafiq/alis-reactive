# HTML Probe And API Trace

## Purpose

The raw HTML probe answers what to onboard. It must prove the Syncfusion JS object shape before any C# vertical slice is written.

For Fusion components, the question is not "what exists on the component?" The question is "what runtime behavior is missing after the Syncfusion MVC builder has already configured initial render?"

The expected output is an API trace matrix in the HTML page and a matching
trace JSON file under the Fusion artifact tree, not a prose summary hidden in
chat.

## Probe Checklist

1. Load the same vendor assets used by the sandbox:
   - `/vendor/syncfusion/material.css`
   - `/vendor/syncfusion/dist/ej2.min.js`
2. Instantiate the exact object:
   - `new ej.{namespace}.{ClassName}(options).appendTo("#{id}")`
3. Expose the instance:
   - `window.__fusionProbe.ej2`
4. Read the shipped JS source and d.ts for the exact class.
5. Compare against the Syncfusion MVC builder surface.
6. Trace:
   - property reads
   - property writes
   - method existence
   - method calls with exact arguments
   - method return values
   - event payload shape
   - event payload writable properties
   - event payload methods and their visible/runtime effect
   - visible DOM effect
7. Record one row per candidate member.
8. Render the trace matrix in the HTML page.
9. Write `traces/raw-ej2-{api-set}.trace.json`.
10. Link the trace from `master-usecases-index.md`.
11. Onboard only proven rows that are runtime gaps, not builder-only static options.

## Trace Matrix

| JS Expression | Kind | Builder Coverage | Args | Return | Event Payload | Proof | C# API |
|---|---|---|---|---|---|---|---|
| `ej2.prompt` | property read/write | builder sets initial `Prompt(...)`; runtime read/write still useful | none | `string` | n/a | value appears in footer | `Prompt()` / `SetPrompt(...)` |
| `ej2.executePrompt("hello")` | method call | not builder-covered | `string` | `void` | `promptRequest.prompt` | event fires, prompt rendered | `ExecutePrompt("hello")` |
| `ej2.addPromptResponse("ok", true)` | method call | not builder-covered | `string`, `bool` | `void` | n/a | response rendered | `AddPromptResponse(...)` |
| `ej2.getEvents()` | method read | not builder-covered | none | array | n/a | returned array count logged | `GetEvents()` |
| `ej2.selectedChips` | property read/write | builder sets initial `SelectedChips(...)`; runtime read/write still useful | `string[]` for chip values, `number[]` for index chips | selected ids/indexes | n/a | property reflects selected chips; index writes require `dataBind()` to update DOM; value writes are not stable on delete-enabled chips | `SelectedChipValues()` / `SelectedChipIndexes()` / `SetSelectedChipIndexes(...)` |
| `ej2.select([0, 2])` | method call | not builder-covered | `number[]` | `void` | n/a | both indexed chips become active | `SelectByIndexes(...)` |
| `ej2.remove([0, 2])` | method call | not builder-covered | `number[]` | `void` | deleted event per chip | indexed chips are removed | `RemoveByIndexes(...)` |

Rows above are examples. Replace them with actual observed rows from the running component.

## HTML Candidate Matrix

Before C# work starts, the probe page must show a candidate table:

| Column | Meaning |
|---|---|
| Candidate | JS property, method, event, payload member, or bridge-computed behavior |
| Kind | prop-read, prop-write, method, method-source, event, payload-read, payload-call, bridge |
| Builder | covered, not covered, or static-only |
| Args | exact argument list and order |
| Return/Payload | scalar/object/array shape proven by trace |
| Proof | visible DOM effect, trace row, method return, or event mutation effect |
| Proposed C# | typed Fusion member or event args shape |
| Outcome | implement, bridge-needed, exclude, or needs-proof |

Outcome rules:

| Outcome | Meaning |
|---|---|
| `implement` | direct EJ2 behavior; implement typed Fusion API |
| `bridge-needed` | not direct EJ2 payload, but browser facts prove an Alis bridge can own it |
| `exclude` | builder-covered, internal-only, unstable, or unneeded |
| `needs-proof` | promising but not yet proven enough |

The implementation may include only `implement` rows and intentionally selected `bridge-needed` rows. Excluded and unproven rows stay in the matrix with the reason.

## Browser Console Commands

```javascript
const probe = window.__fusionProbe;
const ej2 = probe.ej2;

probe.member("prompt", () => ej2.prompt);
probe.member("executePrompt type", () => typeof ej2.executePrompt);
probe.call("executePrompt", () => ej2.executePrompt("hello from probe"));
probe.call("addPromptResponse", () => ej2.addPromptResponse("probe response", true));
probe.member("keys", () => Object.keys(ej2).sort());
```

## Event Payload Commands

The generated probe exposes helpers for event args. Use these inside event handlers while the event is firing:

```javascript
const ej2 = new ej.dropdowns.AutoComplete({
  filtering: args => {
    probe.event("filtering", args);
    args.preventDefaultAction = true;
    probe.eventCall("filtering.updateData", args, "updateData", () =>
      args.updateData([{ text: "Aspirin", value: "rx-aspirin" }])
    );
  }
});
```

For schedule popup replacement:

```javascript
const ej2 = new ej.schedule.Schedule({
  popupOpen: args => {
    probe.event("popupOpen", args);
    if (args.type === "Editor") {
      args.cancel = true;
      probe.event("popupOpen.cancelled", args);
    }
  }
});
```

For grid state:

```javascript
const ej2 = new ej.grids.Grid({
  dataStateChange: args => {
    probe.event("dataStateChange", args);
  }
});
```

The payload trace must include `ownKeys`, callable function names, sampled property values, and the result of any method call being onboarded.

## Event Payload Capture

Wire events in the object constructor, not after-the-fact, unless the component docs prove late assignment is supported.

```javascript
const trace = [];

const component = new ej.interactivechat.AIAssistView({
  promptRequest: function (args) {
    trace.push({
      event: "promptRequest",
      keys: Object.keys(args).sort(),
      payload: JSON.parse(JSON.stringify(args, safeJson))
    });
    renderTrace();
  }
});
```

Use a safe serializer because Syncfusion event payloads often contain DOM references.

```javascript
function safeJson(key, value) {
  if (value instanceof Element) return `[Element#${value.id || value.tagName}]`;
  if (typeof value === "function") return "[Function]";
  return value;
}
```

Sanitize values before storing them in `trace`. Many Syncfusion instances contain circular object graphs; `JSON.stringify(trace, replacer)` is too late if the raw circular object is already inside `trace`.

## Decision Gate

Do not implement a member unless all fields are known:

| Required Fact | Why |
|---|---|
| JS path | Needed for `ComponentProperty` / `ComponentMethod` |
| Builder coverage | Prevents wrapping static configuration already handled by Syncfusion MVC |
| Access kind | read, write, call, event |
| Args shape and order | Needed for `WithArgs<T...>()` and runtime call order |
| Return shape | Needed for `Read<TReturn>(method, args)` |
| Event payload shape | Needed for typed event args and nested classes |
| Event payload methods | Needed for event-arg extension methods |
| Sync/async behavior | Runtime lane must stay correct |
| Visible or trace proof | Prevents onboarding dead APIs |

## Mapping Rules

| Observed JS | C# Model |
|---|---|
| `ej2.x` | `ComponentProperty<T>.Named("x")` |
| `ej2.a.b` | `ComponentProperty<T>.Mapped("name", "a.b")` |
| `ej2.doThing()` | `ComponentMethod.Named("doThing")` |
| `ej2.doThing(a, b)` | `ComponentMethod.Named("doThing").WithArgs<TA, TB>()` |
| `ej2.doThing(a, b)` returns value | `self.Read<TReturn>(DoThingMethod, args)` |
| overloaded `ej2.doThing(...)` | `ComponentMethod.Mapped("doThingForX", "doThing")` and another mapped method for the other shape |
| `args.cancel = true` | `ReactionGraph.Set(PayloadSource.Event(), "cancel", ValueExpression.Literal(true))` |
| `args.updateData(data)` | `ReactionGraph.Call(PayloadSource.Event(), "updateData", args)` |

## Builder Coverage Gate

Before writing C#, classify each traced member:

| Class | Onboard? | Reason |
|---|---:|---|
| Initial render option already exposed by `Syncfusion.EJ2.*Builder` | No | the builder is already typed and owns SSR/static setup |
| Property that reactive gather/condition/http needs to read after render | Yes | runtime state source |
| Property that a reactive pipeline needs to mutate after render | Yes | runtime behavior |
| Public JS method | Yes | builder cannot express post-render method execution |
| Public JS method returning value | Yes | typed source for gather/conditions/http |
| Event payload or cancellable event args | Yes | reactive plan needs typed event data/mutation |

## Browser Data Shape Gate

Syncfusion field names are evaluated in the browser against the serialized JSON
shape, not the C# property shape. If the app uses camelCase JSON, builder string
fields must use camelCase:

| C# Property | Browser Data Field | Syncfusion Builder Field |
|---|---|---|
| `Status` | `status` | `KeyField("status")` |
| `Id` | `id` | `HeaderField = "id"` |
| `Summary` | `summary` | `ContentField = "summary"` |
| `FacilityId` | `facilityId` | `SwimlaneSettings.KeyField = "facilityId"` |

Typed `FusionTemplate.Create<T>()` expressions already emit camelCase bindings.
Do not replace typed templates with raw string templates to fix casing. Fix the
Syncfusion builder field names to the real browser data shape.

Literal objects passed as runtime method arguments also serialize through the
plan serializer. Confirm their browser shape in the rendered plan before using
them for methods like `addColumn(...)` or `openDialog(...)`.

## Probe Artifact Policy

Raw EJ2 probes for accepted API sets are durable workflow artifacts and belong
under:

```text
tools/FusionOnboarding/wwwroot/onboarding/fusion/{componentName}/probes/
```

Throwaway experiments outside that tree may be deleted after the accepted probe
and trace artifacts exist.
