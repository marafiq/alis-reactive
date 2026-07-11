# TextBox Playwright Proof

Status: audited. Every onboarded `FusionTextBox` member has focused typed DSL
Playwright proof through the Resident Profile journey. Each member is bound to a
fails-when-broken assertion, recorded per member in
`proof/behavioral-coverage.json`. That behavioral coverage was verified green
against the latest run by the onboarding behavioral-coverage gate, which is the
authority for the per-member outcome rather than any single static log path.

## Proof Surface

- Test file: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/TextBox/WhenUsingFusionTextBox.cs`
- Test class: `Alis.Reactive.PlaywrightTests.Components.Fusion.TextBox.WhenUsingFusionTextBox`
- Sandbox view: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/TextBox/Index.cshtml`
- Route: `http://localhost:5220/Sandbox/Components/TextBox`
- Command: `scripts/playwright.sh --filter "FullyQualifiedName~Components.Fusion.TextBox.WhenUsingFusionTextBox"`
- Per-member fails-when-broken map: `proof/behavioral-coverage.json`

## Journey

A care coordinator updates a resident's profile card: the preferred name the
resident goes by (shown to staff on the directory) and a dietary note kitchen
staff read. The card opens with the name on file, the coordinator edits it (live
preview, then a committed record showing what changed and who changed it), can
pull in the legal name, focus the field to edit and blur it when done, review and
update the dietary note, then save the profile. Each behavior is one isolated
nested vertical slice driven by real typing, real focus/blur, and real button
clicks, with no DOM poking and no `page.evaluate()` (the gather test is the
single allowed `request.PostData` assertion).

## Member To Proof Map

| Onboarded member | Behavior (one user-visible journey) | Proving test | What breaks the test |
| --- | --- | --- | --- |
| `FusionTextBox(...)` render helper | profile opens showing the name on file with a directory search affordance | `profile_opens_showing_the_name_on_file_with_a_directory_search_affordance` | the builder stops rendering the field bound to the model value (asserts the input reads `Margaret` and the directory preview reads `Margaret`) |
| `AddAppendIcon(string)` method | profile opens showing the name on file with a directory search affordance | `profile_opens_showing_the_name_on_file_with_a_directory_search_affordance` | `AddAppendIcon` stops appending the search icon on DomReady (asserts exactly one `.e-icons.e-search` inside the input group) |
| `Input` event selector | typing a new preferred name updates the directory preview | `typing_a_new_preferred_name_updates_the_directory_preview` | the `input` event stops firing the reactive handler while typing, so the preview never updates |
| `Reactive` event wiring | typing a new preferred name updates the directory preview | `typing_a_new_preferred_name_updates_the_directory_preview` | the `.Reactive` wiring stops connecting the textbox event to its plan pipeline, so typing runs no reaction |
| `FusionTextBoxInputArgs` | typing a new preferred name updates the directory preview | `typing_a_new_preferred_name_updates_the_directory_preview` | the input payload contract stops delivering the payload, so neither the preview nor the replacing-name line updates |
| `FusionTextBoxInputArgs.Value` | typing a new preferred name updates the directory preview | `typing_a_new_preferred_name_updates_the_directory_preview` | `Value` stops carrying the typed text (asserts the directory preview reads `Margie`) |
| `FusionTextBoxInputArgs.PreviousValue` | typing a new preferred name updates the directory preview | `typing_a_new_preferred_name_updates_the_directory_preview` | `PreviousValue` stops carrying the value being replaced (asserts the replacing line reads `Margaret`) |
| `Changed` event selector | finishing a name edit records the saved name and what it changed from | `finishing_a_name_edit_records_the_saved_name_and_what_it_changed_from` | the `change` event stops firing on blur, so the committed-name record never updates |
| `FusionTextBoxChangeArgs` | finishing a name edit records the saved name and what it changed from | `finishing_a_name_edit_records_the_saved_name_and_what_it_changed_from` | the change payload contract stops delivering the payload, so the saved name and changed-from never appear |
| `FusionTextBoxChangeArgs.Value` | finishing a name edit records the saved name and what it changed from | `finishing_a_name_edit_records_the_saved_name_and_what_it_changed_from` | `Value` stops carrying the committed name (asserts `Last saved name` reads `Margie`) |
| `FusionTextBoxChangeArgs.PreviousValue` | finishing a name edit records the saved name and what it changed from | `finishing_a_name_edit_records_the_saved_name_and_what_it_changed_from` | `PreviousValue` stops carrying the value before the commit (asserts `Changed from` reads `Margaret`) |
| `FusionTextBoxChangeArgs.IsInteracted` | filling the legal name records it as filled from the record, not a manual edit | `filling_the_legal_name_records_it_as_filled_from_the_record_not_a_manual_edit` | `IsInteracted` stops distinguishing a programmatic SetValue (false) from a hand-typed edit; the test asserts the "Filled from the resident's record." message that only renders when `IsInteracted` is false |
| `SetValue(string?)` method | filling the legal name records it as filled from the record, not a manual edit | `filling_the_legal_name_records_it_as_filled_from_the_record_not_a_manual_edit` | `SetValue` stops writing the value (asserts the input and the directory preview both read `Margaret Whitfield`) |
| `FocusIn()` method | starting an edit focuses the name field | `starting_an_edit_focuses_the_name_field` | `FocusIn` stops moving focus into the field (asserts the name input is focused) |
| `FocusOut()` method | marking the edit done moves focus off the name field | `marking_the_edit_done_moves_focus_off_the_name_field` | `FocusOut` stops removing focus (asserts the name input is no longer focused after Done) |
| `Focus` event selector | opening the dietary field shows the note already on file | `opening_the_dietary_field_shows_the_note_already_on_file` | the `focus` event stops firing, so the guidance and on-file note never appear |
| `FusionTextBoxFocusArgs` | opening the dietary field shows the note already on file | `opening_the_dietary_field_shows_the_note_already_on_file` | the focus payload contract stops delivering the payload, so the on-file note line never updates |
| `FusionTextBoxFocusArgs.Value` | opening the dietary field shows the note already on file | `opening_the_dietary_field_shows_the_note_already_on_file` | `Value` stops carrying the note on file at focus (asserts the on-file line reads the seeded `No shellfish`) |
| `Blur` event selector | leaving the dietary field captures the updated note | `leaving_the_dietary_field_captures_the_updated_note` | the `blur` event stops firing, so the captured-note confirmation never updates |
| `FusionTextBoxBlurArgs` | leaving the dietary field captures the updated note | `leaving_the_dietary_field_captures_the_updated_note` | the blur payload contract stops delivering the payload, so the captured-note line never updates |
| `FusionTextBoxBlurArgs.Value` | leaving the dietary field captures the updated note | `leaving_the_dietary_field_captures_the_updated_note` | `Value` stops carrying the note at blur (asserts `Captured note` reads `Low sodium, no shellfish`) |
| `Value(...)` read source | saving posts the preferred name and dietary note to the server | `saving_posts_the_preferred_name_and_dietary_note_to_the_server` | the `Value` source stops yielding the current text into the gather body (asserts the POST carries `"preferredName":"Margie"` and `"dietaryNote":"Low sodium, no shellfish"`) |

The journey also proves the `Value()` source feeds a guard
(`saving_a_profile_without_a_name_asks_for_one_first`) and feeds the visible
server confirmation (`saving_the_profile_confirms_it_with_the_residents_name`),
giving the read source a third and fourth consumer path beyond the gather body.

## Proof Criteria Met

- real typing, real focus/blur, and real button clicks drive every behavior; no DOM poking or `page.evaluate()`;
- the render proof asserts the model value (`Margaret`) is bound at first paint and the append icon is present;
- the `input` proof asserts the live preview (`Value`) and the replacing-name line (`PreviousValue`) together;
- the `change` proof asserts the committed name and the changed-from value;
- the `isInteracted` proof asserts the programmatic-fill message that only renders when the flag is false, distinct from the hand-typed-edit message;
- the focus and blur proofs assert the value present at focus (seeded note) and at blur (edited note);
- the `Value` gather proof asserts the POST body carries both declared keys;
- every test asserts no console errors.

## Coverage Completeness

`proof/typed-api-coverage-matrix.md` lists all 22 typed `FusionTextBox` members
and is `audited` with every row `row-proven`. `proof/behavioral-coverage.json`
maps each of the 22 members to the named test above whose assertion is
unsatisfiable by that member's defect. No onboarded member is left without a
fails-when-broken assertion. The single test that proves more than four members
(`typing_a_new_preferred_name_updates_the_directory_preview`) declares its
distinct-assertion fan-out in `proof/behavioral-coverage.json`.
