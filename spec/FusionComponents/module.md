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

- `M21.N1` kept: `FusionMultiSelect<TValue>`, the reference, TValue the type of the member it binds: `Value`,
  `DataSource<TItem>()`, `DataBind`, `ShowPopup`, `HidePopup`, `Change` and `Filtering`, with `IInputComponent.Input`
  committed by `Change` (M7.N3); the payloads `FusionMultiSelect<TValue>.MultiSelectChangeEventArgs` (`Value`,
  `IsInteracted`) and `FusionMultiSelect<TValue>.FilteringEventArgs` (`Text`, `PreventDefaultAction`, `UpdateData`); and
  the render helpers `field.FusionMultiSelect()`, `field.FusionMultiSelect(build => ..)` and
  `field.FusionMultiSelect(items, build => ..)`, which render it with the bound member's type and hand EJ2 the items by
  M21.D1, with `Fields<TItem>(text, value)` and `Fields<TItem>(text, value, groupBy)`. It replaces main's seven files,
  whose doors carry their verdicts in `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's
  public member (`@syncfusion/ej2-dropdowns/src/multi-select/multi-select.d.ts:170`, `:416`, `:528`, `:615`, `:945`,
  `:952`). `Value` is a `Property<TValue>` whose type argument admits null, as the vendor's value holds null until a
  choice (M21.X1).
- `M21.N2` kept: the package. `FusionComponent`, the base of every Fusion declaration, carries `FusionVendor`:
  `ComponentVendor.Define("fusion", objectPath: "ej2_instances.0", subscribe: "addEventListener", unsubscribe:
  "removeEventListener")`, with no payload kind, since each event declares its payload's place (N17.1; module 12). It
  replaces main's string vendor (`Alis.Reactive.Fusion/FusionComponent.cs:14` at 623dd4a27). Every Fusion render helper
  writes the C# values it hands EJ2 by M21.D1, so `AddReactiveFusion` leaves (gate 2). Every Fusion declaration marks
  `optional: true` on each member EJ2's TypeScript marks `?` (D5.3, N5.4), which ALIS022 checks both ways; neither line
  has the mark. Each slice's declaration names each member as EJ2 names it, in Pascal case, so M11.D1 gives its
  JavaScript name, with `name:` only where C# refuses the name (CS0111, two members of one name and one parameter list);
  nests each payload, and each object a member holds or a method returns, in its own component under EJ2's type name,
  each data member of it named by `[JsonPropertyName]` with EJ2's name (X17.48); and types a value of the app's own data
  that EJ2 types as a union by a type parameter, the bound member's for an input, which the vendor contract analyzer
  checks at each type argument against one type of the union (ALIS037, gate 2).
- `M21.N3` kept: `FusionAIAssistView`: `Prompt` as `Property<string>`, `ActiveView` as `Property<int>`,
  `ExecutePrompt(prompt)`, `AddPromptResponse(outputResponse)`, the whole response (M21.D901), and
  `AddPromptResponse(Arg<FusionAIAssistView.PromptModel>)`, a prompt with its response, `ScrollToBottom`,
  `PromptRequest`, `PromptChanged` and `StopRespondingClick`; the payloads `FusionAIAssistView.PromptRequestEventArgs`
  (`Prompt`, `PromptSuggestions`, no `Cancel`, M21.X901), `FusionAIAssistView.PromptChangedEventArgs` (`Value`,
  `PreviousValue`) and `FusionAIAssistView.StopRespondingEventArgs` (`Prompt`, `DataIndex`), and
  `FusionAIAssistView.PromptModel` (`Prompt`, `Response`); and the render helper
  `Html.FusionAIAssistView(plan, elementId, build => ..)`, which writes `enableStreaming` false after `build` and
  returns the rendered component in place of main's `FusionAIAssistViewBuilder<TModel>`. It replaces main's nine files
  (`Alis.Reactive.Fusion/Components/FusionAIAssistView/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-interactive-chat/src/ai-assistview/ai-assistview.d.ts:480`, `:538`, `:940`, `:950`, `:959`, `:728`,
  `:734`, `:741`, `@syncfusion/ej2-interactive-chat/src/ai-assistview/ai-assistview-model.d.ts:8`).
- `M21.N4` kept: `FusionAccordion`: `ExpandItem(isExpand, index)`, `EnableItem(index, isEnable)` and `Expanded`; the
  payload `FusionAccordion.ExpandedEventArgs` (`Index`, `IsExpanded`); and the render helper
  `Html.FusionAccordion(plan, elementId, build => ..)`, which returns the rendered component in place of main's
  `FusionAccordionBuilder<TModel>`. It replaces main's seven files (`Alis.Reactive.Fusion/Components/FusionAccordion/`
  at 623dd4a27), whose doors carry their verdicts in `spec/FusionComponents/features.json`. Each JavaScript name is EJ2
  33.1.47's public member (`@syncfusion/ej2-navigations/src/accordion/accordion.d.ts:441`, `:432`, `:311`, `:45`).
- `M21.N5` kept: `FusionAutoComplete<TValue>`, TValue the type of the member it binds, as EJ2 marks the value
  AutoComplete inherits from ComboBox a generic type (`@syncfusion/ej2-dropdowns/src/combo-box/combo-box.d.ts:228`):
  `Value`, a `Property<TValue>` whose type argument admits null (M21.X501), `DataSource<TItem>()`, `Enabled`,
  `DataBind`, `FocusIn`, `FocusOut`, `ShowPopup`, `HidePopup`, `Clear`, `Change` and `Filtering`, with
  `IInputComponent.Input` committed by `Change` (M7.N3); the payloads `FusionAutoComplete<TValue>.ChangeEventArgs`
  (`Value`, `IsInteracted`) and `FusionAutoComplete<TValue>.FilteringEventArgs` (`Text`, `PreventDefaultAction`,
  `UpdateData`); and the render helpers `field.FusionAutoComplete(build => ..)` and `field.FusionAutoComplete(items,
  build => ..)`, which render it with the bound member's type and hand EJ2 the items by M21.D1, with
  `Fields<TItem>(text, value)` and `Fields<TItem>(text, value, groupBy)` (M21.X503). `Text` leaves: EJ2 marks
  AutoComplete's `text` `@private` (M21.X502, gate 2). It replaces main's seven files
  (`Alis.Reactive.Fusion/Components/FusionAutoComplete/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-dropdowns/src/combo-box/combo-box.d.ts:231`, `:373`, `:380`, `:387`,
  `@syncfusion/ej2-dropdowns/src/auto-complete/auto-complete.d.ts:140`, `:256`, `:263`,
  `@syncfusion/ej2-dropdowns/src/drop-down-list/drop-down-list.d.ts:157`, `:365`,
  `@syncfusion/ej2-dropdowns/src/drop-down-base/drop-down-base.d.ts:390`, `:826`, `:838`, `:847`).
- `M21.N6` kept: `FusionBreadcrumb`: `ActiveItem` (`activeItem`), `DataBind` and `ItemClick`, whose payload
  `FusionBreadcrumb.BreadcrumbClickEventArgs` reads its `item`, a `FusionBreadcrumb.BreadcrumbItemModel`; and the render
  helper `Html.FusionBreadcrumb(plan, id, build => ..)`. It replaces main's seven files
  (`Alis.Reactive.Fusion/Components/FusionBreadcrumb/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-navigations/src/breadcrumb/breadcrumb.d.ts:144`, `:223`).
- `M21.N7` kept: `FusionBulletChart`: `DataSource<TItem>()`, `TooltipRender` and `BulletChartMouseClick`, and no
  `GetActualIndex` (M21.X902); the payloads `FusionBulletChart.IBulletchartTooltipEventArgs` (`Value` and `Target` as
  the formatted text the browser hands, `Name`, `Text` and `Template` as `Property<string>`, M21.X903) and
  `FusionBulletChart.IBulletMouseEventArgs` (`Target`, `X`, `Y`); and the render helpers
  `Html.FusionBulletChart(plan, elementId, build => ..)` and
  `Html.FusionBulletChart(plan, elementId, items, build => ..)`, which hands EJ2 the items by M21.D1, with
  `Fields<TItem>(value, target)` and `Fields<TItem>(value, target, category)`, each returning the rendered component in
  place of main's `FusionBulletChartBuilder<TModel>`. It replaces main's eight files
  (`Alis.Reactive.Fusion/Components/FusionBulletChart/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-charts/src/bullet-chart/bullet-chart.d.ts:236`, `:260`, `:266`, `:248`, `:284`, `:302`,
  `@syncfusion/ej2-charts/src/bullet-chart/model/bullet-interface.d.ts:132`, `:169`).
- `M21.N8` kept: `FusionButton`: `Content`, `IconCss` and `CssClass` as `Property<string>`, `Disabled`, `IsPrimary` and
  `IsToggle` as `Property<bool>`, `IconPosition` as `Property<FusionButtonIconPosition>`, `DataBind`, `Click` and
  `FocusIn`; the enum `FusionButtonIconPosition`, EJ2's `IconPosition` under the redesign's name beside the class, as a
  nested type would share the property's name (CS0102), written by its names through a string enum converter (M21.X801);
  and the render helper `Html.FusionButton(plan, elementId, build => ..)`, which returns the rendered component in place
  of main's `FusionButtonBuilder<TModel>`. It replaces main's five files
  (`Alis.Reactive.Fusion/Components/FusionButton/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-buttons/src/button/button.d.ts:86`, `:58`, `:79`, `:64`, `:70`, `:92`, `:51`, `:8`, `:176`, `:184`,
  `@syncfusion/ej2-base/src/component.d.ts:129`).
- `M21.N9` kept: `FusionCarousel`: `SelectedIndex` as `Property<int>`, `Next`, `Prev`, `SlideChanging` and
  `SlideChanged`; the payloads `FusionCarousel.SlideChangingEventArgs` (`CurrentIndex`, `NextIndex`, `IsSwiped`,
  `SlideDirection`, `Cancel`) and `FusionCarousel.SlideChangedEventArgs` (`CurrentIndex`, `PreviousIndex`, `IsSwiped`,
  `SlideDirection`), the direction a `FusionCarousel.CarouselSlideDirection` written by its names through a string enum
  converter (M21.X904); and the render helper `Html.FusionCarousel(plan, elementId, build => ..)`, which returns the
  rendered component in place of main's `FusionCarouselBuilder<TModel>`. It replaces main's eight files
  (`Alis.Reactive.Fusion/Components/FusionCarousel/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-navigations/src/carousel/carousel.d.ts:215`, `:405`, `:399`, `:330`, `:336`, `:12`).
- `M21.N10` kept: `FusionCheckBox`: `Checked`, `Indeterminate` and `Disabled` as `Property<bool>`, `DataBind`, `Click`,
  `FocusIn` and `Change`, with `IInputComponent.Input` committed by `Change` (M7.N3); the payload
  `FusionCheckBox.ChangeEventArgs` (`Checked`, `probe:ej2-payload-checkbox-change`); and the render helpers
  `field.FusionCheckBox()` and `field.FusionCheckBox(build => ..)` over a member of `bool`. It replaces main's six files
  (`Alis.Reactive.Fusion/Components/FusionCheckBox/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-buttons/src/check-box/check-box.d.ts:52`, `:73`, `:66`, `:198`, `:206`, `:39`,
  `@syncfusion/ej2-buttons/src/common/common.d.ts:93`, `@syncfusion/ej2-base/src/component.d.ts:129`).
- `M21.N11` kept: `FusionChipList`: `Add`; `Select(index)`, `Select(indexes)` and `Select(values, selectionType)` with
  `FusionChipList.SelectionType`; `Remove(index)` and `Remove(indexes)`; `Find` returning a
  `FusionChipList.ChipDataArgs`; `GetSelectedChips<TData>()` and `GetSelectedChip<TData>()` (`getSelectedChips`,
  CS0111); `DataBind`; and `SelectedChips<TValue>()`, its kind the app's choice among those the browser holds; `Click`
  and `Deleted` and their payloads; `FusionChipList.SelectedItems<TData>` with `Texts`, `Data` and `Indexes`, the last
  by `[JsonPropertyName("Indexes")]`, and `FusionChipList.SelectedItem<TData>` with `Text`, `Index` and `Data`; and the
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
- `M21.N13` kept: `FusionComboBox<TValue>`, TValue the type of the member it binds, as EJ2 marks ComboBox's value a
  generic type (`@syncfusion/ej2-dropdowns/src/combo-box/combo-box.d.ts:228`): `Value`, a `Property<TValue>` whose type
  argument admits null, `Text` as `Property<string?>` (M21.X504), `Index` as `Property<int?>`, `DataBind`, `FocusIn`,
  `FocusOut`, `ShowPopup`, `HidePopup`, `Clear`, `Change`, `Focus` and `Blur`, with `IInputComponent.Input` committed by
  `Change` (M7.N3); the payload `FusionComboBox<TValue>.ChangeEventArgs` (`Value`, `IsInteracted`), and the focus and
  blur payloads, which EJ2 types as `Object`, declaring no member (their class name a gate 2 gap); and the render
  helpers `field.FusionComboBox()`, `field.FusionComboBox(build => ..)` and `field.FusionComboBox(items, build => ..)`,
  which render it with the bound member's type and hand EJ2 the items by M21.D1, a door neither line has, with
  `Fields<TItem>(text, value)` (M21.X505). It replaces main's eight files
  (`Alis.Reactive.Fusion/Components/FusionComboBox/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-dropdowns/src/combo-box/combo-box.d.ts:92`, `:223`, `:231`, `:360`, `:367`, `:373`, `:380`, `:387`,
  `@syncfusion/ej2-dropdowns/src/drop-down-list/drop-down-list.d.ts:365`, `:389`, `:395`).
- `M21.N14` kept: `FusionContextMenu`: `Open(top, left)` and `Close`; `BeforeItemRender`, `BeforeOpen`, `OnOpen`,
  `BeforeClose`, `OnClose` and `Select` and their payloads, `FusionContextMenu.BeforeOpenCloseMenuEventArgs` with
  `IsFocused` (optional, `isFocused`), which beforeClose holds (M21.X405); and the render helper
  `Html.FusionContextMenu(plan, id, build => ..)`. It replaces main's twelve files
  (`Alis.Reactive.Fusion/Components/FusionContextMenu/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-navigations/src/context-menu/context-menu.d.ts:77`, `:84`;
  `@syncfusion/ej2-navigations/src/common/menu-base.d.ts:207-242`, `:552`).
- `M21.N15` kept: `FusionDatePicker<TValue>`, TValue the type of the member it binds, as EJ2 marks the picker's value a
  generic type (`@syncfusion/ej2-calendars/src/datepicker/datepicker.d.ts:83-87`): `Value`, a `Property<TValue>` whose
  type argument admits null (M21.X701), `FocusIn`, `FocusOut` and `Change`, with `IInputComponent.Input` committed by
  `Change` (M7.N3); the payload `FusionDatePicker<TValue>.ChangedEventArgs` (`Value`, a `TValue`, and `IsInteracted`);
  and the render helpers `field.FusionDatePicker()` and `field.FusionDatePicker(build => ..)`, which render it with the
  bound member's type, a date ALIS037 holds (M21.X702). It replaces main's six files
  (`Alis.Reactive.Fusion/Components/FusionDatePicker/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-calendars/src/datepicker/datepicker.d.ts:87`, `:488`, `:494`,
  `@syncfusion/ej2-calendars/src/calendar/calendar.d.ts:551`, `:698`, `:710`).
- `M21.N16` kept: `FusionDateRangePicker`: `StartDate` and `EndDate` as `Property<DateTime?>` and `Value` as
  `Property<DateTime[]?>` (M21.X703, M21.D702), `FocusIn`, `FocusOut` and `Change`, with `IInputComponent.Input`
  committed by `Change` (M7.N3); the payload `FusionDateRangePicker.RangeEventArgs` (`StartDate`, `EndDate`, `DaySpan`,
  `IsInteracted`, `probe:ej2-payload-daterangepicker-change`); and the render helpers `field.FusionDateRangePicker()`
  and `field.FusionDateRangePicker(build => ..)` over a member of `DateTime[]` or `DateTime[]?` alone (M21.X704). It
  replaces main's six files (`Alis.Reactive.Fusion/Components/FusionDateRangePicker/` at 623dd4a27), whose doors carry
  their verdicts in `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-calendars/src/daterangepicker/daterangepicker.d.ts:289`, `:295`, `:183`, `:793`, `:799`, `:265`,
  `:37`, `:39`, `:41`, `:51`).
- `M21.N17` kept: `FusionDateTimePicker<TValue>`, TValue the type of the member it binds, as EJ2 marks the picker's
  value a generic type (`@syncfusion/ej2-calendars/src/datetimepicker/datetimepicker.d.ts:90-93`): `Value`, a
  `Property<TValue>` whose type argument admits null (M21.X705), `FocusIn`, `FocusOut` and `Change`, with
  `IInputComponent.Input` committed by `Change` (M7.N3), its value written and read as a date for a `DateTime?` member
  and as an instant for a `DateTimeOffset?` one (M21.X706); the payload `FusionDateTimePicker<TValue>.ChangedEventArgs`
  (`Value`, a `TValue`, and `IsInteracted`); and the render helpers `field.FusionDateTimePicker()` and
  `field.FusionDateTimePicker(build => ..)`, which render it with the bound member's type, a date ALIS037 holds. It
  replaces main's six files (`Alis.Reactive.Fusion/Components/FusionDateTimePicker/` at 623dd4a27), whose doors carry
  their verdicts in `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-calendars/src/datetimepicker/datetimepicker.d.ts:93`, `:511`, `:517`,
  `@syncfusion/ej2-calendars/src/calendar/calendar.d.ts:551`, `:698`, `:710`).
- `M21.N18` kept: `FusionDialog`: `Show`, `Hide` and `RefreshPosition`; `BeforeOpen` and `BeforeClose` and their
  payloads `FusionDialog.BeforeOpenEventArgs` (`Cancel`) and `FusionDialog.BeforeCloseEventArgs` (`Cancel`,
  `IsInteracted`); `Open`, `Close` and `OverlayClick`, each an `Event` with no payload (D17.4), as EJ2 types them
  `Object`; its templates and buttons, the vendor builder's through `build` in 1.0, a footer button's click a gate 2
  item; and the render helper `Html.FusionDialog(plan, elementId, build => ..)`, which returns the rendered component in
  place of main's `FusionDialogBuilder<TModel>`. It replaces main's eleven files
  (`Alis.Reactive.Fusion/Components/FusionDialog/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-popups/src/dialog/dialog.d.ts:813`, `:820`, `:798`, `:588`, `:605`, `:572`, `:596`, `:636`, `:134`,
  `:159`, `:163`).
- `M21.N19` kept: `FusionDropDownButton`: `Content` and `CssClass` as `Property<string>`, `Disabled` as
  `Property<bool>`, `DataBind`, `Toggle`, `FocusIn`, `RemoveItems(items, isUniqueId)` and `Select`; the payload
  `FusionDropDownButton.MenuEventArgs` (`Item`) and its item `FusionDropDownButton.ItemModel` (`Id`, `Text`, `IconCss`,
  `Url`, `Separator`, `Disabled`), each named by `[JsonPropertyName]` (M21.X807); and the render helper
  `Html.FusionDropDownButton(plan, elementId, build => ..)`, which returns the rendered component in place of main's
  `FusionDropDownButtonBuilder<TModel>`. It replaces main's seven files
  (`Alis.Reactive.Fusion/Components/FusionDropDownButton/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-splitbuttons/src/drop-down-button/drop-down-button.d.ts:68`, `:75`, `:81`, `:215`, `:308`, `:239`,
  `:188`, `@syncfusion/ej2-splitbuttons/src/common/common.d.ts:72`,
  `@syncfusion/ej2-splitbuttons/src/common/common-model.d.ts:21`, `:35`, `:14`, `:42`, `:28`, `:49`,
  `@syncfusion/ej2-base/src/component.d.ts:129`).
- `M21.N20` kept: `FusionDropDownList<TValue>`, TValue the type of the member it binds, as EJ2 marks DropDownList's
  value a generic type (`@syncfusion/ej2-dropdowns/src/drop-down-list/drop-down-list.d.ts:306`): `Value`, a
  `Property<TValue>` whose type argument admits null, `Text` as `Property<string?>` (M21.X506), `DataSource<TItem>()`,
  `DataBind`, `FocusIn`, `FocusOut`, `ShowPopup`, `HidePopup`, `Clear`, `Change`, `Focus` and `Blur`, with
  `IInputComponent.Input` committed by `Change` (M7.N3); the payload `FusionDropDownList<TValue>.ChangeEventArgs`
  (`Value`, `IsInteracted`), and the focus and blur payloads, which EJ2 types as `Object`, declaring no member (their
  class name a gate 2 gap); and the render helpers `field.FusionDropDownList()`, `field.FusionDropDownList(build => ..)`
  and `field.FusionDropDownList(items, build => ..)`, which render it with the bound member's type and hand EJ2 the
  items by M21.D1, with `Fields<TItem>(text, value)` and `Fields<TItem>(text, value, groupBy)` (M21.X507). It replaces
  main's eight files (`Alis.Reactive.Fusion/Components/FusionDropDownList/` at 623dd4a27), whose doors carry their
  verdicts in `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
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
- `M21.N22` kept: `FusionUploader`, Fusion and EJ2's class name
  (`@syncfusion/ej2-inputs/src/uploader/uploader.d.ts:483`), which both lines' `FusionFileUpload` is renamed to (the
  owner, 2026-11-09): `GetFilesData` in place of the private `filesData` (M21.X707), `Files` (M21.D701), `ClearAll`,
  `Selected` and the framework's `FilesChanged`, with `IInputComponent.Input` committed by `FilesChanged` (M7.N3); the
  payloads `FusionUploader.SelectedEventArgs` (`FilesData`, `Cancel`, `probe:ej2-payload-uploader-selected`) and
  `FusionUploader.FilesChangedEventArgs` (`FilesData`), with no `FilesCount` or `IsInteracted` (M21.X708); the item
  `FusionUploader.FileInfo` (`Name`, `Size`, `Type`, `Status`, `RawFile`); the framework's component code that raises
  `FilesChanged` (`Alis.Reactive.Assets/components/fusion/file-upload.ts` at 65fdf118d), reading the list through
  `getFilesData()`; and the render helpers `field.FusionUploader()` and `field.FusionUploader(build => ..)` over a file
  member alone (M21.X710). It replaces main's six files (`Alis.Reactive.Fusion/Components/FusionFileUpload/` at
  623dd4a27), whose doors carry their verdicts in `spec/FusionComponents/features.json`. Each JavaScript name is EJ2
  33.1.47's public member (`@syncfusion/ej2-inputs/src/uploader/uploader.d.ts:1177`, `:1170`, `:724`, `:162`, `:166`,
  `:92`, `:101`, `:109`, `:105`, `:97`).
- `M21.N23` kept: `FusionGrid`: `DataSource<TRow>()` and `Page<TRow>()` (`dataSource`, CS0111), `Refresh`, the paging,
  sorting, filtering, searching and grouping methods, the column methods (`ShowColumns` and `HideColumns` by field,
  `AutoFitColumns` with and without a field, `ReorderColumns`, `GetColumnByField` returning a `FusionGrid.Column`,
  `RefreshHeader`, `OpenColumnChooser`), selection, editing (`AddRecord` with and without an index, `UpdateRow`,
  `SetRowData`, `SetCellValue`, `EditCell`, `UpdateCell`, `SaveCell`, `GetBatchChanges<TRow>()` returning a
  `FusionGrid.BatchChanges<TRow>`, `StartEdit`, `EndEdit`, `CloseEdit`, `DeleteRecord`), `ExcelExport`, `CsvExport` and
  `PdfExport` as `MethodAsync`, and `Print`; ten events and their payloads, nested in `FusionGrid` under EJ2's names;
  `FusionGrid.SortDirection` and `FusionGrid.FilterOperator` with System.Text.Json's string enum converter; the render
  helper `Html.FusionGrid(plan, id, build => ..)` and `Html.FusionGrid(plan, id, rows, build => ..)`, which hands EJ2
  the rows by M21.D1; `FusionGridColumns.Of<TRow>().Field(..)`; `FusionGridEditTemplates` and its dialog form; and
  `FusionGridValidation.From<TRow>(rules).Field(..)`, a column's rules from the validator (N16.2). It replaces main's
  twenty-two files (`Alis.Reactive.Fusion/Components/FusionGrid/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-grids/src/grid/base/grid.d.ts:1558`, `:3067`, `:3076`, `:3479`, `:3565`, `:3687`, `:3849`, `:3859`,
  `:3870`).
- `M21.N24` kept: `FusionInPlaceEditor<TValue>`, TValue the type of the member it binds: `Value` (`value`), a
  `Property<TValue>` (M21.X404); `Disabled` and `EnableEditMode`, a `Property<bool>` each; `Save`;
  `Element.ClassList.Add` and `Remove`; `BeginEdit`, `EndEdit`, `Change`, `ActionBegin`, `ActionSuccess`, `SubmitClick`
  and `CancelClick` and their payloads, nested in it under EJ2's names, change's `Value` and `PreviousValue` the bound
  member's type; `IInputComponent.Input` committed by `EndEdit` (M7.N3); and the render helpers
  `field.FusionInPlaceEditor()` and `field.FusionInPlaceEditor(build => ..)`, which render it with the member's type and
  start it at the member's value by M21.D1. It replaces main's twelve files
  (`Alis.Reactive.Fusion/Components/FusionInPlaceEditor/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
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
- `M21.N26` kept: `FusionKanban`: `DataSource<TCard>()`, `GetColumnData<TCard, TKey>`, `GetSwimlaneData<TCard>`,
  `DataBind`, `AddCard`, `UpdateCard`, `DeleteCard` (a card, a list or a key, M21.X302), `AddColumn<TKey>`,
  `DeleteColumn`, `ShowColumn<TKey>`, `HideColumn<TKey>`, `ShowSpinner`, `HideSpinner`, `OpenDialog` with
  `FusionKanban.CurrentAction` (M21.X301) and `CloseDialog`; fourteen events and their payloads, nested in it under
  EJ2's names, each cancel a `Property<bool>` and `DataSourceChanged`'s `EndEdit` and `CancelEdit` its methods;
  `FusionKanban.ColumnsModel<TKey>` and `FusionKanban.HeaderArgs<TKey>`, each member named by `[JsonPropertyName]` and a
  column's key typed by the cards' key (M21.X303); and the render helper `Html.FusionKanban(plan, id, build => ..)`. It
  replaces main's seven files (`Alis.Reactive.Fusion/Components/FusionKanban/` at 623dd4a27), whose doors carry their
  verdicts in `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-kanban/src/kanban/base/kanban.d.ts:112`, `:234`, `:429`, `:468`, `:484`, `:502`, `:511`, `:527`).
- `M21.N27` kept: `FusionListBox<TValue>`, TValue the list its items' values make, as EJ2 marks the list box's value a
  generic type (`@syncfusion/ej2-dropdowns/src/list-box/list-box.d.ts:154`): `Value`, a `Property<TValue>`, `DataBind`,
  `SelectItems`, `SelectAll` and `EnableItems`, each taking every argument EJ2 takes, and `Change`; the payload
  `FusionListBox<TValue>.ListBoxChangeEventArgs` (`Value`); and the render helpers `Html.FusionListBox<TValue>(plan, id,
  build => ..)`, TValue the view's, and `Html.FusionListBox(plan, id, items, build => ..)`, TValue the list of the
  items' value member, which hands EJ2 the items by M21.D1, a door neither line has. It replaces main's seven files
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
- `M21.N29` kept: `FusionMention<TValue>`, TValue the type of its items' value member, as EJ2 marks the mention's
  changed value a generic type (`@syncfusion/ej2-dropdowns/src/mention/mention.d.ts:13`): `Search`, `ShowPopup`,
  `HidePopup`, `Change`, `Opened` and `Closed`; the payloads `FusionMention<TValue>.MentionChangeEventArgs` (`Value`, a
  TValue, `IsInteracted`) and `FusionMention<TValue>.PopupEventArgs`; and the render helpers
  `Html.FusionMention<TValue>(plan, id, build => ..)`, TValue the view's, and `Html.FusionMention(plan, id, items, build
  => ..)`, TValue the type of the items' value member, which hands EJ2 the items by M21.D1, with `Fields<TItem>(text,
  value)` (M21.X510), the Mention's instance handed to the view's id by the framework's own component code
  (`Alis.Reactive.Assets/components/fusion/mention.ts:7-11` at 65fdf118d). It replaces main's eight files
  (`Alis.Reactive.Fusion/Components/FusionMention/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-dropdowns/src/mention/mention.d.ts:15`, `:282`, `:294`, `:300`, `:407`, `:413`, `:469`).
- `M21.N30` kept: `FusionMenu`: `Open` and `Close`; `BeforeItemRender`, `BeforeOpen`, `OnOpen`, `BeforeClose`, `OnClose`
  and `Select` and their payloads, `FusionMenu.BeforeOpenCloseMenuEventArgs` with `IsFocused` (optional, `isFocused`),
  which beforeClose holds (M21.X406); and the render helper `Html.FusionMenu(plan, id, build => ..)`. It replaces main's
  twelve files (`Alis.Reactive.Fusion/Components/FusionMenu/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-navigations/src/menu/menu.d.ts:119`, `:126`;
  `@syncfusion/ej2-navigations/src/common/menu-base.d.ts:207-242`, `:552`).
- `M21.N31` kept: `FusionMultiColumnComboBox<TValue>`, TValue the type of the rows' value member, as EJ2 marks the
  change payload's value a generic type
  (`@syncfusion/ej2-multicolumn-combobox/src/multicolumn-combobox/multi-column-combo-box.d.ts:387`): `Value` and `Text`
  as `Property<string?>`, the text EJ2 declares for the component's value (M21.X511), `DataSource<TRow>()`, `DataBind`,
  `FocusIn`, `FocusOut`, `ShowPopup`, `HidePopup` and `Change`, with `IInputComponent.Input` committed by `Change`
  (M7.N3); the payload `FusionMultiColumnComboBox<TValue>.ChangeEventArgs` (`Value`, a TValue, one kind of EJ2's `number
  | string`, `IsInteracted`); and the render helpers `field.FusionMultiColumnComboBox<TValue>()` and
  `field.FusionMultiColumnComboBox<TValue>(build => ..)`, TValue the view's, and `field.FusionMultiColumnComboBox(rows,
  build => ..)`, TValue the type of the rows' value member, which hands EJ2 the rows by M21.D1, with
  `Fields<TItem>(text, value)` and `Fields<TItem>(text, value, groupBy)` (M21.X512). It replaces main's six files
  (`Alis.Reactive.Fusion/Components/FusionMultiColumnComboBox/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-multicolumn-combobox/src/multicolumn-combobox/multi-column-combo-box.d.ts:361`, `:389`, `:437`,
  `:443`, `:451`, `:757`, `:898`, `:905`, `:913`, `:920`).
- `M21.N32` kept: `FusionNumericTextBox<TValue>`, `TValue` the bound member's type as EJ2 marks the value generic:
  `Value` and `Min` as `Property<TValue>` (M21.X607), `FocusIn`, `FocusOut`, `Increment`, `Decrement`, `Change`, `Focus`
  and `Blur`, with `IInputComponent.Input` committed by `Change` (M7.N3); the payloads
  `FusionNumericTextBox<TValue>.ChangeEventArgs` (`Value` and `PreviousValue` as `ReadOnly<TValue>`, M21.X608, and
  `IsInteracted`), `FusionNumericTextBox<TValue>.NumericFocusEventArgs` and
  `FusionNumericTextBox<TValue>.NumericBlurEventArgs`, of which the declaration reads no member; and the render helper
  `field.FusionNumericTextBox(build => ..)`, which renders `FusionNumericTextBox<TProp>`. It replaces main's eight files
  (`Alis.Reactive.Fusion/Components/FusionNumericTextBox/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
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
- `M21.N34` kept: `FusionPivotView`: `DataSourceSettings` as `LiveObject<FusionPivotView.DataSourceSettingsModel>()`
  with `DataSource<TRow>()`, `DisplayOption` as `LiveObject<FusionPivotView.DisplayOptionModel>()` with `View` and
  `Primary`, which carry the shown view, as no door declares the `@hidden` `currentView` (M21.D301), `Refresh`,
  `GetPersistData`, `LoadPersistData`, `ShowConditionalFormattingDialog` and `CreateCalculatedFieldDialog`; `DataBound`
  and `CellSelecting`, whose payload `FusionPivotView.PivotCellSelectedEventArgs` reads its cell as
  `FusionPivotView.IAxisSet`, each member named by `[JsonPropertyName]`, with no door for its loosely typed
  `actualText`, `rowHeaders` and `columnHeaders` (M21.X304); and the render helper `Html.FusionPivotView(plan, id, build
  => ..)`. It replaces main's eight files (`Alis.Reactive.Fusion/Components/FusionPivotView/` at 623dd4a27), whose doors
  carry their verdicts in `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-pivotview/src/pivotview/base/pivotview.d.ts:1447`, `:1476`, `:2244`, `:2460`, `:2467`, `:2816`).
- `M21.N35` kept: `FusionProgressButton`: `Content` and `CssClass` as `Property<string>`, `Disabled` and
  `EnableProgress` as `Property<bool>`, `DataBind`, `Start()` and `Start(percent)`, `Stop`, `ProgressComplete`,
  `FocusIn`, `Begin`, `Progress` and `End`; the payload `FusionProgressButton.ProgressEventArgs`, `Percent` and `Step`
  as `Property<double>` (M21.X802) and `CurrentDuration` as `ReadOnly<double>`; and the render helper
  `Html.FusionProgressButton(plan, elementId, build => ..)`, which returns the rendered component in place of main's
  `FusionProgressButtonBuilder<TModel>`. It replaces main's seven files
  (`Alis.Reactive.Fusion/Components/FusionProgressButton/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-splitbuttons/src/progress-button/progress-button.d.ts:140`, `:134`, `:121`, `:90`, `:218`, `:224`,
  `:230`, `:279`, `:175`, `:182`, `:189`, `:312`, `:316`, `:322`, `@syncfusion/ej2-base/src/component.d.ts:129`).
- `M21.N36` kept: `FusionRadioButton`: `Checked` and `Disabled` as `Property<bool>`, `DataBind`, `GetSelectedValue`,
  `Click`, `FocusIn` and `Change`; the payload `FusionRadioButton.ChangeArgs` (`Value`,
  `probe:ej2-payload-radiobutton-change`); and the render helper `Html.FusionRadioButton(plan, elementId, build => ..)`,
  which returns the rendered component in place of main's `FusionRadioButtonBuilder<TModel>`. It replaces main's seven
  files (`Alis.Reactive.Fusion/Components/FusionRadioButton/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-buttons/src/radio-button/radio-button.d.ts:49`, `:63`, `:134`, `:187`, `:195`, `:36`, `:202`,
  `@syncfusion/ej2-base/src/component.d.ts:129`).
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
  `LiveObject<FusionSchedule.EventSettingsModel>()` with `DataSource<TEvent>()`, `DataBind`, `GetEvents<TEvent>`,
  `AddEvent`, `SaveEvent`, `DeleteEvent<TKey>`, `OpenEditor`, `CloseEditor`, `RefreshEvents`, `Print` and `ScrollTo`;
  `FusionSchedule.CurrentAction`, with System.Text.Json's string enum converter (M21.X305); nine events and their
  payloads, nested in it under EJ2's names, an event's data the app's own class `TEvent` and `Navigating`'s dates read
  as `DateTime` (M21.X306); and the render helper `Html.FusionSchedule(plan, id, build => ..)`. It replaces main's
  fifteen files (`Alis.Reactive.Fusion/Components/FusionSchedule/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-schedule/src/schedule/base/schedule.d.ts:250`, `:274`, `:707`, `:867`, `:1716`, `:1726`, `:1878`).
- `M21.N40` kept: `FusionSidebar`: `IsOpen` as `Property<bool>`, `Show`, `Hide`, `Toggle`, `Open` and `Close`; the
  payload `FusionSidebar.EventArgs` (`IsInteracted`, and `Cancel`, which EJ2 reads back and neither line declares); and
  the render helper `Html.FusionSidebar(plan, elementId, build => ..)`, which returns the rendered component in place of
  main's `FusionSidebarBuilder<TModel>`. It replaces main's seven files
  (`Alis.Reactive.Fusion/Components/FusionSidebar/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-navigations/src/sidebar/sidebar.d.ts:106`, `:246`, `:238`, `:265`, `:202`, `:196`, `:320`, `:332`).
- `M21.N41` kept: `FusionSlider<TValue>`, `TValue` the bound member's type as EJ2 marks the value generic: one `Value`
  as `Property<TValue>`, one number for a slider of one handle and a list of numbers for a range (M21.X614); `DataBind`,
  `Change` and `Changed`, with `IInputComponent.Input` committed by `Changed` (M7.N3); the payload
  `FusionSlider<TValue>.SliderChangeEventArgs` (`Value` and `PreviousValue` as `ReadOnly<TValue>`, M21.X616, `Text`,
  `Action`, `IsInteracted`); and the render helpers `field.FusionSlider()` and `field.FusionSlider(build => ..)`, which
  render `FusionSlider<TProp>` at the member's value (M21.X615), a list member as EJ2's `Range` type
  (`@syncfusion/ej2-inputs/src/slider/slider.d.ts:423`). It replaces main's six files
  (`Alis.Reactive.Fusion/Components/FusionSlider/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-inputs/src/slider/slider.d.ts:366`, `:514`, `:521`, `:70`, `:76`, `:80`, `:84`, `:88`,
  `@syncfusion/ej2-base/src/component.d.ts:129`).
- `M21.N42` kept: `FusionSmartPasteButton`: `Disabled` as `Property<bool>`, `Click` and `FocusIn`; the render helper
  `Html.FusionSmartPasteButton(elementId, options => ..)` with `FusionSmartPasteButtonOptions` (`Content`,
  `AiAssistEndpoint`, `CssClass`, `IsPrimary`), which writes the construction options and the page's antiforgery request
  token into the button with no script and refuses an empty endpoint (M21.X805); and the framework's component code
  (`Alis.Reactive.Assets/components/fusion/smart-paste-button.ts` at 65fdf118d), which builds the SmartPasteButton when
  the element mounts and carries the AI boundary of M21.D801 (M21.X803, M21.X804). It replaces main's four files
  (`Alis.Reactive.Fusion/Components/FusionSmartPasteButton/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-buttons/src/button/button.d.ts:64`, `:176`, `:184`,
  `@syncfusion/ej2-buttons/src/smart-paste-button/smart-paste-button.d.ts:16`, `:23`).
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
- `M21.N44` kept: `FusionSplitButton`: `Content` and `CssClass` as `Property<string>`, `Disabled` as `Property<bool>`,
  `DataBind`, `Toggle`, `FocusIn`, `RemoveItems(items, isUniqueId)`, `Click`, an `Event` with no payload (D17.4), and
  `Select`; the payload `FusionSplitButton.MenuEventArgs` (`Item`) and its item `FusionSplitButton.ItemModel` (`Id`,
  `Text`, `IconCss`, `Url`, `Separator`, `Disabled`), each named by `[JsonPropertyName]` (M21.X808); and the render
  helper `Html.FusionSplitButton(plan, elementId, build => ..)`, which returns the rendered component in place of main's
  `FusionSplitButtonBuilder<TModel>`. It replaces main's eight files
  (`Alis.Reactive.Fusion/Components/FusionSplitButton/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-splitbuttons/src/split-button/split-button.d.ts:32`, `:39`, `:45`, `:183`, `:204`, `:166`, `:102`,
  `:120`, `@syncfusion/ej2-splitbuttons/src/common/common.d.ts:72`,
  `@syncfusion/ej2-splitbuttons/src/common/common-model.d.ts:21`, `:35`, `:14`, `:42`, `:28`, `:49`,
  `@syncfusion/ej2-base/src/component.d.ts:129`).
- `M21.N45` kept: `FusionStepper`: `ActiveStep` as `Property<int>`, `NextStep`, `PreviousStep`, `Reset`,
  `RefreshProgressbar`, `StepChanged`, `StepChanging` and `StepClick`; the payloads
  `FusionStepper.StepperChangedEventArgs` (`PreviousStep`, `ActiveStep`, `IsInteracted`),
  `FusionStepper.StepperChangingEventArgs` (the same and `Cancel`) and `FusionStepper.StepperClickEventArgs`
  (`PreviousStep`, `ActiveStep`); and the render helper `Html.FusionStepper(plan, elementId, build => ..)`, which
  returns the rendered component in place of main's `FusionStepperBuilder<TModel>`. It replaces main's nine files
  (`Alis.Reactive.Fusion/Components/FusionStepper/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-navigations/src/stepper/stepper.d.ts:158`, `:340`, `:346`, `:352`, `:358`, `:238`, `:244`, `:250`).
- `M21.N46` kept: `FusionSwitch`: `Checked` as `Property<bool>` and `Change`, with `IInputComponent.Input` committed by
  `Change` (M7.N3); the payload `FusionSwitch.ChangeEventArgs` (`Checked`), with no `IsInteracted`, which the browser
  does not hand (M21.X711, `probe:ej2-payload-switch-change`); and the render helpers `field.FusionSwitch()` and
  `field.FusionSwitch(build => ..)` over a member of `bool`. It replaces main's six files
  (`Alis.Reactive.Fusion/Components/FusionSwitch/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-buttons/src/switch/switch.d.ts:51`, `:38`, `@syncfusion/ej2-buttons/src/common/common.d.ts:93`).
- `M21.N47` kept: `FusionTab`: `SelectedItem` as `Property<int>`, `Select(index)`, `HideTab(index, value)` and
  `Selected`; the payload `FusionTab.SelectEventArgs` (`SelectedIndex`, `PreviousIndex`, `IsSwiped`); and the render
  helper `Html.FusionTab(plan, elementId, build => ..)`, which returns the rendered component in place of main's
  `FusionTabBuilder<TModel>`. It replaces main's seven files (`Alis.Reactive.Fusion/Components/FusionTab/` at
  623dd4a27), whose doors carry their verdicts in `spec/FusionComponents/features.json`. Each JavaScript name is EJ2
  33.1.47's public member (`@syncfusion/ej2-navigations/src/tab/tab.d.ts:351`, `:662`, `:653`, `:483`).
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
- `M21.N50` kept: `FusionTimePicker<TValue>`, TValue the type of the member it binds, as EJ2 marks the picker's value a
  generic type (`@syncfusion/ej2-calendars/src/timepicker/timepicker.d.ts:373-376`): `Value`, a `Property<TValue>` whose
  type argument admits null (M21.X712), `FocusIn`, `FocusOut` and `Change`, with `IInputComponent.Input` committed by
  `Change` (M7.N3); the payload `FusionTimePicker<TValue>.ChangeEventArgs` (`Value`, a `TValue`, and `IsInteracted`);
  and the render helpers `field.FusionTimePicker()` and `field.FusionTimePicker(build => ..)`, which render it with the
  bound member's type, a date ALIS037 holds (M21.X713). It replaces main's six files
  (`Alis.Reactive.Fusion/Components/FusionTimePicker/` at 623dd4a27), whose doors carry their verdicts in
  `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-calendars/src/timepicker/timepicker.d.ts:376`, `:640`, `:632`, `:439`, `:17`, `:12`).
- `M21.N51` kept: `FusionToolbar`: `Disable` and `Clicked`, whose payload `FusionToolbar.ClickEventArgs` reads its
  `item`, a `FusionToolbar.ItemModel`; and the render helper `Html.FusionToolbar(plan, id, build => ..)`. It replaces
  main's seven files (`Alis.Reactive.Fusion/Components/FusionToolbar/` at 623dd4a27), whose doors carry their verdicts
  in `spec/FusionComponents/features.json`. Each JavaScript name is EJ2 33.1.47's public member
  (`@syncfusion/ej2-navigations/src/toolbar/toolbar.d.ts:344`, `:414`).
- `M21.N52` kept: `FusionTooltip`, the Tooltip of `@syncfusion/ej2-popups`: `Open`, `Close` and `Refresh`;
  `BeforeRender`, `BeforeOpen`, `AfterOpen`, `BeforeClose` and `AfterClose`, named as EJ2 raises them (M21.X806), and
  their payload `FusionTooltip.TooltipEventArgs` (`Cancel`, `IsInteracted`); and the render helper
  `Html.FusionTooltip(plan, elementId, build => ..)`, which returns the rendered component in place of main's
  `FusionTooltipBuilder<TModel>`. It replaces main's eleven files (`Alis.Reactive.Fusion/Components/FusionTooltip/` at
  623dd4a27), whose doors carry their verdicts in `spec/FusionComponents/features.json`. Each JavaScript name is EJ2
  33.1.47's public member (`@syncfusion/ej2-popups/src/tooltip/tooltip.d.ts:541`, `:548`, `:555`, `:350`, `:361`,
  `:369`, `:377`, `:385`, `:76`, `:96`).

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
  (`Alis.Reactive.Fusion/Components/FusionMultiSelect/FusionMultiSelect.cs:19` at 65fdf118d) where EJ2's `value` holds a
  list of one of four kinds or null by default (`@syncfusion/ej2-dropdowns/src/multi-select/multi-select.d.ts:416`), so
  main's clear, `SetValue(null)`
  (`Alis.Reactive.Fusion/Components/FusionMultiSelect/FusionMultiSelectExtensions.cs:36-41` at 623dd4a27), is a nullable
  warning and a box of numbers reads as text; the rewrite's `FusionMultiSelect<TValue>` types it by the bound member,
  whose type admits null (ALIS019, ALIS037), and keeps the clear.
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
  (`Alis.Reactive.Fusion/Components/FusionKanban/FusionKanban.cs:87` at 65fdf118d), and its sandbox keeps EJ2's `"Add"`
  as a constant of its own; the rewrite takes `FusionKanban.CurrentAction`, EJ2's `CurrentAction`, written as
  `openDialog` reads it.
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
- An enum argument and a data member are written as the vendor reads them (M21.X303 to M21.X305).
  `FusionSchedule.CurrentAction` carries System.Text.Json's string enum converter, and
  `FusionKanban.ColumnsModel<TKey>`, `FusionKanban.HeaderArgs<TKey>` and `FusionPivotView.IAxisSet` name each member by
  `[JsonPropertyName]`; a column's key is typed by the cards' key, as a header hands it as the app gave it
  (`probe:ej2-payload-kanban-queryCellInfo-number`), and the pivot's loosely typed `actualText`, `rowHeaders` and
  `columnHeaders` are declared by no door (`@syncfusion/ej2-pivotview/src/base/engine.d.ts:1489`, `:1557`, `:1561`).
- A pivot's shown view is read from public members (M21.D301). Both lines read `currentView`, which EJ2 marks `@hidden`
  (`@syncfusion/ej2-pivotview/src/pivotview/base/pivotview.d.ts:1037-1038`); the public `displayOption` carries the same
  view, and the rewrite declares no `currentView`.
- A multiple selection's indexes read as EJ2 writes them (M21.X401). Main names them `indexes` and the redesign drops
  them, while `getSelectedChips` writes `Indexes` (`@syncfusion/ej2-buttons/src/chips/chip-list.d.ts:56`,
  `probe:ej2-chiplist-multiple-selection`); the rewrite declares `[JsonPropertyName("Indexes")]`.
- An in-place editor calls only public members (M21.X402). Both lines call EJ2's private `disable` and `setFocus`
  (`@syncfusion/ej2-inplace-editor/src/inplace-editor/base/inplace-editor.d.ts:541`, `:525`); the rewrite declares the
  public `disabled` and `enableEditMode`. Its starting value is read with no compile (M21.X403), and its value, its
  input and change's values are typed by the member it binds, the kind each editor type hands (M21.X404,
  `probe:ej2-inplaceeditor-change-Date`).
- A context menu's and a menu's focus is read where EJ2 writes it (M21.X405, M21.X406). Main reads `isFocused` on
  `beforeOpen`, which EJ2 never writes there (`probe:ej2-payload-menu-beforeOpen`), and the redesign dropped it; EJ2
  writes it on `beforeClose` and reads it back to focus the parent item
  (`@syncfusion/ej2-navigations/src/common/menu-base.js:711`, `:820`), so the rewrite declares it optional on
  `BeforeOpenCloseMenuEventArgs`, the payload both events hand.
- One name for a vendor member, EJ2's (M11.D1). The redesign names one JavaScript member by several C# members, each
  with its JavaScript name as text (`Alis.Reactive.Fusion/Components/FusionChipList/FusionChipList.cs:43-59` at
  65fdf118d, `Alis.Reactive.Fusion/Components/FusionGrid/FusionGrid.cs:111` at 65fdf118d), so a reader of Syncfusion's
  documentation looks a member up under a second name; the rewrite names each as EJ2 does, its parameter kinds overloads
  of one name, and writes `name:` only where C# refuses the name.
- A value of the app's own data reads as the app's type. EJ2 types a bound value, a column's key and a selection as
  unions of kinds (`@syncfusion/ej2-inplace-editor/src/inplace-editor/base/inplace-editor.d.ts:187`,
  `@syncfusion/ej2-kanban/src/kanban/base/interface.d.ts:179`, `@syncfusion/ej2-buttons/src/chips/chip-list.d.ts:287`),
  which both lines read as text, so a number held there orders as text; the rewrite types each by a type parameter, the
  bound member's for an input, and the browser facts pin the kind each holds (`probe:ej2-inplaceeditor-change-Numeric`,
  `probe:ej2-payload-kanban-queryCellInfo-number`, `probe:ej2-chiplist-texts-selection`).
- A box's value and text hold null as the vendor's do (M21.X501, M21.X504, M21.X506, M21.X508, M21.X511). The redesign
  declares the value and text of AutoComplete, ComboBox, DropDownList, DropDownTree and MultiColumnComboBox non-null
  (`Alis.Reactive.Fusion/Components/FusionDropDownList/FusionDropDownList.cs:24` at 65fdf118d) where EJ2 documents each
  null until a choice (`@syncfusion/ej2-dropdowns/src/drop-down-list/drop-down-list.d.ts:305-308`); ALIS019 refuses the
  declarations. Each box's value is typed on its own terms: by the member it binds where EJ2 marks the value a generic
  type (AutoComplete, ComboBox, DropDownList), as the list of text EJ2 declares for DropDownTree's, and as the text EJ2
  declares for MultiColumnComboBox's; each admits null, so main's clears keep working.
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
  `probe:ej2-payload-numerictextbox-change-after-cleared`); the rewrite reads the bound member's type, `TValue`, which
  admits null (`decimal?` for a dose), so a view can check a cleared dose.
- A component starts at the member's value (M21.X605, M21.X612, M21.X613, M21.X615). Both lines render Slider, Rating
  and OtpInput by the vendor's builder that binds no member
  (`Alis.Reactive.Fusion/Components/FusionSlider/FusionSlider.cs:83` at 65fdf118d) and SmartTextArea at its options'
  value (`Alis.Reactive.Fusion/Components/FusionSmartTextArea/FusionSmartTextArea.cs:144` at 65fdf118d), so each starts
  empty or where the vendor starts one, whatever the model holds, and the redesign's sandbox repeats the value in the
  build callback (`Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Slider/Index.cshtml:30` at 65fdf118d);
  the rewrite starts each at the member's value by M21.D1.
- A range slider reads its handles (M21.X616). Both lines read a slider's change value as one number
  (`Alis.Reactive.Fusion/Components/FusionSlider/FusionSlider.cs:43` at 65fdf118d), while a range slider hands a list
  (`probe:ej2-payload-slider-rest-change`); the rewrite's payload reads `Value` and `PreviousValue` as the bound
  member's type, a list for a range, as the component's one `Value` reads its value.
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
- A picker's value holds null as the vendor's does (M21.X701, M21.X703, M21.X705, M21.X712). Both lines declare the
  value of DatePicker, DateTimePicker, TimePicker and DateRangePicker as a date with no null
  (`Alis.Reactive.Fusion/Components/FusionDatePicker/FusionDatePicker.cs:23` at 65fdf118d), where EJ2 documents each
  null until one is chosen (`@syncfusion/ej2-calendars/src/datepicker/datepicker.d.ts:83-87`); ALIS019 refuses the
  declarations. The rewrite's DatePicker, DateTimePicker and TimePicker are generic over the member they bind, as EJ2
  marks each value a generic type, their value admitting null as the member's type does, and the DateRangePicker's days
  are `DateTime?`, so a view can clear a picker.
- A picker binds the member its value reads (M21.X702, M21.X704, M21.X706, M21.X713, M21.X710). Both lines render the
  four pickers and the uploader over a member of any type
  (`Alis.Reactive.Fusion/Components/FusionDatePicker/FusionDatePicker.cs:70` at 65fdf118d); the rewrite's DatePicker,
  DateTimePicker and TimePicker take the bound member's type as their type argument, which the vendor contract analyzer
  holds to a date (ALIS037), the DateRangePicker's and the uploader's helpers take the list of dates or the file member
  each value is, and a DateTimePicker bound to a `DateTimeOffset?` sends the picked moment as an instant with the
  browser's offset, where both lines send a wall time with none (D3.9).
- A payload declares what the browser hands (M21.X708, M21.X711, M21.D702). Both lines declare `IsInteracted` on the
  Switch's change payload and main declares `FilesCount` and `IsInteracted` on the uploader's selected payload, members
  no listener receives (`probe:ej2-payload-switch-change`, `probe:ej2-payload-uploader-selected`), so a condition on
  them never holds; the rewrite declares neither, and DateRangePicker's value, a union in its TypeScript, is the list of
  two dates the browser holds (`probe:ej2-daterangepicker-value-after-typing`), each fact a test the rewrite ships and
  reruns on every upgrade.
- The uploader reads its files through the vendor's public door (M21.X707, M21.D701). The redesign reads the uploader's
  `filesData`, which EJ2 marks private (`@syncfusion/ej2-inputs/src/uploader/uploader.d.ts:535-540`), in its declaration
  and its component code; the rewrite calls the public `getFilesData()`
  (`@syncfusion/ej2-inputs/src/uploader/uploader.d.ts:1177`), sends the files the user chose alone, and takes EJ2's
  class name, `FusionUploader`.
- A button's icon position reaches EJ2 by name (M21.X801). The redesign writes `FusionButtonIconPosition` as its number,
  since the enum carries no converter (`Alis.Reactive.Fusion/Components/FusionButton/FusionButtonIconPosition.cs:6` at
  65fdf118d), and EJ2 calls `toLowerCase` on the value to place the icon
  (`@syncfusion/ej2-buttons/src/button/button.js:124`), so on a button with text and an icon the write throws and the
  button loses its icon. ALIS037 refuses that declaration; the rewrite's enum carries a string enum converter, written
  Left, Right, Top or Bottom, and refuses a value outside the four where the view renders, as main's own map does.
- An event is heard where its vendor raises it, with the payload it declares (M21.X806). Main's Tooltip `Opened` and
  `Closed` listen to `open` and `close`, two of its methods (`@syncfusion/ej2-popups/src/tooltip/tooltip.d.ts:541`), so
  they never fire; ALIS018 refuses such a declaration, and the rewrite's `AfterOpen` and `AfterClose` name EJ2's events.
  Dialog's open, close and overlayClick, which EJ2 types `Object`, and SplitButton's click, whose payload holds the
  element alone, are events with no payload (D17.4), so no member ships without a probe.
- A progress handler steers its progress (M21.X802). The redesign declares the progress payload's `Percent` and `Step`
  read-only, where EJ2 reads both back from the listener
  (`@syncfusion/ej2-splitbuttons/src/progress-button/progress-button.js:277`, `:280`); the rewrite declares them
  `Property<double>`, and adds `Stop`, EJ2's public stop, which neither line declares.
- A Smart Paste works against a protected endpoint and in a partial view, and a button with no endpoint is refused where
  the view renders (M21.X803, M21.X804, M21.X805, M21.D801). Both lines post the AI request with no antiforgery token
  (`Alis.Reactive.Assets/components/fusion/smart-paste-button.ts:18` at 65fdf118d), main builds the button in a script
  that waits for DOMContentLoaded, and both read the text of any status as an answer. The rewrite sends EJ2's
  ChatOptions as JSON with the page's request token in the header `RequestVerificationToken`, hands EJ2 the whole text
  of a 200 to 299 response and nothing otherwise, never a stream, and builds the button when it mounts.
- An AI Assist view answers from the app's endpoint, whole (M21.D901, M21.X901). Main's methods take literals alone, so
  no response body reaches the view, and its `CancelRequest` writes a cancel EJ2 never reads
  (`@syncfusion/ej2-interactive-chat/src/ai-assistview/ai-assistview.js:1354`), which leaves a declined prompt waiting
  with its stop button showing. The rewrite's `AddPromptResponse` takes any source and sets the response whole, a prompt
  with its response goes in through EJ2's `PromptModel`, no door takes a cancel, and the render helper keeps streaming
  off in 1.0.
- A bullet chart's items are the app's typed data, and its tooltip reads what the browser hands (M21.X902, M21.X903).
  Neither line has a door of its own for the chart's items or their fields, which both leave to the vendor builder's
  object data and text field names; the rewrite hands the items by M21.D1 with typed `Fields<TItem>`,
  `DataSource<TItem>()` replaces them, the tooltip's value and targets are the formatted text a real hover pins, its
  template is writable as EJ2 reads it back, and the read of the private `getActualIndex` leaves.
- A carousel's slide direction is EJ2's two names, and a sidebar can be kept open or closed (M21.X904). Both lines read
  the direction as any text (`@syncfusion/ej2-navigations/src/carousel/carousel.d.ts:12`); the rewrite reads
  `FusionCarousel.CarouselSlideDirection`, and its sidebar payload adds the `Cancel` EJ2 reads back on open and close.
