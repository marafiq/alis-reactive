using System.Collections.Generic;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed component operations and value reads for <see cref="FusionTextBox"/> in a Reactive Plan pipeline.
    /// </summary>
    public static class FusionTextBoxExtensions
    {
        private static readonly FusionTextBox Component = new FusionTextBox();

        private static readonly ComponentProperty<string> ValueProperty =
            ComponentProperty<string>.Named(Component.ValueMember);

        private static readonly ComponentMethod DataBindMethod =
            ComponentMethod.Named("dataBind");

        private static readonly ComponentMethod FocusInMethod =
            ComponentMethod.Named("focusIn");

        private static readonly ComponentMethod FocusOutMethod =
            ComponentMethod.Named("focusOut");

        private static readonly ComponentMethod AddAppendIconMethod =
            ComponentMethod.Mapped("addAppendIcon", "addIcon").WithArgs<string, string>();

        /// <summary>Sets the visible textbox value.</summary>
        /// <param name="value">The text to set, or <see langword="null"/> to clear.</param>
        public static ComponentRef<FusionTextBox, TModel> SetValue<TModel>(
            this ComponentRef<FusionTextBox, TModel> self, string? value)
            where TModel : class
            => self
                .EmitSet(ValueProperty, ValueExpression.LiteralRaw(value, Shape.String))
                .EmitCall(DataBindMethod);

        /// <summary>Moves focus into the textbox.</summary>
        public static ComponentRef<FusionTextBox, TModel> FocusIn<TModel>(
            this ComponentRef<FusionTextBox, TModel> self)
            where TModel : class
            => self.EmitCall(FocusInMethod);

        /// <summary>Removes focus from the textbox.</summary>
        public static ComponentRef<FusionTextBox, TModel> FocusOut<TModel>(
            this ComponentRef<FusionTextBox, TModel> self)
            where TModel : class
            => self.EmitCall(FocusOutMethod);

        /// <summary>Adds an append icon to the textbox input.</summary>
        /// <param name="iconCssClass">CSS classes for the icon.</param>
        public static ComponentRef<FusionTextBox, TModel> AddAppendIcon<TModel>(
            this ComponentRef<FusionTextBox, TModel> self,
            string iconCssClass)
            where TModel : class
            => self.EmitCall(
                AddAppendIconMethod,
                new List<ValueExpression>
                {
                    ValueExpression.Literal("append"),
                    ValueExpression.Literal(iconCssClass)
                });

        /// <summary>Reads the current text value for use in conditions or gather.</summary>
        /// <returns>A typed source representing the textbox's current value.</returns>
        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<FusionTextBox, TModel> self)
            where TModel : class
            => self.Read(ValueProperty);
    }
}
