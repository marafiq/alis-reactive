using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Mutation extensions for <see cref="FusionTooltip"/> in a reactive pipeline.
    /// </summary>
    /// <remarks>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via the pipeline:
    /// <c>p.Component&lt;FusionTooltip&gt;("my-tooltip").Open()</c>.
    /// Non-input component: no <c>Value()</c> read or <c>SetValue()</c>.
    /// </remarks>
    public static class FusionTooltipExtensions
    {
        /// <summary>
        /// Opens the tooltip programmatically on the target element.
        /// Runtime: ej2.open(targetElement)
        /// </summary>
        public static ComponentRef<FusionTooltip, TModel> Open<TModel>(
            this ComponentRef<FusionTooltip, TModel> self)
            where TModel : class
            => self.EmitCall("open", new System.Collections.Generic.List<ValueProducer>());

        /// <summary>
        /// Closes the tooltip.
        /// Runtime: ej2.close()
        /// </summary>
        public static ComponentRef<FusionTooltip, TModel> Close<TModel>(
            this ComponentRef<FusionTooltip, TModel> self)
            where TModel : class
            => self.EmitCall("close", new System.Collections.Generic.List<ValueProducer>());

        /// <summary>
        /// Refreshes the tooltip position and content.
        /// Runtime: ej2.refresh()
        /// </summary>
        public static ComponentRef<FusionTooltip, TModel> Refresh<TModel>(
            this ComponentRef<FusionTooltip, TModel> self)
            where TModel : class
            => self.EmitCall("refresh", new System.Collections.Generic.List<ValueProducer>());
    }
}
