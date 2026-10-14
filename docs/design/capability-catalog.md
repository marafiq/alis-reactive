# Capability catalog: the release's behavior contract

Read from the Playwright suites, the sandbox views they drive and the source at `a2c497b`
(2026-10-11); DSL names and `file:line` references updated at `f36f62e` (C# references again at `6ac4198`), where a component, a payload
and a plugin are used as the objects they are (`rate.Value.Set(..)`, `grid.Refresh()`, `args.Cancel.Set(true)`). Every capability lists where a browser proves it. "Plan-only" means an NUnit fixture
inside the Playwright project asserts plan JSON without opening a browser; under the release rule
(Playwright is the only verification) that is not proof.

## 1. JS objects the DSL touches

| Object | How it is reached | Members used |
|--------|-------------------|--------------|
| Component | `p.Component<T>(m => m.X)`; `Component<T, TOtherModel>(m => ..)` (another view model, e.g. a partial writing the shell's field); `Component<T>("id")`; `Component<T>()` app-level by fixed id | properties read/write, methods with typed args, returning methods, events |
| Event payload | the args parameter of a reaction, a declared object (`args.Value`, `args.Cancel.Set(true)`) | nested and indexed reads (`args.BatchChanges.Get(x => x.ChangedRecords[0].OpenTasks)`, `Data[0].Summary`); writes (`Cancel` in 10 slices, `PreventDefaultAction`, BulletChart `Text`); void calls after an async gap (`args.UpdateData(json.Get(..))`, Kanban `args.EndEdit()`) |
| Response body | `OnSuccess<T>((json, s) => ..)`, `OnError<T>(status, ..)`; `json` is the body (`Data<T>`) | typed reads (`json.Get(x => x.Name)`), nested paths, arrays, whole body (`json`) |
| Plugin | a `Plugin` declaration, `plan.RegisterPlugin<T>()`; registered by the app bundle on `window.__alisPlugins` | typed properties (`ReadOnly<T>`), functions with args (`slugify.Slug(..)`), commands (`analytics.Track(..)`) |
| Element | `p.Element("id")` | `Text.Set`, `Html.Set`, `ClassList.Add/Remove/Toggle`, `Show`/`Hide` |
| Array element | `p.From(...)`, `p.Element(id).Children`, `p.Element(id).ClassList.Items` | per-element method reads (`GetAttribute`, `StartsWith`) |

The plan and runtime treat components and plugins alike: the runtime locates either as a plan object
(`Alis.Reactive.Assets/runtime/objects/world.ts:14-18`), and in C# both are `JsObject` declarations bound through
`ComponentBinding` and `PluginBinding` (`Alis.Reactive/Components/Contracts/ObjectBinding.cs:57-72, 89-100`). Event payloads are read by path,
through `EventPayloadBinding` (`Alis.Reactive/Components/Contracts/ObjectBinding.cs:78-86`).

## 2. Capabilities and their proofs

**Component members**
- Property write from a literal: everywhere (DatePicker `admission_date_opens_set_to_june_15_2026`).
- Property write from a response: Grid, Kanban, Schedule, Pivot data sources; Schedule and Pivot
  write nested mapped paths (`eventSettings.dataSource`, `dataSourceSettings.dataSource`).
- Property write from a payload into another component: Grid BatchChangeReview.
- Property write from another component's read through arrays: ArrayGrid (`Rows<T>()` → `Where` →
  `DataSource.Set`).
- Method with typed literal or object args: Grid `AddRecord(new Row)`, Mention `Search(s, x, y)`,
  ListBox `SelectItems([..], bool, bool)`, Kanban `AddColumn`.
- Method with field-expression args (typed columns): Grid `SortColumn`, `FilterByColumn`, `GroupColumn`,
  `EditCell`, `SetCellValue`.
- Method args from a response: Grid ServerRoster (`AddRecordAt(json.Get(x => x.Row), 0)`, `UpdateRow`,
  `SetRowData`), Kanban add/update/delete, Pivot `LoadPersistData`.
- Returning method as a source: RadioButton `GetSelectedValue()` into text, `Eq` and gather; Grid
  `GetSelectedRecords<TRow>()`, `GetCurrentViewRecords<TRow>()`, `GetRowIndexByPrimaryKey(..)`, `GetBatchChanges<TRow>()`
  into gather; Schedule `GetEvents<T>()`, `CurrentView`; ChipList `GetSelectedChips()` into a
  `Dispatch` payload then `p.From(..).Any/Count`; BulletChart `GetActualIndex(..)` into text.
- Programmatic writes fire change with `IsInteracted = false` and journeys branch on it
  (TimePicker, TextBox, TextArea, Numeric, Slider, OtpInput, Rating `Reset`).
- A component calls its own method inside its own handler (OtpInput Focus → FocusOut).
- Write then read in one branch (CheckBox `Disabled.Set(false)` then `Disabled`).

**Event payloads**
- Reads, nested, indexed, nullable, typed `TRow` (Grid `RecordClick<TRow>`, Kanban `<TCard>`).
- Writes: cancel on Grid `BeginEdit`/`ActionBegin`/`CellSave`/`BeforeBatchSave`, Carousel,
  Stepper, ListView, Menu; `preventDefaultAction` on AutoComplete/MultiSelect filtering.
- Void method with an argument from the HTTP response, called after the response returns:
  AutoComplete `filtering_response_populates_autocomplete_dropdown`, MultiSelect.
- Reactions stay synchronous so payload writes land before the vendor reads them
  (`Alis.Reactive.Assets/runtime/grammar/execute.ts:17-26`; `Alis.Reactive.Assets/runtime/plans/start.ts:46-58`).
- Native payload is the element (`event.currentTarget`), so any element property is readable;
  CustomEvent payload is `detail`; Fusion payload is the args object, `{}` when absent.

**HTTP**
- GET/POST/PUT/DELETE; JSON default, form data with files; GET writes gather to the query string.
- Gather builds the typed request the endpoint binds (`Gather<TRequest>`, or `Gather(..)` for the view
  model): `Form()` and `Form(m => ..)` send the form's inputs by binding name; `Set(x => x.Member, ..)`
  takes a literal or any source (component member `rate.Value`, event payload `args.Value`, URL query
  `p.Url<TQuery>().Get(..)`, previous response, plugin, array value); `Header(name, ..)`. A member
  named like a `{placeholder}` fills the route.
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
- Rule kinds and shapes (Required differs by shape and nullability), peers, conditions (`WhenField(s)` at
  `a2c497b`, FluentValidation's `When(x => ..)` at `f36f62e`), nested validators and `ClientRuleEach`
  (`RuleForEach(..).SetValidator(..)` at `f36f62e`) over rendered items, hidden fields routed to the summary,
  live clear on `input` and revalidate on `blur` and the component's change event, submit-time
  validation, server errors inline or in the summary, rules for fields a later partial renders,
  DI-resolved validators, grid column rules from the same validator.

**Arrays, dispatch, plugins, app-level**
- Arrays over payloads, responses, DOM collections and component reads; `Where`, `Count`, `Any`,
  `Sum`, `OrderByDescending`, `Find` proven; `Select`, `OrderBy`, `All` not browser-proven.
- `Dispatch(appEvent)`, `Dispatch(appEvent, payload)`, `Dispatch(appEvent, d => d.Set(..))` from component values, `CustomEvent<T>`.
- Plugins: functions over response arrays, plugin source in conditions and gather.
- Drawer, Loader, Toast, Confirm by fixed id; NativeActionLink carries its own plan.
- C#-authored templates (`FusionTemplate` with `EventButton` → CustomEvent), grid edit templates.

## 3. Where the core knows about specific components or vendors

These are the dependencies the release inverts.

**C# core**
- ~~Declaration APIs are internal; Fusion and Native reach them through `InternalsVisibleTo`; 24 slice
  files hand-write payload members with `ReactionGraph`~~: closed. A declaration is a public `JsObject`
  (`Alis.Reactive/Components/Contracts/JsObject.cs:35`), a payload member is a `ReadOnly<T>` or `Prop<T>`
  (`Alis.Reactive/Components/Contracts/JsMembers.cs:21, 77`), no slice names `ReactionGraph`, and
  `InternalsVisibleTo` names only the contract generator (`Alis.Reactive/Alis.Reactive.csproj:69`).
- ~~An input's value must be one named property: `IInputComponent.ValueMember` feeds registration,
  gather, validation~~: closed. An input declares `ComponentInput` (its value property and the
  event that commits it); a field another plan renders is read through the model by the id its
  member gives it (`Alis.Reactive/Validation/FormChecks.cs:124-126`).
- ~~`RegisteredInputValueRead.ForFusionInPlaceEditorValueRead`~~: closed; one bound read, `ValueExpression.ReadBound(inputId, shape)`,
  serves every component (`Alis.Reactive/PlanModel/Values/ValueExpression.cs:89-90, 268-269`).
- ~~`ComponentVendor.Fusion` token~~: closed by D2 (`FusionComponent.FusionVendor` declares it);
  the EJ2 projections left `ClientRule`, the public browser-rule contract a component reads back (`Alis.Reactive/Validation/ClientRule.cs:10-14`),
  for the grid's own slice (`Alis.Reactive.Fusion/Components/FusionGrid/FusionGridValidation.cs:117-150`).
- ~~`Include<TComponent>` reads the input's declared value; `NativeGatherExtensions` assumes NativeTextBox~~:
  closed; both were deleted with the typed request, and `Form(m => ..)` sends an input by its binding name.
- ~~Method contracts are keyed per component instance, so two arities of one member on one grid
  throw~~: closed; the member-contract registry is gone, and each call carries the argument shapes its own
  C# member declares (`Alis.Reactive/Components/Contracts/ComponentMember.cs:57, 62-63`; `Alis.Reactive/PlanModel/Reactions/Steps.cs:76-82`),
  so two C# members typing one JS method do not collide (`Alis.Reactive/Components/Contracts/JsObject.cs:208-209, 214-221`).

**Runtime**
- ~~Two hard-coded drivers and two event adapters~~: closed by D2; vendors are plan data and one
  driver follows them.
- ~~Live clearing hard-codes `"change"` and silently skips a not-ready Fusion object~~: closed. Each input
  declares the event that commits it, the model re-raises it as `alis:commit` and logs a subscription it
  cannot make (`Alis.Reactive.Assets/runtime/modules/model.ts:160-175`); `input`/`blur` still go to the element
  (`Alis.Reactive.Assets/runtime/modules/form/form.ts:211-228`).
- ~~Injection calls `ej.base.append(nodes, container, true)`~~: closed. A slot re-creates the inserted
  `<script>` nodes itself (`Alis.Reactive.Assets/runtime/modules/slot.ts:62-73`), and the RadioGroup and CheckList scripts became
  component code (`Alis.Reactive.Assets/components/native/radio-group.ts:6-14`, `Alis.Reactive.Assets/components/native/check-list.ts:10-20`).
- ~~The request writer unwraps Syncfusion's uploader `rawFile`~~: closed; the uploader declares its files as
  `FilesData.Each(file => file.RawFile)`, read at `filesData[].rawFile` (`Alis.Reactive.Fusion/Components/FusionFileUpload/FusionFileUpload.cs:19`).
- ~~`p.Confirm()` needs `window.alis.confirm`; `root.ts` statically imports confirm, action link, drawer,
  loader~~: closed. `alis.confirm` is a plugin the Fusion component code provides (`Alis.Reactive.Assets/components/fusion/confirm.ts:101-106`),
  and `root.ts` registers only the runtime's own modules (`Alis.Reactive.Assets/runtime/root.ts:32-38`).
- The drawer clears its content on `transitionend` without unloading the slot (`Alis.Reactive.Assets/components/native/drawer.ts:48-55, 71-75`).

~~**Native builders** emit inline `<script>`, against "views carry no JavaScript"; their `SetValue` does not
sync the boxes~~: closed. The scripts are component code (`Alis.Reactive.Assets/components/native/radio-group.ts:6-14`,
`Alis.Reactive.Assets/components/native/check-list.ts:10-20`), and neither declaration has a `SetValue`.

## 4. Proofs that are weaker than they look

- HttpMixing else-path tests dispatch PascalCase keys through `Page.EvaluateAsync`
  (`WhenTriggerDrivenConditionsMixWithHttp.cs:420,454`); payload paths are camelCase, so every read
  is missing and the test would pass with true values too.
- ArrayManager send test gathers `array.Count("items")`: the count of the string
  `"items"` (5) happens to equal the resident count (`ArrayManager/Index.cshtml:93`).
- AutoComplete/MultiSelect filtering tests (payload text set on the request) assert only the endpoint
  name in the URL.
- The 422 summary text is hard-coded in the view; ColorPicker toggle asserts only echo text.
- Plan-only fixtures: RequestInput, CapitalsNaming, NativeActionLink serialization, BehaviorGraph,
  ClientRules. Component-method sources in gather and conditions, URL and plugin sources in
  conditions, and event-payload gather are proven only there.
- Not browser-proven at all: Fusion live revalidation, InPlaceEditor BeginEdit cancel, slot-owned
  rules removed on unload, two partials merging into one `planId`, Grid returning methods into
  arrays or conditions, `ArrayContains`/`EndsWith`, `Select`/`OrderBy`/`All`.
