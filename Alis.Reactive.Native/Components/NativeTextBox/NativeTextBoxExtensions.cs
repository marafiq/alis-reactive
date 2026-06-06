using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Reactive Plan operations for <see cref="NativeTextBox"/> values and focus.
    /// </summary>
    /// <remarks>
    /// Use on a component reference resolved from a pipeline, for example:
    /// <code>p.Component&lt;NativeTextBox&gt;(m =&gt; m.Name).SetValue("hello")</code>
    /// </remarks>
    public static class NativeTextBoxExtensions
    {
        private static readonly ComponentProperty<string> ValueProperty =
            ComponentProperty<string>.Named("value");

        private static readonly ComponentMethod FocusMethod =
            ComponentMethod.Named("focus");

        /// <summary>
        /// Writes the text input value through the component contract.
        /// </summary>
        public static ComponentRef<NativeTextBox, TModel> SetValue<TModel>(
            this ComponentRef<NativeTextBox, TModel> self, string value)
            where TModel : class
        {
            return self.EmitSet(ValueProperty, ValueExpression.Literal(value));
        }

        /// <summary>
        /// Moves keyboard focus into the text input.
        /// </summary>
        public static ComponentRef<NativeTextBox, TModel> FocusIn<TModel>(
            this ComponentRef<NativeTextBox, TModel> self)
            where TModel : class
        {
            return self.EmitCall(FocusMethod);
        }

        /// <summary>
        /// Reads the current text input value for use in conditions or gather.
        /// </summary>
        /// <returns>A typed source representing the input's current value.</returns>
        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<NativeTextBox, TModel> self)
            where TModel : class
        {
            return self.Read(ValueProperty);
        }
    }
}
