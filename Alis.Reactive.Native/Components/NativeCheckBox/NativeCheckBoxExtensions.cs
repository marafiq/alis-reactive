using Alis.Reactive.Builders.Conditions;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Pipeline extensions for <see cref="NativeCheckBox"/>: set checked state, focus, and read.
    /// </summary>
    /// <remarks>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via
    /// <see cref="Builders.PipelineBuilder{TModel}.Component{TComponent}(System.Linq.Expressions.Expression{System.Func{TModel, object}})"/>:
    /// <code>p.Component&lt;NativeCheckBox&gt;(m =&gt; m.IsActive).SetChecked(true)</code>
    /// </remarks>
    public static class NativeCheckBoxExtensions
    {
        private static readonly NativeCheckBox _component = new NativeCheckBox();

        /// <summary>
        /// Sets the checkbox checked state in the browser.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <param name="self">The checkbox component reference.</param>
        /// <param name="isChecked"><see langword="true"/> to check, <see langword="false"/> to uncheck.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<NativeCheckBox, TModel> SetChecked<TModel>(
            this ComponentRef<NativeCheckBox, TModel> self, bool isChecked)
            where TModel : class
        {
            return self.Set("checked", isChecked, coerceAs: "boolean");
        }

        /// <summary>
        /// Moves keyboard focus into the checkbox.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<NativeCheckBox, TModel> FocusIn<TModel>(
            this ComponentRef<NativeCheckBox, TModel> self)
            where TModel : class
        {
            return self.Call("focus");
        }

        /// <summary>
        /// Reads the current checked state for use in conditions or gather.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>A typed value expression representing the checkbox's current checked state.</returns>
        public static ComponentValueExpression<bool> Value<TModel>(
            this ComponentRef<NativeCheckBox, TModel> self)
            where TModel : class
        {
            return new ComponentValueExpression<bool>(self.TargetId, _component.Vendor, _component.ValueMemberPath);
        }
    }
}
