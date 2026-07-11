# Toolbar Playwright Proof

Status: audited. Every onboarded `FusionToolbar` member has focused typed DSL
Playwright proof through the resident account command-bar journey. Each member is
bound to a fails-when-broken assertion, recorded per member in
`proof/behavioral-coverage.json`. That behavioral coverage was verified green
against the latest run by the onboarding behavioral-coverage gate, which is the
authority for the per-member outcome rather than any single static log path.

## Proof Surface

- Test file: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Toolbar/WhenUsingFusionToolbar.cs`
- Test class: `Alis.Reactive.PlaywrightTests.Components.Fusion.Toolbar.WhenUsingFusionToolbar`
- Sandbox view: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Toolbar/Index.cshtml`
- Route: `http://localhost:5220/Sandbox/Components/FusionToolbar`
- Command: `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Toolbar.WhenUsingFusionToolbar"`
- Per-member fails-when-broken map: `proof/behavioral-coverage.json`

## Journey

A resident manages their account from a command bar. They can request maintenance
or message their care team, which records the started action in a status banner;
they can pay their balance, which locks the command bar, posts the clicked command
to the server, and shows the server's confirmation; and "Done" unlocks the bar.
Each behavior is one isolated nested vertical slice driven by real toolbar-item
clicks and button clicks, with no DOM poking and no `page.evaluate()` (the gather
test is the single allowed `request.PostData` assertion).

## Member To Proof Map

| Onboarded member | Behavior (one user-visible journey) | Proving test | What breaks the test |
| --- | --- | --- | --- |
| `FusionToolbar(...)` render helper | the account command bar opens with the resident's actions | `the_account_command_bar_opens_with_the_residents_actions` | the builder stops rendering the command bar (asserts the toolbar root is visible and shows the three command labels) |
| `Clicked` event selector | requesting maintenance shows which action the resident started | `requesting_maintenance_shows_which_action_the_resident_started` | the event stops firing when a command is clicked, so the status banner never appears |
| `Reactive` event wiring | requesting maintenance shows which action the resident started | `requesting_maintenance_shows_which_action_the_resident_started` | the `.Reactive` wiring stops connecting `clicked` to its plan pipeline, so clicking runs no reaction |
| `FusionToolbarClickedArgs` | requesting maintenance shows which action the resident started | `requesting_maintenance_shows_which_action_the_resident_started` | the payload contract stops delivering the click payload into the plan, so the banner that reads from it never updates |
| `FusionToolbarClickedArgs.Item` | requesting maintenance shows which action the resident started | `requesting_maintenance_shows_which_action_the_resident_started` | `Item` stops carrying the clicked item object, so the `Item.Text` read yields nothing and the banner is empty |
| `FusionToolbarItem.Text` | requesting maintenance shows which action the resident started | `requesting_maintenance_shows_which_action_the_resident_started` | `Text` stops carrying the clicked command's label (asserts the banner reads exactly "Request maintenance") |
| `FusionToolbarItem` | messaging the care team shows that action started | `messaging_the_care_team_shows_that_action_started` | the typed item contract stops carrying per-item identity, so a second distinct command cannot show its own label (asserts the banner reads "Message care team", not the prior command's text) |
| `FusionToolbarItem.Id` | paying the balance runs the payment workflow and shows the server confirmation | `paying_the_balance_runs_the_payment_workflow_and_shows_the_server_confirmation` | `Id` stops carrying the clicked id, so the `When(args, x => x.Item.Id).Eq("pay-balance")` branch never matches and the server payment confirmation never appears |
| `Disable(bool)` method | paying locks the command bar and done unlocks it | `paying_locks_the_command_bar_and_done_unlocks_it` | `Disable` stops toggling the toolbar disabled state (asserts the root gains `e-overlay` after Pay balance and loses it after Done) |
| `FusionToolbarItem.Disabled` | paying posts the clicked command payload to the server | `paying_posts_the_clicked_command_payload_to_the_server` | `Disabled` stops riding the gather body (asserts the POST carries `"commandDisabled":false` under its declared key, P025) |

## Proof Criteria Met

- real toolbar-item clicks and button clicks drive every behavior; no DOM poking or `page.evaluate()`;
- the render proof asserts the command bar root is visible and all three command labels render;
- the `clicked` proof asserts the visible status banner names the started action through `Item.Text`;
- a second command proves `FusionToolbarItem` carries each item's own label, not a constant;
- the `Item.Id` proof asserts the Pay-balance branch reaches the server confirmation rather than the Else status branch;
- the `Disable` proof asserts the `e-overlay` lock appears after paying and is gone after Done;
- the gather proof asserts the POST body carries `"commandId":"pay-balance"`, `"commandText":"Pay balance"`, and `"commandDisabled":false` under their declared keys;
- every test asserts no console errors.

## Coverage Completeness

`proof/typed-api-coverage-matrix.md` lists all 10 typed `FusionToolbar` members
and is `audited` with every row `row-proven`. `proof/behavioral-coverage.json`
maps each of the 10 members to the named test above whose assertion is
unsatisfiable by that member's defect. No onboarded member is left without a
fails-when-broken assertion.
