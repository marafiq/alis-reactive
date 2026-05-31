using System.Diagnostics.CodeAnalysis;

namespace Alis.Reactive.PlanModel;

/// <summary>
/// Picks the most specific compatible shape from two contracts (merge), and decides
/// whether one shape may flow into a member declared with another (accept).
/// </summary>
internal static class ShapeContractCompatibility
{
    /// <summary>
    /// Merge a producer/consumer pair into the most specific shape that satisfies both.
    /// Returns false (merged=null) on a genuine conflict.
    /// </summary>
    internal static bool TryMergeContracts(Shape existing, Shape incoming, [NotNullWhen(true)] out Shape? merged)
    {
        ArgumentNullException.ThrowIfNull(existing);
        ArgumentNullException.ThrowIfNull(incoming);

        // Equal contracts merge to themselves.
        if (existing.Equals(incoming))
        {
            return Merge(existing, out merged);
        }

        // None conflicts with everything (an absent value cannot satisfy a typed member).
        if (existing.IsNone || incoming.IsNone)
        {
            return Conflict(out merged);
        }

        // Any is identity: it adopts the other side.
        if (existing.Kind == "any")
        {
            return Merge(incoming, out merged);
        }

        if (incoming.Kind == "any")
        {
            return Merge(existing, out merged);
        }

        // Nullable is transparent: a nullable side absorbs the other into its inner merge,
        // re-wrapping the result as nullable.
        if (existing.TryGetNullableInnerShape(out Shape existingInner))
        {
            return MergeNullable(existingInner, Unwrap(incoming), out merged);
        }

        if (incoming.TryGetNullableInnerShape(out Shape incomingInner))
        {
            return MergeNullable(Unwrap(existing), incomingInner, out merged);
        }

        // Arrays recurse on the item shape.
        if (existing.TryGetArrayItemShape(out Shape existingItem)
            && incoming.TryGetArrayItemShape(out Shape incomingItem))
        {
            if (!TryMergeContracts(existingItem, incomingItem, out Shape? mergedItem))
            {
                return Conflict(out merged);
            }

            return Merge(Shape.ArrayOf(mergedItem), out merged);
        }

        // Objects union their fields.
        if (existing.TryGetObjectContract(out ShapeObjectContract existingContract)
            && incoming.TryGetObjectContract(out ShapeObjectContract incomingContract))
        {
            return MergeObjects(existingContract, incomingContract, out merged);
        }

        // Different scalar kinds, or array-vs-object, etc.
        return Conflict(out merged);
    }

    /// <summary>
    /// True when a value of <paramref name="actual"/> may be written to a member
    /// declared as <paramref name="expected"/>.
    /// </summary>
    internal static bool CanAccept(Shape expected, Shape actual)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(actual);

        if (expected.Equals(actual))
        {
            return true;
        }

        // None accepts nothing and is accepted by nothing.
        if (expected.IsNone || actual.IsNone)
        {
            return false;
        }

        // Any accepts anything and is accepted by anything.
        if (expected.Kind == "any" || actual.Kind == "any")
        {
            return true;
        }

        // Nullable is transparent on either side.
        if (expected.TryGetNullableInnerShape(out Shape expectedInner))
        {
            return CanAccept(expectedInner, Unwrap(actual));
        }

        if (actual.TryGetNullableInnerShape(out Shape actualInner))
        {
            return CanAccept(expected, actualInner);
        }

        // Arrays recurse.
        if (expected.TryGetArrayItemShape(out Shape expectedItem)
            && actual.TryGetArrayItemShape(out Shape actualItem))
        {
            return CanAccept(expectedItem, actualItem);
        }

        // Objects: recurse field-by-field.
        if (expected.TryGetObjectContract(out ShapeObjectContract expectedContract)
            && actual.TryGetObjectContract(out ShapeObjectContract actualContract))
        {
            return CanAcceptObject(expectedContract, actualContract);
        }

        return false;
    }

    private static bool MergeNullable(Shape existingInner, Shape incomingInner, [NotNullWhen(true)] out Shape? merged)
    {
        if (!TryMergeContracts(existingInner, incomingInner, out Shape? mergedInner))
        {
            return Conflict(out merged);
        }

        return Merge(Shape.Nullable(mergedInner), out merged);
    }

    private static Shape Unwrap(Shape shape) =>
        shape.TryGetNullableInnerShape(out Shape inner) ? inner : shape;

    private static bool MergeObjects(
        ShapeObjectContract existing,
        ShapeObjectContract incoming,
        [NotNullWhen(true)] out Shape? merged)
    {
        Dictionary<string, Shape> union = new(existing.Fields);

        foreach ((string key, Shape incomingShape) in incoming.Fields)
        {
            if (union.TryGetValue(key, out Shape? existingShape))
            {
                if (!TryMergeContracts(existingShape, incomingShape, out Shape? mergedField))
                {
                    return Conflict(out merged);
                }

                union[key] = mergedField;
            }
            else
            {
                union[key] = incomingShape;
            }
        }

        bool bothOpen = existing.AllowsAdditionalFields && incoming.AllowsAdditionalFields;
        if (bothOpen && union.Count == 0)
        {
            return Merge(Shape.OpenObject(), out merged);
        }

        return Merge(Shape.ObjectOf(union), out merged);
    }

    private static bool CanAcceptObject(ShapeObjectContract expected, ShapeObjectContract actual)
    {
        // An expected open object with no declared fields accepts any object.
        if (expected.AllowsAdditionalFields && expected.Fields.Count == 0)
        {
            return true;
        }

        foreach ((string key, Shape expectedShape) in expected.Fields)
        {
            if (actual.Fields.TryGetValue(key, out Shape? actualShape))
            {
                if (!CanAccept(expectedShape, actualShape))
                {
                    return false;
                }
            }
            else if (!actual.AllowsAdditionalFields)
            {
                // A missing required field is only acceptable when the actual side is open.
                return false;
            }
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
