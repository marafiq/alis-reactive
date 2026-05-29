using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Picks the most specific compatible shape from producer and consumer contracts.
    /// Returns an explicit conflict when no merged shape can satisfy both contracts.
    /// </summary>
    internal static class ShapeContractCompatibility
    {
        internal static bool TryMergeContracts(
            Shape existing,
            Shape incoming,
            [NotNullWhen(true)] out Shape? merged)
        {
            if (existing == null) throw new System.ArgumentNullException(nameof(existing));
            if (incoming == null) throw new System.ArgumentNullException(nameof(incoming));

            if (existing == incoming) return Merge(existing, out merged);
            if (existing == Shape.None || incoming == Shape.None) return Conflict(out merged);
            if (existing == Shape.Any) return Merge(incoming, out merged);
            if (incoming == Shape.Any) return Merge(existing, out merged);
            if (existing.IsNullableOf(incoming)) return Merge(existing, out merged);
            if (incoming.IsNullableOf(existing)) return Merge(incoming, out merged);
            if (TryMergeArrayContracts(existing, incoming, out merged)) return true;
            if (TryMergeObjectContracts(existing, incoming, out merged)) return true;
            return Conflict(out merged);
        }

        internal static bool CanAccept(Shape expected, Shape actual)
        {
            if (expected == null) throw new System.ArgumentNullException(nameof(expected));
            if (actual == null) throw new System.ArgumentNullException(nameof(actual));

            if (expected == actual) return true;
            if (expected == Shape.None || actual == Shape.None) return false;
            if (expected == Shape.Any || actual == Shape.Any) return true;
            if (expected.IsNullableOf(actual) || actual.IsNullableOf(expected)) return true;
            if (CanAcceptArray(expected, actual)) return true;
            if (CanAcceptObject(expected, actual)) return true;
            return false;
        }

        private static bool TryMergeArrayContracts(
            Shape existing,
            Shape incoming,
            [NotNullWhen(true)] out Shape? merged)
        {
            if (!existing.TryGetArrayItemShape(out var existingItem) ||
                !incoming.TryGetArrayItemShape(out var incomingItem))
            {
                return Conflict(out merged);
            }

            if (!TryMergeContracts(existingItem, incomingItem, out var mergedItem))
                return Conflict(out merged);

            return Merge(Shape.ArrayOf(mergedItem), out merged);
        }

        private static bool TryMergeObjectContracts(
            Shape existing,
            Shape incoming,
            [NotNullWhen(true)] out Shape? merged)
        {
            if (!existing.TryGetObjectContract(out var existingObject) ||
                !incoming.TryGetObjectContract(out var incomingObject))
            {
                return Conflict(out merged);
            }

            return TryMergeObjectContracts(existingObject, incomingObject, out merged);
        }

        private static bool TryMergeObjectContracts(
            ShapeObjectContract existing,
            ShapeObjectContract incoming,
            [NotNullWhen(true)] out Shape? merged)
        {
            var mergedFields = new Dictionary<string, Shape>(System.StringComparer.Ordinal);
            foreach (var field in existing.Fields)
                mergedFields.Add(field.Key, field.Value);

            foreach (var field in incoming.Fields)
            {
                if (!mergedFields.TryGetValue(field.Key, out var existingField))
                {
                    mergedFields.Add(field.Key, field.Value);
                    continue;
                }

                if (!TryMergeContracts(existingField, field.Value, out var mergedField))
                    return Conflict(out merged);

                mergedFields[field.Key] = mergedField;
            }

            var bothAllowAnyField = existing.AllowsAdditionalFields && incoming.AllowsAdditionalFields;
            if (bothAllowAnyField && mergedFields.Count == 0)
                return Merge(Shape.OpenObject(), out merged);

            return Merge(Shape.ObjectOf(mergedFields), out merged);
        }

        private static bool CanAcceptArray(Shape expected, Shape actual)
        {
            if (!expected.TryGetArrayItemShape(out var expectedItem) ||
                !actual.TryGetArrayItemShape(out var actualItem))
                return false;

            return CanAccept(expectedItem, actualItem);
        }

        private static bool CanAcceptObject(Shape expected, Shape actual)
        {
            if (!expected.TryGetObjectContract(out var expectedObject) ||
                !actual.TryGetObjectContract(out var actualObject))
                return false;

            var expectedAcceptsAnyObject =
                expectedObject.AllowsAdditionalFields && expectedObject.Fields.Count == 0;
            if (expectedAcceptsAnyObject) return true;

            foreach (var expectedField in expectedObject.Fields)
            {
                if (!actualObject.Fields.TryGetValue(expectedField.Key, out var actualField))
                {
                    if (actualObject.AllowsAdditionalFields) continue;
                    return false;
                }

                if (!CanAccept(expectedField.Value, actualField))
                    return false;
            }

            return true;
        }

        private static bool Merge(Shape shape, [NotNullWhen(true)] out Shape? merged)
        {
            merged = shape ?? throw new System.ArgumentNullException(nameof(shape));
            return true;
        }

        private static bool Conflict([NotNullWhen(true)] out Shape? merged)
        {
            merged = null;
            return false;
        }
    }
}
