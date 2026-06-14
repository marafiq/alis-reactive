# CheckBox Audit Report

Status: audited and closed. `FusionCheckBox` is fully onboarded. Every public
typed member has deterministic raw EJ2 evidence, an explicit C# name decision, an
authoritative primitive mapping, a vertical-slice file, and focused typed DSL
Playwright proof bound to a fails-when-broken assertion. The generated typed API
coverage matrix is `audited` with every row `row-proven`.

## Accepted Members

The CheckBox slice onboards the post-render runtime surface; initial render
configuration stays on the Syncfusion `CheckBoxBuilder`.

- `FusionCheckBox(this InputBoundField<TModel, bool> setup, Action<CheckBoxBuilder> build)` — the field render helper, bound to a boolean model property.
- `FusionCheckBoxEvents.Changed` — the `change` event selector, with `Reactive(...)` wiring through `ComponentEventOnboarding.Wire`.
- `FusionCheckBoxChangeArgs.Checked` — the checked state after the change, read from `event.checked`.
- `SetChecked(this ComponentRef<FusionCheckBox, TModel> self, bool isChecked)` — writes `checked` and chains `dataBind()` to repaint the box.
- `SetIndeterminate(this ComponentRef<FusionCheckBox, TModel> self, bool isIndeterminate)` — writes `indeterminate` and chains `dataBind()`; the box shows the `e-stop` dash.
- `SetDisabled(this ComponentRef<FusionCheckBox, TModel> self, bool disabled)` — writes `disabled` and chains `dataBind()`; the wrapper carries `e-checkbox-disabled`.
- `Click(this ComponentRef<FusionCheckBox, TModel> self)` — calls `click()`; toggles the checked state and fires `change`.
- `FocusIn(this ComponentRef<FusionCheckBox, TModel> self)` — calls `focusIn()`; moves focus into the input.
- `Checked(this ComponentRef<FusionCheckBox, TModel> self)` — reads `checked` as a typed `bool` source for gather, conditions, and set text.
- `Indeterminate(this ComponentRef<FusionCheckBox, TModel> self)` — reads `indeterminate` as a typed `bool` source.
- `Disabled(this ComponentRef<FusionCheckBox, TModel> self)` — reads `disabled` as a typed `bool` source.

The 13 typed API coverage matrix rows are these members plus the
`FusionCheckBoxChangeArgs` payload contract row and the `Reactive` event-wiring
row that carry them.

## Excluded Candidates

- `change.event` — browser-owned DOM `Event` (raw trace sample `isTrusted`); excluded from the public typed payload per `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them`.
- `change.name` (inherited `BaseEventArgs.name`) — duplicate event identity metadata (raw trace `name: "change"`); the `Changed` selector already owns the event identity.
- `created` event — DOM-native lifecycle event with no typed payload (`discovery/event-payload-surface.json` marks it dom-native); no focused Senior Living use case.
- `cssClass`, `enableHtmlSanitizer`, `label`, `labelPosition`, `name`, `value` — builder-owned initial-render configuration (`discovery/public-api-surface.json` marks each `builder.covered = true`); no post-render read/write proven necessary.
- `indeterminate` and `disabled` first-paint values — builder-owned at initial render; their accepted rows above are the proven post-render read/write behaviors, not the initial-render configuration.
- `destroy()` method — lifecycle cleanup, not plan behavior (`discovery/public-api-surface.json` classifies it `skip`).

## Parity Accounting

`discovery/parity-accounting.json` records the per-component parity judgment
consumed by `compute-fusion-parity.mjs`: the 10 builder-covered members and the
`destroy` skip-decision member are accounted by the surface itself, and the three
members that are neither builder-covered nor skipped (`checked`, `click`,
`focusIn`) are accounted as onboarded-typed. Generated parity is 14/14 = 100.0%.

## Skill Pattern Map

This audit confirms the following reusable patterns in `_skill/pattern-map.md`.
No new pattern row was required: the CheckBox slice is a clean application of
existing precedent, with every defect class already covered.

- `_skill/pattern-map.md#p000-discovery-is-exhaustive-but-public-c-dsl-is-selective` — 14 EJ2 members discovered; the typed C# surface accepts the proven runtime members and excludes builder-owned and lifecycle candidates with evidence.
- `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them` — `change.event` excluded as a browser-owned DOM object rather than exposed as `object`/`dynamic`.
- `_skill/pattern-map.md#p005-event-payload-coverage-is-property-level` — `FusionCheckBoxChangeArgs` is proven by its property `Checked`, not as one opaque class row.
- `_skill/pattern-map.md#p018-writable-payload-fields-need-lifecycle-effect-proof` — `SetChecked`, `SetIndeterminate`, and `SetDisabled` are proven by the visible box state changing (checkmark, `e-stop` dash, `e-checkbox-disabled`), not by a silent property set.
- `_skill/pattern-map.md#p020-public-methods-need-their-own-row-proof` — `Click`, `FocusIn`, and each read source have their own row and their own fails-when-broken proof.

## Remote Data Lane

`CheckBox` is a boolean single-value input, not a data-capable component: it binds
to one boolean model property and emits one scalar. It has no remote binding,
paging, filtering, lookup, virtualization, or server-query lane to onboard. The
realistic remote workflow for a checkbox is sending the chosen election to the
server, which the `Checked()`/`Indeterminate()` gather row proves: the move-in
form POSTs `"agreementAccepted":true` and `"housekeepingNeedsFollowUp":true` and
the resident sees the server confirmation. This satisfies the remote-behavior
expectation for this component class (see `references/automation-gates.md` Gate 6
applicability — CheckBox is not a board/grid/scheduler/list/tree).

## Commands Run

- raw probe and trace, real headless browser:
  `node .claude/skills/onboard-fusion-component/scripts/run-fusion-probe-trace.mjs --component check-box --api-set core`
  — wrote `traces/raw-ej2-core.trace.json` (15 trace rows, including the `change` payload keys `checked`/`event`/`name`, the `click()` toggle of `checked`, the `indeterminate`/`disabled` reads and their `e-stop`/`e-checkbox-disabled` class evidence after `dataBind()`, the `focusIn()` active element, and `prototype methods` showing `click`, `focusIn`, `disabled`, `checked`, and `change`).
- parity, generated:
  `node .claude/skills/onboard-fusion-component/scripts/compute-fusion-parity.mjs --component check-box` — 14/14 = 100.0% PASS.
- typed API coverage matrix, current against source:
  `node .claude/skills/onboard-fusion-component/scripts/write-fusion-typed-api-coverage.mjs --component check-box --fusion-type FusionCheckBox --write` — 13 typed rows, all `row-proven`.
- onboarding status inventory:
  `node .claude/skills/onboard-fusion-component/scripts/report-fusion-onboarding-status.mjs --write`.
- behavioral coverage gate:
  `node .claude/skills/onboard-fusion-component/scripts/verify-behavioral-coverage.mjs --component check-box` — 13/13 members mapped, all green against the latest TRX.
- artifact gate verifier:
  `node .claude/skills/onboard-fusion-component/scripts/verify-fusion-artifact-gates.mjs --component check-box`.
- behavior proof:
  `scripts/playwright.sh --filter "FullyQualifiedName~Components.Fusion.CheckBox.WhenUsingFusionCheckBox"` — 9 tests, all Passed; the per-member outcome is recorded green in `proof/behavioral-coverage.json` and verified by the onboarding behavioral-coverage gate.

## Coverage Matrix Summary

Current generated matrix count: 13 typed C# API rows, 0 supplemental audit rows,
13 total rows. The latest count check shows 13 row-proven matrix rows and 0
matrix rows without `row-proven` status.

| Item | Count |
|---|---:|
| Typed C# API rows | 13 |
| Supplemental audit rows | 0 |
| Total typed coverage matrix rows | 13 |

## Linked Artifacts

- Master index: `master-usecases-index.md`
- Source inventory: `discovery/source-inventory.md`
- Public API surface: `discovery/public-api-surface.json`
- Event payload surface: `discovery/event-payload-surface.json`
- MVC builder coverage: `discovery/mvc-builder-coverage.md`
- Blazor candidates: `discovery/blazor-candidates.md`
- Parity accounting: `discovery/parity-accounting.json`
- Raw EJ2 core probe: `probes/raw-ej2-core.html`
- Raw EJ2 core trace: `traces/raw-ej2-core.trace.json`
- Primitive map: `mapping/primitive-map.md`
- C# name decisions: `mapping/csharp-name-decisions.md`
- Vertical slice plan: `mapping/vertical-slice-plan.md`
- Typed API coverage matrix: `proof/typed-api-coverage-matrix.md`
- Behavioral coverage: `proof/behavioral-coverage.json`
- Playwright proof: `proof/playwright-proof.md`
- Skill pattern map: `_skill/pattern-map.md`

## Commit Boundary

This audit closes the CheckBox component row. The artifacts agree end to end on
the same member shape, payload shape, sync lane, C# API, runtime behavior, and
Playwright proof. The discovery JSON, trace, and coverage matrix remain generated
by the skill scripts and were not hand-edited; the parity accounting and the
mapping and proof narratives are the named judgment artifacts.
