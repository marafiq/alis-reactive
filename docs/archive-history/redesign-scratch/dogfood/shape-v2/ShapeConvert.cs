using System.Globalization;

namespace Alis.Reactive.PlanModel;

/// <summary>
/// Discriminated result — caller MUST check <see cref="Ok"/> before using <see cref="Value"/>.
/// Port artifact: the TS <c>ConvertResult&lt;T&gt;</c> union becomes a readonly struct.
/// </summary>
internal readonly struct ConvertResult<T>
{
    private ConvertResult(bool ok, T? value, string? error)
    {
        Ok = ok;
        Value = value;
        Error = error;
    }

    internal bool Ok { get; }

    internal T? Value { get; }

    internal string? Error { get; }

    internal static ConvertResult<T> OkResult(T value) => new(true, value, null);

    internal static ConvertResult<T> Err(string error) => new(false, default, error);
}

/// <summary>
/// The single Shape -&gt; value conversion engine, ported from <c>core/shape-convert.ts</c>.
///
/// Port artifacts (TS -&gt; C# value model):
///   - a JS value = <c>object?</c>; <c>null</c>/<c>undefined</c> ("missing") = C# <c>null</c>.
///   - a plain object = <see cref="Dictionary{TKey,TValue}"/> keyed by string.
///   - an array = an <see cref="IReadOnlyList{T}"/> of <c>object?</c> (we emit <c>List&lt;object?&gt;</c>).
///   - a Date = a <see cref="DateTime"/>; an epoch-ms "date number" = a numeric value.
///   - <c>NaN</c> = <see cref="double.NaN"/>.
/// </summary>
internal static class ShapeConvert
{
    internal static object? ApplyShape(object? value, Shape shape)
    {
        switch (shape.Kind)
        {
            case "string":
                return ApplyScalar(value, ToString, asOriginal: true);
            case "number":
                return ApplyScalar(value, ToNumber, asOriginal: true);
            case "boolean":
                return ApplyScalar(value, ToBoolean, asOriginal: true);
            case "date":
                return ApplyScalar(value, ToDate, asOriginal: true);
            case "array":
                return ApplyArrayShape(value, shape, asOriginal: true).Value;
            case "object":
                return ApplyObjectShape(value, shape, asOriginal: true).Value;
            case "nullable":
                return ApplyNullableShape(value, shape, asOriginal: true).Value;
            case "raw":
            case "any":
            case "none":
                return value;
            default:
                throw new InvalidOperationException($"[alis] unknown shape kind: \"{shape.Kind}\"");
        }
    }

    internal static ConvertResult<object?> ConvertByShape(object? value, Shape shape)
    {
        switch (shape.Kind)
        {
            case "string":
                return Box(ToString(value));
            case "number":
                return Box(ToNumber(value));
            case "boolean":
                return Box(ToBoolean(value));
            case "date":
                return Box(ToDate(value));
            case "array":
                return ApplyArrayShape(value, shape, asOriginal: false);
            case "object":
                return ApplyObjectShape(value, shape, asOriginal: false);
            case "nullable":
                return ApplyNullableShape(value, shape, asOriginal: false);
            case "raw":
            case "any":
            case "none":
                return ConvertResult<object?>.OkResult(value);
            default:
                throw new InvalidOperationException($"[alis] unknown shape kind: \"{shape.Kind}\"");
        }
    }

    // ---- scalar coercions (each total, each returns ConvertResult) -------------------------

    internal static ConvertResult<string> ToString(object? value)
    {
        if (IsMissing(value))
        {
            return ConvertResult<string>.OkResult(string.Empty);
        }

        if (value is string s)
        {
            return ConvertResult<string>.OkResult(s);
        }

        if (value is bool b)
        {
            return ConvertResult<string>.OkResult(b ? "true" : "false");
        }

        if (IsNumber(value))
        {
            return ConvertResult<string>.OkResult(NumberToJsString(ToDoubleNumber(value!)));
        }

        if (value is DateTime dt)
        {
            return ConvertResult<string>.OkResult(ToIsoString(dt));
        }

        if (value is IReadOnlyList<object?> array)
        {
            // JS String([1,2]) -> "1,2": each element stringified, comma-joined, missing -> "".
            return ConvertResult<string>.OkResult(JsArrayToString(array));
        }

        if (IsPlainObject(value))
        {
            return ConvertResult<string>.Err("Cannot convert a plain object to a string.");
        }

        return ConvertResult<string>.Err($"Cannot convert value to a string: {value}");
    }

    internal static ConvertResult<double> ToNumber(object? value)
    {
        if (IsMissing(value))
        {
            return ConvertResult<double>.OkResult(0);
        }

        if (IsNumber(value))
        {
            double n = ToDoubleNumber(value!);
            return double.IsFinite(n)
                ? ConvertResult<double>.OkResult(n)
                : ConvertResult<double>.Err("Number is not finite.");
        }

        if (value is bool b)
        {
            return ConvertResult<double>.OkResult(b ? 1 : 0);
        }

        if (value is string s)
        {
            // JS Number("") and Number("  ") -> 0; Number("abc") -> NaN -> Err.
            if (s.Trim().Length == 0)
            {
                return ConvertResult<double>.OkResult(0);
            }

            double parsed = JsNumber(s);
            return double.IsFinite(parsed)
                ? ConvertResult<double>.OkResult(parsed)
                : ConvertResult<double>.Err($"Cannot parse \"{s}\" as a number.");
        }

        if (value is DateTime dt)
        {
            double ms = ToEpochMs(dt);
            return double.IsFinite(ms)
                ? ConvertResult<double>.OkResult(ms)
                : ConvertResult<double>.Err("Date time is not finite.");
        }

        if (IsPlainObject(value) || value is IReadOnlyList<object?>)
        {
            return ConvertResult<double>.Err("Cannot convert a non-scalar value to a number.");
        }

        return ConvertResult<double>.Err($"Cannot convert value to a number: {value}");
    }

    internal static ConvertResult<bool> ToBoolean(object? value)
    {
        if (IsMissing(value))
        {
            return ConvertResult<bool>.OkResult(false);
        }

        if (value is bool b)
        {
            return ConvertResult<bool>.OkResult(b);
        }

        if (value is string s)
        {
            // Falsy ONLY for "", "false", "0"; everything else (incl. "no"/"off") -> true.
            bool falsy = s is "" or "false" or "0";
            return ConvertResult<bool>.OkResult(!falsy);
        }

        if (IsNumber(value))
        {
            double n = ToDoubleNumber(value!);
            bool truthy = n != 0 && !double.IsNaN(n);
            return ConvertResult<bool>.OkResult(truthy);
        }

        if (value is DateTime)
        {
            return ConvertResult<bool>.OkResult(true);
        }

        if (value is IReadOnlyList<object?> array)
        {
            return ConvertResult<bool>.OkResult(array.Count > 0);
        }

        if (IsPlainObject(value))
        {
            return ConvertResult<bool>.Err("Cannot convert a plain object to a boolean.");
        }

        return ConvertResult<bool>.Err($"Cannot convert value to a boolean: {value}");
    }

    internal static ConvertResult<double> ToDate(object? value)
    {
        if (IsMissing(value))
        {
            return ConvertResult<double>.OkResult(double.NaN);
        }

        if (value is DateTime dt)
        {
            return ConvertResult<double>.OkResult(ToEpochMs(dt));
        }

        if (IsNumber(value))
        {
            double ms = ToDoubleNumber(value!);
            return double.IsFinite(ms)
                ? ConvertResult<double>.OkResult(ms)
                : ConvertResult<double>.Err("Epoch milliseconds value is not finite.");
        }

        if (value is string s)
        {
            if (TryParseDateOnly(s, out double localMidnightMs))
            {
                return ConvertResult<double>.OkResult(localMidnightMs);
            }

            double parsed = JsDateParse(s);
            return double.IsFinite(parsed)
                ? ConvertResult<double>.OkResult(parsed)
                : ConvertResult<double>.Err($"Cannot parse \"{s}\" as a date.");
        }

        return ConvertResult<double>.Err($"Cannot convert value to a date: {value}");
    }

    internal static ConvertResult<IReadOnlyList<object?>> ToArray(object? value)
    {
        if (value is IReadOnlyList<object?> array)
        {
            return ConvertResult<IReadOnlyList<object?>>.OkResult(array);
        }

        if (IsMissing(value) || (value is string s && s.Length == 0))
        {
            return ConvertResult<IReadOnlyList<object?>>.OkResult(new List<object?>());
        }

        if (IsPlainObject(value))
        {
            return ConvertResult<IReadOnlyList<object?>>.Err("Cannot convert a plain object to an array.");
        }

        return ConvertResult<IReadOnlyList<object?>>.OkResult(new List<object?> { value });
    }

    internal static ConvertResult<IReadOnlyDictionary<string, object?>> ToPlainObject(object? value)
    {
        if (value is IReadOnlyDictionary<string, object?> dict)
        {
            return ConvertResult<IReadOnlyDictionary<string, object?>>.OkResult(dict);
        }

        return ConvertResult<IReadOnlyDictionary<string, object?>>.Err("Value is not a plain object.");
    }

    // ---- shaped composites -----------------------------------------------------------------

    private static object? ApplyScalar<T>(object? value, Func<object?, ConvertResult<T>> coerce, bool asOriginal)
    {
        ConvertResult<T> result = coerce(value);
        if (result.Ok)
        {
            return result.Value;
        }

        // applyShape: return the original value on a miss (never throws on a type mismatch).
        return value;
    }

    private static ConvertResult<object?> ApplyArrayShape(object? value, Shape shape, bool asOriginal)
    {
        shape.TryGetArrayItemShape(out Shape itemShape);

        ConvertResult<IReadOnlyList<object?>> arrayResult = ToArray(value);
        if (!arrayResult.Ok)
        {
            return asOriginal
                ? ConvertResult<object?>.OkResult(value)
                : ConvertResult<object?>.Err(arrayResult.Error!);
        }

        List<object?> shaped = [];
        foreach (object? item in arrayResult.Value!)
        {
            if (asOriginal)
            {
                shaped.Add(ApplyShape(item, itemShape));
            }
            else
            {
                ConvertResult<object?> itemResult = ConvertByShape(item, itemShape);
                if (!itemResult.Ok)
                {
                    return ConvertResult<object?>.Err(itemResult.Error!);
                }

                shaped.Add(itemResult.Value);
            }
        }

        return ConvertResult<object?>.OkResult(shaped);
    }

    private static ConvertResult<object?> ApplyNullableShape(object? value, Shape shape, bool asOriginal)
    {
        shape.TryGetNullableInnerShape(out Shape inner);

        if (IsMissing(value))
        {
            return ConvertResult<object?>.OkResult(null);
        }

        return asOriginal
            ? ConvertResult<object?>.OkResult(ApplyShape(value, inner))
            : ConvertByShape(value, inner);
    }

    private static ConvertResult<object?> ApplyObjectShape(object? value, Shape shape, bool asOriginal)
    {
        shape.TryGetObjectContract(out ShapeObjectContract contract);

        ConvertResult<IReadOnlyDictionary<string, object?>> objectResult = ToPlainObject(value);
        if (!objectResult.Ok)
        {
            return asOriginal
                ? ConvertResult<object?>.OkResult(value)
                : ConvertResult<object?>.Err(objectResult.Error!);
        }

        IReadOnlyDictionary<string, object?> input = objectResult.Value!;

        // additional && zero declared fields -> return the input unchanged.
        if (contract.AllowsAdditionalFields && contract.Fields.Count == 0)
        {
            return ConvertResult<object?>.OkResult(value);
        }

        Dictionary<string, object?> output = [];

        // When open, copy all input keys first.
        if (contract.AllowsAdditionalFields)
        {
            foreach ((string key, object? raw) in input)
            {
                output[key] = raw;
            }
        }

        // For each declared field: SKIP entirely when the input lacks that key.
        foreach ((string field, Shape fieldShape) in contract.Fields)
        {
            if (!input.TryGetValue(field, out object? fieldValue))
            {
                continue;
            }

            if (asOriginal)
            {
                output[field] = ApplyShape(fieldValue, fieldShape);
            }
            else
            {
                ConvertResult<object?> fieldResult = ConvertByShape(fieldValue, fieldShape);
                if (!fieldResult.Ok)
                {
                    return ConvertResult<object?>.Err(fieldResult.Error!);
                }

                output[field] = fieldResult.Value;
            }
        }

        return ConvertResult<object?>.OkResult(output);
    }

    private static ConvertResult<object?> Box<T>(ConvertResult<T> result) =>
        result.Ok
            ? ConvertResult<object?>.OkResult(result.Value)
            : ConvertResult<object?>.Err(result.Error!);

    // ---- JS value-model helpers ------------------------------------------------------------

    internal static bool IsMissing(object? value) => value is null;

    private static bool IsNumber(object? value) =>
        value is byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal;

    private static bool IsPlainObject(object? value) => value is IReadOnlyDictionary<string, object?>;

    private static double ToDoubleNumber(object value) =>
        value switch
        {
            double d => d,
            float f => f,
            decimal m => (double)m,
            byte b => b,
            sbyte sb => sb,
            short s => s,
            ushort us => us,
            int i => i,
            uint ui => ui,
            long l => l,
            ulong ul => ul,
            _ => Convert.ToDouble(value, CultureInfo.InvariantCulture),
        };

    private static string NumberToJsString(double value)
    {
        if (double.IsNaN(value))
        {
            return "NaN";
        }

        if (double.IsPositiveInfinity(value))
        {
            return "Infinity";
        }

        if (double.IsNegativeInfinity(value))
        {
            return "-Infinity";
        }

        // Integral doubles print without a decimal point, matching JS `${5}` -> "5".
        if (value == Math.Floor(value) && Math.Abs(value) < 1e15)
        {
            return ((long)value).ToString(CultureInfo.InvariantCulture);
        }

        return value.ToString("R", CultureInfo.InvariantCulture);
    }

    private static string JsArrayToString(IReadOnlyList<object?> array)
    {
        // JS Array.prototype.toString: join with "," ; null/undefined -> "".
        IEnumerable<string> parts = array.Select(item =>
        {
            if (IsMissing(item))
            {
                return string.Empty;
            }

            ConvertResult<string> r = ToString(item);
            return r.Ok ? r.Value! : string.Empty;
        });

        return string.Join(",", parts);
    }

    private static double JsNumber(string s)
    {
        // Number(s) for a non-empty, non-whitespace string: full-string parse, else NaN.
        string trimmed = s.Trim();
        return double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed)
            ? parsed
            : double.NaN;
    }

    private static bool TryParseDateOnly(string s, out double localMidnightMs)
    {
        localMidnightMs = double.NaN;

        // "YYYY-MM-DD" -> LOCAL midnight epoch ms (the date-only fast path).
        if (DateTime.TryParseExact(
                s,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime parsed))
        {
            DateTime localMidnight = DateTime.SpecifyKind(parsed.Date, DateTimeKind.Local);
            localMidnightMs = ToEpochMs(localMidnight);
            return true;
        }

        return false;
    }

    private static double JsDateParse(string s) =>
        DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out DateTime parsed)
            ? ToEpochMs(DateTime.SpecifyKind(parsed, DateTimeKind.Utc))
            : double.NaN;

    private static double ToEpochMs(DateTime value)
    {
        DateTimeOffset offset = value.Kind == DateTimeKind.Unspecified
            ? new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Local))
            : new DateTimeOffset(value);

        return offset.ToUnixTimeMilliseconds();
    }

    internal static string ToIsoString(DateTime value)
    {
        DateTime utc = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Local).ToUniversalTime(),
        };

        return utc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
    }

    internal static string EpochMsToIsoString(double epochMs)
    {
        DateTimeOffset offset = DateTimeOffset.FromUnixTimeMilliseconds((long)epochMs);
        return offset.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
    }
}
