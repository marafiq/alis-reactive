# Shape kernel — spec-only dogfood proof

This is a **dogfood test of the redesign blueprint's "super-mechanical" promise**: can the
`Shape` micro-module be implemented and proven from its spec + fixtures **alone**, with no
access to the current source?

## How it was built

Built using **only** these two inputs:

- `docs/design/redesign/scaffold/Shape.md` — the module spec (surface, contract, skeleton, §6 fixtures)
- the Shape rows in `docs/design/redesign/scaffold/_fixtures.md` (Module 1, `F-Shape-*`)

**No file under `Alis.Reactive/**` was read.** No other module spec, no matrix, no design doc,
no existing tests. The whole point is to prove the spec + fixtures are sufficient on their own.

## What's here

| File | Contents |
|---|---|
| `Shape.cs` | `Shape` value object, `ShapeStructure` (+ 5 private sealed subclasses), `ShapeObjectContract`, `ShapeJsonConverter` — the authoring side (spec §2a–c, §5a) |
| `ShapeContractCompatibility.cs` | the merge/accept algebra (spec §2d, §5b) |
| `ShapeConvert.cs` | the runtime conversion engine ported to C#: `ShapeConverter.ApplyShape`/`ConvertByShape`, the scalar coercions, `RuntimeShapeTag` (the generated union the engine switches on), and `RuntimeShape` + `FormatForWire` (spec §2e–f, §5c–d) |
| `Program.cs` | every §6 A/B/C/D/E fixture + the extra `F-Shape-*` behavior rows, encoded as runnable assertions; exits non-zero on any failure |

The TS-defined runtime engine (`applyShape`/`convertByShape`/`formatForWire`) is ported to C#
so the whole proof is a single buildable/runnable **.NET 10** artifact, as the task requires.
`RuntimeShapeTag` is parsed straight from the C# `Shape`'s serialized JSON, so the runtime side
reads the exact bytes the domain emits — the spec's "same bytes everywhere" / shape-once intent.

## Run it

```bash
cd docs/design/redesign/dogfood/shape
dotnet run            # prints PASS/FAIL per fixture, exits 0 only if all green
```

Expected tail: `Fixtures: 54/54 passed.` / `ALL GREEN` / exit 0.

This project is **deliberately NOT added to any `.slnx`**. Empty local `Directory.Build.props`
and `Directory.Build.targets` stop MSBuild from inheriting the repo-root build, keeping the proof
self-contained.

## Fixtures encoded: 54

- **A. CLR inference** (12): `clr_string_is_string`, `clr_int_is_number`, `clr_bool_is_boolean`,
  `clr_datetime_is_date`, `clr_nullable_int_is_nullable_number`, `clr_guid_is_string`,
  `clr_enum_is_string`, `clr_list_of_t_is_array_of_t`, `clr_dictionary_is_any`,
  `clr_unknown_is_any`, `from_value_null_is_none`, `collection_item_shape_or_none_for_non_collection`
- **B. Construction invariants** (4): `array_of_none_is_rejected`, `nullable_of_none_is_rejected`,
  `nullable_scalar_is_scalar`, `object_is_not_scalar`
- **C. Serialization** (7): `scalar_serializes_kind_only`, `array_serializes_item`,
  `nullable_serializes_inner`, `object_of_fields_serializes_closed`,
  `open_object_serializes_additional_true`, `read_is_not_supported`, `describe_contract_nested`
- **D. Equality + algebra** (16): `equal_array_shapes_are_equal`, `different_object_fields_are_unequal`,
  `merge_equal_is_self`, `merge_any_yields_other`, `merge_none_conflicts`,
  `merge_nullable_absorbs_inner`, `merge_arrays_recurse`, `merge_objects_union_fields`,
  `merge_field_conflict_is_conflict`, `accept_any_either_side`, `reject_none_either_side`,
  `accept_open_object`, `reject_missing_required_field`, `accept_equal_self`,
  `accept_array_recurse`, `accept_nullable_either_side`
- **E. Runtime conversion** (13): `apply_string_coerces_number`, `apply_number_parses_text`,
  `apply_boolean_truthy_text`, `apply_date_only_is_local_midnight`, `apply_array_recurses_items`,
  `apply_object_keeps_open_extras`, `apply_nullable_missing_is_null`, `apply_raw_is_identity`,
  `convert_object_into_scalar_is_err`, `format_date_to_iso`, `format_nullable_unwraps`,
  `format_unshaped_passthrough`, `runtime_shape_item_of_array`
- **F-Shape (Module 1 behavior)** (2): `F-Shape-Once_value_is_shaped_exactly_once`,
  `F-Shape-Number_non_finite_text_is_err`

## What had to be invented (the spec gaps — the real finding)

These are decisions the spec + fixtures did **not** fully pin, so I had to choose. Each is a
candidate spec gap to close back in `Shape.md`/`_fixtures.md`.

1. **Exact numeric CLR set.** Spec says "covers all CLR numeric types" / "int/byte/long/decimal/double…"
   but never enumerates it. I chose `byte, sbyte, short, ushort, int, uint, long, ulong, float,
   double, decimal, nint, nuint`. Whether `nint`/`nuint`/unsigned types count is a guess.
2. **"Supported collection" boundary rule.** Fixtures give two endpoints (`List<T>`/`int[]`/
   `IEnumerable<string>` → array; `Dictionary<,>` → any) but no general rule. I inferred:
   *implements `IEnumerable<T>` (element = generic arg) or is an array, AND is not `string`, AND is
   not `IDictionary`/`IDictionary<,>`* → array; non-generic `IEnumerable` (not string/dict) → `array<any>`.
   The treatment of non-generic `IEnumerable`, and whether `string` is excluded by special-case vs
   by being a scalar, are inventions.
3. **`toNumber` parse semantics.** Spec says "missing→0; non-finite→err" only. I chose JS-`Number()`-like
   behavior: empty/whitespace string → 0, `bool` → err (a boolean is not a number), unparseable/non-finite
   string → err. The bool-rejection and the empty-string-is-0 rule are inventions.
4. **`toBoolean` truthy rule for arbitrary strings.** Spec lists the falsy set (`"" / "false" / "0" /
   0 / NaN → false`). For every *other* string I return `true`. That "everything else is true" closure
   is invented (e.g. `"no"`/`"off"` are treated as `true`).
5. **`toDate` non-date-only parsing + epoch basis.** Spec pins only "date-only `YYYY-MM-DD` → LOCAL
   midnight ms". For full datetime strings I used `DateTimeOffset.Parse` → `ToUnixTimeMilliseconds`,
   and treated an incoming number as already-epoch-ms. The full-datetime path and the number-passthrough
   are inventions.
6. **`formatForWire` date ISO format string.** Spec says "ISO-8601 string" / round-trip-safe `"O"`.
   I emit UTC `yyyy-MM-ddTHH:mm:ss.fffZ`. The exact precision (ms vs ticks) and UTC-vs-local choice
   are invented; the fixture `format_date_to_iso` only said "ISO-8601 string" so I picked a concrete one.
7. **`merge`/`accept` object recursion details beyond the named rows.** The fixtures cover closed+closed
   union, field conflict, open+open, open-accepts-any-object, and missing-required-field. I had to invent:
   (a) the `additional` flag on a closed+closed *union* result (kept `false`), (b) `accept` field
   direction = *every expected-declared field must exist & be acceptable in actual* (extra actual fields
   ignored), and (c) closed+open / open+closed *merge* behavior (falls through field-union; only
   open+open with zero fields short-circuits to `OpenObject`). None of these mixed cases has a fixture.
8. **Runtime value model for ports.** Since the engine is ported to C#, I had to choose host
   representations: JS object ≙ `Dictionary<string,object?>`, JS array ≙ `object?[]`, JS number ≙
   `double`, date epoch ≙ `double` ms. This is a porting artifact, not a domain decision, but it is a
   choice the spec (being TS-only for the runtime) does not make for a C# proof.
9. **`ConvertResult<T>` shape in C#.** The spec's TS `ConvertResult` is `{ok:true,value} | {ok:false,error}`.
   I modeled it as a readonly struct with `Ok`/`Value`/`Error`. Trivial, but a representation choice.

### Not a gap (resolved cleanly from spec)

- Null-unrepresentable, `None` sentinel, the two `ArgumentException`s, write-only converter, `kind`-first
  JSON body, structural equality, `IsScalar` (incl. `Nullable<scalar>`), `DescribeContract` format,
  `merge_*`/`accept_*` named rows, all `applyShape` switch arms, `formatForWire` nullable-unwrap and
  unshaped-passthrough, `RuntimeShape.item()`/`isDeclared` — all were directly typeable from the spec
  surface + the §6 fixture expected values. The skeleton's `// TODO` markers mapped 1:1 to fixtures.

## Verdict

The spec + fixtures were **sufficient to build a compiling, fully-passing implementation** of the
Shape kernel without reading any source. Every gap above is a coercion-engine *detail* (numeric set,
collection boundary, scalar-parse edge semantics, ISO format, mixed object merge/accept) — i.e. the
spec is strong on **structure and the authoring/algebra contract** and looser on the **runtime
coercion micro-rules**, where the prose ("non-finite→err", "ISO-8601 string") leaves the exact
algorithm to the implementer. Closing items 1–7 with one fixture each would make the module fully
mechanical.
