using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Mutation extensions for <see cref="FusionDialog"/> in a reactive pipeline.
    /// </summary>
    /// <remarks>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via the pipeline:
    /// <c>p.Component&lt;FusionDialog&gt;("edit-dialog").Show()</c>.
    /// Non-input component: no <c>Value()</c> read or <c>SetValue()</c>.
    /// </remarks>
    public static class FusionDialogExtensions
    {
        /// <summary>
        /// Shows the dialog.
        /// Runtime: ej2.show()
        /// </summary>
        public static ComponentRef<FusionDialog, TModel> Show<TModel>(
            this ComponentRef<FusionDialog, TModel> self)
            where TModel : class
            => self.EmitCall("show", new System.Collections.Generic.List<ValueProducer>());

        /// <summary>
        /// Hides the dialog.
        /// Runtime: ej2.hide()
        /// </summary>
        public static ComponentRef<FusionDialog, TModel> Hide<TModel>(
            this ComponentRef<FusionDialog, TModel> self)
            where TModel : class
            => self.EmitCall("hide", new System.Collections.Generic.List<ValueProducer>());

        /// <summary>
        /// Refreshes the dialog position and dimensions.
        /// Runtime: ej2.refreshPosition()
        /// </summary>
        public static ComponentRef<FusionDialog, TModel> RefreshPosition<TModel>(
            this ComponentRef<FusionDialog, TModel> self)
            where TModel : class
            => self.EmitCall("refreshPosition", new System.Collections.Generic.List<ValueProducer>());
    }
}
