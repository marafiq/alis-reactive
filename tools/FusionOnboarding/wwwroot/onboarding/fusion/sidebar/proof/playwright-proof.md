# Sidebar Playwright Proof

Status: audited. Every onboarded `FusionSidebar` member has focused typed DSL
Playwright proof through the Resident Care Dashboard journey. Each member is
bound to a fails-when-broken assertion, recorded per member in
`proof/behavioral-coverage.json`. That behavioral coverage was verified green
against the latest run by the onboarding behavioral-coverage gate, which is the
authority for the per-member outcome rather than any single static log path.

## Proof Surface

- Test file: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Sidebar/WhenUsingFusionSidebar.cs`
- Test class: `Alis.Reactive.PlaywrightTests.Components.Fusion.Sidebar.WhenUsingFusionSidebar`
- Locator: `tests/Alis.Reactive.Playwright.Extensions/FusionSidebarLocator.cs`
- Sandbox view: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Sidebar/Index.cshtml`
- Route: `http://localhost:5220/Sandbox/Components/FusionSidebar`
- Command: `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Sidebar.WhenUsingFusionSidebar"`
- Per-member fails-when-broken map: `proof/behavioral-coverage.json`

## Journey

A care coordinator works the Resident Care Dashboard. Care-services navigation
lives in a slide-out panel. The coordinator opens it to load the live service
list, jumps to a workflow, and tucks it away again — either with its Close
button or by tapping back on the dashboard. Each behavior is one isolated nested
vertical slice driven by real button clicks and dashboard taps, with no DOM
poking and no `page.evaluate()` (the gather test is the single allowed
`request.PostData` assertion).

## Member To Proof Map

| Onboarded member | Behavior (one user-visible journey) | Proving test | What breaks the test |
| --- | --- | --- | --- |
| `FusionSidebar(...)` render helper | the dashboard opens with the care-services menu tucked away | `the_dashboard_opens_with_the_care_services_menu_tucked_away` | the builder stops rendering the panel onto the dashboard (asserts the controlled root `#care-services-panel` is `e-close` at first paint and the menu reads "tucked away") |
| `FusionSidebarEvents.Opened` | opening the menu loads the live care service list | `opening_the_menu_loads_the_live_care_service_list` | the `open` event selector stops firing the reactive handler, so the open status and the server-loaded service list never appear |
| `Reactive` event wiring | opening the menu loads the live care service list | `opening_the_menu_loads_the_live_care_service_list` | the `.Reactive` wiring stops connecting the `open` event to its plan pipeline, so opening the panel runs no reaction |
| `FusionSidebarTransitionArgs` | opening the menu loads the live care service list | `opening_the_menu_loads_the_live_care_service_list` | the transition event arg contract stops delivering the open payload into the plan, so the open POST never fires and "3 care services available" never appears |
| `FusionSidebarTransitionArgs.IsInteracted` | dismissing the menu by tapping the dashboard records that you closed it | `dismissing_the_menu_by_tapping_the_dashboard_records_that_you_closed_it` | `IsInteracted` stops distinguishing a coordinator who dismissed the panel themselves (tap the dashboard -> close WITH an event -> true -> "You closed the care-services menu.") from the button's API close (false -> "closed automatically", asserted by `closing_the_menu_with_the_button_tucks_it_away`) |
| `FusionSidebarEvents.Closed` | closing the menu with the button tucks it away | `closing_the_menu_with_the_button_tucks_it_away` | the `close` event selector stops firing the reactive handler when `Hide()` runs, so the "closed automatically" status never appears |
| `Show(...)` method | opening the menu slides the care-services panel into view | `opening_the_menu_slides_the_care_services_panel_into_view` | `Show()` stops opening the panel (clicks Open and asserts the root becomes `e-open` and the Care-plan nav link inside the panel becomes visible) |
| `Hide(...)` method | closing the menu with the button tucks it away | `closing_the_menu_with_the_button_tucks_it_away` | `Hide()` stops closing the panel (opens then clicks Close and asserts the root returns to `e-close`) |
| `IsOpen(...)` read source | closing the menu posts that the panel is now shut | `closing_the_menu_posts_that_the_panel_is_now_shut` | the `IsOpen()` source stops yielding the panel state into the close gather body (asserts the close POST carries `"isOpen":false` after `Hide()`) |
| `Toggle(...)` method | the menu button toggles the panel open then collapse toggles it shut | `the_menu_button_toggles_the_panel_open_then_collapse_toggles_it_shut` | `Toggle()` stops flipping the panel state (toggles the tucked-away panel open via the header button, then toggles the open panel shut via the in-panel Collapse; fails whether `Toggle` stops opening or stops closing) |

## Proof Criteria Met

- real button clicks and dashboard taps drive every behavior; no DOM poking or `page.evaluate()`;
- the render proof asserts the panel renders tucked away (`e-close`) at first paint;
- the `Show()` proof asserts the panel reaches `e-open` and an in-panel nav link becomes visible;
- the `Opened` + `Reactive` + payload proof asserts the open status and the server-loaded "3 care services available" appear together through the POST round-trip;
- the `IsInteracted` proof asserts the user-dismissal note ("You closed...") and the API-close note ("closed automatically") in two sibling tests, proving both branches;
- the `Hide()` + `Closed` proof asserts the panel returns to `e-close` and the close status appears;
- the `IsOpen()` gather proof asserts the close POST body carries `"isOpen":false` under the declared key;
- the `Toggle()` proof flips the panel both directions from the header and in-panel buttons;
- every test asserts no console errors.

## Coverage Completeness

`proof/typed-api-coverage-matrix.md` lists all 10 typed `FusionSidebar` members
and is `audited` with every row `row-proven`. `proof/behavioral-coverage.json`
maps each of the 10 members to the named test above whose assertion is
unsatisfiable by that member's defect. No onboarded member is left without a
fails-when-broken assertion.
