using System;
using System.Collections.Generic;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// The comparisons that apply only to some types: ordering to comparable values (numbers, dates and
    /// times of day, and their nullable forms), text operators to text, and <c>ArrayContains</c> to collections.
    /// The compiler offers each only where it means something. A <see cref="TimeSpan"/> orders in C# but reaches
    /// the browser as its text, which does not order as its length does, so ordering one is refused where the page
    /// renders.
    /// </summary>
    public static class ConditionOperators
    {
        /// <summary>Matches when the source is greater than <paramref name="operand"/>.</summary>
        public static TNext Gt<TModel, TValue, TNext>(this ConditionSourceBuilder<TModel, TValue, TNext> source, TValue operand)
            where TModel : class where TNext : Condition<TModel> where TValue : struct, IComparable<TValue> =>
            source.CompareWith(Comparison.GreaterThan, operand);

        /// <summary>Matches when the source is greater than or equal to <paramref name="operand"/>.</summary>
        public static TNext Gte<TModel, TValue, TNext>(this ConditionSourceBuilder<TModel, TValue, TNext> source, TValue operand)
            where TModel : class where TNext : Condition<TModel> where TValue : struct, IComparable<TValue> =>
            source.CompareWith(Comparison.GreaterThanOrEqual, operand);

        /// <summary>Matches when the source is less than <paramref name="operand"/>.</summary>
        public static TNext Lt<TModel, TValue, TNext>(this ConditionSourceBuilder<TModel, TValue, TNext> source, TValue operand)
            where TModel : class where TNext : Condition<TModel> where TValue : struct, IComparable<TValue> =>
            source.CompareWith(Comparison.LessThan, operand);

        /// <summary>Matches when the source is less than or equal to <paramref name="operand"/>.</summary>
        public static TNext Lte<TModel, TValue, TNext>(this ConditionSourceBuilder<TModel, TValue, TNext> source, TValue operand)
            where TModel : class where TNext : Condition<TModel> where TValue : struct, IComparable<TValue> =>
            source.CompareWith(Comparison.LessThanOrEqual, operand);

        /// <summary>Matches when the source lies between <paramref name="low"/> and <paramref name="high"/>, both included.</summary>
        public static TNext Between<TModel, TValue, TNext>(this ConditionSourceBuilder<TModel, TValue, TNext> source, TValue low, TValue high)
            where TModel : class where TNext : Condition<TModel> where TValue : struct, IComparable<TValue> =>
            source.CompareWithRange(low, high);

        /// <summary>Matches when the source is greater than another value read when the condition runs.</summary>
        public static TNext Gt<TModel, TValue, TNext>(this ConditionSourceBuilder<TModel, TValue, TNext> source, TypedSource<TValue> right)
            where TModel : class where TNext : Condition<TModel> where TValue : struct, IComparable<TValue> =>
            source.CompareWith(Comparison.GreaterThan, right);

        /// <summary>Matches when the source is greater than or equal to another value read when the condition runs.</summary>
        public static TNext Gte<TModel, TValue, TNext>(this ConditionSourceBuilder<TModel, TValue, TNext> source, TypedSource<TValue> right)
            where TModel : class where TNext : Condition<TModel> where TValue : struct, IComparable<TValue> =>
            source.CompareWith(Comparison.GreaterThanOrEqual, right);

        /// <summary>Matches when the source is less than another value read when the condition runs.</summary>
        public static TNext Lt<TModel, TValue, TNext>(this ConditionSourceBuilder<TModel, TValue, TNext> source, TypedSource<TValue> right)
            where TModel : class where TNext : Condition<TModel> where TValue : struct, IComparable<TValue> =>
            source.CompareWith(Comparison.LessThan, right);

        /// <summary>Matches when the source is less than or equal to another value read when the condition runs.</summary>
        public static TNext Lte<TModel, TValue, TNext>(this ConditionSourceBuilder<TModel, TValue, TNext> source, TypedSource<TValue> right)
            where TModel : class where TNext : Condition<TModel> where TValue : struct, IComparable<TValue> =>
            source.CompareWith(Comparison.LessThanOrEqual, right);

        /// <summary>Matches when the source has a value greater than <paramref name="operand"/>.</summary>
        public static TNext Gt<TModel, TValue, TNext>(this ConditionSourceBuilder<TModel, TValue?, TNext> source, TValue operand)
            where TModel : class where TNext : Condition<TModel> where TValue : struct, IComparable<TValue> =>
            source.CompareWith(Comparison.GreaterThan, operand);

        /// <summary>Matches when the source has a value greater than or equal to <paramref name="operand"/>.</summary>
        public static TNext Gte<TModel, TValue, TNext>(this ConditionSourceBuilder<TModel, TValue?, TNext> source, TValue operand)
            where TModel : class where TNext : Condition<TModel> where TValue : struct, IComparable<TValue> =>
            source.CompareWith(Comparison.GreaterThanOrEqual, operand);

        /// <summary>Matches when the source has a value less than <paramref name="operand"/>.</summary>
        public static TNext Lt<TModel, TValue, TNext>(this ConditionSourceBuilder<TModel, TValue?, TNext> source, TValue operand)
            where TModel : class where TNext : Condition<TModel> where TValue : struct, IComparable<TValue> =>
            source.CompareWith(Comparison.LessThan, operand);

        /// <summary>Matches when the source has a value less than or equal to <paramref name="operand"/>.</summary>
        public static TNext Lte<TModel, TValue, TNext>(this ConditionSourceBuilder<TModel, TValue?, TNext> source, TValue operand)
            where TModel : class where TNext : Condition<TModel> where TValue : struct, IComparable<TValue> =>
            source.CompareWith(Comparison.LessThanOrEqual, operand);

        /// <summary>Matches when the source has a value between <paramref name="low"/> and <paramref name="high"/>, both included.</summary>
        public static TNext Between<TModel, TValue, TNext>(this ConditionSourceBuilder<TModel, TValue?, TNext> source, TValue low, TValue high)
            where TModel : class where TNext : Condition<TModel> where TValue : struct, IComparable<TValue> =>
            source.CompareWithRange(low, high);

        /// <summary>Matches when the source has a value greater than another value read when the condition runs.</summary>
        public static TNext Gt<TModel, TValue, TNext>(this ConditionSourceBuilder<TModel, TValue?, TNext> source, TypedSource<TValue?> right)
            where TModel : class where TNext : Condition<TModel> where TValue : struct, IComparable<TValue> =>
            source.CompareWith(Comparison.GreaterThan, right);

        /// <summary>Matches when the source has a value greater than or equal to another value read when the condition runs.</summary>
        public static TNext Gte<TModel, TValue, TNext>(this ConditionSourceBuilder<TModel, TValue?, TNext> source, TypedSource<TValue?> right)
            where TModel : class where TNext : Condition<TModel> where TValue : struct, IComparable<TValue> =>
            source.CompareWith(Comparison.GreaterThanOrEqual, right);

        /// <summary>Matches when the source has a value less than another value read when the condition runs.</summary>
        public static TNext Lt<TModel, TValue, TNext>(this ConditionSourceBuilder<TModel, TValue?, TNext> source, TypedSource<TValue?> right)
            where TModel : class where TNext : Condition<TModel> where TValue : struct, IComparable<TValue> =>
            source.CompareWith(Comparison.LessThan, right);

        /// <summary>Matches when the source has a value less than or equal to another value read when the condition runs.</summary>
        public static TNext Lte<TModel, TValue, TNext>(this ConditionSourceBuilder<TModel, TValue?, TNext> source, TypedSource<TValue?> right)
            where TModel : class where TNext : Condition<TModel> where TValue : struct, IComparable<TValue> =>
            source.CompareWith(Comparison.LessThanOrEqual, right);

        /// <summary>Matches text that contains <paramref name="substring"/>.</summary>
        public static TNext Contains<TModel, TText, TNext>(this ConditionSourceBuilder<TModel, TText, TNext> source, string substring)
            where TModel : class where TNext : Condition<TModel> where TText : IEnumerable<char>? =>
            source.CompareWithText(TextTest.Contains, substring);

        /// <summary>Matches text that starts with <paramref name="prefix"/>.</summary>
        public static TNext StartsWith<TModel, TText, TNext>(this ConditionSourceBuilder<TModel, TText, TNext> source, string prefix)
            where TModel : class where TNext : Condition<TModel> where TText : IEnumerable<char>? =>
            source.CompareWithText(TextTest.StartsWith, prefix);

        /// <summary>Matches text that ends with <paramref name="suffix"/>.</summary>
        public static TNext EndsWith<TModel, TText, TNext>(this ConditionSourceBuilder<TModel, TText, TNext> source, string suffix)
            where TModel : class where TNext : Condition<TModel> where TText : IEnumerable<char>? =>
            source.CompareWithText(TextTest.EndsWith, suffix);

        /// <summary>Matches text the regular expression <paramref name="pattern"/> matches.</summary>
        public static TNext Matches<TModel, TText, TNext>(this ConditionSourceBuilder<TModel, TText, TNext> source, string pattern)
            where TModel : class where TNext : Condition<TModel> where TText : IEnumerable<char>? =>
            source.CompareWithText(TextTest.Matches, pattern);

        /// <summary>Matches text at least <paramref name="length"/> characters long.</summary>
        public static TNext MinLength<TModel, TText, TNext>(this ConditionSourceBuilder<TModel, TText, TNext> source, int length)
            where TModel : class where TNext : Condition<TModel> where TText : IEnumerable<char>? =>
            source.CompareWithLength(length);

        /// <summary>Matches an array that holds <paramref name="item"/>.</summary>
        public static TNext ArrayContains<TModel, TItem, TNext>(this ConditionSourceBuilder<TModel, TItem[], TNext> source, TItem item)
            where TModel : class where TNext : Condition<TModel> =>
            source.ContainsItem(item);

        /// <summary>Matches a list that holds <paramref name="item"/>.</summary>
        public static TNext ArrayContains<TModel, TItem, TNext>(this ConditionSourceBuilder<TModel, List<TItem>, TNext> source, TItem item)
            where TModel : class where TNext : Condition<TModel> =>
            source.ContainsItem(item);

        /// <summary>Matches a list that holds <paramref name="item"/>.</summary>
        public static TNext ArrayContains<TModel, TItem, TNext>(this ConditionSourceBuilder<TModel, IReadOnlyList<TItem>, TNext> source, TItem item)
            where TModel : class where TNext : Condition<TModel> =>
            source.ContainsItem(item);

        /// <summary>Matches a collection that holds <paramref name="item"/>.</summary>
        public static TNext ArrayContains<TModel, TItem, TNext>(this ConditionSourceBuilder<TModel, IEnumerable<TItem>, TNext> source, TItem item)
            where TModel : class where TNext : Condition<TModel> =>
            source.ContainsItem(item);
    }
}
