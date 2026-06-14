# FusionCarousel — Blind Reviewer Verdict

A blind-reviewer agent reviewed the carousel Playwright suite with ONLY
`.claude/memory/bdd-principles.md` and the test file (no context on why the tests
were written, no access to this artifact tree). Its prompt explicitly permitted a
REJECT verdict. It confirmed the live review target up front, opened the page in a
real browser, and re-ran the pipeline-dependent journeys.

- Review target: branch `tiny-safe-but-important-refactorings`,
  HEAD `c28967e6cc793cf481a46090bad129ca699105cc`.
- Suite under review: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Carousel/WhenUsingFusionCarousel.cs` (157 lines).

## VERDICT: PASS

> all 6 tests are behavior-named, independent, real-interaction-only, and each
> fails-when-broken with a nameable bug. No REJECT-level violations.

Quoted reviewer evidence (the literal verdict object the reviewer returned):

```json
{"verdict":"PASS","findings":["Review target confirmed live: branch tiny-safe-but-important-refactorings, HEAD c28967e6cc793cf481a46090bad129ca699105cc, the carousel test present in the working tree at tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Carousel/WhenUsingFusionCarousel.cs (157 lines). All findings below were re-verified against that tree and the running sandbox.","VERDICT: PASS. All 6 tests are behavior-named, independent (each calls OpenReview() for a fresh navigation), real-interaction-only (no page.evaluate, no ej2_instances, no DOM poking, no skips/Ignore/Retry, no try-catch around asserts), and each fails-when-broken with a nameable bug. I opened the page in a real browser and reproduced every journey.","Rule 3 (fails-when-broken) holds per test. (1) review_opens_on_the_first_care_plan_section: asserts ActiveSectionTitle 'Medications' + #review-position 'Section 1 of 3: Medications' — catches a wrong initial SelectedIndex(0) or a broken DomReady When/Eq position branch. (2) advancing...records_it_to_the_chart: catches a Next() that does not advance, a broken SlideChanged.CurrentIndex narration, or a broken gather->POST->OnSuccess chart write (ChartLine assert is exact full text). (3) stepping_back...came_from_the_later_section: catches a broken SlideChanged.PreviousIndex (CameFrom) or SlideDirection=='Previous' (Movement). (4) reaching...recorded_as_a_button_move: catches SlideChanged.IsSwiped==false mis-handling (Method 'Reached using the navigation buttons.' pinned exactly). (5) medications_signoff_stays_locked: catches removal/breakage of the SlideChanging NextIndex==0 PreventTransition guard — if the lock breaks, the carousel moves to Medications and the 'Therapy Goals' / 'Section 2 of 3' asserts fail. (6) recording...posts_the_slide_change_payload: catches any dropped/renamed gather key or wrong value type.","BROWSER-VERIFIED each behavior (no console errors throughout): Next advances Medications->Therapy Goals and the Chart shows 'Recorded: moved forward to Therapy Goals (from Medications), using the buttons.'; from Discharge Steps, Previous lands on Therapy Goals with 'after the discharge steps' + 'You went back a section.'; from Therapy Goals, Previous is BLOCKED — carousel stays on 'Section 2 of 3: Therapy Goals' and the amber lock banner shows all four lines the test asserts (locked / went-back / still-on-therapy-goals / button-press-not-applied). This is a genuine unhappy-path proof, not happy-path-only.","Gather wire contract (test 6) confirmed at the source of truth: POSTing the asserted body {\"sectionIndex\":1,\"cameFromIndex\":0,\"direction\":\"Next\",\"bySwipe\":false} to the live /Sandbox/Components/Carousel/Record endpoint binds cleanly to CarePlanReviewEntry and returns the exact chart line observed in the browser. Keys/types (sectionIndex:int, cameFromIndex:int, direction:string, bySwipe:bool) match FromEvent(args, CurrentIndex/PreviousIndex/SlideDirection/IsSwiped). Note: the in-browser MCP network reader did not surface this tab's XHR (tooling limitation), so the POST body was verified via the server binding + the rendered chart line rather than a captured PostData frame; this is the sanctioned framework-gather assertion and it is correctly scoped to request.PostData in the test.","NOT an echo-span/debug-div violation (considered and rejected): the #review-* and #gate-* spans render as a real product screen — heading 'Guided Care-Plan Review', section cards, a status card narrating the review, an amber lock banner, a green Chart panel. It passed the screenshot test by eye. It also matches the repo's own cited exemplar: Grid Billing's Billing.cshtml carries a 'Roster Tools' status panel (billing-status/tool-status/toolbar-item-id/toolbar-cancel/toolbar-event) surfacing event-payload fields as visible text. The carousel's panels are cleaner than that exemplar (product prose, not a font-mono dump). The payload members (CurrentIndex/PreviousIndex/IsSwiped/SlideDirection on SlideChanged; +NextIndex/Cancel on SlideChanging; PreventTransition writing cancel=true) are all real typed members in Alis.Reactive.Fusion/Components/FusionCarousel/Events/*.cs.","Single borderline item (defensible, not a defect): WhenUsingFusionCarousel.cs:99 uses ToContainTextAsync(\"using the buttons\") on #chart-line, where the BDD no-hack list flags 'ToContainText when you mean ToHaveText'. Here it is a deliberately scoped substring assertion of the navigation-method clause (the exact thing test 4 is about), and the on-screen method is ALREADY pinned exactly on line 98 via ToHaveTextAsync(\"Reached using the navigation buttons.\"). The full chart line is asserted exactly elsewhere (line 62). Acceptable as-is; if a reviewer wants zero ambiguity it could assert the full chart line, but it is not pass-hacking.","7-behavior contract: the carousel is a navigation/display component with no form input, so behaviors VALIDATES / CONDITIONALLY-VALIDATES / LIVE-CLEARS (3-5) do not apply. The suite covers the applicable subset — RENDERS, INTERACTS, GATHERS, server round-trip (SUBMITS-equivalent) — plus a real unhappy-path (lock guard) and the direction/came-from/method narrations. Appropriate for this component type; no missing-behavior finding.","Nested Vertical Slice contract satisfied: own model (FusionCarouselModel + CarePlanReviewEntry/Response, referenced by no other journey), own view (one route /Sandbox/Components/Carousel), own controller (CarouselController with immutable SectionNames, no shared mutable state), own world (each test navigates fresh; no ordering or shared state). Locators in FusionCarouselLocator.cs key on the EJ2-stamped .e-carousel-item.e-active (a real user-visible element), not internal instances."]}
```

## Findings, expanded

- **Review target confirmed live.** Branch `tiny-safe-but-important-refactorings`,
  HEAD `c28967e6cc793cf481a46090bad129ca699105cc`; the carousel test is present in
  the working tree at
  `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Carousel/WhenUsingFusionCarousel.cs`
  (157 lines). Every finding was re-verified against that tree and the running
  sandbox.
- **Verdict — PASS.** All 6 tests are behavior-named, independent (each calls
  `OpenReview()` for a fresh navigation), real-interaction-only (no
  `page.evaluate`, no `ej2_instances`, no DOM poking, no skips/`Ignore`/`Retry`,
  no try/catch around asserts), and each fails-when-broken with a nameable bug.
  The reviewer opened the page in a real browser and reproduced every journey.
- **Rule 3 (fails-when-broken) holds per test.** (1)
  `review_opens_on_the_first_care_plan_section` — catches a wrong initial
  `SelectedIndex(0)` or a broken `DomReady` When/Eq position branch. (2)
  `advancing...records_it_to_the_chart` — catches a `Next()` that does not advance,
  a broken `SlideChanged.CurrentIndex` narration, or a broken
  gather→POST→OnSuccess chart write (the chart-line assert is exact full text). (3)
  `stepping_back...came_from_the_later_section` — catches a broken
  `SlideChanged.PreviousIndex` (CameFrom) or `SlideDirection=='Previous'`
  (Movement). (4) `reaching...recorded_as_a_button_move` — catches
  `SlideChanged.IsSwiped==false` mis-handling (Method
  "Reached using the navigation buttons." pinned exactly). (5)
  `the_medications_signoff_stays_locked...` — catches removal/breakage of the
  `SlideChanging` `NextIndex==0` `PreventTransition` guard. (6)
  `recording...posts_the_slide_change_payload` — catches any dropped/renamed gather
  key or wrong value type.
- **Browser-verified each behavior (no console errors throughout).** Next advances
  Medications → Therapy Goals with
  "Recorded: moved forward to Therapy Goals (from Medications), using the buttons.";
  from Discharge Steps, Previous lands on Therapy Goals with
  "after the discharge steps" + "You went back a section."; from Therapy Goals,
  Previous is BLOCKED — the carousel stays on "Section 2 of 3: Therapy Goals" and
  the amber lock banner shows all four lines the test asserts. A genuine
  unhappy-path proof, not happy-path-only.
- **Gather wire contract (test 6) confirmed at the source of truth.** POSTing
  `{"sectionIndex":1,"cameFromIndex":0,"direction":"Next","bySwipe":false}` to the
  live `/Sandbox/Components/Carousel/Record` endpoint binds cleanly to
  `CarePlanReviewEntry` and returns the exact chart line observed in the browser.
  Keys/types match `FromEvent(args, CurrentIndex/PreviousIndex/SlideDirection/IsSwiped)`.
  The in-browser network reader did not surface this tab's XHR (tooling
  limitation), so the POST body was verified via the server binding + the rendered
  chart line; the assertion is the sanctioned framework-gather exception, correctly
  scoped to `request.PostData`.
- **Not an echo-span/debug-div violation (considered and rejected).** The
  `#review-*` and `#gate-*` spans render as a real product screen and pass the
  screenshot test by eye; this matches the repo's cited Grid Billing exemplar. The
  payload members are all real typed members in
  `Alis.Reactive.Fusion/Components/FusionCarousel/Events/*.cs`.
- **Single borderline item (defensible, not a defect).**
  `WhenUsingFusionCarousel.cs:99` uses `ToContainTextAsync("using the buttons")` on
  `#chart-line` — a deliberately scoped substring assertion of the navigation-method
  clause; the on-screen method is already pinned exactly on line 98 via
  `ToHaveTextAsync("Reached using the navigation buttons.")` and the full chart line
  is asserted exactly on line 62. Acceptable; not pass-hacking.
- **7-behavior contract — applicable subset covered.** The carousel is a
  navigation/display component with no form input, so VALIDATES /
  CONDITIONALLY-VALIDATES / LIVE-CLEARS do not apply. The suite covers RENDERS,
  INTERACTS, GATHERS, and the server round-trip, plus a real unhappy-path (lock
  guard). No missing-behavior finding.
- **Nested Vertical Slice contract satisfied.** Own model
  (`FusionCarouselModel` + `CarePlanReviewEntry`/`Response`), own view (one route
  `/Sandbox/Components/Carousel`), own controller (`CarouselController` with
  immutable `SectionNames`), own world (each test navigates fresh). Locators key on
  the EJ2-stamped `.e-carousel-item.e-active`, a real user-visible element.

## Author re-verification (parent, at source)

Independently re-confirmed before accepting: the artifact gate
(`verify-fusion-artifact-gates.mjs --component carousel`) passes; the behavioral
coverage gate (`verify-behavioral-coverage.mjs --component carousel`) passes; the
six-test suite passes through `scripts/playwright.sh` with the latest TRX at
`tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260614-154111.trx`.
The verdict aligns with the source evidence.
