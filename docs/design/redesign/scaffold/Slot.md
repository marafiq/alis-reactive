# Slot — Implementation Spec (scaffold)

> Mechanical build spec for the **Slot** micro-module. A developer opens this
> file, reads the surface + skeleton + fixtures, and types the obvious body.
> Every claim is grounded in actual source, cited inline. Names are from
> [`03-naming.md`](../03-naming.md); responsibility/ownership from
> [`02-micro-modules.md`](../02-micro-modules.md); acceptance fixtures from
> [`04-matrix-validation-components-slots.md`](../04-matrix-validation-components-slots.md)
> Band C.

---

## 1. Responsibility, Ownership, Dependencies

**Responsibility (one sentence).** Compose plans: join same-model plan scripts by
`PlanId` on the server, and load/unload partials by `SlotId` in the browser —
recomposing the active `PlanDocument` from a boot snapshot plus currently-loaded
slots, aborting only slot-owned behavior on unload.

**What Slot owns.**

- `→` (C# author/plan side): nothing of its own. Slot's *authoring axis* is the
  `PlanScope` discriminator (`root` vs `partial`) already surfaced by the **Plan**
  module's `ReactivePlan`/`ResolvePlan` verbs, and the *load/unload trigger* is the
  `inject` reaction already surfaced by the **Reaction** module's `p.Into(elementId)`
  verb. Slot adds **no new C# DSL verb and no new C# plan node** — confirmed by
  Band C: *"There is no `InjectInto` and no `p.Slot(...)` verb in source."*
- `⇒` (TS runtime side — Slot's real surface):
  - `injectPartial` — `injectHtml(container, html, slot)`: extract embedded
    `[data-reactive-plan]` scripts, set the container HTML, and route to load
    (plans present) or unload (no plans). (renamed file role of `execution/inject.ts`)
  - `AppliedPlans` — the composition state object: boot snapshots + loaded slots +
    per-slot `AbortController`. Exposes `register` / `loadPartialSlot` /
    `unloadPartialSlot` / `get`. (renamed from `AppliedBrowserPlans` in
    `lifecycle/browser-plans.ts` — *"drops the redundant 'Browser'"*, per
    [`03-naming.md`](../03-naming.md) Slot table)
  - `recompose` — builds a **new** `PlanDocument` from the boot snapshot plus the
    slots still loaded for a `planId`. Replaces today's in-place `resetPlanDocument`
    mutation. (per [`03-naming.md`](../03-naming.md): *"it composes, it does not
    'reset'; and it no longer mutates in place"*)
  - `MergePolicy` — the **one** replace-vs-append rule, shared by the C# container
    merge and the TS recompose: type contracts merge, components replace-by-key
    (with the layout-object / object-target / validation-container join rules),
    behaviors append. Today this lives split across `composeBootPlanInto` /
    `composeSlotPlanInto` + `component-merge.ts` + `object-contracts.ts`; Slot makes
    it one named policy used by both `composeInitialPlans` (boot SSR-join) and
    `recompose`.
  - `composeInitialPlans` — boot-time SSR join: group incoming plans by `planId`,
    apply the boot arm of `MergePolicy`. (kept from `browser-plans.ts`)

**What Slot depends on** (from the module-dependency graph; acyclic):

- **Plan** — Slot recomposes a `PlanDocument`, so it depends on the document type
  Plan owns. **Plan does NOT depend back on Slot.** The boot path reaches slot
  injection through the **Reaction** `inject` handler (`executeInject` →
  `injectHtml`), never by Plan importing Slot. This is the layered replacement for
  today's boot↔browser-plans cycle.
- **Component** — the merge rules key on `BrowserObject` identity
  (`id`/`vendor`/`type`), `ComponentRole` (`layout-object` / `object-target` /
  `validation-container`), `InputBinding`, and `BrowserObjectContract` members.
  Slot reads these as merge keys; it does not own them.

**What Slot does NOT own / must not invent.**

- No new C# DSL verb, no `p.Slot(...)`, no `InjectInto` — Band C is explicit.
- No new C# plan node — the `inject` node is `InjectReaction` and belongs to
  **Reaction** (`PlanModel/ReactionGraph.cs:424`).
- No `activeRuntimePlan` singleton resurrection — the active plan is passed
  explicitly (Plan module). Slot's `AppliedPlans` is the composition store, not the
  "currently executing" plan global.

---

## 2. Public Surface

### 2.1 C# author/plan side — *nothing new*

Slot adds no C# type. Its two authoring touch-points are owned elsewhere and only
*named here so the dev does not re-create them*:

| Authoring axis | Owner module | Source today |
|---|---|---|
| `PlanScope` (`root` / `partial`) chosen by `Html.ReactivePlan<T>()` vs `Html.ResolvePlan<T>()` | **Plan** | `Razor/Extensions/PlanExtensions.cs:51,90`; `ReactivePlan.cs:165–204` (`ReactivePlanScope` → `PlanIdentity.Root`/`Partial`); serialized scope node `PlanModel/PlanTerms.cs:85–115` (`PlanScope`/`RootPlanScope`/`PartialPlanScope`, `kind:"root"|"partial"`) |
| `p.Into(elementId)` → `inject` reaction (the load/unload trigger) | **Reaction** | `Builders/PipelineBuilder.cs:269–279` (`Into`); `PlanModel/ReactionGraph.cs:44,423–440` (`InjectReaction`, `kind:"inject"`, `Slot`, `Value`) |

> A dev implementing Slot writes **no C#**. If you find yourself adding a C# type
> to Slot, stop — it belongs to Plan or Reaction.

### 2.2 TS runtime side — the surface Slot exposes

All signatures below mirror the existing source; the spec only **renames** to the
`03-naming.md` vocabulary and **fixes** the two debts (in-place mutation → fresh
document; split merge → one `MergePolicy`).

**`lifecycle/applied-plans.ts`** (renamed from `lifecycle/browser-plans.ts`)

```ts
/** Wiring callbacks Slot invokes after a (re)compose — supplied by boot. */
export interface BrowserPlanWiring {
  wireBehaviors: (behaviors: Behavior[], plan: PlanDocument, signal?: AbortSignal) => void;
  wireContainerValidation: (plan: PlanDocument, signal?: AbortSignal) => void;
}

/**
 * The composition state for all applied plans: boot snapshots, loaded partial
 * slots, and each slot's AbortController. Active plans are recomposed from the
 * boot snapshot plus currently-loaded slots — the boot snapshot is never mutated.
 */
export class AppliedPlans {
  /** Records a booted plan and snapshots it as the immutable boot baseline. */
  register(plan: PlanDocument): void;

  /**
   * Loads a partial slot: replaces any prior load on this slot, snapshots the
   * incoming plans, recomposes every affected planId into a fresh PlanDocument,
   * then wires the slot's behaviors and container validation under a new
   * AbortController. Returns the affected planIds.
   */
  loadPartialSlot(slotId: string, plans: PlanDocument[], wiring: BrowserPlanWiring): string[];

  /**
   * Unloads a partial slot: aborts the slot's AbortController (drops slot-owned
   * listeners/validation), drops its plans, and recomposes the affected planIds
   * from the boot snapshot plus the remaining slots. Returns the affected planIds.
   */
  unloadPartialSlot(slotId: string): string[];

  /** The current active PlanDocument for a planId, or undefined if none. */
  get(planId: string): PlanDocument | undefined;

  /** Test-only: aborts all slots and clears all composition state. */
  reset(): void;
}

/** The single applied-plans store for the page. */
export const appliedPlans: AppliedPlans;

/**
 * Boot-time SSR join: groups incoming plans by planId and applies the boot arm of
 * MergePolicy, yielding one composed PlanDocument per distinct planId.
 */
export function composeInitialPlans(plans: PlanDocument[]): PlanDocument[];
```

**`lifecycle/merge-policy.ts`** (new home; folds `composeBootPlanInto` /
`composeSlotPlanInto` and re-exports the component/contract merge primitives)

```ts
/**
 * The one replace-vs-append rule shared by the C# container merge and TS
 * recompose. Type contracts merge structurally; components merge by key with the
 * boot-vs-slot join rules; behaviors append in order.
 */
export const MergePolicy: {
  /** Boot arm: SSR join of same-model plans (composeInitialPlans + recompose base). */
  composeBootPlanInto(target: PlanDocument, incoming: PlanDocument): void;
  /** Slot arm: layer a loaded partial onto the boot baseline. */
  composeSlotPlanInto(target: PlanDocument, incoming: PlanDocument, bootPlan: PlanDocument | undefined): void;
};

/** Builds a NEW PlanDocument with empty contents for a planId (never reused/mutated in place). */
export function emptyPlan(planId: string): PlanDocument;

/** A deep-enough copy that recompose/load can layer onto without mutating the source. */
export function snapshotPlan(plan: PlanDocument): PlanDocument;
```

**`execution/inject.ts`** (`injectPartial` role — kept, name unchanged at call site)

```ts
/**
 * Inject HTML into a container, using ej.base.append when available (SF init).
 * Extracts embedded <script data-reactive-plan> elements first; when present,
 * loads them into the slot, otherwise unloads the slot.
 */
export function injectPartial(container: HTMLElement, html: string, slot: string): void;
```

> `injectPartial` is the renamed export of today's `injectHtml`
> (`execution/inject.ts:22`). The Reaction `executeInject` handler calls it; that
> call site (Reaction module) updates to the new name.

**`lifecycle/boot.ts`** — Slot's two host-callable entry points stay here (boot
owns wiring; it delegates composition to `AppliedPlans`):

```ts
/** Host/runtime entry: load a partial slot's plans and recompose. */
export function loadPartialSlot(slotId: string, incoming: PlanDocument[]): void;
/** Host/runtime entry: unload a partial slot and recompose. */
export function unloadPartialSlot(slotId: string): void;
```

### 2.3 TS contract counterpart (generated, do NOT hand-write)

Slot does not introduce a new wire node. The only contract types it reads are the
`PlanScope` union and `PlanDocument`, both already generated into
`runtime/types/plan.ts` by the **Kind** kernel (`PlanContractGenerator`):

```ts
export interface PlanDocument {
  version: 3;
  planId: string;
  scope: PlanScope;                                 // root | partial
  types: Record<string, BrowserObjectContract>;
  components: Record<string, ComponentObject>;
  behaviors: Behavior[];
}
export type PlanScope = RootPlanScope | PartialPlanScope;
export interface RootPlanScope { kind: "root"; }
export interface PartialPlanScope { kind: "partial"; }
```

(`runtime/types/plan.ts:5–24`, auto-generated header line 1–3.) Slot reads these;
it never edits `plan.ts`.

---

## 3. Input → Output Contract

| Path | Input | Output | Invariants |
|---|---|---|---|
| **SSR join** (`composeInitialPlans`) | `PlanDocument[]` discovered at boot (multiple scripts, some sharing a `planId`) | one composed `PlanDocument` per distinct `planId` | grouping is by `planId` exactly; within a group, `MergePolicy.composeBootPlanInto` is applied in document order; behaviors append in encounter order |
| **Slot load** (`loadPartialSlot`) | `slotId: string`, `plans: PlanDocument[]`, `wiring` | mutated `appliedPlans` store: a fresh active `PlanDocument` per affected `planId`; affected `planId[]` returned | the **boot snapshot is never mutated**; loading a slot first **replaces** any prior load on the same `slotId`; recompose runs *before* wiring so wiring sees the composed plan; each load gets its own `AbortController` |
| **Slot unload** (`unloadPartialSlot`) | `slotId: string` | active `PlanDocument` reverts to boot + remaining slots; affected `planId[]` returned | unload **aborts only that slot's** `AbortController`; boot/app-level objects stay mounted; a `planId` with no boot snapshot **and** no remaining slots is removed from the active set |
| **Inject route** (`injectPartial`) | `container`, `html`, `slot` | container HTML set; load when embedded plans present, unload when none | empty `[data-reactive-plan]` script body is a real **external-input boundary error** (`throw`), not a silent skip — the injected HTML is non-framework input |

**Value-object / construction invariants (null is unrepresentable by construction,
not guarded by exceptions):**

- `AppliedPlans` holds three `Map`s that are **never null** — created in the field
  initializer. There is no "unset" state to guard.
- `recompose` **always builds a fresh `PlanDocument`** via `emptyPlan(planId)` /
  `snapshotPlan`; it never receives or returns a partially-initialized document, so
  there is no nullable "current document" to defend.
- A slot that is not loaded simply has **no entry** in `partialSlotLoads` —
  `unloadSlot` of an absent slot returns `[]`. Absence is modeled by map absence,
  not by a null sentinel.
- The single throw in the whole module is at the **injected-HTML boundary** (empty
  plan element). That is correct: injected HTML is external, non-framework input.
  There are **no** defensive throws over the framework-generated `PlanDocument`
  shape.

---

## 4. File Layout

Slot lives in the runtime; per the cohesion rule its files sit together in
`lifecycle/` (composition) + the one `execution/` injector. C# touch-points are
*not* created — they already exist in Plan/Reaction.

| File | Action | Role |
|---|---|---|
| `Alis.Reactive.Assets/runtime/lifecycle/applied-plans.ts` | rename of `browser-plans.ts` | `AppliedPlans` class + `appliedPlans` + `composeInitialPlans` |
| `Alis.Reactive.Assets/runtime/lifecycle/merge-policy.ts` | new (extracted from `browser-plans.ts`) | `MergePolicy`, `emptyPlan`, `snapshotPlan` |
| `Alis.Reactive.Assets/runtime/lifecycle/component-merge.ts` | kept | component replace/join primitives `MergePolicy` calls |
| `Alis.Reactive.Assets/runtime/lifecycle/object-contracts.ts` | kept | `mergeObjectContracts` (type-contract merge) |
| `Alis.Reactive.Assets/runtime/lifecycle/boot.ts` | edit | `loadPartialSlot`/`unloadPartialSlot` delegate to `appliedPlans`; import path updated |
| `Alis.Reactive.Assets/runtime/execution/inject.ts` | edit | export `injectPartial` (renamed `injectHtml`); import `loadPartialSlot`/`unloadPartialSlot` from boot |
| `Alis.Reactive.Assets/runtime/__tests__/slot.*.test.ts` | new | the §6 fixtures |

> No file under `Alis.Reactive/` (C#) is created or edited by the Slot module
> itself. The `inject.ts` export rename forces a one-line call-site update in the
> **Reaction** `executeInject` handler — that edit is logged against Reaction.

---

## 5. Compile-Ready Skeleton

Bodies are `// TODO` referencing the §6 fixtures and the source the dev mirrors.

### `lifecycle/merge-policy.ts`

```ts
// merge-policy.ts — the ONE replace-vs-append rule shared by C# container merge
// and TS recompose. Type contracts merge; components replace-by-key with the
// boot/slot join rules; behaviors append in order.

import type { PlanDocument } from "../types";
import { mergeBootComponent, mergeSlotComponent } from "./component-merge";
import { mergeObjectContracts } from "./object-contracts";

export const MergePolicy = {
  composeBootPlanInto(target: PlanDocument, incoming: PlanDocument): void {
    // TODO: mergeTypeContracts; for each incoming component → mergeBootComponent;
    // target.behaviors.push(...incoming.behaviors).  Mirror browser-plans.ts:153–161.
    // Fixture: ssr_join_merges_types_replaces_components_appends_behaviors
  },

  composeSlotPlanInto(
    target: PlanDocument,
    incoming: PlanDocument,
    bootPlan: PlanDocument | undefined,
  ): void {
    // TODO: mergeTypeContracts; for each incoming component → mergeSlotComponent
    // (pass whether it existed in bootPlan); behaviors append.
    // Mirror browser-plans.ts:163–180.
    // Fixture: slot_components_replace_boot_components_by_key
  },
};

function mergeTypeContracts(target: PlanDocument, incoming: PlanDocument): void {
  // TODO: for each [typeKey, contract] of incoming.types →
  //   target.types[typeKey] = mergeObjectContracts(target.types[typeKey], contract)
  // Mirror browser-plans.ts:182–186.
}

export function emptyPlan(planId: string): PlanDocument {
  // TODO: return a NEW doc { version:3, planId, scope:{kind:"root"}, types:{},
  //   components:{}, behaviors:[] }.  Mirror browser-plans.ts:197–206.
  // Fixture: recompose_builds_fresh_document_never_mutates_boot_snapshot
}

export function snapshotPlan(plan: PlanDocument): PlanDocument {
  // TODO: shallow-copy each field into a NEW doc (spread scope/types/components,
  //   slice behaviors).  Mirror browser-plans.ts:208–217.
  // Fixture: recompose_builds_fresh_document_never_mutates_boot_snapshot
}
```

### `lifecycle/applied-plans.ts`

```ts
// applied-plans.ts — composition state for all applied plans: boot snapshots,
// loaded partial slots, per-slot AbortController. Active plans recompose from
// the boot snapshot plus currently-loaded slots; the boot snapshot is immutable.

import type { PlanDocument, Behavior } from "../types";
import { MergePolicy, emptyPlan, snapshotPlan } from "./merge-policy";

export interface BrowserPlanWiring {
  wireBehaviors: (behaviors: Behavior[], plan: PlanDocument, signal?: AbortSignal) => void;
  wireContainerValidation: (plan: PlanDocument, signal?: AbortSignal) => void;
}

interface PartialSlotLoad {
  readonly abortController: AbortController;
  readonly plans: PlanDocument[];
}

export class AppliedPlans {
  private readonly activePlans = new Map<string, PlanDocument>();
  private readonly bootSnapshots = new Map<string, PlanDocument>();
  private readonly partialSlotLoads = new Map<string, PartialSlotLoad>();

  register(plan: PlanDocument): void {
    // TODO: activePlans.set(planId, plan); bootSnapshots.set(planId, snapshotPlan(plan)).
    // Mirror browser-plans.ts:31–34.  Fixture: boot_plan_registered_as_immutable_snapshot
  }

  loadPartialSlot(slotId: string, plans: PlanDocument[], wiring: BrowserPlanWiring): string[] {
    // TODO: affected = new Set(unloadSlot(slotId)); abort = new AbortController();
    //   slotPlans = plans.map(snapshotPlan); record load; add loaded planIds to affected;
    //   recomposePlans(affected); for each slotPlan wire behaviors under abort.signal;
    //   for each loaded planId wire container validation; return [...affected].
    // Mirror browser-plans.ts:36–57.
    // Fixtures: slot_load_recomposes_then_wires_under_abort_signal,
    //           loading_slot_replaces_prior_load_on_same_slot
  }

  unloadPartialSlot(slotId: string): string[] {
    // TODO: affected = new Set(unloadSlot(slotId)); recomposePlans(affected);
    //   return [...affected].  Mirror browser-plans.ts:59–63.
    // Fixture: slot_unload_aborts_slot_wiring_and_reverts_active_plan
  }

  get(planId: string): PlanDocument | undefined {
    // TODO: return activePlans.get(planId).  Mirror browser-plans.ts:65–67.
  }

  reset(): void {
    // TODO: abortSlots(); clear all three maps.  Mirror browser-plans.ts:69–74.
  }

  private unloadSlot(slotId: string): string[] {
    // TODO: look up slot; if absent return []; else delete it, abort its controller,
    //   return planIds in its plans.  Mirror browser-plans.ts:76–83.
    // Fixture: unloading_absent_slot_is_a_noop
  }

  private recomposePlans(planIds: Iterable<string>): void {
    // TODO: for each planId → recomposePlan(planId).  Mirror browser-plans.ts:85–89.
  }

  private recomposePlan(planId: string): void {
    // TODO: bootPlan = bootSnapshots.get(planId); slotPlans = slotPlansFor(planId);
    //   if no boot AND no slots → activePlans.delete(planId); return.
    //   target = emptyPlan(planId) registered into activePlans (NEW doc, not reset-in-place);
    //   if bootPlan → MergePolicy.composeBootPlanInto(target, bootPlan);
    //   for each slotPlan → MergePolicy.composeSlotPlanInto(target, slotPlan, bootPlan).
    // Mirror browser-plans.ts:91–110 BUT build a fresh doc (drop resetPlanDocument).
    // Fixtures: recompose_builds_fresh_document_never_mutates_boot_snapshot,
    //           planid_with_no_boot_and_no_slots_is_dropped
  }

  private slotPlansFor(planId: string): PlanDocument[] {
    // TODO: collect every loaded slot plan whose planId matches.
    // Mirror browser-plans.ts:122–131.
  }

  private abortSlots(): void {
    // TODO: abort every slot's controller.  Mirror browser-plans.ts:133–137.
  }
}

export const appliedPlans = new AppliedPlans();

export function composeInitialPlans(plans: PlanDocument[]): PlanDocument[] {
  // TODO: group by planId into emptyPlan(planId); MergePolicy.composeBootPlanInto
  //   each; return the composed docs.  Mirror browser-plans.ts:142–151.
  // Fixture: independent_model_partial_boots_as_separate_plan
}
```

### `execution/inject.ts`

```ts
// inject.ts — injectPartial: inject HTML into a slot, extract embedded plan
// scripts, route to load (plans present) or unload (none).

import type { PlanDocument } from "../types";
import { loadPartialSlot, unloadPartialSlot } from "../lifecycle/boot";

interface SyncfusionBase { append(nodes: ChildNode[], target: HTMLElement, shouldClone?: boolean): void; }
interface SyncfusionGlobal { readonly ej?: { readonly base?: SyncfusionBase }; }

export function injectPartial(container: HTMLElement, html: string, slot: string): void {
  // TODO: parse html into a temp div; querySelectorAll("[data-reactive-plan]");
  //   for each: trim textContent — THROW on empty (external-input boundary);
  //   JSON.parse into plans[]; el.remove().  Set container.innerHTML="".
  //   Append via ej.base.append when present else container.append.
  //   if plans.length === 0 → unloadPartialSlot(slot) else loadPartialSlot(slot, plans).
  // Mirror inject.ts:22–49 (export renamed injectHtml → injectPartial).
  // Fixtures: injecting_html_with_plan_scripts_loads_the_slot,
  //           injecting_empty_html_unloads_the_slot,
  //           empty_plan_script_in_injected_html_throws
}
```

### `lifecycle/boot.ts` (Slot's two delegating entry points — edit, not rewrite)

```ts
import { appliedPlans, type BrowserPlanWiring } from "./applied-plans";

export function loadPartialSlot(slotId: string, incoming: PlanDocument[]): void {
  // TODO: affected = appliedPlans.loadPartialSlot(slotId, incoming, browserPlanWiring());
  //   clear validation summary per affected planId; log.  Mirror boot.ts:74–89.
}

export function unloadPartialSlot(slotId: string): void {
  // TODO: affected = appliedPlans.unloadPartialSlot(slotId);
  //   clear validation summary per affected planId; log.  Mirror boot.ts:91–102.
}
```

---

## 6. Acceptance Fixtures (matrix cases this module satisfies)

From [`04-matrix-validation-components-slots.md`](../04-matrix-validation-components-slots.md)
**Band C — Partial Slots / Composition** (5 deterministic cases). Each matrix row
becomes one named acceptance fixture. The two authoring-only rows (root, SSR-join,
independent) are proven through `composeInitialPlans`; the two browser rows (load,
unload) through `AppliedPlans` + `injectPartial`.

| Matrix row (Band C) | Acceptance fixture name | Asserts |
|---|---|---|
| **Root view plan** | `root_view_plan_boots_as_single_plan` | `composeInitialPlans([root])` yields one `PlanDocument`, `scope.kind:"root"`, `planId` = model `FullName`; booted once. |
| **Same-model partial (SSR join)** | `same_model_partial_ssr_joins_into_one_plan` | two scripts with the same `planId` → `composeInitialPlans` produces **one** doc: types merged, components replaced-by-key, behaviors appended (the boot arm of `MergePolicy`). |
| **Independent-model partial** | `independent_model_partial_boots_as_separate_plan` | two scripts with different `planId` → `composeInitialPlans` yields **two** docs, booted independently. |
| **Browser slot injection (load)** | `injecting_html_with_plan_scripts_loads_the_slot` | `injectPartial(container, htmlWithPlanScript, slot)` → embedded plan extracted, `loadPartialSlot` recomposes a **fresh** active doc for the affected `planId`, slot behaviors/validation wired under the slot's `AbortController`. |
| **Browser slot unload** | `injecting_empty_html_unloads_the_slot` | `injectPartial(container, htmlNoPlanScript, slot)` → `unloadPartialSlot` aborts only that slot's controller, drops its plans, active doc reverts to boot + remaining slots; boot/app-level objects stay mounted. |

**Supporting unit fixtures (prove the debt-fixes the redesign requires):**

| Fixture name | Asserts (the invariant the redesign fixes) |
|---|---|
| `boot_plan_registered_as_immutable_snapshot` | `register` snapshots; later recompose does not mutate the boot snapshot. |
| `recompose_builds_fresh_document_never_mutates_boot_snapshot` | recompose returns a **new** object identity per `planId` (drops in-place `resetPlanDocument`). |
| `slot_components_replace_boot_components_by_key` | slot arm of `MergePolicy`: same component key → slot replaces boot; behaviors still append. |
| `ssr_join_merges_types_replaces_components_appends_behaviors` | boot arm of `MergePolicy`: one shared rule, identical to the C# container merge. |
| `loading_slot_replaces_prior_load_on_same_slot` | re-loading the same `slotId` aborts/replaces the prior load (the leading `unloadSlot` in `loadPartialSlot`). |
| `slot_unload_aborts_slot_wiring_and_reverts_active_plan` | unload aborts only slot-owned wiring; app-level/boot stay mounted. |
| `unloading_absent_slot_is_a_noop` | `unloadPartialSlot` of an unknown slot returns `[]`, mutates nothing (absence modeled by map absence, not a null guard). |
| `planid_with_no_boot_and_no_slots_is_dropped` | a `planId` present only via a now-unloaded slot is removed from the active set. |
| `empty_plan_script_in_injected_html_throws` | empty `[data-reactive-plan]` body throws — the **one** legitimate boundary throw (external injected HTML). |

> Coverage gate: every Band C row (5) maps to a named fixture above; the 9
> supporting fixtures prove the redesign's stated Slot fixes (fresh document, one
> `MergePolicy`, snapshot-safety, boundary-only throw). No Band C row is left
> uncovered.
