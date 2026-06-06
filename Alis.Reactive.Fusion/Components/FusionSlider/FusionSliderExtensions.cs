using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed mutations and value reading for <see cref="FusionSlider"/> in a reactive pipeline.
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

        /// <summary>Sets the scalar slider value and flushes it into the visible slider.</summary>
        /// <param name="value">The numeric value to set.</param>
        public static ComponentRef<FusionSlider, TModel> SetValue<TModel>(
            this ComponentRef<FusionSlider, TModel> self,
            double value)
            where TModel : class
            => self
                .EmitSet(ValueProperty, ValueExpression.Literal(value))
                .EmitCall(DataBindMethod);

        /// <summary>Sets the two-value slider range and flushes it into the visible slider.</summary>
        /// <param name="start">The first value written into Syncfusion's range value.</param>
        /// <param name="end">The second value written into Syncfusion's range value.</param>
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

        /// <summary>Reads the current scalar value for use in conditions or gather.</summary>
        /// <returns>A typed source representing the slider's current scalar value.</returns>
        public static TypedComponentSource<double> Value<TModel>(
            this ComponentRef<FusionSlider, TModel> self)
            where TModel : class
            => self.Read(ValueProperty);

        /// <summary>Reads the current range value for use in gather or display pipelines.</summary>
        /// <returns>A typed source representing the slider's current range value.</returns>
        public static TypedComponentSource<double[]> RangeValue<TModel>(
            this ComponentRef<FusionSlider, TModel> self)
            where TModel : class
            => self.Read(RangeValueProperty);
    }
}
