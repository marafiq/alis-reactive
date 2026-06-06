using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Reactive Plan operations for <see cref="NativeRadioGroup"/> selected value and focus.
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
        /// Writes the selected radio value through the component contract.
        /// </summary>
        /// <param name="value">The radio option value to select.</param>
        public static ComponentRef<NativeRadioGroup, TModel> SetValue<TModel>(
            this ComponentRef<NativeRadioGroup, TModel> self, string value)
            where TModel : class
        {
            return self.EmitSet(ValueProperty, ValueExpression.Literal(value));
        }

        /// <summary>
        /// Writes the selected radio value from the triggering event payload.
        /// </summary>
        /// <typeparam name="TSource">The event payload type containing the value.</typeparam>
        /// <param name="source">Trigger payload placeholder.</param>
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
        public static ComponentRef<NativeRadioGroup, TModel> FocusIn<TModel>(
            this ComponentRef<NativeRadioGroup, TModel> self)
            where TModel : class
        {
            return self.EmitCall(FocusMethod);
        }

        /// <summary>
        /// Reads the currently selected value for use in conditions or gather.
        /// </summary>
        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<NativeRadioGroup, TModel> self)
            where TModel : class
        {
            return self.Read(ValueProperty);
        }
    }
}
