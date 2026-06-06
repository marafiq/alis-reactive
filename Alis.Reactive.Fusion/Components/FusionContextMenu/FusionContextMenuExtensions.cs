using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Reactive Plan pipeline extensions for <see cref="FusionContextMenu"/>.
    /// </summary>
    public static class FusionContextMenuExtensions
    {
        private static readonly ComponentMethod OpenMethod =
            ComponentMethod.Named("open").WithArgs<double, double>();

        private static readonly ComponentMethod CloseMethod =
            ComponentMethod.Named("close");

        /// <summary>
        /// Opens the context menu at the specified position.
        /// </summary>
        public static ComponentRef<FusionContextMenu, TModel> Open<TModel>(
            this ComponentRef<FusionContextMenu, TModel> self,
            double top,
            double left)
            where TModel : class
            => self.EmitCall(OpenMethod, new System.Collections.Generic.List<ValueExpression>
            {
                ValueExpression.Literal(top),
                ValueExpression.Literal(left)
            });

        /// <summary>
        /// Closes the context menu.
        /// </summary>
        public static ComponentRef<FusionContextMenu, TModel> Close<TModel>(
            this ComponentRef<FusionContextMenu, TModel> self)
            where TModel : class
            => self.EmitCall(CloseMethod);
    }
}
