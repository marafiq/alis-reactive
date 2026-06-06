using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed component operations and value reads for <see cref="FusionRating"/> in a Reactive Plan pipeline.
    /// </summary>
    public static class FusionRatingExtensions
    {
        private static readonly FusionRating Component = new FusionRating();

        private static readonly ComponentProperty<double> ValueProperty =
            ComponentProperty<double>.Named(Component.ValueMember);

        private static readonly ComponentMethod DataBindMethod =
            ComponentMethod.Named("dataBind");

        private static readonly ComponentMethod ResetMethod =
            ComponentMethod.Named("reset");

        /// <summary>Sets the visible rating value.</summary>
        /// <param name="value">The rating value to set.</param>
        public static ComponentRef<FusionRating, TModel> SetValue<TModel>(
            this ComponentRef<FusionRating, TModel> self,
            double value)
            where TModel : class
            => self
                .EmitSet(ValueProperty, ValueExpression.Literal(value))
                .EmitCall(DataBindMethod);

        /// <summary>Resets the rating value to its minimum.</summary>
        public static ComponentRef<FusionRating, TModel> Reset<TModel>(
            this ComponentRef<FusionRating, TModel> self)
            where TModel : class
            => self.EmitCall(ResetMethod);

        /// <summary>Reads the current rating value for use in conditions or gather.</summary>
        /// <returns>A typed source representing the rating's current value.</returns>
        public static TypedComponentSource<double> Value<TModel>(
            this ComponentRef<FusionRating, TModel> self)
            where TModel : class
            => self.Read(ValueProperty);
    }
}
