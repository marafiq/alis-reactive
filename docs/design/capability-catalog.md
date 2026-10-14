# Capability catalog: the release's behavior contract

Read at `e837ad94` (2026-10-14) on `feature/component-redesign`, from the sandbox journeys, their
Playwright fixtures and the source. It was first read at `a2c497b` (2026-10-11) and updated at
`f36f62e`; the exercise pages it named then became journeys with their own fixtures from `9563f1d9`
through `e837ad94`, so every proof below was found again at `e837ad94`. A proof names the test (fully
qualified), quotes the assertion that fails when the capability breaks and gives its `file:line`; where
no test proves a capability, the entry says so. Every fixture at `e837ad94` derives `PlaywrightTestBase`
and drives a page: no plan-only fixture is left. This catalog was read, not run; CI ran every test at `e837ad94`:
run [38568507905](https://github.com/marafiq/alis-reactive/actions/runs/38568507905) succeeded, 1,583
tests passing at the first attempt and two InPlaceEditor medical-record-number tests only on a re-run
(flaky; neither is cited here), and `verify-net48`
([38568508122](https://github.com/marafiq/alis-reactive/actions/runs/38568508122)) succeeded. The component rows of [`use-cases/components.md`](use-cases/components.md)
("Status at HEAD") give the view line for each component capability.

## 1. JS objects the DSL touches

| Object | How it is reached | Members used |
|--------|-------------------|--------------|
| Component | `p.Component<T>(m => m.X)`; `Component<T, TOtherModel>(m => ..)` (another view model, e.g. a partial writing the shell's field); `Component<T>("id")`; `Component<T>()` app-level by fixed id | properties read/write, methods with typed args, returning methods, events |
| Event payload | the args parameter of a reaction, a declared object (`args.Value`, `args.Cancel.Set(true)`) | nested and indexed reads (`args.BatchChanges.Get(x => x.ChangedRecords)`, `args.Data.Get(x => x[0].Summary)`); writes (`Cancel`, `PreventDefaultAction`, BulletChart `Text`); void calls after an async gap (`args.UpdateData(found.Get(..))`, Kanban `args.CancelEdit()`) |
| Response body | `OnSuccess<T>((json, s) => ..)`, `OnError<T>(status, ..)`; `json` is the body (`Data<T>`) | typed reads (`json.Get(x => x.Name)`), nested paths, arrays, whole body (`json`) |
| Plugin | a `Plugin` declaration, `plan.RegisterPlugin<T>()`; registered by the app bundle on `window.__alisPlugins` | typed properties (`visitLog.Entries`), functions with args (`slugify.Slug(..)`), commands (`visitLog.Record(..)`) |
| Element | `p.Element("id")` | `Text.Set`, `Html.Set`, `ClassList.Add/Remove/Toggle`, `Show`/`Hide` |
| Array element | `p.From(...)`, `p.Element(id).Children`, `p.Element(id).ClassList.Items` | per-element method reads (`GetAttribute`, `StartsWith`, `EndsWith`) |

The plan and runtime treat components and plugins alike: the runtime locates either as a plan object
(`Alis.Reactive.Assets/runtime/objects/world.ts:14-18`), and in C# both are `JsObject` declarations bound through
`ComponentBinding` and `PluginBinding` (`Alis.Reactive/Components/Contracts/ObjectBinding.cs:57-72, 89-100`). Event payloads are read by path,
through `EventPayloadBinding` (`Alis.Reactive/Components/Contracts/ObjectBinding.cs:78-86`).

## 2. Capabilities and their proofs

Test names below are fully qualified; files are under `tests/Alis.Reactive.PlaywrightTests/`.

**Component members**
- Property write from a literal: Alis.Reactive.PlaywrightTests.Components.Fusion.DateTimePicker.WhenDateTimeSelected.taking_the_held_time_fills_it_in_as_the_offices_hold_and_the_clinic_confirms_she_is_in — `await Expect(Appointment.Input).ToHaveValueAsync("11/12/2026 2:00 PM");` (tests/Alis.Reactive.PlaywrightTests/Components/Fusion/DateTimePicker/WhenDateTimeSelected.cs:89).
- Property write from a response: Grid, Kanban, Schedule, Pivot data sources; Schedule and Pivot write a property of an object they hold (`schedule.EventSettings.DataSource`, `pivot.DataSourceSettings.DataSource`). Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridKeyedUpdate.the_board_lists_the_wings_residents_for_the_evening — `await Expect(Rows.First).ToContainTextAsync("Martha Collins");` (tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridKeyedUpdate.cs:52); Alis.Reactive.PlaywrightTests.Components.Fusion.Schedule.WhenUsingFusionSchedule.the_week_opens_with_its_shifts_and_how_many_need_coverage — `await Expect(StaffedShifts.First).ToBeVisibleAsync(new() { Timeout = 10000 });` (tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Schedule/WhenUsingFusionSchedule.cs:206); Alis.Reactive.PlaywrightTests.Components.Fusion.PivotView.WhenUsingFusionPivotView.loading_october_moves_the_report_to_the_latest_three_months — `await Expect(Report.ColumnHeading("October")).ToBeVisibleAsync(new() { Timeout = 10000 });` (tests/Alis.Reactive.PlaywrightTests/Components/Fusion/PivotView/WhenUsingFusionPivotView.cs:173); Kanban `Cards<T>()`: components row 66.
- Property write from a payload into another component: Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridBatchChangeReview.reviewing_lists_each_changed_resident_with_the_new_checks — `await Expect(ChangeToSave("Henry Liu").Locator($"td[aria-colindex='{ChecksColumn}']")).ToHaveTextAsync("4");` (tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridBatchChangeReview.cs:101).
- Property write from another component's read through arrays (`Rows<T>()` → `Where` → `OrderBy` → `DataSource.Set`): Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenBindingArrayToGrid.showing_active_only_narrows_the_roster_without_asking_the_server_again — `await Expect(Rows).ToHaveCountAsync(3, new() { Timeout = 10000 });` (tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenBindingArrayToGrid.cs:61).
- Method with typed literal or object args: Grid `AddRecord<T>(row)`: Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridResidentAdmissionAudit.moving_zara_in_adds_her_to_the_roster_and_names_the_change — `await Expect(Roster.Rows.First).ToContainTextAsync("Zara Ahmed");` (tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridResidentAdmissionAudit.cs:47); Mention `Search(text, x, y)`: Alis.Reactive.PlaywrightTests.Components.Fusion.Mention.WhenUsingFusionMention.finding_a_day_nurse_lists_only_the_nurses_whose_names_hold_the_typed_word — `await Expect(DayShift.Popup).ToHaveCSSAsync("left", "64px");` (tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Mention/WhenUsingFusionMention.cs:156); ListBox `SelectItems([..], bool, bool)` and Kanban `AddColumn`: components rows 36 and 42.
- Method with field-expression args (typed columns): Grid `SortColumn`: Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.highest_risk_first_replaces_the_name_sort_with_the_high_risk_residents_on_top — `await Expect(Change).ToHaveTextAsync("Sorted by risk, highest first.", new() { Timeout = 10000 });` (tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridDirectory.cs:125); `FilterByColumn`: Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.the_north_wing_view_lists_only_north_wing_residents — `await Expect(Change).ToHaveTextAsync("Filtered: wing is \"North\".");` (tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridDirectory.cs:155); `GroupColumn`: Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.grouping_by_care_level_lists_every_resident_under_their_care_level — `await Expect(Change).ToHaveTextAsync("Grouped by care level.");` (tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridDirectory.cs:253); `EditCell`: Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridShiftTaskTally.starting_the_tally_opens_the_first_residents_open_tasks — `await Expect(Editor("openTasks")).ToHaveValueAsync("2");` (tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridShiftTaskTally.cs:71); `SetCellValue`: components rows 38 and 41.
- Method args from a response: Grid `AddRecordAt(json, 0)` and `SetRowData`: components row 35; Kanban `AddCard`, `UpdateCard`, `DeleteCard`: Alis.Reactive.PlaywrightTests.Components.Fusion.Kanban.WhenUsingFusionKanban.the_front_desks_urgent_call_goes_to_the_top_of_open_once_the_work_order_system_has_it, Alis.Reactive.PlaywrightTests.Components.Fusion.Kanban.WhenUsingFusionKanban.when_the_part_arrives_the_wheelchair_repair_resumes_and_the_waiting_column_goes_away, Alis.Reactive.PlaywrightTests.Components.Fusion.Kanban.WhenUsingFusionKanban.filing_away_finished_work_clears_the_done_column (assertions not quoted here); Pivot `LoadPersistData`: components row 50.
- Returning method as a source: RadioButton `GetSelectedValue()` into a condition: components row 64; Grid `GetSelectedRecords<TRow>()`, `GetSelectedRowIndexes()` and `GetRowIndexByPrimaryKey(..)`: components rows 61 and 62; `GetCurrentViewRecords<TRow>()` into an array and a returning method in a condition: components row 68; Schedule `GetEvents<T>()`: components row 61; ChipList `GetSelectedChips()` into a `Dispatch` payload, then `p.From(..).Any/Count`: components row 65; a single-selection ChipList's `GetSelectedChip().Text` gathered into every request that answers with the board (`4cae6570`): components row 137; BulletChart `GetActualIndex(..)` into a condition: components row 63. `GetBatchChanges<TRow>()` is not mapped here.
- Programmatic writes fire change with `IsInteracted = false` and journeys branch on it: TextBox: Alis.Reactive.PlaywrightTests.Components.Fusion.TextBox.WhenUsingFusionTextBox.filling_the_legal_name_records_it_as_filled_from_the_record_not_a_manual_edit — `await Expect(EditSource).ToHaveTextAsync("Filled from the resident's record.");` (tests/Alis.Reactive.PlaywrightTests/Components/Fusion/TextBox/WhenUsingFusionTextBox.cs:105); NumericTextBox: Alis.Reactive.PlaywrightTests.Components.Fusion.NumericTextBox.WhenNumericValueEntered.a_meal_count_applied_from_a_template_is_marked_as_applied — `await Expect(MealsSource).ToHaveTextAsync("This number of meals was applied from a plan template.");` (tests/Alis.Reactive.PlaywrightTests/Components/Fusion/NumericTextBox/WhenNumericValueEntered.cs:108); OtpInput: Alis.Reactive.PlaywrightTests.Components.Fusion.OtpInput.WhenUsingFusionOtpInput.a_walk_in_code_is_not_checked_against_todays_visits — `await Expect(Status).ToHaveTextAsync("Walk-in code issued. It opens the front door today only.");` (tests/Alis.Reactive.PlaywrightTests/Components/Fusion/OtpInput/WhenUsingFusionOtpInput.cs:233). TimePicker, TextArea, Slider and Rating are not mapped here.
- A component calls its own method inside its own handler (OtpInput `ValueChanged` → `FocusOut`): components row 105.
- Write then read in one branch (CheckBox `Disabled.Set(false)` then `Disabled`): components row 106.

**Event payloads**
- Reads, nested, indexed, nullable, typed `TRow` (Grid `RecordClick<TRow>`, Kanban `<TCard>`): components rows 91, 92 and 93.
- Writes: `Cancel` on Grid `BeginEdit`: components row 95; on Grid `BeforeBatchSave`: Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridBatchChangeReview.reviewing_keeps_every_change_pending_until_it_is_saved — `await Expect(Checks("Henry Liu")).ToHaveClassAsync(Changed);` (tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridBatchChangeReview.cs:115); on Stepper `StepChanging`: Alis.Reactive.PlaywrightTests.Components.Fusion.Stepper.WhenUsingFusionStepperLazyWizard.the_care_plan_does_not_open_from_the_stepper_before_the_intake_is_saved — `await Expect(Steps.OpenStep).ToContainTextAsync("Intake");` (tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Stepper/WhenUsingFusionStepperLazyWizard.cs:197); on Menu `BeforeOpen`: components row 81; `PreventDefaultAction` on AutoComplete filtering: components row 99. Grid `ActionBegin` and `CellSave`, Carousel and ListView cancels are not mapped here.
- Void method with an argument from the HTTP response, called after the response returns: AutoComplete: components row 101; MultiSelect: Alis.Reactive.PlaywrightTests.Components.Fusion.MultiSelect.WhenUsingFusionMultiSelect.typing_part_of_a_supplement_offers_what_the_kitchens_catalog_matches — `await Expect(Supplements.PopupItems).ToHaveTextAsync(new[] { "Glucerna", "Glucerna Hunger Smart" });` (tests/Alis.Reactive.PlaywrightTests/Components/Fusion/MultiSelect/WhenUsingFusionMultiSelect.cs:198).
- Reactions stay synchronous so payload writes land before the vendor reads them
  (`Alis.Reactive.Assets/runtime/grammar/execute.ts:17-26`; `Alis.Reactive.Assets/runtime/plans/start.ts:46-58`).
- Native payload is the element (`event.currentTarget`), so any element property is readable;
  CustomEvent payload is `detail`; Fusion payload is the args object, `{}` when absent
  (`Alis.Reactive.Assets/runtime/objects/vendor-driver.ts:35-48`).

**HTTP**
- GET, POST, PUT, DELETE; GET writes gather to the query string: GET: Alis.Reactive.PlaywrightTests.HttpPipeline.ResidentRegister.WhenCoordinatorKeepsResidentRegister.finding_part_of_a_name_lists_every_record_that_holds_it — `await Expect(SearchCount).ToHaveTextAsync("2");` (tests/Alis.Reactive.PlaywrightTests/HttpPipeline/ResidentRegister/WhenCoordinatorKeepsResidentRegister.cs:108); PUT: Alis.Reactive.PlaywrightTests.HttpPipeline.ResidentRegister.WhenCoordinatorKeepsResidentRegister.moving_a_resident_to_an_open_room_says_where_they_now_live_and_updates_their_record — `await Expect(MoveStatus).ToHaveTextAsync("Helen Park now lives in Maple 220.");` (tests/Alis.Reactive.PlaywrightTests/HttpPipeline/ResidentRegister/WhenCoordinatorKeepsResidentRegister.cs:132); DELETE: Alis.Reactive.PlaywrightTests.HttpPipeline.ResidentRegister.WhenCoordinatorKeepsResidentRegister.removing_a_duplicate_record_takes_it_off_the_register — `await Expect(RegisterStatus).ToHaveTextAsync("Removed record 1047 for Helen Park: duplicate entry.");` (tests/Alis.Reactive.PlaywrightTests/HttpPipeline/ResidentRegister/WhenCoordinatorKeepsResidentRegister.cs:187); POST: components row 58.
- JSON is the default body; form data carries files: components row 127.
- Gather builds the typed request the endpoint binds (`Gather<TRequest>`, or `Gather(..)` for the view model):
  `Form()` and `Form(m => ..)` send the form's inputs by binding name (components rows 125 and 126);
  `Set(x => x.Member, ..)` takes a literal or any source: component member (components row 58), event payload
  (components row 103), URL query (`p.Url<TQuery>().Get(..)`, below), previous response (below), plugin
  (below), array value (components row 61); `Header(name, ..)`; a member named like a `{placeholder}`
  fills the route (components rows 59 and 94). URL query and header: Alis.Reactive.PlaywrightTests.HttpPipeline.ShiftEmailLink.WhenNurseOpensShiftEmailLink.the_link_opens_the_residents_chart_at_its_community_with_the_dose_due_on_its_shift — `await Expect(Community).ToHaveTextAsync("Maple Grove");` (tests/Alis.Reactive.PlaywrightTests/HttpPipeline/ShiftEmailLink/WhenNurseOpensShiftEmailLink.cs:146); previous response, in a chained request's header: Alis.Reactive.PlaywrightTests.Components.Brought.VisitPlanner.WhenHoldingVisitsAtTheFrontDesk.the_familys_holds_are_asked_of_the_desk_of_the_wing_the_week_names — `await Expect(DeskName).ToHaveTextAsync("Maple wing desk");` (tests/Alis.Reactive.PlaywrightTests/Components/Brought/VisitPlanner/WhenHoldingVisitsAtTheFrontDesk.cs:95); plugin, in a header: Alis.Reactive.PlaywrightTests.Plugins.ArrayManager.WhenWorkingTheWardDesk.tonights_staffing_is_planned_from_the_census_and_the_residents_in_residence — `await Expect(StaffingPlan).ToHaveTextAsync(` (tests/Alis.Reactive.PlaywrightTests/Plugins/ArrayManager/WhenWorkingTheWardDesk.cs:152).
- Typed success and error by exact status, then any status; a network failure reaches only the status-less
  error routes. 409: Alis.Reactive.PlaywrightTests.HttpPipeline.ResidentRegister.WhenCoordinatorKeepsResidentRegister.moving_a_resident_into_another_residents_room_names_who_lives_there — `await Expect(MoveStatus).ToHaveTextAsync("Maple 216 is Arthur Bell's room.");` (tests/Alis.Reactive.PlaywrightTests/HttpPipeline/ResidentRegister/WhenCoordinatorKeepsResidentRegister.cs:144); any status: Alis.Reactive.PlaywrightTests.Conditions.PharmacyReview.WhenNurseChecksNewOrderWithPharmacy.when_the_physicians_pager_is_down_the_nurse_is_told_to_call — `await Expect(Physician).ToHaveTextAsync("The page did not go through: call the physician directly.");` (tests/Alis.Reactive.PlaywrightTests/Conditions/PharmacyReview/WhenNurseChecksNewOrderWithPharmacy.cs:234); network failure: Alis.Reactive.PlaywrightTests.Components.Brought.VisitPlanner.WhenHoldingVisitsAtTheFrontDesk.when_the_desk_cannot_be_reached_the_time_stays_booked_but_is_not_held — `await Expect(Status).ToHaveTextAsync("The front desk is not answering; the time is booked here but not held.");` (tests/Alis.Reactive.PlaywrightTests/Components/Brought/VisitPlanner/WhenHoldingVisitsAtTheFrontDesk.cs:236).
- The body is read by its Content-Type (`Alis.Reactive.Assets/runtime/modules/http.ts:181-190`): JSON is
  parsed (every typed `OnSuccess<T>` above) and HTML kept as text (every `Into`, components row 12). A plain-text
  or unknown body has no journey at `e837ad94`: the ContentType page was replaced by Family Call
  (`7c373175`), whose answers are JSON.
- Chained (one follow-up, success only) and nested requests gathering from the previous response: components row 59
  and the previous-response proof above; `Parallel(..).OnAllSettled`: Alis.Reactive.PlaywrightTests.HttpPipeline.ShiftEmailLink.WhenNurseOpensShiftEmailLink.checking_with_the_pharmacy_and_the_lab_shows_each_ones_own_answer — `await Expect(PartnersStatus).ToHaveTextAsync("Pharmacy and lab checked.");` (tests/Alis.Reactive.PlaywrightTests/HttpPipeline/ShiftEmailLink/WhenNurseOpensShiftEmailLink.cs:200);
  `WhileLoading`: Alis.Reactive.PlaywrightTests.HttpPipeline.ResidentRegister.WhenCoordinatorKeepsResidentRegister.the_register_says_it_is_removing_a_record_until_the_server_answers — `await Expect(RegisterStatus).ToHaveTextAsync("Removing record 1047...");` (tests/Alis.Reactive.PlaywrightTests/HttpPipeline/ResidentRegister/WhenCoordinatorKeepsResidentRegister.cs:202); `Finally` after an abort: Alis.Reactive.PlaywrightTests.HttpPipeline.ShiftHandover.WhenChargeNurseMarksHandoverRead.the_loader_lifts_when_the_note_cannot_reach_the_server — `await Expect(Loader).ToBeHiddenAsync();` (tests/Alis.Reactive.PlaywrightTests/HttpPipeline/ShiftHandover/WhenChargeNurseMarksHandoverRead.cs:91). That the parent's Finally runs before a chained request starts is not mapped here.
- `Validate<T>(form)` before send: Alis.Reactive.PlaywrightTests.Patterns.MoveInPacket.WhenCoordinatorCompletesMoveInPacket.saving_without_the_residents_name_asks_for_it_and_saves_nothing_until_it_is_given — `Assert.That(_packetsSent, Has.Count.EqualTo(1), "A packet the page can see has no resident is not saved; the one save is the one with her name.");` (tests/Alis.Reactive.PlaywrightTests/Patterns/MoveInPacket/WhenCoordinatorCompletesMoveInPacket.cs:257); `ValidationErrors(form)` maps server errors to fields: Alis.Reactive.PlaywrightTests.Validation.RoomHold.WhenCoordinatorHoldsRoom.a_prospect_who_already_holds_a_room_is_refused_by_the_reservations_office — `.ToHaveTextAsync("Ruth Carter already holds Maple 218. Release that hold before placing another.");` (tests/Alis.Reactive.PlaywrightTests/Validation/RoomHold/WhenCoordinatorHoldsRoom.cs:112).
- `Into(id)` partial injection with slot lifecycle; injected partials carry Fusion and Native inputs, their own plans and
  realtime connections, and steer the parent page: components row 12; realtime in a partial: Alis.Reactive.PlaywrightTests.HttpPipeline.RealTime.WhenHubConnectionEstablishes.the_resident_panel_shares_the_pages_live_connection — `await Expect(PanelConnections).ToHaveTextAsync("1");` (tests/Alis.Reactive.PlaywrightTests/HttpPipeline/RealTime/WhenHubConnectionEstablishes.cs:78).
- An HTTP step holds back the steps authored after it (the MAR line is written after the pharmacy request, `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Conditions/PharmacyReview/Index.cshtml:162`): Alis.Reactive.PlaywrightTests.Conditions.PharmacyReview.WhenNurseChecksNewOrderWithPharmacy.the_order_is_not_put_on_the_mar_until_the_pharmacy_answers — `await Expect(Mar).ToHaveTextAsync("Not on the MAR yet.");` (tests/Alis.Reactive.PlaywrightTests/Conditions/PharmacyReview/WhenNurseChecksNewOrderWithPharmacy.cs:169) while the answer is held, then `await Expect(Mar).ToHaveTextAsync(Ready, new() { Timeout = 5000 });` (:171).
- ServerPush and SignalR: Alis.Reactive.PlaywrightTests.HttpPipeline.RealTime.WhenFacilityAlertBroadcasts.facility_alert_updates_the_alert_panel; Alis.Reactive.PlaywrightTests.HttpPipeline.RealTime.WhenNotificationPushArrives.notification_count_and_message_update_on_screen — `await Expect(NotifCount).ToContainTextAsync("99", new() { Timeout = 5000 });` (tests/Alis.Reactive.PlaywrightTests/HttpPipeline/RealTime/WhenNotificationPushArrives.cs:22); one connection per hub: the partial proof above; the retry indicator: Alis.Reactive.PlaywrightTests.HttpPipeline.RealTime.WhenLiveConnectionDrops.losing_the_hub_connection_shows_the_retry_indicator_and_clicking_it_restores_live_updates — `await Expect(RetryIndicator).ToBeHiddenAsync(new() { Timeout = 15000 });` (tests/Alis.Reactive.PlaywrightTests/HttpPipeline/RealTime/WhenLiveConnectionDrops.cs:103). One `EventSource` per URL is not mapped here.

**Conditions**
- Sources: payload, component property, response body, trigger payload inside response scopes,
  source-vs-source. All operators by shape; an `And` chain or an `Or` chain (a chain is one or the other, and a
  group mixes them); `Not`; `ElseIf`/`Else` first match; nested branches; `Confirm`; conditions mixed in authored
  order with sets, dispatch, HTTP, parallel, chained and plugin steps. The comparison engine has no vendor code.
  Operators at their boundaries: Alis.Reactive.PlaywrightTests.Conditions.MedicationRound.WhenCheckingDosesOnTheMedicationRound (for example `a_pulse_of_exactly_60_or_exactly_100_is_within_range`); source-vs-source: components row 57; conditions over a response routing HTTP: the HTTP-step proof above; `Confirm`: components rows 113 and 114. The other rows are in [`use-cases/conditions.md`](use-cases/conditions.md).

**Validation**
- Rule kinds and shapes (Required differs by shape and nullability), peers, conditions (FluentValidation's
  `When(x => ..)`, around a block or on one rule), nested validators and `RuleForEach(..).SetValidator(..)` over rendered items,
  hidden fields routed to the summary, live clear on `input` (in the capture phase since `d9dccd69`) and revalidate on
  `blur` and the component's commit event, submit-time validation, server errors inline or in the summary, rules for
  fields a later partial renders, DI-resolved validators, grid column rules from the same validator. Proofs:
  peers: Alis.Reactive.PlaywrightTests.Validation.Contract.WhenPartialsLoadWithValidation.equalto_compares_two_parent_fields — `await Expect(ErrorFor("ConfirmEmail")).ToContainTextAsync("must match");` (tests/Alis.Reactive.PlaywrightTests/Validation/Contract/WhenPartialsLoadWithValidation.cs:127);
  a rule's own `When`: Alis.Reactive.PlaywrightTests.Validation.MoveInReview.WhenCoordinatorReviewsMoveIn.a_fall_risk_of_seven_needs_a_reassessment_plan_and_eight_does_not — `await Expect(_plan.ErrorFor(m => m.ReassessmentPlan)).ToHaveTextAsync("A moderate fall risk needs a reassessment plan.");` (tests/Alis.Reactive.PlaywrightTests/Validation/MoveInReview/WhenCoordinatorReviewsMoveIn.cs:190);
  `RuleForEach`: Alis.Reactive.PlaywrightTests.Validation.EmergencyContacts.WhenCoordinatorRecordsEmergencyContacts.a_contact_without_a_phone_is_asked_for_one_when_the_resident_has_contacts — `await Expect(ErrorFor(m => m.Contacts[0].Phone)).ToHaveTextAsync("Phone is required.");` (tests/Alis.Reactive.PlaywrightTests/Validation/EmergencyContacts/WhenCoordinatorRecordsEmergencyContacts.cs:75);
  hidden field to the summary: Alis.Reactive.PlaywrightTests.Components.Native.WhenNurseRecordsNightRoomChecks.choosing_the_room_clears_the_missing_resident_from_the_summary_at_once — `await Expect(ValidationSummary).ToHaveTextAsync(NoResident);` (tests/Alis.Reactive.PlaywrightTests/Components/Native/WhenNurseRecordsNightRoomChecks.cs:131);
  live clear: Alis.Reactive.PlaywrightTests.Patterns.MoveInPacket.WhenCoordinatorCompletesMoveInPacket.typing_the_name_clears_the_request_for_it_at_once — `await Expect(_plan.ErrorFor(m => m.ResidentName)).ToBeHiddenAsync();` (tests/Alis.Reactive.PlaywrightTests/Patterns/MoveInPacket/WhenCoordinatorCompletesMoveInPacket.cs:288);
  revalidate on blur: Alis.Reactive.PlaywrightTests.Patterns.MoveInPacket.WhenCoordinatorCompletesMoveInPacket.emptying_the_corrected_name_asks_for_it_again_on_leaving_the_field — `await Expect(_plan.ErrorFor(m => m.ResidentName)).ToHaveTextAsync("Enter the resident's name.", new() { Timeout = 5000 });` (tests/Alis.Reactive.PlaywrightTests/Patterns/MoveInPacket/WhenCoordinatorCompletesMoveInPacket.cs:306);
  revalidate on a Fusion component's commit event: Alis.Reactive.PlaywrightTests.Components.Fusion.InputMask.WhenMaskedInputEntered.a_number_is_refused_while_its_ten_digits_are_not_all_in — `.ToHaveTextAsync("Enter all ten digits of the family member's phone number.", new() { Timeout = 5000 });` (tests/Alis.Reactive.PlaywrightTests/Components/Fusion/InputMask/WhenMaskedInputEntered.cs:65);
  server error in the summary: Alis.Reactive.PlaywrightTests.Validation.IntakeSteps.WhenCoordinatorTakesIntakeInSteps.a_family_phone_out_of_house_format_on_the_care_step_is_refused_by_the_server_with_the_same_message — `await Expect(ValidationSummary).ToHaveTextAsync(FamilyPhoneFormat, new() { Timeout = 5000 });` (tests/Alis.Reactive.PlaywrightTests/Validation/IntakeSteps/WhenCoordinatorTakesIntakeInSteps.cs:243), inline: the `ValidationErrors` proof above;
  fields a later partial renders: Alis.Reactive.PlaywrightTests.Validation.Contract.WhenAjaxPartialsLoadWithValidation.full_ajax_partial_lifecycle — `await Expect(ErrorFor("Address.Street")).ToContainTextAsync("required");` (tests/Alis.Reactive.PlaywrightTests/Validation/Contract/WhenAjaxPartialsLoadWithValidation.cs:74);
  a DI-resolved validator (`RoomRateValidator(RateSheetPolicy policy)`): Alis.Reactive.PlaywrightTests.Validation.RoomRate.WhenAdministratorAddsRoomRate.a_monthly_rate_over_the_communitys_cap_is_asked_for_in_the_rate_policys_words — `await Expect(_plan.ErrorFor(m => m.MonthlyRate)).ToHaveTextAsync("Maple Grove caps a room at $15,000 a month.");` (tests/Alis.Reactive.PlaywrightTests/Validation/RoomRate/WhenAdministratorAddsRoomRate.cs:182);
  grid column rules: Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridCareOps.more_than_seven_open_tasks_are_refused_in_the_cell — `await Expect(Page.GetByText("Open tasks run from 0 to 7.")).ToBeVisibleAsync(new() { Timeout = 10000 });` (tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridCareOps.cs:328).
  Required by shape and nullability, and submit-time validation, are proven across the journeys above; the rows are in [`use-cases/validation.md`](use-cases/validation.md).

**Arrays, dispatch, plugins, app-level**
- Arrays over payloads, responses, DOM collections and component reads. `Where`, `Count`, `Any`, `Sum`,
  `OrderByDescending`, `Find`, `Select`, `OrderBy` and `All` are each used by a journey; `Select`: components row 65;
  `OrderBy`: components row 24 (the names Ada, Cy, Ed in order, tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenBindingArrayToGrid.cs:62-64);
  `All`: Alis.Reactive.PlaywrightTests.Components.Native.ShiftReport.WhenReviewingShiftAlerts.the_shift_report_is_ready_to_sign_only_when_every_alert_is_acknowledged — `await Expect(ReadyToSign).ToBeHiddenAsync();` (tests/Alis.Reactive.PlaywrightTests/Components/Native/ShiftReport/WhenReviewingShiftAlerts.cs:201);
  `EndsWith` over DOM elements: Alis.Reactive.PlaywrightTests.Components.Native.DomOps.WhenCheckingTheLunchTables.only_low_and_reduced_sodium_diets_are_plated_from_the_low_sodium_line — `await Expect(Figure(4, "low-sodium-line")).ToHaveTextAsync("2");` (tests/Alis.Reactive.PlaywrightTests/Components/Native/DomOps/WhenCheckingTheLunchTables.cs:75);
  `ArrayContains`: components row 73.
- `Dispatch(appEvent)`, `Dispatch(appEvent, payload)`, `Dispatch(appEvent, d => d.Set(..))` from component values, `CustomEvent<T>`:
  components rows 117 (`Dispatch(CareOpsEvents.CareopsRowsRefresh)`), 60 and 65; with a payload object: Alis.Reactive.PlaywrightTests.HttpPipeline.ResidentChart.WhenCoordinatorOpensResidentChart.flagging_a_chart_for_review_names_it_by_its_medical_record_number — `await Expect(ReviewFlag).ToHaveTextAsync("MRN-20417");` (tests/Alis.Reactive.PlaywrightTests/HttpPipeline/ResidentChart/WhenCoordinatorOpensResidentChart.cs:103).
- Plugins: functions over response arrays: Alis.Reactive.PlaywrightTests.Plugins.ArrayManager.WhenWorkingTheWardDesk.only_the_residents_in_residence_are_counted_in_residence — `await Expect(InResidence).ToHaveTextAsync("3");` (tests/Alis.Reactive.PlaywrightTests/Plugins/ArrayManager/WhenWorkingTheWardDesk.cs:72); a plugin source in a condition: Alis.Reactive.PlaywrightTests.Plugins.ArrayManager.WhenWorkingTheWardDesk.a_ward_with_nobody_critical_says_so_and_a_ward_with_a_critical_resident_calls_it_out — `await Expect(SomeoneCritical).ToBeVisibleAsync();` (tests/Alis.Reactive.PlaywrightTests/Plugins/ArrayManager/WhenWorkingTheWardDesk.cs:105); in gather: the HTTP plugin proof above; a command: Alis.Reactive.PlaywrightTests.Components.Brought.VisitPlanner.WhenWatchingTheVisitBoard.opening_the_board_records_it_in_the_activity_list — `await Expect(Activity.First).ToHaveTextAsync("Board opened for this week");` (tests/Alis.Reactive.PlaywrightTests/Components/Brought/VisitPlanner/WhenWatchingTheVisitBoard.cs:111); written to a component property: components row 25.
- Drawer, Loader, Toast, Confirm by fixed id; NativeActionLink carries its own plan: components rows 107 to 117.
- C#-authored templates (`FusionTemplate` with `EventButton` → CustomEvent), grid edit templates: components rows 118 to 121.

## 3. Where the core knows about specific components or vendors

These are the dependencies the release inverts. Each is closed; the `file:line` references are at `e837ad94`.

**C# core**
- ~~Declaration APIs are internal; Fusion and Native reach them through `InternalsVisibleTo`; 24 slice
  files hand-write payload members with `ReactionGraph`~~: closed. A declaration is a public `JsObject`
  (`Alis.Reactive/Components/Contracts/JsObject.cs:35`), a payload member is a `ReadOnly<T>` or `Prop<T>`
  (`Alis.Reactive/Components/Contracts/JsMembers.cs:22, 78`), no slice names `ReactionGraph`, and
  `InternalsVisibleTo` names only the contract generator (`Alis.Reactive/Alis.Reactive.csproj:69`).
- ~~An input's value must be one named property: `IInputComponent.ValueMember` feeds registration,
  gather, validation~~: closed. An input declares `ComponentInput` (its value property and the
  event that commits it: the InPlaceEditor's `EndEdit` since `bfe0834c`, the FileUpload's `FilesChanged`
  since `beab8f92`, components row 136); a field another plan renders is read through the model by the id its
  member gives it (`Alis.Reactive/Validation/FormChecks.cs:124-127`).
- ~~`RegisteredInputValueRead.ForFusionInPlaceEditorValueRead`~~: closed; one bound read, `ValueExpression.ReadBound(inputId, shape)`,
  serves every component (`Alis.Reactive/PlanModel/Values/ValueExpression.cs:89-90, 268-269`).
- ~~`ComponentVendor.Fusion` token~~: closed by D2 (`FusionComponent.FusionVendor` declares it);
  the EJ2 projections left `ClientRule`, the public browser-rule contract a component reads back (`Alis.Reactive/Validation/ClientRule.cs:15`),
  for the grid's own slice (`Alis.Reactive.Fusion/Components/FusionGrid/FusionGridValidation.cs:106-161`).
- ~~`Include<TComponent>` reads the input's declared value; `NativeGatherExtensions` assumes NativeTextBox~~:
  closed; both were deleted with the typed request, and `Form(m => ..)` sends an input by its binding name.
- ~~Method contracts are keyed per component instance, so two arities of one member on one grid
  throw~~: closed; the member-contract registry is gone, and each call carries the argument shapes its own
  C# member declares (`Alis.Reactive/Components/Contracts/ComponentMember.cs:57, 62-63`; `Alis.Reactive/PlanModel/Reactions/Steps.cs:76-82`),
  so two C# members typing one JS method do not collide (`Alis.Reactive/Components/Contracts/JsObject.cs:255-256, 260-261`).

**Runtime**
- ~~Two hard-coded drivers and two event adapters~~: closed by D2; vendors are plan data and one
  driver follows them.
- ~~Live clearing hard-codes `"change"` and silently skips a not-ready Fusion object~~: closed. Each input
  declares the event that commits it, the model re-raises it as `alis:commit` and logs a subscription it
  cannot make (`Alis.Reactive.Assets/runtime/modules/model.ts:160-175`); the form clears a field's message on the element's
  `input` in the capture phase, ahead of the component (`d9dccd69`), and rechecks on `blur` and `alis:commit`
  (`Alis.Reactive.Assets/runtime/modules/form/form.ts:234-240`).
- ~~Injection calls `ej.base.append(nodes, container, true)`~~: closed. A slot re-creates the inserted
  `<script>` nodes itself (`Alis.Reactive.Assets/runtime/modules/slot.ts:62-74`), and the RadioGroup and CheckList scripts became
  component code (`Alis.Reactive.Assets/components/native/radio-group.ts:10-31`, `Alis.Reactive.Assets/components/native/check-list.ts:8-29`).
- ~~The request writer unwraps Syncfusion's uploader `rawFile`~~: closed; the uploader declares its files as
  `FilesData.Each(file => file.RawFile)`, read at `filesData[].rawFile` (`Alis.Reactive.Fusion/Components/FusionFileUpload/FusionFileUpload.cs:19`).
- ~~`p.Confirm()` needs `window.alis.confirm`; `root.ts` statically imports confirm, action link, drawer,
  loader~~: closed. `alis.confirm` is a plugin the Fusion component code provides (`Alis.Reactive.Assets/components/fusion/confirm.ts:104-112`),
  and `root.ts` registers only the runtime's own modules (`Alis.Reactive.Assets/runtime/root.ts:32-38`).
- The drawer clears its content on `transitionend` without unloading the slot (`Alis.Reactive.Assets/components/native/drawer.ts:48-55, 71-75`).

~~**Native builders** emit inline `<script>`, against "views carry no JavaScript"; their `SetValue` does not
sync the boxes~~: closed. The scripts are component code (`Alis.Reactive.Assets/components/native/radio-group.ts:10-31`,
`Alis.Reactive.Assets/components/native/check-list.ts:8-29`), neither declaration has a `SetValue`, and since `e4fd8d32` a written
`Value` checks exactly the boxes or the one radio it names (`check-list.ts:14-23`, `radio-group.ts:15-23`); a hidden field
the page writes raises `change` (`Alis.Reactive.Assets/components/native/hidden-field.ts:15-23`).

## 4. Proofs that were weaker than they looked

Found at `a2c497b`; each line says where it stands at `e837ad94`.

- HttpMixing else-path tests dispatched PascalCase keys through `Page.EvaluateAsync`, so every read missed:
  closed. The keys were fixed in `1ca7c9a`, the HttpMixing slice was deleted in `3705d0b3`, and no fixture
  at `e837ad94` calls `EvaluateAsync`.
- The ArrayManager send test gathered `array.Count("items")`, the count of a string: closed. The Ward Desk
  journey (`7c373175`) passes the residents to `array.Count(..)` (`Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Plugins/ArrayManager/Index.cshtml:109`),
  and its staffing test reads the plan the server makes from that header (the plugin-in-gather proof above).
- AutoComplete/MultiSelect filtering tests asserted only the endpoint name in the URL: closed; both now assert the
  server's answer for what was typed (components row 103; the MultiSelect proof in section 2).
- A 422 route's text is written by the view, not read from the body: still true of the Resident Register's 422 route
  (`Alis.Reactive.SandboxApp/Areas/Sandbox/Views/HttpPipeline/ResidentRegister/Index.cshtml:93-97`), so
  Alis.Reactive.PlaywrightTests.HttpPipeline.ResidentRegister.WhenCoordinatorKeepsResidentRegister.moving_a_resident_without_naming_the_room_asks_for_it — `await Expect(MoveStatus).ToHaveTextAsync("Choose a resident and the room they move to.");` (tests/Alis.Reactive.PlaywrightTests/HttpPipeline/ResidentRegister/WhenCoordinatorKeepsResidentRegister.cs:157)
  proves the route by status, not a read of the 422 body. The ColorPicker toggle that asserted only echo text is closed
  (components row 53).
- Plan-only fixtures (RequestInput, CapitalsNaming, NativeActionLink serialization, BehaviorGraph, ClientRules):
  none remains; every fixture drives a page. Component-method sources in gather and conditions (components rows 61 and 68),
  URL and plugin sources (components row 25 and the plugin-in-condition proof above) and event-payload gather
  (components row 94) are browser-proven.
- Once not browser-proven at all, each proven at `e837ad94`: Fusion live revalidation (the InputMask proof in section 2),
  InPlaceEditor `BeginEdit` cancel (components row 97), slot-owned rules removed on unload
  (Alis.Reactive.PlaywrightTests.Validation.IntakeSteps.WhenCoordinatorTakesIntakeInSteps.finishing_the_care_step_saves_the_intake_without_rechecking_the_resident_step — `await Expect(ValidationSummary).ToBeHiddenAsync();`, tests/Alis.Reactive.PlaywrightTests/Validation/IntakeSteps/WhenCoordinatorTakesIntakeInSteps.cs:120),
  two partials joining one plan (Alis.Reactive.PlaywrightTests.Validation.Contract.WhenPartialsLoadWithValidation.partial_field_errors_show_inline — `await Expect(ErrorFor("Address.Street")).ToContainTextAsync("required");`, tests/Alis.Reactive.PlaywrightTests/Validation/Contract/WhenPartialsLoadWithValidation.cs:54),
  Grid returning methods into arrays or conditions (components row 68), `ArrayContains` and `EndsWith`, and `Select`,
  `OrderBy` and `All` (section 2, arrays).
- Still without a journey at `e837ad94`: a plain-text or unknown response body (section 2, HTTP); one `EventSource`
  per URL, and the parent's Finally running before a chained request, which are not mapped here.
