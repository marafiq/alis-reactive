# CheckBox Playwright Proof

Status: audited. Every onboarded `FusionCheckBox` member has focused typed DSL
Playwright proof through the Move-In Services Agreement journey. Each member is
bound to a fails-when-broken assertion, recorded per member in
`proof/behavioral-coverage.json`. That behavioral coverage was verified green
against the latest run by the onboarding behavioral-coverage gate, which is the
authority for the per-member outcome rather than any single static log path.

## Proof Surface

- Test file: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/CheckBox/WhenUsingFusionCheckBox.cs`
- Test class: `Alis.Reactive.PlaywrightTests.Components.Fusion.CheckBox.WhenUsingFusionCheckBox`
- Sandbox view: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/CheckBox/Index.cshtml`
- Route: `http://localhost:5220/Sandbox/Components/FusionCheckBox`
- Command: `scripts/playwright.sh --filter "FullyQualifiedName~Components.Fusion.CheckBox.WhenUsingFusionCheckBox"`
- Per-member fails-when-broken map: `proof/behavioral-coverage.json`

## Journey

A new resident completes their Move-In Services Agreement with a move-in
coordinator. Accepting the residency agreement unlocks the optional services; the
coordinator can pre-select a recommended service, mark one for follow-up when the
resident is undecided, or toggle one on the resident's behalf, then save the
elections. Each behavior is one isolated nested vertical slice driven by real
checkbox clicks and button clicks, with no DOM poking and no `page.evaluate()`
(the gather test is the single allowed `request.PostData` assertion). A trusted
click on the visible EJ2 box (`.e-frame`) toggles the checkbox the way a resident
would.

## Member To Proof Map

| Onboarded member | Behavior (one user-visible journey) | Proving test | What breaks the test |
| --- | --- | --- | --- |
| `FusionCheckBox(...)` render helper | move-in form opens with optional services locked | `move_in_form_opens_with_optional_services_locked_until_the_agreement_is_accepted` | the builder stops rendering the model-bound checkboxes (asserts the agreement box visible and unchecked, the housekeeping box locked) |
| `SetDisabled(bool)` | move-in form opens with optional services locked | `move_in_form_opens_with_optional_services_locked_until_the_agreement_is_accepted` | the DomReady `SetDisabled(true)` stops writing, so housekeeping is not greyed out on load (asserts the housekeeping box is disabled) |
| `Disabled()` read source | move-in form opens with optional services locked | `move_in_form_opens_with_optional_services_locked_until_the_agreement_is_accepted` | `Disabled()` stops yielding into its condition, so the locked message is wrong (asserts `Locked until you accept the residency agreement.`) |
| `Changed` event selector | accepting the residency agreement unlocks the optional services | `accepting_the_residency_agreement_unlocks_the_optional_services` | the `change` event stops firing the reactive handler when the resident accepts, so housekeeping stays locked |
| `Reactive` event wiring | accepting the residency agreement unlocks the optional services | `accepting_the_residency_agreement_unlocks_the_optional_services` | the `.Reactive` wiring stops connecting `change` to its plan pipeline, so accepting runs no reaction |
| `FusionCheckBoxChangeArgs` | accepting the residency agreement unlocks the optional services | `accepting_the_residency_agreement_unlocks_the_optional_services` | the payload contract stops delivering the change payload, so the accepted branch never runs and the box stays disabled |
| `FusionCheckBoxChangeArgs.Checked` | the agreement message follows whether the resident accepts or declines | `the_agreement_message_follows_whether_the_resident_accepts_or_declines` | `Checked` stops carrying the state after each change (toggles on then off; asserts `accepted` then `Please accept`) |
| `SetChecked(bool)` | adding recommended housekeeping checks the box for the resident | `adding_recommended_housekeeping_checks_the_box_for_the_resident` | `SetChecked` stops writing the checked state (asserts the housekeeping box becomes checked) |
| `SetIndeterminate(bool)` | flagging housekeeping for follow-up marks it undecided | `flagging_housekeeping_for_follow_up_marks_it_undecided` | `SetIndeterminate` stops writing the indeterminate state (asserts the box shows the EJ2 indeterminate dash, `e-stop`) |
| `Indeterminate()` read source | flagging housekeeping for follow-up marks it undecided | `flagging_housekeeping_for_follow_up_marks_it_undecided` | `Indeterminate()` stops yielding into its condition (asserts the follow-up message `A coordinator will follow up with you about weekly housekeeping.`) |
| `Click()` method | toggling housekeeping for the resident checks the box | `toggling_housekeeping_for_the_resident_checks_the_box` | `Click` stops invoking the rendered checkbox click (asserts the box becomes checked) |
| `FocusIn()` method | jumping back to the agreement focuses the agreement checkbox | `jumping_back_to_the_agreement_focuses_the_agreement_checkbox` | `FocusIn` stops moving focus into the input (asserts the agreement input is the focused element) |
| `Checked()` read source | saving posts the agreement and service elections to the server | `saving_posts_the_agreement_and_service_elections_to_the_server` | the `Checked()` source stops yielding into the gather body (asserts the POST carries `"agreementAccepted":true`) |

`Indeterminate()` is additionally consumed as a gather source in
`saving_posts_the_agreement_and_service_elections_to_the_server` (asserts the POST
carries `"housekeepingNeedsFollowUp":true`); its primary fails-when-broken proof
is the follow-up dash and message test above.

## Proof Criteria Met

- real checkbox clicks and button clicks drive every behavior; no DOM poking or `page.evaluate()`;
- the render proof asserts both boxes render and the optional service is locked at first paint;
- the `change` proof asserts the optional service unlocks when the agreement is accepted;
- the `Checked` payload proof toggles the agreement on then off and asserts the message follows both ways;
- the `SetChecked` proof asserts the box becomes checked after the coordinator pre-selects it;
- the `SetIndeterminate`/`Indeterminate()` proof asserts the `e-stop` dash and the follow-up message;
- the `SetDisabled`/`Disabled()` proof asserts the locked state and message on load and the unlocked message after accepting;
- the `Click` proof asserts the box toggles checked on the resident's behalf;
- the `FocusIn` proof asserts the agreement input becomes the focused element;
- the gather proof asserts the POST body carries `"agreementAccepted":true` and `"housekeepingNeedsFollowUp":true` under their declared keys;
- every test asserts no console errors.

## Coverage Completeness

`proof/typed-api-coverage-matrix.md` lists all 13 typed `FusionCheckBox` members
and is `audited` with every row `row-proven`. `proof/behavioral-coverage.json`
maps each of the 13 members to the named test above whose assertion is
unsatisfiable by that member's defect. No onboarded member is left without a
fails-when-broken assertion.
