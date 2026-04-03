using Alis.Reactive.Builders.Conditions;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Pipeline extensions for <see cref="NativeTextArea"/>: set value, focus, and read.
    /// </summary>
    /// <remarks>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via
    /// <see cref="Builders.PipelineBuilder{TModel}.Component{TComponent}(System.Linq.Expressions.Expression{System.Func{TModel, object}})"/>:
    /// <code>p.Component&lt;NativeTextArea&gt;(m =&gt; m.Notes).SetValue("updated")</code>
    /// </remarks>
    public static class NativeTextAreaExtensions
    {
        private static readonly NativeTextArea _component = new NativeTextArea();

        /// <summary>
        /// Sets the textarea value in the browser.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <param name="self">The textarea component reference.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<NativeTextArea, TModel> SetValue<TModel>(
            this ComponentRef<NativeTextArea, TModel> self, string value)
            where TModel : class
        {
            return self.Set("value", value);
        }

        /// <summary>
        /// Moves keyboard focus into the textarea.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<NativeTextArea, TModel> FocusIn<TModel>(
            this ComponentRef<NativeTextArea, TModel> self)
            where TModel : class
        {
            return self.Call("focus");
        }

        /// <summary>
        /// Reads the current textarea value for use in conditions or gather.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>A typed value expression representing the textarea's current value.</returns>
        public static ComponentValueExpression<string> Value<TModel>(
            this ComponentRef<NativeTextArea, TModel> self)
            where TModel : class
        {
            return new ComponentValueExpression<string>(self.TargetId, _component.Vendor, _component.ValueMemberPath);
        }
    }
}
