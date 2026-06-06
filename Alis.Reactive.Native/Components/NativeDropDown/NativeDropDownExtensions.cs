using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Reactive Plan operations for <see cref="NativeDropDown"/> selected value and focus.
    /// </summary>
    /// <remarks>
    /// Use on a component reference resolved from a pipeline, for example:
    /// <code>p.Component&lt;NativeDropDown&gt;(m =&gt; m.Status).SetValue("active")</code>
    /// </remarks>
    public static class NativeDropDownExtensions
    {
        private static readonly ComponentProperty<string> ValueProperty =
            ComponentProperty<string>.Named("value");

        private static readonly ComponentMethod FocusMethod =
            ComponentMethod.Named("focus");

        /// <summary>
        /// Writes the selected option value through the component contract.
        /// </summary>
        /// <typeparam name="TModel">The view model that owns the component reference.</typeparam>
        /// <param name="self">The dropdown component reference.</param>
        /// <param name="value">The option value to select.</param>
        public static ComponentRef<NativeDropDown, TModel> SetValue<TModel>(
            this ComponentRef<NativeDropDown, TModel> self, string value)
            where TModel : class
        {
            return self.EmitSet(ValueProperty, ValueExpression.Literal(value));
        }

        /// <summary>
        /// Moves keyboard focus into the dropdown.
        /// </summary>
        /// <typeparam name="TModel">The view model that owns the component reference.</typeparam>
        public static ComponentRef<NativeDropDown, TModel> FocusIn<TModel>(
            this ComponentRef<NativeDropDown, TModel> self)
            where TModel : class
        {
            return self.EmitCall(FocusMethod);
        }

        /// <summary>
        /// Reads the currently selected value for use in conditions or gather.
        /// </summary>
        /// <typeparam name="TModel">The view model that owns the component reference.</typeparam>
        /// <returns>A typed source representing the dropdown's selected value.</returns>
        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<NativeDropDown, TModel> self)
            where TModel : class
        {
            return self.Read(ValueProperty);
        }
    }
}
