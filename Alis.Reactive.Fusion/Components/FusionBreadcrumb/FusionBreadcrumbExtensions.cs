using Alis.Reactive.PlanModel;
using Alis.Reactive.Builders.Conditions;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Reads and updates <see cref="FusionBreadcrumb"/> active-item state from a Reactive Plan pipeline.
    /// </summary>
    public static class FusionBreadcrumbExtensions
    {
        private static readonly FusionBreadcrumb Component = new FusionBreadcrumb();

        private static readonly ComponentProperty<string> ActiveItemProperty =
            ComponentProperty<string>.Named("activeItem");

        private static readonly ComponentMethod DataBindMethod =
            ComponentMethod.Named("dataBind");

        /// <summary>
        /// Sets the active breadcrumb item URL or text.
        /// </summary>
        public static ComponentRef<FusionBreadcrumb, TModel> SetActiveItem<TModel>(
            this ComponentRef<FusionBreadcrumb, TModel> self,
            string activeItem)
            where TModel : class
            => self.EmitSet(ActiveItemProperty, ValueExpression.Literal(activeItem))
                .EmitCall(DataBindMethod);

        /// <summary>
        /// Reads the current active breadcrumb item URL or text.
        /// </summary>
        public static TypedComponentSource<string> ActiveItem<TModel>(
            this ComponentRef<FusionBreadcrumb, TModel> self)
            where TModel : class
            => self.Read(ActiveItemProperty);
    }
}
