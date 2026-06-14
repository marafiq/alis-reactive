# Sidebar Audit Report

Status: audited and closed. `FusionSidebar` is fully onboarded. Every public
typed member has deterministic raw EJ2 evidence, an explicit C# name decision,
an authoritative primitive mapping, a vertical-slice file, and focused typed DSL
Playwright proof bound to a fails-when-broken assertion. The generated typed API
coverage matrix is `audited` with every row `row-proven`.

## Accepted Members

The Sidebar slice onboards the post-render runtime surface; initial render
configuration stays on the Syncfusion `SidebarBuilder`. The Sidebar is a
navigation component, not an input component.

- `FusionSidebar(this IHtmlHelper<TModel> html, ReactivePlan<TModel> plan, string elementId, Action<SidebarBuilder> build)` — the render helper, carrying the controlled component id into the plan.
- `FusionSidebarEvents.Opened` — the `open` transition event selector, with `Reactive(...)` wiring through `ComponentEventOnboarding.Wire`.
- `FusionSidebarEvents.Closed` — the `close` transition event selector.
- `FusionSidebarTransitionArgs.IsInteracted` — user-dismissal versus API-close flag, read from `event.isInteracted` (`true` only when a DOM event drove the transition).
- `Show(this ComponentRef<FusionSidebar, TModel> self)` — calls `show()` to slide the panel into view.
- `Hide(this ComponentRef<FusionSidebar, TModel> self)` — calls `hide()` to tuck the panel away.
- `Toggle(this ComponentRef<FusionSidebar, TModel> self)` — calls `toggle()` to flip the panel state.
- `IsOpen(this ComponentRef<FusionSidebar, TModel> self)` — reads `isOpen` as a typed `bool` source for gather, conditions, and set text.

## Excluded Candidates

- `open`/`close` `.cancel` — writable transition guard; no Senior Living use case cancels the slide, and a writable cancel would need its own lifecycle-effect proof (`_skill/pattern-map.md#p018-writable-payload-fields-need-lifecycle-effect-proof`).
- `open`/`close` `.element` — browser-owned DOM `HTMLElement`; excluded per `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them`.
- `open`/`close` `.event` — browser-owned DOM `MouseEvent | Event`; excluded as a browser-owned object rather than exposed as `object`/`dynamic`.
- `open`/`close` `.model` — broad vendor object (the whole Sidebar instance); the component ref already owns the instance join.
- `open`/`close` `.name` — duplicate event identity metadata; the `Opened`/`Closed` selector already owns the event identity.
- `change`, `created`, `destroyed` events — `change` carries only a browser-owned `element` and duplicate `name` and no `isInteracted`; `created`/`destroyed` are broad vendor `Object` lifecycle payloads with no typed members; the `open`/`close` pair already covers the transition with the proven `isInteracted` payload.
- `height`, `locale`, `defaultBackdropDiv` — no proven post-render read/write use case; `defaultBackdropDiv` is a browser-owned element.
- `type`, `position`, `width`, `isOpen` (initial), `closeOnDocumentClick`, `animate`, `enableDock`, `enableGestures`, `showBackdrop`, `target`, `zIndex` — builder-owned static configuration; configured on `SidebarBuilder` at render, no post-render read/write proven necessary (the `isOpen` read is its own accepted row).
- `destroy()` method — lifecycle cleanup, not plan behavior.

## Skill Pattern Map

This audit confirms the following reusable patterns in `_skill/pattern-map.md`.
No new pattern row was required: the Sidebar slice is a clean application of
existing precedent, with every defect class already covered.

- `_skill/pattern-map.md#p000-discovery-is-exhaustive-but-public-c-dsl-is-selective` — the full EJ2 Sidebar surface is discovered; 8 public C# members accepted, the rest recorded and excluded with evidence.
- `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them` — `element` and `event` excluded as browser-owned DOM objects rather than exposed as `object`/`dynamic`.
- `_skill/pattern-map.md#p005-event-payload-coverage-is-property-level` — `FusionSidebarTransitionArgs` is proven at the property level (`IsInteracted`), not as one class row, and the unaccepted payload keys are each recorded and excluded.
- `_skill/pattern-map.md#p018-writable-payload-fields-need-lifecycle-effect-proof` — the writable `cancel` guard is excluded rather than exposed without a lifecycle-effect proof.
- `_skill/pattern-map.md#p020-public-methods-need-their-own-row-proof` — `Show`, `Hide`, `Toggle`, and the `IsOpen` read each have their own row and their own fails-when-broken proof.

## Remote Data Lane

`Sidebar` is a navigation container, not a data-capable component: it has no
remote binding, paging, filtering, lookup, virtualization, or server-query lane
to onboard. The realistic remote workflow for a sidebar is reporting its open
state to the server, which the `IsOpen()` gather row proves: closing the panel
POSTs `"isOpen":false` and the coordinator sees the server confirmation
("Care-services menu closed — services hidden."). This satisfies the
remote-behavior expectation for this component class (Sidebar is not a
board/grid/scheduler/list/tree).

## Commands Run

- raw probe and trace, real headless browser:
  `node .claude/skills/onboard-fusion-component/scripts/run-fusion-probe-trace.mjs --component sidebar --api-set core`
  — wrote `traces/raw-ej2-core.trace.json` (20 trace rows: real `open`/`close` transitions with the full `EventArgs` payload, the `change` companion, `show()`/`hide()`/`toggle()` calls each followed by the `isOpen` read flipping state, the `created` lifecycle event, the instance `own keys`, and `prototype methods`).
- typed API coverage matrix, current against source:
  `node .claude/skills/onboard-fusion-component/scripts/write-fusion-typed-api-coverage.mjs --component sidebar --fusion-type FusionSidebar --check` — reported current.
- onboarding status inventory:
  `node .claude/skills/onboard-fusion-component/scripts/report-fusion-onboarding-status.mjs --write`.
- artifact gate verifier:
  `node .claude/skills/onboard-fusion-component/scripts/verify-fusion-artifact-gates.mjs --component sidebar`.
- behavior proof:
  `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Sidebar.WhenUsingFusionSidebar"` — the per-member outcome is recorded green in `proof/behavioral-coverage.json` and verified by the onboarding behavioral-coverage gate.

## Counts

| Item | Count |
|---|---:|
| Typed C# API rows | 10 |
| Supplemental audit rows | 0 |
| Total typed coverage matrix rows | 10 |

## Coverage Matrix Summary

Current generated matrix count: 10 typed C# API rows, 0 supplemental audit rows, 10 total rows. The latest count check shows 10 row-proven matrix rows and 0 matrix rows without `row-proven` status.

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

This audit closes the Sidebar component row. The artifacts agree end to end on
the same member shape, payload shape, sync lane, C# API, runtime behavior, and
Playwright proof. The discovery JSON, trace, and coverage matrix remain
generated by the skill scripts and were not hand-edited.
