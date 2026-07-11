# TextArea Playwright Proof

Status: audited. Every onboarded `FusionTextArea` member has focused typed DSL
Playwright proof through the Resident Daily Care Log journey. Each member is
bound to a fails-when-broken assertion, recorded per member in
`proof/behavioral-coverage.json`. That behavioral coverage was verified green
against the latest run by the onboarding behavioral-coverage gate, which is the
authority for the per-member outcome rather than any single static log path.

## Proof Surface

- Test file: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/TextArea/WhenUsingFusionTextArea.cs`
- Test class: `Alis.Reactive.PlaywrightTests.Components.Fusion.TextArea.WhenUsingFusionTextArea`
- Sandbox view: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/TextArea/Index.cshtml`
- Route: `http://localhost:5220/Sandbox/Components/Fusion/TextArea`
- Command: `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.TextArea.WhenUsingFusionTextArea"`
- Per-member fails-when-broken map: `proof/behavioral-coverage.json`

## Journey

A caregiver opens, edits, and saves a resident's Daily Care Log note. The log
carries over last shift's note; the caregiver edits it (seeing a live preview
and what they replaced), can restore last shift's note, focuses and leaves the
field, and finally saves the note to the server. Each behavior is one isolated
nested vertical slice driven by real typing, real focus changes, and real button
clicks, with no DOM poking and no `page.evaluate()` (the gather test is the
single allowed `request.PostData` assertion).

## Member To Proof Map

| Onboarded member | Behavior (one user-visible journey) | Proving test | What breaks the test |
| --- | --- | --- | --- |
| `FusionTextArea(...)` render helper | care log opens showing the note carried over from last shift | `care_log_opens_showing_the_note_carried_over_from_last_shift` | the builder stops rendering the textarea bound to the model value, so the carried-over note never appears in the field |
| `Input` event selector | editing the note updates the live preview and remembers what it was | `editing_the_note_updates_the_live_preview_and_remembers_what_it_was` | the `input` event stops firing the reactive handler while editing, so the live preview never updates as the caregiver types |
| `FusionTextAreaInputArgs` | editing the note updates the live preview and remembers what it was | `editing_the_note_updates_the_live_preview_and_remembers_what_it_was` | the `input` payload contract stops delivering the payload into the plan, so editing shows neither the live preview nor the prior text |
| `FusionTextAreaInputArgs.Value` | editing the note updates the live preview and remembers what it was | `editing_the_note_updates_the_live_preview_and_remembers_what_it_was` | `Value` stops carrying the freshly typed text (asserts the live preview reads `Hydration encouraged at lunch.`) |
| `FusionTextAreaInputArgs.PreviousValue` | editing the note updates the live preview and remembers what it was | `editing_the_note_updates_the_live_preview_and_remembers_what_it_was` | `PreviousValue` stops carrying the text before the keystroke (asserts the before-keystroke line still reads last shift's note) |
| `Changed` event selector | finishing an edit records the committed note and what it replaced | `finishing_an_edit_records_the_committed_note_and_what_it_replaced` | the `change` event stops firing when the committed text changes and focus leaves, so the committed record never updates after an edit |
| `FusionTextAreaChangeArgs` | finishing an edit records the committed note and what it replaced | `finishing_an_edit_records_the_committed_note_and_what_it_replaced` | the `change` payload contract stops delivering the commit payload, so finishing an edit records neither the committed note nor what it replaced |
| `FusionTextAreaChangeArgs.Value` | finishing an edit records the committed note and what it replaced | `finishing_an_edit_records_the_committed_note_and_what_it_replaced` | `Value` stops carrying the committed text (asserts the committed note reads `Hydration encouraged at lunch.` after blur) |
| `FusionTextAreaChangeArgs.PreviousValue` | finishing an edit records the committed note and what it replaced | `finishing_an_edit_records_the_committed_note_and_what_it_replaced` | `PreviousValue` stops carrying the prior committed text (asserts the replace line reads last shift's note) |
| `FusionTextAreaChangeArgs.IsInteracted` | using last shift's note fills it back and marks it not edited by hand | `using_last_shifts_note_fills_it_back_and_marks_it_not_edited_by_hand` | `IsInteracted` stops distinguishing a programmatic `SetValue` (false, filled not edited by hand) from a hand-typed edit (true, edited by you); the test types an edit then restores and asserts the status flips to the not-edited-by-hand branch |
| `Focus` event selector | opening the note shows it is being edited now with the note on file | `opening_the_note_shows_it_is_being_edited_now_with_the_note_on_file` | the `focus` event stops firing when the caregiver opens the note, so the editing-now status never appears |
| `FusionTextAreaFocusArgs` | opening the note shows it is being edited now with the note on file | `opening_the_note_shows_it_is_being_edited_now_with_the_note_on_file` | the `focus` payload contract stops delivering the payload, so opening the note shows neither the editing status nor the note-on-file snapshot |
| `FusionTextAreaFocusArgs.Value` | opening the note shows it is being edited now with the note on file | `opening_the_note_shows_it_is_being_edited_now_with_the_note_on_file` | `Value` stops carrying the text on file when focus arrives (asserts the focus snapshot reads last shift's note) |
| `Reactive` event wiring | opening the note shows it is being edited now with the note on file | `opening_the_note_shows_it_is_being_edited_now_with_the_note_on_file` | the `.Reactive` wiring stops connecting the textarea's events to their plan pipelines, so focusing the note runs no reaction and the status never changes |
| `Blur` event selector | leaving the note holds the change for autosave | `leaving_the_note_holds_the_change_for_autosave` | the `blur` event stops firing when the caregiver leaves the note, so the autosave-hold status never appears |
| `FusionTextAreaBlurArgs` | leaving the note holds the change for autosave | `leaving_the_note_holds_the_change_for_autosave` | the `blur` payload contract stops delivering the payload, so leaving the note shows neither the autosave status nor the held-value snapshot |
| `FusionTextAreaBlurArgs.Value` | leaving the note holds the change for autosave | `leaving_the_note_holds_the_change_for_autosave` | `Value` stops carrying the text held in the field when focus leaves (asserts the blur snapshot reads last shift's note) |
| `FocusIn(...)` method | resuming editing focuses the note | `resuming_editing_focuses_the_note` | `FocusIn` stops moving focus into the textarea (the test clicks Resume editing and asserts the textarea becomes focused) |
| `FocusOut(...)` method | closing the note moves focus off it | `closing_the_note_moves_focus_off_it` | `FocusOut` stops removing focus from the textarea (the test focuses the note, clicks Done documenting, and asserts the textarea is no longer focused) |
| `SetValue(string?)` method | using last shift's note fills it back and marks it not edited by hand | `using_last_shifts_note_fills_it_back_and_marks_it_not_edited_by_hand` | `SetValue` stops writing the given text onto the field (the test types a draft, restores, and asserts the textarea value returns to last shift's note) |
| `Value(...)` read source | saving posts the care note to the server | `saving_posts_the_care_note_to_the_server` | the `Value` source stops yielding the current note into the gather body (asserts the POST carries `"careNote":"Hydration encouraged at lunch."`) |

## Supporting Behaviors

The same journey carries three further isolated slices that exercise the
accepted members through their guard and pipeline context:

- `saving_an_empty_note_asks_the_caregiver_to_write_one_first` — proves the `When(...).IsEmpty()` guard over the committed `Value` blocks the save and prompts the caregiver.
- `saving_the_note_confirms_it_was_recorded` — proves the `Gather -> Post -> OnSuccess` pipeline reveals the green confirmation panel with the saved note.
- `saving_posts_the_care_note_to_the_server` — proves the `Value()` gather row posts the declared key.

## Proof Criteria Met

- real typing, real focus changes, and real button clicks drive every behavior; no DOM poking or `page.evaluate()`;
- the render proof asserts the carried-over model value (last shift's note) is bound at first paint;
- the `input` proof asserts the live preview and the before-keystroke text change together while typing;
- the `change` proof asserts the committed note and the replaced text after focus leaves;
- the `isInteracted` proof asserts the hand-edited branch and the programmatic-fill branch in sequence;
- the `focus` and `blur` proofs assert the value snapshot and status text when focus arrives and leaves;
- the `FocusIn` and `FocusOut` proofs assert the textarea gains and loses focus;
- the `SetValue` proof asserts the textarea returns to the restored note;
- the `Value` gather proof asserts the POST body carries `"careNote":"Hydration encouraged at lunch."` under the declared key;
- every test asserts no console errors.

## Coverage Completeness

`proof/typed-api-coverage-matrix.md` lists all 21 typed `FusionTextArea` members
and is `audited` with every row `row-proven`. `proof/behavioral-coverage.json`
maps each of the 21 members to the named test above whose assertion is
unsatisfiable by that member's defect. No onboarded member is left without a
fails-when-broken assertion.

The 7-behavior contract for this input component resolves as follows. RENDERS,
INTERACTS, GATHERS, and SUBMITS are each proven by the named tests above.
VALIDATES, CONDITIONALLY VALIDATES, and LIVE-CLEARS are justified as
untestable-here: `FusionTextArea` exposes only the four event members
(`input`, `change`, `focus`, `blur`) and carries no validation event of its own;
validation is a cross-cutting `InputField` capability, not a TextArea member,
and the `CareNote` model declares no `Required`-style rule. The closest
unhappy-path guard — the empty-note save block — is proven by
`saving_an_empty_note_asks_the_caregiver_to_write_one_first` through the
`When(...).IsEmpty()` guard over the committed value.
