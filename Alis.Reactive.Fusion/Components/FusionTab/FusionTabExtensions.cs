using Alis.Reactive.PlanModel;

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
        private static readonly CapabilityMethod SelectMethod = CapabilityMethod.Named("select");
        private static readonly CapabilityMethod HideTabMethod = CapabilityMethod.Named("hideTab");
        private static readonly CapabilityProperty SelectedItemProperty = CapabilityProperty.Named("selectedItem");

        /// <summary>
        /// Selects a tab by index: ej2.select(index).
        /// </summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <param name="index">The zero-based tab index.</param>
        /// <returns>The component reference for continued chaining.</returns>
        public static ComponentRef<FusionTab, TModel> Select<TModel>(
            this ComponentRef<FusionTab, TModel> self, int index) where TModel : class
            => self.Call(SelectMethod, index);

        /// <summary>
        /// Shows or hides a tab by index: ej2.hideTab(index, isHidden).
        /// </summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <param name="index">The zero-based tab index.</param>
        /// <param name="isHidden"><see langword="true"/> to hide the tab; otherwise, <see langword="false"/>.</param>
        /// <returns>The component reference for continued chaining.</returns>
        public static ComponentRef<FusionTab, TModel> HideTab<TModel>(
            this ComponentRef<FusionTab, TModel> self, int index, bool isHidden = true)
            where TModel : class
            => self.Call(HideTabMethod, index, isHidden);

        /// <summary>
        /// Sets the selected tab index via property: ej2.selectedItem = index.
        /// </summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <param name="index">The zero-based tab index.</param>
        /// <returns>The component reference for continued chaining.</returns>
        public static ComponentRef<FusionTab, TModel> SetSelectedItem<TModel>(
            this ComponentRef<FusionTab, TModel> self, int index) where TModel : class
            => self.Set(SelectedItemProperty, index);
    }
}
