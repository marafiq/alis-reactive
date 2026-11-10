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
- `M21.N5` kept: `FusionAutoComplete`: `Value`, typed by the items' value member as M21.N1's items overload types
  MultiSelect's (gate 2) and admitting null (M21.X501), `DataSource<TItem>()`, `Enabled`, `DataBind`, `FocusIn`,
  `FocusOut`, `ShowPopup`, `HidePopup`, `Clear`, `Change` and `Filtering`, with `IInputComponent.Input` committed by
  `Change` (M7.N3); the payloads `FusionAutoComplete.ChangeEventArgs` (`Value`, `IsInteracted`) and
  `FusionAutoComplete.FilteringEventArgs` (`Text`, `PreventDefaultAction`, `UpdateData`); and the render helpers
  `field.FusionAutoComplete(build => ..)` and `field.FusionAutoComplete(items, build => ..)`, which hands EJ2 the items
  by M21.D1, with `Fields<TItem>(text, value)` and `Fields<TItem>(text, value, groupBy)` (M21.X503). `Text` leaves: EJ2
  marks AutoComplete's `text` `@private` (M21.X502, gate 2). It replaces main's seven files
  (`Alis.Reactive.Fusion/Components/FusionAutoComplete/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-dropdowns/src/combo-box/combo-box.d.ts:231`, `:373`, `:380`, `:387`,
  `@syncfusion/ej2-dropdowns/src/auto-complete/auto-complete.d.ts:140`, `:256`, `:263`,
  `@syncfusion/ej2-dropdowns/src/drop-down-list/drop-down-list.d.ts:157`, `:365`,
  `@syncfusion/ej2-dropdowns/src/drop-down-base/drop-down-base.d.ts:390`, `:826`, `:838`, `:847`).
- `M21.N6` kept: `FusionBreadcrumb`: `ActiveItem` (`activeItem`), `DataBind` and `ItemClick`, whose payload
  `FusionBreadcrumbItemClickArgs` reads its `item`; and the render helper `Html.FusionBreadcrumb(plan, id, build =>
  ..)`. It replaces main's seven files (`Alis.Reactive.Fusion/Components/FusionBreadcrumb/` at 623dd4a27), whose doors
  carry their verdicts in `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-navigations/src/breadcrumb/breadcrumb.d.ts:144`, `:223`).
- `M21.N11` kept: `FusionChipList`: `Add`, `SelectByIndex`, `SelectByIndexes` and `SelectByText` (`select`),
  `RemoveByIndex` and `RemoveByIndexes` (`remove`), `Find` returning a `FusionChipData`, `GetSelectedChip()` and
  `GetSelectedChips()` (`getSelectedChips`), `DataBind`, and `SelectedChipValue`, `SelectedChipIndex`,
  `SelectedChipValues` and `SelectedChipIndexes` (`selectedChips`); `Click` and `Deleted` and their payloads;
  `FusionSelectedChips` with `Texts`, `Data` and `Indexes`, the last by `[JsonPropertyName("Indexes")]`; and the
  render helper `Html.FusionChipList(plan, id, build => ..)`. It replaces main's eleven files
  (`Alis.Reactive.Fusion/Components/FusionChipList/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-buttons/src/chips/chip-list.d.ts:287`, `:428`, `:439`, `:449`, `:461`, `:468`).
- `M21.N12` kept: `FusionColorPicker`: `Value` as `Property<string>`, since EJ2's value is a color that starts at
  `#008000ff` (M21.X611), `Disabled`, `Toggle` and `Change`, with `IInputComponent.Input` committed by `Change` (M7.N3);
  the payload `FusionColorPicker.ColorPickerEventArgs` (`Value`); and the render helpers `field.FusionColorPicker()` and
  `field.FusionColorPicker(build => ..)`. It replaces main's six files
  (`Alis.Reactive.Fusion/Components/FusionColorPicker/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-inputs/src/color-picker/color-picker.d.ts:46`, `:59`, `:146`, `:268`, `:368`).
- `M21.N13` kept: `FusionComboBox`: `Value`, typed by the items' value member as M21.N1's items overload types
  MultiSelect's (gate 2) and admitting null, `Text` as `Property<string?>` (M21.X504), `Index` as `Property<int?>`,
  `DataBind`, `FocusIn`, `FocusOut`, `ShowPopup`, `HidePopup`, `Clear`, `Change`, `Focus` and `Blur`, with
  `IInputComponent.Input` committed by `Change` (M7.N3); the payload `FusionComboBox.ChangeEventArgs` (`Value`,
  `IsInteracted`), and the focus and blur payloads, which EJ2 types as `Object`, declaring no member (their class name a
  gate 2 gap); and the render helpers `field.FusionComboBox()`, `field.FusionComboBox(build => ..)` and
  `field.FusionComboBox(items, build => ..)`, which hands EJ2 the items by M21.D1, a door neither line has, with
  `Fields<TItem>(text, value)` (M21.X505). It replaces main's eight files
  (`Alis.Reactive.Fusion/Components/FusionComboBox/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-dropdowns/src/combo-box/combo-box.d.ts:92`, `:223`, `:231`, `:360`, `:367`, `:373`, `:380`, `:387`,
  `@syncfusion/ej2-dropdowns/src/drop-down-list/drop-down-list.d.ts:365`, `:389`, `:395`).
- `M21.N14` kept: `FusionContextMenu`: `Open(top, left)` and `Close`; `BeforeItemRender`, `BeforeOpen`, `OnOpen`,
  `BeforeClose`, `OnClose` and `Select` and their payloads, `FusionContextMenuBeforeOpenArgs` with `IsFocused`
  (optional, `isFocused`); and the render helper `Html.FusionContextMenu(plan, id, build => ..)`. It replaces main's
  twelve files (`Alis.Reactive.Fusion/Components/FusionContextMenu/` at 623dd4a27), whose doors carry their verdicts
  in `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-navigations/src/context-menu/context-menu.d.ts:77`, `:84`;
  `@syncfusion/ej2-navigations/src/common/menu-base.d.ts:207-242`, `:552`).
- `M21.N20` kept: `FusionDropDownList`: `Value`, typed by the items' value member as M21.N1's items overload types
  MultiSelect's (gate 2) and admitting null, `Text` as `Property<string?>` (M21.X506), `DataSource<TItem>()`,
  `DataBind`, `FocusIn`, `FocusOut`, `ShowPopup`, `HidePopup`, `Clear`, `Change`, `Focus` and `Blur`, with
  `IInputComponent.Input` committed by `Change` (M7.N3); the payload `FusionDropDownList.ChangeEventArgs` (`Value`,
  `IsInteracted`), and the focus and blur payloads, which EJ2 types as `Object`, declaring no member (their class name a
  gate 2 gap); and the render helpers `field.FusionDropDownList()`, `field.FusionDropDownList(build => ..)` and
  `field.FusionDropDownList(items, build => ..)`, which hands EJ2 the items by M21.D1, with `Fields<TItem>(text, value)`
  and `Fields<TItem>(text, value, groupBy)` (M21.X507). It replaces main's eight files
  (`Alis.Reactive.Fusion/Components/FusionDropDownList/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-dropdowns/src/drop-down-list/drop-down-list.d.ts:301`, `:308`, `:365`, `:389`, `:395`, `:685`,
  `:694`, `:700`, `:706`, `:742`, `@syncfusion/ej2-dropdowns/src/drop-down-base/drop-down-base.d.ts:390`).
- `M21.N21` kept: `FusionDropDownTree`: `Value` as `Property<string[]?>` and `Text` as `Property<string?>` (M21.X508),
  `DataBind`, `ShowPopup`, `HidePopup`, `Clear` and `Change`, with `IInputComponent.Input` committed by `Change`
  (M7.N3); the payload `FusionDropDownTree.DdtChangeEventArgs` (`Value`, `OldValue`, `IsInteracted`); and the render
  helpers `field.FusionDropDownTree()` and `field.FusionDropDownTree(build => ..)`. It replaces main's six files
  (`Alis.Reactive.Fusion/Components/FusionDropDownTree/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-dropdowns/src/drop-down-tree/drop-down-tree.d.ts:127`, `:131`, `:135`, `:627`, `:646`, `:695`,
  `:931`, `:964`, `:978`).
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
- `M21.N24` kept: `FusionInPlaceEditor`: `Value` (`value`), a `Property<string?>` read as text by M21.X404's default,
  the member's typed read being `pipeline.Input(m => m.Member)` (N14.4); `Disabled` and `EnableEditMode`, a
  `Property<bool>` each; `Save`; `Element.ClassList.Add` and `Remove`; `BeginEdit`, `EndEdit`, `Change`,
  `ActionBegin`, `ActionSuccess`, `SubmitClick` and `CancelClick` and their payloads; `IInputComponent.Input`
  committed by `EndEdit` (M7.N3); and the render helpers `field.FusionInPlaceEditor()` and
  `field.FusionInPlaceEditor(build => ..)`, which start the editor at the member's value by M21.D1. It replaces main's
  twelve files (`Alis.Reactive.Fusion/Components/FusionInPlaceEditor/` at 623dd4a27), whose doors carry their verdicts
  in `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-inplace-editor/src/inplace-editor/base/inplace-editor.d.ts:187`, `:320`, `:336`, `:408`, `:415`,
  `:435`, `:441`, `:449`, `:456`, `:463`, `:587`).
- `M21.N25` kept: `FusionMaskedTextBox`, Fusion and EJ2's class name
  (`@syncfusion/ej2-inputs/src/maskedtextbox/maskedtextbox/maskedtextbox.d.ts:16`), which both lines' `FusionInputMask`
  is renamed to (the owner, 2026-11-09): `Value` as `Property<string?>` (M21.X609), `FocusIn` and `Change`, with
  `IInputComponent.Input` committed by `Change` (M7.N3); the payload `FusionMaskedTextBox.MaskChangeEventArgs` (`Value`,
  `IsInteracted`); and the render helpers `field.FusionMaskedTextBox()` and `field.FusionMaskedTextBox(build => ..)`. It
  replaces main's six files (`Alis.Reactive.Fusion/Components/FusionInputMask/` at 623dd4a27), whose doors carry their
  verdicts in `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-inputs/src/maskedtextbox/maskedtextbox/maskedtextbox.d.ts:144`, `:198`, `:284`, `:308`, `:315`).
- `M21.N26` kept: `FusionKanban`: `Cards<TCard>()` (`dataSource`), `GetColumnData<TCard>`, `GetSwimlaneData<TCard>`,
  `DataBind`, `AddCard`, `UpdateCard`, `DeleteCard` (a card, a list or a key, M21.X302), `AddColumn`, `DeleteColumn`,
  `ShowColumn`, `HideColumn`, `ShowSpinner`, `HideSpinner`, `OpenDialog` with `FusionKanbanDialogAction` (M21.X301)
  and `CloseDialog`; fourteen events and their payloads, each cancel a `Property<bool>` and `DataSourceChanged`'s
  `EndEdit` and `CancelEdit` its methods; `FusionKanbanColumn` and `FusionKanbanHeaderArgs`, each member named by
  `[JsonPropertyName]` (M21.X303); and the render helper `Html.FusionKanban(plan, id, build => ..)`. It replaces
  main's seven files (`Alis.Reactive.Fusion/Components/FusionKanban/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-kanban/src/kanban/base/kanban.d.ts:112`, `:234`, `:429`, `:468`, `:484`, `:502`, `:511`, `:527`).
- `M21.N27` kept: `FusionListBox`: `Value`, a list typed by the items' value member as M21.N1's items overload types
  MultiSelect's (gate 2), `DataBind`, `SelectItems`, `SelectAll` and `EnableItems`, each taking every argument EJ2
  takes, and `Change`; the payload `FusionListBox.ListBoxChangeEventArgs` (`Value`); and the render helpers
  `Html.FusionListBox(plan, id, build => ..)` and `Html.FusionListBox(plan, id, items, build => ..)`, which hands EJ2
  the items by M21.D1, a door neither line has. It replaces main's seven files
  (`Alis.Reactive.Fusion/Components/FusionListBox/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-dropdowns/src/list-box/list-box.d.ts:156`, `:274`, `:409`, `:418`, `:425`, `:724`).
- `M21.N28` kept: `FusionListView`: `SelectItem<TItem>` and `UnselectItem<TItem>`, each taking the item as the list
  holds it (M21.X509), `CheckAllItems`, `UncheckAllItems` and `Select`; the payload `FusionListView.SelectEventArgs`
  (`Text`, `Index`, `IsInteracted`, `IsChecked`, `Cancel`); and the render helper
  `Html.FusionListView(plan, id, build => ..)`. It replaces main's seven files
  (`Alis.Reactive.Fusion/Components/FusionListView/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-lists/src/list-view/list-view.d.ts:392`, `:462`, `:466`, `:532`, `:538`, `:706`, `:818`, `:826`,
  `:830`, `:834`).
- `M21.N29` kept: `FusionMention`: `Search`, `ShowPopup`, `HidePopup`, `Change`, `Opened` and `Closed`; the payloads
  `FusionMention.MentionChangeEventArgs` (`Value`, typed by the items' value member as M21.N1's items overload types
  MultiSelect's (gate 2), `IsInteracted`) and `FusionMention.PopupEventArgs`; and the render helpers
  `Html.FusionMention(plan, id, build => ..)` and `Html.FusionMention(plan, id, items, build => ..)`, which hands EJ2
  the items by M21.D1, with `Fields<TItem>(text, value)` (M21.X510), the Mention's instance handed to the view's id by
  the framework's own component code (`Alis.Reactive.Assets/components/fusion/mention.ts:7-11` at 65fdf118d). It
  replaces main's eight files (`Alis.Reactive.Fusion/Components/FusionMention/` at 623dd4a27), whose doors carry their
  verdicts in `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-dropdowns/src/mention/mention.d.ts:15`, `:282`, `:294`, `:300`, `:407`, `:413`, `:469`).
- `M21.N30` kept: `FusionMenu`: `Open` and `Close`; `BeforeItemRender`, `BeforeOpen`, `OnOpen`, `BeforeClose`,
  `OnClose` and `Select` and their payloads, `FusionMenuBeforeOpenArgs` with `IsFocused` (optional, `isFocused`); and
  the render helper `Html.FusionMenu(plan, id, build => ..)`. It replaces main's twelve files
  (`Alis.Reactive.Fusion/Components/FusionMenu/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-navigations/src/menu/menu.d.ts:119`, `:126`;
  `@syncfusion/ej2-navigations/src/common/menu-base.d.ts:207-242`, `:552`).
- `M21.N31` kept: `FusionMultiColumnComboBox`: `Value` and `Text` as `Property<string?>` (M21.X511),
  `DataSource<TRow>()`, `DataBind`, `FocusIn`, `FocusOut`, `ShowPopup`, `HidePopup` and `Change`, with
  `IInputComponent.Input` committed by `Change` (M7.N3); the payload `FusionMultiColumnComboBox.ChangeEventArgs`
  (`Value`, typed by the rows' value member as M21.N1's items overload types MultiSelect's (gate 2), `IsInteracted`);
  and the render helpers `field.FusionMultiColumnComboBox()`, `field.FusionMultiColumnComboBox(build => ..)` and
  `field.FusionMultiColumnComboBox(rows, build => ..)`, which hands EJ2 the rows by M21.D1, with
  `Fields<TItem>(text, value)` and `Fields<TItem>(text, value, groupBy)` (M21.X512). It replaces main's six files
  (`Alis.Reactive.Fusion/Components/FusionMultiColumnComboBox/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-multicolumn-combobox/src/multicolumn-combobox/multi-column-combo-box.d.ts:361`, `:389`, `:437`,
  `:443`, `:451`, `:757`, `:898`, `:905`, `:913`, `:920`).
- `M21.N32` kept: `FusionNumericTextBox`: `Value` and `Min` as `Property<decimal?>` (M21.X607), `FocusIn`, `FocusOut`,
  `Increment`, `Decrement`, `Change`, `Focus` and `Blur`, with `IInputComponent.Input` committed by `Change` (M7.N3);
  the payloads `FusionNumericTextBox.ChangeEventArgs` (`Value` and `PreviousValue` as `ReadOnly<decimal?>`, M21.X608,
  and `IsInteracted`), `FusionNumericTextBox.NumericFocusEventArgs` and `FusionNumericTextBox.NumericBlurEventArgs`, of
  which the declaration reads no member; and the render helper `field.FusionNumericTextBox(build => ..)`. It replaces
  main's eight files (`Alis.Reactive.Fusion/Components/FusionNumericTextBox/` at 623dd4a27), whose doors carry their
  verdicts in `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-inputs/src/numerictextbox/numerictextbox.d.ts:71`, `:82`, `:272`, `:278`, `:284`, `:373`, `:381`,
  `:401`, `:407`, `:440`, `:445`, `:454`).
- `M21.N33` kept: `FusionOtpInput`: `Value` as `Property<string>`, the text the browser holds
  (`probe:ej2-otpinput-value-after-typing`), `DataBind`, `FocusIn`, `FocusOut`, `Input`, `ValueChanged`, `Focus` and
  `Blur`, with `IInputComponent.Input` committed by `ValueChanged` (M7.N3); the payloads
  `FusionOtpInput.OtpInputEventArgs` (`Value`, `PreviousValue`, `Index`), `FusionOtpInput.OtpChangedEventArgs` (`Value`,
  `PreviousValue`, `IsInteracted`) and `FusionOtpInput.OtpFocusEventArgs` of `Focus` and `Blur` (`Value`, `Index`,
  `IsInteracted`), each value read as the text the browser hands where EJ2's TypeScript declares `string | number`; and
  the render helpers `field.FusionOtpInput()` and `field.FusionOtpInput(build => ..)` over a member of text, which start
  at the member's value (M21.X612). It replaces main's nine files (`Alis.Reactive.Fusion/Components/FusionOtpInput/` at
  623dd4a27), whose doors carry their verdicts in `spec/FusionComponents/features.json`. Each JavaScript name is EJ2
  33.1.47's public member (`@syncfusion/ej2-inputs/src/otp-input/otp-input.d.ts:163`, `:266`, `:272`, `:278`, `:284`,
  `:349`, `:355`, `:72`, `:76`, `:80`, `:97`, `:101`, `:105`, `:122`, `:126`, `:130`,
  `@syncfusion/ej2-base/src/component.d.ts:129`).
- `M21.N34` kept: `FusionPivotView`: `CurrentView` (`currentView`, which EJ2 marks `@hidden`, a gate 2 item),
  `DataSourceSettings` as `LiveObject<FusionPivotViewDataSourceSettings>()` with `Rows<TRow>()` (`dataSource`),
  `DisplayOption` as `LiveObject<FusionPivotViewDisplayOption>()` with `View` and `Primary` (M21.D301), `Refresh`,
  `GetPersistData`, `LoadPersistData`, `ShowConditionalFormattingDialog` and `CreateCalculatedFieldDialog`; `DataBound`
  and `CellSelecting`, whose payload reads its cell as `FusionPivotViewCellData`, each member named by
  `[JsonPropertyName]` (M21.X304); and the render helper `Html.FusionPivotView(plan, id, build => ..)`. It replaces
  main's eight files (`Alis.Reactive.Fusion/Components/FusionPivotView/` at 623dd4a27), whose doors carry their verdicts
  in `spec/FusionComponents/features.json`. Each JavaScript name but `currentView` is EJ2 33.1.47's public member
  (`@syncfusion/ej2-pivotview/src/pivotview/base/pivotview.d.ts:1447`, `:1476`, `:2244`, `:2460`, `:2467`, `:2816`).
- `M21.N37` kept: `FusionRating`: `Value` as `Property<double>`, since EJ2's value starts at 0, `DataBind`, `Reset` and
  `ValueChanged`, with `IInputComponent.Input` committed by `ValueChanged` (M7.N3); the payload
  `FusionRating.RatingChangedEventArgs` (`Value`, `PreviousValue`, `IsInteracted`); and the render helpers
  `field.FusionRating()` and `field.FusionRating(build => ..)` over a member of `double`, which start at the member's
  value (M21.X613). It replaces main's six files (`Alis.Reactive.Fusion/Components/FusionRating/` at 623dd4a27), whose
  doors carry their verdicts in `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public
  member (`@syncfusion/ej2-inputs/src/rating/rating.d.ts:281`, `:318`, `:381`, `:57`, `:61`, `:65`,
  `@syncfusion/ej2-base/src/component.d.ts:129`).
- `M21.N38` kept: `FusionRichTextEditor`: `Value` as `Property<string?>` (M21.X610), `FocusIn` and `Change`, with
  `IInputComponent.Input` committed by `Change` (M7.N3); the payload `FusionRichTextEditor.ChangeEventArgs` (`Value`,
  `IsInteracted`); and the render helpers `field.FusionRichTextEditor()` and `field.FusionRichTextEditor(build => ..)`.
  It replaces main's six files (`Alis.Reactive.Fusion/Components/FusionRichTextEditor/` at 623dd4a27), whose doors carry
  their verdicts in `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-richtexteditor/src/rich-text-editor/base/rich-text-editor.d.ts:750`, `:1249`, `:1432`,
  `@syncfusion/ej2-richtexteditor/src/rich-text-editor/base/interface.d.ts:557`, `:561`).
- `M21.N39` kept: `FusionSchedule`: `CurrentView`, `SelectedDate`, `EventSettings` as
  `LiveObject<FusionScheduleEventSettings>()` with `Events<TEvent>()` (`dataSource`), `DataBind`, `GetEvents<TEvent>`,
  `AddEvent`, `SaveEvent`, `DeleteEvent<TKey>`, `OpenEditor`, `CloseEditor`, `RefreshEvents`, `Print` and `ScrollTo`;
  `ScheduleAction`, with System.Text.Json's string enum converter (M21.X305); nine events and their payloads, an
  event's data the app's own class `TEvent` and `Navigating`'s dates read as `DateTime` (M21.X306); and the render
  helper `Html.FusionSchedule(plan, id, build => ..)`. It replaces main's fifteen files
  (`Alis.Reactive.Fusion/Components/FusionSchedule/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-schedule/src/schedule/base/schedule.d.ts:250`, `:274`, `:707`, `:867`, `:1716`, `:1726`, `:1878`).
- `M21.N41` kept: `FusionSlider`: `Value` as `Property<double?>` for a slider of one handle and `RangeValue` as
  `Property<double[]?>` for a range, each reading `value` (M21.X614), the second by a `name:` since the compiler refuses
  two members named `Value`; `DataBind`, `Change` and `Changed`, with `IInputComponent.Input` committed by `Changed`
  (M7.N3); the payload `FusionSlider.SliderChangeEventArgs` (`Value` and `PreviousValue` as numbers, `RangeValue` and
  `PreviousRangeValue` as lists, M21.X616, `Text`, `Action`, `IsInteracted`); and the render helpers
  `field.FusionSlider()` and `field.FusionSlider(build => ..)`, which start at the member's value (M21.X615). It
  replaces main's six files (`Alis.Reactive.Fusion/Components/FusionSlider/` at 623dd4a27), whose doors carry their
  verdicts in `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-inputs/src/slider/slider.d.ts:366`, `:514`, `:521`, `:70`, `:76`, `:80`, `:84`, `:88`,
  `@syncfusion/ej2-base/src/component.d.ts:129`).
- `M21.N43` kept: `FusionSmartTextArea`: `Value` as `Property<string?>` (M21.X604), `FocusIn`, `Change`,
  `BeforeSuggestionInsert` and `AfterSuggestionInsert`, with `IInputComponent.Input` committed by `Change` (M7.N3); the
  payloads `FusionSmartTextArea.ChangedEventArgs` (`Value`, `PreviousValue`, `IsInteracted`),
  `FusionSmartTextArea.BeforeSuggestionInsertArgs` (`Text`, `Cancel`) and
  `FusionSmartTextArea.AfterSuggestionInsertArgs` (`InsertedText`, by M21.D601); `FusionSmartTextAreaOptions` and
  `FusionSmartSuggestionMode` (EJ2's `SuggestionMode`, its names gate 2's); and the render helpers
  `field.FusionSmartTextArea()` and `field.FusionSmartTextArea(configure => ..)`, which start at the member's value
  (M21.X605), the component built by the framework's own component code
  (`Alis.Reactive.Assets/components/fusion/smart-text-area.ts:26-30` at 65fdf118d). It replaces main's eight files
  (`Alis.Reactive.Fusion/Components/FusionSmartTextArea/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-inputs/src/smart-textarea/smart-textarea.d.ts:39`, `:43`, `:65`, `:71`, `:78`, `:85`, `:101`,
  `@syncfusion/ej2-inputs/src/textarea/textarea.d.ts:39`, `:186`, `:281`,
  `@syncfusion/ej2-inputs/src/textbox/textbox.d.ts:11`, `:23`, `:27`), and `insertedText` the member its JavaScript
  hands (`@syncfusion/ej2-inputs/src/smart-textarea/smart-textarea.js:420`, `:508`).
- `M21.N48` kept: `FusionTextArea`: `Value` as `Property<string?>` (M21.X603), `DataBind`, `FocusIn`, `FocusOut`,
  `Input`, `Change`, `Focus` and `Blur`, with `IInputComponent.Input` committed by `Change` (M7.N3); the payloads
  `FusionTextArea.InputEventArgs` (`Value`, `PreviousValue`), `FusionTextArea.ChangedEventArgs` (`Value`,
  `PreviousValue`, `IsInteracted`), `FusionTextArea.FocusInEventArgs` and `FusionTextArea.FocusOutEventArgs` (`Value`);
  and the render helpers `field.FusionTextArea()` and `field.FusionTextArea(build => ..)`. It replaces main's nine files
  (`Alis.Reactive.Fusion/Components/FusionTextArea/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-inputs/src/textarea/textarea.d.ts:39`, `:186`, `:192`, `:198`, `:204`, `:281`, `:287`,
  `@syncfusion/ej2-inputs/src/textbox/textbox.d.ts:11`, `:19`, `:23`, `:27`, `:31`,
  `@syncfusion/ej2-base/src/component.d.ts:129`).
- `M21.N49` kept: `FusionTextBox`: `Value` as `Property<string?>` (M21.X601), `DataBind`, `FocusIn`, `FocusOut`,
  `AddIcon` with its place typed (M21.X602), `Input`, `Change`, `Focus` and `Blur`, with `IInputComponent.Input`
  committed by `Change` (M7.N3); the payloads `FusionTextBox.InputEventArgs` (`Value`, `PreviousValue`),
  `FusionTextBox.ChangedEventArgs` (`Value`, `PreviousValue`, `IsInteracted`), `FusionTextBox.FocusInEventArgs` and
  `FusionTextBox.FocusOutEventArgs` (`Value`); and the render helpers `field.FusionTextBox()` and
  `field.FusionTextBox(build => ..)`. It replaces main's nine files (`Alis.Reactive.Fusion/Components/FusionTextBox/` at
  623dd4a27), whose doors carry their verdicts in `spec/FusionComponents/features.json`. Each JavaScript name is EJ2
  33.1.47's public member (`@syncfusion/ej2-inputs/src/textbox/textbox.d.ts:81`, `:195`, `:201`, `:207`, `:213`, `:286`,
  `:314`, `:320`, `:11`, `:19`, `:23`, `:27`, `:31`, `@syncfusion/ej2-base/src/component.d.ts:129`).
- `M21.N51` kept: `FusionToolbar`: `Disable` and `Clicked`, whose payload `FusionToolbarClickedArgs` reads its `item`;
  and the render helper `Html.FusionToolbar(plan, id, build => ..)`. It replaces main's seven files
  (`Alis.Reactive.Fusion/Components/FusionToolbar/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-navigations/src/toolbar/toolbar.d.ts:344`, `:414`).

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
- A multiple selection's indexes read as EJ2 writes them (M21.X401). Main names them `indexes` and the redesign drops
  them, while `getSelectedChips` writes `Indexes` (`@syncfusion/ej2-buttons/src/chips/chip-list.d.ts:56`); the rewrite
  declares `[JsonPropertyName("Indexes")]`.
- An in-place editor calls only public members (M21.X402). Both lines call EJ2's private `disable` and `setFocus`
  (`@syncfusion/ej2-inplace-editor/src/inplace-editor/base/inplace-editor.d.ts:541`, `:525`); the rewrite declares the
  public `disabled` and `enableEditMode`. Its starting value is read with no compile (M21.X403), and its value reads
  typed by its member through `pipeline.Input` (M21.X404).
- A context menu's and a menu's `beforeOpen` payload keeps `isFocused`, which main reads and the redesign dropped
  (`@syncfusion/ej2-navigations/src/common/menu-base.d.ts:552`), declared optional as EJ2 marks it.
- A box's value and text hold null as the vendor's do (M21.X501, M21.X504, M21.X506, M21.X508, M21.X511). The redesign
  declares the value and text of AutoComplete, ComboBox, DropDownList, DropDownTree and MultiColumnComboBox non-null
  (`Alis.Reactive.Fusion/Components/FusionDropDownList/FusionDropDownList.cs:24` at 65fdf118d) where EJ2 documents each
  null until a choice (`@syncfusion/ej2-dropdowns/src/drop-down-list/drop-down-list.d.ts:305-308`); ALIS019 refuses the
  declarations, and a value that admits null keeps main's clears.
- AutoComplete names no private member (M21.X502). Both lines write `text`, which EJ2 marks `@private` on AutoComplete
  (`@syncfusion/ej2-dropdowns/src/auto-complete/auto-complete.d.ts:197-200`); the designed default, gate 2's, sets what
  the box shows through `Value`.
- One naming for every list's data (M21.X503, M21.X505, M21.X507, M21.X510, M21.X512). AutoComplete, ComboBox,
  DropDownList, Mention and MultiColumnComboBox each name a field by one rule and their data by another, on one line or
  both (`Alis.Reactive.Fusion/Components/FusionComboBox/FusionComboBox.cs:101` at 65fdf118d); an items overload of each
  render helper writes the data by the plan's writer (M21.D1), so a field named `label` finds `label`.
- A list item is selected by the item the list holds (M21.X509). Both lines select a ListView item by its text, which
  EJ2 finds only in a list of text (`@syncfusion/ej2-lists/src/list-view/list-view.js:1552-1572`), so a list of objects
  selects nothing; the rewrite takes the item.
- A Mention renders with no script in the view. Main's render helper writes a script that waits for the page and falls
  back to the target's instance (`Alis.Reactive.Fusion/Components/FusionMention/FusionMentionHtmlExtensions.cs:73-80` at
  623dd4a27); the redesign hands the instance over in the framework's own component code
  (`Alis.Reactive.Assets/components/fusion/mention.ts:7-11` at 65fdf118d), which the rewrite keeps.
- A ListBox method takes every argument EJ2 takes. Main gives `selectItems`, `selectAll` and `enableItems` six C# names
  with the state and `isValue` fixed (`Alis.Reactive.Fusion/Components/FusionListBox/FusionListBoxExtensions.cs:19-35`
  at 623dd4a27); the redesign takes each as an `Arg`, which the rewrite keeps.
- A field's value holds null as the vendor's does (M21.X601, M21.X603, M21.X604, M21.X607, M21.X609, M21.X610,
  M21.X614). The redesign declares the value of TextBox, TextArea, SmartTextArea, NumericTextBox, MaskedTextBox,
  RichTextEditor and Slider non-null (`Alis.Reactive.Fusion/Components/FusionTextBox/FusionTextBox.cs:19` at 65fdf118d)
  where EJ2 documents each null until a value is set (`@syncfusion/ej2-inputs/src/textbox/textbox.d.ts:76-81`); ALIS019
  refuses the declarations, and a value that admits null keeps main's clears.
- A cleared number reads as nothing (M21.X608). Both lines read NumericTextBox's change value as a number
  (`Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBox.cs:56` at 65fdf118d), while the browser
  hands null for a cleared box and for the change after it (`probe:ej2-payload-numerictextbox-change-cleared`,
  `probe:ej2-payload-numerictextbox-change-after-cleared`); the rewrite reads `decimal?`, so a view can check a cleared
  dose.
- A component starts at the member's value (M21.X605, M21.X612, M21.X613, M21.X615). Both lines render Slider, Rating
  and OtpInput by the vendor's builder that binds no member
  (`Alis.Reactive.Fusion/Components/FusionSlider/FusionSlider.cs:83` at 65fdf118d) and SmartTextArea at its options'
  value (`Alis.Reactive.Fusion/Components/FusionSmartTextArea/FusionSmartTextArea.cs:144` at 65fdf118d), so each starts
  empty or where the vendor starts one, whatever the model holds, and the redesign's sandbox repeats the value in the
  build callback (`Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Slider/Index.cshtml:30` at 65fdf118d);
  the rewrite starts each at the member's value by M21.D1.
- A range slider reads its handles (M21.X616). Both lines read a slider's change value as one number
  (`Alis.Reactive.Fusion/Components/FusionSlider/FusionSlider.cs:43` at 65fdf118d), while a range slider hands a list
  (`probe:ej2-payload-slider-rest-change`); the rewrite's payload reads `RangeValue` and `PreviousRangeValue` as lists,
  as the component's `RangeValue` reads its value.
- An icon's place is typed (M21.X602). The redesign takes `AddIcon`'s place as text
  (`Alis.Reactive.Fusion/Components/FusionTextBox/FusionTextBox.cs:33` at 65fdf118d), and EJ2 prepends the icon for any
  text but `append` (`@syncfusion/ej2-inputs/src/input/input.js:997-1007`); the rewrite takes one of the two places EJ2
  documents (`@syncfusion/ej2-inputs/src/textbox/textbox.d.ts:281`).
- A payload is read by the names the browser hands (M21.D601). EJ2's TypeScript names SmartTextArea's inserted
  suggestion `text` (`@syncfusion/ej2-inputs/src/smart-textarea/smart-textarea.d.ts:48-53`), and its JavaScript hands
  `insertedText` (`@syncfusion/ej2-inputs/src/smart-textarea/smart-textarea.js:420`,
  `probe:ej2-payload-smarttextarea-afterSuggestionInsert`); both lines read `insertedText`, and the rewrite keeps it,
  pinned by the browser fact against the vendor contract's refusal.
- A SmartTextArea renders with no script in the view. Main's render helper writes a script that builds the component
  when the page loads (`Alis.Reactive.Fusion/Components/FusionSmartTextArea/FusionSmartTextAreaHtmlExtensions.cs:82-89`
  at 623dd4a27); the redesign builds it in the framework's own component code
  (`Alis.Reactive.Assets/components/fusion/smart-text-area.ts:26-30` at 65fdf118d), which the rewrite keeps.
- A color is never written as null (M21.X611). Main's `SetValue` takes null
  (`Alis.Reactive.Fusion/Components/FusionColorPicker/FusionColorPickerExtensions.cs:29` at 623dd4a27) where EJ2's value
  is a color that starts at `#008000ff` (`@syncfusion/ej2-inputs/src/color-picker/color-picker.d.ts:41-46`); the
  rewrite's `Value` is `Property<string>`, as the redesign's.
