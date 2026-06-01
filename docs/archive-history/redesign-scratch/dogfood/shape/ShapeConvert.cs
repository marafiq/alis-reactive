// Spec-only C# port of the TS runtime conversion engine.
// Source: Shape.md §2e (ShapeConverter), §2f (RuntimeShape), §5c, §5d, §6 E fixtures
// + the Shape fixtures (Module 1) in _fixtures.md. No framework source read.
//
// The spec defines applyShape / convertByShape / formatForWire in TypeScript over the
// GENERATED Shape union (switch on shape.kind). This dogfood is a .NET proof, so the same
// engine is ported to C# faithfully:
//   - ConvertResult<T>      ≙ the TS discriminated { ok, value } | { ok:false, error }
//   - RuntimeShapeTag       ≙ the generated `Shape` union variant the runtime switches on
//     (kind + optional item / inner / fields / additional), built from the C# Shape so the
//     "same bytes everywhere" invariant holds.

using System.Globalization;
using System.Text.Json;

namespace Alis.Reactive.Runtime;

/// <summary>Discriminated result — caller MUST check Ok before using Value.</summary>
internal readonly struct ConvertResult<T>
{
    internal bool Ok { get; }
    internal T? Value { get; }
    internal string? Error { get; }

    private ConvertResult(bool ok, T? value, string? error) { Ok = ok; Value = value; Error = error; }
    internal static ConvertResult<T> OkResult(T value) => new(true, value, null);
    internal static ConvertResult<T> Err(string error) => new(false, default, error);
}

/// <summary>
/// The generated `Shape` union as the runtime sees it: a kind token plus the body the
/// conversion engine switches on. Mirrors the 10-variant TS discriminated union.
/// </summary>
internal sealed class RuntimeShapeTag
{
    internal string Kind { get; }
    internal RuntimeShapeTag? Item { get; }     // array
    internal RuntimeShapeTag? Inner { get; }    // nullable
    internal IReadOnlyDictionary<string, RuntimeShapeTag>? Fields { get; } // object
    internal bool Additional { get; }           // object

    internal RuntimeShapeTag(
        string kind,
        RuntimeShapeTag? item = null,
        RuntimeShapeTag? inner = null,
        IReadOnlyDictionary<string, RuntimeShapeTag>? fields = null,
        bool additional = false)
    {
        Kind = kind;
        Item = item;
        Inner = inner;
        Fields = fields;
        Additional = additional;
    }

    internal static readonly RuntimeShapeTag String  = new("string");
    internal static readonly RuntimeShapeTag Number  = new("number");
    internal static readonly RuntimeShapeTag Boolean = new("boolean");
    internal static readonly RuntimeShapeTag Date    = new("date");
    internal static readonly RuntimeShapeTag Raw     = new("raw");
    internal static readonly RuntimeShapeTag Any     = new("any");
    internal static readonly RuntimeShapeTag None    = new("none");

    internal static RuntimeShapeTag Array(RuntimeShapeTag item) => new("array", item: item);
    internal static RuntimeShapeTag Nullable(RuntimeShapeTag inner) => new("nullable", inner: inner);
    internal static RuntimeShapeTag Object(IReadOnlyDictionary<string, RuntimeShapeTag> fields, bool additional)
        => new("object", fields: fields, additional: additional);
    internal static RuntimeShapeTag OpenObject()
        => new("object", fields: new Dictionary<string, RuntimeShapeTag>(), additional: true);

    /// <summary>Parse the runtime tag straight from the C# Shape's serialized JSON — proves the
    /// runtime reads the exact bytes the domain emits ("same bytes everywhere", shape-once).</summary>
    internal static RuntimeShapeTag FromShapeJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return FromElement(doc.RootElement);
    }

    private static RuntimeShapeTag FromElement(JsonElement el)
    {
        var kind = el.GetProperty("kind").GetString()!;
        switch (kind)
        {
            case "array":
                return Array(FromElement(el.GetProperty("item")));
            case "nullable":
                return Nullable(FromElement(el.GetProperty("inner")));
            case "object":
                var fields = new Dictionary<string, RuntimeShapeTag>();
                foreach (var p in el.GetProperty("fields").EnumerateObject())
                    fields[p.Name] = FromElement(p.Value);
                var additional = el.GetProperty("additional").GetBoolean();
                return Object(fields, additional);
            default:
                return new RuntimeShapeTag(kind);
        }
    }
}

/// <summary>The ONE conversion engine. applyShape / convertByShape + total scalar coercions.</summary>
internal static class ShapeConverter
{
    private static bool IsMissing(object? v) => v is null;

    // --- scalar coercions (each total, each returns ConvertResult) ---

    internal static ConvertResult<string> ToStringValue(object? value)
    {
        if (IsMissing(value)) return ConvertResult<string>.OkResult("");
        if (value is string s) return ConvertResult<string>.OkResult(s);
        if (value is bool b) return ConvertResult<string>.OkResult(b ? "true" : "false");
        if (IsPlainObjectOrArray(value)) return ConvertResult<string>.Err("cannot coerce object/array to string");
        return ConvertResult<string>.OkResult(JsToString(value!));
    }

    internal static ConvertResult<double> ToNumber(object? value)
    {
        if (IsMissing(value)) return ConvertResult<double>.OkResult(0);
        if (value is bool) return ConvertResult<double>.Err("boolean is not a number");
        if (value is double d) return double.IsFinite(d) ? ConvertResult<double>.OkResult(d) : ConvertResult<double>.Err("non-finite");
        if (TryAsNumber(value, out var n)) return double.IsFinite(n) ? ConvertResult<double>.OkResult(n) : ConvertResult<double>.Err("non-finite");
        if (value is string str)
        {
            var trimmed = str.Trim();
            if (trimmed.Length == 0) return ConvertResult<double>.OkResult(0); // JS Number("") === 0
            if (double.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) && double.IsFinite(parsed))
                return ConvertResult<double>.OkResult(parsed);
            return ConvertResult<double>.Err("non-finite");
        }
        return ConvertResult<double>.Err("not a number");
    }

    internal static ConvertResult<bool> ToBoolean(object? value)
    {
        if (IsMissing(value)) return ConvertResult<bool>.OkResult(false);
        if (value is bool b) return ConvertResult<bool>.OkResult(b);
        if (value is string s)
        {
            // explicit falsy text per spec: "" / "false" / "0"
            if (s.Length == 0 || s == "false" || s == "0") return ConvertResult<bool>.OkResult(false);
            return ConvertResult<bool>.OkResult(true);
        }
        if (TryAsNumber(value, out var n))
            return ConvertResult<bool>.OkResult(!(n == 0 || double.IsNaN(n))); // 0 / NaN → false
        return ConvertResult<bool>.OkResult(true);
    }

    /// <summary>Date → epoch ms. A date-only "YYYY-MM-DD" string anchors to LOCAL midnight.</summary>
    internal static ConvertResult<double> ToDate(object? value)
    {
        if (IsMissing(value)) return ConvertResult<double>.OkResult(0);
        if (TryAsNumber(value, out var n)) return ConvertResult<double>.OkResult(n); // already epoch ms
        if (value is string s)
        {
            var trimmed = s.Trim();
            if (IsDateOnly(trimmed))
            {
                var date = DateTime.ParseExact(trimmed, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None);
                var localMidnight = DateTime.SpecifyKind(date, DateTimeKind.Local);
                return ConvertResult<double>.OkResult(EpochMs(localMidnight));
            }
            if (DateTimeOffset.TryParse(trimmed, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto))
                return ConvertResult<double>.OkResult(dto.ToUnixTimeMilliseconds());
            return ConvertResult<double>.Err("unparseable date");
        }
        return ConvertResult<double>.Err("not a date");
    }

    internal static ConvertResult<object?[]> ToArray(object? value)
    {
        if (IsMissing(value)) return ConvertResult<object?[]>.OkResult(System.Array.Empty<object?>());
        if (value is string es && es.Length == 0) return ConvertResult<object?[]>.OkResult(System.Array.Empty<object?>());
        if (value is object?[] arr) return ConvertResult<object?[]>.OkResult(arr);
        if (value is IEnumerable<object?> en && value is not string)
            return ConvertResult<object?[]>.OkResult(en.ToArray());
        if (IsPlainObject(value)) return ConvertResult<object?[]>.Err("cannot coerce object to array");
        return ConvertResult<object?[]>.OkResult(new[] { value }); // scalar → [v]
    }

    internal static ConvertResult<Dictionary<string, object?>> ToPlainObject(object? value)
    {
        if (value is Dictionary<string, object?> map) return ConvertResult<Dictionary<string, object?>>.OkResult(map);
        return ConvertResult<Dictionary<string, object?>>.Err("not a plain object");
    }

    // --- the two entry points ---

    internal static object? ApplyShape(object? value, RuntimeShapeTag shape)
    {
        switch (shape.Kind)
        {
            case "string":  { var r = ToStringValue(value); return r.Ok ? r.Value : value; }
            case "number":  { var r = ToNumber(value);      return r.Ok ? r.Value : value; }
            case "boolean": { var r = ToBoolean(value);     return r.Ok ? r.Value : value; }
            case "date":    { var r = ToDate(value);        return r.Ok ? r.Value : value; }
            case "array":   return ApplyArrayShape(value, shape);
            case "object":  return ApplyObjectShape(value, shape);
            case "nullable":return IsMissing(value) ? null : ApplyShape(value, shape.Inner!);
            case "raw":
            case "any":
            case "none":    return value; // identity
            default:        throw new InvalidOperationException($"[alis] unknown shape kind: \"{shape.Kind}\"");
        }
    }

    internal static ConvertResult<object?> ConvertByShape(object? value, RuntimeShapeTag shape)
    {
        switch (shape.Kind)
        {
            case "string":  { var r = ToStringValue(value); return Lift(r); }
            case "number":  { var r = ToNumber(value);      return Lift(r); }
            case "boolean": { var r = ToBoolean(value);     return Lift(r); }
            case "date":    { var r = ToDate(value);        return Lift(r); }
            case "array":
            {
                var r = ToArray(value);
                if (!r.Ok) return ConvertResult<object?>.Err(r.Error!);
                var items = new object?[r.Value!.Length];
                for (var i = 0; i < r.Value.Length; i++)
                {
                    var ir = ConvertByShape(r.Value[i], shape.Item!);
                    if (!ir.Ok) return ConvertResult<object?>.Err(ir.Error!);
                    items[i] = ir.Value;
                }
                return ConvertResult<object?>.OkResult(items);
            }
            case "object":
            {
                var r = ToPlainObject(value);
                if (!r.Ok) return ConvertResult<object?>.Err(r.Error!);
                return ConvertResult<object?>.OkResult(ConvertObject(r.Value!, shape));
            }
            case "nullable":
                return IsMissing(value)
                    ? ConvertResult<object?>.OkResult(null)
                    : ConvertByShape(value, shape.Inner!);
            case "raw":
            case "any":
            case "none":
                return ConvertResult<object?>.OkResult(value);
            default:
                throw new InvalidOperationException($"[alis] unknown shape kind: \"{shape.Kind}\"");
        }
    }

    // --- array / object recursion for applyShape (never throws) ---

    private static object? ApplyArrayShape(object? value, RuntimeShapeTag shape)
    {
        var r = ToArray(value);
        if (!r.Ok) return value;
        var items = new object?[r.Value!.Length];
        for (var i = 0; i < r.Value.Length; i++)
            items[i] = ApplyShape(r.Value[i], shape.Item!);
        return items;
    }

    private static object? ApplyObjectShape(object? value, RuntimeShapeTag shape)
    {
        var r = ToPlainObject(value);
        if (!r.Ok) return value;
        return ConvertObject(r.Value!, shape);
    }

    private static Dictionary<string, object?> ConvertObject(Dictionary<string, object?> source, RuntimeShapeTag shape)
    {
        var result = new Dictionary<string, object?>();
        var fields = shape.Fields ?? new Dictionary<string, RuntimeShapeTag>();

        foreach (var (name, fieldShape) in fields)
            result[name] = ApplyShape(source.TryGetValue(name, out var v) ? v : null, fieldShape);

        // open object keeps extra (undeclared) keys verbatim
        if (shape.Additional)
            foreach (var (k, v) in source)
                if (!result.ContainsKey(k)) result[k] = v;

        return result;
    }

    // --- helpers ---

    private static ConvertResult<object?> Lift<T>(ConvertResult<T> r)
        => r.Ok ? ConvertResult<object?>.OkResult(r.Value) : ConvertResult<object?>.Err(r.Error!);

    private static bool IsDateOnly(string s)
        => s.Length == 10 && s[4] == '-' && s[7] == '-' &&
           DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

    internal static double EpochMs(DateTime localOrUtc)
    {
        var dto = new DateTimeOffset(localOrUtc);
        return dto.ToUnixTimeMilliseconds();
    }

    private static string JsToString(object value) => value switch
    {
        double d => d == Math.Floor(d) && double.IsFinite(d)
            ? ((long)d).ToString(CultureInfo.InvariantCulture)
            : d.ToString(CultureInfo.InvariantCulture),
        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? "",
    };

    private static bool TryAsNumber(object? value, out double n)
    {
        switch (value)
        {
            case double d: n = d; return true;
            case float f: n = f; return true;
            case int i: n = i; return true;
            case long l: n = l; return true;
            case short sh: n = sh; return true;
            case byte by: n = by; return true;
            case decimal de: n = (double)de; return true;
            default: n = 0; return false;
        }
    }

    private static bool IsPlainObject(object? value)
        => value is Dictionary<string, object?>;

    private static bool IsPlainObjectOrArray(object? value)
        => value is Dictionary<string, object?> || value is object?[] ||
           (value is IEnumerable<object?> && value is not string);
}

/// <summary>The shape-once wrapper + egress (RuntimeShape).</summary>
internal sealed class RuntimeShape
{
    private static readonly RuntimeShapeTag UnshapedTag = RuntimeShapeTag.None;
    private readonly RuntimeShapeTag _shape;

    private RuntimeShape(RuntimeShapeTag shape) => _shape = shape;

    internal static RuntimeShape From(RuntimeShapeTag shape) => new(shape);
    internal static RuntimeShape Unshaped() => new(UnshapedTag);

    internal RuntimeShapeTag PlanShape => _shape;
    internal bool IsDeclared => _shape.Kind != "none";

    internal RuntimeShape Item()
        => _shape.Kind == "array" ? From(_shape.Item!) : Unshaped(); // fixture: runtime_shape_item_of_array

    internal RuntimeShapeTag OrDeclared(RuntimeShapeTag declared) => IsDeclared ? _shape : declared;

    internal object? Apply(object? value) => IsDeclared ? ShapeConverter.ApplyShape(value, _shape) : value;
    internal object?[] ApplyEach(object?[] items)
    {
        if (!IsDeclared) return items;
        var result = new object?[items.Length];
        for (var i = 0; i < items.Length; i++) result[i] = ShapeConverter.ApplyShape(items[i], _shape);
        return result;
    }
    internal ConvertResult<object?> Convert(object? value) => ShapeConverter.ConvertByShape(value, _shape);

    /// <summary>SHAPE-ONCE egress: convert a runtime value to its wire form exactly once.
    /// undeclared(none) → passthrough; nullable → recurse inner; date+finite number → ISO string;
    /// everything else is already wire-ready.</summary>
    internal object? FormatForWire(object? value)
    {
        if (!IsDeclared) return value;                               // fixture: format_unshaped_passthrough
        if (_shape.Kind == "nullable")
        {
            if (value is null) return null;
            return RuntimeShape.From(_shape.Inner!).FormatForWire(value); // fixture: format_nullable_unwraps
        }
        if (_shape.Kind == "date" && value is double ms && double.IsFinite(ms))
            return DateTimeOffset.FromUnixTimeMilliseconds((long)ms)
                .ToUniversalTime()
                .ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture); // fixture: format_date_to_iso
        return value;
    }
}
