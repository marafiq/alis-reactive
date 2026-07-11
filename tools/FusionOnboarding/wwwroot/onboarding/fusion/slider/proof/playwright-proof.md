# Slider Playwright Proof

Status: audited. Every onboarded `FusionSlider` member has focused typed DSL
Playwright proof through the Comfort & Care Preferences journey. Each member is
bound to a fails-when-broken assertion, recorded per member in
`proof/behavioral-coverage.json`. That behavioral coverage was verified green
against the latest run by the onboarding behavioral-coverage gate, which is the
authority for the per-member outcome rather than any single static log path.

## Proof Surface

- Test file: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Slider/WhenUsingFusionSlider.cs`
- Test class: `Alis.Reactive.PlaywrightTests.Components.Fusion.Slider.WhenUsingFusionSlider`
- Test-infra locator: `tests/Alis.Reactive.Playwright.Extensions/FusionSliderLocator.cs`
- Sandbox view: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Slider/Index.cshtml`
- Route: `http://localhost:5220/Sandbox/Components/Slider`
- Command: `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Slider.WhenUsingFusionSlider"`
- Per-member fails-when-broken map: `proof/behavioral-coverage.json`

## Journey

A resident sets their Comfort & Care Preferences. The page carries over the room
temperature and afternoon rest window saved last month. The resident adjusts the
temperature by nudging the slider handle, can apply the care team's recommended
temperature, sets the afternoon rest window, then saves. Each behavior is one
isolated nested vertical slice driven by real handle gestures (a trusted handle
click then a keyboard ArrowRight, which EJ2 routes to its change/changed events)
and real button clicks, with no DOM poking and no `page.evaluate()` (the gather
test is the single allowed `request.PostData` assertion).

## Member To Proof Map

| Onboarded member | Behavior (one user-visible journey) | Proving test | What breaks the test |
| --- | --- | --- | --- |
| `FusionSlider(...)` render helper | preferences open showing the temperature carried over from last month | `preferences_open_showing_the_temperature_carried_over_from_last_month` | the builder stops rendering the slider bound to the model value (asserts `aria-valuenow=68` and the visible reading `68`) |
| `Change` event selector | warming the room updates the live reading and the comfort note | `warming_the_room_updates_the_live_reading_and_the_comfort_note` | the Change event stops firing as the handle moves, so the live reading and comfort note never update |
| `Reactive` event wiring | warming the room updates the live reading and the comfort note | `warming_the_room_updates_the_live_reading_and_the_comfort_note` | the `.Reactive` wiring stops connecting `change` to its plan pipeline, so nudging the handle runs no reaction |
| `FusionSliderChangeArgs` | warming the room updates the live reading and the comfort note | `warming_the_room_updates_the_live_reading_and_the_comfort_note` | the payload contract stops delivering the change payload into the plan, so neither the reading (Text) nor the comfort branch (Value) updates |
| `FusionSliderChangeArgs.Value` | warming the room updates the live reading and the comfort note | `warming_the_room_updates_the_live_reading_and_the_comfort_note` | `Value` stops carrying the slider value into the comfort-zone condition (nudges to 70 and asserts the >=66 mid-range message) |
| `FusionSliderChangeArgs.Text` | warming the room updates the live reading and the comfort note | `warming_the_room_updates_the_live_reading_and_the_comfort_note` | `Text` stops carrying the formatted value into the live reading (nudges to 70 and asserts the reading reads `70`) |
| `Changed` event selector | adjusting the temperature records what it changed from | `adjusting_the_temperature_records_what_it_changed_from` | the Changed event stops firing when the handle settles, so the settle note never updates |
| `FusionSliderChangeArgs.PreviousValue` | adjusting the temperature records what it changed from | `adjusting_the_temperature_records_what_it_changed_from` | `PreviousValue` stops carrying the value before the change (nudges the carried-over `68` and asserts the settle note reads the prior `68`) |
| `FusionSliderChangeArgs.Action` | adjusting the temperature records what it changed from | `adjusting_the_temperature_records_what_it_changed_from` | `Action` stops carrying the Syncfusion change-action name (asserts the settle note labels the source `changed`) |
| `FusionSliderChangeArgs.IsInteracted` | choosing a temperature reads differently from applying a recommendation | `choosing_a_temperature_reads_differently_from_applying_a_recommendation` | `IsInteracted` stops distinguishing a chosen value (true, "You set this temperature yourself") from an applied one (false, "recommended by your care team"); asserts both messages in sequence |
| `SetValue(double)` method | applying the recommended temperature moves the slider to 72 | `applying_the_recommended_temperature_moves_the_slider_to_72` | `SetValue` stops writing the given value and repainting (asserts the handle moves to `aria-valuenow=72`) |
| `Value(...)` read source | checking a warm temperature warns about an overnight check | `checking_a_warm_temperature_warns_about_an_overnight_check` | the `Value` source stops reading the current value into the guidance condition (raises to `74` and asserts the >=74 overnight-check warning) |
| `SetRangeValue(double, double)` method | applying the recommended rest window moves both handles and updates the summary | `applying_the_recommended_rest_window_moves_both_handles_and_updates_the_summary` | `SetRangeValue` stops writing both range handles (asserts the two handles move to `aria-valuenow` 14 and 16) |
| `RangeValue(...)` read source | applying the recommended rest window moves both handles and updates the summary | `applying_the_recommended_rest_window_moves_both_handles_and_updates_the_summary` | the `RangeValue` source stops reading the written window back out (asserts the saved summary reads `14,16`) |

## Additional Journey Coverage

Two further tests prove the `Value()` and `RangeValue()` sources through the HTTP
gather pipeline (a realistic save), beyond the single-member mappings above:

- `saving_confirms_the_temperature_and_rest_window` — the resident saves the
  carried-over preferences and sees the server confirmation "Saved. We'll keep
  your room at 68°F and hold non-urgent visits from 13:00 to 15:00.", proving the
  `Value()` and `RangeValue()` sources feed the gather body and the success
  response routes back to the page.
- `saving_posts_the_temperature_and_rest_window_to_the_server` — the framework
  gather test asserts the POST body carries `"roomTemperature":68` and
  `"quietHours":[13,15]` under their declared keys.

## Proof Criteria Met

- real handle gestures (trusted click then ArrowRight) and real button clicks drive every behavior; no DOM poking or `page.evaluate()`;
- the render proof asserts the carried-over model value (`68`) is bound at first paint;
- the `change` proof asserts the live reading and the value-selected comfort message change together as the handle moves;
- the `changed` proof asserts the settle note records the prior value and the change-action name;
- the `IsInteracted` proof asserts the user-chosen message and the recommendation message in sequence (a nudge then an Apply);
- the `SetValue` proof asserts the handle moves to the applied value `72`;
- the `Value()` proof raises the temperature and asserts the warm-branch guidance message;
- the `SetRangeValue`/`RangeValue` proof asserts both handles move and the summary reads `14,16`;
- the `Value()`/`RangeValue()` gather proof asserts the POST body carries both sources under their declared keys;
- every test asserts no console errors.

## Coverage Completeness

`proof/typed-api-coverage-matrix.md` lists all 14 typed `FusionSlider` members
and is `audited` with every row `row-proven`. `proof/behavioral-coverage.json`
maps each of the 14 members to the named test above whose assertion is
unsatisfiable by that member's defect, with one declared `acceptedFanout` (the
`warming_the_room...` test, whose single drag gesture carries distinct
fails-when-broken assertions for the reading, the comfort branch, and the
reaction running at all). No onboarded member is left without a fails-when-broken
assertion.
