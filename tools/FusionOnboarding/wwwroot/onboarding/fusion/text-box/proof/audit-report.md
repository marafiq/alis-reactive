# TextBox Audit Report

Status: audited and closed. `FusionTextBox` is fully onboarded. Every public
typed member has deterministic raw EJ2 evidence, an explicit C# name decision, an
authoritative primitive mapping, a vertical-slice file, and focused typed DSL
Playwright proof bound to a fails-when-broken assertion. The generated typed API
coverage matrix is `audited` with every row `row-proven`.

## Accepted Members

The TextBox slice onboards the post-render runtime surface; initial render
configuration stays on the Syncfusion `TextBoxBuilder`.

- `FusionTextBox(this InputBoundField<TModel, TProp> setup, Action<TextBoxBuilder> build)` — the field render helper, bound to a string model property.
- `FusionTextBoxEvents.Input` — the `input` event selector; `FusionTextBoxInputArgs` carries `Value` and `PreviousValue`.
- `FusionTextBoxEvents.Changed` — the `change` event selector; `FusionTextBoxChangeArgs` carries `Value`, `PreviousValue`, and `IsInteracted`.
- `FusionTextBoxEvents.Focus` — the `focus` event selector; `FusionTextBoxFocusArgs` carries `Value`.
- `FusionTextBoxEvents.Blur` — the `blur` event selector; `FusionTextBoxBlurArgs` carries `Value`.
- `Reactive(...)` — the event-to-plan wiring through `ComponentEventOnboarding.Wire`.
- `SetValue(this ComponentRef<FusionTextBox, TModel> self, string? value)` — writes `value` and chains `dataBind()` to repaint the input.
- `FocusIn(this ComponentRef<FusionTextBox, TModel> self)` — calls `focusIn()` to move focus into the input.
- `FocusOut(this ComponentRef<FusionTextBox, TModel> self)` — calls `focusOut()` to remove focus from the input.
- `AddAppendIcon(this ComponentRef<FusionTextBox, TModel> self, string iconCssClass)` — calls `addIcon("append", css)` to render an append icon inside the input group.
- `Value(this ComponentRef<FusionTextBox, TModel> self)` — reads `value` as a typed `string` source for gather, conditions, and set text.

## Excluded Candidates

- `addAttributes({[k]:string})` — arbitrary string-to-string attribute dictionary; a stringly surface barred from a typed component slice. Initial HTML attributes are builder-owned via `TextBoxBuilder.HtmlAttributes`. Recorded in `discovery/parity-accounting.json`.
- `removeAttributes(string[])` — arbitrary attribute-name string array; the same stringly-surface exclusion. Recorded in `discovery/parity-accounting.json`.
- `readonly` (`boolean`) — builder-owned initial render (`TextBoxBuilder.Readonly`); the runtime disable case is served by the builder-owned `enabled` property. A runtime read-only toggle is a deferred candidate, not onboarded without behavior proof. Recorded in `discovery/parity-accounting.json`.
- `created`, `destroyed` events and `destroy()` method — lifecycle-only; `destroy` classified `skip` in `discovery/public-api-surface.json`; no typed payload.
- `appendTemplate`, `autocomplete`, `cssClass`, `enabled`, `enablePersistence`, `floatLabelType`, `multiline`, `placeholder`, `prependTemplate`, `showClearButton`, `type`, initial `value`, `width` — builder-owned static configuration (`discovery/public-api-surface.json` marks each `builder.covered = true`); no further post-render read/write proven necessary.

## Parity Accounting

`node .claude/skills/onboard-fusion-component/scripts/compute-fusion-parity.mjs --component text-box`
reports 26/26 = 100.0% (threshold 95%): 19 builder-owned, 3 onboarded-typed
(`addIcon`, `focusIn`, `focusOut` matched by vendor name), 4
excluded-with-evidence (`addAttributes`, `removeAttributes`, `readonly`, and the
`skip`-classified `destroy`), 0 unaccounted. The judgment is recorded in
`discovery/parity-accounting.json`.

## Skill Pattern Map

This audit confirms the following reusable patterns in `_skill/pattern-map.md`.
No new pattern row was required: the TextBox slice is a clean application of
existing precedent, with every defect class already covered.

- `_skill/pattern-map.md#p000-discovery-is-exhaustive-but-public-c-dsl-is-selective` — 26 EJ2 members discovered; 11 public C# members accepted (plus four event selectors and the wiring), the rest recorded and excluded with evidence.
- `_skill/pattern-map.md#p005-event-payload-coverage-is-property-level` — each event payload is proven property by property (`Value`, `PreviousValue`, `IsInteracted`), not as one class row.
- `_skill/pattern-map.md#p016-shared-payload-types-do-not-prove-shared-event-rows` — `focus` (`FocusInEventArgs`) and `blur` (`FocusOutEventArgs`) are separate event selectors with their own payloads and their own focus/blur proofs, not collapsed into one row.
- `_skill/pattern-map.md#p018-writable-payload-fields-need-lifecycle-effect-proof` — `SetValue` is proven by the visible input value and the directory preview changing, not by a silent property set; `IsInteracted=false` is proven by the programmatic-fill branch the user sees.
- `_skill/pattern-map.md#p020-public-methods-need-their-own-row-proof` — `SetValue`, `FocusIn`, `FocusOut`, `AddAppendIcon`, and the `Value` read each have their own row and their own fails-when-broken proof.

## Remote Data Lane

`TextBox` is a single-value string input, not a data-capable component: it binds
to one model property and emits one string. It has no remote binding, paging,
filtering, lookup, virtualization, or server-query lane to onboard. The realistic
remote workflow for a text field is sending the entered value to the server,
which the `Value()` gather row proves: the profile POSTs `"preferredName":"Margie"`
and `"dietaryNote":"Low sodium, no shellfish"` and the coordinator sees the server
confirmation. This satisfies the remote-behavior expectation for this component
class (see `references/automation-gates.md` Gate 6 applicability — TextBox is not
a board/grid/scheduler/list/tree).

## Commands Run

- raw probe and trace, real headless browser:
  `node .claude/skills/onboard-fusion-component/scripts/run-fusion-probe-trace.mjs --component text-box --api-set core`
  — wrote `traces/raw-ej2-core.trace.json` (4 trace rows: `created`, `ready`, the instance `own keys` including `inputPreviousValue`, and `prototype methods` showing `addAttributes`, `addIcon`, `blur`, `change`, and `created`).
- discovery artifacts from current source:
  `node .claude/skills/onboard-fusion-component/scripts/write-fusion-discovery-artifacts.mjs --component text-box --fusion-type FusionTextBox --class TextBox --namespace inputs --dts node_modules/@syncfusion/ej2-inputs/src/textbox/textbox.d.ts --js node_modules/@syncfusion/ej2-inputs/src/textbox/textbox.js --xml ~/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml --blazor-package Syncfusion.Blazor.Inputs --blazor-version 32.2.8 --write`.
- parity accounting:
  `node .claude/skills/onboard-fusion-component/scripts/compute-fusion-parity.mjs --component text-box` — 26/26 = 100.0%, PASS.
- typed API coverage matrix, generated from current source and run truth:
  `node .claude/skills/onboard-fusion-component/scripts/write-fusion-typed-api-coverage.mjs --component text-box --fusion-type FusionTextBox --write` — `audited`, all rows `row-proven`.
- onboarding status inventory:
  `node .claude/skills/onboard-fusion-component/scripts/report-fusion-onboarding-status.mjs --write`.
- behavioral coverage gate (0b):
  `node .claude/skills/onboard-fusion-component/scripts/verify-behavioral-coverage.mjs --component text-box` — PASS, 22/22 members mapped.
- artifact gate verifier:
  `node .claude/skills/onboard-fusion-component/scripts/verify-fusion-artifact-gates.mjs --component text-box`.
- behavior proof:
  `scripts/playwright.sh --filter "FullyQualifiedName~Components.Fusion.TextBox.WhenUsingFusionTextBox"` — 11 of 11 passed; the per-member outcome is recorded green in `proof/behavioral-coverage.json` and verified by the onboarding behavioral-coverage gate.

## Coverage Matrix Summary

Current generated matrix count: 22 typed C# API rows, 0 supplemental audit rows,
22 total rows. The latest count check shows 22 row-proven matrix rows and 0
matrix rows without `row-proven` status.

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

This audit closes the TextBox component row. The artifacts agree end to end on the
same member shape, payload shape, sync lane, C# API, runtime behavior, and
Playwright proof. The discovery JSON, trace, and coverage matrix remain generated
by the skill scripts and were not hand-edited.
