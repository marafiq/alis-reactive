# Button Playwright Proof

Status: audited. Every onboarded `FusionButton` member has focused typed DSL
Playwright proof through the Daily Wellness Check-In journey. Each member is bound
to a fails-when-broken assertion, recorded per member in
`proof/behavioral-coverage.json`. That behavioral coverage was verified green
against the latest run by the onboarding behavioral-coverage gate, which is the
authority for the per-member outcome rather than any single static log path.

## Proof Surface

- Test file: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Button/WhenUsingFusionButton.cs`
- Test class: `Alis.Reactive.PlaywrightTests.Components.Fusion.Button.WhenUsingFusionButton`
- Sandbox view: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Button/Index.cshtml`
- Route: `http://localhost:5220/Sandbox/Components/Button`
- Command: `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Button.WhenUsingFusionButton"`
- Per-member fails-when-broken map: `proof/behavioral-coverage.json`

## Journey

A care-team member runs a resident's Daily Wellness Check-In. The check-in opens
personalised and locked; confirming the resident's identity unlocks the action;
the visit's priority is set (complete icon, urgent style, recommended, follow-up);
the action can be triggered or focused for them; then the check-in is recorded and
the server confirms it. Each behavior is one isolated nested vertical slice driven
by real button clicks, with no DOM poking and no `page.evaluate()` (the gather
test is the single allowed `request.PostData` assertion). EJ2 controls are driven
by trusted `.ClickAsync()` clicks.

## Member To Proof Map

| Onboarded member | Behavior (one user-visible journey) | Proving test | What breaks the test |
| --- | --- | --- | --- |
| `FusionButton(...)` render helper | the check-in opens personalised and ready to confirm | `the_checkin_opens_personalised_and_ready_to_confirm` | the helper stops rendering the Syncfusion action, so the personalised action is not visible on open |
| `SetContent(string)` | the check-in opens personalised and ready to confirm | `the_checkin_opens_personalised_and_ready_to_confirm` | the DomReady write stops personalising the label, so the action keeps the builder text instead of "Begin check-in for Eleanor Whitfield" |
| `Content()` read source | the check-in opens personalised and ready to confirm | `the_checkin_opens_personalised_and_ready_to_confirm` | the source stops reading the label, so the visible "Action ready" line does not show the personalised label |
| `SetDisabled(bool)` | confirming identity unlocks the action and locking disables it | `confirming_identity_unlocks_the_action_and_locking_disables_it` | the write stops changing the enabled state, so Confirm leaves it disabled and Lock leaves it enabled |
| `Disabled()` read source | checking readiness reports whether the visit is locked or ready | `checking_readiness_reports_whether_the_visit_is_locked_or_ready` | the source stops feeding the readiness condition, so the message no longer switches between the locked and ready text |
| `SetIcon(string, FusionButtonIconPosition)` | marking the visit complete shows a check icon after the label | `marking_the_visit_complete_shows_a_check_icon_after_the_label` | the write stops updating the icon, so the icon span does not become `e-check` in the `e-icon-right` position |
| `SetCssClass(string)` | flagging the visit urgent restyles the action | `flagging_the_visit_urgent_restyles_the_action` | the write stops applying the classes, so the action does not gain `e-warning urgent-visit` |
| `SetPrimary(bool)` | recommending the check-in promotes it to the primary action | `recommending_the_checkin_promotes_it_to_the_primary_action` | the write stops promoting the action, so it does not gain `e-primary` |
| `SetToggle(bool)` | enabling follow-up then reminding latches the action active | `enabling_follow_up_then_reminding_latches_the_action_active` | the write stops making the action a toggle, so a later `Click()` does not latch `e-active` |
| `Click()` method | enabling follow-up then reminding latches the action active | `enabling_follow_up_then_reminding_latches_the_action_active` | the method stops invoking the click, so the toggled action never latches `e-active` (distinct from the not-active-before assertion) |
| `FocusIn()` method | jumping to the action moves keyboard focus onto it | `jumping_to_the_action_moves_keyboard_focus_onto_it` | the method stops moving focus, so the action is not focused after Jump |
| `CssClass()` read source | recording the check-in confirms its recommendation, follow-up, and priority style | `recording_the_checkin_confirms_its_recommendation_followup_and_priority_style` | the source stops yielding the classes, so the confirmation no longer reports "Priority style: e-warning urgent-visit" |
| `IsPrimary()` read source | recording the check-in confirms its recommendation, follow-up, and priority style | `recording_the_checkin_confirms_its_recommendation_followup_and_priority_style` | the source stops yielding the recommended flag, so the confirmation drops "as the recommended next step" |
| `IsToggle()` read source | recording the check-in confirms its recommendation, follow-up, and priority style | `recording_the_checkin_confirms_its_recommendation_followup_and_priority_style` | the source stops yielding the follow-up flag, so the confirmation drops "with a follow-up flagged" |

A separate framework gather test,
`recording_the_checkin_posts_the_action_state_to_the_server`, asserts the
`request.PostData` carries `action`, `priority`, `recommended`, and `followUp`
under their declared keys — the single allowed `PostData` assertion.

## Proof Criteria Met

- real button clicks drive every behavior; no DOM poking or `page.evaluate()`;
- the render proof asserts the personalised label is bound at first paint and read back into the "Action ready" line;
- the `SetDisabled` proof asserts the action becomes enabled after Confirm and disabled after Lock;
- the `Disabled()` proof asserts the locked and ready readiness messages in sequence over the condition;
- the `SetIcon` proof asserts the icon span gains `e-check` and `e-icon-right`;
- the `SetCssClass`/`SetPrimary` proofs assert the action gains `e-warning urgent-visit` and `e-primary` only after the gesture, and not before;
- the `SetToggle` + `Click()` proof asserts `e-active` is absent before and present after enabling follow-up and clicking;
- the `FocusIn` proof asserts the action is not focused before Jump and focused after;
- the read-source proofs assert the recommended phrasing, follow-up phrasing, and priority style in the server confirmation, plus the gather POST body keys;
- every test asserts no console errors.

## Coverage Completeness

`proof/typed-api-coverage-matrix.md` lists all 14 typed `FusionButton` members and
is `audited` with every row `row-proven`. `proof/behavioral-coverage.json` maps
each of the 14 members to the named test above whose assertion is unsatisfiable by
that member's defect. No onboarded member is left without a fails-when-broken
assertion.
