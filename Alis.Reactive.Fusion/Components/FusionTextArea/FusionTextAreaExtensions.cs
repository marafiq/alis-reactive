using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed component operations and value reads for <see cref="FusionTextArea"/> in a Reactive Plan pipeline.
    /// </summary>
    public static class FusionTextAreaExtensions
    {
        private static readonly FusionTextArea Component = new FusionTextArea();

        private static readonly ComponentProperty<string> ValueProperty =
            ComponentProperty<string>.Named(Component.ValueMember);

        private static readonly ComponentMethod DataBindMethod =
            ComponentMethod.Named("dataBind");

        private static readonly ComponentMethod FocusInMethod =
            ComponentMethod.Named("focusIn");

        private static readonly ComponentMethod FocusOutMethod =
            ComponentMethod.Named("focusOut");

        /// <summary>Sets the visible textarea value.</summary>
        /// <param name="value">Text value, or <see langword="null"/> to clear.</param>
        public static ComponentRef<FusionTextArea, TModel> SetValue<TModel>(
            this ComponentRef<FusionTextArea, TModel> self, string? value)
            where TModel : class
            => self
                .EmitSet(ValueProperty, ValueExpression.LiteralRaw(value, Shape.String))
                .EmitCall(DataBindMethod);

        /// <summary>Moves focus into the textarea.</summary>
        public static ComponentRef<FusionTextArea, TModel> FocusIn<TModel>(
            this ComponentRef<FusionTextArea, TModel> self)
            where TModel : class
            => self.EmitCall(FocusInMethod);

        /// <summary>Removes focus from the textarea.</summary>
        public static ComponentRef<FusionTextArea, TModel> FocusOut<TModel>(
            this ComponentRef<FusionTextArea, TModel> self)
            where TModel : class
            => self.EmitCall(FocusOutMethod);

        /// <summary>Reads the text value for conditions or gather.</summary>
        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<FusionTextArea, TModel> self)
            where TModel : class
            => self.Read(ValueProperty);
    }
}
