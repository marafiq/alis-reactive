using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Reactive Plan operations for <see cref="NativeRadioGroup"/> selected values and focus.
    /// </summary>
    /// <remarks>
    /// Use on a component reference resolved from a pipeline, for example:
    /// <code>p.Component&lt;NativeRadioGroup&gt;(m =&gt; m.Priority).SetValue("high")</code>
    /// </remarks>
    public static class NativeRadioGroupExtensions
    {
        private static readonly ComponentProperty<string> ValueProperty =
            ComponentProperty<string>.Named("value");

        private static readonly ComponentMethod FocusMethod =
            ComponentMethod.Named("focus");

        /// <summary>
        /// Sets the selected radio button value through the component contract.
        /// </summary>
        /// <typeparam name="TModel">The view model that owns the component reference.</typeparam>
        /// <param name="self">The radio group component reference.</param>
        /// <param name="value">The radio option value to select.</param>
        public static ComponentRef<NativeRadioGroup, TModel> SetValue<TModel>(
            this ComponentRef<NativeRadioGroup, TModel> self, string value)
            where TModel : class
        {
            return self.EmitSet(ValueProperty, ValueExpression.Literal(value));
        }

        /// <summary>
        /// Sets the selected radio button value from the current event payload.
        /// </summary>
        /// <typeparam name="TModel">The view model that owns the component reference.</typeparam>
        /// <typeparam name="TSource">The event payload type containing the value.</typeparam>
        /// <param name="self">The radio group component reference.</param>
        /// <param name="source">The typed event payload marker supplied by the trigger callback.</param>
        /// <param name="path">Expression selecting the event payload property to read.</param>
        public static ComponentRef<NativeRadioGroup, TModel> SetValue<TModel, TSource>(
            this ComponentRef<NativeRadioGroup, TModel> self,
            TSource source, Expression<Func<TSource, object?>> path)
            where TModel : class
        {
            var sourcePath = ExpressionPathHelper.ToEventPath(path);
            return self.EmitSet(ValueProperty, ValueExpression.Read(PayloadSource.Event(), "value", Path.Parse(sourcePath)));
        }

        /// <summary>
        /// Moves keyboard focus into the radio group.
        /// </summary>
        /// <typeparam name="TModel">The view model that owns the component reference.</typeparam>
        public static ComponentRef<NativeRadioGroup, TModel> FocusIn<TModel>(
            this ComponentRef<NativeRadioGroup, TModel> self)
            where TModel : class
        {
            return self.EmitCall(FocusMethod);
        }

        /// <summary>
        /// Reads the currently selected value for use in conditions or gather.
        /// </summary>
        /// <typeparam name="TModel">The view model that owns the component reference.</typeparam>
        /// <returns>A typed source representing the radio group's selected value.</returns>
        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<NativeRadioGroup, TModel> self)
            where TModel : class
        {
            return self.Read(ValueProperty);
        }
    }
}
