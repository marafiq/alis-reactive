# Bring your own component

Status: design on `feature/component-redesign`. Verification: Playwright only.

## Goal

A developer takes a dependency on Alis.Reactive and onboards any component that already works in
the browser without us: a Syncfusion control we never wrapped, a vanilla JS widget, their own. They
install, bundle and construct the library; we locate its object and bring the types. One C#
declaration describes the component's JS object (properties, methods, events with payloads) using
the names its own JS or MVC API uses. From then on every framework feature works for it through the
same fluent API, and the framework supplies defaults so the declaration stays short. We work one
component, one vertical slice at a time.

## Decisions (owner)

1. **The app constructs; we locate.** The app's bundle creates the widget on the element the
   framework rendered. The framework only finds the object from that element and talks to it.
2. **Syncfusion keeps the `Fusion` prefix** (`FusionNumericTextBox`). A brought component uses the
   library's own name (`TomSelect`). Members and events use the names the component's JS or MVC API
   uses (`Change`, not `Changed`).

## Rules every component obeys

The framework dictates these; a brought component cannot opt out.

- Input IDs come from `IdGenerator`; `name` is the binding path; other IDs are the developer's
  explicit choice. The runtime resolves by `getElementById` only.
- `InputField` renders the inline error element `{id}_error` (`InputFieldBuilder.cs:56`).
- Views carry no JavaScript. Library code, including construction, lives in the app's bundle, which
  loads before the runtime boots; behavior lives in the plan.
- Strings appear only at the declaration boundary (member names as the library spells them).
  Everything authored after that is typed.
- The runtime trusts framework-generated plans. Errors only at boundaries: object not found on its
  element, member missing.
- Sync reactions stay sync. Member access is sync.

## What already exists (CURRENT, verified in source)

- **The plan already describes JS objects as data.** `PlanDocument.types` holds one contract per
  object: properties (path, shape, access), methods (path, arguments, returns), events (channel)
  (`plan.ts:5-30`). `components` points each instance at its type and vendor (`plan.ts:68-81`).
- **The runtime has no per-component code.** `RuntimeObject` reads, sets and calls any member by
  declared path and shape (`runtime-object.ts:14-35`). Only the four app-level objects have modules.
- **The C# core is generic.** `ComponentProperty<T>` and `ComponentMethod` describe any member
  (`ComponentMember.cs:11-116`); `ComponentRef` emits set, call and read for any of them
  (`ComponentRef.cs:31-102`); `ComponentEventOnboarding.Wire` wires any typed event
  (`ComponentEventOnboarding.cs:13-35`).
- **Payload names already follow JS.** C# `Value` becomes `value` through the standard
  `JsonNamingPolicy.CamelCase` (`JsonMemberName.cs:9`).

## What blocks it (CURRENT)

1. **Encapsulation is inverted.** Every declaration API is `internal`, visible only to our own
   Fusion, Native and FluentValidator assemblies (`Alis.Reactive.csproj:69`). `TypedEvent`'s
   constructor is internal (`TypedEvent.cs:20`). Only we can onboard.
2. **Slices are ceremony.** NumericTextBox is 8 files and 237 lines to say: `value` and `min` are
   properties; `focusIn`, `focusOut`, `increment` and `decrement` are methods; `change`, `focus` and
   `blur` are events. Each member is a private static field plus an extension method; the
   `Reactive(...)` event extension is copied into every slice.
3. **Vendors are code, not data.** Two drivers are hard-coded: Syncfusion finds `ej2_instances[0]`
   and subscribes with `addEventListener`; native uses the element and DOM events
   (`component-driver.ts:74-90`, `event-fusion.ts`, `event-native.ts`). No other object can be
   found or subscribed to.
4. **An input's value must be a property.** Gather reads `properties[valueMember]`
   (`gather.ts:168`, `plan.ts:140-145`). Many libraries expose value only through methods
   (`getValue()`, `setValue()`).
5. **Live validation assumes an event named `change`** (`live-clear.ts:99-106`).

## Design

### 1. One declaration per component, public, in the component's own words

```csharp
// TomSelect.cs is the whole slice. Member names are Tom Select's JS API (slice 2 confirms them in a browser).
public sealed class TomSelect : InputComponent<TomSelect, string>
{
    protected override ComponentVendor Vendor => ComponentVendor.Named("tom-select")
        .FoundAt("tomselect")       // the app ran new TomSelect(element); it sits on element.tomselect
        .EventsVia("on", "off");    // instance.on(name, handler), instance.off(name, handler)

    protected override InputValue<string> Value => InputValue.ByMethods<string>("getValue", "setValue");

    public ComponentMethod Clear => Method("clear");
    public ComponentMethod Open => Method("open");
    public ComponentProperty<bool> IsOpen => Property<bool>("isOpen");
    public ComponentMethod<TomSelectOption> AddOption => Method<TomSelectOption>("addOption");
    public TypedEvent<string> Change => Event<string>("change");   // payload: the new value
    public TypedEvent<string> Type => Event<string>("type");       // payload: the typed text
}
```

The base classes `Component<TSelf>` and `InputComponent<TSelf, TValue>` own the protected
factories (`Property<T>`, `Method`, `Event<T>`) and the defaults. Strings stop here.

### 2. The fluent API does not change

```cshtml
@Html.InputField(plan, m => m.Facility, o => o.Required().Text("Facility"))
     .Component<TomSelect>(on => on.Event(c => c.Change, (value, p) =>
         p.When(value).Eq("north")
          .Then(t => t.Component<FusionNumericTextBox>(m => m.Rate).SetValue(10m))))
```

The framework renders the host element with the framework's ID and name; the app's bundle turns it
into the widget before the runtime boots.

```csharp
p.Component<TomSelect>(m => m.Facility).Call(c => c.Clear);
p.When(p.Component<TomSelect>(m => m.Facility).Value()).Eq("north");
p.Component<TomSelect>(m => m.Facility).Get(c => c.IsOpen);
```

`Set`, `Call` and `Get` are generic verbs on `ComponentRef`. `Value()`, `SetValue()`, gather,
validation and live clear come from `InputComponent` with no extra code. Per-member extension
methods (`.Increment()`) remain possible as one-line sugar, never required.

### 3. Vendors become data in the plan

```jsonc
"vendors": {
  "fusion":     { "root": { "kind": "element-path", "path": "ej2_instances.0" },
                  "events": { "subscribe": "addEventListener", "unsubscribe": "removeEventListener",
                              "payload": "first-argument" } },
  "native":     { "root": { "kind": "element" },
                  "events": { "subscribe": "addEventListener", "payload": "dom-event" } },
  "tom-select": { "root": { "kind": "element-path", "path": "tomselect" },
                  "events": { "subscribe": "on", "unsubscribe": "off", "payload": "first-argument" } }
},
"components": {
  "Intake_Facility": { "vendor": "tom-select",
    "binding": { "kind": "registered-input", "bindingPath": "Facility",
                 "value": { "read": "getValue", "write": "setValue" }, "changeEvent": "change" } }
}
```

Syncfusion and native stop being special: the Fusion and Native packages declare their vendor once,
the same way a developer declares theirs. If a library does not leave its instance on the element,
the app's construction code puts it there (`element.choices = new Choices(element)`).

### 4. The runtime gets smaller, once

- One generic driver interprets the vendor contract: find the root (the element or a path from it),
  subscribe and unsubscribe by the declared method names, shape the payload. The two hard-coded
  drivers and both event adapters fold into it.
- Input value reads and writes go through the declared member, property or method.
- Live validation listens to `binding.changeEvent`.

After this, bringing a component never touches the runtime again.

### 5. Defaults (the developer types less)

| Default | Value | Override |
|---------|-------|----------|
| Input value | property `value` | `InputValue.ByMethods(...)` or `ByProperty(...)` |
| Value-change event | `change` | `ValueChangedBy(c => c.Select)` |
| Event payload | first argument | vendor `payload` |
| Names | C# PascalCase to JS camelCase | standard `[JsonPropertyName]` |
| Syncfusion vendor | `ej2_instances[0]`, `addEventListener` | declared once in Fusion |
| Native vendor | the element, DOM events | declared once in Native |

## Slices, in order

Each slice is one component, proven by Playwright, run targeted while working.

1. **FusionNumericTextBox on the public surface.** One declaration file replaces the 8-file slice;
   event names follow Syncfusion (`Change`, `Focus`, `Blur`). No plan or runtime change. Proof: the
   existing 16 tests stay green unchanged
   (`scripts/playwright.sh --filter "FullyQualifiedName~NumericTextBox|FullyQualifiedName~InPlaceEditor"`).
2. **Tom Select, brought the way a developer would.** `npm install tom-select` in the sandbox, the
   sandbox bundle constructs it on the framework-rendered element, one declaration file. Adds
   vendors-as-data, method-backed value and the declared change event. Proof, one journey: picking
   a facility updates the rate field; submitting empty shows the required message in `{id}_error`;
   picking clears it; submitting posts the picked value.
3. **The remaining Fusion and Native slices**, one at a time, each proven by its existing tests.
4. **Remove `InternalsVisibleTo` for Fusion and Native.** The compiler then proves the public
   surface is enough.

## Rejected

- Generating declarations from npm packages or `.d.ts` files: tried in the old onboarding work and
  it did not hold up; names and behavior still had to be proven in a browser.
- Attributes plus reflection: the codebase rules out reflection-based extraction
  (`coding-principles.md`, Fail-Fast).
- Static abstract interface members: need the .NET 7 runtime; net48 cannot load them.
- Per-member extension methods as the only API: that ceremony is why one slice costs 8 files.
- The framework constructing brought components: the app owns construction (decision 1).

## Open questions

1. **Partial slots.** A brought component inside an injected partial needs the app to construct it
   after injection. Should the runtime raise a document event after a slot loads so the app's
   bundle can construct what arrived?
2. **Vocabulary.** Keep `vendor` in the plan, or rename it `library` now that it is data?
3. **Multi-argument payloads** (for example noUiSlider passes values, then handle): first argument
   only for now, or a typed positional payload?
