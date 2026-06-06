# Workflow Validation

This reference records the workflow-level validation for the deterministic
Fusion onboarding skill. It is not a substitute for per-component proof
artifacts. Each component still needs its own artifact tree under
`tools/FusionOnboarding/wwwroot/onboarding/fusion/{componentName}/`.

## Current Fusion Inventory

Current inventory validation found 51 component slices under
`Alis.Reactive.Fusion/Components/`:

```text
FusionAIAssistView
FusionAccordion
FusionAutoComplete
FusionBreadcrumb
FusionBulletChart
FusionButton
FusionCarousel
FusionCheckBox
FusionChipList
FusionColorPicker
FusionComboBox
FusionContextMenu
FusionDatePicker
FusionDateRangePicker
FusionDateTimePicker
FusionDialog
FusionDropDownButton
FusionDropDownList
FusionDropDownTree
FusionFileUpload
FusionGrid
FusionInPlaceEditor
FusionInputMask
FusionKanban
FusionListBox
FusionListView
FusionMention
FusionMenu
FusionMultiColumnComboBox
FusionMultiSelect
FusionNumericTextBox
FusionOtpInput
FusionPivotView
FusionProgressButton
FusionRadioButton
FusionRating
FusionRichTextEditor
FusionSchedule
FusionSidebar
FusionSlider
FusionSmartPasteButton
FusionSmartTextArea
FusionSplitButton
FusionStepper
FusionSwitch
FusionTab
FusionTextArea
FusionTextBox
FusionTimePicker
FusionToolbar
FusionTooltip
```

The inventory has sandbox views and controllers under the normal Fusion
component paths. Component-specific Playwright folders exist for the broad
inventory except `FusionDialog` and `FusionTooltip`, which currently have
sandbox views/controllers but no `tests/.../Components/Fusion/Dialog` or
`tests/.../Components/Fusion/Tooltip` folder. That gap is exactly why existing
C# and sandbox pages are evidence, not closure.

The deterministic workflow therefore validates two paths:

- new onboarding, where artifacts are built before any C# slice exists;
- existing audit, where current C#, sandbox, and tests are evidence only after
  the raw EJ2 discovery and primitive map prove them.

## Workflow Stress Matrix

| Required Coverage | Current Stress Evidence Used To Validate The Workflow |
|---|---|
| component read props | `FusionSchedule.CurrentView`, `FusionSchedule.SelectedDate`, `FusionKanban.Cards` |
| component write props | `FusionKanban.SetDataSource`, `FusionSchedule.SetDataSource` |
| nested component paths | `FusionSchedule` maps `eventSettings.dataSource`; Grid column field APIs map typed row expressions to browser field paths |
| no-arg methods | Grid `ClearSelection`, `ClearSorting`, `Print`; Kanban `ShowSpinner`; Schedule `CloseEditor` |
| one-arg methods | Grid `GoToPage`, `Search`; Kanban `ShowColumn`; Schedule `ScrollTo` |
| multi-arg methods | Grid `SortBy`, `FilterTextBy`, `SetCellValue`; Kanban `AddCard`, `OpenDialog`; Schedule `OpenEditor` |
| method return sources | Grid `CurrentViewRecords`, `SelectedRowIndexes`, `BatchChanges`; Kanban `ColumnData`, `SwimlaneData`; Schedule `GetEvents` |
| overloads | Grid typed string/int `SetCellValue` and `UpdateCell`; Kanban mapped `addCardAt`; Grid `AddRecord` with and without index |
| object args | Grid typed row `AddRecord`, `UpdateRow`, `SetRowData`; Kanban card/column/dialog args; Schedule event data |
| array args | Grid selected indexes/records sources and data-state arrays; Kanban card arrays and data-source arrays; Schedule event arrays |
| events | Grid data-state/edit/record/selection/toolbar events; Kanban action/card/drag/dialog/data-source events; Schedule cell/action/popup/navigation/render events |
| payload props | Grid `skip`, `take`, `sorted`, `where`, `search`, `action`; Kanban `requestType`, `addedRecords`, `changedRecords`, `deletedRecords`, `data`; Schedule `requestType`, `type`, `data` |
| writable payload props | Grid edit args `cancel`; Kanban cancellable args `cancel`; Schedule popup args `cancel` |
| payload methods | Kanban data-source changed args `endEdit` and `cancelEdit`; workflow requires raw lifecycle proof for any new payload method |
| nested payloads | Grid `action.*`; Schedule `popupOpen.data.*`; Kanban generic card payloads |
| array primitives and typed array sources | Grid `sorted[]`, `where[]`, `search[]`, batch changes; Kanban `List<TCard>` payloads; Schedule event collections through `GetEvents` |
| builder-owned exclusions | Grid edit settings/export/column setup, Kanban initial board setup, Schedule event settings stay on Syncfusion MVC builders unless post-render behavior is proven |
| vertical-slice files | Grid use-case partials, Kanban isolated slice, Schedule isolated slice with event files |
| 100 percent typed API proof expectations | `proof/typed-api-coverage-matrix.md` must have one row per public Fusion API member before a component audit or onboarding closes |

## Grid Stress Result

Grid validates the workflow against a large, partial-file vertical slice:

- component methods with no args, one arg, two args, and three args;
- mapped overloads for `setCellValue`, typed row operations, selection,
  sorting/filtering/grouping/searching, export/tooling, and column movement;
- method return sources for current rows, row indexes, selected rows, and batch
  changes;
- event payloads with scalar, nested, and array members, especially
  `dataStateChange` for sort/page/filter/search/group gestures;
- writable payload `cancel` flags on edit events;
- builder-owned exclusions for initial column definitions, edit settings, and
  export capabilities.

The workflow requirement that every public member gets a coverage-matrix row is
necessary here because one Grid Playwright test cannot prove the whole typed
surface.

## Kanban Stress Result

Kanban validates object and array arguments, stateful board workflows, generic
card payloads, and payload methods:

- `Cards`, `ColumnData`, and `SwimlaneData` exercise property and method-return
  sources;
- `SetDataSource`, `AddCard`, `UpdateCard`, `DeleteCard`, column operations,
  spinner operations, and dialog operations exercise writes and component
  method calls;
- events cover action, binding, card, drag, dialog, and data-source lifecycles;
- payload arrays and generic row-shaped payloads require typed `List<TCard>`
  modeling;
- `EndEdit` and `CancelEdit` are payload method calls and must be proven inside
  the live event lifecycle, not after the callback.

## Schedule Stress Result

Schedule validates nested component paths, non-input display components,
method-return sources, and lifecycle-sensitive payload mutation:

- `eventSettings.dataSource` maps a nested component property path for
  post-render data replacement;
- `CurrentView`, `SelectedDate`, and `GetEvents` exercise read and method-return
  sources;
- `AddEvent`, `SaveEvent`, `DeleteEvent`, `OpenEditor`, `CloseEditor`,
  `RefreshEvents`, `Print`, and `ScrollTo` exercise object arguments and
  method calls;
- action, navigation, popup, cell, event-click, data-bound, and rendered events
  require typed payload discovery;
- `PopupOpen.PreventDefault` proves writable payload behavior through
  `cancel`.

## Audit Rule

When auditing any existing component, do not mark the component closed from
current C# or Playwright files alone. Rebuild the artifact tree, then require:

```text
discovery row -> primitive-map row -> C# public member -> sandbox behavior -> Playwright assertion
```

If the row is missing at any point, either add the missing proof or remove/defer
the public API with an explicit audit-report row.

When a defect is reported against an existing Fusion component, treat the
defect as proof that at least one artifact row is missing, stale, or wrong. The
next pass must identify the first wrong row and carry the correction forward
through discovery, mapping, implementation, typed coverage, Playwright proof,
and audit report before claiming the issue is fixed.
