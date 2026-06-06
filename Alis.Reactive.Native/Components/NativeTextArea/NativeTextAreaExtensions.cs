using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Reactive Plan operations for <see cref="NativeTextArea"/> values and focus.
    /// </summary>
    /// <remarks>
    /// Use on a component reference resolved from a pipeline, for example:
    /// <code>p.Component&lt;NativeTextArea&gt;(m =&gt; m.Notes).SetValue("updated")</code>
    /// </remarks>
    public static class NativeTextAreaExtensions
    {
        private static readonly ComponentProperty<string> ValueProperty =
            ComponentProperty<string>.Named("value");

        private static readonly ComponentMethod FocusMethod =
            ComponentMethod.Named("focus");

        /// <summary>
        /// Writes the textarea value through the component contract.
        /// </summary>
        public static ComponentRef<NativeTextArea, TModel> SetValue<TModel>(
            this ComponentRef<NativeTextArea, TModel> self, string value)
            where TModel : class
        {
            return self.EmitSet(ValueProperty, ValueExpression.Literal(value));
        }

        /// <summary>
        /// Moves keyboard focus into the textarea.
        /// </summary>
        public static ComponentRef<NativeTextArea, TModel> FocusIn<TModel>(
            this ComponentRef<NativeTextArea, TModel> self)
            where TModel : class
        {
            return self.EmitCall(FocusMethod);
        }

        /// <summary>
        /// Reads the current textarea value for use in conditions or gather.
        /// </summary>
        /// <returns>A typed source representing the textarea's current value.</returns>
        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<NativeTextArea, TModel> self)
            where TModel : class
        {
            return self.Read(ValueProperty);
        }
    }
}
