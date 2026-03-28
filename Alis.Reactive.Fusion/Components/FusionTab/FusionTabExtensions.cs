using Alis.Reactive.Descriptors.Mutations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Mutation extensions for <see cref="FusionTab"/> in a reactive pipeline.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via the pipeline:
    /// <c>p.Component&lt;FusionTab&gt;("my-tabs").Select(1)</c>.
    /// </para>
    /// <para>
    /// Non-input component: no <c>Value()</c> read or <c>SetValue()</c>.
    /// </para>
    /// </remarks>
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
