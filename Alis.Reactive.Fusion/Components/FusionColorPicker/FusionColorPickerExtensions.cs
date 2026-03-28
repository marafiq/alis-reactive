using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed mutations and value reading for <see cref="FusionColorPicker"/> in a reactive pipeline.
    /// </summary>
    /// <remarks>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via the pipeline:
    /// <c>p.Component&lt;FusionColorPicker&gt;(m =&gt; m.ThemeColor).SetValue("#ff0000")</c>.
    /// </remarks>
    public static class FusionColorPickerExtensions
    {
        private static readonly FusionColorPicker Component = new FusionColorPicker();

        /// <summary>Sets the color value (hex string, e.g. "#ff0000").</summary>
        /// <param name="value">The hex color string to set, or <see langword="null"/> to clear.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionColorPicker, TModel> SetValue<TModel>(
            this ComponentRef<FusionColorPicker, TModel> self, string? value)
            where TModel : class
            => self.Emit(new SetPropMutation("value"), value: value);

        /// <summary>Toggles the ColorPicker popup open/closed.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionColorPicker, TModel> Toggle<TModel>(
            this ComponentRef<FusionColorPicker, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("toggle"));

        /// <summary>Sets the disabled state of the ColorPicker.</summary>
        /// <param name="disabled"><see langword="true"/> to disable, <see langword="false"/> to enable.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionColorPicker, TModel> Disable<TModel>(
            this ComponentRef<FusionColorPicker, TModel> self, bool disabled = true)
            where TModel : class
            => self.Emit(new SetPropMutation("disabled", coerce: "boolean"),
                value: disabled ? "true" : "false");

        /// <summary>Reads the current color value for use in conditions or gather.</summary>
        /// <returns>A typed source representing the color picker's current hex value.</returns>
        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<FusionColorPicker, TModel> self)
            where TModel : class
            => new TypedComponentSource<string>(self.TargetId, Component.Vendor, Component.ReadExpr);
    }
}
