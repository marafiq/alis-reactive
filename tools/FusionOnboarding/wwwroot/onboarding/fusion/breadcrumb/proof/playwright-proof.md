# Breadcrumb Playwright Proof

Status: audited. Every onboarded `FusionBreadcrumb` member has focused typed DSL
Playwright proof through the Resident Care Record journey. Each member is bound to a
fails-when-broken assertion, recorded per member in `proof/behavioral-coverage.json`.
That behavioral coverage was verified green against the latest run by the onboarding
behavioral-coverage gate (0b), which is the authority for the per-member outcome
rather than any single static log path.

## Proof Surface

- Test file: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Breadcrumb/WhenUsingFusionBreadcrumb.cs`
- Test class: `Alis.Reactive.PlaywrightTests.Components.Fusion.Breadcrumb.WhenUsingFusionBreadcrumb`
- Sandbox view: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Breadcrumb/Index.cshtml`
- Route: `http://localhost:5220/Sandbox/Components/CareRecordBreadcrumb`
- Command: `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Breadcrumb.WhenUsingFusionBreadcrumb"`
- Per-member fails-when-broken map: `proof/behavioral-coverage.json`

## Journey

A care coordinator is deep in a resident's care record. The breadcrumb trail
(Sunrise Court > Eleanor Hughes > Care Plan) lets them step back up to a higher
section. Opening a section reads its heading, icon, summary, and record code from
the clicked crumb, and a button returns the current crumb to the resident overview.
Each behavior is one isolated nested vertical slice driven by real crumb clicks and
a real button click, with no DOM poking and no `page.evaluate()` (the gather test is
the single allowed `request.PostData` assertion).

## Member To Proof Map

| Onboarded member | Behavior (one user-visible journey) | Proving test | What breaks the test |
| --- | --- | --- | --- |
| `FusionBreadcrumb(...)` render helper | the care record opens showing the full trail to the Care Plan | `the_care_record_opens_showing_the_full_trail_to_the_care_plan` | the builder stops rendering the trail (asserts Sunrise Court and Eleanor Hughes crumbs visible, Care Plan current) |
| `ActiveItem(...)` read source | the care record opens showing the full trail to the Care Plan | `the_care_record_opens_showing_the_full_trail_to_the_care_plan` | the `activeItem` read stops yielding the current crumb url, so the DomReady `Eq(carePlanUrl)` branch no longer matches and the viewing-now caption shows the Else text |
| `ItemClick` event selector | stepping up to the resident opens that section of the record | `stepping_up_to_the_resident_opens_that_section_of_the_record` | the event stops firing the reactive handler when a crumb is clicked, so the section panel never opens |
| `Reactive` event wiring | stepping up to the resident opens that section of the record | `stepping_up_to_the_resident_opens_that_section_of_the_record` | the `.Reactive` wiring stops connecting `itemClick` to its plan pipeline, so clicking a crumb runs no reaction |
| `FusionBreadcrumbItemClickArgs` | stepping up to the resident opens that section of the record | `stepping_up_to_the_resident_opens_that_section_of_the_record` | the payload contract stops delivering the click payload, so the heading is never set and the panel stays hidden |
| `FusionBreadcrumbItemClickArgs.Item` | stepping up to the resident opens that section of the record | `stepping_up_to_the_resident_opens_that_section_of_the_record` | the `Item` property stops carrying the clicked crumb, so `Item.Text` resolves to nothing and the heading never reads "Eleanor Hughes" |
| `FusionBreadcrumbItem` | the opened section loads the summary for that record's url | `the_opened_section_loads_the_summary_for_that_records_url` | the item payload contract stops shaping the clicked crumb, so `Item.Url` no longer resolves and the server-resolved community summary never shows |
| `FusionBreadcrumbItem.Text` | stepping up to the resident opens that section of the record | `stepping_up_to_the_resident_opens_that_section_of_the_record` | `Item.Text` stops carrying the crumb's text, so the opened-section heading no longer reads "Eleanor Hughes" |
| `FusionBreadcrumbItem.IconCss` | the opened section is tagged with its record icon | `the_opened_section_is_tagged_with_its_record_icon` | `Item.IconCss` stops carrying the crumb's icon classes, so the icon tag no longer reads "e-icons e-user" for the resident crumb |
| `FusionBreadcrumbItem.Url` | the opened section loads the summary for that record's url | `the_opened_section_loads_the_summary_for_that_records_url` | `Item.Url` stops carrying the crumb's url into the gather body, so the server resolves no summary for the community url |
| `FusionBreadcrumbItem.Id` | the opened section shows the record code for that crumb id | `the_opened_section_shows_the_record_code_for_that_crumb_id` | `Item.Id` stops carrying the crumb's id into the gather body, so the section code badge no longer reads "RES-214" |
| `FusionBreadcrumbItem.Disabled` | opening a section posts the clicked crumb to the server | `opening_a_section_posts_the_clicked_crumb_to_the_server` | `Item.Disabled` stops carrying the crumb's disabled flag into the gather body, so the POST no longer contains `"disabled":false` |
| `SetActiveItem(string)` write | returning to the resident overview moves the current crumb | `returning_to_the_resident_overview_moves_the_current_crumb` | `SetActiveItem` stops writing the resident url and chaining `dataBind()`, so the current crumb stays Care Plan instead of moving to Eleanor Hughes |

## Proof Criteria Met

- real crumb clicks (trusted Playwright `.ClickAsync()`) and a real button click drive every behavior; no DOM poking or `page.evaluate()`;
- the render proof asserts the full trail is rendered and the DomReady `ActiveItem()` read confirms the Care Plan is current at first paint;
- the `itemClick` proof asserts the opened-section heading and panel appear together;
- the `IconCss` proof asserts the opened section's icon tag reads the clicked crumb's icon classes;
- the `Url` proof asserts the server-resolved section summary (resolved from the crumb url) appears;
- the `Id` proof asserts the server-resolved record code (resolved from the crumb id) appears;
- the `Disabled` proof asserts the POST body carries `"disabled":false` under its declared key (the single allowed `request.PostData` assertion);
- the `SetActiveItem` proof asserts the current crumb (`aria-current`) moves from Care Plan to Eleanor Hughes;
- every test asserts no console errors.

## Disabled Payload Note

`FusionBreadcrumbItem.Disabled` is proven through the gather body rather than a
clicked-crumb branch because Syncfusion suppresses `itemClick` on a disabled crumb:
`breadcrumb/bootstrap4.css:123-125` sets `pointer-events: none` on
`.e-breadcrumb-item.e-disabled`, so a disabled crumb cannot receive a trusted click
(verified in a real browser — clicking a disabled crumb fired no `itemClick`). A
real gesture can therefore only ever produce `Disabled = false` from a clicked
(enabled) crumb. The fails-when-broken proof asserts `"disabled":false` is gathered
into the POST: if `Item.Disabled` stops reading the payload key, the exact substring
is absent and the test fails. This is the strongest real-interaction proof available
for the member and uses the framework-gather `request.PostData` exception.

## Coverage Completeness

`proof/typed-api-coverage-matrix.md` lists all 13 typed `FusionBreadcrumb` members
and is `audited` with every row `row-proven`. `proof/behavioral-coverage.json` maps
each of the 13 members to the named test above whose assertion is unsatisfiable by
that member's defect, verified by the 0b behavioral-coverage gate against the latest
TRX. No onboarded member is left without a fails-when-broken assertion.
