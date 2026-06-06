using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Reactive pipeline extensions for <see cref="FusionBulletChart"/>.
    /// </summary>
    public static class FusionBulletChartExtensions
    {
        private static readonly ComponentMethod GetActualIndexMethod =
            ComponentMethod.Named("getActualIndex").WithArgs<int, int>();

        /// <summary>
        /// Calculates the wrapped index used by the chart.
        /// </summary>
        public static TypedComponentSource<int> GetActualIndex<TModel>(
            this ComponentRef<FusionBulletChart, TModel> self,
            int index,
            int totalLength)
            where TModel : class
            => self.Read<int>(
                GetActualIndexMethod,
                new System.Collections.Generic.List<ValueExpression>
                {
                    ValueExpression.Literal(index),
                    ValueExpression.Literal(totalLength)
                });
    }
}
