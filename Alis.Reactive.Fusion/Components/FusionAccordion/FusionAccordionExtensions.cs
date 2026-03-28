using Alis.Reactive.Descriptors.Mutations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Mutation extensions for <see cref="FusionAccordion"/> in a reactive pipeline.
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
        /// <summary>
        /// Expands or collapses a panel by index.
        /// Runtime: ej2.expandItem(isExpand, index)
        /// </summary>
        public static ComponentRef<FusionAccordion, TModel> ExpandItem<TModel>(
            this ComponentRef<FusionAccordion, TModel> self, bool isExpand, int index)
            where TModel : class
            => self.Emit(new CallMutation("expandItem", args: new MethodArg[]
            {
                new LiteralArg(isExpand),
                new LiteralArg(index)
            }));

        /// <summary>
        /// Enables or disables a panel by index.
        /// Runtime: ej2.enableItem(index, isEnable)
        /// </summary>
        public static ComponentRef<FusionAccordion, TModel> EnableItem<TModel>(
            this ComponentRef<FusionAccordion, TModel> self, int index, bool isEnable = true)
            where TModel : class
            => self.Emit(new CallMutation("enableItem", args: new MethodArg[]
            {
                new LiteralArg(index),
                new LiteralArg(isEnable)
            }));
    }
}
