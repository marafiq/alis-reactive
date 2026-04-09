# Schedule Onboarding — Experiment Notes

**Session**: 2026-04-09
**Status**: In progress — proving end-to-end before committing

## What's Proven (with evidence)

### 1. SF Schedule renders with server data via reactive plan
- **How**: DomReady → GET `/api/schedule/assignments` → `SetDataSource(json, j => j.Assignments)`
- **Evidence**: Console log shows `[alis:http] fetch`, `[alis:execute] set eventSettings.dataSource`, `dataBind`
- **Screenshot**: Schedule renders 21 events across 3 shift resource groups

### 2. Facility filter reloads different data
- **How**: FusionDropDownList `.Reactive(evt => evt.Changed)` → GET with Include → SetDataSource
- **Evidence**: Mystery Manor: 10 unassigned, Sunrise Gardens: 3, Oak Ridge: 4 — different staff per facility
- **Screenshot**: Dropdown changes, unassigned count changes, events change

### 3. SF built-in editor opens with correct data
- **How**: Double-click event → SF opens Edit Event popup with Title, Start/End, Shift dropdown, Description
- **Evidence**: `POPUP_OPEN type: "Editor"` in console, all fields populated from event data
- **Screenshot**: Full editor form with "Maria Garcia (RN)", shift dropdown, care notes

### 4. ActionBegin/ActionComplete events fire and trigger server reload
- **How**: `.Reactive(evt => evt.ActionBegin)` with `When(args.RequestType).Eq("eventChange")` → status update
  `.Reactive(evt => evt.ActionComplete)` with `When(args.RequestType).Eq("eventChanged")` → GET refresh
- **Evidence**: Console shows: "Saving..." → "Saved — reloading..." → HTTP GET → SetDataSource → "Ready"
- **Note**: Currently only refreshes from server after SF's local save. No POST of edited data to server yet.

### 5. Mutable in-memory store — CRUD persists across requests
- **How**: `FakeScheduleData` uses `ConcurrentDictionary`, `AssignStaff`/`UnassignStaff` mutate stored data
- **Evidence**: curl test — POST assign → GET returns updated data. POST unassign → GET returns reverted data.

### 6. SF editorTemplate accepts div reference (not just script/x-template)
- **How**: Set `ej2.editorTemplate = '#test-editor-div'` pointing to a hidden div
- **Evidence**: SF rendered the div content inside its editor popup with Save/Delete/Cancel buttons
- **Implication**: We can render a div in Razor with our Html.InputField components

### 7. SF editorTemplate with custom SF sub-components works
- **How**: editorTemplate with `<input class="e-field">`, initialize DropDownList/DateTimePicker in `popupOpen`
- **Evidence**: Custom Staff Member dropdown, Shift Start/End DateTimePickers, Shift dropdown, Care Notes all rendered inside SF editor popup
- **Screenshot**: Full custom editor with SF components inside SF's popup container

### 8. Tooltip works
- **How**: `EnableTooltip = true` in ScheduleEventSettings
- **Evidence**: Hovering over event shows tooltip with subject, date/time

### 9. camelCase field mapping required
- **Key**: API returns camelCase JSON. SF field mappings must match: `id`, `subject`, `startTime`, `shiftId`
- **Resource field**: `Field = "shiftId"` (not "ShiftId") to match API data
- **ScheduleField mappings**: `Id = "id"`, `Subject.Name = "subject"`, `StartTime.Name = "startTime"`, etc.

## What's NOT Proven Yet

### A. Our Html.InputField components inside SF editor
- **Problem**: SF clones the template content. Cloned DOM nodes lose reactive bindings.
- **Possible approach**: Use `Into()` + server partial in `popupOpen` event — like wizard pattern
- **Alternative**: Use `popupOpen` element access to load partial into SF's dialog content area
- **Needs experiment**: Can we `Into("e-dlg-content")` inside the SF editor on `popupOpen`?

### B. POST edited data to server on Save (not just local SF save)
- **Current**: ActionComplete triggers GET refresh, but no POST of the changed data
- **Need**: In ActionBegin with requestType "eventChange", gather the changed event data and POST
- **Options**:
  1. Cancel SF save (`args.cancel = true`) → POST ourselves → on success, refresh via GET
  2. Let SF save locally, then in ActionComplete POST the changed data, then refresh from server

### C. Week navigation passes currentDate to server
- **Current**: Navigating handler has `FromEvent(args, x => x.CurrentDate, "currentDate")` in gather
- **Need to verify**: Does the runtime correctly extract `currentDate` from the SF navigating event args?
- **NavigatingArgs has**: `CurrentDate` and `PreviousDate` properties

### D. Print button
- **Wired**: `p.Component<FusionSchedule>("shift-schedule").Print()`
- **Not tested**: Does `EmitCall("print")` actually work through the runtime?

### E. View tab switching (Day/Week/Month/Agenda)
- **SF handles natively**: View tabs are part of SF Schedule header
- **Not tested**: Does switching views maintain data? Do events render in Month/Agenda views?

### F. Drag and drop
- **Wired**: `AllowDragAndDrop(true)` set on builder
- **Not tested**: Does dragging fire actionBegin/actionComplete? Can we persist the change?

## Key Patterns Discovered

### SF Schedule C# Builder — NO fluent chaining
SF ScheduleBuilder methods return `void`, not the builder. Every call must be a separate statement:
```csharp
b.Width("100%"); b.Height("100%"); b.CurrentView(View.Week);
b.Views(new List<ScheduleView> { ... });
b.Group(new ScheduleGroup { ... });
b.Resources(new List<ScheduleResource> { ... });
b.EventSettings(new ScheduleEventSettings { ... });
```

### SF Schedule type names
- `Syncfusion.EJ2.Schedule.ScheduleView` (not `View`)
- `Syncfusion.EJ2.Schedule.ScheduleGroup`
- `Syncfusion.EJ2.Schedule.ScheduleResource`
- `Syncfusion.EJ2.Schedule.ScheduleEventSettings`
- `Syncfusion.EJ2.Schedule.ScheduleField` (not `EventFieldsMapping`)
- `Syncfusion.EJ2.Schedule.ScheduleFieldOptions` (not `FieldOptions`)
- `Syncfusion.EJ2.Schedule.ScheduleWorkHours`
- `Syncfusion.EJ2.Schedule.ScheduleTimeScale`

### Framework patterns used
- `Html.ReactivePlan<TModel>()` — plan creation
- `Html.On(plan, t => t.DomReady(...))` — initial data load
- `Html.InputField(plan, m => m.Prop, o => o.Label(...)).FusionDropDownList(...)` — typed input
- `.Fields<T>(t => t.Text, v => v.Value)` — typed field mapping (NOT SF FieldSettings object)
- `.Reactive(plan, evt => evt.Changed, (args, p) => {...})` — event wiring on input
- `.Reactive(evt => evt.CellClicked, (args, p) => {...})` — event wiring on display
- `p.Get(...).Gather(g => g.Include<C, M>(m => m.Prop)).Response(...)` — HTTP with gather
- `s.Component<FusionSchedule>("id").SetDataSource(json, j => j.Items)` — data injection
- `p.When(args, x => x.RequestType).Eq("eventChanged").Then(...)` — conditional pipeline
- `p.Element("id").SetText(...)` — DOM updates
- `Html.NativeButton("id", "text").Reactive(plan, evt => evt.Click, ...)` — button events
- `Html.RenderPlan(plan)` — plan rendering

### Existing sandbox patterns for reference
- **Wizard** (`Areas/Sandbox/Views/Conditions/AdmissionWizard/`): Multi-step with `Into()` partial loading
- **Drawer** (`Areas/Sandbox/Views/Components/AppLevel/Drawer/`): `s.Into("alis-drawer-content")` with server partials
- **Grid** (`Areas/Sandbox/Views/Components/Fusion/Grid/`): Server-driven data, SetDataSource, dataStateChange
- **Cascading** (`Areas/Sandbox/Views/AllModulesTogether/Cascading/`): Include gather, SetDataSource on child

## HTML Experiment Files
- `wwwroot/sf-schedule-test.html` — Full event explorer with domain data + filters
- `wwwroot/sf-schedule-api-test.html` — Clean API payload test (proved camelCase works)
- `wwwroot/sf-schedule-editor-test.html` — Custom editorTemplate with SF sub-components
- `wwwroot/sf-tooltip-test.html` — Tooltip event explorer
- `wwwroot/sf-dialog-test.html` — Dialog event explorer

### 10. PreventDefault on PopupOpen — cancel SF editor to use our form
- **How**: Added `FusionSchedulePopupOpenArgsExtensions.PreventDefault(args, pipeline)`
- **Pattern**: Same as `FusionAutoCompleteFilteringArgs.PreventDefault` — sets `evt.cancel = true`
- **Evidence**: AutoComplete Filtering sandbox proves the pattern works end-to-end
- **Code**: `args.PreventDefault(t)` inside `When(args.Type).Eq("Editor").Then(...)`

### 11. Wizard Into() pattern — partial views with their own ReactivePlan
- **How**: Wizard loads step partials via `s.Into("step-container")`, each partial has its own plan
- **Evidence**: `Areas/Sandbox/Views/Conditions/AdmissionWizard/` — multi-step with full forms per step
- **Implication**: Same pattern works for schedule editor — load assignment form partial into a container

## Strategy for End-to-End Editor Flow

1. `PopupOpen` type "Editor" → `args.PreventDefault(t)` cancels SF popup
2. Instead → GET `/Schedule/EditForm?assignmentId=X` → `Into("assignment-form")` (Drawer or section)
3. Server returns partial: `ReactivePlan<EditAssignmentModel>` with:
   - `Html.InputField` for staff dropdown (FusionDropDownList)
   - `Html.InputField` for shift dates (FusionDateTimePicker)
   - `Html.InputField` for notes (NativeTextArea)
   - FluentValidator for validation rules
   - Save button → POST with Gather + Validate
   - Cancel button → hide form
   - `RenderPlan(plan)` — plan merges into page
4. Save POST → on success → GET assignments → SetDataSource → schedule refreshes
5. Unassigned count updates

## BLOCKER: PopupOpen When condition not matching in sandbox

The `When(args, x => x.Type).Eq("Editor")` condition in the PopupOpen reactive handler
is NOT executing. Console shows `popupOpen` event fires (registered at boot), but no
`[alis:execute]` entries for PreventDefault or Drawer commands after double-click.

**What works:** JS experiment proves `args.cancel = true` in `popupOpen` cancels the editor.
**What fails:** Framework's condition evaluation doesn't match the event arg value.

**Debugging needed:**
1. Check the plan JSON — how does the When condition serialize? What path does `x => x.Type` resolve to?
2. Check SF event args at runtime — what property name does SF use? `type`? `Type`?
3. Compare with AutoComplete Filtering PreventDefault — how does its condition match?
4. The ExpressionPathHelper may resolve `x => x.Type` to `evt.type` but SF may use `args.type` differently

**Built but not yet proven:**
- EditAssignmentModel + StaffOption (model)
- ScheduleController.EditForm endpoint (returns partial)
- _EditAssignmentForm.cshtml partial (ReactivePlan, InputField, Gather, Validate pattern)
- PreventDefault extension on FusionSchedulePopupOpenArgs

## Next Steps (in order)
1. **DEBUG**: Inspect plan JSON for PopupOpen behavior — check condition path and value
2. **DEBUG**: Add JS console.log in runtime to see what SF passes as event args
3. **FIX**: Get PreventDefault + When condition working
4. **PROVE**: Drawer opens with our InputField form
5. **WIRE**: Save button → POST with Gather + Validate → refresh schedule
6. Test week navigation, print, view switching
7. Build full AC list
8. Write BDD Playwright tests
