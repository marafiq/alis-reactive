using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Component actions and value sources available inside Reactive Plan pipelines for <see cref="FusionSidebar"/>.
    /// </summary>
    public static class FusionSidebarExtensions
    {
        private static readonly FusionSidebar Component = new FusionSidebar();

        private static readonly ComponentProperty<bool> IsOpenProperty =
            ComponentProperty<bool>.Named("isOpen");

        private static readonly ComponentMethod ShowMethod =
            ComponentMethod.Named("show");

        private static readonly ComponentMethod HideMethod =
            ComponentMethod.Named("hide");

        private static readonly ComponentMethod ToggleMethod =
            ComponentMethod.Named("toggle");

        /// <summary>
        /// Reads whether the sidebar is open.
        /// </summary>
        public static TypedComponentSource<bool> IsOpen<TModel>(
            this ComponentRef<FusionSidebar, TModel> self)
            where TModel : class
            => self.Read(IsOpenProperty);

        /// <summary>
        /// Opens the sidebar.
        /// </summary>
        public static ComponentRef<FusionSidebar, TModel> Show<TModel>(
            this ComponentRef<FusionSidebar, TModel> self)
            where TModel : class
            => self.EmitCall(ShowMethod);

        /// <summary>
        /// Closes the sidebar.
        /// </summary>
        public static ComponentRef<FusionSidebar, TModel> Hide<TModel>(
            this ComponentRef<FusionSidebar, TModel> self)
            where TModel : class
            => self.EmitCall(HideMethod);

        /// <summary>
        /// Toggles the sidebar open state.
        /// </summary>
        public static ComponentRef<FusionSidebar, TModel> Toggle<TModel>(
            this ComponentRef<FusionSidebar, TModel> self)
            where TModel : class
            => self.EmitCall(ToggleMethod);
    }
}
