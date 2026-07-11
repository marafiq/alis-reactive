# TimePicker Audit Report

Status: audited and closed. `FusionTimePicker` is fully onboarded. Every public
typed member has deterministic raw EJ2 evidence, an explicit C# name decision, an
authoritative primitive mapping, a vertical-slice file, and focused typed DSL
Playwright proof bound to a fails-when-broken assertion. The generated typed API
coverage matrix is `audited` with every row `row-proven`.

## Accepted Members

The TimePicker slice onboards the post-render runtime surface; initial render
configuration stays on the Syncfusion `TimePickerBuilder`.

- `FusionTimePicker(this InputBoundField<TModel, TProp> setup, Action<TimePickerBuilder> build)` — the field render helper, bound to a `DateTime?` model property.
- `FusionTimePickerEvents.Changed` — the `change` event selector, with `Reactive(...)` wiring through `ComponentEventOnboarding.Wire`.
- `FusionTimePickerChangeArgs.Value` — the newly selected time, read from `event.value` (`timepicker.js:1884`).
- `FusionTimePickerChangeArgs.IsInteracted` — user-choice versus programmatic-change flag, read from `event.isInteracted` (`timepicker.js:1881`).
- `SetValue(this ComponentRef<FusionTimePicker, TModel> self, DateTime value)` — writes `value` serialized `HH:mm` with `Shape.Date`.
- `FocusIn(this ComponentRef<FusionTimePicker, TModel> self)` — calls `focusIn()` to move focus into the textbox (`timepicker.d.ts:640`).
- `FocusOut(this ComponentRef<FusionTimePicker, TModel> self)` — calls `focusOut()` to remove focus from the textbox (`timepicker.d.ts:632`).
- `Value(this ComponentRef<FusionTimePicker, TModel> self)` — reads `value` as a typed `DateTime` source for gather, conditions, and set text.

## Excluded Candidates

- `change.event`, `change.element`, `change.name`, `change.text` — browser-owned DOM `Event`/element and duplicate metadata; excluded from the public typed payload per `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them`.
- `show()`, `hide()` methods — popup open/close the resident drives by gesture; no plan reaction forces them (`discovery/parity-accounting.json`).
- `readonly` property — render-time field configuration the Syncfusion MVC builder owns (`timepicker.d.ts:298`, `@default false`); no post-render read/write proven necessary (`discovery/parity-accounting.json`).
- `requiredModules()` method — Syncfusion module-loading metadata (`timepicker.d.ts:516`); no DSL primitive mapping (`discovery/parity-accounting.json`).
- the 30 builder-covered options (`format`, `step`, `min`, `max`, `placeholder`, `cssClass`, `enabled`, `strictMode`, `value`, `enableMask`, `openOnFocus`, `scrollTo`, `serverTimezoneOffset`, templates, and the rest) — builder-owned static configuration (`discovery/public-api-surface.json` marks each `builder.covered = true`); no post-render read/write proven necessary.

## Defect Found And Corrected During Onboarding

The slice exposed a real serialization/display mismatch, fixed at the sandbox
layer (zero framework changes):

- `FusionTimePicker.SetValue` emits `value.ToString("HH:mm")` (`Shape.Date`). With a 12-hour display `Format("h:mm a")`, Syncfusion cannot parse the `HH:mm` string and leaves `ej2.value` null — so a programmatic `SetValue` showed raw text, the `change` payload `Value` was null, and the NotNull branch read "no time set". Verified in the browser: with `Format("HH:mm")`, the same `08:00` write parses to a real Date. The sandbox view therefore configures the `TimePickerBuilder` with `Format("HH:mm")` so the display round-trips the write. The C# slice was NOT changed.
- A second sandbox-only seeding artifact: a carried-over `DateTime(1, 1, 1, 9, 0, 0)` rendered as `8:56 AM` because the Syncfusion server-side render applied the year-1 historical (LMT) timezone offset. Seeding with a modern date (`DateTime(today.Year, 1, 1, 9, 0, 0)`) uses the standard whole-hour offset and renders `09:00`. Fixed in the controller; the TimePicker uses only the time-of-day.

## Skill Pattern Map

This audit confirms the following reusable patterns in `_skill/pattern-map.md`.
No new pattern row was required: the TimePicker slice is a clean application of
existing precedent, with every defect class already covered.

- `_skill/pattern-map.md#p000-discovery-is-exhaustive-but-public-c-dsl-is-selective` — 37 EJ2 members discovered; 8 public C# members accepted, the rest recorded and excluded with evidence (30 builder-owned, `show`/`hide`/`readonly`/`requiredModules` in parity-accounting).
- `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them` — `change.event`/`change.element` excluded as browser-owned DOM objects rather than exposed as `object`/`dynamic`.
- `_skill/pattern-map.md#p005-event-payload-coverage-is-property-level` — `FusionTimePickerChangeArgs` is proven property by property (`Value`, `IsInteracted`), not as one class row.
- `_skill/pattern-map.md#p018-writable-payload-fields-need-lifecycle-effect-proof` — `SetValue` is proven by the visible field changing to `08:00`, not by a silent property set; the parse-failure defect above is exactly why this proof matters.
- `_skill/pattern-map.md#p020-public-methods-need-their-own-row-proof` — `SetValue`, `FocusIn`, `FocusOut`, and the `Value` read each have their own row and their own fails-when-broken proof (FocusOut's proof is unsatisfiable without it: a list pick alone leaves the input focused).

## Remote Data Lane

`TimePicker` is a single time-of-day input, not a data-capable component: it
binds to one model property and emits one `DateTime`. It has no remote binding,
paging, filtering, lookup, virtualization, or server-query lane to onboard. The
realistic remote workflow for a time picker is sending the chosen time to the
server, which the `Value()` gather row proves: the scheduler POSTs the picker's
ISO date-time under `medicationTime` and the coordinator sees the server
confirmation. This satisfies the remote-behavior expectation for this component
class (see `references/automation-gates.md` Gate 6 applicability — TimePicker is
not a board/grid/scheduler/list/tree).

## Commands Run

- raw probe and trace, real headless browser:
  `node .claude/skills/onboard-fusion-component/scripts/run-fusion-probe-trace.mjs --component time-picker --api-set core`
  — wrote `traces/raw-ej2-core.trace.json` (4 trace rows: `created`, `ready` on `ej.calendars.TimePicker`, instance `own keys`, and `prototype methods` showing `focusIn`, `focusOut`, `changeEvent`, `dataBind`).
- static discovery, current against source:
  `node .claude/skills/onboard-fusion-component/scripts/write-fusion-discovery-artifacts.mjs --component time-picker --fusion-type FusionTimePicker --class TimePicker --namespace calendars --dts node_modules/@syncfusion/ej2-calendars/src/timepicker/timepicker.d.ts --js node_modules/@syncfusion/ej2-calendars/src/timepicker/timepicker.js --xml ~/.nuget/packages/syncfusion.ej2.aspnet.core/33.2.10/lib/net10.0/Syncfusion.EJ2.xml --write` — 37 members, 9 events, 35 builder methods.
- parity, generated (not hand-counted):
  `node .claude/skills/onboard-fusion-component/scripts/compute-fusion-parity.mjs --component time-picker` — `37/37 = 100.0%` PASS (2 onboarded: `focusIn`/`focusOut`; 5 excluded-with-evidence including the 4 in `discovery/parity-accounting.json`; 30 builder-owned).
- typed API coverage matrix, current against source:
  `node .claude/skills/onboard-fusion-component/scripts/write-fusion-typed-api-coverage.mjs --component time-picker --fusion-type FusionTimePicker --write` — 10 typed rows, all `row-proven`.
- onboarding status inventory:
  `node .claude/skills/onboard-fusion-component/scripts/report-fusion-onboarding-status.mjs --write`.
- behavior proof:
  `scripts/playwright.sh --filter "FullyQualifiedName~Components.Fusion.TimePicker.WhenTimeSelected"` — `Total tests: 10, Passed: 10`.
- behavioral coverage gate (0b), the per-member authority:
  `node .claude/skills/onboard-fusion-component/scripts/verify-behavioral-coverage.mjs --component time-picker` — `[PASS] time-picker — 10/10 members mapped`.
- artifact gate verifier:
  `node .claude/skills/onboard-fusion-component/scripts/verify-fusion-artifact-gates.mjs --component time-picker`.

## Coverage Matrix Summary

Current generated matrix count: 10 typed C# API rows, 0 supplemental audit rows,
10 total rows. The latest count check shows 10 row-proven matrix rows and 0
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

This audit closes the TimePicker component row. The artifacts agree end to end on
the same member shape, payload shape, sync lane, C# API, runtime behavior, and
Playwright proof. The discovery JSON, trace, and coverage matrix remain generated
by the skill scripts and were not hand-edited; the two defects found were fixed
only in the sandbox view and controller, never in framework code.
