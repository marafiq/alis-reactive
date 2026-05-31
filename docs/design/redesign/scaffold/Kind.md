# Kind — Implementation Spec (kernel)

> **Mechanical-coding contract.** Open this file, read the surface + skeleton +
> fixtures, type the obvious bodies. Every term here is grounded in actual source
> (`Alis.Reactive/Serialization/WriteOnlyPolymorphicConverter.cs`,
> `Alis.Reactive/ReactivePlan.cs` (`ReactivePlanSerializer`),
> `Alis.Reactive/PlanModel/PlanTypeScriptContract.cs`,
> `Alis.Reactive/PlanModel/PlanJsonWriter.cs`, `tools/PlanTypeGenerator/Program.cs`,
> `Alis.Reactive.Assets/runtime/core/assert-never.ts`,
> `Alis.Reactive.Assets/runtime/types/plan.ts`) and named per
> [`03-naming.md`](../03-naming.md) §Kind. No design decision is left open below.

---

## 1. Responsibility, ownership, dependencies

**Responsibility (one sentence).** Kind is the *single* C#↔TS discriminator: every
polymorphic plan node carries one `kind` string written by one compile-enforced
mechanism, one serializer turns the node graph into camelCase plan JSON, and one
reflection-driven generator emits the matching TypeScript contract with a build
gate that fails on drift.

**Owns** (names from `03-naming.md` §Kind / `02-micro-modules.md`):

| Concept (name) | Side | What it is | Replaces |
|---|---|---|---|
| `Kind` (the `kind` property convention) | `→` | The string discriminator each plan node carries so both sides agree which node it is. | scattered `Kind` properties (kept, unified) |
| `PlanNodeDiscriminator` | `→` | The ONE polymorphic `JsonConverter<TBase>` that writes a node's concrete `kind`+properties from a compile-enforced base. | `WriteOnlyPolymorphicConverter<T>` + the 11 hand `JsonConverter`s (collapsed to one) |
| `PlanSerializer` | `→` | The single owner of plan-document → camelCase JSON (compact + formatted). | `ReactivePlanSerializer` / `PlanJsonWriter` (unified, renamed) |
| `PlanContractGenerator` | `→` | Reflects the C# plan node families and writes `runtime/types/plan.ts` from them. | `PlanTypeScriptContract` (1,165-line hand mirror) + `TypeScriptContractWriter` (deleted) |
| `ContractDriftGate` | `→` | Build step: fails if `plan.ts` on disk disagrees with what `PlanContractGenerator` would emit. | — *(new; CLAUDE.md claimed generation existed — it did not)* |
| `assertNever` | `⇒` | Compile-time proof a runtime `switch` handled every `kind`. | `assert-never.ts` / `assertNever` *(kept verbatim)* |

**Depends on:** **Shape** only. (Graph edge `Kind --> Shape`; see
[`00-design.md`](../00-design.md) §2.) Kind reflects/serializes nodes whose values
carry a `Shape`, so the generator emits a `Shape` interface and the serializer
defers to Shape's own converter — it never owns Shape's structure. Kind depends on
**nothing else**; every concept-slice (Value, Condition, Reaction, Request,
Trigger, Component, Slot, Validation, Plugin) and the **Plan** spine depend on
Kind, never the reverse.

**Boundary discipline.** `PlanNodeDiscriminator.Read` and every read path throw
`NotSupportedException("Plan types are write-only.")` — this is the *existing* and
correct stance (`WriteOnlyPolymorphicConverter.cs:16`, `Shape.cs:22`): plans are
serialized in C#, never deserialized. That throw is a true type-boundary, not a
plan validator. `PlanContractGenerator` reflecting an un-`kind`-tagged polymorphic
base is a **build-time generator error** (the contract cannot be expressed), not a
runtime check.

---

## 2. The single idiom this module enforces (read before coding)

Every polymorphic plan node today follows exactly this shape — Kind makes it the
*only* shape. A concept-slice author writes a node like this and gets the `kind`,
the JSON, and the TS contract **for free**:

```csharp
// On the abstract base — ONE attribute, ONE converter type:
[JsonConverter(typeof(PlanNodeDiscriminator<ReactionGraph>))]   // was WriteOnlyPolymorphicConverter<ReactionGraph>
public abstract class ReactionGraph { /* base carries NO colliding members */ }

// On each concrete sealed node — the kind is a compile-visible literal:
public sealed class SetReaction : ReactionGraph
{
    public string Kind => "set";          // discriminator (camelCased? NO — kind values are emitted verbatim)
    public Source On { get; }             // properties serialize as camelCase ("on")
    public string Property { get; }
    public ValueExpression Value { get; }
}
```

Two invariants the discriminator enforces, both already true in source and made
*structural* here:

1. **Every concrete node has a `kind`.** Either a non-virtual `public string Kind =>
   "x"` (e.g. `SetReaction.cs:261`, `StartsWhen.cs:39`) or `public abstract string
   Kind { get; }` + `override` (e.g. `ValueReadAccess.cs:445`,
   `ServerPushEventFilter.cs:121`). `PlanContractGenerator` reads it to emit the TS
   `kind: "x"` literal; `PlanNodeDiscriminator` relies on STJ serializing it as the
   `"kind"` property.
2. **`kind` values are emitted verbatim, NOT camelCased.** The values are already
   kebab/lower tokens (`"page-ready"`, `"show-validation-errors"`, `"array-op"`).
   Only *other* property names are camelCased by `PlanSerializer`'s
   `JsonNamingPolicy.CamelCase`. Because the property is literally named `Kind`,
   camelCase turns the property name into `"kind"` and the value passes through
   untouched — exactly today's behavior.

---

## 3. Public surface — exact C# types + signatures

> Visibility mirrors the codebase: the discriminator type is `public` (it is named
> in a `[JsonConverter(...)]` attribute on `public abstract` bases, so it must be
> reachable — same as `WriteOnlyPolymorphicConverter<T>` is `public` today). The
> serializer and generator are `internal` (consumed only by `ReactivePlan.Render`
> and the `PlanTypeGenerator` host). `ContractDriftGate` is `internal`, exercised
> by a test/build target.

### 3.1 `PlanNodeDiscriminator<T>` — the one polymorphic mechanism

```csharp
namespace Alis.Reactive.Serialization;

/// <summary>
/// Serializes a polymorphic plan node by writing its concrete runtime type's
/// properties, so the node's own <c>Kind</c> property becomes the JSON
/// discriminator. The single discriminator mechanism for every plan node family;
/// reading is unsupported because plans are write-only.
/// </summary>
/// <typeparam name="T">The abstract plan-node base (e.g. <c>ReactionGraph</c>).</typeparam>
public sealed class PlanNodeDiscriminator<T> : JsonConverter<T>
{
    /// <summary>Writes the value as its concrete type, emitting <c>kind</c> plus the concrete properties.</summary>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options);

    /// <summary>Always throws: plan nodes are serialized in C#, never read back.</summary>
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options);
}
```

> **Why one type replaces 12.** Today the base carries
> `WriteOnlyPolymorphicConverter<T>` *and* eleven bespoke converters
> (`ShapeJsonConverter`, `BranchCaseJsonConverter`, `BranchGuardJsonConverter`,
> `DispatchReactionJsonConverter`, `DispatchPayloadJsonConverter`,
> `NativeActionLinkSerializer`, …) hand-write `kind`. Each bespoke converter exists
> only to special-case property emission — work the concrete type's own properties
> already describe. `PlanNodeDiscriminator<T>` keeps the one delegation
> (`Serialize(writer, value, value.GetType(), options)`) and the bespoke converters
> are deleted; any node needing a custom shape exposes properties that serialize to
> that shape, never a converter. **If you find yourself writing a second converter,
> the node's properties are wrong — fix the node, not the serializer.**

### 3.2 `PlanSerializer` — sole plan-JSON owner

```csharp
namespace Alis.Reactive.PlanModel; // or Alis.Reactive.Serialization — colocate with the document

/// <summary>
/// The single owner of plan-document → JSON. Emits camelCase property names; node
/// <c>kind</c> values pass through verbatim. Compact for transport, formatted for
/// debugging.
/// </summary>
internal static class PlanSerializer
{
    /// <summary>Serializes the plan document to compact camelCase JSON for the <c>data-reactive-plan</c> script.</summary>
    internal static string Serialize(PlanDocument plan);

    /// <summary>Serializes the plan document to indented camelCase JSON for debugging.</summary>
    internal static string SerializeFormatted(PlanDocument plan);
}
```

> Carries the two `JsonSerializerOptions` (`Compact`, `Formatted`) that live in
> `ReactivePlanSerializer` today (`ReactivePlan.cs:208-217`):
> `PropertyNamingPolicy = JsonNamingPolicy.CamelCase`, `WriteIndented = true` on the
> formatted one. `ReactivePlan.Render`/`RenderFormatted` call
> `PlanSerializer.Serialize(...)` exactly where they call `ReactivePlanSerializer`
> today. `PlanJsonWriter.WriteProperty<T>` (`PlanJsonWriter.cs`) folds into this
> module as the shared `name + value` write helper if any node still needs it; it
> carries no domain branch and is not a second serializer.

### 3.3 `PlanContractGenerator` — generates `plan.ts` from a curated node description

> **BUILD FINDING (2026-05-31, rejected the reflective sub-goal — feature loss).** The original spec
> below assumed reflection over the C# plan-node families would reproduce `plan.ts`. It will not, and
> implementing it would **drop kinds and lose features**: several plan nodes deliberately narrow ONE C#
> class into MANY TS variants the runtime relies on as exhaustive discriminated unions —
> `CompareCondition`→9 op-narrowed interfaces, `ValidationRule`→8 name-narrowed, `LiteralExpression`→4,
> `ComparisonRightOperand`→7, `ComponentObject`→role×binding×container, plus `Dispatch*` reshaped by bespoke
> converters. The C# domain exposes no reflectable source for these splits (verified consumers:
> `sync-condition.ts`, `rule-engine.ts`, `rule-operands.ts`). **Implemented design:** keep the curated
> generator (renamed `PlanTypeScriptContract`→`PlanContractGenerator`, render primitives + variant list
> retained) — the real value is `ContractDriftGate` (§3.4), which makes C#/TS drift a hard build failure
> regardless of whether the contract is reflected or curated. Reflection here would be the same hand-list
> with added kind-dropping risk. The rename + `PlanSerializer` extraction + `PlanNodeDiscriminator` + drift
> gate all shipped and are gate-green; only the "reflect" verb is rejected.

```csharp
namespace Alis.Reactive.PlanModel;

/// <summary>
/// Reflects the C# plan-node families and renders the TypeScript plan contract
/// (<c>plan.ts</c>) from them: one discriminated union per polymorphic base, one
/// interface per concrete node, the <c>kind</c> literal from each node's
/// <c>Kind</c> property, and camelCase member names matching <see cref="PlanSerializer"/>.
/// Replaces the hand-authored mirror.
/// </summary>
internal static class PlanContractGenerator
{
    /// <summary>Renders the full <c>plan.ts</c> source text from the reflected plan-node families.</summary>
    internal static string Render();
}
```

> **What "reflect" means here, concretely.** Starting from `PlanDocument` (the
> aggregate root), walk the reachable plan-node types:
> - A type with `[JsonConverter(typeof(PlanNodeDiscriminator<TBase>))]` → emit a TS
>   `export type TBase = VariantA | VariantB | …;` union over its concrete
>   subclasses (the same shape `Union(...)` produces today,
>   `PlanTypeScriptContract.cs:34`).
> - Each concrete node → `export interface VariantA { kind: "a"; member: TsType; … }`,
>   reading the `Kind` literal and projecting each public property's CLR type to a
>   TS type (camelCased name), exactly the `Interface(...).Requires(...)` rows the
>   hand mirror lists (`PlanTypeScriptContract.cs:26-906`).
> - `Shape` → emit the `Shape` interface/union once (Kind's only dependency).
> - Enum-token unions (`CompareOp`, `ValidationRuleName`, `ArrayOp`, `PayloadScope`,
>   `HttpMethod`, …) → `LiteralUnion(...)` from the token list value object's
>   `.Values` (today these are the `LiteralUnion("CompareOp", CompareOperator.Values)`
>   rows, `PlanTypeScriptContract.cs:707-729`). The generator reads `.Values`; it
>   does not re-list tokens.
>
> The output text is byte-identical in *form* to today's `plan.ts` header
> (`// <auto-generated />` + generator name + `Run` command) so existing tooling and
> the typecheck pipeline are unchanged. The `TypeScriptWriter`/`TypeScriptInterface`/
> `TypeScriptType`/`TypeScriptContract` render helpers
> (`PlanTypeScriptContract.cs:939-1165`) are **kept** as the rendering primitives;
> only the hand-listed `CreateContract()` body (lines 20-909) is replaced by
> reflection.

### 3.4 `ContractDriftGate` — build gate

```csharp
namespace Alis.Reactive.PlanModel;

/// <summary>
/// Fails when the committed <c>plan.ts</c> disagrees with what
/// <see cref="PlanContractGenerator"/> would emit — proving the generated contract
/// is regenerated whenever the C# plan node families change.
/// </summary>
internal static class ContractDriftGate
{
    /// <summary>
    /// Returns the drift verdict: the generated text vs. the on-disk text. The
    /// build/test that owns this gate fails when <c>HasDrift</c> is true and prints
    /// a unified diff so the fix is "rerun the generator".
    /// </summary>
    internal static ContractDriftResult Check(string committedPlanTsPath);
}

/// <summary>The outcome of a drift check: whether the on-disk contract matches the generator.</summary>
internal readonly struct ContractDriftResult
{
    internal bool HasDrift { get; }
    internal string GeneratedContract { get; }
    internal string CommittedContract { get; }
    /// <summary>A human-readable description of the first divergence, empty when there is no drift.</summary>
    internal string Diff { get; }
}
```

> `ContractDriftResult` is a value object: it is constructed from a comparison and
> exposes the verdict. `HasDrift == false` ⇒ `Diff` is empty by construction (no
> sentinel, no null). The gate is run by a focused C# test or an MSBuild target;
> the host `tools/PlanTypeGenerator/Program.cs` keeps writing the file (its
> `PlanTypeScriptContract.Render()` call becomes `PlanContractGenerator.Render()`).

### 3.5 TS counterpart — `assertNever` (kept verbatim)

```ts
// Alis.Reactive.Assets/runtime/core/assert-never.ts — UNCHANGED
export function assertNever(value: never, context: string): never;
```

> Kind's runtime side is exactly this one function. Every concept-slice runtime
> reader (`executeReaction`, `evaluateValue`, `wireTrigger`, `CompareEngine`, …)
> ends its `kind` switch with `assertNever(node, "reaction")` so an un-handled
> generated `kind` is a **compile** error in the slice, not a runtime branch. Kind
> owns the function; the slices own their switches.

---

## 4. Input → Output contract

| | Flows in | Produces | Invariants |
|---|---|---|---|
| **`PlanNodeDiscriminator<T>.Write`** | a polymorphic plan node instance (concrete subtype of `T`) + `Utf8JsonWriter` + options | the node's concrete-type JSON object, including its `kind` and camelCased members | Delegates to `JsonSerializer.Serialize(writer, value, value.GetType(), options)`. The value is never null *by construction* (a node reference in the document graph is always a concrete instance — null nodes are unrepresentable in the plan domain; do **not** add a null guard). |
| **`PlanSerializer.Serialize`** | an immutable `PlanDocument` | one camelCase JSON string | Property names camelCased; `kind` values verbatim; `version: 3` carried as-is. Deterministic: same document ⇒ same bytes. |
| **`PlanContractGenerator.Render`** | (none — reflects the loaded plan-node assembly) | the full `plan.ts` source text | Exactly one TS union per polymorphic base, one interface per concrete node, one `LiteralUnion` per token value object's `.Values`, one `Shape` declaration. Output is stable/ordered so the drift gate is deterministic. |
| **`ContractDriftGate.Check`** | the on-disk `plan.ts` path | a `ContractDriftResult` | `HasDrift` true iff generated text ≠ committed text (normalized line endings, the `TypeScriptWriter.ToString()` convention). `Diff` empty when `!HasDrift`. |

**Null is unrepresentable by construction, not guarded.** The discriminator never
sees a null node because the document graph holds concrete instances; the generator
never sees a `kind`-less polymorphic base because that is a build-time generator
error (it cannot emit a discriminant), surfaced when the generator runs, not as a
defensive runtime branch. `ContractDriftResult.Diff` is empty (`""`) on no-drift —
the empty string *is* the "no divergence" value, not a sentinel for a missing one.

---

## 5. File layout

| File | Action | Contents |
|---|---|---|
| `Alis.Reactive/Serialization/PlanNodeDiscriminator.cs` | **rename + collapse** of `WriteOnlyPolymorphicConverter.cs` | `PlanNodeDiscriminator<T>` (§3.1). Update every `[JsonConverter(typeof(WriteOnlyPolymorphicConverter<X>))]` attribute to `PlanNodeDiscriminator<X>`. |
| `Alis.Reactive/PlanModel/PlanSerializer.cs` | **new** (extract from `ReactivePlan.cs`) | `PlanSerializer` (§3.2). Move the `ReactivePlanSerializer` static + its two `JsonSerializerOptions` out of `ReactivePlan.cs`; fold in `PlanJsonWriter.cs` as the shared write helper if still used. |
| `Alis.Reactive/PlanModel/PlanContractGenerator.cs` | **rewrite** of `PlanTypeScriptContract.cs` | `PlanContractGenerator.Render()` (§3.3) — reflection body replacing `CreateContract()`. **Keep** the `TypeScriptContract`/`TypeScriptWriter`/`TypeScriptInterface`/`TypeScriptType`/`TypeScriptProperty`/`TypeScriptDeclaration`/`TypeScriptTypeAlias` render primitives in this file (lines 939-1165). |
| `Alis.Reactive/PlanModel/ContractDriftGate.cs` | **new** | `ContractDriftGate` + `ContractDriftResult` (§3.4). |
| `tools/PlanTypeGenerator/Program.cs` | **one-line edit** | `PlanTypeScriptContract.Render()` → `PlanContractGenerator.Render()`. |
| `Alis.Reactive/ReactivePlan.cs` | **edit** | `Render()`/`RenderFormatted()` call `PlanSerializer.Serialize`/`SerializeFormatted`; delete the inlined `ReactivePlanSerializer`. |
| `Alis.Reactive.Assets/runtime/core/assert-never.ts` | **unchanged** | `assertNever` (§3.5). |
| `Alis.Reactive.Assets/runtime/types/plan.ts` | **generated output** | regenerated by `PlanContractGenerator`; never hand-edited (kept gitignored-vs-committed per the project's existing convention — the drift gate enforces whichever it is). |
| **Deleted** | — | `Alis.Reactive.Native/Components/NativeActionLink/NativeActionLinkSerializer.cs` and the 11 bespoke `*JsonConverter` types inside `Shape.cs`/`ReactionGraph.cs`/`ValueExpression.cs` **only if** their node properties already serialize correctly through `PlanNodeDiscriminator` (verify per converter; `ShapeJsonConverter` may stay because Shape owns its own structure — that is Shape's call, not Kind's). |

> **Scope fence with Shape.** `ShapeJsonConverter` (`Shape.cs:11-23`) belongs to the
> **Shape** module, not Kind. Kind's generator emits the `Shape` *type* and Kind's
> serializer *defers* to whatever converter Shape declares. Do not delete or rewrite
> `ShapeJsonConverter` in this module — that is the Shape spec's decision.

---

## 6. Compile-ready skeleton

### `Alis.Reactive/Serialization/PlanNodeDiscriminator.cs`

```csharp
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Serialization
{
    /// <summary>
    /// Serializes a polymorphic plan node by writing its concrete runtime type's
    /// properties, so the node's own <c>Kind</c> property becomes the JSON
    /// discriminator. The single discriminator mechanism for every plan node family.
    /// </summary>
    public sealed class PlanNodeDiscriminator<T> : JsonConverter<T>
    {
        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            // TODO: delegate to the concrete type so its `Kind` + properties emit.
            //   JsonSerializer.Serialize(writer, value, value!.GetType(), options);
            // Fixture: discriminator_writes_concrete_kind
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            // TODO: plans are write-only — throw NotSupportedException("Plan types are write-only.").
            // Fixture: discriminator_read_is_unsupported
            => throw new NotImplementedException();
    }
}
```

### `Alis.Reactive/PlanModel/PlanSerializer.cs`

```csharp
using System.Text.Json;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// The single owner of plan-document → JSON: camelCase property names, verbatim
    /// node <c>kind</c> values. Compact for transport, formatted for debugging.
    /// </summary>
    internal static class PlanSerializer
    {
        private static readonly JsonSerializerOptions Compact = new JsonSerializerOptions
        {
            // TODO: PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            // Fixture: serializer_emits_camelcase_members
        };

        private static readonly JsonSerializerOptions Formatted = new JsonSerializerOptions
        {
            // TODO: CamelCase + WriteIndented = true
            // Fixture: serializer_formatted_is_indented
        };

        /// <summary>Serializes the plan document to compact camelCase JSON.</summary>
        internal static string Serialize(PlanDocument plan)
            // TODO: JsonSerializer.Serialize(plan, Compact)
            // Fixtures: serializer_emits_camelcase_members, serializer_emits_kind_verbatim
            => throw new System.NotImplementedException();

        /// <summary>Serializes the plan document to indented camelCase JSON for debugging.</summary>
        internal static string SerializeFormatted(PlanDocument plan)
            // TODO: JsonSerializer.Serialize(plan, Formatted)
            // Fixture: serializer_formatted_is_indented
            => throw new System.NotImplementedException();
    }
}
```

### `Alis.Reactive/PlanModel/PlanContractGenerator.cs`

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Reflects the C# plan-node families and renders the TypeScript plan contract
    /// (<c>plan.ts</c>): one union per polymorphic base, one interface per concrete
    /// node, the <c>kind</c> literal from each node's <c>Kind</c> property, camelCase
    /// members matching <see cref="PlanSerializer"/>, and a <c>LiteralUnion</c> per
    /// token value object's <c>.Values</c>.
    /// </summary>
    internal static class PlanContractGenerator
    {
        private const string Generator = "Alis.Reactive.PlanModel.PlanContractGenerator";
        private const string Command = "npm run generate:plan-types -w Alis.Reactive.Assets";

        /// <summary>Renders the full <c>plan.ts</c> source text from the reflected node families.</summary>
        internal static string Render()
        {
            var contract = TypeScriptContract.GeneratedBy(Generator, Command);

            // TODO: walk the plan-node assembly from PlanDocument outward and Declare:
            //   1. PlanDocument interface (version: 3, planId, scope, types, components, behaviors)
            //      Fixture: generator_emits_plan_document_root
            //   2. For each [PlanNodeDiscriminator<TBase>] base: a union over its concrete subtypes.
            //      Fixture: generator_union_per_polymorphic_base
            //   3. For each concrete node: an interface { kind: "<literal>"; <camelCase members> }.
            //      Fixtures: generator_kind_literal_from_kind_property, generator_camelcase_members
            //   4. The Shape interface/union exactly once (Kind's only dependency).
            //      Fixture: generator_emits_shape_once
            //   5. For each token value object: LiteralUnion(name, valueObject.Values).
            //      Fixture: generator_literal_union_from_values
            // Use the kept render helpers: TypeScriptContract / Interface(...) / Union(...) /
            // LiteralUnion(...) / Alias(...). Output header must stay "// <auto-generated />".

            return contract.Render();
        }

        // --- kept render helpers (UNCHANGED from PlanTypeScriptContract.cs:911-1165) ---
        // TypeScriptContract, TypeScriptWriter, TypeScriptInterface, TypeScriptProperty,
        // TypeScriptType, TypeScriptDeclaration, TypeScriptTypeAlias.
        // Plus the private factories: Interface, ComponentVariant, Alias, Union,
        // LiteralUnion, Literal. Copy them verbatim; only CreateContract()'s body is replaced.
    }
}
```

### `Alis.Reactive/PlanModel/ContractDriftGate.cs`

```csharp
using System.IO;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Fails when the committed <c>plan.ts</c> disagrees with what
    /// <see cref="PlanContractGenerator"/> would emit.
    /// </summary>
    internal static class ContractDriftGate
    {
        /// <summary>Compares the generator output against the on-disk contract.</summary>
        internal static ContractDriftResult Check(string committedPlanTsPath)
        {
            // TODO: generated = PlanContractGenerator.Render();
            //       committed = File.ReadAllText(committedPlanTsPath) (normalize line endings);
            //       hasDrift = generated != committed; build the result (+ first-divergence diff).
            // Fixtures: drift_gate_passes_when_in_sync, drift_gate_fails_on_renamed_member
            throw new System.NotImplementedException();
        }
    }

    /// <summary>The outcome of a drift check.</summary>
    internal readonly struct ContractDriftResult
    {
        internal bool HasDrift { get; }
        internal string GeneratedContract { get; }
        internal string CommittedContract { get; }
        /// <summary>First divergence; empty when there is no drift (the empty string IS "no divergence").</summary>
        internal string Diff { get; }

        internal ContractDriftResult(string generated, string committed, string diff)
        {
            GeneratedContract = generated;
            CommittedContract = committed;
            Diff = diff;
            HasDrift = diff.Length != 0; // TODO: confirm the invariant — Diff empty iff in sync.
        }
    }
}
```

### `tools/PlanTypeGenerator/Program.cs` (the one-line edit)

```csharp
// File.WriteAllText(fullPath, PlanTypeScriptContract.Render());
File.WriteAllText(fullPath, PlanContractGenerator.Render());   // TODO: swap the call
```

---

## 7. Acceptance fixtures — the matrix cases this module must satisfy

Kind is a **kernel**: it has no authoring row of its own in the matrix. Its
correctness is *the cross-cutting fact every band's "Kind" column asserts*. The
fixtures below are named after the determinism each matrix band states Kind must
guarantee. Each maps to a `// Fixture:` marker in the skeleton.

### From `04-matrix-validation-components-slots.md` §"The kernels every row leans on → Kind"

That row states verbatim: *"every plan node carries one `kind` string written by
`PlanNodeDiscriminator` → `PlanSerializer` (camelCase). `PlanContractGenerator`
reflects the node families into `plan.ts`; `ContractDriftGate` fails the build on
drift; `assertNever` ⇒ proves the runtime switch is exhaustive."* That single
sentence is Kind's entire acceptance surface — decomposed into these fixtures:

| Fixture (test name) | Asserts | Grounded in |
|---|---|---|
| `discriminator_writes_concrete_kind` | A `ReactionGraph` reference whose runtime type is `SetReaction` serializes with `"kind":"set"` and the set's members. | `ReactionGraph.cs:261`; `WriteOnlyPolymorphicConverter.cs:12` |
| `discriminator_read_is_unsupported` | `Read` throws `NotSupportedException` ("Plan types are write-only."). | `WriteOnlyPolymorphicConverter.cs:16` |
| `serializer_emits_camelcase_members` | A node's `On`/`PayloadType`/`HubUrl` members emit as `on`/`payloadType`/`hubUrl`. | `ReactivePlan.cs:210`; `plan.ts` `hubUrl` |
| `serializer_emits_kind_verbatim` | Multi-word `kind` tokens emit **un-camelCased**: `"page-ready"`, `"show-validation-errors"`, `"array-op"`. | `StartsWhen.cs:39`; `ReactionGraph.cs:448`; `ValueExpression.cs:542` |
| `serializer_formatted_is_indented` | `SerializeFormatted` produces indented JSON; `Serialize` is compact. | `ReactivePlan.cs:213-220` |
| `generator_emits_plan_document_root` | `plan.ts` declares `PlanDocument { version: 3; planId; scope; types; components; behaviors }`. | `plan.ts:5-12`; `PlanTypeScriptContract.cs:26-32` |
| `generator_union_per_polymorphic_base` | Each `[PlanNodeDiscriminator<TBase>]` base ⇒ one TS union over its concrete subtypes (e.g. `ReactionGraph`, `StartsWhen`, `ValueExpression`, `ServerPushEventFilter`). | `PlanTypeScriptContract.cs:34,59,82,109` |
| `generator_kind_literal_from_kind_property` | Each concrete interface's `kind` literal equals the C# node's `Kind` value (`SetReaction` ⇒ `kind: "set"`). | `ReactionGraph.cs:261`; `plan.ts` literal pattern |
| `generator_camelcase_members` | Generated interface member names match `PlanSerializer`'s camelCase (one source of truth — no member name drifts between serializer and contract). | the determinism note in §Kind |
| `generator_emits_shape_once` | `plan.ts` declares `Shape` exactly once; Kind's only dependency is emitted, not duplicated. | `00-design.md` §2 graph `Kind --> Shape` |
| `generator_literal_union_from_values` | Token unions (`CompareOp`, `ValidationRuleName`, `ArrayOp`, `PayloadScope`, `HttpMethod`) are emitted from each value object's `.Values`, not hand-listed. | `PlanTypeScriptContract.cs:707,149,666,290,436` |
| `drift_gate_passes_when_in_sync` | `ContractDriftGate.Check` returns `HasDrift == false`, `Diff == ""` when `plan.ts` matches the generator. | new mechanism (`05-determinism-proof.md` §"mechanism work" #4) |
| `drift_gate_fails_on_renamed_member` | Renaming a C# node property (e.g. `SetReaction.Property` → `Member`) makes `Check` return `HasDrift == true` with a non-empty `Diff`. | the exact drift class Kind exists to kill (`02-micro-modules.md` §Kind) |

### Cross-band guarantee Kind underwrites (no separate fixture — proven by the slices)

`05-determinism-proof.md` §"How to Add a Feature" Step 3 ("Contract") states the
developer does **nothing by hand**: `PlanContractGenerator` reflects a new node into
`plan.ts` and `ContractDriftGate` fails the build if they forget. The
`generator_*` + `drift_gate_*` fixtures above *are* that guarantee. Step 4
("Runtime") routes on the carried `kind` with `assertNever` — covered by each
slice's own runtime fixtures (Reaction's `executeReaction`, Value's `evaluateValue`,
etc.), not duplicated here; Kind only ships the `assertNever` function unchanged.

### Determinism wins this module locks in (from `02-micro-modules.md` §Kind)

- **12 → 1 converter.** `discriminator_*` + the §5 deletions prove
  `PlanNodeDiscriminator<T>` is the *only* converter authored in this module.
- **1,165-line hand mirror → reflection.** `generator_*` prove `plan.ts` is derived,
  not hand-maintained; the file is regenerated, never edited.
- **No silent drift.** `drift_gate_*` prove a renamed C# member can no longer
  disagree with the runtime — the single largest correctness fix in the redesign.

---

## 8. Pre-flight (mechanical-coding checklist)

- [ ] `PlanNodeDiscriminator<T>` written; every `WriteOnlyPolymorphicConverter<X>`
      attribute reference updated (grep `WriteOnlyPolymorphicConverter` → 0 hits).
- [ ] `PlanSerializer` extracted; `ReactivePlan.Render*` call it; old
      `ReactivePlanSerializer` deleted.
- [ ] `PlanContractGenerator.Render()` reflects the families; render helpers kept;
      `tools/PlanTypeGenerator/Program.cs` swapped.
- [ ] `npm run generate:plan-types` regenerates `plan.ts`; `npm run typecheck` green
      (the generated contract still compiles every runtime switch).
- [ ] `ContractDriftGate` + its two fixtures pass; gate wired into the build/test.
- [ ] All §7 fixtures pass; bespoke `*JsonConverter`s deleted only where their node
      properties serialize correctly through the one discriminator (Shape's own
      converter left to the Shape spec).
- [ ] `git status` clean apart from the intended files; no hand-edit of `plan.ts`.
