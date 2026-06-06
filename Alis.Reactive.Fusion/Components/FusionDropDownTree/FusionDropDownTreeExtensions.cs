using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Reads and updates <see cref="FusionDropDownTree"/> values from a Reactive Plan pipeline.
    /// </summary>
    /// <remarks>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via the pipeline:
    /// <c>p.Component&lt;FusionDropDownTree&gt;(m =&gt; m.ResidentIds).SetValue(new[] { "alice" })</c>.
    /// </remarks>
    public static class FusionDropDownTreeExtensions
    {
        private static readonly FusionDropDownTree Component = new FusionDropDownTree();

        private static readonly ComponentProperty<string[]> ValueProperty =
            ComponentProperty<string[]>.Named(Component.ValueMember);

        private static readonly ComponentProperty<string?> TextProperty =
            ComponentProperty<string?>.Named("text");

        private static readonly ComponentMethod DataBindMethod =
            ComponentMethod.Named("dataBind");

        private static readonly ComponentMethod ShowPopupMethod =
            ComponentMethod.Named("showPopup");

        private static readonly ComponentMethod HidePopupMethod =
            ComponentMethod.Named("hidePopup");

        private static readonly ComponentMethod ClearMethod =
            ComponentMethod.Named("clear");

        /// <summary>Sets selected value IDs.</summary>
        /// <param name="value">Selected IDs, or <see langword="null"/> to clear.</param>
        public static ComponentRef<FusionDropDownTree, TModel> SetValue<TModel>(
            this ComponentRef<FusionDropDownTree, TModel> self, string[]? value)
            where TModel : class
            => self.EmitSet(ValueProperty, value == null
                ? ValueExpression.Null()
                : ValueExpression.LiteralRaw(value, Shape.ArrayOf(Shape.String)));

        /// <summary>Sets display text and lets Syncfusion map it back to the selected value ID.</summary>
        /// <param name="text">Display text, or <see langword="null"/> to clear.</param>
        public static ComponentRef<FusionDropDownTree, TModel> SetText<TModel>(
            this ComponentRef<FusionDropDownTree, TModel> self, string? text)
            where TModel : class
            => self.EmitSet(TextProperty, ValueExpression.LiteralRaw(text, Shape.String));

        /// <summary>Applies pending DropDownTree property changes to the rendered component.</summary>
        public static ComponentRef<FusionDropDownTree, TModel> DataBind<TModel>(
            this ComponentRef<FusionDropDownTree, TModel> self)
            where TModel : class
            => self.EmitCall(DataBindMethod);

        /// <summary>Opens the DropDownTree popup.</summary>
        public static ComponentRef<FusionDropDownTree, TModel> ShowPopup<TModel>(
            this ComponentRef<FusionDropDownTree, TModel> self)
            where TModel : class
            => self.EmitCall(ShowPopupMethod);

        /// <summary>Closes the DropDownTree popup.</summary>
        public static ComponentRef<FusionDropDownTree, TModel> HidePopup<TModel>(
            this ComponentRef<FusionDropDownTree, TModel> self)
            where TModel : class
            => self.EmitCall(HidePopupMethod);

        /// <summary>Clears selected values and display text.</summary>
        public static ComponentRef<FusionDropDownTree, TModel> Clear<TModel>(
            this ComponentRef<FusionDropDownTree, TModel> self)
            where TModel : class
            => self.EmitCall(ClearMethod);

        /// <summary>Reads selected value IDs.</summary>
        public static TypedComponentSource<string[]> Value<TModel>(
            this ComponentRef<FusionDropDownTree, TModel> self)
            where TModel : class
            => self.Read(ValueProperty);

        /// <summary>Reads displayed selection text.</summary>
        public static TypedComponentSource<string?> Text<TModel>(
            this ComponentRef<FusionDropDownTree, TModel> self)
            where TModel : class
            => self.Read(TextProperty);
    }
}
