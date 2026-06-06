using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed post-render component operations and reads for <see cref="FusionRadioButton"/>.
    /// </summary>
    public static class FusionRadioButtonExtensions
    {
        private static readonly ComponentProperty<bool> CheckedProperty =
            ComponentProperty<bool>.Named("checked");

        private static readonly ComponentProperty<bool> DisabledProperty =
            ComponentProperty<bool>.Named("disabled");

        private static readonly ComponentMethod DataBindMethod =
            ComponentMethod.Named("dataBind");

        private static readonly ComponentMethod SelectedValueMethod =
            ComponentMethod.Named("getSelectedValue");

        private static readonly ComponentMethod ClickMethod =
            ComponentMethod.Named("click");

        private static readonly ComponentMethod FocusInMethod =
            ComponentMethod.Named("focusIn");

        /// <summary>Sets whether this radio button is checked.</summary>
        public static ComponentRef<FusionRadioButton, TModel> SetChecked<TModel>(
            this ComponentRef<FusionRadioButton, TModel> self,
            bool isChecked)
            where TModel : class
            => self
                .EmitSet(CheckedProperty, ValueExpression.Literal(isChecked))
                .EmitCall(DataBindMethod);

        /// <summary>Sets whether this radio button is disabled.</summary>
        public static ComponentRef<FusionRadioButton, TModel> SetDisabled<TModel>(
            this ComponentRef<FusionRadioButton, TModel> self,
            bool disabled)
            where TModel : class
            => self
                .EmitSet(DisabledProperty, ValueExpression.Literal(disabled))
                .EmitCall(DataBindMethod);

        /// <summary>Invokes the rendered radio button click.</summary>
        public static ComponentRef<FusionRadioButton, TModel> Click<TModel>(
            this ComponentRef<FusionRadioButton, TModel> self)
            where TModel : class
            => self.EmitCall(ClickMethod);

        /// <summary>Moves focus into the rendered radio button.</summary>
        public static ComponentRef<FusionRadioButton, TModel> FocusIn<TModel>(
            this ComponentRef<FusionRadioButton, TModel> self)
            where TModel : class
            => self.EmitCall(FocusInMethod);

        /// <summary>Reads whether this radio button is currently checked.</summary>
        public static TypedComponentSource<bool> Checked<TModel>(
            this ComponentRef<FusionRadioButton, TModel> self)
            where TModel : class
            => self.Read(CheckedProperty);

        /// <summary>Reads whether this radio button is currently disabled.</summary>
        public static TypedComponentSource<bool> Disabled<TModel>(
            this ComponentRef<FusionRadioButton, TModel> self)
            where TModel : class
            => self.Read(DisabledProperty);

        /// <summary>Reads the selected value from this radio button's named group.</summary>
        public static TypedComponentSource<string> SelectedValue<TModel>(
            this ComponentRef<FusionRadioButton, TModel> self)
            where TModel : class
            => self.Read<string>(SelectedValueMethod);
    }
}
