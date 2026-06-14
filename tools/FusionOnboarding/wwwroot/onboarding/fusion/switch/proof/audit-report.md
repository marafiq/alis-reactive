# FusionSwitch Audit Report

Status: 7 of 8 typed public members behaviorally proven; 1 member is an unprovable
framework-slice onboarding defect (see Defect below). Component NOT closed.

Branch reviewed: tiny-safe-but-important-refactorings (working tree at HEAD).

## Journey

A resident manages their **Care Alert Preferences** at `/Sandbox/Components/Switch`:
a master "receive care alerts" switch, and two delivery-channel switches (email
reminders, text-message alerts). The resident toggles the master, pauses all alerts
with a button, reviews each channel, and saves — the server confirms the channels.

Real-app page only: no echo spans, no plan-JSON panel, no debug buttons. The only
explicit-ID elements are product status/confirmation text (`alerts-status`,
`email-review`, `text-review`, `save-confirmation`).

## Parity

`compute-fusion-parity.mjs --component switch` -> **14/14 = 100.0% PASS**.
- builder-owned: 9 (cssClass, disabled, name, offLabel, onLabel, value, beforeChange, change, created)
- onboarded-typed: 1 (`checked` — backs `SetChecked`/`Value`)
- excluded-with-evidence: 4 (`destroy` skip; `toggle`/`click`/`focusIn` in `discovery/parity-accounting.json`,
  each excluded with a source-grounded reason from switch.js — redundant with the typed
  `SetChecked` write, or incidental native DOM focus)

## Accepted members (proven, row-proven against the latest TRX)

| Member | Proving test | Catches |
|---|---|---|
| `FusionSwitch(...)` HTML extension | `preferences_open_showing_the_resident_saved_toggles` | builder stops rendering switches bound to model |
| `Reactive` | `turning_care_alerts_off_tells_the_resident_alerts_are_paused` | event wiring stops connecting change to pipeline |
| `Changed` | `turning_care_alerts_off_tells_the_resident_alerts_are_paused` | change stops firing the handler |
| `FusionSwitchChangeArgs` | `turning_care_alerts_off_tells_the_resident_alerts_are_paused` | payload stops reaching the plan |
| `FusionSwitchChangeArgs.Checked` | `turning_care_alerts_back_on_tells_the_resident_to_pick_channels` | Checked stops carrying state (proven both directions) |
| `SetChecked(...)` | `pausing_all_alerts_turns_the_master_switch_off` | SetChecked stops writing checked=false |
| `Value(...)` | `saving_posts_each_switch_value_to_the_server` | Value stops yielding live state into gather |

All 9 tests in `WhenSwitchToggles` passed (TRX `playwright-20260614-010016.trx`, exit 0).
Browser-verified by eyes on `/Sandbox/Components/Switch` before the suite: master toggle
flips the status text; Pause button unchecks the switch; review buttons read live state
("Email reminders are on."/"...off."); Save POSTs `{receiveCareAlerts, emailReminders,
textMessageAlerts}` and shows the server summary.

## Defect — `FusionSwitchChangeArgs.IsInteracted` (UNCOVERED, blocks closeout)

`IsInteracted` is a typed public C# property on `FusionSwitchChangeArgs` that maps to a
vendor payload key the Syncfusion switch `change` event never emits. Confirmed three ways:

1. **Source**: `node_modules/@syncfusion/ej2-buttons/src/switch/switch.js:97` builds
   `changeEventArgs = { checked, event }` — no `isInteracted`. (The switch `change` fires
   only on user interaction; switch.js:222 shows a programmatic `checked` set goes through
   `changeState` and does NOT trigger `change`.)
2. **Generated discovery**: `discovery/event-payload-surface.json` lists `ChangeEventArgs`
   members as `checked`, `event`, `name` — no `isInteracted`.
3. **Live browser capture**: on `/Sandbox/Components/Switch`, attaching a listener to the
   EJ2 switch instance and performing a trusted toggle returned payload keys
   `[checked, event, name]`, `hasIsInteracted=false`. The runtime adapter
   `Alis.Reactive.Assets/runtime/events/event-fusion.ts` passes the vendor args through
   unchanged and injects nothing.

So `args.IsInteracted` is always `undefined` at runtime; there is no gesture that makes it
observably true. By BDD Rule 3 (fails-when-broken) no honest test can cover it — the member
is already effectively broken, so any assertion would be a pass-hack.

The skill's prescribed correction is to keep an unprovable member out of the public Fusion
slice — i.e. remove `IsInteracted` from
`Alis.Reactive.Fusion/Components/FusionSwitch/Events/FusionSwitchOnChanged.cs`. That edits
framework code, which the task's CARDINAL RULE forbids. The defect is therefore reported,
not faked or patched. `0b` correctly reports `7/8 members mapped` and FAILS on this member.

Likely root cause: `IsInteracted` was copied from the input-component event template (where
EJ2 `ChangedEventArgs` for DatePicker/NumericTextBox/Slider/Rating genuinely carries
`isInteracted`) into the buttons `change` payload, which does not.

## Commands run

- `node .../compute-fusion-parity.mjs --component switch` -> 14/14 = 100.0% PASS
- `node .../write-fusion-typed-api-coverage.mjs --component switch --fusion-type FusionSwitch --write` -> 8 rows
- `scripts/playwright.sh --filter "FullyQualifiedName~Components.Fusion.Switch."` -> 9/9 Passed
- `node .../verify-behavioral-coverage.mjs --component switch` -> 7/8 mapped, FAIL on FusionSwitchChangeArgs.IsInteracted
