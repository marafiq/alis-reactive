# Rating Playwright Proof

Status: audited. Every onboarded `FusionRating` member has focused typed DSL
Playwright proof through the Monthly Care Satisfaction Survey journey. Each
member is bound to a fails-when-broken assertion, recorded per member in
`proof/behavioral-coverage.json`. That behavioral coverage was verified green
against the latest run by the onboarding behavioral-coverage gate, which is the
authority for the per-member outcome rather than any single static log path.

## Proof Surface

- Test file: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Rating/WhenUsingFusionRating.cs`
- Test class: `Alis.Reactive.PlaywrightTests.Components.Fusion.Rating.WhenUsingFusionRating`
- Sandbox view: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Rating/Index.cshtml`
- Route: `http://localhost:5220/Sandbox/Components/Rating`
- Command: `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Rating.WhenUsingFusionRating"`
- Per-member fails-when-broken map: `proof/behavioral-coverage.json`

## Journey

A resident reviews and updates their Monthly Care Satisfaction Survey. The survey
carries over last month's rating; the resident rates by clicking stars, can
restore last month's score, can clear it, then submits. Each behavior is one
isolated nested vertical slice driven by real star clicks and button clicks, with
no DOM poking and no `page.evaluate()` (the gather test is the single allowed
`request.PostData` assertion).

## Member To Proof Map

| Onboarded member | Behavior (one user-visible journey) | Proving test | What breaks the test |
| --- | --- | --- | --- |
| `FusionRating(...)` render helper | survey opens showing the rating carried over from last month | `survey_opens_showing_the_rating_carried_over_from_last_month` | the builder stops rendering the rating bound to the model value (asserts `aria-valuenow=3` and visible `3` of 5) |
| `ValueChanged` event selector | rating their care shows the score and a matching message | `rating_their_care_shows_the_score_and_a_matching_message` | the event stops firing the reactive handler when a star is clicked, so the visible response never updates |
| `Reactive` event wiring | rating their care shows the score and a matching message | `rating_their_care_shows_the_score_and_a_matching_message` | the `.Reactive` wiring stops connecting `valueChanged` to its plan pipeline, so clicking a star runs no reaction |
| `FusionRatingValueChangedArgs` | rating their care shows the score and a matching message | `rating_their_care_shows_the_score_and_a_matching_message` | the payload contract stops delivering the change payload into the plan, so neither score nor message appears |
| `FusionRatingValueChangedArgs.Value` | rating their care shows the score and a matching message | `rating_their_care_shows_the_score_and_a_matching_message` | `Value` stops carrying the newly selected rating (asserts visible `5` of 5 and the high-score message) |
| `FusionRatingValueChangedArgs.PreviousValue` | lowering their rating records what it changed from | `lowering_their_rating_records_what_it_changed_from` | `PreviousValue` stops carrying the value before the change (lowers carried-over `3` to `2`, asserts the change note reads the prior `3`) |
| `FusionRatingValueChangedArgs.IsInteracted` | clearing a rating the resident chose marks it unrated | `clearing_a_rating_the_resident_chose_marks_it_unrated` | `IsInteracted` stops distinguishing a chosen rating (true, ready to submit) from a programmatic clear (false, please rate); asserts both readiness messages in sequence |
| `Reset(...)` method | clearing a rating the resident chose marks it unrated | `clearing_a_rating_the_resident_chose_marks_it_unrated` | `Reset` stops clearing the rating to 0 (asserts visible rating `aria-valuenow=0` and score text `0` after Clear) |
| `SetValue(double)` method | restoring brings back the rating submitted last month | `restoring_brings_back_the_rating_submitted_last_month` | `SetValue` stops writing the given value onto the rating (rates `5`, restores, asserts the rating returns to `3`) |
| `Value(...)` read source | submitting posts the rating score to the server | `submitting_posts_the_rating_score_to_the_server` | the `Value` source stops yielding the current rating into the gather body (asserts the POST carries `"satisfactionScore":5`) |

## Proof Criteria Met

- real star clicks and button clicks drive every behavior; no DOM poking or `page.evaluate()`;
- the render proof asserts the carried-over model value (`3` of 5) is bound at first paint;
- the `valueChanged` proof asserts the visible score and the score-matched message change together;
- the `PreviousValue` proof lowers the rating and asserts the change note reads the prior value;
- the `IsInteracted` proof asserts the user-chosen readiness message and the programmatic-clear readiness message in sequence;
- the `Reset` proof asserts the rating and score both become `0`;
- the `SetValue` proof asserts the rating returns to the restored value;
- the `Value` gather proof asserts the POST body carries `"satisfactionScore":5` under the declared key;
- every test asserts no console errors.

## Coverage Completeness

`proof/typed-api-coverage-matrix.md` lists all 10 typed `FusionRating` members
and is `audited` with every row `row-proven`. `proof/behavioral-coverage.json`
maps each of the 10 members to the named test above whose assertion is
unsatisfiable by that member's defect. No onboarded member is left without a
fails-when-broken assertion.
