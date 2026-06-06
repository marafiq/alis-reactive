using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed post-render component operations and reads for <see cref="FusionCheckBox"/>.
    /// </summary>
    public static class FusionCheckBoxExtensions
    {
        private static readonly FusionCheckBox Component = new FusionCheckBox();

        private static readonly ComponentProperty<bool> CheckedProperty =
            ComponentProperty<bool>.Named(Component.ValueMember);

        private static readonly ComponentProperty<bool> IndeterminateProperty =
            ComponentProperty<bool>.Named("indeterminate");

        private static readonly ComponentProperty<bool> DisabledProperty =
            ComponentProperty<bool>.Named("disabled");

        private static readonly ComponentMethod DataBindMethod =
            ComponentMethod.Named("dataBind");

        private static readonly ComponentMethod ClickMethod =
            ComponentMethod.Named("click");

        private static readonly ComponentMethod FocusInMethod =
            ComponentMethod.Named("focusIn");

        /// <summary>Sets whether the checkbox is checked.</summary>
        public static ComponentRef<FusionCheckBox, TModel> SetChecked<TModel>(
            this ComponentRef<FusionCheckBox, TModel> self,
            bool isChecked)
            where TModel : class
            => self
                .EmitSet(CheckedProperty, ValueExpression.Literal(isChecked))
                .EmitCall(DataBindMethod);

        /// <summary>Sets whether the checkbox is indeterminate.</summary>
        public static ComponentRef<FusionCheckBox, TModel> SetIndeterminate<TModel>(
            this ComponentRef<FusionCheckBox, TModel> self,
            bool isIndeterminate)
            where TModel : class
            => self
                .EmitSet(IndeterminateProperty, ValueExpression.Literal(isIndeterminate))
                .EmitCall(DataBindMethod);

        /// <summary>Sets whether the checkbox is disabled.</summary>
        public static ComponentRef<FusionCheckBox, TModel> SetDisabled<TModel>(
            this ComponentRef<FusionCheckBox, TModel> self,
            bool disabled)
            where TModel : class
            => self
                .EmitSet(DisabledProperty, ValueExpression.Literal(disabled))
                .EmitCall(DataBindMethod);

        /// <summary>Invokes the rendered checkbox click.</summary>
        public static ComponentRef<FusionCheckBox, TModel> Click<TModel>(
            this ComponentRef<FusionCheckBox, TModel> self)
            where TModel : class
            => self.EmitCall(ClickMethod);

        /// <summary>Moves focus into the rendered checkbox.</summary>
        public static ComponentRef<FusionCheckBox, TModel> FocusIn<TModel>(
            this ComponentRef<FusionCheckBox, TModel> self)
            where TModel : class
            => self.EmitCall(FocusInMethod);

        /// <summary>Reads whether the checkbox is currently checked.</summary>
        public static TypedComponentSource<bool> Checked<TModel>(
            this ComponentRef<FusionCheckBox, TModel> self)
            where TModel : class
            => self.Read(CheckedProperty);

        /// <summary>Reads whether the checkbox is currently indeterminate.</summary>
        public static TypedComponentSource<bool> Indeterminate<TModel>(
            this ComponentRef<FusionCheckBox, TModel> self)
            where TModel : class
            => self.Read(IndeterminateProperty);

        /// <summary>Reads whether the checkbox is currently disabled.</summary>
        public static TypedComponentSource<bool> Disabled<TModel>(
            this ComponentRef<FusionCheckBox, TModel> self)
            where TModel : class
            => self.Read(DisabledProperty);
    }
}
