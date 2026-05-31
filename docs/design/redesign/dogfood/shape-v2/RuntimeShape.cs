namespace Alis.Reactive.PlanModel;

/// <summary>
/// Runtime wrapper around a declared <see cref="Shape"/> plus the SHAPE-ONCE egress
/// (<see cref="FormatForWire"/>). Ported from <c>domain/runtime-shape.ts</c>.
///
/// Port artifact: the TS class wraps the generated-union <c>Shape</c> plain object; here it
/// wraps the C# <see cref="Shape"/> domain value object, navigated via its <c>TryGet…</c> API.
/// </summary>
internal sealed class RuntimeShape
{
    private readonly Shape _shape;

    private RuntimeShape(Shape shape) => _shape = shape;

    internal static RuntimeShape From(Shape shape) => new(shape);

    internal static RuntimeShape Unshaped() => new(Shape.None);

    internal Shape PlanShape => _shape;

    internal bool IsDeclared => _shape.Kind != "none";

    internal RuntimeShape Item() =>
        _shape.TryGetArrayItemShape(out Shape item) ? From(item) : Unshaped();

    internal Shape OrDeclared(Shape declared) => IsDeclared ? _shape : declared;

    internal object? Apply(object? value) =>
        IsDeclared ? ShapeConvert.ApplyShape(value, _shape) : value;

    internal IReadOnlyList<object?> ApplyEach(IReadOnlyList<object?> items)
    {
        if (!IsDeclared)
        {
            return items;
        }

        List<object?> shaped = [];
        foreach (object? item in items)
        {
            shaped.Add(ShapeConvert.ApplyShape(item, _shape));
        }

        return shaped;
    }

    internal ConvertResult<object?> Convert(object? value) => ShapeConvert.ConvertByShape(value, _shape);

    /// <summary>
    /// SHAPE-ONCE egress: convert a runtime value to its wire form exactly once.
    /// undeclared(none) -&gt; passthrough; nullable -&gt; recurse inner; a date whose value is a
    /// FINITE number -&gt; ISO string (UTC, ms, …Z); everything else passes through unchanged.
    /// </summary>
    internal object? FormatForWire(object? value)
    {
        if (!IsDeclared)
        {
            return value;
        }

        if (_shape.TryGetNullableInnerShape(out Shape inner))
        {
            return From(inner).FormatForWire(value);
        }

        if (_shape.Kind == "date" && IsFiniteNumber(value, out double epochMs))
        {
            return ShapeConvert.EpochMsToIsoString(epochMs);
        }

        return value;
    }

    private static bool IsFiniteNumber(object? value, out double epochMs)
    {
        epochMs = double.NaN;

        switch (value)
        {
            case double d when double.IsFinite(d):
                epochMs = d;
                return true;
            case float f when float.IsFinite(f):
                epochMs = f;
                return true;
            case int i:
                epochMs = i;
                return true;
            case long l:
                epochMs = l;
                return true;
            case decimal m:
                epochMs = (double)m;
                return true;
            default:
                return false;
        }
    }
}
