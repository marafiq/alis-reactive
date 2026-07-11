# TimePicker Playwright Proof

Status: audited. Every onboarded `FusionTimePicker` member has focused typed DSL
Playwright proof through the morning-medication-time scheduler journey. Each
member is bound to a fails-when-broken assertion, recorded per member in
`proof/behavioral-coverage.json`. That behavioral coverage was verified green
against the latest run by the onboarding behavioral-coverage gate, which is the
authority for the per-member outcome rather than any single static log path.

## Proof Surface

- Test file: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/TimePicker/WhenTimeSelected.cs`
- Test class: `Alis.Reactive.PlaywrightTests.Components.Fusion.TimePicker.WhenTimeSelected`
- Sandbox view: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/TimePicker/Index.cshtml`
- Route: `http://localhost:5220/Sandbox/Components/TimePicker`
- Command: `scripts/playwright.sh --filter "FullyQualifiedName~Components.Fusion.TimePicker.WhenTimeSelected"`
- Per-member fails-when-broken map: `proof/behavioral-coverage.json`

## Journey

A care coordinator sets a resident's morning medication time. The page carries
over the time currently on the medication record (09:00); the coordinator picks a
new time from the time list, can apply the community's standard morning round
(08:00), can move focus in and out of the field, then confirms and schedules the
time. Each behavior is one isolated nested vertical slice driven by real clicks
and a real popup pick (`TimePickerLocator.SelectTime` clicks the `.e-time-icon`
then the `.e-list-item[data-value=...]`), with no DOM poking and no
`page.evaluate()` (the gather test is the single allowed `request.PostData`
assertion).

## Time-Format Note

The field uses a 24-hour `HH:mm` display so the picker round-trips the `HH:mm`
value `FusionTimePicker.SetValue` emits (a `h:mm a` display leaves an `HH:mm`
write unparsed, `ej2.value` null — proven in the browser). The time the
coordinator sees and works with is the picker input's display value (`HH:mm`),
which is the literal picked value, is timezone-stable, and is the real product
surface the behavior tests assert (no ISO date is shown anywhere on the page).
The `Changed` payload's `Value` is proven through the visible status line, not
a raw value echo. Only the gather body and server confirmation carry the
runtime's `toISOString` serialization, so the gather/submit assertions prove a
real time round-tripped (an ISO date in the POST body, a real `HH:mm` in the
server confirmation, not the null "unscheduled time" fallback) without pinning
the runner's offset.

## Member To Proof Map

| Onboarded member | Behavior (one user-visible journey) | Proving test | What breaks the test |
| --- | --- | --- | --- |
| `FusionTimePicker(...)` render helper | scheduler opens showing the medication time on the record | `scheduler_opens_showing_the_medication_time_on_the_record` | the builder stops rendering the field bound to the model value (asserts the input shows `09:00` carried over) |
| `Changed` event selector | choosing a medication time shows the time and marks it ready | `choosing_a_medication_time_shows_the_time_and_marks_it_ready` | the event stops firing the reactive handler when a time is picked, so the visible response and status never update |
| `Reactive` event wiring | choosing a medication time shows the time and marks it ready | `choosing_a_medication_time_shows_the_time_and_marks_it_ready` | the `.Reactive` wiring stops connecting `change` to its plan pipeline, so picking a time runs no reaction |
| `FusionTimePickerChangeArgs` | choosing a medication time shows the time and marks it ready | `choosing_a_medication_time_shows_the_time_and_marks_it_ready` | the payload contract stops delivering the change payload into the plan, so neither the time nor the status appears |
| `FusionTimePickerChangeArgs.Value` | choosing a medication time shows the time and marks it ready | `choosing_a_medication_time_shows_the_time_and_marks_it_ready` | `Value` stops carrying the newly selected time (asserts the input shows `10:30` and the NotNull branch over `args.Value` routes "This medication time is ready to confirm." — the Else branch "No medication time is set." would fire if `Value` were null, so the assertion is unsatisfiable without a delivered `Value`) |
| `FusionTimePickerChangeArgs.IsInteracted` | a time the coordinator picks is recorded as their choice | `a_time_the_coordinator_picks_is_recorded_as_their_choice` | `IsInteracted` stops distinguishing a time the coordinator chose (true, "You set this medication time.") from a programmatic write |
| `SetValue(DateTime)` method | applying the standard round writes the time and marks it system applied | `applying_the_standard_round_writes_the_time_and_marks_it_system_applied` | `SetValue` stops writing the given time onto the picker (asserts the input shows `08:00`); the same write drives the IsInteracted-false "we applied the standard morning round" message |
| `FocusIn()` method | adjusting the medication time moves focus into the field | `adjusting_the_medication_time_moves_focus_into_the_field` | `FocusIn` stops moving focus into the field (asserts the input is not focused, clicks Adjust, then asserts the input is focused) |
| `FocusOut()` method | choosing a time from the list releases focus from the field | `choosing_a_time_from_the_list_releases_focus_from_the_field` | `FocusOut` stops releasing the field after a pick (Syncfusion leaves the input focused after a list pick; the Changed reaction calls FocusOut, so the test asserts the input is NOT focused after choosing `10:30` — unsatisfiable without FocusOut) |
| `Value(...)` read source | scheduling the medication posts the time to the server | `scheduling_the_medication_posts_the_time_to_the_server` | the `Value` source stops yielding the current time into the gather body (asserts the POST carries the declared key `medicationTime` with the picker's `2026-01-01T` ISO date-time) |

## Additional Proven Behaviors

Two further tests deepen the journey without a new matrix member:

- `confirming_with_no_medication_time_warns_that_a_time_is_required` — the `Value()` condition source, empty branch: clearing the field and confirming warns the time is required (proves `Value()` is `IsEmpty()` after a clear).
- `confirming_with_the_time_on_the_record_reports_it_ready_to_schedule` — the `Value()` condition source, set branch: confirming with the carried-over time reports it ready to schedule.
- `scheduling_the_medication_sends_the_time_and_confirms_it` — the SUBMITS round-trip: the `Value()` gather feeds the POST and the server confirmation the coordinator sees reflects a real scheduled time.

## Proof Criteria Met

- real popup picks (`SelectTime`) and button clicks drive every behavior; no DOM poking or `page.evaluate()`;
- the render proof asserts the carried-over model value (`09:00`) is bound at first paint;
- the `change` proof asserts the visible input time (`10:30`) and the ready-to-confirm status the NotNull branch over the delivered `args.Value` routes — both change together, no raw value echo;
- the `IsInteracted` proof asserts the user-chosen origin message (true) and the system-applied origin message (false via `SetValue`);
- the `FocusIn` proof asserts the field gains focus; the `FocusOut` proof asserts the field loses focus after a pick (unsatisfiable without FocusOut);
- the `Value` gather proof asserts the POST body carries the picker's ISO date-time under the declared key `medicationTime`;
- the submit proof asserts the server confirmation reflects a real scheduled time (not the null "unscheduled time" fallback);
- every test asserts no console errors.

## Coverage Completeness

`proof/typed-api-coverage-matrix.md` lists all 10 typed `FusionTimePicker`
members and is `audited` with every row `row-proven`.
`proof/behavioral-coverage.json` maps each of the 10 members to the named test
above whose assertion is unsatisfiable by that member's defect, verified by
`node .claude/skills/onboard-fusion-component/scripts/verify-behavioral-coverage.mjs --component time-picker`
-> `[PASS] time-picker — 10/10 members mapped`. No onboarded member is left
without a fails-when-broken assertion.
