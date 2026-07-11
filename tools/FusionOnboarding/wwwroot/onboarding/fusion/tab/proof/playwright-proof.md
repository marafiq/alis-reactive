# Tab Playwright Proof

Status: audited. Every onboarded `FusionTab` member has focused typed DSL
Playwright proof through the Resident Care Workspace journey. Each member is
bound to a fails-when-broken assertion, recorded per member in
`proof/behavioral-coverage.json`. That behavioral coverage was verified green
against the latest run by the onboarding behavioral-coverage gate, which is the
authority for the per-member outcome rather than any single static log path.

## Proof Surface

- Test file: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Tab/WhenTabSwitches.cs`
- Test class: `Alis.Reactive.PlaywrightTests.Components.Fusion.Tab.WhenTabSwitches`
- Typed locator: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Tab/FusionTabLocator.cs`
- Sandbox view: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Tab/Index.cshtml`
- Route: `http://localhost:5220/Sandbox/Components/Tab`
- Command: `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Tab.WhenTabSwitches"`
- Per-member fails-when-broken map: `proof/behavioral-coverage.json`

## Journey

A care coordinator works a resident's Care Workspace — a tabbed surface with
Care Schedule, Medications, Incident Reports, and Billing sections. The
coordinator moves between sections by clicking headers, jumps straight to logging
an incident, resumes in Medications, and hides Billing from the view when it
should not be shown. Each behavior is one isolated nested vertical slice driven
by real header clicks and button clicks, with no DOM poking and no
`page.evaluate()`.

## Member To Proof Map

| Onboarded member | Behavior (one user-visible journey) | Proving test | What breaks the test |
| --- | --- | --- | --- |
| `FusionTab(...)` render helper | the workspace opens on the Care Schedule section | `workspace_opens_showing_the_care_schedule_section` | the render helper stops rendering the tab bound to its sections (asserts the workspace opens on the Care Schedule header with that section's content visible) |
| `Selected` event selector | opening Medications shows the current medications | `opening_the_medications_section_shows_the_current_medications` | the `selected` event stops firing the reactive handler when a header is clicked, so opening Medications never updates the active-section line or shows the content |
| `Reactive` event wiring | opening Medications shows the current medications | `opening_the_medications_section_shows_the_current_medications` | the `.Reactive` wiring stops connecting `selected` to its plan pipeline, so clicking the Medications header runs no reaction |
| `FusionTabSelectedArgs` | opening Medications shows the current medications | `opening_the_medications_section_shows_the_current_medications` | the payload contract stops delivering the change payload into the plan, so the `SelectedIndex` branch cannot route and the active-section line never changes |
| `FusionTabSelectedArgs.SelectedIndex` | opening Medications shows the current medications | `opening_the_medications_section_shows_the_current_medications` | `SelectedIndex` stops carrying the newly selected tab index (clicks the Medications header at index 1 and asserts the active-section line reads Medications with the medications content) |
| `FusionTabSelectedArgs.PreviousIndex` | moving between sections records where the coordinator came from | `moving_between_sections_records_where_the_coordinator_came_from` | `PreviousIndex` stops carrying the index of the section just left (opens Medications then Incident Reports and asserts the context line reads "You moved here from Medications.") |
| `FusionTabSelectedArgs.IsSwiped` | opening a section by clicking is recorded as a deliberate selection | `opening_a_section_by_clicking_is_recorded_as_a_deliberate_selection` | `IsSwiped` stops distinguishing a click (false) from a swipe (true); the test clicks a header and asserts the navigation line reads "opened by selection." |
| `SetSelectedItem(int)` method | resuming returns the coordinator to the Medications section | `resuming_returns_the_coordinator_to_the_medications_section` | `SetSelectedItem` stops writing the selected index onto the tab (starts on Care Schedule, clicks Resume, asserts the active header becomes Medications with the content shown) |
| `Select(int)` method | the log-incident shortcut jumps straight to Incident Reports | `the_log_incident_shortcut_jumps_straight_to_incident_reports` | `Select` stops navigating to the given section index (clicks "Log an incident now" and asserts the active header becomes Incident Reports with its content) |
| `HideTab(int, bool)` method | hiding Billing removes it from the workspace | `hiding_billing_removes_it_from_the_workspace` | `HideTab` stops hiding the section at the given index (clicks "Hide billing from this view" and asserts the strip drops from four headers to three with no Billing header) |

## Proof Criteria Met

- real header clicks and button clicks drive every behavior; no DOM poking or `page.evaluate()`;
- the render proof asserts the workspace opens on the Care Schedule section at first paint;
- the `selected` proof asserts the visible active-section line and the section content change together;
- the `SelectedIndex` proof asserts the active section reads Medications after clicking index 1;
- the `PreviousIndex` proof asserts the context line names the section just left;
- the `IsSwiped` proof asserts a click is recorded as a deliberate selection;
- the `SetSelectedItem` proof asserts the resume action activates Medications;
- the `Select` proof asserts the shortcut activates Incident Reports;
- the `HideTab` proof asserts Billing is hidden (the header strip drops from four to three) and restored;
- every test asserts no console errors.

## Coverage Completeness

`proof/typed-api-coverage-matrix.md` lists all 10 typed `FusionTab` members and
is `audited` with every row `row-proven`. `proof/behavioral-coverage.json` maps
each of the 10 members to the named test above whose assertion is unsatisfiable
by that member's defect. No onboarded member is left without a fails-when-broken
assertion. The `restoring_billing_brings_the_tab_back` test additionally proves
the restore direction of `HideTab(index, false)`.
