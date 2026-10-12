# Capability catalog: the release's behavior contract

Read from the Playwright suites, the sandbox views they drive and the source at `a2c497b`
(2026-10-11). Every capability lists where a browser proves it. "Plan-only" means an NUnit fixture
inside the Playwright project asserts plan JSON without opening a browser; under the release rule
(Playwright is the only verification) that is not proof.

## 1. JS objects the DSL touches

| Object | How it is reached | Members used |
|--------|-------------------|--------------|
| Component | `p.Component<T>(m => m.X)`; `Component<T, TOtherModel>(m => ..)` (another view model, e.g. a partial writing the shell's field); `Component<T>("id")`; `Component<T>()` app-level by fixed id | properties read/write, methods with typed args, returning methods, events |
| Event payload | the args parameter of a reaction | nested and indexed reads (`BatchChanges.ChangedRecords[0].OpenTasks`, `Data[0].Summary`); writes (`cancel` in 10 slices, `preventDefaultAction`, BulletChart `text`); void calls after an async gap (`updateData(response)`, Kanban `endEdit()`) |
| Response body | `OnSuccess<T>((res, p) => ..)`, `OnError<T>(status, ..)` | typed reads, nested paths, arrays, whole body |
| Plugin | `p.Plugin(...)`; registered by the app bundle on `window.__alisPlugins` | typed properties, functions with args, commands |
| Element | `p.Element("id")` | text, html, classes, show/hide |
| Array element | `p.From(...)`, `p.FromDom(...)` | per-element method reads (`GetAttribute`, `StartsWith`) |

The plan and runtime already treat components and plugins alike: both resolve to `RuntimeObject`
through the same `types` contracts (`runtime-plan.ts:70-76`). Event payloads are read by path.

## 2. Capabilities and their proofs

**Component members**
- Property write from a literal: everywhere (DatePicker `admission_date_opens_set_to_june_15_2026`).
- Property write from a response: Grid, Kanban, Schedule, Pivot data sources; Schedule and Pivot
  write nested mapped paths (`eventSettings.dataSource`, `dataSourceSettings.dataSource`).
- Property write from a payload into another component: Grid BatchChangeReview.
- Property write from another component's read through arrays: ArrayGrid (`Data()` → `Where` →
  `SetDataSource`).
- Method with typed literal or object args: Grid `AddRecord(new Row)`, Mention `Search(s, x, y)`,
  ListBox `selectItems([..], bool, bool)`, Kanban `AddColumn`.
- Method with field-expression args (typed columns): Grid `SortBy`, `FilterTextBy`, `GroupBy`,
  `EditCell`, `SetCellValue`.
- Method args from a response: Grid ServerRoster (`AddRecord(json, x => x.Row)`, `UpdateRow`,
  `SetRowData`), Kanban add/update/delete, Pivot `LoadPersistedLayout`.
- Returning method as a source: RadioButton `SelectedValue()` into text, `Eq` and gather; Grid
  `SelectedRecords<TModel, TRow>()`, `CurrentViewRecords`, `RowIndexByPrimaryKey`, `BatchChanges`
  into gather; Schedule `GetEvents()`, `CurrentView()`; ChipList `SelectedChips()` into
  `DispatchWith` then `p.From().Any/Count`; BulletChart `GetActualIndex` into text.
- Programmatic writes fire change with `IsInteracted = false` and journeys branch on it
  (TimePicker, TextBox, TextArea, Numeric, Slider, OtpInput, Rating `Reset`).
- A component calls its own method inside its own handler (OtpInput Focus → FocusOut).
- Write then read in one branch (CheckBox `SetDisabled(false)` then `Disabled()`).

**Event payloads**
- Reads, nested, indexed, nullable, typed `TRow` (Grid `RecordClick<TRow>`, Kanban `<TCard>`).
- Writes: cancel on Grid `BeginEdit`/`ActionBegin`/`CellSave`/`BeforeBatchSave`, Carousel,
  Stepper, ListView, Menu; `preventDefaultAction` on AutoComplete/MultiSelect filtering.
- Void method with an argument from the HTTP response, called after the response returns:
  AutoComplete `filtering_response_populates_autocomplete_dropdown`, MultiSelect.
- Reactions stay synchronous so payload writes land before the vendor reads them
  (`trigger.ts:15`, `execute.ts:43`).
- Native payload is the element (`event.currentTarget`), so any element property is readable;
  CustomEvent payload is `detail`; Fusion payload is the args object, `{}` when absent.

**HTTP**
- GET/POST/PUT/DELETE; JSON default, form data with files; GET writes gather to the query string.
- Gather: `IncludeAll`, `Include<TComponent, TModel>(m => ..)`, component source, `FromEvent`,
  URL, headers, route params (literal, URL, component, previous response), static, plugin.
- Typed success and error by exact status, then any status; network failure only reaches
  status-less error routes; content-type sniffing.
- Chained (one follow-up, success only) and nested requests gathering from the previous response;
  `Parallel(..).OnAllSettled`; `WhileLoading` and `Finally` (Finally after success, error,
  unmatched status, abort; the parent's Finally runs before a chained request starts).
- `Validate<T>(form)` before send; `ValidationErrors(form)` maps server errors to fields.
- `Into(id)` partial injection with slot lifecycle; injected partials can carry Fusion and Native
  inputs, their own plans, realtime connections, and steer the parent page.
- An HTTP step holds back the steps authored after it (proven order in HttpMixing).
- ServerPush (one EventSource per URL) and SignalR (one connection per hub, retry indicator).

**Conditions**
- Sources: payload, component property, response body, trigger payload inside response scopes,
  source-vs-source. All operators by shape; `And`/`Or` fold left to right; `Not`; `ElseIf`/`Else`
  first match; nested branches; `Confirm`; conditions mixed in authored order with sets,
  dispatch, HTTP, parallel, chained and plugin steps. The comparison engine has no vendor code.

**Validation**
- Rule kinds and shapes (Required differs by shape and nullability), peers, `WhenField(s)`,
  nested validators and `ClientRuleEach` over rendered items, hidden fields routed to the summary,
  live clear on `input` and revalidate on `blur` and the component's change event, submit-time
  validation, server errors inline or in the summary, rules for fields a later partial renders,
  DI-resolved validators, grid column rules from the same validator.

**Arrays, dispatch, plugins, app-level**
- Arrays over payloads, responses, DOM collections and component reads; `Where`, `Count`, `Any`,
  `Sum`, `OrderByDescending`, `Find` proven; `Select`, `OrderBy`, `All` not browser-proven.
- `Dispatch`, typed `Dispatch<T>`, `DispatchWith` from component values, `CustomEvent<T>`.
- Plugins: functions over response arrays, plugin source in conditions and gather.
- Drawer, Loader, Toast, Confirm by fixed id; NativeActionLink carries its own plan.
- C#-authored templates (`FusionTemplate` with `EventButton` → CustomEvent), grid edit templates.

## 3. Where the core knows about specific components or vendors

These are the dependencies the release inverts.

**C# core**
- Declaration APIs are internal; Fusion and Native reach them through `InternalsVisibleTo`
  (`Alis.Reactive.csproj:69`); 24 slice files hand-write payload members with `ReactionGraph`.
- ~~An input's value must be one named property: `IInputComponent.ValueMember` feeds registration,
  gather, validation~~: closed. An input declares `ComponentInput` (its value property and the
  event that commits it); fields another plan renders still read the literal `"value"` member.
- `RegisteredInputValueRead.ForFusionInPlaceEditorValueRead` (`RegisteredInputValueRead.cs:28`).
- ~~`ComponentVendor.Fusion` token~~: closed by D2 (`FusionComponent.FusionVendor` declares it);
  EJ2-specific projections remain in `ClientRule.cs:21-26`.
- `Include<TComponent>` reads the input's declared value; `NativeGatherExtensions` assumes NativeTextBox.
- Method contracts are keyed per component instance, so two arities of one member on one grid
  throw (`BrowserObjectContract.cs:257-268`); `Mapped` aliases dodge it.

**Runtime**
- ~~Two hard-coded drivers and two event adapters~~: closed by D2; vendors are plan data and one
  driver follows them.
- `live-clear.ts:102` hard-codes `"change"`; `input`/`blur` go to the element; a not-ready
  Fusion object silently skips change wiring (`live-clear.ts:107-117`).
- `inject.ts:31-35` calls `ej.base.append(nodes, container, true)`; the inline scripts in the
  Native RadioGroup and CheckList builders, and Syncfusion's partial scripts, depend on it.
- `request-payload-writer.ts:163` unwraps Syncfusion's uploader `rawFile`.
- `p.Confirm()` needs `window.alis.confirm`, installed by the Syncfusion Dialog module
  (`confirm.ts:103`); `root.ts` statically imports confirm, action link, drawer, loader.
- The drawer clears its content on `transitionend` without unloading the slot (`drawer.ts:25-30`).

**Native builders** emit inline `<script>` (`NativeRadioGroupBuilder.cs:196`,
`NativeCheckListBuilder.cs:200`), against "views carry no JavaScript"; their `SetValue` does not
sync the boxes.

## 4. Proofs that are weaker than they look

- HttpMixing else-path tests dispatch PascalCase keys through `Page.EvaluateAsync`
  (`WhenTriggerDrivenConditionsMixWithHttp.cs:420,455`); payload paths are camelCase, so every read
  is missing and the test would pass with true values too.
- ArrayManager send test gathers `p.Plugin(array.Count).Arg("items")`: the count of the string
  `"items"` (5) happens to equal the resident count (`ArrayManager/Index.cshtml:50`).
- AutoComplete/MultiSelect `FromEvent` tests assert only the endpoint name in the URL.
- The 422 summary text is hard-coded in the view; ColorPicker toggle asserts only echo text.
- Plan-only fixtures: RequestInput, CapitalsNaming, NativeActionLink serialization, BehaviorGraph,
  ClientRules. Component-method sources in gather and conditions, URL and plugin sources in
  conditions, and event-payload gather are proven only there.
- Not browser-proven at all: Fusion live revalidation, InPlaceEditor BeginEdit cancel, slot-owned
  rules removed on unload, two partials merging into one `planId`, Grid returning methods into
  arrays or conditions, `ArrayContains`/`EndsWith`, `Select`/`OrderBy`/`All`.
