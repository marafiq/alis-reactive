using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Updates rendered <see cref="FusionAccordion"/> panel state from a Reactive Plan pipeline.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via the pipeline:
    /// <c>p.Component&lt;FusionAccordion&gt;("my-accordion").ExpandItem(true, 0)</c>.
    /// </para>
    /// <para>
    /// Non-input component: no <c>Value()</c> read or <c>SetValue()</c>.
    /// </para>
    /// </remarks>
    public static class FusionAccordionExtensions
    {
        private static readonly ComponentMethod ExpandItemMethod =
            ComponentMethod.Named("expandItem").WithArgs<bool, int>();

        private static readonly ComponentMethod EnableItemMethod =
            ComponentMethod.Named("enableItem").WithArgs<int, bool>();

        /// <summary>
        /// Expands or collapses a panel by index.
        /// </summary>
        public static ComponentRef<FusionAccordion, TModel> ExpandItem<TModel>(
            this ComponentRef<FusionAccordion, TModel> self, bool isExpand, int index)
            where TModel : class
            => self.EmitCall(ExpandItemMethod, new System.Collections.Generic.List<ValueExpression> { ValueExpression.Literal(isExpand), ValueExpression.Literal(index) });

        /// <summary>
        /// Enables or disables a panel by index.
        /// </summary>
        public static ComponentRef<FusionAccordion, TModel> EnableItem<TModel>(
            this ComponentRef<FusionAccordion, TModel> self, int index, bool isEnable = true)
            where TModel : class
            => self.EmitCall(EnableItemMethod, new System.Collections.Generic.List<ValueExpression> { ValueExpression.Literal(index), ValueExpression.Literal(isEnable) });
    }
}
