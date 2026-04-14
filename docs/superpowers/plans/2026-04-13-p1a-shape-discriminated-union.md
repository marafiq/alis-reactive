# P1a — Shape Discriminated Union (End-to-End)

**Date:** 2026-04-13
**Branch:** `pre/1.0-cleanup` from `origin/release/1.0.0-preview1` @ `df88f154`
**Worktree:** `.codex-worktrees/pre-1.0-cleanup`
**Slice:** First of three under P1 (sentinel cleanup). Subsequent slices: P1b NoOpReaction, P1c locked nullable table.
**Spec:** `docs/superpowers/specs/2026-04-13-pre-1.0-cleanup.md` § P1 Tier 1
**Baseline:** 523 unit tests passing (141+19+78+109+93+54+29), 0 failures, `npm run typecheck` clean, 1095 NRT warnings on `dotnet build` (mostly sandbox views, pre-existing).

**Project compile constraints (verified Round 2):**
- `Alis.Reactive/Alis.Reactive.csproj:4` — `<TargetFrameworks>net48;net10.0</TargetFrameworks>`
- `Alis.Reactive/Alis.Reactive.csproj:5` — `<LangVersion>8</LangVersion>`
- `Alis.Reactive/Alis.Reactive.csproj:6` — `<Nullable>enable</Nullable>`

These constraints bind every line of proposed code. `System.HashCode` does not exist on net48 — it is .NET Standard 2.1+ only. Every `HashCode.*` call must be wrapped in `#if NET6_0_OR_GREATER` with a manual unchecked-hash fallback for net48. Every `is X x` is C# 7. `is not` is C# 9 — forbidden. `init`, `with`, records, target-typed `new()` — all C# 9+ — forbidden.

## Revision history

### Round 3 — 2026-04-13 (this revision)

Round 2 plan reviewed by both reviewers. Architect produced BLOCK with 2 BLOCKs + 7 concerns. File verifier produced BLOCK with 22 verified, 1 FAILED, 2 PARTIAL. Reconciled findings below — every BLOCK addressed, every concern folded in.

| # | Finding (source, severity) | Root-cause fix applied |
|---|---|---|
| R14 | **HashCode net48 BLOCK** — proposed `Shape.cs` uses `HashCode.Combine` and `new HashCode()` unconditionally on three new `GetHashCode` methods. `Alis.Reactive.csproj:4` multi-targets `net48;net10.0`. `System.HashCode` is .NET Standard 2.1+ only — net48 build will fail with CS0103 (architect Part 2(a), verifier C4 FAILED) | Every `HashCode.*` call in the proposed code is wrapped in `#if NET6_0_OR_GREATER ... #else <manual unchecked hash> ... #endif`, mirroring the existing `Shape.cs:170-186` pattern. The false claim that "HashCode is unconditionally available — codebase targets .NET 10 per CLAUDE.md" is removed from the plan; the actual constraint is documented at the top under "Project compile constraints". |
| R15 | **Test #28 contradicts R4 BLOCK** — proposed Test #28 constructs a fresh `OpaqueShape("none")` and asserts `ArrayOf` and `Nullable` throw, but the R4 fix uses `ReferenceEquals(item, None)` which is FALSE for any fresh allocation. The test would fail because no exception is thrown (architect Part 1 R4) | **Adopted architect Option (b): promote `Shape.None` to its own `internal sealed class NoneShape`.** Type-based factory guards (`if (item is NoneShape)`) replace reference-identity guards. The singleton invariant becomes a type-system guarantee — no matter how many `NoneShape` instances exist, the factory rejects all of them. `IsNone => this is NoneShape` (no `ReferenceEquals` trick). Test #28 rewritten to construct a freshly-allocated `NoneShape` from the test project (via IVT) and assert the type-based guard trips. The new test PROVES the type guard works rather than relying on identity. |
| R16 | **OpaqueShape bundles three semantically distinct concepts (raw/any/none) — concern verging on BLOCK** (architect Part 3 finding 2 + 3, verifier observation) | After R15, `OpaqueShape` shrinks to `{raw, any}` only — both genuine "untyped/wildcard" leaves that share `IsScalar=false` coherently. `none` is its own `NoneShape`. The R6 partition is now: `ScalarShape{string,number,boolean,date}` + `OpaqueShape{raw,any}` + `NoneShape{none}` + 3 composite. Each subclass represents a coherent concept. |
| R17 | **`TrueScalarShape` is awkward — naming concern** (architect Part 3 finding 1) | Renamed `TrueScalarShape` → `ScalarShape` everywhere in the plan. The old "ScalarShape" name from Round 1 is gone in the same commit, so no name collision. |
| R18 | **`StartsWhen` precedent prose is wrong** — `StartsWhen.cs:8` declares `internal abstract class`, not `public abstract` as plan claimed. Both base AND subclasses are internal there. Only `RequestInput` (`Request.cs:54`) is a true `public abstract` base with `internal sealed` subclasses (architect Part 2(k), verifier R5 PARTIAL / F1) | `StartsWhen` reference removed from §"Polymorphic serialization — proven precedent" because its `internal abstract` base is not the same shape as P1a's `public abstract` Shape. Only `RequestInput`/`GatherInput` cited as the true precedent. The drop-the-smoke-test decision still holds — one precedent is sufficient. Plan prose corrected. |
| R19 | **`type.GetElementType()` drops null-forgiving operator silently — would generate a new CS8603 warning** (architect Part 2(g)) | Restored `type.GetElementType()!` to match the original Shape.cs:91 and avoid introducing a new NRT warning under `<Nullable>enable</Nullable>`. |
| R20 | **`ObjectShape.GetHashCode` allocates per call via `OrderBy(...)`** — CONCERN, allocation cost on hot path if Shape ever becomes a Dictionary key (architect Part 2(f)) | Replaced `OrderBy(...)` with order-independent XOR over the key hash codes: `int keyHash = 0; foreach (var k in Fields.Keys) keyHash ^= StringComparer.Ordinal.GetHashCode(k);` — O(N), allocation-free, order-independent, equivalent collision profile for the keys-only strategy. |
| R21 | **`<remarks>` documenting `EqualsSameType` cast-unconditionally contract for future subclass authors** (architect Part 2(h)) | Added `<remarks>` XML doc on `protected abstract bool EqualsSameType(Shape other)` declaring the contract: "Implementations MUST cast `other` to their own type unconditionally and MUST NOT add their own type checks. The base `Equals(Shape)` guarantees `other` is a non-null instance of the same runtime type before invoking this method." |
| R22 | **`JsType.cs:74-77` (`a == Shape.Any \|\| a == Shape.None`) is unchanged but never explicitly justified** (architect Part 2(c)) | Plan now documents that lines 74-77 of `JsType.cs` are intentionally untouched. They remain correct because `==` operator on Shape now delegates to the new structural `Equals(Shape)`, which compares `OpaqueShape("any")` to `OpaqueShape("any")` via `GetType()` + `EqualsSameType` — same result as today. |
| R23 | **Wire format empty-ObjectShape change should be surfaced as Playwright awareness** — concern, downgraded to CONCERN by file verifier sweep (no tests assert on the old shape) (architect Part 2(i)) | Plan now states explicitly under §"Playwright" that no existing tests assert on `{"kind":"object"}` without `fields`, verified by file verifier sweep S8. The change is wire-format-widening only (new always-present `fields`); no consumer reads the dropped `additional`. |
| R24 | **Abstract `Shape` base class constructor visibility unspecified** — defaults to `protected` (allowing potential external derivation attempts). `RequestInput.cs:56` precedent uses `private protected`. Architect Round 3 minor concern. | Added explicit `private protected Shape() { }` constructor. Matches `RequestInput.cs:56`. Prevents external derivation attempts entirely (external types cannot call `private protected` ctors). |
| R25 | **Quality gate ripgrep regex typo** — `\|` is a literal pipe in ripgrep's PCRE2 mode, not alternation. The grep checks would look for the wrong text. Architect Round 3 minor concern. | Fixed the regex to use `|` (alternation) for the `Shape.Additional` quality gate row. |

Round 3 ready for re-dispatch to both reviewers.

### Round 2 — 2026-04-13

13 findings (R1-R13) addressed from Round 1 review. Plan revised to:
- Fix C# 9 syntax → C# 8 throughout
- Kill `Additional` dimension entirely (not patch it)
- Drop `additionalProperties:false` from schema change (left for separate slice)
- Use `ReferenceEquals(item, None)` factory guards + new singleton invariant test
- Drop STJ smoke test (precedent existed, but wrong precedent cited — fixed in R18)
- Split `ScalarShape` into `TrueScalarShape` + `OpaqueShape` (further refined in R16/R17)
- Hoisted `EqualsSameType` same-type guard to base
- `ObjectShape.GetHashCode` walks sorted field keys (refined in R20 to allocation-free XOR)
- `IsNone` via `ReferenceEquals(this, None)` (further strengthened in R15 via `NoneShape` type)
- Quality gate: added `npm run build:all`, bundle size, `git diff --stat`
- Implementation order: added IVT pre-check as Step 1
- TS comment fix: "9 kinds" → "10 kinds"

### Round 1 — 2026-04-13

Initial plan written.

---

## Goal

Replace `Alis.Reactive.PlanModel.Shape` (single sealed class with three nullable shape-children properties + a now-dead `Additional` dimension) with a discriminated union of 6 concrete subtypes that make illegal shape states unrepresentable by construction. End-to-end: C# domain → JSON wire format → JSON Schema → TS types → TS runtime. Eliminate the dead `shape.item ?` guards in `shape-convert.ts`, the dead `Shape.OpenObject()` factory, and the `Additional` dimension that has zero readers and (after `OpenObject` deletion) zero producers.

## Why pre-1.0

After 1.0 ships, every one of the following becomes a binary-breaking change requiring a major version bump:

- Removing `Shape.Item`, `Shape.Inner`, `Shape.Fields`, or `Shape.Additional` from the public property surface
- Removing the `additional` field from the JSON wire format
- Demoting subclass types from public to internal
- Replacing nullable shape children with non-nullable subclass-only properties

P1a does all four. Today it costs a slice. Post-1.0 it costs a major version bump. The window closes at 1.0.

## Non-goals (explicit)

- **No DSL public API change.** Zero signature changes in `Builders/`, `Html` extension classes, or anywhere a consumer touches. Internal `Shape.*` factory signatures (`Shape.String`, `Shape.ArrayOf(item)`, `Shape.Nullable(inner)`, `Shape.ObjectOf(dict)`, `Shape.FromClrType(type)`) stay identical — same names, same parameters, same `Shape` return type.
- **No NoOpReaction work.** That is P1b.
- **No locked nullable table commit.** That is P1c.
- **No public API surface audit.** That is P2.
- **No new builder methods.**
- **No P3 wire-format-freeze tests.** P3 introduces snapshot infrastructure separately.
- **No tightening of `additionalProperties: false` on shape `$def`s.** Round 1 BLOCK 3 — left for a separate slice.
- **No drop of net48 target.** Multi-targeting stays. P1a adapts to net48 via `#if` guards.

## DSL public contract — what stays frozen

These are the consumer-facing surfaces that touch Shape transitively. **None of them change.**

| Surface | Why it touches Shape | Frozen guarantee |
|---|---|---|
| `Html.InputField(plan, m => m.X)` | Generates ValueProducer with Shape from `typeof(X)` | Same signature, same return type |
| Component extensions (`.NativeTextBox`, `.FusionDropDownList`, etc.) — 65 files use `Shape.*` factories | Build component contracts that carry shapes | Internal call sites unchanged |
| `p.Get/Post/Put/Delete().Gather().Include(m => m.X)` | Builds GatherInput where each field has a Shape | Internal builder calls unchanged |
| `p.When(m => m.X).EqualTo(...)` | Builds Condition with shape-typed comparisons | Internal builder calls unchanged |
| `Shape.String / Number / Boolean / Date / Raw / Any / None` | Static fields, internal | Identity preserved (same singleton instance returned) |
| `Shape.FromClrType(Type)` | Maps CLR types to shapes | Same signature, same nullable/array/scalar dispatch |

Anything outside this list is internal implementation detail.

## Polymorphic serialization — proven precedent (R18 corrected)

Round 1 architect review flagged `internal sealed class` polymorphic subclasses as untested precedent. Round 1 file verifier disproved this. Round 2 file verifier corrected my prose:

- `Alis.Reactive/PlanModel/Request.cs:53-77` — **the true precedent.** `RequestInput` is `[JsonConverter(typeof(WriteOnlyPolymorphicConverter<RequestInput>))]` on a `public abstract class`, with `internal sealed class GatherInput` as one of its concrete subclasses. This is exactly the shape P1a proposes for `Shape`: public abstract base, internal sealed subclasses, polymorphic serialization via the converter. The 523 baseline tests cover `RequestInput` paths, proving STJ correctly serializes internal sealed subclasses through the converter.
- `Alis.Reactive/PlanModel/StartsWhen.cs:7-17` is NOT a precedent for the same pattern — both the base and its 5 subclasses are `internal`. It exercises STJ on internal types but not the public-abstract-base + internal-sealed-subclass combination. Cited only as supporting evidence that STJ tolerates `internal sealed` polymorphic types in general.
- `Alis.Reactive/Serialization/WriteOnlyPolymorphicConverter.cs:11-12` — `Write` calls `JsonSerializer.Serialize(writer, value, value.GetType(), options)`. STJ 9 (the version pinned at `Alis.Reactive.csproj` package references) reflects on internal types via reflection; the internal modifier affects callers, not STJ's reflection emit. The skip-same-converter protection in STJ prevents infinite recursion.

P1a follows the `RequestInput` pattern. No smoke test or special handling needed.

## End-to-end pipeline trace

### BEFORE

```
Layer 1  C# domain model
         Shape (public sealed class)
           Kind: string
           Item?: Shape       ← nullable, [JsonIgnore(WhenWritingNull)]
           Inner?: Shape      ← nullable, [JsonIgnore(WhenWritingNull)]
           Fields?: IRO<…>    ← nullable, [JsonIgnore(WhenWritingNull)]
           Additional: bool   ← [JsonIgnore(WhenWritingDefault)]
           IsScalar: switch on Kind string
           Equals: walks all nullables
         Internal factories: String/Number/Boolean/Date/Raw/Any/None/ArrayOf/ObjectOf/OpenObject/Nullable/FromClrType
         OpenObject() is the ONLY producer of Additional=true (and has zero callers — dead)

Layer 2  JSON wire format
         {"kind":"string"} … {"kind":"none"}                       ← scalar leaves
         {"kind":"array","item":{...}}                             ← Item written when set
         {"kind":"nullable","inner":{...}}                         ← Inner written when set
         {"kind":"object","fields":{"a":{...}}}                    ← Fields written when set
         {"kind":"object","additional":true}                       ← unreachable (OpenObject dead)

Layer 2  JSON Schema (Alis.Reactive/Schemas/reactive-plan.schema.json:604-680)
         Shape: oneOf [10 variants — already a DU at the wire level]

Layer 3  TS types (Alis.Reactive.SandboxApp/Scripts/types/plan.ts:380-432)
         Shape = StringShape | … | ArrayShape{item} | NullableShape{inner} | ObjectShape{fields?,additional?} | …

Layer 3  TS runtime
         shape-convert.ts:52  `return shape.item ? r.value.map(...) : r.value`  ← DEAD GUARD
         shape-convert.ts:81  `return shape.item ? ok(r.value.map(...)) : r`     ← DEAD GUARD

Layer 1  C# call sites of Shape's nullable fields outside Shape.cs
         JsType.cs:79  if (a.Kind == "nullable" && a.Inner == b) return a;   ← string-Kind narrowing
         JsType.cs:80  if (b.Kind == "nullable" && b.Inner == a) return b;
         JsType.cs:74-77  Untouched — still uses Shape.Any / Shape.None reference comparisons via ==
         (no other consumers; all other grep hits verified false positives)
```

### AFTER

```
Layer 1  C# domain model
         Shape (public abstract class, [JsonConverter(WriteOnlyPolymorphicConverter<Shape>)])
           Kind: abstract string
           IsScalar: internal abstract bool
           IsNone: internal => this is NoneShape       ← R10/R15: type-enforced, no ReferenceEquals
           Equals(Shape): public, non-virtual on base; checks GetType() == other.GetType() then EqualsSameType
           EqualsSameType: protected abstract — subclasses cast unconditionally (R7 + R21 contract)
           GetHashCode: abstract override

         6 internal sealed subclasses:
           ScalarShape : Shape       (R17 renamed from TrueScalarShape)
             IsScalar => true,  Kind passed in ctor, kinds: string|number|boolean|date
             4 singleton instances: String, Number, Boolean, Date

           OpaqueShape : Shape       (R16 narrowed to {raw, any})
             IsScalar => false, Kind passed in ctor, kinds: raw|any
             2 singleton instances: Raw, Any

           NoneShape : Shape         (R15 promoted from OpaqueShape)
             IsScalar => false, Kind => "none" (constant override, no field)
             1 singleton instance: None
             EqualsSameType => true (singleton semantics)

           ArrayShape : Shape
             IsScalar => false, Kind => "array", Item: Shape (NON-NULL)

           NullableShape : Shape
             IsScalar => Inner.IsScalar, Kind => "nullable", Inner: Shape (NON-NULL)

           ObjectShape : Shape
             IsScalar => false, Kind => "object", Fields: IReadOnlyDictionary<string, Shape> (NON-NULL — empty dict allowed)
             *** No Additional property — dimension killed entirely (R2) ***

         Internal factories (signatures unchanged):
           Shape.String/Number/Boolean/Date    → ScalarShape singletons
           Shape.Raw/Any                       → OpaqueShape singletons
           Shape.None                          → NoneShape singleton
           Shape.ArrayOf(item)                 → new ArrayShape(item)  (validates `if (item is NoneShape)` — R15 type-based)
           Shape.ObjectOf(fields)              → new ObjectShape(fields)
           Shape.Nullable(inner)               → new NullableShape(inner)  (validates `if (inner is NoneShape)` — R15 type-based)
           Shape.FromClrType(type)             → unchanged dispatch
           [REMOVED] Shape.OpenObject()        ← dead code, zero callers
           [REMOVED] Shape.Additional          ← dead dimension

Layer 2  JSON wire format (changes)
         {"kind":"string"} … {"kind":"none"}             ← unchanged
         {"kind":"array","item":{...}}                   ← unchanged
         {"kind":"nullable","inner":{...}}               ← unchanged
         {"kind":"object","fields":{}}                   ← WIDENED: fields always written
         {"kind":"object","fields":{"a":{...}}}          ← typical closed object
         (No new variants. No removed variants. ObjectShape gains one always-present field — fields. The Additional field is gone entirely.)

Layer 2  JSON Schema (changes)
         ObjectShape required: ["kind"]                      → ["kind", "fields"]
         ObjectShape properties: kind, fields, additional    → kind, fields  (additional REMOVED)
         (No new $defs. No removed $defs. No oneOf changes. additionalProperties:false NOT added.)

Layer 3  TS types (changes)
         ObjectShape { kind: "object"; fields: Record<string, Shape> }  ← drop ?, drop additional entirely
         Comment fix: "discriminated union — 9 kinds" → "10 kinds" (R13)

Layer 3  TS runtime (changes)
         shape-convert.ts:52  `return r.value.map(v => applyShape(v, shape.item));`        ← drop dead guard
         shape-convert.ts:81  `return ok(r.value.map(v => applyShape(v, shape.item)));`    ← drop dead guard

Layer 1  C# consumer fix
         JsType.cs:79  if (a is NullableShape an && an.Inner.Equals(b)) return a;
         JsType.cs:80  if (b is NullableShape bn && bn.Inner.Equals(a)) return b;
         JsType.cs:74-77  UNCHANGED. The == comparisons against Shape.Any / Shape.None still work because
                         Shape.operator== now delegates to Equals(Shape), which compares OpaqueShape("any")
                         to OpaqueShape("any") via GetType()+EqualsSameType — same result as today, plus
                         NoneShape compared to NoneShape via GetType()+EqualsSameType (returns true).
                         Documented per R22.
```

## File-by-file changes

In dependency order.

### 1. `Alis.Reactive/PlanModel/Shape.cs` — abstract base + 6 internal sealed subclasses (C# 8, net48-compatible)

**Before:** single `public sealed class Shape` (193 lines).

**After:** 7 classes (1 abstract base + 6 internal sealed subclasses).

**Visibility:**
- `Shape` (abstract base) — `public`. Surface = `Kind`, `Equals(Shape)`, `Equals(object)`, `GetHashCode()`, `operator==`, `operator!=`, `IEquatable<Shape>`.
- `ScalarShape`, `OpaqueShape`, `NoneShape`, `ArrayShape`, `NullableShape`, `ObjectShape` — **all `internal sealed class`**.

**Proposed code (C# 8 verified — no `is not`, no records, no init, no with, no target-typed `new()`; HashCode wrapped for net48):**

```csharp
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Declares the expected type for a value in the plan (string, number, date, array, object, etc.).
    /// Construct shapes through framework builders — <c>Shape</c> instances are produced by
    /// <c>Html.InputField</c>, gather/condition builders, and validator extraction. Consumers do
    /// not pattern-match on shapes; the framework owns shape semantics end-to-end.
    /// </summary>
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<Shape>))]
    public abstract class Shape : IEquatable<Shape>
    {
        // ── singleton instances (identity preserved from previous design) ──
        internal static readonly Shape String  = new ScalarShape("string");
        internal static readonly Shape Number  = new ScalarShape("number");
        internal static readonly Shape Boolean = new ScalarShape("boolean");
        internal static readonly Shape Date    = new ScalarShape("date");
        internal static readonly Shape Raw     = new OpaqueShape("raw");
        internal static readonly Shape Any     = new OpaqueShape("any");
        internal static readonly Shape None    = new NoneShape();

        // ── factories (signatures unchanged from previous design) ──
        internal static Shape ArrayOf(Shape item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            // R15: type-based guard. Rejects ANY NoneShape instance, not just the singleton.
            if (item is NoneShape)
                throw new ArgumentException("Array item shape is required.", nameof(item));
            return new ArrayShape(item);
        }

        internal static Shape ObjectOf(Dictionary<string, Shape> fields)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            return new ObjectShape(new ReadOnlyDictionary<string, Shape>(fields));
        }

        internal static Shape Nullable(Shape inner)
        {
            if (inner == null) throw new ArgumentNullException(nameof(inner));
            // R15: type-based guard.
            if (inner is NoneShape)
                throw new ArgumentException("Nullable inner shape is required.", nameof(inner));
            return new NullableShape(inner);
        }

        internal static Shape FromClrType(Type type)
        {
            if (type == null) return Any;

            var underlying = System.Nullable.GetUnderlyingType(type);
            if (underlying != null) return Nullable(FromClrType(underlying));

            if (type == typeof(string))           return String;
            if (type == typeof(bool))             return Boolean;
            if (IsDateType(type))                 return Date;
            if (IsNumericType(type))              return Number;
            if (IsStringSerializedType(type))     return String;
            if (type.IsEnum)                      return String;

            var elementType = GetCollectionElementType(type);
            if (elementType != null) return ArrayOf(FromClrType(elementType));

            return Any;
        }

        private static bool IsDateType(Type type)
            => type == typeof(DateTime) || type == typeof(DateTimeOffset)
#if NET6_0_OR_GREATER
               || type == typeof(DateOnly)
#endif
               ;

        private static bool IsNumericType(Type type)
            => type == typeof(byte) || type == typeof(sbyte) ||
               type == typeof(short) || type == typeof(ushort) ||
               type == typeof(int) || type == typeof(uint) ||
               type == typeof(long) || type == typeof(ulong) ||
               type == typeof(float) || type == typeof(double) || type == typeof(decimal);

        private static bool IsStringSerializedType(Type type)
            => type == typeof(Guid) || type == typeof(TimeSpan)
#if NET6_0_OR_GREATER
               || type == typeof(TimeOnly)
#endif
               ;

        // R19: restored ! to match Shape.cs:91 — avoids a new CS8603 under <Nullable>enable</Nullable>.
        private static Type? GetCollectionElementType(Type type)
        {
            if (type.IsArray) return type.GetElementType()!;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return type.GetGenericArguments()[0];
            return null;
        }

        /// <summary>Gets the shape kind (string, number, boolean, date, array, object, nullable, raw, any, or none).</summary>
        public abstract string Kind { get; }

        internal abstract bool IsScalar { get; }

        // R10/R15: type-enforced. NoneShape is the only class for which this returns true.
        // No ReferenceEquals trick. No singleton-uniqueness invariant.
        internal bool IsNone => this is NoneShape;

        // R24: explicit private protected ctor matches RequestInput.cs:56 precedent.
        // Prevents external assemblies from attempting `class MyShape : Shape` derivation
        // (they cannot satisfy IsScalar/EqualsSameType/GetHashCode anyway, but the explicit
        // private protected makes the intent typed and produces a clearer compile error).
        private protected Shape() { }

        // R7: hoisted same-type guard. Subclasses cast unconditionally in EqualsSameType.
        public bool Equals(Shape? other)
            => !(other is null) && other.GetType() == GetType() && EqualsSameType(other);

        /// <summary>
        /// Same-type structural equality. The base <see cref="Equals(Shape?)"/> guarantees
        /// <paramref name="other"/> is a non-null instance of the same runtime type before invoking this method.
        /// </summary>
        /// <remarks>
        /// R21 contract: implementations MUST cast <paramref name="other"/> to their own type
        /// unconditionally and MUST NOT add their own type checks. Adding an `is X` check here
        /// is dead code that will mask logic errors. The base contract is: same GetType() OR
        /// this method is not called.
        /// </remarks>
        protected abstract bool EqualsSameType(Shape other);

        public override bool Equals(object? obj) => obj is Shape s && Equals(s);
        public abstract override int GetHashCode();

        public static bool operator ==(Shape? left, Shape? right)
            => ReferenceEquals(left, right) || (!(left is null) && left.Equals(right));
        public static bool operator !=(Shape? left, Shape? right) => !(left == right);
    }

    /// <summary>A leaf shape representing a primitive value (string, number, boolean, or date).</summary>
    internal sealed class ScalarShape : Shape
    {
        public override string Kind { get; }
        internal override bool IsScalar => true;
        internal ScalarShape(string kind) { Kind = kind; }
        protected override bool EqualsSameType(Shape other) => Kind == ((ScalarShape)other).Kind;

#if NET6_0_OR_GREATER
        public override int GetHashCode() => Kind.GetHashCode();
#else
        public override int GetHashCode() { unchecked { return Kind.GetHashCode(); } }
#endif
    }

    /// <summary>A leaf shape representing a non-scalar untyped value: raw JSON or untyped any.</summary>
    internal sealed class OpaqueShape : Shape
    {
        public override string Kind { get; }
        internal override bool IsScalar => false;
        internal OpaqueShape(string kind) { Kind = kind; }
        protected override bool EqualsSameType(Shape other) => Kind == ((OpaqueShape)other).Kind;

#if NET6_0_OR_GREATER
        public override int GetHashCode() => Kind.GetHashCode();
#else
        public override int GetHashCode() { unchecked { return Kind.GetHashCode(); } }
#endif
    }

    /// <summary>The absence of a shape. Singleton; type-enforced uniqueness via factory guards.</summary>
    internal sealed class NoneShape : Shape
    {
        public override string Kind => "none";
        internal override bool IsScalar => false;
        internal NoneShape() { }

        // Singleton semantics: any two NoneShape instances are structurally equal.
        protected override bool EqualsSameType(Shape other) => true;

#if NET6_0_OR_GREATER
        public override int GetHashCode() => "none".GetHashCode();
#else
        public override int GetHashCode() { unchecked { return "none".GetHashCode(); } }
#endif
    }

    /// <summary>A shape describing an array whose elements all match <see cref="Item"/>.</summary>
    internal sealed class ArrayShape : Shape
    {
        public override string Kind => "array";
        internal override bool IsScalar => false;
        public Shape Item { get; }

        internal ArrayShape(Shape item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            Item = item;
        }

        protected override bool EqualsSameType(Shape other) => Item.Equals(((ArrayShape)other).Item);

#if NET6_0_OR_GREATER
        public override int GetHashCode() => HashCode.Combine("array", Item);
#else
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + "array".GetHashCode();
                hash = hash * 31 + Item.GetHashCode();
                return hash;
            }
        }
#endif
    }

    /// <summary>A shape that wraps another shape to express that the value may be null.</summary>
    internal sealed class NullableShape : Shape
    {
        public override string Kind => "nullable";
        internal override bool IsScalar => Inner.IsScalar;
        public Shape Inner { get; }

        internal NullableShape(Shape inner)
        {
            if (inner == null) throw new ArgumentNullException(nameof(inner));
            Inner = inner;
        }

        protected override bool EqualsSameType(Shape other) => Inner.Equals(((NullableShape)other).Inner);

#if NET6_0_OR_GREATER
        public override int GetHashCode() => HashCode.Combine("nullable", Inner);
#else
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + "nullable".GetHashCode();
                hash = hash * 31 + Inner.GetHashCode();
                return hash;
            }
        }
#endif
    }

    /// <summary>A shape describing an object with named field shapes.</summary>
    internal sealed class ObjectShape : Shape
    {
        public override string Kind => "object";
        internal override bool IsScalar => false;
        public IReadOnlyDictionary<string, Shape> Fields { get; }

        internal ObjectShape(IReadOnlyDictionary<string, Shape> fields)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            Fields = fields;
        }

        protected override bool EqualsSameType(Shape other)
        {
            var o = (ObjectShape)other;
            if (Fields.Count != o.Fields.Count) return false;
            foreach (var kvp in Fields)
            {
                if (!o.Fields.TryGetValue(kvp.Key, out var v) || !kvp.Value.Equals(v))
                    return false;
            }
            return true;
        }

        // R8 + R20: combine "object", Count, and an order-independent XOR over key hashes.
        // O(N), allocation-free, order-independent. Field-shape hash codes intentionally NOT included
        // — would force every nested shape to evaluate its hash on every parent hash. Keys alone
        // distinguish almost all real shapes. The Equals contract is still respected (Equals-equal
        // implies Hash-equal): two ObjectShapes with identical key sets and identical field shapes
        // hash identically and Equals-equal.
        public override int GetHashCode()
        {
            int keyHash = 0;
            foreach (var key in Fields.Keys)
                keyHash ^= StringComparer.Ordinal.GetHashCode(key);
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + "object".GetHashCode();
                hash = hash * 31 + Fields.Count;
                hash = hash * 31 + keyHash;
                return hash;
            }
        }
    }
}
```

**Notes:**
- **C# 8 verified.** No `is not`. Every `is X x` is C# 7. `!(x is null)` is C# 8.
- **net48 compatible.** All `HashCode.*` calls wrapped in `#if NET6_0_OR_GREATER` with manual unchecked-hash fallback (R14).
- **`OpenObject()` deleted.** Zero callers — confirmed Round 1.
- **`Additional` deleted entirely** (R2).
- **`NoneShape` is its own class** (R15). Type-enforced singleton invariant.
- **`IsNone => this is NoneShape`** (R10/R15). Pattern-based, type-enforced.
- **Factory `None` guards use `is NoneShape`** (R15). Robust against any caller construction.
- **`EqualsSameType` is the hoisted same-type pattern** (R7) with `<remarks>` documenting the contract (R21).
- **`ObjectShape.GetHashCode` uses XOR over key hashes** (R20). O(N), allocation-free, order-independent.
- **`type.GetElementType()!`** (R19). Restored to avoid new CS8603 under `<Nullable>enable</Nullable>`.
- **Nullable annotations on Equals/Equals(object)/operator** match the codebase's `<Nullable>enable</Nullable>` pragma. `Shape?` parameters and `obj is Shape s` narrowing are C# 8 valid.

### 2. `Alis.Reactive/PlanModel/JsType.cs` lines 79-80 — pattern match

**Before:**

```csharp
if (a.Kind == "nullable" && a.Inner == b) return a;
if (b.Kind == "nullable" && b.Inner == a) return b;
```

**After:**

```csharp
if (a is NullableShape an && an.Inner.Equals(b)) return a;
if (b is NullableShape bn && bn.Inner.Equals(a)) return b;
```

This is the only consumer of `Shape.Inner` outside `Shape.cs` itself — verified Round 1.

**Lines 74-77 of `JsType.cs` are intentionally untouched** (R22). They read:

```csharp
if (a == b) return a;
if (a == Shape.Any || a == Shape.None) return b;
if (b == Shape.Any || b == Shape.None) return a;
```

These remain correct under the new Shape contract:
- `a == b` uses the new `operator==` which delegates to `Equals(Shape)` — `GetType() == GetType() && EqualsSameType` — same result for any two same-typed shapes.
- `a == Shape.Any` compares `OpaqueShape("any")` to `OpaqueShape("any")` via `GetType()+EqualsSameType` (Kind compared) — same result as today.
- `a == Shape.None` compares `NoneShape` to `NoneShape` via `GetType()+EqualsSameType` (returns true unconditionally) — same result as today.

No changes needed to lines 74-77.

### 3. `Alis.Reactive/Schemas/reactive-plan.schema.json` — minimal ObjectShape change

**Before** (lines 651-662):

```json
"ObjectShape": {
  "type": "object",
  "required": ["kind"],
  "properties": {
    "kind": { "const": "object" },
    "fields": {
      "type": "object",
      "additionalProperties": { "$ref": "#/$defs/Shape" }
    },
    "additional": { "type": "boolean" }
  }
}
```

**After:**

```json
"ObjectShape": {
  "type": "object",
  "required": ["kind", "fields"],
  "properties": {
    "kind": { "const": "object" },
    "fields": {
      "type": "object",
      "additionalProperties": { "$ref": "#/$defs/Shape" }
    }
  }
}
```

Two changes:
- `required` gains `fields`
- `additional` REMOVED from `properties` entirely (R2)

**Not added:** `additionalProperties: false` (R3).

### 4. `Alis.Reactive.SandboxApp/Scripts/types/plan.ts` — drop ObjectShape optionals, fix stale comment

**Before** (lines 378, 417-421):

```ts
// ── Shape (discriminated union — 9 kinds) ─────────────────────
...
export interface ObjectShape {
  kind: "object";
  fields?: Record<string, Shape>;
  additional?: boolean;
}
```

**After:**

```ts
// ── Shape (discriminated union — 10 kinds) ────────────────────
...
export interface ObjectShape {
  kind: "object";
  fields: Record<string, Shape>;
}
```

Three changes (R13 + R2):
- Comment fixed from "9 kinds" to "10 kinds"
- `fields` becomes required
- `additional` removed entirely

### 5. `Alis.Reactive.SandboxApp/Scripts/core/shape-convert.ts` lines 49-53 and 78-82 — remove dead guards

**Before** (lines 49-53):

```ts
function applyArrayShape(value: unknown, shape: Extract<Shape, { kind: "array" }>): unknown {
  const r = toArray(value);
  if (!r.ok) return value;
  return shape.item ? r.value.map(v => applyShape(v, shape.item)) : r.value;
}
```

**After:**

```ts
function applyArrayShape(value: unknown, shape: Extract<Shape, { kind: "array" }>): unknown {
  const r = toArray(value);
  if (!r.ok) return value;
  return r.value.map(v => applyShape(v, shape.item));
}
```

**Before** (lines 78-82):

```ts
function convertArrayShape(value: unknown, shape: Extract<Shape, { kind: "array" }>): ConvertResult<unknown> {
  const r = toArray(value);
  if (!r.ok) return r;
  return shape.item ? ok(r.value.map(v => applyShape(v, shape.item))) : r;
}
```

**After:**

```ts
function convertArrayShape(value: unknown, shape: Extract<Shape, { kind: "array" }>): ConvertResult<unknown> {
  const r = toArray(value);
  if (!r.ok) return r;
  return ok(r.value.map(v => applyShape(v, shape.item)));
}
```

No other runtime changes. Verified Round 1: `core/wire-format.ts:11` and `execution/gather.ts:115` are properly narrowed.

## Test plan

### New tests — `tests/Alis.Reactive.UnitTests/Shapes/WhenBuildingShapes.cs` (29 tests)

| # | Test name | What it proves |
|---|---|---|
| 1 | `ScalarShape_string_renders_kind_only` | `{"kind":"string"}` |
| 2 | `ScalarShape_number_renders_kind_only` | `{"kind":"number"}` |
| 3 | `ScalarShape_boolean_renders_kind_only` | `{"kind":"boolean"}` |
| 4 | `ScalarShape_date_renders_kind_only` | `{"kind":"date"}` |
| 5 | `OpaqueShape_raw_renders_kind_only` | `{"kind":"raw"}` |
| 6 | `OpaqueShape_any_renders_kind_only` | `{"kind":"any"}` |
| 7 | `NoneShape_renders_kind_only` | `{"kind":"none"}` |
| 8 | `ArrayShape_renders_kind_and_item` | `{"kind":"array","item":{"kind":"string"}}` |
| 9 | `ArrayShape_throws_when_item_is_null` | `ArgumentNullException` |
| 10 | `ArrayShape_throws_when_item_is_NoneShape` | `ArgumentException` (R15 type-based guard) |
| 11 | `NullableShape_renders_kind_and_inner` | `{"kind":"nullable","inner":{"kind":"date"}}` |
| 12 | `NullableShape_throws_when_inner_is_null` | `ArgumentNullException` |
| 13 | `NullableShape_throws_when_inner_is_NoneShape` | `ArgumentException` (R15 type-based guard) |
| 14 | `NullableShape_IsScalar_delegates_to_inner` | nullable-of-string is scalar |
| 15 | `NullableShape_of_array_is_not_scalar` | nullable-of-array is not scalar |
| 16 | `NullableShape_of_nullable_recurses_correctly` | nesting permitted, IsScalar bottoms out |
| 17 | `ObjectShape_empty_renders_with_empty_fields` | `{"kind":"object","fields":{}}` |
| 18 | `ObjectShape_with_named_fields` | `{"kind":"object","fields":{"name":{"kind":"string"}}}` |
| 19 | `ObjectShape_throws_when_fields_is_null` | `ArgumentNullException` |
| 20 | `ObjectShape_no_additional_field_in_json` | The JSON output of any ObjectShape contains NO `"additional"` key (R2) |
| 21 | `Shape_FromClrType_string_returns_singleton_String` | `ReferenceEquals(Shape.String, FromClrType(typeof(string)))` |
| 22 | `Shape_FromClrType_int_returns_singleton_Number` | `ReferenceEquals(Shape.Number, FromClrType(typeof(int)))` |
| 23 | `Shape_FromClrType_nullable_int_wraps_in_NullableShape` | `is NullableShape n && ReferenceEquals(n.Inner, Shape.Number)` |
| 24 | `Shape_FromClrType_string_array_wraps_in_ArrayShape` | `is ArrayShape a && ReferenceEquals(a.Item, Shape.String)` |
| 25 | `Shape_equality_is_structural_for_arrays` | two `ArrayShape(String)` compare equal |
| 26 | `Shape_equality_distinguishes_array_items` | `ArrayShape(String) != ArrayShape(Number)` |
| 27 | `Shape_equality_distinguishes_subclass_kinds` | `String != Raw`, `Array(String) != String`, etc. |
| 28 | `Constructing_a_freshly_allocated_NoneShape_does_not_pass_factory_guards` (R15 rewritten) | Construct `var fresh = new NoneShape();` from the test project (IVT). Assert `Shape.ArrayOf(fresh)` and `Shape.Nullable(fresh)` BOTH throw `ArgumentException`. **Proves the factory guard is type-based, not identity-based** — multiple NoneShape instances all trip the guard. |
| 29 | `NoneShape_singleton_compares_equal_to_freshly_constructed_NoneShape` | Singleton semantics: `Shape.None.Equals(new NoneShape())` returns true. Demonstrates the singleton invariant is type-enforced, not identity-enforced. |

29 tests. Each render-style test calls a builder that reaches Shape (e.g., `ValueProducer.LiteralRaw(value, shape)`) and `AssertSchemaValid(planJson)` validates against the schema.

### Existing tests — must still pass

All 523 existing tests must pass without modification.

### Playwright

Run the full Playwright suite locally. Empty `ObjectShape` JSON now widens from `{"kind":"object"}` to `{"kind":"object","fields":{}}`. **Verified Round 2 sweep S8: no existing test asserts on the old `{"kind":"object"}` form, and no test asserts on `"additional"` being present or absent in plan JSON.** The change is wire-format-widening only.

## Quality gate — measured BEFORE commit

| Metric | Baseline | After P1a | Pass criterion |
|---|---:|---:|---|
| `dotnet test` total passing | 523 | 523 + 29 = **552** | All pass, 0 failures, 0 skipped |
| `dotnet build` warning count | 1095 | ≤ 1095 | Refactor must not raise warnings |
| `dotnet build Alis.Reactive.slnx` exit code | 0 | 0 | C# 8 + net48 + net10.0 verified |
| **`dotnet build` for `net48` TFM specifically** (R14) | 0 | 0 | Confirms `HashCode` `#if` guards work |
| `npm run typecheck` | 0 errors | 0 errors | Clean |
| `npm run lint` | (run baseline) | ≤ baseline | No new ESLint warnings |
| `npm run build:all` exit code | 0 | 0 | esbuild bundle build clean |
| `wwwroot/js/*.js` total bundle size delta | (record baseline) | ≤ baseline + 0 | No regression — guards being removed should slightly shrink |
| `git diff --stat` filenames touched | n/a | exactly the expected file list | No accidental edits to unrelated files |
| `rg "OpenObject" --type cs` | 1 (Shape.cs) | **0** | Dead factory removed |
| `rg "Shape\.Additional|ObjectShape\.Additional|Additional\s*=" --type cs Alis.Reactive` (R25 fix: alternation, not literal pipe) | 1 (Shape.cs) | **0** | Dead dimension removed |
| `rg "\"additional\":" Alis.Reactive.SandboxApp/wwwroot/js/` | (some hits possible from old build) | **0** in plan.js after rebuild | TS/runtime no longer carries `additional` |
| `rg "additional" Alis.Reactive/Schemas/reactive-plan.schema.json` | 2 (in ObjectShape) | **0** | Schema stripped of `additional` |
| `rg -n "shape\.item ?" Alis.Reactive.SandboxApp/Scripts/` | 2 | **0** | Dead guards removed |
| `rg -n "is not " Alis.Reactive/PlanModel/Shape.cs Alis.Reactive/PlanModel/JsType.cs` | 0 | **0** | C# 9 `is not` did not sneak in |
| `rg -n "HashCode\." Alis.Reactive/PlanModel/Shape.cs` (R14) | 1 (in `#if`) | All hits inside `#if NET6_0_OR_GREATER` blocks | net48 fallback present for every `HashCode.*` call |
| `rg -n "\.Item\b\|\.Inner\b\|\.Fields\b\|\.Additional\b" Alis.Reactive/PlanModel/Shape.cs` | (4 properties on base) | properties exist only on subclasses | Properties moved off the base |
| `grep -n "public sealed class \(Scalar\|Opaque\|None\|Array\|Nullable\|Object\)Shape" Alis.Reactive/PlanModel/Shape.cs` | n/a | **0** | All 6 subclasses must be `internal sealed` |
| `grep -n "internal sealed class \(Scalar\|Opaque\|None\|Array\|Nullable\|Object\)Shape" Alis.Reactive/PlanModel/Shape.cs` | n/a | **6** | Exactly 6 internal sealed subclasses |
| `grep -n "TrueScalarShape" Alis.Reactive/PlanModel/Shape.cs` (R17) | n/a | **0** | Renamed to ScalarShape |
| Playwright pass count | (run baseline) | == baseline | No regressions |
| **R11 IVT pre-check** | n/a | 0 hits | `rg "ScalarShape\|OpaqueShape\|NoneShape\|ArrayShape\|NullableShape\|ObjectShape" tests/Alis.Reactive.DesignSystem.Tests tests/Alis.Reactive.NativeTagHelpers.Tests tests/Alis.Reactive.Analyzers.Tests Alis.Reactive.PlaywrightTests Alis.Reactive.Playwright.Extensions` must be 0 |

Any row that fails its criterion blocks the commit.

## Review prompts

Round 3 ready for re-dispatch to both reviewers. Each prompt cites the Round 2 verdict, the 10 fixes in this revision (R14-R23), and asks the reviewer to verify each fix against the actual revised text and attack any new issues introduced.

After implementation: `pr-review-toolkit:review-pr` against the diff.

## Rollback

Single-concept slice. `git revert` the single commit. Plan and Spec files stay; only code reverts.

## Implementation order (after Round 3 SIGN-OFF)

1. **R11 IVT pre-check.** Run `rg "ScalarShape\|OpaqueShape\|NoneShape\|ArrayShape\|NullableShape\|ObjectShape" tests/Alis.Reactive.DesignSystem.Tests tests/Alis.Reactive.NativeTagHelpers.Tests tests/Alis.Reactive.Analyzers.Tests Alis.Reactive.PlaywrightTests Alis.Reactive.Playwright.Extensions`. Must be 0 hits. If not, STOP.
2. Update `Alis.Reactive/PlanModel/Shape.cs` — abstract + 6 internal sealed subclasses including `NoneShape`, dead factory removed, `Additional` dimension removed, C# 8 syntax, `#if NET6_0_OR_GREATER` guards on all `HashCode.*` calls, hoisted `EqualsSameType`, type-enforced `IsNone`.
3. Update `Alis.Reactive/PlanModel/JsType.cs:79-80` — pattern-match. Lines 74-77 stay untouched.
4. Update `Alis.Reactive/Schemas/reactive-plan.schema.json:651-662` — minimal change (require `fields`, drop `additional`).
5. Run `dotnet build Alis.Reactive.slnx` — must compile cleanly on BOTH net48 and net10.0 TFMs, no new warnings.
6. Add `tests/Alis.Reactive.UnitTests/Shapes/WhenBuildingShapes.cs` — 29 tests including R15 type-based guard test.
7. Run `dotnet test` — all 552 must pass.
8. Update `Alis.Reactive.SandboxApp/Scripts/types/plan.ts:378,417-421` — fix comment, drop `?`, drop `additional`.
9. Update `Alis.Reactive.SandboxApp/Scripts/core/shape-convert.ts:52,81` — remove dead guards.
10. Run `npm run typecheck && npm run lint` — clean.
11. Run `npm run build:all` — bundle builds, record bundle size delta.
12. Run full Playwright suite locally — pass count matches baseline.
13. Run all quality-gate checks in the table — every row must match its criterion.
14. `git diff --stat` audit — only the expected files appear.
15. Dispatch `pr-review-toolkit:review-pr` against the diff.
16. Address findings, re-run, re-dispatch until clean SIGN-OFF.
17. Commit with evidence (Round 3 review verdicts + post-impl review verdict + before/after metrics) in the message.
