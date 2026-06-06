using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Component operation extensions for <see cref="FusionDialog"/> in a Reactive Plan pipeline.
    /// </summary>
    /// <remarks>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via the pipeline:
    /// <c>p.Component&lt;FusionDialog&gt;("edit-dialog").Show()</c>.
    /// Non-input component: no <c>Value()</c> read or <c>SetValue()</c>.
    /// </remarks>
    public static class FusionDialogExtensions
    {
        private static readonly ComponentMethod ShowMethod =
            ComponentMethod.Named("show");

        private static readonly ComponentMethod HideMethod =
            ComponentMethod.Named("hide");

        private static readonly ComponentMethod RefreshPositionMethod =
            ComponentMethod.Named("refreshPosition");

        /// <summary>
        /// Shows the dialog.
        /// Runtime: ej2.show()
        /// </summary>
        public static ComponentRef<FusionDialog, TModel> Show<TModel>(
            this ComponentRef<FusionDialog, TModel> self)
            where TModel : class
            => self.EmitCall(ShowMethod);

        /// <summary>
        /// Hides the dialog.
        /// Runtime: ej2.hide()
        /// </summary>
        public static ComponentRef<FusionDialog, TModel> Hide<TModel>(
            this ComponentRef<FusionDialog, TModel> self)
            where TModel : class
            => self.EmitCall(HideMethod);

        /// <summary>
        /// Refreshes the dialog position and dimensions.
        /// Runtime: ej2.refreshPosition()
        /// </summary>
        public static ComponentRef<FusionDialog, TModel> RefreshPosition<TModel>(
            this ComponentRef<FusionDialog, TModel> self)
            where TModel : class
            => self.EmitCall(RefreshPositionMethod);
    }
}
