using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed component operations and value reads for <see cref="FusionComboBox"/> in a Reactive Plan pipeline.
    /// </summary>
    /// <remarks>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via the pipeline:
    /// <c>p.Component&lt;FusionComboBox&gt;(m =&gt; m.Resident).SetValue("alice")</c>.
    /// </remarks>
    public static class FusionComboBoxExtensions
    {
        private static readonly FusionComboBox Component = new FusionComboBox();

        private static readonly ComponentProperty<string?> ValueProperty =
            ComponentProperty<string?>.Named(Component.ValueMember);

        private static readonly ComponentProperty<string?> TextProperty =
            ComponentProperty<string?>.Named("text");

        private static readonly ComponentProperty<int?> IndexProperty =
            ComponentProperty<int?>.Named("index");

        private static readonly ComponentMethod DataBindMethod =
            ComponentMethod.Named("dataBind");

        private static readonly ComponentMethod FocusInMethod =
            ComponentMethod.Named("focusIn");

        private static readonly ComponentMethod FocusOutMethod =
            ComponentMethod.Named("focusOut");

        private static readonly ComponentMethod ShowPopupMethod =
            ComponentMethod.Named("showPopup");

        private static readonly ComponentMethod HidePopupMethod =
            ComponentMethod.Named("hidePopup");

        private static readonly ComponentMethod ClearMethod =
            ComponentMethod.Named("clear");

        /// <summary>Sets the selected value, or clears it with <see langword="null"/>.</summary>
        /// <param name="value">The string value to select.</param>
        public static ComponentRef<FusionComboBox, TModel> SetValue<TModel>(
            this ComponentRef<FusionComboBox, TModel> self, string? value)
            where TModel : class
            => self.EmitSet(ValueProperty, ValueExpression.LiteralRaw(value, Shape.String));

        /// <summary>Sets the displayed text, or clears it with <see langword="null"/>.</summary>
        /// <param name="text">The display text to select.</param>
        public static ComponentRef<FusionComboBox, TModel> SetText<TModel>(
            this ComponentRef<FusionComboBox, TModel> self, string? text)
            where TModel : class
            => self.EmitSet(TextProperty, ValueExpression.LiteralRaw(text, Shape.String));

        /// <summary>Sets the selected list index.</summary>
        /// <param name="index">The zero-based list index to select.</param>
        public static ComponentRef<FusionComboBox, TModel> SetIndex<TModel>(
            this ComponentRef<FusionComboBox, TModel> self, int index)
            where TModel : class
            => self.EmitSet(IndexProperty, ValueExpression.Literal(index));

        /// <summary>Flushes pending property changes through the Syncfusion ComboBox instance.</summary>
        public static ComponentRef<FusionComboBox, TModel> DataBind<TModel>(
            this ComponentRef<FusionComboBox, TModel> self)
            where TModel : class
            => self.EmitCall(DataBindMethod);

        /// <summary>Moves focus into the ComboBox input.</summary>
        public static ComponentRef<FusionComboBox, TModel> FocusIn<TModel>(
            this ComponentRef<FusionComboBox, TModel> self)
            where TModel : class
            => self.EmitCall(FocusInMethod);

        /// <summary>Removes focus from the ComboBox input.</summary>
        public static ComponentRef<FusionComboBox, TModel> FocusOut<TModel>(
            this ComponentRef<FusionComboBox, TModel> self)
            where TModel : class
            => self.EmitCall(FocusOutMethod);

        /// <summary>Opens the ComboBox popup.</summary>
        public static ComponentRef<FusionComboBox, TModel> ShowPopup<TModel>(
            this ComponentRef<FusionComboBox, TModel> self)
            where TModel : class
            => self.EmitCall(ShowPopupMethod);

        /// <summary>Closes the ComboBox popup.</summary>
        public static ComponentRef<FusionComboBox, TModel> HidePopup<TModel>(
            this ComponentRef<FusionComboBox, TModel> self)
            where TModel : class
            => self.EmitCall(HidePopupMethod);

        /// <summary>Clears the selected value, text, and index.</summary>
        public static ComponentRef<FusionComboBox, TModel> Clear<TModel>(
            this ComponentRef<FusionComboBox, TModel> self)
            where TModel : class
            => self.EmitCall(ClearMethod);

        /// <summary>Reads the current selected value for use in conditions or gather.</summary>
        /// <returns>A typed source representing the ComboBox's current value.</returns>
        public static TypedComponentSource<string?> Value<TModel>(
            this ComponentRef<FusionComboBox, TModel> self)
            where TModel : class
            => self.Read(ValueProperty);

        /// <summary>Reads the current display text for use in conditions or gather.</summary>
        /// <returns>A typed source representing the ComboBox's current display text.</returns>
        public static TypedComponentSource<string?> Text<TModel>(
            this ComponentRef<FusionComboBox, TModel> self)
            where TModel : class
            => self.Read(TextProperty);

        /// <summary>Reads the current selected index for use in conditions or gather.</summary>
        /// <returns>A typed source representing the ComboBox's current selected index.</returns>
        public static TypedComponentSource<int?> Index<TModel>(
            this ComponentRef<FusionComboBox, TModel> self)
            where TModel : class
            => self.Read(IndexProperty);
    }
}
