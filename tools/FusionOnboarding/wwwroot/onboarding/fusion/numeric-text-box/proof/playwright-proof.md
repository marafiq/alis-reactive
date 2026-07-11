# NumericTextBox Playwright Proof

Status: audited. Every onboarded `FusionNumericTextBox` member has focused typed
DSL Playwright proof through the Monthly Service Plan journey. Each member is
bound to a fails-when-broken assertion, recorded per member in
`proof/behavioral-coverage.json`. That behavioral coverage was verified green
against the latest run by the onboarding behavioral-coverage gate
(`verify-behavioral-coverage.mjs --component numeric-text-box` -> `[PASS]
numeric-text-box — 18/18 members mapped`), which is the authority for the
per-member outcome rather than any single static log path.

## Proof Surface

- Test file: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/NumericTextBox/WhenNumericValueEntered.cs`
- Test class: `Alis.Reactive.PlaywrightTests.Components.Fusion.NumericTextBox.WhenNumericValueEntered`
- Sandbox view: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/NumericTextBox/Index.cshtml`
- Route: `http://localhost:5220/Sandbox/Components/NumericTextBox`
- Command: `scripts/playwright.sh --filter "FullyQualifiedName~Components.Fusion.NumericTextBox.WhenNumericValueEntered"`
- Per-member fails-when-broken map: `proof/behavioral-coverage.json`

## Journey

A care coordinator sets a resident's Monthly Service Plan — how many catered
meals and wellness check-ins the resident receives each week. The plan carries
over from last month; the coordinator adjusts meals with steppers and plan
templates, lowers the plan floor for a reduced-diet plan, gets nurse-visit
guidance on wellness check-ins, then saves the plan. Each behavior is one
isolated nested vertical slice driven by real typing, real stepper/button
clicks, and real focus gestures, with no DOM poking and no `page.evaluate()`
(the gather test is the single allowed `request.PostData` assertion). Syncfusion
NumericTextBox commits typed values on blur, so typed entries are committed with
a trusted click-fill-Tab gesture.

## Member To Proof Map

| Onboarded member | Behavior (one user-visible journey) | Proving test | What breaks the test |
| --- | --- | --- | --- |
| `FusionNumericTextBox(...)` render helper | plan opens showing the meals carried over from last month | `plan_opens_showing_the_meals_carried_over_from_last_month` | the builder stops rendering the field bound to the model value (asserts the field reads `7.00`, wellness reads `2.00`, summary reads `7`) |
| `Changed` event selector | entering a new meal count updates what the resident receives | `entering_a_new_meal_count_updates_what_the_resident_receives` | the event stops firing the reactive handler when a typed value commits on blur, so the summary never updates |
| `Reactive` event wiring | entering a new meal count updates what the resident receives | `entering_a_new_meal_count_updates_what_the_resident_receives` | the `.Reactive` wiring stops connecting `change` to its plan pipeline, so committing a typed value runs no reaction |
| `FusionNumericTextBoxChangeArgs` | entering a new meal count updates what the resident receives | `entering_a_new_meal_count_updates_what_the_resident_receives` | the payload contract stops delivering the change payload, so neither field nor summary updates |
| `FusionNumericTextBoxChangeArgs.Value` | entering a new meal count updates what the resident receives | `entering_a_new_meal_count_updates_what_the_resident_receives` | `Value` stops carrying the new number (types `10`, asserts field `10.00` and summary `10`) |
| `FusionNumericTextBoxChangeArgs.PreviousValue` | lowering the meal count records what it changed from | `lowering_the_meal_count_records_what_it_changed_from` | `PreviousValue` stops carrying the value before the change (lowers carried-over `7` to `5`, asserts the previous note reads `7`) |
| `FusionNumericTextBoxChangeArgs.IsInteracted` | a meal count the coordinator typed is marked as entered | `a_meal_count_the_coordinator_typed_is_marked_as_entered` | `IsInteracted` stops reporting true for a typed value, so a typed entry is no longer marked "You entered this number of meals." (its complement is proven by the template path below) |
| `SetValue(decimal)` method | applying the standard plan sets meals to fourteen | `applying_the_standard_plan_sets_meals_to_fourteen` | `SetValue` stops writing the standard plan's count (clicks Apply the standard plan, asserts field and summary both become `14`) |
| `SetMin(decimal)` method | allowing a reduced-diet plan lets the meal count drop below the standard floor | `allowing_a_reduced_diet_lets_the_meal_count_drop_below_the_standard_floor` | `SetMin` stops lowering the minimum (proves `3` clamps to floor `4` before, then after `SetMin(2)` the same `3` sticks at `3`) |
| `Increment()` method | adding a meal raises the weekly meal count by one | `adding_a_meal_raises_the_weekly_meal_count_by_one` | `Increment` stops raising the value by one step (clicks Add a meal, asserts meals go from `7` to `8`) |
| `Decrement()` method | removing a meal lowers the weekly meal count by one | `removing_a_meal_lowers_the_weekly_meal_count_by_one` | `Decrement` stops lowering the value by one step (clicks Remove a meal, asserts meals go from `7` to `6`) |
| `Focus` event selector | selecting the wellness field shows guidance on check-ins | `selecting_the_wellness_field_shows_guidance_on_check_ins` | the `focus` event stops firing on focus gain, so the nurse-visit guidance never replaces the prompt |
| `FusionNumericTextBoxFocusArgs` | selecting the wellness field shows guidance on check-ins | `selecting_the_wellness_field_shows_guidance_on_check_ins` | the focus payload contract stops being delivered, so focusing the field runs no reaction |
| `Blur` event selector | leaving the wellness field tidies the guidance | `leaving_the_wellness_field_tidies_the_guidance` | the `blur` event stops firing on focus loss, so the guidance never changes to the saved-state note |
| `FusionNumericTextBoxBlurArgs` | leaving the wellness field tidies the guidance | `leaving_the_wellness_field_tidies_the_guidance` | the blur payload contract stops being delivered, so leaving the field runs no reaction |
| `FocusIn()` method | start entering wellness moves the cursor into the field | `start_entering_wellness_moves_the_cursor_into_the_field` | `FocusIn` stops moving the cursor into the field (clicks Start entering wellness check-ins, asserts the field becomes focused and its Focus-driven guidance appears) |
| `FocusOut()` method | done with wellness moves the cursor out of the field | `done_with_wellness_moves_the_cursor_out_of_the_field` | `FocusOut` stops taking the cursor out (clicks Done with wellness check-ins, asserts the field is no longer focused and its Blur-driven note appears) |
| `Value(...)` read source | saving the plan posts the meal count to the server | `saving_the_plan_posts_the_meal_count_to_the_server` | the `Value` source stops yielding the current count into the gather body (asserts the POST carries `"mealsPerWeek":12`) |

The template-applied complement of `IsInteracted` (false -> "applied from a plan
template") is additionally proven by `a_meal_count_applied_from_a_template_is_marked_as_applied`,
and the submitted-confirmation path by `saving_the_plan_confirms_the_meal_count_to_the_coordinator`.

## Proof Criteria Met

- real typing, real stepper/button clicks, and real focus gestures drive every behavior; no DOM poking or `page.evaluate()`;
- the render proof asserts the carried-over model values (`7.00` meals, `2.00` wellness) are bound at first paint;
- the `change` proof asserts the visible field value and the summary change together;
- the `previousValue` proof lowers the count and asserts the previous note reads the prior value;
- the `isInteracted` proof asserts a typed entry reads "You entered this number of meals." and a template-applied value reads "applied from a plan template.";
- the `SetMin` proof proves the clamp-to-floor before and the below-floor stick after, an assertion unsatisfiable unless the minimum actually changed;
- the `Increment`/`Decrement` proofs assert the value moves exactly one step;
- the `Focus`/`Blur`/`FocusIn`/`FocusOut` proofs assert the guidance text and the field's focus state change as expected;
- the `Value` gather proof asserts the POST body carries `"mealsPerWeek":12` under the declared key;
- every test asserts no console errors.

## Run Evidence

- `scripts/playwright.sh --filter "FullyQualifiedName~Components.Fusion.NumericTextBox.WhenNumericValueEntered"` -> `Test Run Successful. Total tests: 15  Passed: 15`.
- `node .claude/skills/onboard-fusion-component/scripts/verify-behavioral-coverage.mjs --component numeric-text-box` -> `[PASS] numeric-text-box — 18/18 members mapped, 13 proving test(s)` against the latest TRX.

## Coverage Completeness

`proof/typed-api-coverage-matrix.md` lists all 18 typed `FusionNumericTextBox`
members and is `audited` with every row `row-proven`.
`proof/behavioral-coverage.json` maps each of the 18 members to the named test
above whose assertion is unsatisfiable by that member's defect. No onboarded
member is left without a fails-when-broken assertion. One gesture
(`entering_a_new_meal_count_updates_what_the_resident_receives`) covers four
members through distinct assertions; the fan-out is declared in
`proof/behavioral-coverage.json` `acceptedFanout`.
