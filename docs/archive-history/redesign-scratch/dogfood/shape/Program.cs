// Spec-only fixture runner. Every assertion is one named fixture from
// docs/design/redesign/scaffold/Shape.md §6 (A/B/C/D/E) + the Module 1 F-Shape-* rows
// in _fixtures.md. Exits non-zero if any fixture fails. No framework source was read.

using System.Text.Json;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Runtime;

var runner = new FixtureRunner();

// ===========================================================================
// A. CLR inference (FromClrType / FromValue) — §6 A + F-Shape-* rows
// ===========================================================================

runner.Check("clr_string_is_string", () =>
    Shape.FromClrType(typeof(string)).Kind == "string");

runner.Check("clr_int_is_number", () =>
    Shape.FromClrType(typeof(int)).Kind == "number" &&
    Shape.FromClrType(typeof(byte)).Kind == "number" &&
    Shape.FromClrType(typeof(long)).Kind == "number" &&
    Shape.FromClrType(typeof(decimal)).Kind == "number" &&
    Shape.FromClrType(typeof(double)).Kind == "number");

runner.Check("clr_bool_is_boolean", () =>
    Shape.FromClrType(typeof(bool)).Kind == "boolean");

runner.Check("clr_datetime_is_date", () =>
    Shape.FromClrType(typeof(DateTime)).Kind == "date" &&
    Shape.FromClrType(typeof(DateTimeOffset)).Kind == "date" &&
    Shape.FromClrType(typeof(DateOnly)).Kind == "date");

runner.Check("clr_nullable_int_is_nullable_number", () =>
    Shape.FromClrType(typeof(int?)) == Shape.Nullable(Shape.Number));

runner.Check("clr_guid_is_string", () =>
    Shape.FromClrType(typeof(Guid)).Kind == "string" &&
    Shape.FromClrType(typeof(TimeSpan)).Kind == "string" &&
    Shape.FromClrType(typeof(TimeOnly)).Kind == "string");

runner.Check("clr_enum_is_string", () =>
    Shape.FromClrType(typeof(SampleEnum)).Kind == "string");

runner.Check("clr_list_of_t_is_array_of_t", () =>
    Shape.FromClrType(typeof(List<int>)) == Shape.ArrayOf(Shape.Number) &&
    Shape.FromClrType(typeof(int[])) == Shape.ArrayOf(Shape.Number) &&
    Shape.FromClrType(typeof(IEnumerable<string>)) == Shape.ArrayOf(Shape.String));

runner.Check("clr_dictionary_is_any", () =>
    Shape.FromClrType(typeof(Dictionary<string, int>)).Kind == "any");

runner.Check("clr_unknown_is_any", () =>
    Shape.FromClrType(typeof(SamplePoco)).Kind == "any");

runner.Check("from_value_null_is_none", () =>
    Shape.FromValue(null).Kind == "none");

runner.Check("collection_item_shape_or_none_for_non_collection", () =>
    Shape.CollectionItemShapeOrNone(typeof(int)).Kind == "none" &&
    Shape.CollectionItemShapeOrNone(typeof(List<int>)) == Shape.Number);

// ===========================================================================
// B. Construction invariants — §6 B
// ===========================================================================

runner.CheckThrows<ArgumentException>("array_of_none_is_rejected", () =>
    Shape.ArrayOf(Shape.None));

runner.CheckThrows<ArgumentException>("nullable_of_none_is_rejected", () =>
    Shape.Nullable(Shape.None));

runner.Check("nullable_scalar_is_scalar", () =>
    Shape.Nullable(Shape.String).IsScalar);

runner.Check("object_is_not_scalar", () =>
    !Shape.ObjectOf(new() { ["a"] = Shape.String }).IsScalar);

// ===========================================================================
// C. Serialization (write-only, matches TS union) — §6 C
// ===========================================================================

runner.Check("scalar_serializes_kind_only", () =>
    Json(Shape.String) == "{\"kind\":\"string\"}");

runner.Check("array_serializes_item", () =>
    Json(Shape.ArrayOf(Shape.Number)) == "{\"kind\":\"array\",\"item\":{\"kind\":\"number\"}}");

runner.Check("nullable_serializes_inner", () =>
    Json(Shape.Nullable(Shape.Date)) == "{\"kind\":\"nullable\",\"inner\":{\"kind\":\"date\"}}");

runner.Check("object_of_fields_serializes_closed", () =>
    Json(Shape.ObjectOf(new() { ["a"] = Shape.String }))
        == "{\"kind\":\"object\",\"fields\":{\"a\":{\"kind\":\"string\"}},\"additional\":false}");

runner.Check("open_object_serializes_additional_true", () =>
    Json(Shape.OpenObject()) == "{\"kind\":\"object\",\"fields\":{},\"additional\":true}");

runner.CheckThrows<NotSupportedException>("read_is_not_supported", () =>
{
    var reader = new Utf8JsonReader("{\"kind\":\"string\"}"u8);
    new ShapeJsonConverter().Read(ref reader, typeof(Shape), JsonSerializerOptions.Default);
});

runner.Check("describe_contract_nested", () =>
    Shape.ArrayOf(Shape.ObjectOf(new() { ["a"] = Shape.String })).DescribeContract()
        == "array<object{a:string}>");

// ===========================================================================
// D. Equality + algebra (ShapeContractCompatibility) — §6 D
// ===========================================================================

runner.Check("equal_array_shapes_are_equal", () =>
    Shape.ArrayOf(Shape.Number) == Shape.ArrayOf(Shape.Number) &&
    Shape.ArrayOf(Shape.Number).GetHashCode() == Shape.ArrayOf(Shape.Number).GetHashCode());

runner.Check("different_object_fields_are_unequal", () =>
    Shape.ObjectOf(new() { ["a"] = Shape.String }) != Shape.ObjectOf(new() { ["a"] = Shape.Number }));

runner.Check("merge_equal_is_self", () =>
    ShapeContractCompatibility.TryMergeContracts(Shape.String, Shape.String, out var m) && m == Shape.String);

runner.Check("merge_any_yields_other", () =>
    ShapeContractCompatibility.TryMergeContracts(Shape.Any, Shape.Number, out var m) && m == Shape.Number);

runner.Check("merge_none_conflicts", () =>
    !ShapeContractCompatibility.TryMergeContracts(Shape.None, Shape.String, out var m) && m is null);

runner.Check("merge_nullable_absorbs_inner", () =>
    ShapeContractCompatibility.TryMergeContracts(Shape.Nullable(Shape.String), Shape.String, out var m) &&
    m == Shape.Nullable(Shape.String));

runner.Check("merge_arrays_recurse", () =>
    ShapeContractCompatibility.TryMergeContracts(Shape.ArrayOf(Shape.Any), Shape.ArrayOf(Shape.String), out var m) &&
    m == Shape.ArrayOf(Shape.String));

runner.Check("merge_objects_union_fields", () =>
    ShapeContractCompatibility.TryMergeContracts(
        Shape.ObjectOf(new() { ["a"] = Shape.String }),
        Shape.ObjectOf(new() { ["b"] = Shape.Number }),
        out var m) &&
    m == Shape.ObjectOf(new() { ["a"] = Shape.String, ["b"] = Shape.Number }));

runner.Check("merge_field_conflict_is_conflict", () =>
    !ShapeContractCompatibility.TryMergeContracts(
        Shape.ObjectOf(new() { ["a"] = Shape.String }),
        Shape.ObjectOf(new() { ["a"] = Shape.Number }),
        out var m) && m is null);

runner.Check("accept_any_either_side", () =>
    ShapeContractCompatibility.CanAccept(Shape.Any, Shape.ObjectOf(new() { ["a"] = Shape.String })) &&
    ShapeContractCompatibility.CanAccept(Shape.String, Shape.Any));

runner.Check("reject_none_either_side", () =>
    !ShapeContractCompatibility.CanAccept(Shape.None, Shape.String) &&
    !ShapeContractCompatibility.CanAccept(Shape.String, Shape.None));

runner.Check("accept_open_object", () =>
    ShapeContractCompatibility.CanAccept(Shape.OpenObject(), Shape.ObjectOf(new() { ["a"] = Shape.String })));

runner.Check("reject_missing_required_field", () =>
    !ShapeContractCompatibility.CanAccept(Shape.ObjectOf(new() { ["a"] = Shape.String }), Shape.OpenObject()));

// extra equality/accept coverage implied by §6 D row text
runner.Check("accept_equal_self", () =>
    ShapeContractCompatibility.CanAccept(Shape.String, Shape.String));

runner.Check("accept_array_recurse", () =>
    ShapeContractCompatibility.CanAccept(Shape.ArrayOf(Shape.Any), Shape.ArrayOf(Shape.String)));

runner.Check("accept_nullable_either_side", () =>
    ShapeContractCompatibility.CanAccept(Shape.Nullable(Shape.String), Shape.String) &&
    ShapeContractCompatibility.CanAccept(Shape.String, Shape.Nullable(Shape.String)));

// ===========================================================================
// E. Runtime conversion (applyShape / convertByShape / formatForWire) — §6 E
//    Runtime tags are parsed straight from the C# Shape's JSON ("same bytes everywhere").
// ===========================================================================

RuntimeShapeTag Tag(Shape s) => RuntimeShapeTag.FromShapeJson(Json(s));

runner.Check("apply_string_coerces_number", () =>
    Equals(ShapeConverter.ApplyShape(5d, RuntimeShapeTag.String), "5"));

runner.Check("apply_number_parses_text", () =>
    Equals(ShapeConverter.ApplyShape("3", RuntimeShapeTag.Number), 3d));

runner.Check("apply_boolean_truthy_text", () =>
    Equals(ShapeConverter.ApplyShape("false", RuntimeShapeTag.Boolean), false));

runner.Check("apply_date_only_is_local_midnight", () =>
{
    var result = ShapeConverter.ApplyShape("2026-01-15", RuntimeShapeTag.Date);
    var expected = ShapeConverter.EpochMs(DateTime.SpecifyKind(new DateTime(2026, 1, 15), DateTimeKind.Local));
    return result is double ms && ms == expected;
});

runner.Check("apply_array_recurses_items", () =>
{
    var result = ShapeConverter.ApplyShape(
        new object?[] { "1", "2" },
        Tag(Shape.ArrayOf(Shape.Number)));
    return result is object?[] arr && arr.Length == 2 && Equals(arr[0], 1d) && Equals(arr[1], 2d);
});

runner.Check("apply_object_keeps_open_extras", () =>
{
    var src = new Dictionary<string, object?> { ["a"] = 1d, ["x"] = 2d };
    var result = ShapeConverter.ApplyShape(src, RuntimeShapeTag.OpenObject());
    return result is Dictionary<string, object?> obj &&
           obj.Count == 2 && Equals(obj["a"], 1d) && Equals(obj["x"], 2d);
});

runner.Check("apply_nullable_missing_is_null", () =>
    ShapeConverter.ApplyShape(null, RuntimeShapeTag.Nullable(RuntimeShapeTag.String)) is null);

runner.Check("apply_raw_is_identity", () =>
{
    var v = new object();
    return ReferenceEquals(ShapeConverter.ApplyShape(v, RuntimeShapeTag.Raw), v) &&
           ReferenceEquals(ShapeConverter.ApplyShape(v, RuntimeShapeTag.Any), v) &&
           ReferenceEquals(ShapeConverter.ApplyShape(v, RuntimeShapeTag.None), v);
});

runner.Check("convert_object_into_scalar_is_err", () =>
{
    var r = ShapeConverter.ConvertByShape(new Dictionary<string, object?>(), RuntimeShapeTag.String);
    return !r.Ok;
});

runner.Check("format_date_to_iso", () =>
{
    var epochMs = (double)DateTimeOffset.Parse("2026-01-15T10:30:00Z").ToUnixTimeMilliseconds();
    var wire = RuntimeShape.From(RuntimeShapeTag.Date).FormatForWire(epochMs);
    return wire is string s && s == "2026-01-15T10:30:00.000Z";
});

runner.Check("format_nullable_unwraps", () =>
{
    var epochMs = (double)DateTimeOffset.Parse("2026-01-15T10:30:00Z").ToUnixTimeMilliseconds();
    var wire = RuntimeShape.From(RuntimeShapeTag.Nullable(RuntimeShapeTag.Date)).FormatForWire(epochMs);
    return wire is string s && s == "2026-01-15T10:30:00.000Z";
});

runner.Check("format_unshaped_passthrough", () =>
{
    var v = new object();
    return ReferenceEquals(RuntimeShape.From(RuntimeShapeTag.None).FormatForWire(v), v);
});

runner.Check("runtime_shape_item_of_array", () =>
{
    var item = RuntimeShape.From(Tag(Shape.ArrayOf(Shape.Number))).Item();
    return item.IsDeclared && item.PlanShape.Kind == "number";
});

// ===========================================================================
// F-Shape-* (Module 1 rows) that add behavior beyond §6
// ===========================================================================

runner.Check("F-Shape-Once_value_is_shaped_exactly_once", () =>
{
    // date number → ISO once on egress; a second formatForWire on the ISO string is a no-op
    // (string under date is not re-derived because egress already produced wire form).
    var epochMs = (double)DateTimeOffset.Parse("2026-01-15T00:00:00Z").ToUnixTimeMilliseconds();
    var once = RuntimeShape.From(RuntimeShapeTag.Date).FormatForWire(epochMs);
    // once is a string; feeding the string back is identity (already wire-ready, not a number)
    var twice = RuntimeShape.From(RuntimeShapeTag.Date).FormatForWire(once);
    return Equals(once, twice);
});

runner.Check("F-Shape-Number_non_finite_text_is_err", () =>
{
    var r = ShapeConverter.ConvertByShape("not-a-number", RuntimeShapeTag.Number);
    return !r.Ok;
});

return runner.Report();

// ---------------------------------------------------------------------------

static string Json(Shape shape) =>
    JsonSerializer.Serialize(shape, new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

enum SampleEnum { A, B }
sealed class SamplePoco { public int X { get; set; } }

sealed class FixtureRunner
{
    private int _total;
    private int _passed;
    private readonly List<string> _failures = new();

    internal void Check(string id, Func<bool> assertion)
    {
        _total++;
        try
        {
            if (assertion())
            {
                _passed++;
                Console.WriteLine($"PASS  {id}");
            }
            else
            {
                _failures.Add(id);
                Console.WriteLine($"FAIL  {id}  (assertion returned false)");
            }
        }
        catch (Exception ex)
        {
            _failures.Add(id);
            Console.WriteLine($"FAIL  {id}  (threw {ex.GetType().Name}: {ex.Message})");
        }
    }

    internal void CheckThrows<TException>(string id, Action action) where TException : Exception
    {
        _total++;
        try
        {
            action();
            _failures.Add(id);
            Console.WriteLine($"FAIL  {id}  (expected {typeof(TException).Name}, nothing thrown)");
        }
        catch (TException)
        {
            _passed++;
            Console.WriteLine($"PASS  {id}");
        }
        catch (Exception ex)
        {
            _failures.Add(id);
            Console.WriteLine($"FAIL  {id}  (expected {typeof(TException).Name}, got {ex.GetType().Name})");
        }
    }

    internal int Report()
    {
        Console.WriteLine();
        Console.WriteLine($"Fixtures: {_passed}/{_total} passed.");
        if (_failures.Count > 0)
        {
            Console.WriteLine("Failures: " + string.Join(", ", _failures));
            return 1;
        }
        Console.WriteLine("ALL GREEN");
        return 0;
    }
}
