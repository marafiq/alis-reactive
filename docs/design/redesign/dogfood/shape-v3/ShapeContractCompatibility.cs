using System.Diagnostics.CodeAnalysis;

namespace Alis.Reactive.PlanModel;

/// <summary>
/// Picks the most specific compatible shape from two contracts (merge), and
/// decides whether one shape may flow into a member declared with another (accept).
/// Used by gather assignment merging and contract-member checks.
/// </summary>
internal static class ShapeContractCompatibility
{
    /// <summary>
    /// Merge a producer/consumer pair into the most specific shape that satisfies both.
    /// Returns false (merged=null) on genuine conflict — the caller surfaces it.
    /// </summary>
    internal static bool TryMergeContracts(Shape existing, Shape incoming, [NotNullWhen(true)] out Shape? merged)
    {
        ArgumentNullException.ThrowIfNull(existing);
        ArgumentNullException.ThrowIfNull(incoming);

        if (existing.Equals(incoming))
            return Merge(existing, out merged);

        if (existing.IsNone || incoming.IsNone)
            return Conflict(out merged);

        if (existing == Shape.Any)
            return Merge(incoming, out merged);
        if (incoming == Shape.Any)
            return Merge(existing, out merged);

        // Narrow nullable: nullable<X> merges with X (yielding nullable<X>) ONLY when the
        // inner equals the other. Not transparent — nullable<any> + string falls through.
        if (existing.IsNullableOf(incoming))
            return Merge(existing, out merged);
        if (incoming.IsNullableOf(existing))
            return Merge(incoming, out merged);

        if (existing.TryGetArrayItemShape(out var existingItem) && incoming.TryGetArrayItemShape(out var incomingItem))
        {
            if (TryMergeContracts(existingItem, incomingItem, out var mergedItem))
                return Merge(Shape.ArrayOf(mergedItem), out merged);
            return Conflict(out merged);
        }

        if (existing.TryGetObjectContract(out var existingContract) && incoming.TryGetObjectContract(out var incomingContract))
            return TryMergeObjects(existingContract, incomingContract, out merged);

        return Conflict(out merged);
    }

    /// <summary>
    /// True when a value of <paramref name="actual"/> may be written to a member
    /// declared as <paramref name="expected"/>. Any accepts/accepted-by anything;
    /// None accepts nothing; nullable is narrow — nullable&lt;X&gt; is accepted with
    /// X only when the inner shapes are equal (IsNullableOf), so nullable&lt;any&gt;
    /// does NOT accept a string; arrays/objects recurse.
    /// </summary>
    internal static bool CanAccept(Shape expected, Shape actual)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(actual);

        if (expected.Equals(actual))
            return true;

        if (expected.IsNone || actual.IsNone)
            return false;

        if (expected == Shape.Any || actual == Shape.Any)
            return true;

        // Narrow nullable: accepted only when one side is exactly nullable<the-other>.
        if (expected.IsNullableOf(actual) || actual.IsNullableOf(expected))
            return true;

        if (expected.TryGetArrayItemShape(out var expectedItem) && actual.TryGetArrayItemShape(out var actualItem))
            return CanAccept(expectedItem, actualItem);

        if (expected.TryGetObjectContract(out var expectedContract) && actual.TryGetObjectContract(out var actualContract))
            return CanAcceptObject(expectedContract, actualContract);

        return false;
    }

    private static bool TryMergeObjects(ShapeObjectContract existing, ShapeObjectContract incoming, [NotNullWhen(true)] out Shape? merged)
    {
        var union = new Dictionary<string, Shape>(existing.Fields);
        foreach (var (key, incomingShape) in incoming.Fields)
        {
            if (union.TryGetValue(key, out var existingShape))
            {
                if (!TryMergeContracts(existingShape, incomingShape, out var mergedField))
                    return Conflict(out merged);
                union[key] = mergedField;
            }
            else
            {
                union[key] = incomingShape;
            }
        }

        var bothAllowAdditional = existing.AllowsAdditionalFields && incoming.AllowsAdditionalFields;
        if (bothAllowAdditional && union.Count == 0)
            return Merge(Shape.OpenObject(), out merged);

        return Merge(Shape.ObjectOf(union), out merged);
    }

    private static bool CanAcceptObject(ShapeObjectContract expected, ShapeObjectContract actual)
    {
        // Expected open with no declared fields accepts any object.
        if (expected.AllowsAdditionalFields && expected.Fields.Count == 0)
            return true;

        foreach (var (key, expectedShape) in expected.Fields)
        {
            if (actual.Fields.TryGetValue(key, out var actualShape))
            {
                if (!CanAccept(expectedShape, actualShape))
                    return false;
            }
            else if (!actual.AllowsAdditionalFields)
            {
                return false; // closed actual cannot supply the missing required field.
            }
            // else: actual is open → the missing field is permitted.
        }

        return true;
    }

    private static bool Merge(Shape shape, [NotNullWhen(true)] out Shape? merged)
    {
        merged = shape;
        return true;
    }

    private static bool Conflict([NotNullWhen(true)] out Shape? merged)
    {
        merged = null;
        return false;
    }
}
