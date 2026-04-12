# Onboarding Plan: FusionTooltip, FusionDialog, FusionSchedule

**Date**: 2026-04-09
**Branch**: codex/final-reactive-schema
**Category**: Display components (non-input, no form value binding)
**Pattern**: 7-file vertical slice per component (matches FusionGrid, FusionAccordion, FusionTab)

## Experiments

All event payloads, method behaviors, and property effects verified in HTML test files:

- `wwwroot/sf-tooltip-test.html` — Tooltip events, dynamic content, positioning
- `wwwroot/sf-dialog-test.html` — Dialog modal/non-modal, buttons, form content, overlay
- `wwwroot/sf-schedule-test.html` — Schedule with senior-living domain data, resource grouping, filters, all events

## Phase 1: FusionTooltip

### Component Type
Display component. No form value. Wraps `ej.popups.Tooltip`.

### SF EJ2 API Surface
- **Namespace**: `ej.popups.Tooltip`
- **C# Builder**: `html.EJS().Tooltip(elementId)` → `TooltipBuilder`

### Events to Wire

| Alis Event Name | SF JS Event | Args Class | Key Fields | Verified In |
|----------------|-------------|------------|------------|-------------|
| `BeforeOpen` | `beforeOpen` | `FusionTooltipBeforeOpenArgs` | `cancel` (bool), `target` (element ref) | sf-tooltip-test.html: fires before tooltip shows, cancel=true prevents |
| `BeforeClose` | `beforeClose` | `FusionTooltipBeforeCloseArgs` | `cancel` (bool) | sf-tooltip-test.html: fires before tooltip hides |
| `Opened` | `open` | `FusionTooltipOpenedArgs` | (none — notification only) | sf-tooltip-test.html: fires after visible |
| `Closed` | `close` | `FusionTooltipClosedArgs` | (none — notification only) | sf-tooltip-test.html: fires after hidden |
| `BeforeRender` | `beforeRender` | `FusionTooltipBeforeRenderArgs` | `cancel` (bool) | sf-tooltip-test.html: fires before content renders, used to set dynamic content |

### Methods to Expose (via ComponentRef extensions)

| Method | SF JS Method | Params | Verified Behavior |
|--------|-------------|--------|-------------------|
| `Open` | `open(target)` | target element | Opens tooltip on specified element programmatically |
| `Close` | `close()` | none | Closes tooltip |
| `Refresh` | `refresh()` | none | Re-renders tooltip in current position |

### Builder Properties (set at render time)

| Property | SF Property | Type | Default | Notes |
|----------|-----------|------|---------|-------|
| `Position` | `position` | string | `"TopCenter"` | TopLeft, TopCenter, TopRight, BottomLeft, etc. |
| `OpensOn` | `opensOn` | string | `"Hover"` | Hover, Click, Focus, Custom |
| `Content` | `content` | string | — | Static HTML content |
| `Target` | `target` | string | — | CSS selector for target elements |
| `ShowTipPointer` | `showTipPointer` | bool | true | Arrow pointer |
| `IsSticky` | `isSticky` | bool | false | Stays open until explicitly closed |
| `Width` | `width` | string | "auto" | |
| `CssClass` | `cssClass` | string | — | Custom styling |

### File Inventory

```
Alis.Reactive.Fusion/Components/FusionTooltip/
├── FusionTooltip.cs                          ← sealed class : FusionComponent
├── FusionTooltipBuilder.cs                   ← wraps TooltipBuilder.Render()
├── FusionTooltipEvents.cs                    ← sealed singleton, 5 TypedEvent properties
├── FusionTooltipExtensions.cs                ← Open(), Close(), Refresh() on ComponentRef
├── FusionTooltipHtmlExtensions.cs            ← Html.FusionTooltip(plan, id, build)
├── FusionTooltipReactiveExtensions.cs        ← .Reactive(evt => evt.BeforeOpen, ...)
└── Events/
    ├── FusionTooltipOnBeforeOpen.cs           ← cancel, target
    ├── FusionTooltipOnBeforeClose.cs          ← cancel
    ├── FusionTooltipOnOpened.cs               ← empty
    ├── FusionTooltipOnClosed.cs               ← empty
    └── FusionTooltipOnBeforeRender.cs         ← cancel
```

---

## Phase 2: FusionDialog

### Component Type
Display component. No form value. Wraps `ej.popups.Dialog`.

### SF EJ2 API Surface
- **Namespace**: `ej.popups.Dialog`
- **C# Builder**: `html.EJS().Dialog(elementId)` → `DialogBuilder`

### Events to Wire

| Alis Event Name | SF JS Event | Args Class | Key Fields | Verified In |
|----------------|-------------|------------|------------|-------------|
| `BeforeOpen` | `beforeOpen` | `FusionDialogBeforeOpenArgs` | `cancel` (bool) | sf-dialog-test.html: fires before dialog opens, cancel=true prevents |
| `BeforeClose` | `beforeClose` | `FusionDialogBeforeCloseArgs` | `cancel` (bool), `isInteracted` (bool) | sf-dialog-test.html: isInteracted=true when user closes via X or overlay |
| `Opened` | `open` | `FusionDialogOpenedArgs` | (notification only) | sf-dialog-test.html: fires after dialog visible |
| `Closed` | `close` | `FusionDialogClosedArgs` | (notification only) | sf-dialog-test.html: fires after dialog hidden |
| `OverlayClick` | `overlayClick` | `FusionDialogOverlayClickArgs` | (notification only) | sf-dialog-test.html: fires when modal overlay clicked |

### Methods to Expose

| Method | SF JS Method | Params | Verified Behavior |
|--------|-------------|--------|-------------------|
| `Show` | `show()` | none | Shows the dialog |
| `Hide` | `hide()` | none | Hides the dialog |
| `Refresh` | `refresh()` | none | Recalculates position |

### Builder Properties

| Property | SF Property | Type | Default | Notes |
|----------|-----------|------|---------|-------|
| `Header` | `header` | string | — | Title text |
| `Content` | `content` | string | — | Body content (HTML string or element ref) |
| `IsModal` | `isModal` | bool | false | Modal overlay |
| `ShowCloseIcon` | `showCloseIcon` | bool | false | X button in header |
| `Width` | `width` | string | "100%" | |
| `Height` | `height` | string | "auto" | |
| `Visible` | `visible` | bool | false | Initial visibility |
| `CssClass` | `cssClass` | string | — | |
| `Buttons` | `buttons` | array | — | Footer action buttons |
| `Position` | `position` | object | center | { X, Y } |
| `AnimationSettings` | `animationSettings` | object | — | Open/close animation |

### File Inventory

```
Alis.Reactive.Fusion/Components/FusionDialog/
├── FusionDialog.cs                           ← sealed class : FusionComponent
├── FusionDialogBuilder.cs                    ← wraps DialogBuilder.Render()
├── FusionDialogEvents.cs                     ← sealed singleton, 5 TypedEvent properties
├── FusionDialogExtensions.cs                 ← Show(), Hide(), Refresh() on ComponentRef
├── FusionDialogHtmlExtensions.cs             ← Html.FusionDialog(plan, id, build)
├── FusionDialogReactiveExtensions.cs         ← .Reactive(evt => evt.BeforeOpen, ...)
└── Events/
    ├── FusionDialogOnBeforeOpen.cs            ← cancel
    ├── FusionDialogOnBeforeClose.cs           ← cancel, isInteracted
    ├── FusionDialogOnOpened.cs                ← empty
    ├── FusionDialogOnClosed.cs                ← empty
    └── FusionDialogOnOverlayClick.cs          ← empty
```

---

## Phase 3: FusionSchedule

### Component Type
Display component. No form value. Wraps `ej.schedule.Schedule`.

### SF EJ2 API Surface
- **Namespace**: `ej.schedule.Schedule`
- **C# Builder**: `html.EJS().Schedule(elementId)` → `ScheduleBuilder`

### Events to Wire

| Alis Event Name | SF JS Event | Args Class | Key Fields | Verified In |
|----------------|-------------|------------|------------|-------------|
| `CellClicked` | `cellClick` | `FusionScheduleCellClickArgs` | `startTime` (DateTime), `endTime` (DateTime), `groupIndex` (int), `isAllDay` (bool) | sf-schedule-test.html: fires on cell click with exact time slot + resource group index |
| `EventClicked` | `eventClick` | `FusionScheduleEventClickArgs` | (event data comes via `select` event, not directly on eventClick) | sf-schedule-test.html: fires with element ref, event data in preceding select |
| `ActionBegin` | `actionBegin` | `FusionScheduleActionBeginArgs` | `requestType` (string: "eventCreate"/"eventChange"/"eventRemove"/"dateNavigate"/"viewNavigate"), `cancel` (bool) | sf-schedule-test.html: fires before every scheduler action |
| `ActionComplete` | `actionComplete` | `FusionScheduleActionCompleteArgs` | `requestType` (string), `addedRecords` (array), `changedRecords` (array), `deletedRecords` (array) | sf-schedule-test.html: fires after action completes with affected records |
| `Navigating` | `navigating` | `FusionScheduleNavigatingArgs` | `action` (string: "date"/"view"), `currentDate` (DateTime), `previousDate` (DateTime), `cancel` (bool) | sf-schedule-test.html: fires before date/view navigation |
| `PopupOpen` | `popupOpen` | `FusionSchedulePopupOpenArgs` | `type` (string: "QuickInfo"/"Editor"/"DeleteAlert"), `cancel` (bool) | sf-schedule-test.html: fires before any popup, type distinguishes QuickInfo vs Editor modal |
| `PopupClose` | `popupClose` | `FusionSchedulePopupCloseArgs` | `type` (string) | sf-schedule-test.html: fires when popup closes |
| `DataBound` | `dataBound` | `FusionScheduleDataBoundArgs` | (notification only) | sf-schedule-test.html: fires after data loaded and rendered |
| `EventRendered` | `eventRendered` | `FusionScheduleEventRenderedArgs` | `cancel` (bool) | sf-schedule-test.html: fires per event, used to add CSS class for unassigned (red) |

### Methods to Expose

| Method | SF JS Method | Params | Verified Behavior |
|--------|-------------|--------|-------------------|
| `AddEvent` | `addEvent(data)` | event object | Adds event to schedule, triggers actionBegin/actionComplete |
| `SaveEvent` | `saveEvent(data)` | event object | Updates existing event |
| `DeleteEvent` | `deleteEvent(id)` | event id | Removes event by ID |
| `OpenEditor` | `openEditor(data, action)` | data + "Save"/"Add" | Opens built-in editor modal |
| `CloseEditor` | `closeEditor()` | none | Closes editor modal |
| `GetEvents` | `getEvents()` | none | Returns all events array |
| `GetCurrentViewEvents` | `getCurrentViewEvents()` | none | Returns visible events only |
| `RefreshEvents` | `refreshEvents()` | none | Re-renders all events |
| `Print` | `print()` | none | Prints current view |
| `ChangeCurrentView` | `currentView = x; dataBind()` | view name | Switches to Day/Week/Month/Agenda/Timeline |

### Builder Properties

| Property | SF Property | Type | Default | Notes |
|----------|-----------|------|---------|-------|
| `CurrentView` | `currentView` | enum | Week | Day, Week, WorkWeek, Month, Agenda, TimelineDay, TimelineWeek |
| `SelectedDate` | `selectedDate` | DateTime | today | |
| `Views` | `views` | list | all | Which view tabs to show |
| `Resources` | `resources` | list | — | Resource groups (shifts, staff, rooms) |
| `Group` | `group` | object | — | Resource grouping hierarchy |
| `EventSettings` | `eventSettings` | object | — | DataSource + field mappings + tooltip |
| `WorkHours` | `workHours` | object | 9-18 | Highlighted business hours |
| `StartHour` | `startHour` | string | "00:00" | Visible range start |
| `EndHour` | `endHour` | string | "23:00" | Visible range end |
| `TimeScale` | `timeScale` | object | 60/2 | Interval + slot count |
| `ReadOnly` | `readonly` | bool | false | |
| `ShowHeaderBar` | `showHeaderBar` | bool | true | |
| `ShowTimeIndicator` | `showTimeIndicator` | bool | true | |
| `AllowDragAndDrop` | `allowDragAndDrop` | bool | true | |
| `EnableTooltip` | `eventSettings.enableTooltip` | bool | false | Hover tooltip on events |
| `TooltipTemplate` | `eventSettings.tooltipTemplate` | string | — | Custom tooltip HTML template |

### File Inventory

```
Alis.Reactive.Fusion/Components/FusionSchedule/
├── FusionSchedule.cs                         ← sealed class : FusionComponent
├── FusionScheduleBuilder.cs                  ← wraps ScheduleBuilder.Render()
├── FusionScheduleEvents.cs                   ← sealed singleton, 9 TypedEvent properties
├── FusionScheduleExtensions.cs               ← AddEvent(), DeleteEvent(), Print(), etc.
├── FusionScheduleHtmlExtensions.cs           ← Html.FusionSchedule(plan, id, build)
├── FusionScheduleReactiveExtensions.cs       ← .Reactive(evt => evt.CellClicked, ...)
└── Events/
    ├── FusionScheduleOnCellClick.cs           ← startTime, endTime, groupIndex, isAllDay
    ├── FusionScheduleOnEventClick.cs          ← (element ref only — data via select)
    ├── FusionScheduleOnActionBegin.cs         ← requestType, cancel
    ├── FusionScheduleOnActionComplete.cs      ← requestType
    ├── FusionScheduleOnNavigating.cs          ← action, currentDate, previousDate, cancel
    ├── FusionScheduleOnPopupOpen.cs           ← type, cancel
    ├── FusionScheduleOnPopupClose.cs          ← type
    ├── FusionScheduleOnDataBound.cs           ← empty
    └── FusionScheduleOnEventRendered.cs       ← cancel
```

---

## Phase 4: Sandbox Page (`/Sandbox/Schedule`)

### Domain Model

```csharp
public class PointInTimeScheduleModel
{
    public string? SelectedFacilityId { get; set; }
    public string? SelectedShiftFilter { get; set; }
    public string? SelectedStaffFilter { get; set; }
    public DateTime ScheduleDate { get; set; } = DateTime.Today;
}
```

### Controller

```csharp
public class ScheduleController : Controller
{
    public IActionResult Index() => View(new PointInTimeScheduleModel());

    [HttpGet("/api/schedule/assignments")]
    public IActionResult GetAssignments(string facilityId, DateTime weekStart)
    {
        // Return shift assignments as JSON for the schedule dataSource
        return Json(FakeScheduleData.GetAssignments(facilityId, weekStart));
    }
}
```

### View Behavior (Reactive Plan)

1. **DomReady** → Load initial assignments via GET `/api/schedule/assignments`
2. **Facility DropDown changed** → Re-fetch assignments for new facility
3. **Schedule CellClicked** → Open FusionDialog with "New Assignment" form
4. **Schedule EventClicked** → Open FusionDialog with "Edit Assignment" form
5. **Schedule Navigating** → Fetch assignments for new date range
6. **Dialog Save button** → POST to `/api/schedule/assign`, refresh schedule
7. **Unassigned events** → Red via eventRendered
8. **Print button** → Call schedule.Print()
9. **Hover on events** → Tooltip shows care items, staff details, estimated time

### Proves

- FusionSchedule renders with resource grouping (3 shifts)
- Events fire and carry correct data through reactive pipeline
- FusionDialog opens/closes programmatically from pipeline
- FusionTooltip shows dynamic content on event hover
- HTTP pipeline loads data, refreshes schedule
- Input components (FusionDropDownList) drive schedule filters
- All via the plan — zero inline JavaScript

---

## Execution Order

1. Onboard FusionTooltip (simplest, 5 events, 3 methods)
2. Onboard FusionDialog (5 events, 3 methods, commonly needed)
3. Onboard FusionSchedule (9 events, 10 methods, uses Tooltip + Dialog)
4. Build Sandbox page proving all 3 together
5. Write Playwright tests for the Sandbox page

## Rules

- Zero TS runtime changes — if TS changes seem needed, the C# plan model is missing information
- `resolution/contracts.ts` owns all SF vendor knowledge
- Each component is a self-contained vertical slice — no cross-component dependencies in the C# layer
- Builder constructors MUST be internal
- Use `onboard-fusion-display` skill for each component
- Test with `dotnet test` + `npm test` after each component
