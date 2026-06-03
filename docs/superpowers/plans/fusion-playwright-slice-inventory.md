# Fusion Playwright Slice Inventory

Inventory produced for the mechanical Playwright organization pass.

Source of truth checked:

- `Alis.Reactive.Fusion/Components/*`
- `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/**/*`
- `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Fusion/**/*`
- `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/**/*`

## Prefix Findings

- Source package slices all use `Fusion*` component folders and type names.
- Sandbox routes are already under `Components/Fusion`; many model/view folders
  intentionally omit the `Fusion` prefix to preserve route readability.
- Playwright class names are mixed: broad component onboarding tests often use
  `WhenUsingFusion*`, while behavior tests use task names such as
  `WhenDateSelected` or `WhenFilteringWithChips`.
- This pass keeps class names behavior-first and makes the folder and namespace
  carry the Syncfusion context: `Components/Fusion/<Slice>/`.

## Move Inventory

| Slice | Pre-move Playwright path | Fusion prefix? | Target path |
| --- | --- | --- | --- |
| Accordion | `Components/Fusion/WhenAccordionPanelExpands.cs` | Folder only | `Components/Fusion/Accordion/` |
| AutoComplete | `Components/Fusion/WhenAutoComplete*.cs` | Folder only | `Components/Fusion/AutoComplete/` |
| ColorPicker | `Components/Fusion/WhenColorPicked.cs` | Folder only | `Components/Fusion/ColorPicker/` |
| DatePicker | `Components/Fusion/WhenDateSelected.cs` | Folder only | `Components/Fusion/DatePicker/` |
| DateRangePicker | `Components/Fusion/WhenDateRangeSelected.cs` | Folder only | `Components/Fusion/DateRangePicker/` |
| DateTimePicker | `Components/Fusion/WhenDateTimeSelected.cs` | Folder only | `Components/Fusion/DateTimePicker/` |
| DropDownList | `Components/Fusion/WhenDropdownItemSelected.cs` | Folder only | `Components/Fusion/DropDownList/` |
| FileUpload | `Components/Fusion/WhenFileUploaded.cs` | Folder only | `Components/Fusion/FileUpload/` |
| Grid | `Components/Fusion/Grid/*`, `WhenBindingArrayToGrid.cs`, `WhenFilteringWithChips.cs` | Mixed | `Components/Fusion/Grid/` |
| InPlaceEditor | `Components/Fusion/WhenInPlaceEditor*.cs`, `WhenResidentProfileForm*InPlaceEditor*.cs` | Mixed | `Components/Fusion/InPlaceEditor/` |
| InputMask | `Components/Fusion/WhenMaskedInputEntered.cs` | Folder only | `Components/Fusion/InputMask/` |
| MultiColumnComboBox | `Components/Fusion/WhenMultiColumnItemSelected.cs` | Folder only | `Components/Fusion/MultiColumnComboBox/` |
| MultiSelect | `Components/Fusion/WhenMultipleItemsSelected.cs` | Folder only | `Components/Fusion/MultiSelect/` |
| NumericTextBox | `Components/Fusion/WhenNumericValueEntered.cs` | Folder only | `Components/Fusion/NumericTextBox/` |
| RichTextEditor | `Components/Fusion/WhenRichTextEdited.cs` | Folder only | `Components/Fusion/RichTextEditor/` |
| Switch | `Components/Fusion/WhenSwitchToggles.cs` | Folder only | `Components/Fusion/Switch/` |
| Tab | `Components/Fusion/WhenTabSwitches.cs` | Folder only | `Components/Fusion/Tab/` |
| TimePicker | `Components/Fusion/WhenTimeSelected.cs` | Folder only | `Components/Fusion/TimePicker/` |
| Remaining onboarded Fusion components | `Components/Fusion/WhenUsingFusion*.cs` | Class + folder | `Components/Fusion/<Slice>/` |

Grid remains a larger vertical slice by design.
