using Alis.Reactive.Descriptors.Mutations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Mutation extensions for FusionTab — Select, HideTab, SetSelectedItem.
    /// Non-input component: no Value() read, no SetValue.
    /// </summary>
    public static class FusionTabExtensions
    {
        /// <summary>
        /// Selects a tab by index: ej2.select(index).
        /// </summary>
        public static ComponentRef<FusionTab, TModel> Select<TModel>(
            this ComponentRef<FusionTab, TModel> self, int index) where TModel : class
            => self.Emit(new CallMutation("select", args: new MethodArg[]
            {
                new LiteralArg(index)
            }));

        /// <summary>
        /// Shows or hides a tab by index: ej2.hideTab(index, isHidden).
        /// </summary>
        public static ComponentRef<FusionTab, TModel> HideTab<TModel>(
            this ComponentRef<FusionTab, TModel> self, int index, bool isHidden = true)
            where TModel : class
            => self.Emit(new CallMutation("hideTab", args: new MethodArg[]
            {
                new LiteralArg(index),
                new LiteralArg(isHidden)
            }));

        /// <summary>
        /// Sets the selected tab index via property: ej2.selectedItem = index.
        /// </summary>
        public static ComponentRef<FusionTab, TModel> SetSelectedItem<TModel>(
            this ComponentRef<FusionTab, TModel> self, int index) where TModel : class
            => self.Emit(new SetPropMutation("selectedItem", coerce: "number"),
                value: index.ToString());
    }
}
