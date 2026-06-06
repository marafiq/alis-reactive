namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed events exposed by the <see cref="FusionBulletChart"/> component.
    /// </summary>
    public sealed class FusionBulletChartEvents
    {
        public static readonly FusionBulletChartEvents Instance = new FusionBulletChartEvents();

        private FusionBulletChartEvents()
        {
        }

        /// <summary>Fires before the tooltip renders.</summary>
        public TypedEvent<FusionBulletChartTooltipRenderArgs> TooltipRender =>
            new TypedEvent<FusionBulletChartTooltipRenderArgs>("tooltipRender", new FusionBulletChartTooltipRenderArgs());

        /// <summary>Fires when the chart surface is clicked.</summary>
        public TypedEvent<FusionBulletChartMouseClickArgs> BulletChartMouseClick =>
            new TypedEvent<FusionBulletChartMouseClickArgs>("bulletChartMouseClick", new FusionBulletChartMouseClickArgs());

    }
}
