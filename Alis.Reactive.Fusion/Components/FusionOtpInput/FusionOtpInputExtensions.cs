using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Reads and updates <see cref="FusionOtpInput"/> values from a Reactive Plan pipeline.
    /// </summary>
    public static class FusionOtpInputExtensions
    {
        private static readonly FusionOtpInput Component = new FusionOtpInput();

        private static readonly ComponentProperty<string> ValueProperty =
            ComponentProperty<string>.Named(Component.ValueMember);

        private static readonly ComponentMethod DataBindMethod =
            ComponentMethod.Named("dataBind");

        private static readonly ComponentMethod FocusInMethod =
            ComponentMethod.Named("focusIn");

        private static readonly ComponentMethod FocusOutMethod =
            ComponentMethod.Named("focusOut");

        /// <summary>Sets the visible OTP value.</summary>
        public static ComponentRef<FusionOtpInput, TModel> SetValue<TModel>(
            this ComponentRef<FusionOtpInput, TModel> self,
            string value)
            where TModel : class
            => self
                .EmitSet(ValueProperty, ValueExpression.Literal(value))
                .EmitCall(DataBindMethod);

        /// <summary>Moves focus into the OTP input.</summary>
        public static ComponentRef<FusionOtpInput, TModel> FocusIn<TModel>(
            this ComponentRef<FusionOtpInput, TModel> self)
            where TModel : class
            => self.EmitCall(FocusInMethod);

        /// <summary>Removes focus from the OTP input.</summary>
        public static ComponentRef<FusionOtpInput, TModel> FocusOut<TModel>(
            this ComponentRef<FusionOtpInput, TModel> self)
            where TModel : class
            => self.EmitCall(FocusOutMethod);

        /// <summary>Reads the OTP value for conditions or gather.</summary>
        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<FusionOtpInput, TModel> self)
            where TModel : class
            => self.Read(ValueProperty);
    }
}
