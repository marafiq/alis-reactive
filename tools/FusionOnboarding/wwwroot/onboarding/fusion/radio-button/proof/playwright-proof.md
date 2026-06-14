# RadioButton Playwright Proof

Status: audited. Every onboarded `FusionRadioButton` member has focused typed
DSL Playwright proof through the Move-in Room and Care Plan journey. Each member
is bound to a fails-when-broken assertion, recorded per member in
`proof/behavioral-coverage.json`. That behavioral coverage was verified green
against the latest run by the onboarding behavioral-coverage gate, which is the
authority for the per-member outcome rather than any single static log path.

## Proof Surface

- Test file: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/RadioButton/WhenUsingFusionRadioButton.cs`
- Test class: `Alis.Reactive.PlaywrightTests.Components.Fusion.RadioButton.WhenUsingFusionRadioButton`
- Sandbox view: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/RadioButton/Index.cshtml`
- Route: `http://localhost:5220/Sandbox/Components/FusionRadioButton`
- Command: `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.RadioButton.WhenUsingFusionRadioButton"`
- Per-member fails-when-broken map: `proof/behavioral-coverage.json`

## Journey

A move-in coordinator helps a resident choose a room and care plan. The intake
opens listing every room option as a radio; the coordinator chooses a room and
sees the selection and its detail, can apply the resident's last assessment to
preselect the recommended room, can mark a room full so it cannot be chosen and
reopen it, can jump to the recommended studio with focus, and confirms the
chosen room to the desk. Each behavior is one isolated nested vertical slice
driven by real radio-label clicks and button clicks, with no DOM poking and no
`page.evaluate()` (the gather test is the single allowed `request.PostData`
assertion).

## Member To Proof Map

| Onboarded member | Behavior (one user-visible journey) | Proving test | What breaks the test |
| --- | --- | --- | --- |
| `FusionRadioButton(...)` render helper | the intake opens with every room option listed | `the_intake_opens_with_every_room_option_listed` | the render helper stops building the options from the model, so the intake opens without the Studio/One-bedroom/Companion suite radios and their labels |
| `Changed` event selector | choosing the companion suite shows it as the selected room | `choosing_the_companion_suite_shows_it_as_the_selected_room` | the `change` event stops firing when a radio is clicked, so the visible selected-room line never updates after choosing the companion suite |
| `Reactive` event wiring | choosing the companion suite shows it as the selected room | `choosing_the_companion_suite_shows_it_as_the_selected_room` | the `.Reactive` wiring stops connecting `change` to its plan pipeline, so choosing the companion suite runs no reaction |
| `FusionRadioButtonChangeArgs` | choosing the companion suite shows it as the selected room | `choosing_the_companion_suite_shows_it_as_the_selected_room` | the change payload contract stops delivering the payload into the plan, so choosing the companion suite shows neither the selected-room value nor its condition-routed detail |
| `FusionRadioButtonChangeArgs.Value` | choosing the studio shows the studio as the selected room | `choosing_the_studio_shows_the_studio_as_the_selected_room` | `Value` stops carrying the chosen option (the test chooses the studio and asserts the selected-room line reads `Studio Apartment` and the studio-specific detail, not the companion suite's) |
| `SetChecked(bool)` method | applying her last assessment selects the recommended room | `applying_her_last_assessment_selects_the_recommended_room` | `SetChecked` stops writing the checked property onto the companion suite, so applying her last assessment leaves the companion radio unchecked (asserts it becomes checked without a click) |
| `SelectedValue(...)` read source | applying her last assessment selects the recommended room | `applying_her_last_assessment_selects_the_recommended_room` | `SelectedValue` stops reading the group's chosen value, so after applying her last assessment the selected-room line does not read back `Shared Companion Suite` |
| `Checked(...)` read source | confirming the companion suite posts that it was chosen | `confirming_the_companion_suite_posts_that_it_was_chosen` | the `Checked` source stops yielding the companion suite's checked state into the gather body (asserts the POST carries `"companionSuiteChosen":true` after the suite is checked) |
| `SetDisabled(bool)` method | marking the companion suite full takes it off the list | `marking_the_companion_suite_full_takes_it_off_the_list` | `SetDisabled` stops writing the disabled property, so marking the companion suite full leaves its radio enabled (asserts the radio input becomes disabled) |
| `Disabled(...)` read source | marking the companion suite full takes it off the list | `marking_the_companion_suite_full_takes_it_off_the_list` | the `Disabled` source stops reading the disabled state, so the condition over it never posts the full-this-month notice the test asserts |
| `Click(...)` method | taking her to the recommended studio selects and focuses it | `taking_her_to_the_recommended_studio_selects_and_focuses_it` | `Click` stops invoking the studio radio's selection, so the studio is left unchecked (asserts the studio becomes checked and its selected-room line shows) |
| `FocusIn(...)` method | taking her to the recommended studio selects and focuses it | `taking_her_to_the_recommended_studio_selects_and_focuses_it` | `FocusIn` stops moving keyboard focus to the studio radio, so the coordinator is not landed on the recommended option (asserts the studio input is focused) |

## Proof Criteria Met

- real radio-label clicks and button clicks drive every behavior; no DOM poking or `page.evaluate()`;
- the render proof asserts all three room labels are visible and the initial selected-room line;
- the `change` proof asserts the visible selected-room value and the condition-routed detail change together;
- the `Value` proof chooses the studio and asserts the studio value and detail, distinct from the companion suite's, proving the payload carries the clicked option rather than a fixed string;
- the `SetChecked`/`SelectedValue` proof asserts the recommended room becomes checked and reads back without a click;
- the `SetDisabled`/`Disabled` proof asserts the radio becomes disabled and the unavailable notice routes;
- the reopening behavior asserts the disabled radio becomes enabled and the notice flips back;
- the `Click`/`FocusIn` proof asserts the studio becomes selected and focused;
- the `Checked` gather proof asserts the POST body carries `"companionSuiteChosen":true` under the declared key;
- every test asserts no console errors.

## Coverage Completeness

`proof/typed-api-coverage-matrix.md` lists all 12 typed `FusionRadioButton`
members and is `audited` with every row `row-proven`.
`proof/behavioral-coverage.json` maps each of the 12 members to the named test
above whose assertion is unsatisfiable by that member's defect. No onboarded
member is left without a fails-when-broken assertion.

The 7-behavior contract for this slice is recorded honestly: RENDERS (the intake
render), INTERACTS (choosing a room and reading back the selection),
GATHERS/SUBMITS (the `Checked` source posting `companionSuiteChosen` to the
server). VALIDATES, CONDITIONALLY VALIDATES, and LIVE-CLEARS are a justified
exclusion for this journey: a room is chosen through a radio group with no
required-field error surface, so the slice carries no `AbstractValidator` and no
`.Validate<>()`. This is a structural exclusion recorded per the Coverage
Completeness Gate, not an unwritten gap.
