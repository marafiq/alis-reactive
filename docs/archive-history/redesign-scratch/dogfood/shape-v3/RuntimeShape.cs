namespace Alis.Reactive.PlanModel;

/// <summary>
/// Port of the TS <c>domain/runtime-shape.ts</c> wrapper: carries the declared
/// <see cref="Shape"/> for a value on the gather path and applies the SHAPE-ONCE
/// egress rule (<see cref="FormatForWire"/>).
/// </summary>
internal sealed class RuntimeShape
{
    private static readonly Shape UnshapedPlanShape = Shape.None;

    private readonly Shape _shape;

    private RuntimeShape(Shape shape) => _shape = shape;

    internal static RuntimeShape From(Shape shape) => new(shape);

    internal static RuntimeShape Unshaped() => new(UnshapedPlanShape);

    internal Shape PlanShape => _shape;

    internal bool IsDeclared => _shape.Kind != "none";

    internal RuntimeShape Item()
    {
        if (_shape.TryGetArrayItemShape(out var item))
            return From(item);
        return Unshaped();
    }

    internal Shape OrDeclared(Shape declared) => IsDeclared ? _shape : declared;

    internal object? Apply(object? value) => IsDeclared ? ShapeConvert.ApplyShape(value, _shape) : value;

    internal IReadOnlyList<object?> ApplyEach(IReadOnlyList<object?> items)
    {
        if (!IsDeclared)
            return items;
        var result = new object?[items.Count];
        for (var i = 0; i < items.Count; i++)
            result[i] = ShapeConvert.ApplyShape(items[i], _shape);
        return result;
    }

    internal ShapeConvert.ConvertResult<object?> Convert(object? value) =>
        ShapeConvert.ConvertByShape(value, _shape);

    /// <summary>
    /// SHAPE-ONCE egress: convert a runtime value to its wire form exactly once.
    /// undeclared(none) → passthrough; nullable → recurse the inner shape; a date
    /// whose value is a finite number → ISO string; everything else passes through.
    /// </summary>
    internal object? FormatForWire(object? value) => FormatForWire(value, _shape);

    private static object? FormatForWire(object? value, Shape shape)
    {
        if (shape.Kind == "none")
            return value;

        if (shape.TryGetNullableInnerShape(out var inner))
            return FormatForWire(value, inner);

        if (shape.Kind == "date" && value is double d && double.IsFinite(d))
        {
            var dto = DateTimeOffset.FromUnixTimeMilliseconds((long)d).ToUniversalTime();
            return dto.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture);
        }

        return value;
    }
}
