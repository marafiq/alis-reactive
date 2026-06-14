# Fusion framework defects surfaced by the onboarding audit (2026-06-14)

The "audit + upgrade" half of the onboarding goal. Driving each Fusion component to
**100% behavioral coverage** (every typed public member proven by a fails-when-broken
Playwright test, BDD Rule 3) forces a test for every member — and that has surfaced
typed public members that **cannot be honestly tested because the framework/vendor never
makes them observable**. Each is a real defect, not a test gap. Each blocks 0b closeout
for its component and is reported here rather than papered over with a pass-hack.

These need a decision the autonomous run will not make unilaterally — two are
public-contract / runtime changes that belong to the RC3 contract workstream, and the
vendor-driver one is architecturally sensitive. All three were found with zero framework
edits; the components are reverted to HEAD pending the decision, and re-run cleanly once
fixed (the engine reproduces).

---

## 1. switch — `FusionSwitchChangeArgs.IsInteracted` (public-contract)

**Defect.** `FusionSwitchOnChanged.cs:15` declares `public bool IsInteracted { get; set; }`,
but the EJ2 switch `change` event never emits `isInteracted`. So at runtime the member is
always `undefined`/default — a typed public property that structurally cannot carry a real
value.

**Evidence (three ways).**
- Vendor source: `node_modules/@syncfusion/ej2-buttons/src/switch/switch.js:97` builds
  `changeEventArgs = { checked, event }` — no `isInteracted`. A programmatic `checked` set
  goes through `changeState` (switch.js:222) and does not fire `change` at all.
- Generated discovery: `switch/discovery/event-payload-surface.json` — ChangeEventArgs
  members are `checked`, `event`, `name`. No `isInteracted`.
- Live browser: trusted toggle on `/Sandbox/Components/Switch` returned payload keys
  `[checked, event, name]`, `hasIsInteracted=false`. `event-fusion.ts` passes vendor args
  through unchanged.

**Fix.** Remove `IsInteracted` from `FusionSwitchChangeArgs`. This is a public-contract
narrowing — aligns with RC3 contract tightening; makes the unrepresentable-behavior
principle hold (the DSL should not let an author bind `x => x.IsInteracted` for switch).
No live code reads it (only compiled `bin/*.xml` + one archive doc).

**Disposition.** Committed earlier as a documented below-bar hand-off
(`switch/proof/audit-report.md`, `behavioral-coverage.blocked.json`). switch is 7/8 — the
other 7 members are proven. Closes immediately once the property is removed.

---

## 2. chip-list — `FusionSelectedChips.Indexes` (runtime payload-casing bug)

**Defect.** EJ2 `getSelectedChips()` emits the key `Indexes` (capital I). The framework
camelCases every payload read path to `indexes` (lowercase), and the object-shape converter
then drops the unmatched field, so `FusionSelectedChips.Indexes` silently resolves empty —
a typed public member that never carries its value.

**Evidence.**
- Vendor payload: committed trace `chip-list/traces/raw-ej2-core.trace.json` recorded
  `hasCapitalIndexes:true`, `hasLowercaseIndexes:false` (re-confirm at the trace on re-run).
- Read path: `ExpressionPathHelper.cs:61` camelCases the member path to `indexes`.
- Drop: `shape-convert.ts:107-109` (`applyObjectShape`) drops the field whose key doesn't
  match, so the capital-`Indexes` value is discarded. Net: always empty at runtime.
- Pattern recorded as P025 in the chip-list audit (reverted with the component).

**Fix (runtime, not public-contract).** The payload read path must preserve / case-fold to
the vendor's actual key for this member (or the shape converter must match case-insensitively
for vendor payloads). Needs care: the camelCasing is global, so the fix must be scoped to
the vendor-payload boundary and regression-checked across components that rely on it.

**Disposition.** chip-list reverted to compiling HEAD. 31/32 members were proven (clean view,
15 passing BDD tests); only `Indexes` is blocked. Re-run closes it once the casing is fixed.

---

## 3. color-picker — `Disable()` (vendor-driver property-reflection bug)

**Defect.** `Disable()` emits a runtime SET of the EJ2 `disabled` property. The
vendor-neutral runtime assigns it by direct property write (`runtime-object.ts:21-26`), but
the EJ2 ColorPicker only reflects `disabled` into the DOM via `onPropertyChanged` / `dataBind`
(`color-picker.js:1814 -> 736`). A direct assignment bypasses that, so the picker is not
actually disabled — the member is a no-op at runtime.

**Evidence.** Three clean-page browser probes: after `Disable()`, the property reads `true`
but there is zero DOM/style change and the picker stays interactive. `runtime-object.ts:21-26`
(direct assign) vs `color-picker.js:1814->736` (reflection only via onPropertyChanged).

**Fix (vendor-driver / runtime).** Property sets that EJ2 only reflects via `onPropertyChanged`
must be routed through the component's property setter / `dataBind`, not a direct field write.
Architecturally sensitive — touches the vendor-neutral set path; must stay vendor-isolated and
be regression-checked (other components may rely on direct assignment working for their props).

**Disposition.** color-picker reverted to HEAD. 8/9 members proven (clean "Resident Door
Signage" view, 7 passing tests); only `Disable()` blocked. Re-run closes it once fixed.

---

## Also deferred (not defects — onboarding-path gaps)

- **otp-input** — onboarding reached 0b PASS (26/26) + artifacts, but the **blind reviewer
  REJECTED** the view: a debug dashboard (state-echo spans: "code so far", "box 0", "was X
  before box 3") rather than a product screen. The blind review did its job. Reverted; needs
  a view rework with a stricter "no debug/echo elements" instruction, then re-run.
- **input-mask / kanban / schedule** — discovery generator fail-closed (correct): event-payload
  types resolve ambiguously across many d.ts. Need manual disambiguation of the payload type.
- **smart-paste-button / smart-text-area** — Syncfusion ships **no MVC builder** for these AI
  "smart" components (builderMethods=0). Need a non-builder onboarding path or are not yet
  MVC-supported.

## Why this matters

100%-coverage onboarding is doing exactly what an audit should: it cannot be satisfied by a
member the framework never makes observable, so it forces these defects into the open instead
of letting a plausible-but-dead typed property ship. The clean components onboard to the full
bar; the defective ones stop at a named, evidence-backed framework finding.
