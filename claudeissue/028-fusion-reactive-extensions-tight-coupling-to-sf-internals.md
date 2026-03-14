# DESIGN-028: Fusion ReactiveExtensions Tight Coupling to Syncfusion builder.model Internals

## Status: Design Issue — Fragile Vendor Coupling

## Files
- `Alis.Reactive.Fusion/Components/FusionDropDownList/FusionDropDownListReactiveExtensions.cs:37-39`
- `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxReactiveExtensions.cs:37-39`

## How to Reproduce

1. Inspect the Fusion reactive extension:
   ```csharp
   var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
   var componentId = (string)attrs["id"];
   var bindingPath = (string)attrs["name"];
   ```
2. `builder.model` is a Syncfusion internal type (e.g., `DropDownListModel`).
3. `HtmlAttributes` is a Syncfusion property that holds the HTML attributes dictionary.
4. This code makes three assumptions:
   - `builder.model` is accessible (Syncfusion internal, could change between versions)
   - `HtmlAttributes` is non-null (requires `HtmlAttributes()` to be called on the builder first)
   - `"id"` and `"name"` keys exist in the dictionary (requires IdGenerator-based factory to have set them)
5. If any assumption is violated, the code throws an unhelpful `InvalidCastException` or `KeyNotFoundException`.

## Deep Reasoning: Why This Is a Design Issue

The Native components solve this cleanly: the builder stores `ElementId` and `BindingPath` as internal properties set in the constructor. The reactive extension reads these properties directly — no vendor API introspection needed.

The Fusion components use a different pattern because they delegate HTML rendering to Syncfusion's `DropDownListFor<>` / `NumericTextBoxFor<>` tag helpers, which are closed-source. The element ID and binding path are set via `HtmlAttributes(new { id = ..., name = ... })` on the SF builder, and there is no other way to retrieve them except by reading back from the SF model's `HtmlAttributes` dictionary.

This creates a fragile coupling: if Syncfusion changes the `HtmlAttributes` property name, changes its type, or moves it in a version upgrade, the framework breaks with a cryptic runtime error (not a compile error, because the access goes through dynamic dictionary lookup).

Additionally, the three unsafe operations on lines 37-39 have no error messages. If `.Reactive()` is called before the factory method (which sets `HtmlAttributes`), the cast to `IDictionary<string, object>` fails with `InvalidCastException: Object reference not set to an instance of an object` or `Unable to cast object of type 'Object' to type 'IDictionary<String,Object>'`. Neither message tells the developer what went wrong.

## How Fixing This Improves the Codebase

1. **Store componentId and bindingPath on a wrapper**: Create a thin wrapper type around the SF builder that stores the values set by the factory method. The reactive extension reads from the wrapper, not from SF internals.
2. **Fail-fast with clear error**: If `componentId` or `bindingPath` is not set when `.Reactive()` is called, throw: `"Call Html.DropDownListFor(plan, expr) before .Reactive() — component is not registered."`.
3. **SF version resilience**: The wrapper isolates the framework from SF internal changes. Only the factory method touches SF APIs; the reactive extension reads from framework-owned storage.

## How This Fix Will Not Break Existing Features

- The factory method (`DropDownListFor`) already computes `componentId` and `bindingPath` using `IdGenerator.For()` and `html.NameFor()`. The fix stores these values in the wrapper at factory time — same data, different storage location.
- The SF builder chain is unaffected — the wrapper wraps it, not replaces it.
- The `.Reactive()` call reads from the wrapper instead of from `builder.model.HtmlAttributes`. Same values, safer access.
- All existing Fusion tests and Playwright tests continue to work because the factory → reactive chain order is maintained.
