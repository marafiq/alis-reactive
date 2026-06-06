using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Reactive pipeline extensions for <see cref="FusionToolbar"/>.
    /// </summary>
    public static class FusionToolbarExtensions
    {
        private static readonly FusionToolbar Component = new FusionToolbar();

        private static readonly ComponentMethod DisableMethod =
            ComponentMethod.Named("disable").WithArgs<bool>();

        /// <summary>
        /// Disables or enables the toolbar.
        /// </summary>
        public static ComponentRef<FusionToolbar, TModel> Disable<TModel>(
            this ComponentRef<FusionToolbar, TModel> self,
            bool value)
            where TModel : class
            => self.EmitCall(DisableMethod, new System.Collections.Generic.List<ValueExpression>
            {
                ValueExpression.Literal(value)
            });
    }
}
