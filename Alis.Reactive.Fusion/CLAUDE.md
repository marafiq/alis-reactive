# Alis.Reactive.Fusion — Syncfusion Slices

Root `../CLAUDE.md` is authoritative. This file adds only what is specific to a Fusion slice.

One self-contained file per Syncfusion EJ2 component: the declaration (its properties, methods and
events by their JavaScript names, `FusionX : FusionComponent`), its payload classes, and its render
helper (`field.Render<FusionX>(...)` for an input, `plan.Render<FusionX>(id, ...)` otherwise). No
per-member extension methods, events classes or reactive extensions: a page uses the declaration's
own members (`rate.Min.Set(0m)`, `grid.Refresh()`) and the core `.Reactive`. Slices compile against
the core's public surface only, as a third-party component would. Duplication between slices is
intentional; there are no shared behavior base classes.

## Declaring members

Each JavaScript member is a real C# member. Its JavaScript name is the C# name camel-cased, unless
`name:` gives one. From `FusionGrid.cs`, `FusionNumericTextBox.cs` and `FusionToast.cs`:

```csharp
public Prop<decimal> Value => Prop<decimal>();                                 // read and written
public ReadOnly<bool> IsInteracted => ReadOnly<bool>();                        // read (on a payload)
public Prop<FusionToastTone> Tone => Prop<FusionToastTone>("cssClass");       // a JavaScript name of its own
public void Refresh() => Call();                                               // a method
public void SelectRow(Arg<int> index) => Call(index);                          // a method with an argument
public TypedSource<int[]> GetSelectedRowIndexes() => Invoke<int[]>();          // a method that returns a value
public void HideColumn(Arg<Field> key) => Call(key, Fixed("field"), name: "hideColumns");
public TypedEvent<FusionNumericTextBoxChangeArgs> Change => Event<FusionNumericTextBoxChangeArgs>();   // an event
public TypedEvent<FusionTooltipOpenedArgs> Opened => Event<FusionTooltipOpenedArgs>("open");     // beside an Open() method
```

- `CallAsync()` and `InvokeAsync<T>()` declare a method that returns a promise; the steps after it
  wait for it to settle.
- `Arg<T>` parameters take a literal or a value read when the step runs, in the JavaScript method's
  own order. Name each parameter as the EJ2 API names it.
- `Fixed(literal)` is an argument the JavaScript method always takes and the page never chooses,
  such as the `"field"` key of a column method. `Pass<TDeclared, TSource>(items)` passes the page's
  own list where EJ2 declares a wider type (`FusionAutoCompleteFilteringArgs.UpdateData`).
- Members typed by the app's class are generic methods (`GetSelectedRecords<TRow>()`,
  `RowSelected<TRow>()`); row members are `Field` arguments.
- An event payload is a class deriving `JsObject`: `ReadOnly<T>` for what it carries, `Prop<T>` for
  what EJ2 reads back after the handler (`Cancel`, `PreventDefaultAction`), and methods for what it
  can do (`UpdateData`). Plain data inside a payload (a row, an action) stays a plain class.
- The declaration and its payload classes sit between `#nullable disable annotations` and
  `#nullable restore annotations`, so a page can pass values of either nullability. The render
  helper stays outside that region.

The method in full: `docs/design/bring-your-own-component.md`.

## When editing here

- Zero changes in `Alis.Reactive.Assets/runtime/`. If a runtime change seems needed, the plan is
  missing information; make the C# slice carry it.
- New typed surface (members, events, builder methods) stays inside the slice. Changing anything
  shared risks every slice, so trace downstream use first. Why: one shared-surface change once
  cascaded across 170+ files.
- Member and event payload names come from the EJ2 source, not from memory or vendor docs alone. A
  payload property like `cancel` is behavior to prove, not a string to copy.
- Input IDs derive from the model expression; every other ID is the developer's explicit choice.
- Typed members only (ALIS009). Strings stop at the plugin boundary.

## Proof

`dotnet build` proves shape only. A slice member is supported when a Playwright test drives it on a
sandbox page and fails if it breaks.
