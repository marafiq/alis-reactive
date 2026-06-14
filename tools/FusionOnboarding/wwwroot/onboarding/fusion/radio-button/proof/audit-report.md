# RadioButton Audit Report

Status: audited and closed. `FusionRadioButton` is fully onboarded. Every public
typed member has deterministic raw EJ2 evidence, an explicit C# name decision,
an authoritative primitive mapping, a vertical-slice file, and focused typed DSL
Playwright proof bound to a fails-when-broken assertion. The generated typed API
coverage matrix is `audited` with every row `row-proven`.

## Accepted Members

The RadioButton slice onboards the post-render runtime surface; initial option
configuration (label, name, value, checked) stays on the Syncfusion
`RadioButtonBuilder`.

- `FusionRadioButton(...)` — the element render helper that renders one EJ2 RadioButton with a stable element id and builder-owned options.
- `FusionRadioButtonEvents.Changed` — the `change` event selector, with `Reactive(...)` wiring through `ComponentEventOnboarding.Wire`.
- `FusionRadioButtonChangeArgs.Value` — the selected radio value, read from `event.value` (core trace row 7 shows `"Shared Companion Suite"`).
- `SetChecked(this ComponentRef<FusionRadioButton, TModel> self, bool isChecked)` — writes `checked` and chains `dataBind()` to repaint the radio.
- `SetDisabled(this ComponentRef<FusionRadioButton, TModel> self, bool disabled)` — writes `disabled` and chains `dataBind()` to repaint the radio.
- `Checked(this ComponentRef<FusionRadioButton, TModel> self)` — reads `checked` as a typed `bool` source for gather, conditions, and set text.
- `Disabled(this ComponentRef<FusionRadioButton, TModel> self)` — reads `disabled` as a typed `bool` source for conditions and set text.
- `SelectedValue(this ComponentRef<FusionRadioButton, TModel> self)` — calls `getSelectedValue()` and yields the group's selected value as a typed `string` source.
- `Click(this ComponentRef<FusionRadioButton, TModel> self)` — calls `click()` to select the radio.
- `FocusIn(this ComponentRef<FusionRadioButton, TModel> self)` — calls `focusIn()` to move keyboard focus into the radio.

## Excluded Candidates

- `change.event` — browser-owned DOM `Event` (core trace row 7 `event.sample { isTrusted: true }`); excluded from the public typed payload per `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them`.
- `change.name` (inherited `BaseEventArgs.name`) — duplicate event identity metadata; the `Changed` selector already owns the event identity.
- `created` event — DOM-native lifecycle event with an undefined payload (core trace row 1); no focused Senior Living use case.
- `value` property as a standalone member — `discovery/public-api-surface.json` marks it `builder.covered = true`; the per-button value is configured on `RadioButtonBuilder` and surfaced through the `change.value` payload and the `getSelectedValue()` group read, so no standalone read/write member is proven necessary.
- `name`, `label`, `labelPosition`, `cssClass`, `enableHtmlSanitizer` — builder-owned static configuration (`discovery/public-api-surface.json` marks each `builder.covered = true`); no post-render read/write proven necessary.
- `destroy()` method — lifecycle cleanup, not plan behavior (`discovery/public-api-surface.json` classifies it `skip`).

## Skill Pattern Map

This audit confirms the following reusable patterns in `_skill/pattern-map.md`.
No new pattern row was required: the RadioButton slice is a clean application of
existing precedent, with every defect class already covered.

- `_skill/pattern-map.md#p000-discovery-is-exhaustive-but-public-c-dsl-is-selective` — 14 EJ2 members discovered; 9 public C# members accepted, the rest recorded and excluded with evidence.
- `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them` — `change.event` excluded as a browser-owned DOM object rather than exposed as `object`/`dynamic`.
- `_skill/pattern-map.md#p005-event-payload-coverage-is-property-level` — `FusionRadioButtonChangeArgs` is proven property by property (`Value`), not as one class row.
- `_skill/pattern-map.md#p018-writable-payload-fields-need-lifecycle-effect-proof` — `SetChecked` is proven by the visible radio becoming checked, and `SetDisabled` by the visible radio becoming disabled, not by a silent property set.
- `_skill/pattern-map.md#p020-public-methods-need-their-own-row-proof` — `SelectedValue`, `Click`, `FocusIn`, and the `Checked`/`Disabled` reads each have their own row and their own fails-when-broken proof.

## Remote Data Lane

`RadioButton` is a single-choice input, not a data-capable component: it renders
a group of options and emits one chosen value. It has no remote binding, paging,
filtering, lookup, virtualization, or server-query lane to onboard. The realistic
remote workflow for a radio choice is sending the chosen option to the server,
which the `Checked()` gather row proves: confirming the companion suite POSTs
`"companionSuiteChosen":true` and the resident sees the desk confirmation. This
satisfies the remote-behavior expectation for this component class (see
`references/automation-gates.md` Gate 6 applicability — RadioButton is not a
board/grid/scheduler/list/tree).

## Commands Run

- raw probe and trace, real headless browser:
  `node .claude/skills/onboard-fusion-component/scripts/run-fusion-probe-trace.mjs --component radio-button --api-set core`
  — wrote `traces/raw-ej2-core.trace.json` (14 trace rows, including the real `change` payload `{ event, name, value: "Shared Companion Suite" }`, `created`, the `checked`/`disabled`/`value` reads, `getSelectedValue` returning the group selection, the `click` and `focusIn` calls, the instance `own keys` including `isFocused` and `initialCheckedValue`, and `prototype methods` showing `click`, `focusIn`, `dataBind`, and `destroy`).
- typed API coverage matrix, current against source:
  `node .claude/skills/onboard-fusion-component/scripts/write-fusion-typed-api-coverage.mjs --component radio-button --fusion-type FusionRadioButton --check` — reported current.
- onboarding status inventory:
  `node .claude/skills/onboard-fusion-component/scripts/report-fusion-onboarding-status.mjs --write`.
- artifact gate verifier:
  `node .claude/skills/onboard-fusion-component/scripts/verify-fusion-artifact-gates.mjs --component radio-button`.
- behavior proof:
  `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.RadioButton.WhenUsingFusionRadioButton"` — the per-member outcome is recorded green in `proof/behavioral-coverage.json` and verified by the onboarding behavioral-coverage gate.

## Coverage Matrix Summary

Current generated matrix count: 12 typed C# API rows, 0 supplemental audit rows,
12 total rows. The latest count check shows 12 row-proven matrix rows and 0
matrix rows without `row-proven` status.

## Linked Artifacts

- Master index: `master-usecases-index.md`
- Source inventory: `discovery/source-inventory.md`
- Public API surface: `discovery/public-api-surface.json`
- Event payload surface: `discovery/event-payload-surface.json`
- MVC builder coverage: `discovery/mvc-builder-coverage.md`
- Blazor candidates: `discovery/blazor-candidates.md`
- Raw EJ2 core probe: `probes/raw-ej2-core.html`
- Raw EJ2 core trace: `traces/raw-ej2-core.trace.json`
- Primitive map: `mapping/primitive-map.md`
- C# name decisions: `mapping/csharp-name-decisions.md`
- Vertical slice plan: `mapping/vertical-slice-plan.md`
- Typed API coverage matrix: `proof/typed-api-coverage-matrix.md`
- Behavioral coverage: `proof/behavioral-coverage.json`
- Playwright proof: `proof/playwright-proof.md`
- Blind reviewer verdict: `proof/blind-review.md`
- Skill pattern map: `_skill/pattern-map.md`

## Commit Boundary

This audit closes the RadioButton component row. The artifacts agree end to end
on the same member shape, payload shape, sync lane, C# API, runtime behavior,
and Playwright proof. The discovery JSON, trace, and coverage matrix remain
generated by the skill scripts and were not hand-edited.
