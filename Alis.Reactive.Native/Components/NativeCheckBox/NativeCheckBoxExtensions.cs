using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Reactive Plan operations for <see cref="NativeCheckBox"/> checked state and focus.
    /// </summary>
    /// <remarks>
    /// Use on a component reference resolved from a pipeline, for example:
    /// <code>p.Component&lt;NativeCheckBox&gt;(m =&gt; m.IsActive).SetChecked(true)</code>
    /// </remarks>
    public static class NativeCheckBoxExtensions
    {
        private static readonly ComponentProperty<bool> CheckedProperty =
            ComponentProperty<bool>.Named("checked");

        private static readonly ComponentMethod FocusMethod =
            ComponentMethod.Named("focus");

        /// <summary>
        /// Writes the checkbox checked state through the component contract.
        /// </summary>
        /// <param name="isChecked"><see langword="true"/> to check, <see langword="false"/> to uncheck.</param>
        public static ComponentRef<NativeCheckBox, TModel> SetChecked<TModel>(
            this ComponentRef<NativeCheckBox, TModel> self, bool isChecked)
            where TModel : class
        {
            return self.EmitSet(CheckedProperty, ValueExpression.Literal(isChecked));
        }

        /// <summary>
        /// Moves keyboard focus into the checkbox.
        /// </summary>
        public static ComponentRef<NativeCheckBox, TModel> FocusIn<TModel>(
            this ComponentRef<NativeCheckBox, TModel> self)
            where TModel : class
        {
            return self.EmitCall(FocusMethod);
        }

        /// <summary>
        /// Reads the checked state for conditions or gather.
        /// </summary>
        public static TypedComponentSource<bool> Value<TModel>(
            this ComponentRef<NativeCheckBox, TModel> self)
            where TModel : class
        {
            return self.Read(CheckedProperty);
        }
    }
}
