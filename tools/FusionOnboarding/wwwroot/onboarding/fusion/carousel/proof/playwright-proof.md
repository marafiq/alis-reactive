# Carousel Playwright Proof

Status: proven. Every onboarded `FusionCarousel` member is exercised through the
typed Fusion DSL on a real sandbox view and asserted by a fails-when-broken
Playwright test. Proof runs through `scripts/playwright.sh`; the per-member map
is `proof/behavioral-coverage.json` and is verified against the latest TRX by the
behavioral-coverage gate.

## Journey

A nurse runs a resident's **Guided Care-Plan Review**: the review opens on the
Medications section, the nurse moves forward and back through the sections with
the navigation buttons, each reached section is recorded to the chart, and the
medications sign-off stays locked once the review has moved past it.

- View: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Carousel/Index.cshtml`
- Route: `/Sandbox/Components/Carousel`
- Test: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Carousel/WhenUsingFusionCarousel.cs`

## Test To Member Map

| Test | Members proven | What the user sees |
| --- | --- | --- |
| `review_opens_on_the_first_care_plan_section` | `FusionCarousel(...)`, `SelectedIndex()` | the review opens with the Medications slide active and "Section 1 of 3: Medications" |
| `advancing_to_the_next_section_shows_it_and_records_it_to_the_chart` | `Next()`, `Reactive`, `SlideChanged`, `FusionCarouselSlideChangedArgs`, `FusionCarouselSlideChangedArgs.CurrentIndex` | clicking Next shows Therapy Goals and the chart records "moved forward to Therapy Goals (from Medications), using the buttons" |
| `stepping_back_a_section_says_it_came_from_the_later_section` | `Previous()`, `FusionCarouselSlideChangedArgs.PreviousIndex`, `FusionCarouselSlideChangedArgs.SlideDirection` | stepping back from Discharge Steps shows "after the discharge steps" and "You went back a section." |
| `reaching_a_section_with_the_buttons_is_recorded_as_a_button_move` | `FusionCarouselSlideChangedArgs.IsSwiped` | "Reached using the navigation buttons." on screen and "using the buttons" in the chart |
| `the_medications_signoff_stays_locked_when_stepping_back_to_it` | `SlideChanging`, `FusionCarouselSlideChangingArgs`, `FusionCarouselSlideChangingArgs.NextIndex`, `FusionCarouselSlideChangingArgs.PreventTransition()`, `FusionCarouselSlideChangingArgs.Cancel`, `FusionCarouselSlideChangingArgs.CurrentIndex`, `FusionCarouselSlideChangingArgs.SlideDirection`, `FusionCarouselSlideChangingArgs.IsSwiped` | stepping back onto Medications is blocked: the carousel stays on Therapy Goals and the lock notice explains the direction, the section, and the button gesture |
| `recording_a_section_posts_the_slide_change_payload_to_the_server` | gather pipeline (framework gather test asserting `request.PostData`) | the POST body carries `sectionIndex`, `cameFromIndex`, `direction`, `bySwipe` |

The `recording_a_section_posts_...` test is the framework gather-pipeline
proof; the four `slideChanged` payload members it carries are each separately
proven fails-when-broken through visible behavior in the rows above, so it is an
additional gather assertion, not the sole proof of any member.

## Commands Run

- behavior proof:
  `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Carousel.WhenUsingFusionCarousel"` — "Test Run Successful. Total tests: 6, Passed: 6".
  Latest TRX: `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260614-154111.trx`.
- behavioral coverage gate (0b):
  `node .claude/skills/onboard-fusion-component/scripts/verify-behavioral-coverage.mjs --component carousel` — "[PASS] carousel — 19/19 members mapped, 5 proving test(s)".
- parity:
  `node .claude/skills/onboard-fusion-component/scripts/compute-fusion-parity.mjs --component carousel` — "parity: 32/32 = 100.0% -> PASS".
