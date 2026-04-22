using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Pipeline extensions for <see cref="NativeCheckBox"/>: set checked state, focus-in.
    /// Typed Value&lt;TProp&gt;() and SetValue&lt;TProp&gt;(value) are provided by the
    /// <see cref="InputComponentRef{TComponent, TModel}"/> base class. SetChecked is a
    /// semantic alias specific to checkboxes.
    /// </summary>
    public static class NativeCheckBoxExtensions
    {
        /// <summary>
        /// Sets the checkbox checked state in the browser. Semantic alias for
        /// <c>.SetValue(isChecked)</c>.
        /// </summary>
        public static ComponentRef<NativeCheckBox, TModel> SetChecked<TModel>(
            this ComponentRef<NativeCheckBox, TModel> self, bool isChecked)
            where TModel : class
            => self.EmitSet("checked", ValueProducer.Literal(isChecked));

        /// <summary>
        /// Moves keyboard focus into the checkbox.
        /// </summary>
        public static ComponentRef<NativeCheckBox, TModel> FocusIn<TModel>(
            this ComponentRef<NativeCheckBox, TModel> self)
            where TModel : class
            => self.EmitCall("focus");
    }
}
