# NumericTextBox Audit Report

Status: audited and closed. `FusionNumericTextBox` is fully onboarded. Every
public typed member has deterministic raw EJ2 evidence, an explicit C# name
decision, an authoritative primitive mapping, a vertical-slice file, and focused
typed DSL Playwright proof bound to a fails-when-broken assertion. The generated
typed API coverage matrix is `audited` with every row `row-proven`.

## Accepted Members

The NumericTextBox slice onboards the post-render runtime surface; initial render
configuration stays on the Syncfusion `NumericTextBoxBuilder`.

- `FusionNumericTextBox(this InputBoundField<TModel, TProp> setup, Action<NumericTextBoxBuilder> build)` — the field render helper, bound to a numeric model property.
- `FusionNumericTextBoxEvents.Changed` — the `change` event selector, with `Reactive(...)` wiring through `ComponentEventOnboarding.Wire`.
- `FusionNumericTextBoxEvents.Focus` — the `focus` event selector.
- `FusionNumericTextBoxEvents.Blur` — the `blur` event selector.
- `FusionNumericTextBoxChangeArgs.Value` — the new number, read from `event.value`.
- `FusionNumericTextBoxChangeArgs.PreviousValue` — the number before the change, read from `event.previousValue`.
- `FusionNumericTextBoxChangeArgs.IsInteracted` — user-typed versus programmatic-change flag, read from `event.isInteracted`.
- `FusionNumericTextBoxFocusArgs`, `FusionNumericTextBoxBlurArgs` — typed focus/blur payloads that carry no data the DSL reads (intentionally empty).
- `SetValue(this ComponentRef<FusionNumericTextBox, TModel> self, decimal value)` — writes the `value` property.
- `SetMin(this ComponentRef<FusionNumericTextBox, TModel> self, decimal min)` — writes the `min` property; subsequent below-floor entries clamp to the new minimum.
- `Increment(this ComponentRef<FusionNumericTextBox, TModel> self)` — calls `increment()`; the value rises one step.
- `Decrement(this ComponentRef<FusionNumericTextBox, TModel> self)` — calls `decrement()`; the value drops one step.
- `FocusIn(this ComponentRef<FusionNumericTextBox, TModel> self)` — calls `focusIn()`; the field gains focus.
- `FocusOut(this ComponentRef<FusionNumericTextBox, TModel> self)` — calls `focusOut()`; the field loses focus.
- `Value(this ComponentRef<FusionNumericTextBox, TModel> self)` — reads `value` as a typed `decimal` source for gather, conditions, and set text.

## Excluded Candidates

- `getText()` method — returns the vendor-formatted display string (Format/Currency/Decimals applied); the builder already owns the format, and the typed `Value()` read already covers the numeric value, so `getText` adds a duplicate format-coupled string read with no distinct typed use case. Recorded in `discovery/parity-accounting.json`.
- `readonly` property — initial read-only render configuration owned by the Syncfusion MVC builder (`numerictextbox.d.ts:139`); no post-render read/write proven. Recorded in `discovery/parity-accounting.json`.
- `created`, `destroyed` events — lifecycle-only events with no typed payload the DSL reads.
- `allowMouseWheel`, `appendTemplate`, `cssClass`, `currency`, `decimals`, `enabled`, `enablePersistence`, `floatLabelType`, `format`, `max`, `placeholder`, `prependTemplate`, `showClearButton`, `showSpinButton`, `step`, `strictMode`, `validateDecimalOnType`, `width`, and the initial `value`/`min` options — builder-owned static configuration (`discovery/public-api-surface.json` marks each `builder.covered = true`); no post-render read/write proven necessary beyond the accepted `value`/`min` writes.
- `destroy()` method — lifecycle cleanup, not plan behavior (`discovery/public-api-surface.json` classifies it `skip`).

## Skill Pattern Map

This audit confirms the following reusable patterns in `_skill/pattern-map.md`.
No new pattern row was required: the NumericTextBox slice is a clean application
of existing precedent, with every defect class already covered.

- `_skill/pattern-map.md#p000-discovery-is-exhaustive-but-public-c-dsl-is-selective` — 32 EJ2 members discovered; 15 public C# members accepted, the rest recorded and excluded with evidence.
- `_skill/pattern-map.md#p005-event-payload-coverage-is-property-level` — `FusionNumericTextBoxChangeArgs` is proven property by property (`Value`, `PreviousValue`, `IsInteracted`), not as one class row.
- `_skill/pattern-map.md#p018-writable-payload-fields-need-lifecycle-effect-proof` — `SetValue` is proven by the visible value changing, and `SetMin` by a previously-clamped below-floor value sticking after the minimum is lowered, not by a silent property set.
- `_skill/pattern-map.md#p020-public-methods-need-their-own-row-proof` — `SetValue`, `SetMin`, `Increment`, `Decrement`, `FocusIn`, `FocusOut`, and the `Value` read each have their own row and their own fails-when-broken proof.

## Remote Data Lane

`NumericTextBox` is a numeric single-value input, not a data-capable component:
it binds to one model property and emits one scalar. It has no remote binding,
paging, filtering, lookup, virtualization, or server-query lane to onboard. The
realistic remote workflow for a numeric field is sending the chosen value to the
server, which the `Value()` gather row proves: the plan POSTs `"mealsPerWeek":12`
and the coordinator sees the server confirmation. This satisfies the
remote-behavior expectation for this component class (see
`references/automation-gates.md` Gate 6 applicability — NumericTextBox is not a
board/grid/scheduler/list/tree).

## Commands Run

- raw probe and trace, real headless browser:
  `node .claude/skills/onboard-fusion-component/scripts/run-fusion-probe-trace.mjs --component numeric-text-box --api-set core`
  — wrote `traces/raw-ej2-core.trace.json` (4 trace rows: `created`, `ready` for `ej.inputs.NumericTextBox`, the instance `own keys`, and `prototype methods` showing `blur`, `change`, `changeValue`, `clear`; the d.ts confirms `increment`/`decrement`/`focusIn`/`focusOut`/`getText`/`value`).
- discovery artifacts regenerated from current EJ2 source:
  `node .claude/skills/onboard-fusion-component/scripts/write-fusion-discovery-artifacts.mjs --component numeric-text-box --fusion-type FusionNumericTextBox --class NumericTextBox --namespace inputs --dts ...numerictextbox.d.ts --js ...numerictextbox.js --xml ...Syncfusion.EJ2.xml --write`.
- parity, generated by tool:
  `node .claude/skills/onboard-fusion-component/scripts/compute-fusion-parity.mjs --component numeric-text-box` — `parity 32/32 = 100.0% -> PASS` (25 builder-owned, 4 onboarded-typed, 3 excluded-with-evidence, 0 unaccounted).
- typed API coverage matrix, current against source:
  `node .claude/skills/onboard-fusion-component/scripts/write-fusion-typed-api-coverage.mjs --component numeric-text-box --fusion-type FusionNumericTextBox --write` — 18 typed rows, every row `row-proven`.
- behavior proof:
  `scripts/playwright.sh --filter "FullyQualifiedName~Components.Fusion.NumericTextBox.WhenNumericValueEntered"` — `Test Run Successful. Total tests: 15  Passed: 15`.
- behavioral coverage gate (0b):
  `node .claude/skills/onboard-fusion-component/scripts/verify-behavioral-coverage.mjs --component numeric-text-box` — `[PASS] numeric-text-box — 18/18 members mapped, 13 proving test(s)` against the latest TRX.

## Coverage Matrix Summary

Current generated matrix count: 18 typed C# API rows, 0 supplemental audit rows,
18 total rows. The latest count check shows 18 row-proven matrix rows and 0
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

This audit closes the NumericTextBox component row. The artifacts agree end to
end on the same member shape, payload shape, sync lane, C# API, runtime behavior,
and Playwright proof. The discovery JSON, trace, and coverage matrix remain
generated by the skill scripts and were not hand-edited; `parity-accounting.json`
is the hand-written per-component parity judgment named by
`compute-fusion-parity.mjs`.
