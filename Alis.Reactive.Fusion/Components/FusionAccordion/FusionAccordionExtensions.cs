using Alis.Reactive.Descriptors.Mutations;
using Alis.Reactive.Descriptors.Values;

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
            => self.Emit(new CallMutation("expandItem", args: new CommandValue[]
            {
                CommandValue.FromLiteral(isExpand),
                CommandValue.FromLiteral(index)
            }));

        /// <summary>
        /// Enables or disables a panel by index.
        /// Runtime: ej2.enableItem(index, isEnable)
        /// </summary>
        public static ComponentRef<FusionAccordion, TModel> EnableItem<TModel>(
            this ComponentRef<FusionAccordion, TModel> self, int index, bool isEnable = true)
            where TModel : class
            => self.Emit(new CallMutation("enableItem", args: new CommandValue[]
            {
                CommandValue.FromLiteral(index),
                CommandValue.FromLiteral(isEnable)
            }));
    }
}
