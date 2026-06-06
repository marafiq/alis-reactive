using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload delivered before the BulletChart tooltip is rendered.
    /// </summary>
    public sealed class FusionBulletChartTooltipRenderArgs
    {
        /// <summary>The actual value of the feature bar.</summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>The target values of the comparative bar.</summary>
        public string[] Target { get; set; } = System.Array.Empty<string>();

        /// <summary>Syncfusion event token exposed as <c>args.name</c>.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>The tooltip template markup, when template rendering is enabled.</summary>
        public string? Template { get; set; }

        /// <summary>The tooltip text, when template rendering is not enabled.</summary>
        public string? Text { get; set; }
    }

    /// <summary>
    /// Typed event-payload operations for the tooltip render event args of <see cref="FusionBulletChart"/>.
    /// </summary>
    public static class FusionBulletChartTooltipRenderArgsExtensions
    {
        /// <summary>Replaces the tooltip text before Syncfusion renders it.</summary>
        public static void SetText(
            this FusionBulletChartTooltipRenderArgs args,
            IReactionEmitter pipeline,
            string text)
        {
            pipeline.AddStep(ReactionGraph.Set(
                PayloadSource.Event(),
                "text",
                ValueExpression.Literal(text)));
        }
    }
}
