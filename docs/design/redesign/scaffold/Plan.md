# Plan — Implementation Spec (scaffold)

> Mechanical build spec for the **Plan** micro-module (the document *spine*). A
> developer opens this file, reads the surface + skeleton + fixtures, and types the
> obvious body. Every claim is grounded in actual source, cited inline. Names are
> from [`03-naming.md`](../03-naming.md) (Plan table); responsibility/ownership from
> [`02-micro-modules.md`](../02-micro-modules.md) (Plan row); acceptance fixtures
> from [`04-matrix-validation-components-slots.md`](../04-matrix-validation-components-slots.md)
> Band C + the "kernels every row leans on" preamble; design grounding
> [`00-design.md`](../00-design.md) §2 (module map + `Slot -> Plan` note) and §3
> (Plan-document spine).
>
> Source read as the requirement: `Alis.Reactive/ReactivePlan.cs`,
> `Alis.Reactive/PlanModel/PlanBuildContext.cs`,
> `Alis.Reactive/PlanModel/PlanDocument.cs`,
> `Alis.Reactive/PlanModel/PlanTerms.cs` (`PlanString`/`PlanId`/`PlanIdentity`/`PlanScope`),
> `Alis.Reactive/Razor/Extensions/PlanExtensions.cs`,
> `Alis.Reactive.Assets/runtime/root.ts`, `Alis.Reactive.Assets/runtime/lifecycle/boot.ts`,
> `Alis.Reactive.Assets/runtime/execution/execute.ts` (the `activeRuntimePlan`
> singleton, lines 28–42), `Alis.Reactive.Assets/runtime/domain/runtime-plan.ts`,
> `Alis.Reactive.Assets/runtime/types/plan.ts` (lines 5–24).

---

## 1. Responsibility · Owns · Depends

**Responsibility (one sentence).** The plan-document spine: an authoring sink
(`PlanBuildContext`) the concept-slices write into, frozen into an immutable
`PlanDocument` (version 3), serialized into a `<script data-reactive-plan>` element
by `RenderPlan`, discovered by `root`, and booted by `boot` with the active plan
passed **explicitly** — no hidden singleton.

**Owns** (from `02-micro-modules.md` Plan row, names from `03-naming.md` Plan table):

| Side | Owns |
|---|---|
| `→` C# authoring/plan | `ReactivePlan<TModel>` (the view entry object), `PlanBuildContext` (narrow Declare/Wire sink), `PlanDocument` (immutable v3: `planId`/`scope`/`types`/`components`/`behaviors`), `PlanIdentity` + `PlanId` + `PlanScope`/`RootPlanScope`/`PartialPlanScope` (the model-derived key + root/partial discriminator), `PlanExtensions` (`ReactivePlan`/`ResolvePlan`/`RenderPlan`), and **one id-sanitization rule** (`PlanElementId`) for the `<script>`/summary element ids |
| `⇒` TS runtime | `root` (`root.ts`: discover `[data-reactive-plan]`, parse, `composeInitialPlans`, `boot` each), `boot` (`lifecycle/boot.ts`: wire each composed plan), `ActivePlan` — the active plan passed **explicitly** down to `executeReaction`, replacing the `activeRuntimePlan` global |
| contract | the generated `PlanDocument` + `PlanScope`/`RootPlanScope`/`PartialPlanScope` interfaces in `types/plan.ts` (lines 5–24) — emitted by **Kind**'s `PlanContractGenerator`, not hand-authored here |

**Depends on** (from the module-dependency graph in `00-design.md` §2 — acyclic, Plan
is the aggregate root, edges point *out*): **Trigger** (`StartsWhen`/`Behavior` —
boot wires them), **Reaction** (`executeReaction` — boot threads `ActivePlan` into
it; the `inject` reaction is the only path that reaches Slot), **Component**
(`ComponentObjects`/`BrowserObject` — `PlanBuildContext` declares them, boot wires
container validation per component), **Slot** (`PlanScope` is the slot axis; Slot's
`composeInitialPlans`/`AppliedPlans` recompose the `PlanDocument` Plan owns — **Slot
depends on Plan, Plan does NOT import Slot**), and **Kind** (the serializer +
generated contract). Plan does **not** depend on Value, Condition, Request, Shape,
Validation, or Plugin directly — those are reached through Reaction/Component.

> **The `Slot -> Plan` edge, restated for the implementer** (`00-design.md` §2 note):
> composition is a *downward* dependency on the `PlanDocument` type, never an upward
> callback into `boot`. `root.ts` reaches `composeInitialPlans` (Slot) at boot, and
> the `inject` reaction reaches `loadPartialSlot`/`unloadPartialSlot` (Slot) through
> the **Reaction** `executeInject` handler. Plan never imports `applied-plans.ts`.
> This is the layered replacement for today's boot↔browser-plans cycle.

**What Plan does NOT own / must not invent.**

- **Not the JSON mechanism.** `PlanSerializer` (camelCase, sole JSON owner),
  `PlanNodeDiscriminator`, `PlanContractGenerator`, and `ContractDriftGate` belong to
  **Kind**. Today these live as `ReactivePlanSerializer` (`ReactivePlan.cs:206–221`),
  `WriteOnlyPolymorphicConverter` (`Serialization/`), and `PlanTypeScriptContract`
  (`PlanModel/PlanTypeScriptContract.cs`). Plan *calls* `PlanSerializer.Serialize`;
  it does not own the converter or the generator. If you find yourself editing
  `plan.ts` by hand or writing a `JsonConverter`, stop — that is Kind.
- **Not the composition state.** `AppliedPlans` (boot snapshots + slots + abort),
  `recompose`, `MergePolicy`, `composeInitialPlans` belong to **Slot**
  (`lifecycle/applied-plans.ts` / `merge-policy.ts`). Plan's `boot` *delegates* to
  them. The `activeRuntimePlan` singleton and the four `reset*ForTests` functions are
  **deleted**, not relocated.
- **Not `executeReaction`.** That is **Reaction**. Plan's job is to pass `ActivePlan`
  into it explicitly.
- **No `p.Slot(...)`/`InjectInto`** — Band C is explicit there is no such verb.

---

## 2. Public Surface

> "Public" here means the surface other modules + the view layer call. Per Rule 8
> (API surface frozen): plan-model types have `internal` constructors; the only
> genuinely `public` members are the view-facing ones Razor and `Html.RenderPlan`
> rely on (`ReactivePlan<TModel>`, `PlanExtensions`, `PlanScope.Kind`). The TS
> counterparts are `export`ed because they cross the runtime module boundary.

### 2a. C# — `ReactivePlan<TModel>` (the view entry object)

`Alis.Reactive/ReactivePlan.cs`. Kept as-is in shape; the only Plan-pass change is
that `Render()` calls **Kind**'s `PlanSerializer` (renamed from
`ReactivePlanSerializer`) and `RenderPlan` uses the shared `PlanElementId` rule.

```csharp
namespace Alis.Reactive;

/// <summary>
/// Collects reactive behavior for a view — triggers, reactions, and component
/// registrations — and serializes it as a <see cref="PlanModel.PlanDocument"/> for
/// browser execution. Created at the top of a view by
/// <c>Html.ReactivePlan&lt;TModel&gt;()</c> and serialized at the bottom by
/// <c>Html.RenderPlan(plan)</c>.
/// </summary>
public sealed class ReactivePlan<TModel> where TModel : class
```

| Member | Signature | Intent (XML-doc-style) |
|---|---|---|
| ctor | `internal ReactivePlan(ReactivePlanScope scope, IServiceProvider? services)` | Opens a `PlanBuildContext` for the model `TModel` under the given scope (root/partial). `internal` — devs enter through `Html.ReactivePlan`/`ResolvePlan`. |
| `PlanId` | `public string PlanId` | The model-derived key (`typeof(TModel).FullName`). Stable across root + same-model partials so they compose. |
| `IsPartial` | `public bool IsPartial` | True when this plan is a partial that merges into a parent plan. |
| `Context` | `internal PlanBuildContext Context` | The authoring sink the DSL builders write into. |
| `RegisterPlugin(...)` | `public void` / `public TPlugin` | Register a plugin contract before any `p.Plugin(...)` read (delegates to Plugin module). |
| `Render()` | `public string Render()` | Resolve all registrations + validation jobs, then `PlanSerializer.Serialize(Context.BuildPlan())` → compact camelCase JSON. |
| `RenderFormatted()` | `public string RenderFormatted()` | Same, indented JSON for debugging. |
| `RendersValidationSummary` | `internal bool` | Root views render the `<div data-reactive-validation-summary>` fallback; partials do not (from `ReactivePlanScope`). |

### 2b. C# — `PlanBuildContext` (the authoring sink — narrow Declare/Wire verbs)

`Alis.Reactive/PlanModel/PlanBuildContext.cs`. Kept as-is — it is already the narrow
sink the design wants. It exposes only the mutation verbs the DSL needs while
authoring, and snapshots into a `PlanDocument`.

```csharp
namespace Alis.Reactive.PlanModel;

/// <summary>
/// The authoring sink the public DSL builders write into while a view composes a
/// plan. Exposes only narrow Declare/Wire verbs; snapshots into an immutable
/// <see cref="PlanDocument"/> via <see cref="BuildPlan"/>.
/// </summary>
public sealed class PlanBuildContext
```

| Member | Signature | Intent |
|---|---|---|
| ctor | `internal PlanBuildContext(PlanIdentity identity, RegisteredInputComponents registrations)` | Construct over a non-null identity + registration set. The two `??throw ArgumentNullException` are the authoring boundary. |
| `BuildPlan` | `internal PlanDocument BuildPlan()` | Snapshot identity + object contracts + components + behaviors into the immutable document. |
| `DeclareElement` / `DeclareObjectTarget` / `DeclareLayoutObject` / `DeclareInputComponent` | `internal ComponentKey …` | Declare a browser object (native element, page object, layout object, model-bound input) — Component module owns the objects; Plan owns the sink verb. |
| `DeclareProperty` / `DeclareMethod` / `DeclarePluginProperty` / `DeclarePluginMethod` / `RegisterPlugin` | `internal …` | Declare contract members on a declared object (Component/Plugin). |
| `WireComponentEvent` | `internal void WireComponentEvent(string componentId, string vendor, string eventName, ReactionGraph reaction)` | Declare the target object + add a `Behavior.On(StartsWhen.ComponentEvent(...), reaction)` (Trigger + Reaction). |
| `AddBehavior` | `internal void AddBehavior(Behavior behavior)` | Append one trigger→reaction edge to the `BehaviorGraph`. |
| `RegisterValidationJob` / `ValidationJobs` / `GetComponent` / `SetComponent` / `RequireRegistrationById` / `RegisterInputComponents` | `internal …` | Validation enrichment + component lookup hooks resolved during `Render()` (Validation/Component). |

> A dev implementing Plan **does not redesign this surface** — it is already the
> narrow sink. The Plan pass only confirms it builds a `PlanDocument` and that
> `BuildPlan` is the one freeze point.

### 2c. C# — `PlanDocument` (the immutable v3 document)

`Alis.Reactive/PlanModel/PlanDocument.cs`. The frozen contract. `internal sealed`;
serialized by Kind's `PlanSerializer`.

```csharp
namespace Alis.Reactive.PlanModel;

/// <summary>
/// The immutable plan document — the serialized contract between C# and the browser
/// runtime. Produced by <see cref="PlanBuildContext.BuildPlan"/> once construction is
/// complete. Version 3.
/// </summary>
internal sealed class PlanDocument
```

| Member | Signature | Intent |
|---|---|---|
| ctor | `internal PlanDocument(PlanIdentity identity, IReadOnlyDictionary<string,BrowserObjectContract> types, IReadOnlyDictionary<string,ComponentObject> components, IReadOnlyList<Behavior> behaviors)` | Construct from a built identity + snapshots. Every argument is supplied by `BuildPlan` — never null, by construction. |
| `Version` | `public int Version => 3` | The contract version. Constant 3. |
| `PlanId` | `public string PlanId` | `identity.PlanIdForJson` — the model `FullName`. |
| `Scope` | `public PlanScope Scope` | `root` or `partial` (`identity.ScopeForJson`). |
| `Types` | `public IReadOnlyDictionary<string, BrowserObjectContract> Types` | The declared object contracts (Component). |
| `Components` | `public IReadOnlyDictionary<string, ComponentObject> Components` | The declared browser objects (Component). |
| `Behaviors` | `public IReadOnlyList<Behavior> Behaviors` | The trigger→reaction edges (Trigger/Reaction). |

### 2d. C# — `PlanId` / `PlanIdentity` / `PlanScope` (identity + scope)

`Alis.Reactive/PlanModel/PlanTerms.cs`. `PlanId` is a `PlanString` value object;
`PlanIdentity` pairs it with a scope; `PlanScope` is the polymorphic root/partial
discriminator (its `kind` is written by Kind's polymorphic mechanism).

```csharp
internal sealed class PlanId : PlanString
{
    internal static PlanId ForModel(Type modelType);   // modelType.FullName
    internal static PlanId Of(string value);
}

internal sealed class PlanIdentity
{
    internal string PlanIdForJson { get; }              // _planId.Value
    internal PlanScope ScopeForJson { get; }
    internal static PlanIdentity Root(PlanId planId);
    internal static PlanIdentity Partial(PlanId planId);
}

/// <summary>Base class for plan merge scope. Not constructed in application code.</summary>
[JsonConverter(typeof(WriteOnlyPolymorphicConverter<PlanScope>))]  // mechanism owned by Kind
public abstract class PlanScope
{
    internal static PlanScope Root { get; }             // RootPlanScope
    internal static PlanScope Partial { get; }          // PartialPlanScope
    public abstract string Kind { get; }                // "root" | "partial"
}
public sealed class RootPlanScope    : PlanScope { public override string Kind => "root"; }
public sealed class PartialPlanScope : PlanScope { public override string Kind => "partial"; }
```

> `PlanString` (base value object) enforces non-null + non-empty in its constructor —
> that is the authoring boundary, and the reason `PlanId` is never null downstream.
> Null is unrepresentable by construction (see §3), not guarded later.

### 2e. C# — `PlanExtensions` (the view verbs) + `PlanElementId` (the one id rule)

`Alis.Reactive/Razor/Extensions/PlanExtensions.cs`. The three view verbs. The Plan
pass adds **one** shared `PlanElementId` rule because the sanitization
(`PlanId.Replace('.','-').Replace('+','-')`) is **duplicated** in the net48 and net10
arms of `RenderPlan` (`PlanExtensions.cs:116` and `:137`) — that duplication is the
debt this module closes.

```csharp
namespace Alis.Reactive.Native.Extensions;

public static class PlanExtensions
{
    /// <summary>Creates a root-view <see cref="ReactivePlan{TModel}"/>. First call in a view.</summary>
    public static ReactivePlan<TModel> ReactivePlan<TModel>(this IHtmlHelper<TModel> html) where TModel : class;

    /// <summary>Creates a partial-view plan that merges into the parent plan by shared PlanId.</summary>
    public static ReactivePlan<TModel> ResolvePlan<TModel>(this IHtmlHelper<TModel> html) where TModel : class;

    /// <summary>
    /// Serializes the plan into a <c>&lt;script type="application/json"
    /// data-reactive-plan&gt;</c> element (plus the validation-summary div for root
    /// views). Last call in every view; a plan that is not rendered produces no
    /// reactive behavior.
    /// </summary>
    public static IHtmlContent RenderPlan<TModel>(this IHtmlHelper<TModel> html, ReactivePlan<TModel> plan) where TModel : class;
}

/// <summary>The ONE rule that turns a PlanId into a DOM-safe element id suffix.
/// Used by both the net48 and net10 RenderPlan arms so they cannot diverge.</summary>
internal static class PlanElementId
{
    internal static string For(string planId);          // planId.Replace('.','-').Replace('+','-')
}
```

> `ReactivePlan`/`ResolvePlan`/`RenderPlan` keep their `#if NET48` / `#else` arms
> verbatim (System.Web vs AspNetCore Html types). The **only** change is both
> `RenderPlan` arms call `PlanElementId.For(plan.PlanId)` instead of inlining the two
> `.Replace(...)` calls.

### 2f. TS — `root` (discover) — `root.ts`

Crosses the contract: `PlanDocument`/`PlanScope` here are the **generated** types
from `types/plan.ts` (owned by Kind). `root.ts` is the bundle entry point.

```ts
// root.ts — runtime entry. Auto-discovers [data-reactive-plan] scripts on page load,
// composes same-model plans, boots each composed plan.

startRuntimeWhenDocumentIsReady(): void;   // wait for DOMContentLoaded, then startRuntime
startRuntime(): void;                       // drain plugin queue, init app-level singletons, compose + boot
discoverPlans(): PlanDocument[];            // querySelectorAll("[data-reactive-plan]"), JSON.parse each
```

> `discoverPlans` is one of the 3 justified wide DOM queries (root.ts:25 in
> CLAUDE.md). An empty `[data-reactive-plan]` body **throws** — external/injected
> input is the real boundary (`root.ts:68`).

### 2g. TS — `boot` (wire) — `lifecycle/boot.ts`

```ts
// boot.ts — wires behaviors + container validation for one composed plan, then
// records it. The active plan is passed EXPLICITLY into executeReaction (no global).

/** Wire one composed plan: container validation first, then behaviors (two-phase:
 *  non-page-ready listeners before page-ready), then register with AppliedPlans. */
export function boot(plan: PlanDocument): void;

/** Host/runtime entry: load a partial slot's plans and recompose. Delegates to Slot. */
export function loadPartialSlot(slotId: string, incoming: PlanDocument[]): void;

/** Host/runtime entry: unload a partial slot and recompose. Delegates to Slot. */
export function unloadPartialSlot(slotId: string): void;

/** The currently composed active document for a planId, or undefined. (Slot's store.) */
export function getBootedPlan(planId: string): PlanDocument | undefined;

/** Test-only: abort boot wiring + reset Slot/validation/plugin singletons. */
export function resetBootStateForTests(): void;
```

### 2h. TS — `ActivePlan` — the explicit active plan (DELETES the singleton)

The single largest Plan-pass change. Today `execution/execute.ts:28–42` holds:

```ts
let activeRuntimePlan: RuntimePlan | undefined;          // DELETE
export function setActivePlan(plan: PlanDocument): void; // DELETE
export function resetActivePlanForTests(): void;         // DELETE
function runtimePlanFor(plan: PlanDocument | undefined): RuntimePlan {
  if (plan) return RuntimePlan.from(plan);
  if (activeRuntimePlan) return activeRuntimePlan;        // the fallback that hides "whichever booted last"
  throw new Error("[alis] no active plan");
}
```

After the Plan pass, `executeReaction` takes the active plan **required, not
optional**, and `RuntimePlan.from(plan)` (already memoized via the `WeakMap` in
`runtime-plan.ts:13`) is the only resolution path:

```ts
// execute.ts (Reaction module edits its signature; Plan owns the contract):
export function executeReaction(
  reaction: ReactionGraph,
  plan: PlanDocument,          // REQUIRED — passed down by boot/trigger/http; no optional, no global
  ctx?: ExecContext,
): ReactionCompletion;
```

`boot.ts` drops the `setActivePlan(plan)` call (`boot.ts:42`) — the plan is already
threaded into `wireBehavior(..., plan, ...)` (`boot.ts:58,62`), which passes it to
`executeReaction(reaction, plan, ...)` (`trigger.ts:24`). Every existing caller
already passes the plan explicitly (`http.ts:197,265,284`; `signalr.ts:123`;
`server-push.ts:89`; `native-action-link.ts:44` via `payload.plan`). The only edit is
**deleting the optional/global fallback**, so a missing plan is a compile error, not a
runtime "no active plan" throw.

### 2i. TS contract counterpart (generated, do NOT hand-write)

Plan introduces no new wire node. The contract types it reads are generated into
`types/plan.ts` (lines 5–24) by Kind's `PlanContractGenerator`:

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

Plan reads these; it never edits `plan.ts`.

---

## 3. Input → Output Contract

| Path | Input | Output | Invariants (by construction, not guarded) |
|---|---|---|---|
| **Create** (`Html.ReactivePlan<M>()` / `ResolvePlan<M>()`) | an `IHtmlHelper<TModel>`; the chosen scope (root/partial) | a `ReactivePlan<TModel>` over a fresh `PlanBuildContext` | `PlanId = typeof(TModel).FullName`, non-null (`PlanId.ForModel` throws only if a type has no full name — the authoring boundary). Scope is `Root` or `Partial`, never null. |
| **Author** (DSL builders write into `Context`) | Declare/Wire calls | mutations accumulated in `BrowserObjectContracts` + `ComponentObjects` + `BehaviorGraph` | the sink exposes only narrow verbs; there is no public mutable state to corrupt. |
| **Freeze** (`Context.BuildPlan()`) | the accumulated draft | one immutable `PlanDocument` (v3) | snapshots are taken once; `Types`/`Components`/`Behaviors` are read-only collections; `Version` is constant 3. |
| **Serialize** (`plan.Render()`) | a `PlanDocument` | compact camelCase JSON | delegates to **Kind**'s `PlanSerializer`; write-only (the polymorphic converters' `Read` throw — plan types are write-only). |
| **Render the element** (`RenderPlan`) | a `ReactivePlan<TModel>` | `<script id="alis-plan-{PlanElementId.For(planId)}" type="application/json" data-reactive-plan>{json}</script>` (+ `<div data-reactive-validation-summary>` for root views) | the element id is produced by the **one** `PlanElementId` rule, identical in both framework arms; root views emit the summary div, partials do not. |
| **Discover** (`root.discoverPlans`) | the DOM | `PlanDocument[]` (one per `[data-reactive-plan]` script) | an empty plan-script body **throws** — external/injected input is a real boundary; valid framework JSON is trusted, not validated. |
| **Boot** (`root` → `composeInitialPlans` (Slot) → `boot`) | discovered `PlanDocument[]` | each composed plan wired (container validation + behaviors) and registered | the active plan is threaded **explicitly** into `executeReaction`; there is no `activeRuntimePlan` global and no "whichever booted last" ambiguity. |

**Value-object / construction invariants — null is unrepresentable by construction,
not guarded by exceptions:**

- `PlanId` is a `PlanString`: its constructor rejects null/empty (`PlanTerms.cs:17–22`).
  Downstream code never sees a null `PlanId`, so there is no null guard on
  `PlanDocument.PlanId` — the value object closed the hole at the boundary.
- `PlanIdentity` rejects null `planId`/`scope` in its private ctor; the only entry
  points (`Root`/`Partial`) supply the canonical `PlanScope.Root`/`PlanScope.Partial`
  singletons, so `Scope` is never null. `PlanDocument` therefore stores them without
  a guard.
- `PlanScope` is a closed two-case polymorphic type (`RootPlanScope`/`PartialPlanScope`),
  each a singleton with a constant `Kind`. There is no "unset" scope to defend.
- `PlanDocument` is constructed only by `PlanBuildContext.BuildPlan`, which always
  supplies non-null snapshots — there is no partially-initialized document and thus no
  nullable field to defend. `Version` is a constant, not a settable field.
- The single C# throw in the create path is `PlanId.ForModel` on a type with no
  `FullName` (`PlanTerms.cs:55–57`) — a developer authoring boundary. The single TS
  throw in the discover path is the empty-plan-script body (`root.ts:68`) — an
  external-input boundary. There are **no** defensive throws over the
  framework-generated `PlanDocument` shape, and the `"[alis] no active plan"` fallback
  throw is **removed** by making the plan a required parameter.

---

## 4. File Layout

Plan's C# lives where it already does (it is the spine; do not relocate). The TS
side removes the singleton and threads the plan explicitly.

| Layer | File | Action | Role |
|---|---|---|---|
| C# domain | `Alis.Reactive/ReactivePlan.cs` | edit | `ReactivePlan<TModel>` + `ReactivePlanScope`/`RootViewPlanScope`/`PartialViewPlanScope`. `Render()` calls Kind's `PlanSerializer` (rename `ReactivePlanSerializer` → moves to Kind). |
| C# domain | `Alis.Reactive/PlanModel/PlanBuildContext.cs` | kept | the authoring sink (no surface change). |
| C# domain | `Alis.Reactive/PlanModel/PlanDocument.cs` | kept | the immutable v3 document. |
| C# domain | `Alis.Reactive/PlanModel/PlanTerms.cs` | kept | `PlanString`/`PlanId`/`PlanIdentity`/`PlanScope`/`RootPlanScope`/`PartialPlanScope` (these stay here; other terms are owned by their slices). |
| C# Razor | `Alis.Reactive/Razor/Extensions/PlanExtensions.cs` | edit | `ReactivePlan`/`ResolvePlan`/`RenderPlan` (both arms call `PlanElementId.For`) + the new `PlanElementId` rule. |
| TS runtime | `Alis.Reactive.Assets/runtime/root.ts` | kept | `discoverPlans` + `startRuntime` (compose via Slot, boot each). |
| TS runtime | `Alis.Reactive.Assets/runtime/lifecycle/boot.ts` | edit | `boot` drops `setActivePlan`; `loadPartialSlot`/`unloadPartialSlot`/`getBootedPlan`/`resetBootStateForTests` delegate to Slot. |
| TS runtime | `Alis.Reactive.Assets/runtime/execution/execute.ts` | edit (Reaction owns the file; Plan owns the contract) | delete `activeRuntimePlan`/`setActivePlan`/`resetActivePlanForTests`; make `plan` a required parameter of `executeReaction`. |
| Contract (generated, **not** hand-edited) | `Alis.Reactive.Assets/runtime/types/plan.ts` (lines 5–24) | read-only | `PlanDocument` + `PlanScope` union — emitted by Kind. |
| C# tests | `tests/Alis.Reactive.UnitTests/PlanModel/PlanDocumentTests.cs` | new | the §6 A/B/C fixtures (identity, freeze, render-element). |
| TS tests | `Alis.Reactive.Assets/runtime/__tests__/boot.test.ts` | edit | the §6 D/E fixtures (discover, boot, explicit active plan). |

> The `setActivePlan`/`resetActivePlanForTests` deletions force one-line updates at
> their call sites (`boot.ts:42`, `boot.ts:127`). Those edits are logged against Plan
> because Plan owns the active-plan threading contract; the file itself is Reaction's.

---

## 5. Compile-Ready Skeleton

Bodies are `// TODO` referencing the §6 fixtures and the source the dev mirrors.
The C# document/identity types already exist and pass — the skeleton is the shape a
dev reproduces when rebuilding the spine cleanly. The TS edits are the load-bearing
change.

### `Razor/Extensions/PlanExtensions.cs` — the one id rule (decision point)

```csharp
namespace Alis.Reactive.Native.Extensions;

/// <summary>The ONE rule turning a PlanId into a DOM-safe element id suffix, shared
/// by both framework arms of RenderPlan so they cannot diverge.</summary>
internal static class PlanElementId
{
    internal static string For(string planId) =>
        planId.Replace('.', '-').Replace('+', '-');   // fixture: plan_element_id_sanitizes_dots_and_plus
}

public static class PlanExtensions
{
#if NET48
    public static IHtmlString RenderPlan<TModel>(this HtmlHelper<TModel> html, ReactivePlan<TModel> plan)
        where TModel : class
    {
        var json = plan.Render();
        var elementId = PlanElementId.For(plan.PlanId);   // was: plan.PlanId.Replace('.','-').Replace('+','-')
        var script = $"<script type=\"application/json\" id=\"alis-plan-{elementId}\" data-reactive-plan data-trace=\"trace\">{json}</script>";
        // TODO: if !plan.RendersValidationSummary return MvcHtmlString(script);
        //       else append <div data-reactive-validation-summary="{HtmlEncode(PlanId)}" hidden></div>
        //       fixtures: root_view_renders_script_and_summary, partial_renders_script_only
    }
#else
    public static IHtmlContent RenderPlan<TModel>(this IHtmlHelper<TModel> html, ReactivePlan<TModel> plan)
        where TModel : class
    {
        // TODO: identical to the net48 arm but with HtmlString / IHtmlContent.
        //       Same PlanElementId.For + RendersValidationSummary branch.
    }
#endif
    // ReactivePlan / ResolvePlan keep their #if arms verbatim (only the scope differs:
    // ReactivePlanScope.RootView vs .PartialView). Fixtures: create_root_plan_is_root_scope,
    // create_partial_plan_is_partial_scope.
}
```

### `ReactivePlan.cs` — `Render` delegates to Kind's serializer (decision point)

```csharp
namespace Alis.Reactive;

public sealed class ReactivePlan<TModel> where TModel : class
{
    public string PlanId => _planId.Value;                 // model FullName
    public bool IsPartial => _scope.IsPartial;
    internal PlanBuildContext Context => _context;

    /// <summary>Resolve registrations + validation, then serialize the frozen document.</summary>
    public string Render()
    {
        ResolveAll(_services);
        return PlanSerializer.Serialize(_context.BuildPlan());   // Kind owns PlanSerializer
        // fixture: render_serializes_built_document_camelcase
    }

    private void ResolveAll(IServiceProvider? services)
    {
        // TODO: _context.RegisterInputComponents(); if no validation jobs return;
        //       else bind queued validation jobs (Validation module).
        //       (verbatim from ReactivePlan.cs:116–129)
    }
}

// ReactivePlanScope / RootViewPlanScope / PartialViewPlanScope kept verbatim
// (ReactivePlan.cs:165–204): RootView → PlanIdentity.Root + RendersValidationSummary=true;
// PartialView → PlanIdentity.Partial + RendersValidationSummary=false.
```

### `execution/execute.ts` — delete the singleton, require the plan (load-bearing)

```ts
// execute.ts — ReactionGraph executor. The plan is passed EXPLICITLY; no global.

import type { PlanDocument, ReactionGraph, ExecContext } from "../types";
import { RuntimePlan } from "../domain/runtime-plan";
import { ExecutionContext } from "../domain/execution-context";

export type ReactionCompletion = void | Promise<void>;

// DELETED: let activeRuntimePlan; setActivePlan; resetActivePlanForTests; runtimePlanFor's fallback.

export function executeReaction(
  reaction: ReactionGraph,
  plan: PlanDocument,                 // required — fixture: execute_requires_explicit_plan
  ctx?: ExecContext,
): ReactionCompletion {
  return executeReactionWith(reaction, RuntimePlan.from(plan), ExecutionContext.from(ctx));
  // RuntimePlan.from is already memoized (runtime-plan.ts:13 WeakMap) — fixture: runtime_plan_memoized_per_document
}

// executeReactionWith + all step executors keep their bodies; only the entry signature changed.
```

### `lifecycle/boot.ts` — drop `setActivePlan` (load-bearing)

```ts
// boot.ts — wire one composed plan; the plan threads explicitly into executeReaction.

import type { PlanDocument, Behavior } from "../types";
import { wireBehavior } from "../execution/trigger";
import { wireLiveValidation } from "../validation/live-clear";
import { appliedPlans, type BrowserPlanWiring } from "./applied-plans";   // Slot

export function boot(plan: PlanDocument): void {
  // TODO: wireContainerValidation(plan, signal); wireBehaviors(plan.behaviors, plan, signal);
  //       appliedPlans.register(plan); markReactiveBooted(plan).
  //       DELETE the old setActivePlan(plan) call (boot.ts:42).
  //       fixtures: boot_wires_behaviors_with_explicit_plan, boot_registers_plan_with_applied_plans
}

function wireBehaviors(behaviors: Behavior[], plan: PlanDocument, signal?: AbortSignal): void {
  // TODO: two-phase — non-page-ready first, then page-ready; each → wireBehavior(.., plan, ..).
  //       fixture: page_ready_behaviors_wire_after_event_listeners
}

export function resetBootStateForTests(): void {
  // TODO: abort boot signal; appliedPlans.reset(); reset live-clear/action-link/plugin singletons.
  //       DELETE resetActivePlanForTests() (the singleton it reset is gone).
  //       fixture: reset_boot_state_clears_applied_plans_no_active_plan_global
}
```

### `root.ts` — discover + compose + boot (kept; shown for completeness)

```ts
// root.ts — entry. discover [data-reactive-plan], composeInitialPlans (Slot), boot each.

import { boot } from "./lifecycle/boot";
import { composeInitialPlans } from "./lifecycle/applied-plans";   // Slot
import type { PlanDocument } from "./types";

function startRuntime(): void {
  // drain plugin queue; init app-level singletons;
  for (const plan of composeInitialPlans(discoverPlans())) boot(plan);
}

function discoverPlans(): PlanDocument[] {
  // TODO: querySelectorAll("[data-reactive-plan]"); for each: trim textContent —
  //   THROW on empty (external-input boundary); JSON.parse; push.
  //   fixtures: discovers_each_reactive_plan_script, empty_plan_script_throws
}
```

---

## 6. Acceptance Fixtures (matrix cases this module satisfies)

The Plan-spine rows the module must satisfy, by name. Band C rows (root, SSR-join,
independent) are proved through `ReactivePlan`/`ResolvePlan` + `RenderPlan` (C#) and
`root`/`composeInitialPlans` (TS). The "kernels every row leans on — Plan" preamble
(`04-matrix-*`, line 49–52) fixes the spine invariants (v3 document, explicit
`ActivePlan`, id-sanitization). Each becomes one named test.

> The **composition** assertions (SSR merge of two same-model scripts into one doc;
> slot load/unload) are owned and proved by the **Slot** module's fixtures
> (`composeInitialPlans`, `AppliedPlans`). Plan proves the *spine* the composition
> runs on: a correct root/partial document is produced, discovered, and booted with
> an explicit active plan.

### A. Identity + scope (C# — `ReactivePlan`/`PlanIdentity`/`PlanScope`)

| Fixture | Input | Expected |
|---|---|---|
| `plan_id_is_model_full_name` | `Html.ReactivePlan<OrderModel>()` | `PlanId == "…Models.OrderModel"` (the model `FullName`) |
| `create_root_plan_is_root_scope` | `Html.ReactivePlan<M>()` | `Scope.Kind == "root"`, `IsPartial == false`, `RendersValidationSummary == true` |
| `create_partial_plan_is_partial_scope` | `Html.ResolvePlan<M>()` | `Scope.Kind == "partial"`, `IsPartial == true`, `RendersValidationSummary == false` |
| `same_model_root_and_partial_share_plan_id` | root `ReactivePlan<M>` + partial `ResolvePlan<M>` | both `PlanId` equal (so Slot's SSR join can merge them) |
| `plan_string_rejects_empty_id` | `PlanId.Of("")` | `ArgumentException` (value-object authoring boundary; proves null/empty unrepresentable downstream) |

### B. Freeze (C# — `PlanBuildContext.BuildPlan` → `PlanDocument`)

| Fixture | Input | Expected |
|---|---|---|
| `build_plan_is_version_3` | any built plan | `PlanDocument.Version == 3` |
| `build_plan_snapshots_types_components_behaviors` | a context with a declared object + a behavior | `Types`/`Components`/`Behaviors` carry the declared entries; collections are read-only |
| `built_document_carries_root_scope` | root context | `Scope.Kind == "root"` on the document |
| `built_document_carries_partial_scope` | partial context | `Scope.Kind == "partial"` on the document |

### C. Render element + serialization (C# — `RenderPlan` + Kind's `PlanSerializer`)

| Fixture | Input | Expected |
|---|---|---|
| `render_serializes_built_document_camelcase` | `plan.Render()` | compact JSON `{ "version":3, "planId":…, "scope":{"kind":"root"}, "types":…, "components":…, "behaviors":… }` (camelCase) |
| `plan_element_id_sanitizes_dots_and_plus` | `PlanElementId.For("A.B+C")` | `"A-B-C"` (the one shared rule; both framework arms identical) |
| `root_view_renders_script_and_summary` | `RenderPlan` on a root plan | a `<script id="alis-plan-{sanitized}" data-reactive-plan>` **and** a `<div data-reactive-validation-summary="{planId}" hidden>` |
| `partial_renders_script_only` | `RenderPlan` on a partial plan | the `<script>` only — no summary div (`RendersValidationSummary == false`) |
| `plan_document_is_write_only` | the polymorphic converter's `Read` | `NotSupportedException` (plan types are write-only; mechanism owned by Kind) |

### D. Discover (TS — `root.discoverPlans`)

| Fixture | Input | Expected |
|---|---|---|
| `discovers_each_reactive_plan_script` | DOM with two `[data-reactive-plan]` scripts | `discoverPlans()` returns two parsed `PlanDocument`s |
| `empty_plan_script_throws` | a `[data-reactive-plan]` with empty body | throws (`"[alis] empty plan element"`) — the one external-input boundary throw |
| `discover_then_compose_then_boot_each` | DOM with a root script | `startRuntime` composes via `composeInitialPlans` (Slot) and boots the one composed plan once |

### E. Boot with explicit active plan (TS — `boot` / `executeReaction`)

| Fixture | Input | Expected (the invariant the redesign fixes) |
|---|---|---|
| `boot_wires_behaviors_with_explicit_plan` | `boot(plan)` with a behavior | `wireBehavior` receives `plan`; `executeReaction` is called with that `plan`, never a global |
| `boot_registers_plan_with_applied_plans` | `boot(plan)` | `appliedPlans.register(plan)` is called (Slot store), and `markReactiveBooted` records the planId |
| `page_ready_behaviors_wire_after_event_listeners` | a plan with a page-ready + a document-event behavior | the document-event listener is wired before the page-ready behavior executes (two-phase) |
| `execute_requires_explicit_plan` | call `executeReaction(reaction, plan)` | resolves through `RuntimePlan.from(plan)`; there is **no** optional/global overload — a missing plan is a compile error, not a `"no active plan"` runtime throw |
| `runtime_plan_memoized_per_document` | `executeReaction` twice with the same `plan` object | `RuntimePlan.from` returns the same memoized instance (WeakMap, `runtime-plan.ts:13`) — DOM/vendor roots resolved once, not per reaction |
| `reset_boot_state_clears_applied_plans_no_active_plan_global` | `resetBootStateForTests()` | aborts boot wiring + `appliedPlans.reset()`; there is no `activeRuntimePlan` to reset (the global and `resetActivePlanForTests` are deleted) |

### Coverage gate

Every Plan-spine surface is covered: identity/scope (A), freeze to v3 document (B),
serialize + render the script/summary element with the one id rule (C), discover (D),
and boot with an **explicit** active plan (E). The redesign's three named Plan fixes
each map to a fixture: the duplicated id-sanitization → `plan_element_id_sanitizes_dots_and_plus`
+ the shared `PlanElementId` rule; the `activeRuntimePlan` singleton →
`execute_requires_explicit_plan` + `reset_boot_state_clears_applied_plans_no_active_plan_global`;
the per-read `RuntimePlan` rebuild → `runtime_plan_memoized_per_document`. The Band C
**composition** rows (SSR merge, slot load/unload) are proved by **Slot**'s fixtures —
Plan provides the document spine they compose; it does not duplicate them.
