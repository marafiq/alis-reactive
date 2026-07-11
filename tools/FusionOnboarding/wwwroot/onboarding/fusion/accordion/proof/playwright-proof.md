# Accordion Playwright Proof

Status: audited. Every onboarded `FusionAccordion` member has focused typed DSL
Playwright proof through the resident "My Care Plan" journey. Each member is bound to a
fails-when-broken assertion, recorded per member in `proof/behavioral-coverage.json`.
That behavioral coverage was verified green against the latest run by the onboarding
behavioral-coverage gate, which is the authority for the per-member outcome rather than
any single static log path.

## Proof Surface

- Test file: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Accordion/WhenUsingFusionAccordion.cs`
- Test class: `Alis.Reactive.PlaywrightTests.Components.Fusion.Accordion.WhenUsingFusionAccordion`
- Locator: `tests/Alis.Reactive.Playwright.Extensions/FusionAccordionLocator.cs`
- Sandbox view: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Accordion/Index.cshtml`
- Route: `http://localhost:5220/Sandbox/Components/Accordion`
- Command: `scripts/playwright.sh --filter "FullyQualifiedName~Components.Fusion.Accordion.WhenUsingFusionAccordion"`
- Per-member fails-when-broken map: `proof/behavioral-coverage.json`

## Journey

A resident reads their personal "My Care Plan" page. The plan is organized into three
collapsible sections: My Care Team, My Services & Monthly Charges, and My Care Agreement.
Opening a section tells the resident which section they are reading; the monthly-charges
section loads its detail on demand the first time it is opened; and the care-agreement
section stays locked (overlaid) until the resident confirms they have reviewed their
agreement. Each behavior is one isolated nested vertical slice driven by real header
clicks and button clicks, with no DOM poking and no `page.evaluate()`. The page carries
only real-app elements — no echo spans, no debug panel, no plan-JSON dump.

## Member To Proof Map

| Onboarded member | Behavior (one user-visible journey) | Proving test | What breaks the test |
| --- | --- | --- | --- |
| `FusionAccordion(...)` render helper | the care plan opens showing its three sections | `the_care_plan_opens_showing_its_three_sections` | the builder stops rendering the accordion, so the three section headers do not appear and the agreement section is not overlaid (asserts all three headers and the `e-overlay` lock) |
| `Expanded` event selector | opening the care-team section shows its content and names it as open | `opening_the_care_team_section_shows_its_content_and_names_it_as_open` | the event stops firing the reactive handler when a section opens, so the open-section label never updates |
| `Reactive` event wiring | opening the care-team section shows its content and names it as open | `opening_the_care_team_section_shows_its_content_and_names_it_as_open` | the `.Reactive` wiring stops connecting `expanded` to its plan pipeline, so opening a section runs no reaction |
| `FusionAccordionExpandedArgs` | opening the care-team section shows its content and names it as open | `opening_the_care_team_section_shows_its_content_and_names_it_as_open` | the payload contract stops delivering the expand payload into the plan, so the open-section label does not change to "My Care Team" |
| `FusionAccordionExpandedArgs.Index` | opening a different section names that section, not the first | `opening_a_different_section_names_that_section_not_the_first` | `Index` stops carrying the real panel index, so opening the second section still names the first (asserts the label follows to "My Services & Monthly Charges") |
| `FusionAccordionExpandedArgs.IsExpanded` | closing the open section shows no section open | `closing_the_open_section_shows_no_section_open` | `IsExpanded` stops distinguishing expand from collapse, so collapsing a section does not route the Else branch (asserts the label becomes "No section open") |
| `ExpandItem(bool, int)` method | opening my care plan summary expands the care-team section | `opening_my_care_plan_summary_expands_the_care_team_section` | `ExpandItem` stops expanding the addressed panel, so the care-team content stays hidden after the summary button click (asserts the content is hidden before, visible after) |
| `EnableItem(int, bool)` method | confirming the care agreement unlocks the agreement section | `confirming_the_care_agreement_unlocks_the_agreement_section` | `EnableItem` stops unlocking the addressed panel, so the agreement section keeps `e-overlay` and stays unreadable (asserts overlay present and content hidden while locked, overlay gone and content readable after confirming) |

## Proof Criteria Met

- real header clicks and button clicks drive every behavior; no DOM poking or `page.evaluate()`;
- the render proof asserts all three section headers and the initial locked (`e-overlay`) state;
- the `expanded` proof asserts the visible section content and the open-section label change together;
- the `Index` proof opens two different sections and asserts the label follows the panel index;
- the `IsExpanded` proof opens then collapses a section and asserts the label returns to "No section open";
- the `ExpandItem` proof asserts the care-team content is hidden before the button and visible after;
- the `EnableItem` proof asserts the section is overlaid and unreadable while locked, and readable only after the overlay is removed;
- a further journey test (`opening_the_charges_section_loads_this_months_charges`) proves the `Index == 1` branch drives the HTTP load-on-expand, asserting the fetched charges (billing period and "$6,090") become visible;
- every test asserts no console errors.

## Coverage Completeness

`proof/typed-api-coverage-matrix.md` lists all 8 typed `FusionAccordion` members and is
`audited` with every row `row-proven`. `proof/behavioral-coverage.json` maps each of the
8 members to the named test above whose assertion is unsatisfiable by that member's
defect. No onboarded member is left without a fails-when-broken assertion.
