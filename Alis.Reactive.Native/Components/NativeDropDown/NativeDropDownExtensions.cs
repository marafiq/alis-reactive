using Alis.Reactive.Builders.Conditions;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Pipeline extensions for <see cref="NativeDropDown"/>: set selected value, focus, and read.
    /// </summary>
    /// <remarks>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via
    /// <see cref="Builders.PipelineBuilder{TModel}.Component{TComponent}(System.Linq.Expressions.Expression{System.Func{TModel, object}})"/>:
    /// <code>p.Component&lt;NativeDropDown&gt;(m =&gt; m.Status).SetValue("active")</code>
    /// </remarks>
    public static class NativeDropDownExtensions
    {
        private static readonly NativeDropDown _component = new NativeDropDown();

        /// <summary>
        /// Sets the selected option value in the browser.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <param name="self">The dropdown component reference.</param>
        /// <param name="value">The option value to select.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<NativeDropDown, TModel> SetValue<TModel>(
            this ComponentRef<NativeDropDown, TModel> self, string value)
            where TModel : class
        {
            return self.Set("value", value);
        }

        /// <summary>
        /// Moves keyboard focus into the dropdown.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<NativeDropDown, TModel> FocusIn<TModel>(
            this ComponentRef<NativeDropDown, TModel> self)
            where TModel : class
        {
            return self.Call("focus");
        }

        /// <summary>
        /// Reads the currently selected value for use in conditions or gather.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>A typed value expression representing the dropdown's selected value.</returns>
        public static ComponentValueExpression<string> Value<TModel>(
            this ComponentRef<NativeDropDown, TModel> self)
            where TModel : class
        {
            return new ComponentValueExpression<string>(self.TargetId, _component.Vendor, _component.ValueMemberPath);
        }
    }
}
