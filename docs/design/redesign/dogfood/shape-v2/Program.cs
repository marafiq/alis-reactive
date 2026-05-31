using System.Collections;
using System.Globalization;
using System.Text.Json;
using Alis.Reactive.PlanModel;

// =============================================================================================
// Shape micro-module dogfood proof (v2). Every fixture from Shape.md §6 (A/B/C/D/E) plus the
// F-Shape rows from _fixtures.md Module 1, encoded as runnable assertions.
// =============================================================================================

var runner = new FixtureRunner();

// ---------------------------------------------------------------------------------------------
// A. CLR inference (FromClrType / FromValue) — P-SHAPE axis
// ---------------------------------------------------------------------------------------------

runner.Run("clr_string_is_string", () =>
    AssertEqual(Shape.String, Shape.FromClrType(typeof(string))));

runner.Run("clr_int_is_number", () =>
{
    AssertEqual(Shape.Number, Shape.FromClrType(typeof(int)));
    AssertEqual(Shape.Number, Shape.FromClrType(typeof(byte)));
    AssertEqual(Shape.Number, Shape.FromClrType(typeof(sbyte)));
    AssertEqual(Shape.Number, Shape.FromClrType(typeof(short)));
    AssertEqual(Shape.Number, Shape.FromClrType(typeof(ushort)));
    AssertEqual(Shape.Number, Shape.FromClrType(typeof(uint)));
    AssertEqual(Shape.Number, Shape.FromClrType(typeof(long)));
    AssertEqual(Shape.Number, Shape.FromClrType(typeof(ulong)));
    AssertEqual(Shape.Number, Shape.FromClrType(typeof(float)));
    AssertEqual(Shape.Number, Shape.FromClrType(typeof(double)));
    AssertEqual(Shape.Number, Shape.FromClrType(typeof(decimal)));
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
    AssertEqual(Shape.Any, Shape.FromClrType(typeof(UnclassifiablePoco))));

runner.Run("clr_nint_is_any", () =>
{
    AssertEqual(Shape.Any, Shape.FromClrType(typeof(nint)));
    AssertEqual(Shape.Any, Shape.FromClrType(typeof(nuint)));
});

runner.Run("clr_non_generic_enumerable_is_any", () =>
    AssertEqual(Shape.Any, Shape.FromClrType(typeof(ArrayList))));

runner.Run("clr_ambiguous_multi_enumerable_is_any", () =>
    AssertEqual(Shape.Any, Shape.FromClrType(typeof(DualEnumerable))));

runner.Run("from_value_null_is_none", () =>
    AssertEqual(Shape.None, Shape.FromValue(null)));

runner.Run("collection_item_shape_or_none_for_non_collection", () =>
    AssertEqual(Shape.None, Shape.CollectionItemShapeOrNone(typeof(int))));

runner.Run("collection_item_shape_or_none_for_non_generic_enumerable", () =>
    AssertEqual(Shape.None, Shape.CollectionItemShapeOrNone(typeof(ArrayList))));

// ---------------------------------------------------------------------------------------------
// B. Construction invariants (null unrepresentable by construction)
// ---------------------------------------------------------------------------------------------

runner.Run("array_of_none_is_rejected", () =>
    AssertThrows<ArgumentException>(() => Shape.ArrayOf(Shape.None)));

runner.Run("nullable_of_none_is_rejected", () =>
    AssertThrows<ArgumentException>(() => Shape.Nullable(Shape.None)));

runner.Run("nullable_scalar_is_scalar", () =>
    AssertTrue(Shape.Nullable(Shape.String).IsScalar));

runner.Run("object_is_not_scalar", () =>
    AssertFalse(ObjectOf(("a", Shape.String)).IsScalar));

// ---------------------------------------------------------------------------------------------
// C. Serialization (write-only, matches TS union)
// ---------------------------------------------------------------------------------------------

runner.Run("scalar_serializes_kind_only", () =>
    AssertJson("{\"kind\":\"string\"}", Shape.String));

runner.Run("array_serializes_item", () =>
    AssertJson("{\"kind\":\"array\",\"item\":{\"kind\":\"number\"}}", Shape.ArrayOf(Shape.Number)));

runner.Run("nullable_serializes_inner", () =>
    AssertJson("{\"kind\":\"nullable\",\"inner\":{\"kind\":\"date\"}}", Shape.Nullable(Shape.Date)));

runner.Run("object_of_fields_serializes_closed", () =>
    AssertJson(
        "{\"kind\":\"object\",\"fields\":{\"a\":{\"kind\":\"string\"}},\"additional\":false}",
        ObjectOf(("a", Shape.String))));

runner.Run("open_object_serializes_additional_true", () =>
    AssertJson("{\"kind\":\"object\",\"fields\":{},\"additional\":true}", Shape.OpenObject()));

runner.Run("read_is_not_supported", () =>
    AssertThrows<NotSupportedException>(() =>
    {
        // The converter's Read must reject any attempt to deserialize a Shape.
        _ = JsonSerializer.Deserialize<Shape>("{\"kind\":\"string\"}");
    }));

runner.Run("describe_contract_nested", () =>
    AssertEqual("array<object{a:string}>", Shape.ArrayOf(ObjectOf(("a", Shape.String))).DescribeContract()));

runner.Run("describe_open_object", () =>
    AssertEqual("object<open>", Shape.OpenObject().DescribeContract()));

runner.Run("describe_closed_multi_field", () =>
    AssertEqual(
        "object{a:string, b:number}",
        ObjectOf(("a", Shape.String), ("b", Shape.Number)).DescribeContract()));

// ---------------------------------------------------------------------------------------------
// D. Equality + algebra (ShapeContractCompatibility)
// ---------------------------------------------------------------------------------------------

runner.Run("equal_array_shapes_are_equal", () =>
{
    Shape left = Shape.ArrayOf(Shape.Number);
    Shape right = Shape.ArrayOf(Shape.Number);
    AssertTrue(left == right);
    AssertEqual(left.GetHashCode(), right.GetHashCode());
});

runner.Run("different_object_fields_are_unequal", () =>
    AssertFalse(ObjectOf(("a", Shape.String)) == ObjectOf(("a", Shape.Number))));

runner.Run("merge_equal_is_self", () =>
    AssertMerge(Shape.String, Shape.String, expected: Shape.String));

runner.Run("merge_any_yields_other", () =>
    AssertMerge(Shape.Any, Shape.Number, expected: Shape.Number));

runner.Run("merge_none_conflicts", () =>
    AssertConflict(Shape.None, Shape.String));

runner.Run("merge_nullable_absorbs_inner", () =>
    AssertMerge(Shape.Nullable(Shape.String), Shape.String, expected: Shape.Nullable(Shape.String)));

runner.Run("merge_arrays_recurse", () =>
    AssertMerge(Shape.ArrayOf(Shape.Any), Shape.ArrayOf(Shape.String), expected: Shape.ArrayOf(Shape.String)));

runner.Run("merge_objects_union_fields", () =>
    AssertMerge(
        ObjectOf(("a", Shape.String)),
        ObjectOf(("b", Shape.Number)),
        expected: ObjectOf(("a", Shape.String), ("b", Shape.Number))));

runner.Run("merge_field_conflict_is_conflict", () =>
    AssertConflict(ObjectOf(("a", Shape.String)), ObjectOf(("a", Shape.Number))));

runner.Run("merge_closed_with_open_is_closed", () =>
{
    AssertTrue(ShapeContractCompatibility.TryMergeContracts(
        ObjectOf(("a", Shape.String)), Shape.OpenObject(), out Shape? merged));
    AssertEqual(ObjectOf(("a", Shape.String)), merged!);
    AssertJson(
        "{\"kind\":\"object\",\"fields\":{\"a\":{\"kind\":\"string\"}},\"additional\":false}",
        merged!);
});

runner.Run("accept_any_either_side", () =>
{
    AssertTrue(ShapeContractCompatibility.CanAccept(Shape.Any, ObjectOf(("a", Shape.String))));
    AssertTrue(ShapeContractCompatibility.CanAccept(Shape.String, Shape.Any));
});

runner.Run("reject_none_either_side", () =>
    AssertFalse(ShapeContractCompatibility.CanAccept(Shape.None, Shape.String)));

runner.Run("accept_open_object", () =>
    AssertTrue(ShapeContractCompatibility.CanAccept(Shape.OpenObject(), ObjectOf(("a", Shape.String)))));

runner.Run("accept_missing_field_when_actual_open", () =>
    AssertTrue(ShapeContractCompatibility.CanAccept(ObjectOf(("a", Shape.String)), Shape.OpenObject())));

runner.Run("reject_missing_required_field", () =>
    AssertFalse(ShapeContractCompatibility.CanAccept(ObjectOf(("a", Shape.String)), ObjectOf(("b", Shape.Number)))));

// ---------------------------------------------------------------------------------------------
// E. Runtime conversion (applyShape / convertByShape / formatForWire)
// ---------------------------------------------------------------------------------------------

runner.Run("apply_string_coerces_number", () =>
    AssertEqual("5", ShapeConvert.ApplyShape(5, Shape.String)));

runner.Run("apply_number_parses_text", () =>
    AssertEqual(3d, ShapeConvert.ApplyShape("3", Shape.Number)));

runner.Run("apply_boolean_truthy_text", () =>
    AssertEqual(false, ShapeConvert.ApplyShape("false", Shape.Boolean)));

runner.Run("apply_date_only_is_local_midnight", () =>
{
    double expected = LocalMidnightEpochMs(2026, 1, 15);
    AssertEqual(expected, ShapeConvert.ApplyShape("2026-01-15", Shape.Date));
});

runner.Run("apply_array_recurses_items", () =>
{
    object? result = ShapeConvert.ApplyShape(Arr("1", "2"), Shape.ArrayOf(Shape.Number));
    AssertArrayEqual(new object?[] { 1d, 2d }, result);
});

runner.Run("apply_object_keeps_open_extras", () =>
{
    object? result = ShapeConvert.ApplyShape(Obj(("a", 1), ("x", 2)), Shape.OpenObject());
    AssertObjectEqual(new Dictionary<string, object?> { ["a"] = 1, ["x"] = 2 }, result);
});

runner.Run("apply_nullable_missing_is_null", () =>
    AssertNull(ShapeConvert.ApplyShape(null, Shape.Nullable(Shape.String))));

runner.Run("apply_raw_is_identity", () =>
{
    object marker = new();
    AssertSame(marker, ShapeConvert.ApplyShape(marker, Shape.Raw));
    AssertSame(marker, ShapeConvert.ApplyShape(marker, Shape.Any));
    AssertSame(marker, ShapeConvert.ApplyShape(marker, Shape.None));
});

runner.Run("apply_object_skips_absent_declared_field", () =>
{
    object? result = ShapeConvert.ApplyShape(Obj(), ObjectOf(("a", Shape.String)));
    AssertObjectEqual(new Dictionary<string, object?>(), result);
});

runner.Run("to_string_array_is_comma_joined", () =>
    AssertOk("1,2", ShapeConvert.ToString(Arr(1, 2))));

runner.Run("to_number_bool_is_one", () =>
{
    AssertOk(1d, ShapeConvert.ToNumber(true));
    AssertOk(0d, ShapeConvert.ToNumber(false));
});

runner.Run("to_number_unparseable_is_err", () =>
{
    AssertErr(ShapeConvert.ToNumber("abc"));
    AssertOk(0d, ShapeConvert.ToNumber(""));
    AssertOk(0d, ShapeConvert.ToNumber("   "));
});

runner.Run("to_boolean_arbitrary_text_is_true", () =>
{
    AssertOk(true, ShapeConvert.ToBoolean("no"));
    AssertOk(true, ShapeConvert.ToBoolean("off"));
    AssertOk(false, ShapeConvert.ToBoolean(""));
    AssertOk(false, ShapeConvert.ToBoolean("false"));
    AssertOk(false, ShapeConvert.ToBoolean("0"));
});

runner.Run("to_boolean_empty_array_is_false", () =>
    AssertOk(false, ShapeConvert.ToBoolean(Arr())));

runner.Run("to_boolean_nonempty_array_is_true", () =>
    AssertOk(true, ShapeConvert.ToBoolean(Arr(0))));

runner.Run("to_boolean_object_is_err", () =>
    AssertErr(ShapeConvert.ToBoolean(Obj())));

runner.Run("to_date_missing_is_nan", () =>
{
    AssertOkNan(ShapeConvert.ToDate(null));
});

runner.Run("convert_object_into_scalar_is_err", () =>
    AssertErr(ShapeConvert.ConvertByShape(Obj(), Shape.String)));

runner.Run("format_date_to_iso", () =>
{
    double epochMs = LocalMidnightEpochMs(2026, 1, 15);
    object? wire = RuntimeShape.From(Shape.Date).FormatForWire(epochMs);
    AssertEqual(ShapeConvert.EpochMsToIsoString(epochMs), wire);
    // ISO must be UTC, millisecond precision, trailing Z.
    AssertTrue(wire is string s && s.EndsWith("Z", StringComparison.Ordinal) && s.Contains('.'));
});

runner.Run("format_nullable_unwraps", () =>
{
    double epochMs = LocalMidnightEpochMs(2026, 1, 15);
    object? wire = RuntimeShape.From(Shape.Nullable(Shape.Date)).FormatForWire(epochMs);
    AssertEqual(ShapeConvert.EpochMsToIsoString(epochMs), wire);
});

runner.Run("format_unshaped_passthrough", () =>
{
    object marker = new();
    AssertSame(marker, RuntimeShape.From(Shape.None).FormatForWire(marker));
});

runner.Run("format_nan_date_passthrough", () =>
{
    object? wire = RuntimeShape.From(Shape.Date).FormatForWire(double.NaN);
    AssertTrue(wire is double d && double.IsNaN(d));
});

runner.Run("runtime_shape_item_of_array", () =>
{
    RuntimeShape item = RuntimeShape.From(Shape.ArrayOf(Shape.Number)).Item();
    AssertTrue(item.IsDeclared);
    AssertEqual(Shape.Number, item.PlanShape);
});

// ---------------------------------------------------------------------------------------------
// F. F-Shape rows from _fixtures.md Module 1 (the shape-tag JSON + browser behavior)
// ---------------------------------------------------------------------------------------------

runner.Run("F-Shape-String", () =>
{
    AssertJson("{\"kind\":\"string\"}", Shape.String);
    AssertEqual("5", ShapeConvert.ApplyShape(5, Shape.String));
    // null -> null on the nullable string path (String itself coerces missing to "").
    AssertNull(ShapeConvert.ApplyShape(null, Shape.Nullable(Shape.String)));
});

runner.Run("F-Shape-Number", () =>
{
    AssertJson("{\"kind\":\"number\"}", Shape.Number);
    AssertEqual(3d, ShapeConvert.ApplyShape("3", Shape.Number));
});

runner.Run("F-Shape-Boolean", () =>
{
    AssertJson("{\"kind\":\"boolean\"}", Shape.Boolean);
    AssertEqual(true, ShapeConvert.ApplyShape("true", Shape.Boolean));
});

runner.Run("F-Shape-Date", () =>
{
    AssertJson("{\"kind\":\"date\"}", Shape.Date);
    // runtime egress of a finite epoch-ms number = ISO (UTC, ms, …Z).
    double epochMs = LocalMidnightEpochMs(2026, 1, 15);
    AssertEqual(ShapeConvert.EpochMsToIsoString(epochMs), RuntimeShape.From(Shape.Date).FormatForWire(epochMs));
});

runner.Run("F-Shape-NullableScalar", () =>
{
    AssertJson("{\"kind\":\"nullable\",\"inner\":{\"kind\":\"number\"}}", Shape.Nullable(Shape.Number));
    AssertEqual(3d, ShapeConvert.ApplyShape("3", Shape.Nullable(Shape.Number)));
    AssertNull(ShapeConvert.ApplyShape(null, Shape.Nullable(Shape.Number)));
});

runner.Run("F-Shape-Array", () =>
{
    AssertJson("{\"kind\":\"array\",\"item\":{\"kind\":\"number\"}}", Shape.ArrayOf(Shape.Number));
    object? result = ShapeConvert.ApplyShape(Arr("1", "2"), Shape.ArrayOf(Shape.Number));
    AssertArrayEqual(new object?[] { 1d, 2d }, result);
});

runner.Run("F-Shape-Object", () =>
{
    AssertJson(
        "{\"kind\":\"object\",\"fields\":{\"a\":{\"kind\":\"string\"}},\"additional\":false}",
        ObjectOf(("a", Shape.String)));
    // present declared field shaped; extra keys dropped (closed); absent declared field skipped.
    object? present = ShapeConvert.ApplyShape(Obj(("a", 5), ("x", 9)), ObjectOf(("a", Shape.String)));
    AssertObjectEqual(new Dictionary<string, object?> { ["a"] = "5" }, present);
    object? absent = ShapeConvert.ApplyShape(Obj(), ObjectOf(("a", Shape.String)));
    AssertObjectEqual(new Dictionary<string, object?>(), absent);
});

runner.Run("F-Shape-Raw", () =>
{
    AssertJson("{\"kind\":\"raw\"}", Shape.Raw);
    object marker = new();
    AssertSame(marker, ShapeConvert.ApplyShape(marker, Shape.Raw));
});

runner.Run("F-Shape-Any", () =>
{
    AssertJson("{\"kind\":\"any\"}", Shape.Any);
    object marker = new();
    AssertSame(marker, ShapeConvert.ApplyShape(marker, Shape.Any));
});

runner.Run("F-Shape-None", () =>
{
    AssertJson("{\"kind\":\"none\"}", Shape.None);
    object marker = new();
    AssertSame(marker, ShapeConvert.ApplyShape(marker, Shape.None));
});

runner.Run("F-Shape-Once", () =>
{
    // A value is shaped exactly once on egress: formatForWire is idempotent on its own output
    // (a string is not a finite number, so a second pass is a passthrough).
    double epochMs = LocalMidnightEpochMs(2026, 1, 15);
    RuntimeShape date = RuntimeShape.From(Shape.Date);
    object? first = date.FormatForWire(epochMs);
    object? second = date.FormatForWire(first);
    AssertEqual(first, second);
});

return runner.Report();

// =============================================================================================
// Assertion helpers + JS value-model builders
// =============================================================================================

static Shape ObjectOf(params (string Name, Shape Shape)[] fields)
{
    Dictionary<string, Shape> map = [];
    foreach ((string name, Shape shape) in fields)
    {
        map[name] = shape;
    }

    return Shape.ObjectOf(map);
}

static List<object?> Arr(params object?[] items) => [.. items];

static Dictionary<string, object?> Obj(params (string Name, object? Value)[] fields)
{
    Dictionary<string, object?> map = [];
    foreach ((string name, object? value) in fields)
    {
        map[name] = value;
    }

    return map;
}

static double LocalMidnightEpochMs(int year, int month, int day)
{
    DateTime localMidnight = DateTime.SpecifyKind(new DateTime(year, month, day), DateTimeKind.Local);
    return new DateTimeOffset(localMidnight).ToUnixTimeMilliseconds();
}

static void AssertTrue(bool condition)
{
    if (!condition)
    {
        throw new FixtureException("Expected true but was false.");
    }
}

static void AssertFalse(bool condition)
{
    if (condition)
    {
        throw new FixtureException("Expected false but was true.");
    }
}

static void AssertNull(object? value)
{
    if (value is not null)
    {
        throw new FixtureException($"Expected null but was {Describe(value)}.");
    }
}

static void AssertSame(object expected, object? actual)
{
    if (!ReferenceEquals(expected, actual))
    {
        throw new FixtureException($"Expected the same reference but was {Describe(actual)}.");
    }
}

static void AssertEqual(object? expected, object? actual)
{
    if (!Equals(expected, actual))
    {
        throw new FixtureException($"Expected {Describe(expected)} but was {Describe(actual)}.");
    }
}

static void AssertJson(string expected, Shape shape)
{
    string actual = JsonSerializer.Serialize(shape);
    if (!JsonEquivalent(expected, actual))
    {
        throw new FixtureException($"Expected JSON {expected} but was {actual}.");
    }
}

static void AssertThrows<TException>(Action action)
    where TException : Exception
{
    try
    {
        action();
    }
    catch (TException)
    {
        return;
    }
    catch (Exception other) when (other is not FixtureException)
    {
        // STJ wraps converter exceptions; unwrap to find the real cause.
        if (UnwrapsTo<TException>(other))
        {
            return;
        }

        throw new FixtureException($"Expected {typeof(TException).Name} but caught {other.GetType().Name}: {other.Message}.");
    }

    throw new FixtureException($"Expected {typeof(TException).Name} but nothing was thrown.");
}

static bool UnwrapsTo<TException>(Exception ex)
    where TException : Exception
{
    Exception? current = ex;
    while (current is not null)
    {
        if (current is TException)
        {
            return true;
        }

        current = current.InnerException;
    }

    return false;
}

static void AssertMerge(Shape existing, Shape incoming, Shape expected)
{
    bool ok = ShapeContractCompatibility.TryMergeContracts(existing, incoming, out Shape? merged);
    if (!ok)
    {
        throw new FixtureException($"Expected merge to {expected.DescribeContract()} but it conflicted.");
    }

    if (!expected.Equals(merged))
    {
        throw new FixtureException($"Expected merge {expected.DescribeContract()} but was {merged!.DescribeContract()}.");
    }
}

static void AssertConflict(Shape existing, Shape incoming)
{
    bool ok = ShapeContractCompatibility.TryMergeContracts(existing, incoming, out Shape? merged);
    if (ok)
    {
        throw new FixtureException($"Expected a conflict but merged to {merged!.DescribeContract()}.");
    }

    if (merged is not null)
    {
        throw new FixtureException("Expected merged=null on conflict.");
    }
}

static void AssertOk<T>(T expected, ConvertResult<T> result)
{
    if (!result.Ok)
    {
        throw new FixtureException($"Expected ok({Describe(expected)}) but was err: {result.Error}.");
    }

    if (!Equals(expected, result.Value))
    {
        throw new FixtureException($"Expected ok({Describe(expected)}) but was ok({Describe(result.Value)}).");
    }
}

static void AssertOkNan(ConvertResult<double> result)
{
    if (!result.Ok)
    {
        throw new FixtureException($"Expected ok(NaN) but was err: {result.Error}.");
    }

    if (!double.IsNaN(result.Value))
    {
        throw new FixtureException($"Expected ok(NaN) but was ok({result.Value}).");
    }
}

static void AssertErr<T>(ConvertResult<T> result)
{
    if (result.Ok)
    {
        throw new FixtureException($"Expected err but was ok({Describe(result.Value)}).");
    }
}

static void AssertArrayEqual(object?[] expected, object? actual)
{
    if (actual is not IReadOnlyList<object?> list)
    {
        throw new FixtureException($"Expected an array but was {Describe(actual)}.");
    }

    if (list.Count != expected.Length)
    {
        throw new FixtureException($"Expected array length {expected.Length} but was {list.Count}.");
    }

    for (int i = 0; i < expected.Length; i++)
    {
        if (!Equals(expected[i], list[i]))
        {
            throw new FixtureException($"Array index {i}: expected {Describe(expected[i])} but was {Describe(list[i])}.");
        }
    }
}

static void AssertObjectEqual(Dictionary<string, object?> expected, object? actual)
{
    if (actual is not IReadOnlyDictionary<string, object?> map)
    {
        throw new FixtureException($"Expected an object but was {Describe(actual)}.");
    }

    if (map.Count != expected.Count)
    {
        throw new FixtureException($"Expected {expected.Count} keys but was {map.Count} ({string.Join(",", map.Keys)}).");
    }

    foreach ((string key, object? value) in expected)
    {
        if (!map.TryGetValue(key, out object? actualValue))
        {
            throw new FixtureException($"Expected key \"{key}\" was missing.");
        }

        if (!Equals(value, actualValue))
        {
            throw new FixtureException($"Key \"{key}\": expected {Describe(value)} but was {Describe(actualValue)}.");
        }
    }
}

static bool JsonEquivalent(string expected, string actual)
{
    using JsonDocument expectedDoc = JsonDocument.Parse(expected);
    using JsonDocument actualDoc = JsonDocument.Parse(actual);
    return JsonElementsEqual(expectedDoc.RootElement, actualDoc.RootElement);
}

static bool JsonElementsEqual(JsonElement a, JsonElement b)
{
    if (a.ValueKind != b.ValueKind)
    {
        return false;
    }

    switch (a.ValueKind)
    {
        case JsonValueKind.Object:
            Dictionary<string, JsonElement> aProps = a.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);
            Dictionary<string, JsonElement> bProps = b.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);
            if (aProps.Count != bProps.Count)
            {
                return false;
            }

            foreach ((string name, JsonElement value) in aProps)
            {
                if (!bProps.TryGetValue(name, out JsonElement other) || !JsonElementsEqual(value, other))
                {
                    return false;
                }
            }

            return true;
        case JsonValueKind.Array:
            if (a.GetArrayLength() != b.GetArrayLength())
            {
                return false;
            }

            JsonElement.ArrayEnumerator aItems = a.EnumerateArray();
            JsonElement.ArrayEnumerator bItems = b.EnumerateArray();
            while (aItems.MoveNext() && bItems.MoveNext())
            {
                if (!JsonElementsEqual(aItems.Current, bItems.Current))
                {
                    return false;
                }
            }

            return true;
        case JsonValueKind.String:
            return a.GetString() == b.GetString();
        case JsonValueKind.Number:
            return a.GetRawText() == b.GetRawText();
        case JsonValueKind.True:
        case JsonValueKind.False:
        case JsonValueKind.Null:
        case JsonValueKind.Undefined:
            return true;
        default:
            return a.GetRawText() == b.GetRawText();
    }
}

static string Describe(object? value) =>
    value switch
    {
        null => "null",
        string s => $"\"{s}\"",
        bool b => b ? "true" : "false",
        double d => d.ToString(CultureInfo.InvariantCulture),
        _ => $"{value} ({value.GetType().Name})",
    };

internal enum SampleEnum
{
    First,
    Second,
}

internal sealed class UnclassifiablePoco
{
    public int Value { get; set; }
}

// A type implementing two distinct IEnumerable<T> — ambiguous item type -> Any.
internal sealed class DualEnumerable : IEnumerable<int>, IEnumerable<string>
{
    IEnumerator<int> IEnumerable<int>.GetEnumerator() => Enumerable.Empty<int>().GetEnumerator();

    IEnumerator<string> IEnumerable<string>.GetEnumerator() => Enumerable.Empty<string>().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Enumerable.Empty<int>().GetEnumerator();
}

internal sealed class FixtureException(string message) : Exception(message);

internal sealed class FixtureRunner
{
    private int _passed;
    private int _total;
    private readonly List<string> _failures = [];

    internal void Run(string id, Action fixture)
    {
        _total++;
        try
        {
            fixture();
            _passed++;
        }
        catch (FixtureException ex)
        {
            _failures.Add($"FAIL {id}: {ex.Message}");
        }
        catch (Exception ex)
        {
            _failures.Add($"FAIL {id}: unexpected {ex.GetType().Name}: {ex.Message}");
        }
    }

    internal int Report()
    {
        foreach (string failure in _failures)
        {
            Console.WriteLine(failure);
        }

        Console.WriteLine($"Fixtures: {_passed}/{_total} passed.");

        if (_passed == _total)
        {
            Console.WriteLine("ALL GREEN");
            return 0;
        }

        return 1;
    }
}
