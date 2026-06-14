# TextArea Audit Report

Status: audited and closed. `FusionTextArea` is fully onboarded. Every public
typed member has deterministic raw EJ2 evidence, an explicit C# name decision,
an authoritative primitive mapping, a vertical-slice file, and focused typed DSL
Playwright proof bound to a fails-when-broken assertion. The generated typed API
coverage matrix is `audited` with every row `row-proven`.

## Accepted Members

The TextArea slice onboards the post-render runtime surface; initial render
configuration stays on the Syncfusion `TextAreaBuilder`.

- `FusionTextArea(this InputBoundField<TModel, TProp> setup, Action<TextAreaBuilder> build)` — the field render helper, bound to a string model property.
- `FusionTextAreaEvents.Input` — the `input` event selector, fired while editing, with `Reactive(...)` wiring through `ComponentEventOnboarding.Wire`.
- `FusionTextAreaEvents.Changed` — the `change` event selector, fired after focus leaves.
- `FusionTextAreaEvents.Focus` — the `focus` event selector, fired when focus arrives.
- `FusionTextAreaEvents.Blur` — the `blur` event selector, fired when focus leaves.
- `FusionTextAreaInputArgs.Value` / `.PreviousValue` — the freshly typed text and the text before the keystroke, read from `event.value` and `event.previousValue`.
- `FusionTextAreaChangeArgs.Value` / `.PreviousValue` / `.IsInteracted` — the committed text, the prior committed text, and the hand-edit-versus-programmatic flag.
- `FusionTextAreaFocusArgs.Value` — the text on file when focus arrives, read from `event.value`.
- `FusionTextAreaBlurArgs.Value` — the text held in the field when focus leaves, read from `event.value`.
- `SetValue(this ComponentRef<FusionTextArea, TModel> self, string? value)` — writes `value` and chains `dataBind()` to repaint the textarea.
- `FocusIn(this ComponentRef<FusionTextArea, TModel> self)` — calls `focusIn()` to move focus into the textarea.
- `FocusOut(this ComponentRef<FusionTextArea, TModel> self)` — calls `focusOut()` to remove focus from the textarea.
- `Value(this ComponentRef<FusionTextArea, TModel> self)` — reads `value` as a typed `string` source for gather, conditions, and set text.

## Excluded Candidates

- `*.event` (`Event`) on every event payload — browser-owned DOM `Event`; excluded from the public typed payload per `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them`.
- `*.container` (`HTMLElement`) on every event payload — browser-owned DOM element; excluded for the same reason.
- `change.isInteraction` — deprecated misspelled alias superseded by `isInteracted`; excluded to avoid duplicating the same flag.
- `created`, `destroyed` events — DOM-native lifecycle events typed `Object` with no narrowed payload; no focused Senior Living use case.
- `addAttributes`, `removeAttributes`, `readonly` — raw attribute and read-only manipulation; no focused Senior Living use case and the DSL does not express attribute bags.
- `adornmentFlow`, `adornmentOrientation`, `appendTemplate`, `cols`, `cssClass`, `enabled`, `enablePersistence`, `floatLabelType`, `maxLength`, `placeholder`, `prependTemplate`, `resizeMode`, `rows`, `showClearButton`, `width` — builder-owned static configuration (`discovery/public-api-surface.json` marks each `builder.covered = true`); no post-render read/write proven necessary.
- `destroy()` method — lifecycle cleanup, not plan behavior (`discovery/public-api-surface.json` classifies it `skip`).

## Skill Pattern Map

This audit confirms the following reusable patterns in `_skill/pattern-map.md`.
No new pattern row was required: the TextArea slice is a clean application of
existing precedent, with every defect class already covered.

- `_skill/pattern-map.md#p000-discovery-is-exhaustive-but-public-c-dsl-is-selective` — 28 EJ2 members discovered; 13 public C# members accepted, the rest recorded and excluded with evidence.
- `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them` — every `*.event` and `*.container` payload field excluded as a browser-owned DOM object rather than exposed as `object`/`dynamic`.
- `_skill/pattern-map.md#p005-event-payload-coverage-is-property-level` — each event payload is proven property by property (`Value`, `PreviousValue`, `IsInteracted`), not as one class row.
- `_skill/pattern-map.md#p018-writable-payload-fields-need-lifecycle-effect-proof` — `SetValue` is proven by the visible textarea text changing, not by a silent property set.
- `_skill/pattern-map.md#p020-public-methods-need-their-own-row-proof` — `SetValue`, `FocusIn`, `FocusOut`, and the `Value` read each have their own row and their own fails-when-broken proof.

## Remote Data Lane

`TextArea` is a single-value string input, not a data-capable component: it binds
to one model property and emits one scalar. It has no remote binding, paging,
filtering, lookup, virtualization, or server-query lane to onboard. The
realistic remote workflow for a care note is sending the written text to the
server, which the `Value()` gather row proves: the care log POSTs
`"careNote":"Hydration encouraged at lunch."` and the caregiver sees the server
confirmation. This satisfies the remote-behavior expectation for this component
class (see `references/automation-gates.md` Gate 6 applicability — TextArea is
not a board/grid/scheduler/list/tree).

## Commands Run

- raw probe and trace, real headless browser:
  `node .claude/skills/onboard-fusion-component/scripts/run-fusion-probe-trace.mjs --component text-area --api-set core`
  — wrote `traces/raw-ej2-core.trace.json` (4 trace rows: `created` with the DOM-native undefined payload, `ready` confirming the `ej.inputs.TextArea` instance, the instance `own keys`, and `prototype methods` showing `blur`, `change`, and the `dataBind` repaint method).
- typed API coverage matrix, current against source:
  `node .claude/skills/onboard-fusion-component/scripts/write-fusion-typed-api-coverage.mjs --component text-area --fusion-type FusionTextArea --check` — reported current.
- onboarding status inventory:
  `node .claude/skills/onboard-fusion-component/scripts/report-fusion-onboarding-status.mjs --write`.
- artifact gate verifier:
  `node .claude/skills/onboard-fusion-component/scripts/verify-fusion-artifact-gates.mjs --component text-area`.
- behavior proof:
  `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.TextArea.WhenUsingFusionTextArea"` — the per-member outcome is recorded green in `proof/behavioral-coverage.json` and verified by the onboarding behavioral-coverage gate.

## Coverage Matrix Summary

Current generated matrix count: 21 typed C# API rows, 0 supplemental audit
rows, 21 total rows. The latest count check shows 21 row-proven matrix rows and
0 matrix rows without `row-proven` status.

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
- Skill pattern map: `_skill/pattern-map.md`

## Commit Boundary

This audit closes the TextArea component row. The artifacts agree end to end on
the same member shape, payload shape, sync lane, C# API, runtime behavior, and
Playwright proof. The discovery JSON, trace, and coverage matrix remain
generated by the skill scripts and were not hand-edited.
