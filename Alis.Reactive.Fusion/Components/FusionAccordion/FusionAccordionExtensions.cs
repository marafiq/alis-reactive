using Alis.Reactive.Descriptors.Mutations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Mutation extensions for FusionAccordion.
    /// Methods: expandItem(isExpand, index), enableItem(index, isEnable).
    /// No Value() — non-input component has nothing to read.
    /// </summary>
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
