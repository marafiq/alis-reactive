using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;

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
            return self.Emit(new SetPropMutation("value"), value: value);
        }

        /// <summary>
        /// Moves keyboard focus into the textarea.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<NativeTextArea, TModel> FocusIn<TModel>(
            this ComponentRef<NativeTextArea, TModel> self)
            where TModel : class
        {
            return self.Emit(new CallMutation("focus"));
        }

        /// <summary>
        /// Reads the current textarea value for use in conditions or gather.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <returns>A typed source representing the textarea's current value.</returns>
        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<NativeTextArea, TModel> self)
            where TModel : class
        {
            return new TypedComponentSource<string>(self.TargetId, _component.Vendor, _component.ReadExpr);
        }
    }
}
