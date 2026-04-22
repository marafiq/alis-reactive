using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed mutations for <see cref="FusionColorPicker"/> beyond Value/SetValue.
    /// Typed Value&lt;TProp&gt;() and SetValue&lt;TProp&gt;(...) are provided by the
    /// <see cref="InputComponentRef{TComponent, TModel}"/> base class.
    /// </summary>
    public static class FusionColorPickerExtensions
    {
        /// <summary>Toggles the ColorPicker popup open/closed.</summary>
        public static ComponentRef<FusionColorPicker, TModel> Toggle<TModel>(
            this ComponentRef<FusionColorPicker, TModel> self)
            where TModel : class
            => self.EmitCall("toggle");

        /// <summary>Sets the disabled state of the ColorPicker.</summary>
        public static ComponentRef<FusionColorPicker, TModel> Disable<TModel>(
            this ComponentRef<FusionColorPicker, TModel> self, bool disabled = true)
            where TModel : class
            => self.EmitSet("disabled", ValueProducer.Literal(disabled));
    }
}
