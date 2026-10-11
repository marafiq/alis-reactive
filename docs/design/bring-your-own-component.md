# Bring your own component

Status: proposal on `feature/component-redesign`. Verification: Playwright only.

## Goal

A developer takes a dependency on Alis.Reactive and onboards any component that already works in
the browser without us: a Syncfusion control we never wrapped, a vanilla JS widget, their own. They
install and bundle the library; we bring the types. One C# declaration describes the component's JS
object (properties, methods, events with payloads) using the names its own JS or MVC API uses. From
then on every framework feature works for it through the same fluent API, and the framework supplies
defaults so the declaration stays short. We work one component, one vertical slice at a time.

## Rules every component obeys

The framework dictates these; a brought component cannot opt out.

- Input IDs come from `IdGenerator`; `name` is the binding path; other IDs are the developer's
  explicit choice. The runtime resolves by `getElementById` only.
- `InputField` renders the inline error element `{id}_error` (`InputFieldBuilder.cs:56`).
- Views carry no JavaScript. Library code lives in the app's bundle; behavior lives in the plan.
- Strings appear only at the declaration boundary (member names as the library spells them).
  Everything authored after that is typed.
- The runtime trusts framework-generated plans. Errors only at boundaries: library not
  registered, object not found, member missing.
- Sync reactions stay sync. Construction and member access are sync.

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
- **Developer JS already has an entry point.** An app bundle pushes onto `window.__alisPlugins`
  before boot and the runtime registers it (`sandbox-plugins.ts`, `plugins/catalog.ts:32`).

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
   (`component-driver.ts:74-90`, `event-fusion.ts`, `event-native.ts`). Nothing else can be found or
   subscribed to.
4. **An input's value must be a property.** Gather reads `properties[valueMember]`
   (`gather.ts:168`, `plan.ts:140-145`). Many libraries expose value only through methods
   (`getValue()`, `setValue()`).
5. **Live validation assumes an event named `change`** (`live-clear.ts:99-106`).
6. **Nothing constructs a library component.** Syncfusion's MVC helper constructs its own; any other
   library has no way in without JS in the view.

## Design (PROPOSED)

### 1. One declaration per component, public, in the component's own words

```csharp
// TomSelect.cs is the whole slice. Member names are Tom Select's JS API (slice 2 confirms them in a browser).
public sealed class TomSelect : InputComponent<TomSelect, string>
{
    protected override ComponentVendor Vendor => ComponentVendor.Named("tom-select")
        .CreatedBy("TomSelect", CreateStyle.New)  // new TomSelect(element, options)
        .EventsVia("on", "off")                   // instance.on(name, handler)
        .DestroyWith("destroy");

    protected override InputValue<string> Value => InputValue.ByMethods<string>("getValue", "setValue");

    public ComponentMethod Clear => Method("clear");
    public ComponentMethod Open => Method("open");
    public ComponentProperty<bool> IsOpen => Property<bool>("isOpen");
    public ComponentMethod<TomSelectOption> AddOption => Method<TomSelectOption>("addOption");
    public TypedEvent<string> Change => Event<string>("change");   // payload: the new value
    public TypedEvent<string> Type => Event<string>("type");       // payload: the typed text

    public sealed class Options                   // standard System.Text.Json, camelCase
    {
        public int? MaxItems { get; set; }
        public string Placeholder { get; set; }
        public bool Create { get; set; }
    }
}
```

The base classes `Component<TSelf>` and `InputComponent<TSelf, TValue>` own the protected
factories (`Property<T>`, `Method`, `Event<T>`) and the defaults. Strings stop here.

### 2. The fluent API does not change

```cshtml
@Html.InputField(plan, m => m.Facility, o => o.Required().Text("Facility"))
     .Component<TomSelect>(new TomSelect.Options { MaxItems = 1, Placeholder = "Pick a facility" },
         on => on.Event(c => c.Change, (value, p) =>
             p.When(value).Eq("north")
              .Then(t => t.Component<FusionNumericTextBox>(m => m.Rate).SetValue(10m))))
```

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
  "tom-select": { "root": { "kind": "created" },
                  "create": { "style": "new", "plugin": "TomSelect" },
                  "events": { "subscribe": "on", "unsubscribe": "off", "payload": "first-argument" },
                  "destroy": "destroy" }
},
"components": {
  "Intake_Facility": { "vendor": "tom-select", "create": { "options": { "maxItems": 1 } },
    "binding": { "kind": "registered-input", "bindingPath": "Facility",
                 "value": { "read": "getValue", "write": "setValue" }, "changeEvent": "change" } }
}
```

Syncfusion and native stop being special: the Fusion and Native packages declare their vendor once,
the same way a developer declares theirs.

### 4. The runtime gets smaller, once

- One generic driver interprets the vendor contract: find the root (element, element path, or the
  instance it created), subscribe and unsubscribe by the declared method names, shape the payload.
  The two hard-coded drivers and both event adapters fold into it.
- At boot (and on slot load), a component whose vendor declares `create` is constructed from the
  constructor the app registered on `__alisPlugins`, with the typed options from the plan. Slot
  unload calls the declared `destroy`.
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

1. **NumericTextBox on the public surface.** One declaration file replaces the 8-file slice. No
   plan or runtime change. Proof: the existing 16 tests stay green unchanged
   (`scripts/playwright.sh --filter "FullyQualifiedName~NumericTextBox|FullyQualifiedName~InPlaceEditor"`).
2. **Tom Select, brought the way a developer would.** `npm install tom-select` in the sandbox,
   registered in `sandbox-plugins.ts`, declared in one file. Adds vendors-as-data, construction,
   method-backed value and the declared change event. Proof, one journey: picking a facility
   updates the rate field; submitting empty shows the required message in `{id}_error`; picking
   clears it; submitting posts the picked value.
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
- Looking constructors up on `window`: implicit; the plugin queue is the explicit path that exists.

## Open questions for the owner, most design-changing first

1. **Construction.** Should the framework construct brought components from typed options
   (proposed: keeps views JS-free and options typed), or should the app bundle construct them and
   we only locate the object?
2. **Class names.** Following the component's own name makes `NumericTextBox`, which collides with
   Syncfusion's `Syncfusion.EJ2.Inputs.NumericTextBox` builder in views. Keep `Fusion` for
   Syncfusion only, or rename and use a namespace alias?
3. **Vocabulary.** Keep `vendor` in the plan, or rename it `library` now that it is data?
4. **Multi-argument payloads** (for example noUiSlider passes values, then handle): first argument
   only for now, or a typed positional payload?
