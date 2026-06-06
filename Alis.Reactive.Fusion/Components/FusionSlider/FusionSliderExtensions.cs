using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed component operations and value reads for <see cref="FusionSlider"/> in a Reactive Plan pipeline.
    /// </summary>
    public static class FusionSliderExtensions
    {
        private static readonly FusionSlider Component = new FusionSlider();

        private static readonly ComponentProperty<double> ValueProperty =
            ComponentProperty<double>.Named(Component.ValueMember);

        private static readonly ComponentProperty<double[]> RangeValueProperty =
            ComponentProperty<double[]>.Mapped("rangeValue", Component.ValueMember);

        private static readonly ComponentMethod DataBindMethod =
            ComponentMethod.Named("dataBind");

        /// <summary>Sets the visible scalar slider value.</summary>
        /// <param name="value">Numeric value to set.</param>
        public static ComponentRef<FusionSlider, TModel> SetValue<TModel>(
            this ComponentRef<FusionSlider, TModel> self,
            double value)
            where TModel : class
            => self
                .EmitSet(ValueProperty, ValueExpression.Literal(value))
                .EmitCall(DataBindMethod);

        /// <summary>Sets the visible two-value slider range.</summary>
        /// <param name="start">First range value.</param>
        /// <param name="end">Second range value.</param>
        public static ComponentRef<FusionSlider, TModel> SetRangeValue<TModel>(
            this ComponentRef<FusionSlider, TModel> self,
            double start,
            double end)
            where TModel : class
            => self
                .EmitSet(
                    RangeValueProperty,
                    ValueExpression.LiteralRaw(new[] { start, end }, Shape.ArrayOf(Shape.Number)))
                .EmitCall(DataBindMethod);

        /// <summary>Reads the scalar value for conditions or gather.</summary>
        public static TypedComponentSource<double> Value<TModel>(
            this ComponentRef<FusionSlider, TModel> self)
            where TModel : class
            => self.Read(ValueProperty);

        /// <summary>Reads the range value for gather or display pipelines.</summary>
        public static TypedComponentSource<double[]> RangeValue<TModel>(
            this ComponentRef<FusionSlider, TModel> self)
            where TModel : class
            => self.Read(RangeValueProperty);
    }
}
