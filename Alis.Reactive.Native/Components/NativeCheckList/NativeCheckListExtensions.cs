using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Reactive Plan operations for <see cref="NativeCheckList"/> checked values and focus.
    /// </summary>
    /// <remarks>
    /// Use on a component reference resolved from a pipeline, for example:
    /// <code>p.Component&lt;NativeCheckList&gt;(m =&gt; m.Allergies).SetValue(new[] { "peanuts" })</code>
    /// </remarks>
    public static class NativeCheckListExtensions
    {
        private static readonly ComponentProperty<string[]> ValueProperty =
            ComponentProperty<string[]>.Named("value");

        private static readonly ComponentMethod FocusMethod =
            ComponentMethod.Named("focus");

        /// <summary>
        /// Writes checked values through the component contract.
        /// </summary>
        /// <param name="value">The array of option values to check.</param>
        public static ComponentRef<NativeCheckList, TModel> SetValue<TModel>(
            this ComponentRef<NativeCheckList, TModel> self, string[] value)
            where TModel : class
        {
            var items = new System.Collections.Generic.List<ValueExpression>();
            foreach (var selectedValue in value)
                items.Add(ValueExpression.Literal(selectedValue));
            return self.EmitSet(ValueProperty, ValueExpression.Array(items));
        }

        /// <summary>
        /// Writes checked values from the current event payload.
        /// </summary>
        /// <typeparam name="TSource">The event payload type containing the values.</typeparam>
        /// <param name="source">The typed event payload marker supplied by the trigger callback.</param>
        /// <param name="path">Expression selecting the event payload property to read.</param>
        public static ComponentRef<NativeCheckList, TModel> SetValue<TModel, TSource>(
            this ComponentRef<NativeCheckList, TModel> self,
            TSource source, Expression<Func<TSource, object?>> path)
            where TModel : class
        {
            var sourcePath = ExpressionPathHelper.ToEventPath(path);
            return self.EmitSet(ValueProperty, ValueExpression.Read(PayloadSource.Event(), "value", Path.Parse(sourcePath)));
        }

        /// <summary>
        /// Moves keyboard focus into the check list.
        /// </summary>
        public static ComponentRef<NativeCheckList, TModel> FocusIn<TModel>(
            this ComponentRef<NativeCheckList, TModel> self)
            where TModel : class
        {
            return self.EmitCall(FocusMethod);
        }

        /// <summary>
        /// Reads the currently checked values for use in conditions or gather.
        /// </summary>
        public static TypedComponentSource<string[]> Value<TModel>(
            this ComponentRef<NativeCheckList, TModel> self)
            where TModel : class
        {
            return self.Read(ValueProperty);
        }
    }
}
