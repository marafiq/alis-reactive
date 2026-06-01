using System.Globalization;

namespace Alis.Reactive.PlanModel;

/// <summary>
/// A faithful C# port of the TS <c>core/shape-convert.ts</c> engine
/// (applyShape / convertByShape / toX). Runtime coercion is JS-semantics-authoritative,
/// so this port mirrors the JavaScript result: number→string is <c>String(n)</c>,
/// string→number is <c>Number(s)</c>, date→wire is <c>Date.toISOString()</c>.
///
/// Values flow as plain CLR objects modelling JS runtime values:
///   missing  → <c>null</c> (covers JS null/undefined)
///   number   → <c>double</c> (including <c>double.NaN</c>)
///   boolean  → <c>bool</c>
///   string   → <c>string</c>
///   date     → <see cref="JsDate"/> (wraps epoch ms)
///   array    → <c>IReadOnlyList&lt;object?&gt;</c> (an <c>object?[]</c> or <c>List&lt;object?&gt;</c>)
///   object   → <c>IReadOnlyDictionary&lt;string, object?&gt;</c> (a plain JS object)
/// </summary>
internal static class ShapeConvert
{
    internal readonly record struct ConvertResult<T>(bool Ok, T Value, string Error)
    {
        internal static ConvertResult<T> Okay(T value) => new(true, value, string.Empty);
        internal static ConvertResult<T> Err(string error) => new(false, default!, error);
    }

    private static ConvertResult<object?> Ok(object? value) => ConvertResult<object?>.Okay(value);
    private static ConvertResult<object?> ErrObj(string error) => ConvertResult<object?>.Err(error);

    private static bool IsMissingInput(object? v) => v is null;

    // ---- value-kind probes (mirror JS typeof / Array.isArray / plain-object checks) ----

    private static bool IsArray(object? v) =>
        v is IReadOnlyList<object?>;

    private static IReadOnlyList<object?> AsArray(object? v) => (IReadOnlyList<object?>)v!;

    private static bool IsPlainObject(object? v) =>
        v is IReadOnlyDictionary<string, object?>;

    private static IReadOnlyDictionary<string, object?> AsObject(object? v) =>
        (IReadOnlyDictionary<string, object?>)v!;

    private static bool IsDate(object? v) => v is JsDate;

    // =====================================================================
    // applyShape — original-on-miss
    // =====================================================================

    internal static object? ApplyShape(object? value, Shape shape)
    {
        switch (shape.Kind)
        {
            case "string": return ApplyScalar(value, ToString);
            case "number": return ApplyScalar(value, ToNumber);
            case "boolean": return ApplyScalar(value, ToBoolean);
            case "date": return ApplyScalar(value, ToDate);
            case "array": return ApplyArrayShape(value, shape);
            case "object": return ApplyObjectShape(value, shape);
            case "nullable":
                if (IsMissingInput(value))
                    return null;
                shape.TryGetNullableInnerShape(out var inner);
                return ApplyShape(value, inner);
            case "raw":
            case "any":
            case "none":
                return value;
            default:
                throw new InvalidOperationException($"[alis] unknown shape kind: \"{shape.Kind}\"");
        }
    }

    private static object? ApplyScalar<T>(object? value, Func<object?, ConvertResult<T>> coerce)
    {
        var result = coerce(value);
        return result.Ok ? Box(result.Value) : value;
    }

    private static object? Box<T>(T value) => value;

    private static object? ApplyArrayShape(object? value, Shape shape)
    {
        var array = ToArray(value);
        if (!array.Ok)
            return value; // original-on-miss for applyShape
        shape.TryGetArrayItemShape(out var item);
        return ApplyArrayItemShape(array.Value, item);
    }

    private static IReadOnlyList<object?> ApplyArrayItemShape(IReadOnlyList<object?> items, Shape itemShape)
    {
        var result = new object?[items.Count];
        for (var i = 0; i < items.Count; i++)
            result[i] = ApplyShape(items[i], itemShape);
        return result;
    }

    private static object? ApplyObjectShape(object? value, Shape shape)
    {
        var plain = ToPlainObject(value);
        if (!plain.Ok)
            return value; // original-on-miss for applyShape
        shape.TryGetObjectContract(out var contract);
        return ApplyObjectFields(plain.Value, contract);
    }

    private static IReadOnlyDictionary<string, object?> ApplyObjectFields(
        IReadOnlyDictionary<string, object?> input, ShapeObjectContract contract)
    {
        var additional = contract.AllowsAdditionalFields;

        // Open object with zero declared fields → input unchanged.
        if (additional && contract.Fields.Count == 0)
            return input;

        var result = new Dictionary<string, object?>();

        if (additional)
        {
            // copy all input keys first
            foreach (var (key, val) in input)
                result[key] = val;
        }

        foreach (var (key, fieldShape) in contract.Fields)
        {
            // absent declared field is SKIPPED — never materialized to its zero.
            if (!input.TryGetValue(key, out var fieldValue))
                continue;
            result[key] = ApplyShape(fieldValue, fieldShape);
        }

        return result;
    }

    // =====================================================================
    // convertByShape — strict at top + through nullable, lenient on nested items/fields
    // =====================================================================

    internal static ConvertResult<object?> ConvertByShape(object? value, Shape shape)
    {
        switch (shape.Kind)
        {
            case "string": return Widen(ToString(value));
            case "number": return Widen(ToNumber(value));
            case "boolean": return Widen(ToBoolean(value));
            case "date": return Widen(ToDate(value));
            case "array":
            {
                var array = ToArray(value);
                if (!array.Ok)
                    return ErrObj(array.Error);
                shape.TryGetArrayItemShape(out var item);
                return Ok(ApplyArrayItemShape(array.Value, item));
            }
            case "object":
            {
                var plain = ToPlainObject(value);
                if (!plain.Ok)
                    return ErrObj(plain.Error);
                shape.TryGetObjectContract(out var contract);
                return Ok(ApplyObjectFields(plain.Value, contract));
            }
            case "nullable":
                if (IsMissingInput(value))
                    return Ok(null);
                shape.TryGetNullableInnerShape(out var inner);
                return ConvertByShape(value, inner); // STRICT — the only nested-strict path
            case "raw":
            case "any":
            case "none":
                return Ok(value);
            default:
                throw new InvalidOperationException($"[alis] unknown shape kind: \"{shape.Kind}\"");
        }
    }

    private static ConvertResult<object?> Widen<T>(ConvertResult<T> r) =>
        r.Ok ? Ok(r.Value) : ErrObj(r.Error);

    // =====================================================================
    // scalar coercions — each total, each returns ConvertResult, JS semantics
    // =====================================================================

    internal static ConvertResult<string> ToString(object? value)
    {
        if (IsMissingInput(value))
            return ConvertResult<string>.Okay("");
        if (value is string s)
            return ConvertResult<string>.Okay(s);
        if (value is double d)
            return ConvertResult<string>.Okay(JsNumberToString(d));
        if (value is bool b)
            return ConvertResult<string>.Okay(b ? "true" : "false");
        if (IsDate(value))
            return ConvertResult<string>.Okay(((JsDate)value!).ToIsoString());
        if (IsArray(value))
            return ConvertResult<string>.Okay(JsArrayToString(AsArray(value)));
        // plain object → Err
        return ConvertResult<string>.Err("[alis] cannot coerce object to string");
    }

    internal static ConvertResult<double> ToNumber(object? value)
    {
        if (IsMissingInput(value))
            return ConvertResult<double>.Okay(0d);
        if (value is double d)
            return double.IsFinite(d)
                ? ConvertResult<double>.Okay(d)
                : ConvertResult<double>.Err("[alis] non-finite number");
        if (value is bool b)
            return ConvertResult<double>.Okay(b ? 1d : 0d);
        if (value is string s)
        {
            var n = JsNumber(s);
            return double.IsFinite(n)
                ? ConvertResult<double>.Okay(n)
                : ConvertResult<double>.Err($"[alis] cannot coerce \"{s}\" to number");
        }
        if (IsDate(value))
        {
            var ms = ((JsDate)value!).EpochMs;
            return double.IsFinite(ms)
                ? ConvertResult<double>.Okay(ms)
                : ConvertResult<double>.Err("[alis] invalid date to number");
        }
        return ConvertResult<double>.Err("[alis] cannot coerce object to number");
    }

    internal static ConvertResult<bool> ToBoolean(object? value)
    {
        if (IsMissingInput(value))
            return ConvertResult<bool>.Okay(false);
        if (value is bool b)
            return ConvertResult<bool>.Okay(b);
        if (value is string s)
            return ConvertResult<bool>.Okay(!(s == "" || s == "false" || s == "0"));
        if (value is double d)
            return ConvertResult<bool>.Okay(d != 0d && !double.IsNaN(d));
        if (IsDate(value))
            return ConvertResult<bool>.Okay(true);
        if (IsArray(value))
            return ConvertResult<bool>.Okay(AsArray(value).Count > 0);
        return ConvertResult<bool>.Err("[alis] cannot coerce object to boolean");
    }

    internal static ConvertResult<double> ToDate(object? value)
    {
        // MISSING → ok(NaN), NOT epoch 0.
        if (IsMissingInput(value))
            return ConvertResult<double>.Okay(double.NaN);
        if (IsDate(value))
            return ConvertResult<double>.Okay(((JsDate)value!).EpochMs);
        if (value is double d)
            return double.IsFinite(d)
                ? ConvertResult<double>.Okay(d)
                : ConvertResult<double>.Err("[alis] non-finite date number");
        if (value is string s)
        {
            if (TryParseDateOnly(s, out var localMidnightMs))
                return ConvertResult<double>.Okay(localMidnightMs);
            var ms = JsDateParse(s);
            return double.IsFinite(ms)
                ? ConvertResult<double>.Okay(ms)
                : ConvertResult<double>.Err($"[alis] cannot coerce \"{s}\" to date");
        }
        if (value is bool)
            return ConvertResult<double>.Err("[alis] cannot coerce boolean to date");
        return ConvertResult<double>.Err("[alis] cannot coerce object to date");
    }

    internal static ConvertResult<IReadOnlyList<object?>> ToArray(object? value)
    {
        if (IsArray(value))
            return ConvertResult<IReadOnlyList<object?>>.Okay(AsArray(value));
        if (IsMissingInput(value) || (value is string es && es == ""))
            return ConvertResult<IReadOnlyList<object?>>.Okay(Array.Empty<object?>());
        if (IsPlainObject(value))
            return ConvertResult<IReadOnlyList<object?>>.Err("[alis] cannot coerce object to array");
        // scalar → [v]
        return ConvertResult<IReadOnlyList<object?>>.Okay(new[] { value });
    }

    internal static ConvertResult<IReadOnlyDictionary<string, object?>> ToPlainObject(object? value)
    {
        if (IsPlainObject(value))
            return ConvertResult<IReadOnlyDictionary<string, object?>>.Okay(AsObject(value));
        // array / Date / scalar / missing → Err
        return ConvertResult<IReadOnlyDictionary<string, object?>>.Err("[alis] not a plain object");
    }

    // =====================================================================
    // JS primitive-conversion helpers
    // =====================================================================

    // String(number): integer-valued doubles render without a decimal point;
    // NaN → "NaN"; ±Infinity → "Infinity"/"-Infinity".
    private static string JsNumberToString(double d)
    {
        if (double.IsNaN(d))
            return "NaN";
        if (double.IsPositiveInfinity(d))
            return "Infinity";
        if (double.IsNegativeInfinity(d))
            return "-Infinity";
        // "R" round-trips; JS drops the trailing ".0" for integral values, which "R" also does.
        return d.ToString("R", CultureInfo.InvariantCulture);
    }

    // String(array): comma-joined; null/undefined elements render as "".
    private static string JsArrayToString(IReadOnlyList<object?> items)
    {
        var parts = new string[items.Count];
        for (var i = 0; i < items.Count; i++)
        {
            var el = items[i];
            if (el is null)
                parts[i] = "";
            else
            {
                var r = ToString(el);
                parts[i] = r.Ok ? r.Value : "[object Object]";
            }
        }
        return string.Join(",", parts);
    }

    // Number(string): ""/whitespace → 0; otherwise invariant parse, else NaN.
    private static double JsNumber(string s)
    {
        if (s.Trim().Length == 0)
            return 0d;
        if (double.TryParse(s.Trim(), NumberStyles.Float | NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture, out var n))
            return n;
        return double.NaN;
    }

    // "YYYY-MM-DD" → LOCAL midnight epoch ms.
    private static bool TryParseDateOnly(string s, out double localMidnightMs)
    {
        localMidnightMs = double.NaN;
        if (!DateOnly.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return false;
        var localMidnight = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Local);
        var utc = localMidnight.ToUniversalTime();
        localMidnightMs = (utc - DateTimeOffset.UnixEpoch.UtcDateTime).TotalMilliseconds;
        return true;
    }

    // new Date(s).getTime() for non date-only strings; NaN on parse failure.
    private static double JsDateParse(string s)
    {
        if (DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dto))
            return dto.ToUnixTimeMilliseconds();
        return double.NaN;
    }
}

/// <summary>A JS Date modelled as epoch milliseconds (UTC).</summary>
internal sealed class JsDate(double epochMs)
{
    internal double EpochMs { get; } = epochMs;

    internal string ToIsoString()
    {
        // Date.toISOString(): UTC, millisecond precision, trailing Z.
        var dto = DateTimeOffset.FromUnixTimeMilliseconds((long)EpochMs).ToUniversalTime();
        return dto.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
    }
}
