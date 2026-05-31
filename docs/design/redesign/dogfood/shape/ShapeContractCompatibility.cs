// Spec-only reconstruction of the merge/accept algebra.
// Source: Shape.md §2d, §3 (Merge/Accept rows), §5b, §6 D fixtures. No framework source read.

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
    /// Rules (ordered): == → self; Any → other; None → conflict; nullable absorbs inner;
    /// arrays recurse; objects union fields (conflict on a shared field that conflicts);
    /// else conflict.
    /// </summary>
    internal static bool TryMergeContracts(Shape existing, Shape incoming, [NotNullWhen(true)] out Shape? merged)
    {
        if (existing is null) throw new ArgumentNullException(nameof(existing));
        if (incoming is null) throw new ArgumentNullException(nameof(incoming));

        // fixture: merge_equal_is_self
        if (existing.Equals(incoming)) return Merge(existing, out merged);

        // fixture: merge_none_conflicts — None conflicts with everything (even Any).
        if (existing.IsNone || incoming.IsNone) return Conflict(out merged);

        // fixture: merge_any_yields_other — Any is identity; the other side wins.
        if (existing.Kind == "any") return Merge(incoming, out merged);
        if (incoming.Kind == "any") return Merge(existing, out merged);

        // fixture: merge_nullable_absorbs_inner — nullable is transparent; merge inners,
        // result stays nullable.
        if (existing.TryGetNullableInnerShape(out var exInner) || incoming.TryGetNullableInnerShape(out _))
            return MergeNullable(existing, incoming, out merged);

        // fixture: merge_arrays_recurse
        if (existing.TryGetArrayItemShape(out var exItem) && incoming.TryGetArrayItemShape(out var inItem))
        {
            if (!TryMergeContracts(exItem, inItem, out var mergedItem)) return Conflict(out merged);
            return Merge(Shape.ArrayOf(mergedItem), out merged);
        }

        // fixtures: merge_objects_union_fields, merge_field_conflict_is_conflict
        if (existing.TryGetObjectContract(out var exObj) && incoming.TryGetObjectContract(out var inObj))
            return MergeObjects(exObj, inObj, out merged);

        return Conflict(out merged);
    }

    /// <summary>
    /// True when a value of <paramref name="actual"/> may be written to a member
    /// declared as <paramref name="expected"/>.
    /// Rules: Any accepts/accepted-by anything; None accepts nothing (either side);
    /// nullable transparent; arrays/objects recurse; open-object-with-no-fields accepts
    /// any object; a closed expected with a field missing in actual is rejected.
    /// </summary>
    internal static bool CanAccept(Shape expected, Shape actual)
    {
        if (expected is null) throw new ArgumentNullException(nameof(expected));
        if (actual is null) throw new ArgumentNullException(nameof(actual));

        // fixture: accept_equal
        if (expected.Equals(actual)) return true;

        // fixture: reject_none_either_side — None accepts nothing and is accepted by nothing.
        if (expected.IsNone || actual.IsNone) return false;

        // fixture: accept_any_either_side
        if (expected.Kind == "any" || actual.Kind == "any") return true;

        // nullable is transparent on both sides (spec §3 "Accept": nullable transparent).
        if (expected.TryGetNullableInnerShape(out var exInner)) expected = exInner;
        if (actual.TryGetNullableInnerShape(out var acInner)) actual = acInner;
        if (expected.Equals(actual)) return true;
        if (expected.Kind == "any" || actual.Kind == "any") return true; // Any re-check after unwrap

        // fixture: accept_array_recurse
        if (expected.TryGetArrayItemShape(out var exItem))
            return actual.TryGetArrayItemShape(out var acItem) && CanAccept(exItem, acItem);

        // fixtures: accept_open_object, reject_missing_required_field
        if (expected.TryGetObjectContract(out var exObj))
        {
            if (!actual.TryGetObjectContract(out var acObj)) return false;

            // open expected (no declared fields) accepts any object
            if (exObj.AllowsAdditionalFields && exObj.Fields.Count == 0) return true;

            // every field the expected contract declares must be present and acceptable
            foreach (var (name, fieldShape) in exObj.Fields)
            {
                if (!acObj.Fields.TryGetValue(name, out var actualFieldShape)) return false;
                if (!CanAccept(fieldShape, actualFieldShape)) return false;
            }
            return true;
        }

        // scalars that are not equal and not Any cannot accept each other
        return false;
    }

    // --- helpers (pure mechanism) ---

    private static bool MergeNullable(Shape existing, Shape incoming, [NotNullWhen(true)] out Shape? merged)
    {
        var exInner = existing.TryGetNullableInnerShape(out var ei) ? ei : existing;
        var inInner = incoming.TryGetNullableInnerShape(out var ii) ? ii : incoming;
        if (!TryMergeContracts(exInner, inInner, out var mergedInner)) return Conflict(out merged);
        return Merge(Shape.Nullable(mergedInner), out merged);
    }

    private static bool MergeObjects(ShapeObjectContract existing, ShapeObjectContract incoming, [NotNullWhen(true)] out Shape? merged)
    {
        // two open objects with no fields → OpenObject (spec §3 merge row)
        if (existing.Fields.Count == 0 && incoming.Fields.Count == 0 &&
            existing.AllowsAdditionalFields && incoming.AllowsAdditionalFields)
            return Merge(Shape.OpenObject(), out merged);

        var union = new Dictionary<string, Shape>(existing.Fields);
        foreach (var (name, inField) in incoming.Fields)
        {
            if (union.TryGetValue(name, out var exField))
            {
                // shared field: merge; conflict on the field is a contract conflict.
                if (!TryMergeContracts(exField, inField, out var mergedField)) return Conflict(out merged);
                union[name] = mergedField;
            }
            else
            {
                union[name] = inField;
            }
        }
        return Merge(Shape.ObjectOf(union), out merged);
    }

    private static bool Merge(Shape shape, [NotNullWhen(true)] out Shape? merged) { merged = shape; return true; }
    private static bool Conflict([NotNullWhen(true)] out Shape? merged) { merged = null; return false; }
}
