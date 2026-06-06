using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Component operation extensions for <see cref="FusionTab"/> in a Reactive Plan pipeline.
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
        private static readonly ComponentMethod SelectMethod =
            ComponentMethod.Named("select").WithArgs<int>();

        private static readonly ComponentMethod HideTabMethod =
            ComponentMethod.Named("hideTab").WithArgs<int, bool>();

        private static readonly ComponentProperty<int> SelectedItemProperty =
            ComponentProperty<int>.Named("selectedItem");

        /// <summary>
        /// Selects a tab by index: ej2.select(index).
        /// </summary>
        public static ComponentRef<FusionTab, TModel> Select<TModel>(
            this ComponentRef<FusionTab, TModel> self, int index) where TModel : class
            => self.EmitCall(SelectMethod, new System.Collections.Generic.List<ValueExpression> { ValueExpression.Literal(index) });

        /// <summary>
        /// Shows or hides a tab by index: ej2.hideTab(index, isHidden).
        /// </summary>
        public static ComponentRef<FusionTab, TModel> HideTab<TModel>(
            this ComponentRef<FusionTab, TModel> self, int index, bool isHidden = true)
            where TModel : class
            => self.EmitCall(HideTabMethod, new System.Collections.Generic.List<ValueExpression> { ValueExpression.Literal(index), ValueExpression.Literal(isHidden) });

        /// <summary>
        /// Sets the selected tab index via property: ej2.selectedItem = index.
        /// </summary>
        public static ComponentRef<FusionTab, TModel> SetSelectedItem<TModel>(
            this ComponentRef<FusionTab, TModel> self, int index) where TModel : class
            => self.EmitSet(SelectedItemProperty, ValueExpression.Literal(index));
    }
}
