# Slider Audit Report

Status: audited and closed. `FusionSlider` is fully onboarded. Every public typed
member has deterministic raw EJ2 evidence, an explicit C# name decision, an
authoritative primitive mapping, a vertical-slice file, and focused typed DSL
Playwright proof bound to a fails-when-broken assertion. The generated typed API
coverage matrix is `audited` with every row `row-proven`.

## Accepted Members

The Slider slice onboards the post-render runtime surface; initial render
configuration stays on the Syncfusion `SliderBuilder`.

- `FusionSlider(this InputBoundField<TModel, TProp> setup, Action<SliderBuilder> build)` — the field render helper, bound to a numeric model property (scalar) or a number-array property (range).
- `FusionSliderEvents.Change` — the `change` event selector (fires as the handle moves), with `Reactive(...)` wiring through `ComponentEventOnboarding.Wire`.
- `FusionSliderEvents.Changed` — the `changed` event selector (fires when the handle settles and from `setValue()`).
- `FusionSliderChangeArgs.Value` — the slider value after the change, read from `event.value`.
- `FusionSliderChangeArgs.PreviousValue` — the value before the change, read from `event.previousValue` (`previousVal` for `change`, `previousChanged` for `changed`).
- `FusionSliderChangeArgs.Text` — the formatted value string, read from `event.text`.
- `FusionSliderChangeArgs.Action` — the Syncfusion change-action name, read from `event.action`.
- `FusionSliderChangeArgs.IsInteracted` — user-choice versus programmatic-change flag, read from `event.isInteracted`.
- `SetValue(this ComponentRef<FusionSlider, TModel> self, double value)` — writes `value` and chains `dataBind()` to repaint the handle.
- `SetRangeValue(this ComponentRef<FusionSlider, TModel> self, double start, double end)` — writes the number array onto `value` (plan member `rangeValue`) and chains `dataBind()` to repaint both handles.
- `Value(this ComponentRef<FusionSlider, TModel> self)` — reads `value` as a typed `double` source for gather, conditions, and set text.
- `RangeValue(this ComponentRef<FusionSlider, TModel> self)` — reads `value` as a typed `double[]` source (plan member `rangeValue`) for gather and set text.

## Excluded Candidates

- `created`, `renderedTicks`, `renderingTicks`, `tooltipChange` events — builder-owned render/tick/tooltip hooks; `discovery/public-api-surface.json` marks each `builder.covered = true`; no focused Senior Living runtime use case.
- `colorRange`, `cssClass`, `customValues`, `enableAnimation`, `enabled`, `enableHtmlSanitizer`, `limits`, `max`, `min`, `orientation`, `showButtons`, `step`, `ticks`, `tooltip`, `type`, `width` — builder-owned static configuration (`discovery/public-api-surface.json` marks each `builder.covered = true`); no post-render read/write proven necessary.
- `initialTooltip`, `readonly` properties — excluded with source-grounded reasons in `discovery/parity-accounting.json` (`slider.d.ts:356` undocumented tooltip-init flag; `slider.d.ts:413` read-only render mode owned by the builder).
- `reposition()`, `setTooltip()` methods — excluded with source-grounded reasons in `discovery/parity-accounting.json` (`slider.d.ts:653` layout-recovery helper; `slider.d.ts:713` imperative tooltip-text setter); no visible domain outcome a plan asserts.
- `destroy()` method — lifecycle cleanup, not plan behavior (`discovery/public-api-surface.json` classifies it `skip`).

## Skill Pattern Map

This audit confirms the following reusable patterns in `_skill/pattern-map.md`.
No new pattern row was required: the Slider slice is a clean application of
existing precedent, with every defect class already covered.

- `_skill/pattern-map.md#p000-discovery-is-exhaustive-but-public-c-dsl-is-selective` — 28 EJ2 members discovered; 12 public C# members accepted, the rest recorded and excluded with evidence (builder-owned, parity-accounting, or skip).
- `_skill/pattern-map.md#p016-shared-payload-types-do-not-prove-shared-event-rows` — `change` and `changed` are two distinct Syncfusion events that share the `SliderChangeEventArgs` shape; each keeps its own selector row (`Change`, `Changed`) and its own fails-when-broken proof rather than collapsing into one event row.
- `_skill/pattern-map.md#p005-event-payload-coverage-is-property-level` — `FusionSliderChangeArgs` is proven property by property (`Value`, `PreviousValue`, `Text`, `Action`, `IsInteracted`), not as one class row.
- `_skill/pattern-map.md#p018-writable-payload-fields-need-lifecycle-effect-proof` — `SetValue` is proven by the visible handle moving to the written value, and `SetRangeValue` by both range handles moving, not by a silent property set.
- `_skill/pattern-map.md#p020-public-methods-need-their-own-row-proof` — `SetValue`, `SetRangeValue`, and the `Value`/`RangeValue` reads each have their own row and their own fails-when-broken proof.
- `_skill/pattern-map.md#p012-exclusion-rows-require-explicit-exclusion-proof` — every excluded member (builder-owned, `initialTooltip`/`readonly`, `reposition`/`setTooltip`, `destroy`) carries explicit source-backed exclusion proof, not a bare omission.

## What Went Wrong And Was Corrected

The slider discovery artifacts were originally generated with a doubled global
namespace (`ej.ej.inputs` in `discovery/public-api-surface.json` and the probe),
so the raw EJ2 trace runner threw `Cannot read properties of undefined (reading
'inputs')` and could not instantiate the component. Corrected at the source by
regenerating discovery with the right namespace argument
(`write-fusion-discovery-artifacts.mjs --namespace inputs`), which produced
`ej.inputs.Slider` in both the surface JSON and the probe; the trace runner then
wrote `traces/raw-ej2-core.trace.json`. Parity re-ran at 100% and the member set
was unchanged (28 vendor members). No C# slice, sandbox, or test was patched to
hide this — the fix was at the generated discovery layer.

## Remote Data Lane

`Slider` is a numeric single-value (or two-value range) input, not a data-capable
component: it binds to one model property and emits one numeric value or a
two-number array. It has no remote binding, paging, filtering, lookup,
virtualization, or server-query lane to onboard. The realistic remote workflow
for a preference slider is saving the chosen values to the server, which the
`Value()` and `RangeValue()` gather rows prove: the page POSTs
`"roomTemperature":68` and `"quietHours":[13,15]` and the resident sees the
server confirmation. This satisfies the remote-behavior expectation for this
component class (see `references/automation-gates.md` Gate 6 applicability —
Slider is not a board/grid/scheduler/list/tree).

## Commands Run

- raw probe and trace, real headless browser:
  `node .claude/skills/onboard-fusion-component/scripts/run-fusion-probe-trace.mjs --component slider --api-set core`
  — wrote `traces/raw-ej2-core.trace.json` (4 trace rows: created, ready, instance own-keys, prototype methods showing `change`, `changed`, `changeEvent`, `buttonClick`, `dataBind`).
- parity, generated by tool:
  `node .claude/skills/onboard-fusion-component/scripts/compute-fusion-parity.mjs --component slider` — `28/28 = 100.0% PASS`.
- typed API coverage matrix, current against source:
  `node .claude/skills/onboard-fusion-component/scripts/write-fusion-typed-api-coverage.mjs --component slider --fusion-type FusionSlider --write` — 14 rows, all `row-proven`.
- onboarding status inventory:
  `node .claude/skills/onboard-fusion-component/scripts/report-fusion-onboarding-status.mjs --write`.
- artifact gate verifier:
  `node .claude/skills/onboard-fusion-component/scripts/verify-fusion-artifact-gates.mjs --component slider`.
- behavior proof:
  `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Slider.WhenUsingFusionSlider"` — 9 tests, all passed; the per-member outcome is recorded green in `proof/behavioral-coverage.json` and verified by the onboarding behavioral-coverage gate (`verify-behavioral-coverage.mjs --component slider` → `[PASS] slider — 14/14 members mapped`).

## Coverage Matrix Summary

Current generated matrix count: 14 typed C# API rows, 0 supplemental audit rows,
14 total rows. The latest count check shows 14 row-proven matrix rows and 0
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

This audit closes the Slider component row. The artifacts agree end to end on the
same member shape, payload shape, sync lane, C# API, runtime behavior, and
Playwright proof. The discovery JSON, trace, and coverage matrix remain generated
by the skill scripts and were not hand-edited.
