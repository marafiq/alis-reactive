# Playwright Patterns for Fusion Components

## Typed Locators (Alis.Reactive.Playwright.Extensions)

The `Alis.Reactive.Playwright.Extensions` project provides typed locators that encapsulate SF DOM structure. Prefer these over raw `Page.Locator()` calls:

- `ComponentScope` -- scoped entry point, knows `IdGenerator` pattern (`{TypeScope}__{PropertyName}`)
- `AutoCompleteLocator` -- `TypeAndSelect`, `Type`, `Clear`
- `DropDownListLocator`, `MultiSelectLocator` -- SF dropdown interactions
- `DatePickerLocator`, `TimePickerLocator`, `DateTimePickerLocator`, `DateRangePickerLocator` -- date/time inputs
- `NumericTextBoxLocator`, `InputMaskLocator`, `RichTextEditorLocator`, `MultiColumnComboBoxLocator`
- `PagePlan<TModel>` -- reads the plan JSON from the page, provides expression-based component locators

```csharp
// ComponentScope approach (untyped, uses property name strings):
var scope = new ComponentScope(Page, typeof(AutoCompleteModel));
var physician = scope.AutoComplete("Physician");
await physician.TypeAndSelect("smi", "Dr. Smith");

// PagePlan<TModel> approach (typed, uses expressions — breaks at compile time on rename):
var plan = await PagePlan<AutoCompleteModel>.FromPage(Page);
var physician = plan.AutoComplete(m => m.Physician);
await physician.TypeAndSelect("smi", "Dr. Smith");
```

## Test File Structure

Reference: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/AutoComplete/WhenAutoCompleteFiltersRemotely.cs`

Test file goes in `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/{Component}/When{BehaviorHappens}.cs` (e.g., `WhenAutoCompleteSuggests.cs`). Name describes the behavior under test, not the component.

```csharp
[TestFixture]
public class WhenAutoCompleteSuggests : PlaywrightTestBase
{
    // Use ComponentScope + typed locators from Playwright.Extensions
    // instead of raw string constants for Scope/ComponentId

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }
}
```

**For filtering events**, use `PressSequentiallyAsync` (not `FillAsync`) to trigger SF events.

## AutoComplete Filtering

Type directly into the component input:

```csharp
private async Task TypeInComponent(string text)
{
    var input = Page.Locator($"#{ComponentId}");
    await Expect(input).ToBeVisibleAsync();
    await input.ClickAsync();
    await input.PressSequentiallyAsync(text, new() { Delay = 50 });
}
```

## MultiSelect Filtering — DOM Structure

SF MultiSelect creates a SEPARATE filter input (sibling `input.e-dropdownbase`).
You CANNOT type into the component input itself — you must target the filter input:

```
SF MultiSelect DOM with AllowFiltering:
  .e-multi-select-wrapper (grandparent)
    └── span.e-searcher (parent)
        ├── input.e-dropdownbase (filter input — TYPE HERE)
        └── input#ComponentId (component input — NOT here)
```

```csharp
private async Task TypeInComponent(string text)
{
    // Target the filter input sibling, not the component input
    var filterInput = Page.Locator($"#{ComponentId}")
        .Locator("xpath=preceding-sibling::input[contains(@class,'e-dropdownbase')]");
    await Expect(filterInput).ToBeVisibleAsync(new() { Timeout = 5000 });
    await filterInput.ClickAsync();
    await filterInput.PressSequentiallyAsync(text, new() { Delay = 50 });
}
```

## Behavior-Focused Assertions

**Test the user-visible behavior**, not framework internals:
- Filtering: type -> HTTP fires -> popup shows results -> status element updates
- Changed: select item -> value displayed in echo element
- Cascade: parent change -> child populated from server
- Conditions: value matches -> then branch, doesn't -> else branch

## Popup Verification for `updateData`

Check popup items (not ej2.dataSource):

```csharp
var popupItems = Page.Locator(".e-ddl.e-popup .e-list-item");
await Expect(popupItems.First).ToBeVisibleAsync(new() { Timeout = 5000 });
```
