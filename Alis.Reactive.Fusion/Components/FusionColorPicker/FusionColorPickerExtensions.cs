using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed component operations and value reads for <see cref="FusionColorPicker"/> in a Reactive Plan pipeline.
    /// </summary>
    /// <remarks>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via the pipeline:
    /// <c>p.Component&lt;FusionColorPicker&gt;(m =&gt; m.ThemeColor).SetValue("#ff0000")</c>.
    /// </remarks>
    public static class FusionColorPickerExtensions
    {
        private static readonly FusionColorPicker Component = new FusionColorPicker();

        private static readonly ComponentProperty<string> ValueProperty =
            ComponentProperty<string>.Named(Component.ValueMember);

        private static readonly ComponentProperty<bool> DisabledProperty =
            ComponentProperty<bool>.Named("disabled");

        private static readonly ComponentMethod ToggleMethod =
            ComponentMethod.Named("toggle");

        /// <summary>Sets the color value (hex string, e.g. "#ff0000").</summary>
        /// <param name="value">The hex color string to set, or <see langword="null"/> to clear.</param>
        public static ComponentRef<FusionColorPicker, TModel> SetValue<TModel>(
            this ComponentRef<FusionColorPicker, TModel> self, string? value)
            where TModel : class
            => self.EmitSet(ValueProperty, ValueExpression.LiteralRaw(value, Shape.String));

        /// <summary>Toggles the ColorPicker popup open/closed.</summary>
        public static ComponentRef<FusionColorPicker, TModel> Toggle<TModel>(
            this ComponentRef<FusionColorPicker, TModel> self)
            where TModel : class
            => self.EmitCall(ToggleMethod);

        /// <summary>Sets the disabled state of the ColorPicker.</summary>
        /// <param name="disabled"><see langword="true"/> to disable, <see langword="false"/> to enable.</param>
        public static ComponentRef<FusionColorPicker, TModel> Disable<TModel>(
            this ComponentRef<FusionColorPicker, TModel> self, bool disabled = true)
            where TModel : class
            => self.EmitSet(DisabledProperty, ValueExpression.Literal(disabled));

        /// <summary>Reads the current color value for use in conditions or gather.</summary>
        /// <remarks>
        /// Pass to a <c>When()</c> condition guard or use as a source argument for component operations:
        /// <c>p.When(p.Component&lt;FusionColorPicker&gt;(m =&gt; m.ThemeColor).Value()).NotNull().Then(p =&gt; { ... })</c>.
        /// </remarks>
        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<FusionColorPicker, TModel> self)
            where TModel : class
            => self.Read(ValueProperty);
    }
}
