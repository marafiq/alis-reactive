# Fusion components

The Syncfusion EJ2 slices: each one declaration of a component's members and events by their JavaScript names, the
payloads its events hand a listener, and the render helpers that render its element, built on the core's public
surface as an app's own component is, with FusionMultiSelect the reference (D17.8). Module 21 of `spec/CLAUDE.md`.

## Core questions

- Q17: the dependency is inverted, so Fusion gets nothing an app does not (D17.1); a component is one declaration and a
  render helper (D17.2); Syncfusion's vendor is data: its object at `ej2_instances.0` on the element, listened to by
  `addEventListener` and `removeEventListener` (D17.3, N17.7), each event's payload the listener's first argument
  (D17.4, N17.1); the vendor contract checks each declaration against EJ2's TypeScript (D17.5, N17.2); every vendor
  package is pinned to one version, 33.1.47 (D17.6); a render helper starts its component in the state the model gives
  it, written as the plan writes a literal (D17.13, N17.10).
- Q5: a component is a live object, its members declared by the door of their JavaScript kind, with presence and
  nullability marks (D5.1, D5.3, N5.4), and its payloads read with `Read` (N5.6) and called by method kind (N5.3).
- Q2: a member is named by the app's System.Text.Json options, never by a rule of the framework's own (D2.9); a
  component's member by its JavaScript name (D2.7).
- Q7: every value position takes every source, so a property is set and a method passed any source of its type.

## Boundaries

A declaration's vocabulary is module 11's, a vendor's listening and its driver module 12's, an input's value and the
event that commits it module 7's (M7.N3), and a render helper's starting state, N17.10's `ValueJson` and
`Json(options)`, module 7's; this module holds each Syncfusion slice that uses them. The template engine and its
conditionals (FusionTemplateBuilder, FusionConditionalBuilder, FusionTemplate, FusionTemplateExpression) are module
22's; FusionConfirm and FusionToast module 23's; the action link and the row actions (D17.15) module 25's. A slice stays
whole: FusionGrid's edit templates (`FusionGridEditTemplates`) and its exports stay here. The vendor contract analyzer's
code (N17.2) is module 20's; the rules it holds a Fusion declaration to are this module's defects.

Each slice is one feature, `M21.F<k>`, and one door, `M21.N<k>`, with the same k: 1 FusionMultiSelect, 2 the package
(FusionComponent and ReactiveFusionServiceCollectionExtensions), 3 to 52 the other slices in alphabetical order. A
slice's door is its declaration class, its payload declarations and its render helpers; every member of the slice is a
member of that door. Main's slice is seven files of a marker class, an events class, `ComponentRef` extensions, payload
classes with setters, payload extensions, a `Reactive` extension on the vendor's builder and a render helper
(`Alis.Reactive.Fusion/Components/FusionMultiSelect/` at 623dd4a27); the redesign's is one declaration file
(`Alis.Reactive.Fusion/Components/FusionMultiSelect/FusionMultiSelect.cs` at 65fdf118d), the shape the rewrite keeps.

## Doors

Each name is gate 2's; each door is the capability it carries.

- `M21.N1` kept: `FusionMultiSelect`, the reference: `Value`, `Items<TItem>()` (`dataSource`), `DataBind`, `ShowPopup`,
  `HidePopup`, `Change` and `Filtering`, with `IInputComponent.Input` committed by `Change` (M7.N3); the payloads
  `FusionMultiSelectChangeArgs` (`value`, `isInteracted`) and `FusionMultiSelectFilteringArgs` (`text`,
  `preventDefaultAction`, `updateData`); and the render helpers `field.FusionMultiSelect()`,
  `field.FusionMultiSelect(build => ..)` and `field.FusionMultiSelect(items, build => ..)`, which hands EJ2 the items
  by M21.D1, with `Fields<TItem>(text, value)` and `Fields<TItem>(text, value, groupBy)`.
  It replaces main's seven files, whose doors carry their verdicts in `spec/FusionComponents/features.json`. Each
  JavaScript name is EJ2 33.1.47's public member (`@syncfusion/ej2-dropdowns/src/multi-select/multi-select.d.ts:170`,
  `:416`, `:528`, `:615`, `:945`, `:952`). `Value` is declared `Property<string[]?>`, as the vendor's value holds null
  until a choice (M21.X1).
- `M21.N2` kept: the package. `FusionComponent`, the base of every Fusion declaration, carries `FusionVendor`:
  `ComponentVendor.Define("fusion", objectPath: "ej2_instances.0", subscribe: "addEventListener", unsubscribe:
  "removeEventListener")`, with no payload kind, since each event declares its payload's place (N17.1; module 12). It
  replaces main's string vendor (`Alis.Reactive.Fusion/FusionComponent.cs:14` at 623dd4a27). Every Fusion render helper
  writes the C# values it hands EJ2 by M21.D1, so `AddReactiveFusion` leaves (gate 2). Every Fusion declaration marks
  `optional: true` on each member EJ2's TypeScript marks `?` (D5.3, N5.4), which ALIS022 checks both ways; neither line
  has the mark.
- `M21.N23` kept: `FusionGrid`: `Rows<TRow>()` and `Page<TRow>()` (`dataSource`), `Refresh`, the paging, sorting,
  filtering, searching and grouping methods, the column methods (`ShowColumn` and `HideColumn` by `showColumns` and
  `hideColumns` with `"field"`, `AutoFitColumns`, `AutoFitColumn` by `autoFitColumns`, `ReorderColumns`,
  `GetColumnByField` returning a `FusionGridColumn`, `RefreshHeader`, `OpenColumnChooser`), selection, editing
  (`AddRecord`, `AddRecordAt` by `addRecord`, `UpdateRow`, `SetRowData`, `SetCellValue`, `EditCell`, `UpdateCell`,
  `SaveCell`, `GetBatchChanges`, `StartEdit`, `EndEdit`, `CloseEdit`, `DeleteRecord`), `ExcelExport`, `CsvExport` and
  `PdfExport` as `MethodAsync`, and `Print`; ten events and their payloads; `FusionGridSortDirection` and
  `FusionGridTextFilterOperator` with System.Text.Json's string enum converter; the render helper
  `Html.FusionGrid(plan, id, build => ..)` and `Html.FusionGrid(plan, id, rows, build => ..)`, which hands EJ2 the rows
  by M21.D1; `FusionGridColumns.Of<TRow>().Field(..)`; `FusionGridEditTemplates` and its dialog form; and
  `FusionGridValidation.From<TRow>(rules).Field(..)`, a column's rules from the validator (N16.2). It replaces main's
  twenty-two files (`Alis.Reactive.Fusion/Components/FusionGrid/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-grids/src/grid/base/grid.d.ts:1558`, `:3067`, `:3076`, `:3479`, `:3687`, `:3849`, `:3859`,
  `:3870`).
- `M21.N26` kept: `FusionKanban`: `Cards<TCard>()` (`dataSource`), `GetColumnData<TCard>`, `GetSwimlaneData<TCard>`,
  `DataBind`, `AddCard`, `UpdateCard`, `DeleteCard` (a card, a list or a key, M21.X302), `AddColumn`, `DeleteColumn`,
  `ShowColumn`, `HideColumn`, `ShowSpinner`, `HideSpinner`, `OpenDialog` with `FusionKanbanDialogAction` (M21.X301)
  and `CloseDialog`; fourteen events and their payloads, each cancel a `Property<bool>` and `DataSourceChanged`'s
  `EndEdit` and `CancelEdit` its methods; `FusionKanbanColumn` and `FusionKanbanHeaderArgs`, each member named by
  `[JsonPropertyName]` (M21.X303); and the render helper `Html.FusionKanban(plan, id, build => ..)`. It replaces
  main's seven files (`Alis.Reactive.Fusion/Components/FusionKanban/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-kanban/src/kanban/base/kanban.d.ts:112`, `:234`, `:429`, `:468`, `:484`, `:502`, `:511`, `:527`).
- `M21.N34` kept: `FusionPivotView`: `CurrentView` (`currentView`, which EJ2 marks `@hidden`, a gate 2 item),
  `DataSourceSettings` as `LiveObject<FusionPivotViewDataSourceSettings>()` with `Rows<TRow>()` (`dataSource`),
  `DisplayOption` as `LiveObject<FusionPivotViewDisplayOption>()` with `View` and `Primary` (M21.D301), `Refresh`,
  `GetPersistData`, `LoadPersistData`, `ShowConditionalFormattingDialog` and `CreateCalculatedFieldDialog`; `DataBound`
  and `CellSelecting`, whose payload reads its cell as `FusionPivotViewCellData`, each member named by
  `[JsonPropertyName]` (M21.X304); and the render helper `Html.FusionPivotView(plan, id, build => ..)`. It replaces
  main's eight files (`Alis.Reactive.Fusion/Components/FusionPivotView/` at 623dd4a27), whose doors carry their verdicts
  in `spec/FusionComponents/features.json`. Each JavaScript name but `currentView` is EJ2 33.1.47's public member
  (`@syncfusion/ej2-pivotview/src/pivotview/base/pivotview.d.ts:1447`, `:1476`, `:2244`, `:2460`, `:2467`, `:2816`).
- `M21.N39` kept: `FusionSchedule`: `CurrentView`, `SelectedDate`, `EventSettings` as
  `LiveObject<FusionScheduleEventSettings>()` with `Events<TEvent>()` (`dataSource`), `DataBind`, `GetEvents<TEvent>`,
  `AddEvent`, `SaveEvent`, `DeleteEvent<TKey>`, `OpenEditor`, `CloseEditor`, `RefreshEvents`, `Print` and `ScrollTo`;
  `ScheduleAction`, with System.Text.Json's string enum converter (M21.X305); nine events and their payloads, an
  event's data the app's own class `TEvent` and `Navigating`'s dates read as `DateTime` (M21.X306); and the render
  helper `Html.FusionSchedule(plan, id, build => ..)`. It replaces main's fifteen files
  (`Alis.Reactive.Fusion/Components/FusionSchedule/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-schedule/src/schedule/base/schedule.d.ts:250`, `:274`, `:707`, `:867`, `:1716`, `:1726`, `:1878`).

The meaning `M21.D1` and the defects `M21.X1` to `M21.X3` are in `spec/FusionComponents/features.json`, on the
features they give.

## Improvements

- One naming for a slice's data (M21.D1, M21.X2). EJ2's builder writes C# data through Newtonsoft's process-wide
  `JsonConvert.DefaultSettings` and never reads a `JsonPropertyName` (`probe:fusion-ej2-builder-newtonsoft-defaults`).
  The redesign names a text field by System.Text.Json (`Alis.Reactive/Members/JsonMemberName.cs:11` at 65fdf118d), so a
  member named `label` shows no text in data EJ2 wrote as `displayLabel`; main names it by the camel case of its C#
  name (`Alis.Reactive/PlanAuthoring/ExpressionPaths/JsonMemberName.cs:9` at 623dd4a27), so data an endpoint wrote as
  `label` shows none. A JRaw of the plan's literal text passes through EJ2 as written
  (`probe:fusion-ej2-builder-raw-json`), so one writer names both.
- No process-wide setting (M21.X3). `AddReactiveFusion` sets Newtonsoft's default settings for the whole process on
  both lines (`Alis.Reactive.Fusion/ReactiveFusionServiceCollectionExtensions.cs:63` at 65fdf118d), so every
  `JsonConvert` call an app makes changes its names; the rewrite writes no Newtonsoft setting.
- A declaration types what the vendor holds (M21.X1). The redesign declares `Value` as `Property<string[]>`
  (`Alis.Reactive.Fusion/Components/FusionMultiSelect/FusionMultiSelect.cs:19` at 65fdf118d) where EJ2's `value` holds
  null by default, so main's clear, `SetValue(null)`
  (`Alis.Reactive.Fusion/Components/FusionMultiSelect/FusionMultiSelectExtensions.cs:36-41` at 623dd4a27), is a
  nullable warning; ALIS019 refuses the declaration and `Property<string[]?>` keeps the clear.
- An event's doc says when it is raised. The redesign's `Change` says it is "Raised when the selection changes"
  (`Alis.Reactive.Fusion/Components/FusionMultiSelect/FusionMultiSelect.cs:34` at 65fdf118d); EJ2 raises it at focus
  out unless `changeOnBlur` is false (`@syncfusion/ej2-dropdowns/src/multi-select/multi-select.d.ts:361-366`), which
  is when the live check re-checks the input it commits.
- One file per slice. Main's `Reactive` extension reads the element id back from the vendor builder's HTML attributes
  (`Alis.Reactive.Fusion/Components/FusionMultiSelect/FusionMultiSelectReactiveExtensions.cs:31-32` at 623dd4a27); the
  rendered component's `Reactive(event, pipeline)` (M12.N3) knows its id, so the slice needs no builder extension.
- An export is awaited (M21.X201). Both lines drop the promise `excelExport`, `csvExport` and `pdfExport` return
  (`@syncfusion/ej2-grids/src/grid/base/grid.d.ts:3849`, `:3859`, `:3870`), so a step after an export runs before
  its file is written; the rewrite declares them `MethodAsync` (D11.3, B15).
- A column's rule is checked by its editor or refused (M21.X203). Both lines drop a conditional rule and a rule the
  column validator has no key for where the view renders
  (`Alis.Reactive.Fusion/Components/FusionGrid/FusionGridValidation.cs:112` at 65fdf118d); the rewrite refuses it
  unless it is the server's.
- A select editor opens at the row's value (M21.X204). Main's select templates mark no option selected
  (`Alis.Reactive.Fusion/Components/FusionGrid/FusionGridEditTemplates.cs:88-95` at 623dd4a27); the redesign's do
  (`Alis.Reactive.Fusion/Components/FusionGrid/FusionGridEditTemplates.cs:121-122` at 65fdf118d), and the rewrite keeps
  them.
- A column checks an email or a URL as the server does (M21.X205). Main gives a column EJ2's own email and url rules
  (`@syncfusion/ej2-inputs/src/form-validator/form-validator.js:29-32`); the rewrite writes the validator's own
  patterns (`Alis.Reactive/PlanModel/Validation/BrowserRule.cs:62` at 65fdf118d).
- An enum argument is written as the vendor reads it (M21.X207, `@syncfusion/ej2-grids/src/grid/base/enum.d.ts:40`).
- A Kanban dialog's action is typed (M21.X301). The redesign's `OpenDialog` takes text
  (`Alis.Reactive.Fusion/Components/FusionKanban/FusionKanban.cs:87` at 65fdf118d), and its sandbox keeps EJ2's
  `"Add"` as a constant of its own; the rewrite takes `FusionKanbanDialogAction`, written as `openDialog` reads it.
- A card is deleted by its key again (M21.X302). The redesign's `DeleteCard` takes only a class
  (`Alis.Reactive.Fusion/Components/FusionKanban/FusionKanban.cs:58` at 65fdf118d), so main's delete by a response's
  card id (`Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Kanban/Index.cshtml:109` at 623dd4a27) does
  not compile; the rewrite takes a card, a list or a key, as `deleteCard` does.
- Every Kanban, PivotView and Schedule method takes any source (Q7). Main's `AddColumn`, `OpenAddDialog` and
  `OpenEditDialog` write C# literals only, and its `ColumnData`, `SwimlaneData`, `ShowColumn` and `ScrollTo` take a
  literal string (`Alis.Reactive.Fusion/Components/FusionKanban/FusionKanbanExtensions.cs:147-158` at 623dd4a27); the
  redesign takes an `Arg` of each, which the rewrite keeps.
- An event's data is the app's own class. Main's `FusionScheduleEventData` holds one app's staffing fields
  (`Alis.Reactive.Fusion/Components/FusionSchedule/Events/FusionScheduleOnEventRendered.cs:23-45` at 623dd4a27); the
  redesign reads `TEvent`, which the rewrite keeps.
- A schedule's dates are dates (M21.X306). Main declares `Navigating`'s dates as text, so a label shows the UTC instant
  (`Alis.Reactive.Assets/runtime/shared/shape-convert.ts:155` at 623dd4a27) and a request sends it; the redesign and
  the rewrite read `DateTime`.
- An enum argument and a data member are written as the vendor reads them (M21.X303 to M21.X305). `ScheduleAction`
  carries System.Text.Json's string enum converter, and the Kanban column and header and the pivot's cell name each
  member by `[JsonPropertyName]`.
- A pivot's shown view is read from public members (M21.D301). Both lines read `currentView`, which EJ2 marks
  `@hidden` (`@syncfusion/ej2-pivotview/src/pivotview/base/pivotview.d.ts:1037-1038`); the public `displayOption`
  carries the same view.
