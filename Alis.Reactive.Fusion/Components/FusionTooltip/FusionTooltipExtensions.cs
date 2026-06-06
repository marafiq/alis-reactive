using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Opens, closes, and refreshes the rendered <see cref="FusionTooltip"/> from a Reactive Plan pipeline.
    /// </summary>
    /// <remarks>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via the pipeline:
    /// <c>p.Component&lt;FusionTooltip&gt;("my-tooltip").Open()</c>.
    /// Non-input component: no <c>Value()</c> read or <c>SetValue()</c>.
    /// </remarks>
    public static class FusionTooltipExtensions
    {
        private static readonly ComponentMethod OpenMethod =
            ComponentMethod.Named("open");

        private static readonly ComponentMethod CloseMethod =
            ComponentMethod.Named("close");

        private static readonly ComponentMethod RefreshMethod =
            ComponentMethod.Named("refresh");

        /// <summary>
        /// Opens the tooltip on its target element.
        /// </summary>
        public static ComponentRef<FusionTooltip, TModel> Open<TModel>(
            this ComponentRef<FusionTooltip, TModel> self)
            where TModel : class
            => self.EmitCall(OpenMethod);

        /// <summary>
        /// Closes the tooltip.
        /// </summary>
        public static ComponentRef<FusionTooltip, TModel> Close<TModel>(
            this ComponentRef<FusionTooltip, TModel> self)
            where TModel : class
            => self.EmitCall(CloseMethod);

        /// <summary>
        /// Refreshes the tooltip position and content.
        /// </summary>
        public static ComponentRef<FusionTooltip, TModel> Refresh<TModel>(
            this ComponentRef<FusionTooltip, TModel> self)
            where TModel : class
            => self.EmitCall(RefreshMethod);
    }
}
