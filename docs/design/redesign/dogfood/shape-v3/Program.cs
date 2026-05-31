using System.Collections;
using System.Globalization;
using System.Text.Json;
using Alis.Reactive.PlanModel;

var runner = new FixtureRunner();

// =====================================================================
// A. CLR inference (FromClrType / FromValue) — P-SHAPE axis
// =====================================================================

runner.Run("clr_string_is_string", () =>
    AssertEqual(Shape.String, Shape.FromClrType(typeof(string))));

runner.Run("clr_int_is_number", () =>
{
    AssertEqual(Shape.Number, Shape.FromClrType(typeof(int)));
    AssertEqual(Shape.Number, Shape.FromClrType(typeof(byte)));
    AssertEqual(Shape.Number, Shape.FromClrType(typeof(long)));
    AssertEqual(Shape.Number, Shape.FromClrType(typeof(decimal)));
    AssertEqual(Shape.Number, Shape.FromClrType(typeof(double)));
    AssertEqual(Shape.Number, Shape.FromClrType(typeof(sbyte)));
    AssertEqual(Shape.Number, Shape.FromClrType(typeof(short)));
    AssertEqual(Shape.Number, Shape.FromClrType(typeof(ushort)));
    AssertEqual(Shape.Number, Shape.FromClrType(typeof(uint)));
    AssertEqual(Shape.Number, Shape.FromClrType(typeof(ulong)));
    AssertEqual(Shape.Number, Shape.FromClrType(typeof(float)));
});

runner.Run("clr_bool_is_boolean", () =>
    AssertEqual(Shape.Boolean, Shape.FromClrType(typeof(bool))));

runner.Run("clr_datetime_is_date", () =>
{
    AssertEqual(Shape.Date, Shape.FromClrType(typeof(DateTime)));
    AssertEqual(Shape.Date, Shape.FromClrType(typeof(DateTimeOffset)));
    AssertEqual(Shape.Date, Shape.FromClrType(typeof(DateOnly)));
});

runner.Run("clr_nullable_int_is_nullable_number", () =>
    AssertEqual(Shape.Nullable(Shape.Number), Shape.FromClrType(typeof(int?))));

runner.Run("clr_guid_is_string", () =>
{
    AssertEqual(Shape.String, Shape.FromClrType(typeof(Guid)));
    AssertEqual(Shape.String, Shape.FromClrType(typeof(TimeSpan)));
    AssertEqual(Shape.String, Shape.FromClrType(typeof(TimeOnly)));
});

runner.Run("clr_enum_is_string", () =>
    AssertEqual(Shape.String, Shape.FromClrType(typeof(SampleEnum))));

runner.Run("clr_list_of_t_is_array_of_t", () =>
{
    AssertEqual(Shape.ArrayOf(Shape.Number), Shape.FromClrType(typeof(List<int>)));
    AssertEqual(Shape.ArrayOf(Shape.Number), Shape.FromClrType(typeof(int[])));
    AssertEqual(Shape.ArrayOf(Shape.String), Shape.FromClrType(typeof(IEnumerable<string>)));
});

runner.Run("clr_dictionary_is_any", () =>
    AssertEqual(Shape.Any, Shape.FromClrType(typeof(Dictionary<string, int>))));

runner.Run("clr_unknown_is_any", () =>
    AssertEqual(Shape.Any, Shape.FromClrType(typeof(UnclassifiedPoco))));

runner.Run("clr_nint_is_any", () =>
{
    AssertEqual(Shape.Any, Shape.FromClrType(typeof(nint)));
    AssertEqual(Shape.Any, Shape.FromClrType(typeof(nuint)));
});

runner.Run("clr_non_generic_enumerable_is_any", () =>
    AssertEqual(Shape.Any, Shape.FromClrType(typeof(ArrayList))));

runner.Run("clr_ambiguous_multi_enumerable_is_any", () =>
    AssertEqual(Shape.Any, Shape.FromClrType(typeof(MultiEnumerable))));

runner.Run("from_value_null_is_none", () =>
    AssertEqual(Shape.None, Shape.FromValue(null)));

runner.Run("collection_item_shape_or_none_for_non_collection", () =>
    AssertEqual(Shape.None, Shape.CollectionItemShapeOrNone(typeof(int))));

runner.Run("collection_item_shape_or_none_for_non_generic_enumerable", () =>
    AssertEqual(Shape.None, Shape.CollectionItemShapeOrNone(typeof(ArrayList))));

// =====================================================================
// B. Construction invariants
// =====================================================================

runner.Run("array_of_none_is_rejected", () =>
    AssertThrows<ArgumentException>(() => Shape.ArrayOf(Shape.None)));

runner.Run("nullable_of_none_is_rejected", () =>
    AssertThrows<ArgumentException>(() => Shape.Nullable(Shape.None)));

runner.Run("nullable_scalar_is_scalar", () =>
    AssertTrue(Shape.Nullable(Shape.String).IsScalar));

runner.Run("object_is_not_scalar", () =>
    AssertFalse(Shape.ObjectOf(new() { ["a"] = Shape.String }).IsScalar));

// =====================================================================
// C. Serialization (write-only, matches TS union)
// =====================================================================

runner.Run("scalar_serializes_kind_only", () =>
    AssertJson("{\"kind\":\"string\"}", Shape.String));

runner.Run("array_serializes_item", () =>
    AssertJson("{\"kind\":\"array\",\"item\":{\"kind\":\"number\"}}", Shape.ArrayOf(Shape.Number)));

runner.Run("nullable_serializes_inner", () =>
    AssertJson("{\"kind\":\"nullable\",\"inner\":{\"kind\":\"date\"}}", Shape.Nullable(Shape.Date)));

runner.Run("object_of_fields_serializes_closed", () =>
    AssertJson("{\"kind\":\"object\",\"fields\":{\"a\":{\"kind\":\"string\"}},\"additional\":false}",
        Shape.ObjectOf(new() { ["a"] = Shape.String })));

runner.Run("open_object_serializes_additional_true", () =>
    AssertJson("{\"kind\":\"object\",\"fields\":{},\"additional\":true}", Shape.OpenObject()));

runner.Run("read_is_not_supported", () =>
    AssertThrows<NotSupportedException>(() =>
    {
        // The converter's Read is reached through deserialization of a Shape.
        JsonSerializer.Deserialize<Shape>("{\"kind\":\"string\"}");
    }));

runner.Run("describe_contract_nested", () =>
    AssertEqual("array<object{a:string}>",
        Shape.ArrayOf(Shape.ObjectOf(new() { ["a"] = Shape.String })).DescribeContract()));

runner.Run("describe_open_object", () =>
    AssertEqual("object<open>", Shape.OpenObject().DescribeContract()));

runner.Run("describe_closed_multi_field", () =>
    AssertEqual("object{a:string, b:number}",
        Shape.ObjectOf(new() { ["a"] = Shape.String, ["b"] = Shape.Number }).DescribeContract()));

// =====================================================================
// D. Equality + algebra (ShapeContractCompatibility)
// =====================================================================

runner.Run("equal_array_shapes_are_equal", () =>
{
    var l = Shape.ArrayOf(Shape.Number);
    var r = Shape.ArrayOf(Shape.Number);
    AssertTrue(l == r);
    AssertEqual(l.GetHashCode(), r.GetHashCode());
});

runner.Run("different_object_fields_are_unequal", () =>
    AssertFalse(Shape.ObjectOf(new() { ["a"] = Shape.String }) == Shape.ObjectOf(new() { ["a"] = Shape.Number })));

runner.Run("merge_equal_is_self", () =>
    AssertMerge(Shape.String, Shape.String, true, Shape.String));

runner.Run("merge_any_yields_other", () =>
    AssertMerge(Shape.Any, Shape.Number, true, Shape.Number));

runner.Run("merge_none_conflicts", () =>
    AssertMerge(Shape.None, Shape.String, false, null));

runner.Run("merge_nullable_absorbs_inner", () =>
    AssertMerge(Shape.Nullable(Shape.String), Shape.String, true, Shape.Nullable(Shape.String)));

runner.Run("merge_nullable_any_vs_scalar_is_conflict", () =>
    AssertMerge(Shape.Nullable(Shape.Any), Shape.String, false, null));

runner.Run("merge_arrays_recurse", () =>
    AssertMerge(Shape.ArrayOf(Shape.Any), Shape.ArrayOf(Shape.String), true, Shape.ArrayOf(Shape.String)));

runner.Run("merge_objects_union_fields", () =>
    AssertMerge(
        Shape.ObjectOf(new() { ["a"] = Shape.String }),
        Shape.ObjectOf(new() { ["b"] = Shape.Number }),
        true,
        Shape.ObjectOf(new() { ["a"] = Shape.String, ["b"] = Shape.Number })));

runner.Run("merge_field_conflict_is_conflict", () =>
    AssertMerge(
        Shape.ObjectOf(new() { ["a"] = Shape.String }),
        Shape.ObjectOf(new() { ["a"] = Shape.Number }),
        false, null));

runner.Run("merge_closed_with_open_is_closed", () =>
{
    AssertMerge(
        Shape.ObjectOf(new() { ["a"] = Shape.String }),
        Shape.OpenObject(),
        true,
        Shape.ObjectOf(new() { ["a"] = Shape.String }));
    // closed must mean additional:false
    ShapeContractCompatibility.TryMergeContracts(
        Shape.ObjectOf(new() { ["a"] = Shape.String }), Shape.OpenObject(), out var merged);
    merged!.TryGetObjectContract(out var contract);
    AssertFalse(contract.AllowsAdditionalFields);
});

runner.Run("accept_any_either_side", () =>
{
    AssertTrue(ShapeContractCompatibility.CanAccept(Shape.Any, Shape.ObjectOf(new() { ["a"] = Shape.String })));
    AssertTrue(ShapeContractCompatibility.CanAccept(Shape.String, Shape.Any));
});

runner.Run("reject_none_either_side", () =>
    AssertFalse(ShapeContractCompatibility.CanAccept(Shape.None, Shape.String)));

runner.Run("accept_nullable_inner_exact", () =>
{
    AssertTrue(ShapeContractCompatibility.CanAccept(Shape.Nullable(Shape.String), Shape.String));
    AssertTrue(ShapeContractCompatibility.CanAccept(Shape.String, Shape.Nullable(Shape.String)));
});

runner.Run("accept_nullable_any_vs_scalar_rejects", () =>
{
    AssertFalse(ShapeContractCompatibility.CanAccept(Shape.Nullable(Shape.Any), Shape.String));
    AssertFalse(ShapeContractCompatibility.CanAccept(
        Shape.Nullable(Shape.ArrayOf(Shape.Any)), Shape.ArrayOf(Shape.String)));
});

runner.Run("accept_open_object", () =>
    AssertTrue(ShapeContractCompatibility.CanAccept(Shape.OpenObject(), Shape.ObjectOf(new() { ["a"] = Shape.String }))));

runner.Run("accept_missing_field_when_actual_open", () =>
    AssertTrue(ShapeContractCompatibility.CanAccept(Shape.ObjectOf(new() { ["a"] = Shape.String }), Shape.OpenObject())));

runner.Run("reject_missing_required_field", () =>
    AssertFalse(ShapeContractCompatibility.CanAccept(
        Shape.ObjectOf(new() { ["a"] = Shape.String }),
        Shape.ObjectOf(new() { ["b"] = Shape.Number }))));

// =====================================================================
// E. Runtime conversion (applyShape / convertByShape / formatForWire / toX)
// =====================================================================

runner.Run("apply_string_coerces_number", () =>
    AssertEqual("5", ShapeConvert.ApplyShape(5d, Shape.String)));

runner.Run("apply_number_parses_text", () =>
    AssertEqual(3d, ShapeConvert.ApplyShape("3", Shape.Number)));

runner.Run("apply_boolean_truthy_text", () =>
    AssertEqual(false, ShapeConvert.ApplyShape("false", Shape.Boolean)));

runner.Run("apply_date_only_is_local_midnight", () =>
{
    var expected = LocalMidnightEpochMs(2026, 1, 15);
    AssertEqual(expected, ShapeConvert.ApplyShape("2026-01-15", Shape.Date));
});

runner.Run("apply_array_recurses_items", () =>
    AssertSequence(new object?[] { 1d, 2d },
        ShapeConvert.ApplyShape(new object?[] { "1", "2" }, Shape.ArrayOf(Shape.Number))));

runner.Run("apply_object_keeps_open_extras", () =>
{
    var input = new Dictionary<string, object?> { ["a"] = 1d, ["x"] = 2d };
    var result = (IReadOnlyDictionary<string, object?>)ShapeConvert.ApplyShape(input, Shape.OpenObject())!;
    AssertEqual(2, result.Count);
    AssertEqual(1d, result["a"]);
    AssertEqual(2d, result["x"]);
});

runner.Run("apply_nullable_missing_is_null", () =>
    AssertNull(ShapeConvert.ApplyShape(null, Shape.Nullable(Shape.String))));

runner.Run("apply_raw_is_identity", () =>
{
    var marker = new object();
    AssertSame(marker, ShapeConvert.ApplyShape(marker, Shape.Raw));
    AssertSame(marker, ShapeConvert.ApplyShape(marker, Shape.Any));
    AssertSame(marker, ShapeConvert.ApplyShape(marker, Shape.None));
});

runner.Run("apply_object_skips_absent_declared_field", () =>
{
    var input = new Dictionary<string, object?>();
    var result = (IReadOnlyDictionary<string, object?>)ShapeConvert.ApplyShape(
        input, Shape.ObjectOf(new() { ["a"] = Shape.String }))!;
    AssertEqual(0, result.Count); // skipped, NOT {a:""}
    AssertFalse(result.ContainsKey("a"));
});

runner.Run("to_string_array_is_comma_joined", () =>
    AssertConvertOk("1,2", ShapeConvert.ToString(new object?[] { 1d, 2d })));

runner.Run("to_number_bool_is_one", () =>
{
    AssertConvertOk(1d, ShapeConvert.ToNumber(true));
    AssertConvertOk(0d, ShapeConvert.ToNumber(false));
});

runner.Run("to_number_unparseable_is_err", () =>
{
    AssertConvertErr(ShapeConvert.ToNumber("abc"));
    AssertConvertOk(0d, ShapeConvert.ToNumber(""));
    AssertConvertOk(0d, ShapeConvert.ToNumber("   "));
});

runner.Run("to_boolean_arbitrary_text_is_true", () =>
{
    AssertConvertOk(true, ShapeConvert.ToBoolean("no"));
    AssertConvertOk(true, ShapeConvert.ToBoolean("off"));
});

runner.Run("to_boolean_empty_array_is_false", () =>
    AssertConvertOk(false, ShapeConvert.ToBoolean(Array.Empty<object?>())));

runner.Run("to_boolean_nonempty_array_is_true", () =>
    AssertConvertOk(true, ShapeConvert.ToBoolean(new object?[] { 0d })));

runner.Run("to_boolean_object_is_err", () =>
    AssertConvertErr(ShapeConvert.ToBoolean(new Dictionary<string, object?>())));

runner.Run("to_date_missing_is_nan", () =>
{
    var nullResult = ShapeConvert.ToDate(null);
    AssertTrue(nullResult.Ok);
    AssertTrue(double.IsNaN(nullResult.Value));
});

runner.Run("convert_object_into_scalar_is_err", () =>
    AssertConvertErr(ShapeConvert.ConvertByShape(new Dictionary<string, object?>(), Shape.String)));

runner.Run("convert_array_lenient_on_noncoercible_item", () =>
{
    var input = new object?[] { 1d, 2d, new Dictionary<string, object?>() };
    var result = ShapeConvert.ConvertByShape(input, Shape.ArrayOf(Shape.Number));
    AssertTrue(result.Ok);
    var items = (IReadOnlyList<object?>)result.Value!;
    AssertEqual(3, items.Count);
    AssertEqual(1d, items[0]);
    AssertEqual(2d, items[1]);
    AssertTrue(items[2] is IReadOnlyDictionary<string, object?>); // {} stays as-is
});

runner.Run("convert_object_lenient_on_field_miss", () =>
{
    var input = new Dictionary<string, object?> { ["a"] = new Dictionary<string, object?>() };
    var result = ShapeConvert.ConvertByShape(input, Shape.ObjectOf(new() { ["a"] = Shape.Number }));
    AssertTrue(result.Ok);
    var obj = (IReadOnlyDictionary<string, object?>)result.Value!;
    AssertTrue(obj["a"] is IReadOnlyDictionary<string, object?>); // {} left as-is
});

runner.Run("format_date_to_iso", () =>
{
    // Pick a known epoch ms; expected = new Date(ms).toISOString().
    var ms = 1_768_000_000_000d; // some finite epoch ms
    var expected = ExpectedIso(ms);
    var declared = RuntimeShape.From(Shape.Date);
    AssertEqual(expected, declared.FormatForWire(ms));
});

runner.Run("format_nullable_unwraps", () =>
{
    var ms = 1_768_000_000_000d;
    var expected = ExpectedIso(ms);
    var declared = RuntimeShape.From(Shape.Nullable(Shape.Date));
    AssertEqual(expected, declared.FormatForWire(ms));
});

runner.Run("format_unshaped_passthrough", () =>
{
    var marker = new object();
    AssertSame(marker, RuntimeShape.Unshaped().FormatForWire(marker));
});

runner.Run("format_nan_date_passthrough", () =>
{
    var declared = RuntimeShape.From(Shape.Date);
    var result = declared.FormatForWire(double.NaN);
    AssertTrue(result is double d && double.IsNaN(d));
});

runner.Run("runtime_shape_item_of_array", () =>
    AssertEqual(Shape.Number, RuntimeShape.From(Shape.ArrayOf(Shape.Number)).Item().PlanShape));

// =====================================================================
// F-Shape rows from _fixtures.md Module 1 (the cross-cut catalogue)
// =====================================================================

runner.Run("F-Shape-String", () =>
{
    AssertJson("{\"kind\":\"string\"}", Shape.FromClrType(typeof(string)));
    AssertEqual("5", ShapeConvert.ApplyShape(5d, Shape.String));
    // bare string missing → "" (scalar zero), not null.
    AssertEqual("", ShapeConvert.ApplyShape(null, Shape.String));
    // only nullable<string> yields null for a missing value.
    AssertNull(ShapeConvert.ApplyShape(null, Shape.Nullable(Shape.String)));
});

runner.Run("F-Shape-Number", () =>
{
    AssertJson("{\"kind\":\"number\"}", Shape.FromClrType(typeof(int)));
    AssertJson("{\"kind\":\"number\"}", Shape.FromClrType(typeof(long)));
    AssertJson("{\"kind\":\"number\"}", Shape.FromClrType(typeof(decimal)));
    AssertJson("{\"kind\":\"number\"}", Shape.FromClrType(typeof(double)));
    AssertEqual(3d, ShapeConvert.ApplyShape("3", Shape.Number));
});

runner.Run("F-Shape-Boolean", () =>
{
    AssertJson("{\"kind\":\"boolean\"}", Shape.FromClrType(typeof(bool)));
    AssertEqual(true, ShapeConvert.ApplyShape("true", Shape.Boolean));
});

runner.Run("F-Shape-Date", () =>
{
    AssertJson("{\"kind\":\"date\"}", Shape.FromClrType(typeof(DateTime)));
    // runtime egress of a finite epoch-ms number = Date.toISOString().
    var ms = 1_768_000_000_000d;
    AssertEqual(ExpectedIso(ms), RuntimeShape.From(Shape.Date).FormatForWire(ms));
});

runner.Run("F-Shape-NullableScalar", () =>
{
    AssertJson("{\"kind\":\"nullable\",\"inner\":{\"kind\":\"number\"}}", Shape.FromClrType(typeof(int?)));
    AssertJson("{\"kind\":\"nullable\",\"inner\":{\"kind\":\"date\"}}", Shape.FromClrType(typeof(DateTime?)));
    // present → coerce inner; absent → null (no default)
    AssertEqual(3d, ShapeConvert.ApplyShape("3", Shape.Nullable(Shape.Number)));
    AssertNull(ShapeConvert.ApplyShape(null, Shape.Nullable(Shape.Number)));
});

runner.Run("F-Shape-Array", () =>
{
    AssertJson("{\"kind\":\"array\",\"item\":{\"kind\":\"number\"}}", Shape.FromClrType(typeof(int[])));
    AssertJson("{\"kind\":\"array\",\"item\":{\"kind\":\"string\"}}", Shape.FromClrType(typeof(IEnumerable<string>)));
    // non-generic IEnumerable / dictionary / ambiguous-T → any
    AssertEqual(Shape.Any, Shape.FromClrType(typeof(ArrayList)));
    AssertEqual(Shape.Any, Shape.FromClrType(typeof(Dictionary<string, int>)));
    AssertEqual(Shape.Any, Shape.FromClrType(typeof(MultiEnumerable)));
    // each item shaped by item
    AssertSequence(new object?[] { 1d, 2d },
        ShapeConvert.ApplyShape(new object?[] { "1", "2" }, Shape.ArrayOf(Shape.Number)));
});

runner.Run("F-Shape-Object", () =>
{
    AssertJson("{\"kind\":\"object\",\"fields\":{\"a\":{\"kind\":\"string\"}},\"additional\":false}",
        Shape.ObjectOf(new() { ["a"] = Shape.String }));
    // present declared field shaped; extra keys dropped (closed); absent declared field skipped.
    var input = new Dictionary<string, object?> { ["a"] = 5d, ["x"] = 9d };
    var result = (IReadOnlyDictionary<string, object?>)ShapeConvert.ApplyShape(
        input, Shape.ObjectOf(new() { ["a"] = Shape.String }))!;
    AssertEqual(1, result.Count);     // closed → extra key x dropped
    AssertEqual("5", result["a"]);    // present declared field shaped
    AssertFalse(result.ContainsKey("x"));

    var emptyInput = new Dictionary<string, object?>();
    var skipped = (IReadOnlyDictionary<string, object?>)ShapeConvert.ApplyShape(
        emptyInput, Shape.ObjectOf(new() { ["a"] = Shape.String }))!;
    AssertEqual(0, skipped.Count);    // absent declared field skipped, not materialized
});

runner.Run("F-Shape-Raw", () =>
{
    AssertJson("{\"kind\":\"raw\"}", Shape.Raw);
    var marker = new object();
    AssertSame(marker, ShapeConvert.ApplyShape(marker, Shape.Raw)); // passed through unconverted
});

runner.Run("F-Shape-Any", () =>
{
    AssertJson("{\"kind\":\"any\"}", Shape.Any);
    var marker = new object();
    AssertSame(marker, ShapeConvert.ApplyShape(marker, Shape.Any)); // identity, never a guessed scalar
});

runner.Run("F-Shape-None", () =>
{
    AssertJson("{\"kind\":\"none\"}", Shape.None);
    AssertEqual(Shape.None, Shape.FromValue(null));
    var marker = new object();
    AssertSame(marker, ShapeConvert.ApplyShape(marker, Shape.None)); // absence, identity
});

runner.Run("F-Shape-Once", () =>
{
    // value is shaped exactly once on egress; identical bytes everywhere.
    // formatForWire is idempotent: feeding a finite epoch ms once produces the ISO string;
    // running formatForWire again over the already-ISO string passes it through unchanged.
    var ms = 1_768_000_000_000d;
    var declared = RuntimeShape.From(Shape.Date);
    var once = declared.FormatForWire(ms);
    AssertEqual(ExpectedIso(ms), once);
    var twice = declared.FormatForWire(once); // already a string, not a finite number → passthrough
    AssertSame(once, twice);
});

return runner.Report();

// =====================================================================
// Assertion helpers + date math
// =====================================================================

static void AssertEqual(object? expected, object? actual)
{
    if (expected is Shape es && actual is Shape ascii)
    {
        if (es != ascii)
            throw new FixtureFailure($"expected shape <{es.DescribeContract()}> but got <{ascii.DescribeContract()}>");
        return;
    }
    if (!Equals(expected, actual))
        throw new FixtureFailure($"expected <{Format(expected)}> but got <{Format(actual)}>");
}

static void AssertSame(object? expected, object? actual)
{
    if (!ReferenceEquals(expected, actual))
        throw new FixtureFailure($"expected the SAME reference <{Format(expected)}> but got <{Format(actual)}>");
}

static void AssertTrue(bool condition)
{
    if (!condition)
        throw new FixtureFailure("expected true but got false");
}

static void AssertFalse(bool condition)
{
    if (condition)
        throw new FixtureFailure("expected false but got true");
}

static void AssertNull(object? value)
{
    if (value is not null)
        throw new FixtureFailure($"expected null but got <{Format(value)}>");
}

static void AssertThrows<TException>(Action action) where TException : Exception
{
    try
    {
        action();
    }
    catch (TException)
    {
        return;
    }
    catch (Exception ex)
    {
        throw new FixtureFailure($"expected {typeof(TException).Name} but got {ex.GetType().Name}: {ex.Message}");
    }
    throw new FixtureFailure($"expected {typeof(TException).Name} but no exception was thrown");
}

static void AssertJson(string expected, Shape shape)
{
    var actual = JsonSerializer.Serialize(shape);
    if (actual != expected)
        throw new FixtureFailure($"expected JSON {expected} but got {actual}");
}

static void AssertMerge(Shape existing, Shape incoming, bool expectedOk, Shape? expectedMerged)
{
    var ok = ShapeContractCompatibility.TryMergeContracts(existing, incoming, out var merged);
    if (ok != expectedOk)
        throw new FixtureFailure($"expected merge ok={expectedOk} but got ok={ok}");
    if (!expectedOk)
    {
        if (merged is not null)
            throw new FixtureFailure($"expected null merged on conflict but got <{merged.DescribeContract()}>");
        return;
    }
    if (merged != expectedMerged)
        throw new FixtureFailure(
            $"expected merged <{expectedMerged!.DescribeContract()}> but got <{merged!.DescribeContract()}>");
}

static void AssertSequence(IReadOnlyList<object?> expected, object? actual)
{
    if (actual is not IReadOnlyList<object?> list)
        throw new FixtureFailure($"expected a list but got <{Format(actual)}>");
    if (list.Count != expected.Count)
        throw new FixtureFailure($"expected {expected.Count} items but got {list.Count}");
    for (var i = 0; i < expected.Count; i++)
        if (!Equals(expected[i], list[i]))
            throw new FixtureFailure($"item {i}: expected <{Format(expected[i])}> but got <{Format(list[i])}>");
}

static void AssertConvertOk<T>(T expected, ShapeConvert.ConvertResult<T> result)
{
    if (!result.Ok)
        throw new FixtureFailure($"expected ok(<{Format(expected)}>) but got err: {result.Error}");
    if (!Equals(expected, result.Value))
        throw new FixtureFailure($"expected ok(<{Format(expected)}>) but got ok(<{Format(result.Value)}>)");
}

static void AssertConvertErr<T>(ShapeConvert.ConvertResult<T> result)
{
    if (result.Ok)
        throw new FixtureFailure($"expected err but got ok(<{Format(result.Value)}>)");
}

static string Format(object? value) => value switch
{
    null => "null",
    string s => $"\"{s}\"",
    bool b => b ? "true" : "false",
    double d => d.ToString("R", CultureInfo.InvariantCulture),
    _ => value.ToString() ?? "?"
};

static double LocalMidnightEpochMs(int year, int month, int day)
{
    var localMidnight = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Local);
    var utc = localMidnight.ToUniversalTime();
    return (utc - DateTimeOffset.UnixEpoch.UtcDateTime).TotalMilliseconds;
}

static string ExpectedIso(double epochMs)
{
    var dto = DateTimeOffset.FromUnixTimeMilliseconds((long)epochMs).ToUniversalTime();
    return dto.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
}

// =====================================================================
// Fixture-runner plumbing + sample CLR types for inference fixtures
// =====================================================================

internal sealed class FixtureFailure(string message) : Exception(message);

internal sealed class FixtureRunner
{
    private int _passed;
    private int _total;
    private readonly List<string> _failures = [];

    internal void Run(string name, Action body)
    {
        _total++;
        try
        {
            body();
            _passed++;
        }
        catch (FixtureFailure f)
        {
            _failures.Add($"  FAIL {name}: {f.Message}");
        }
        catch (Exception ex)
        {
            _failures.Add($"  ERROR {name}: {ex.GetType().Name}: {ex.Message}");
        }
    }

    internal int Report()
    {
        foreach (var failure in _failures)
            Console.WriteLine(failure);

        Console.WriteLine($"Fixtures: {_passed}/{_total} passed.");
        if (_passed == _total)
        {
            Console.WriteLine("ALL GREEN");
            return 0;
        }
        return 1;
    }
}

internal enum SampleEnum
{
    A,
    B
}

internal sealed class UnclassifiedPoco
{
    public int Value { get; set; }
}

internal sealed class MultiEnumerable : IEnumerable<int>, IEnumerable<string>
{
    IEnumerator<int> IEnumerable<int>.GetEnumerator() => Enumerable.Empty<int>().GetEnumerator();
    IEnumerator<string> IEnumerable<string>.GetEnumerator() => Enumerable.Empty<string>().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Enumerable.Empty<int>().GetEnumerator();
}
