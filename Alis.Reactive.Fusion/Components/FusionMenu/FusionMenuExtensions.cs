using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Opens and closes the rendered <see cref="FusionMenu"/> from a Reactive Plan pipeline.
    /// </summary>
    public static class FusionMenuExtensions
    {
        private static readonly ComponentMethod OpenMethod =
            ComponentMethod.Named("open");

        private static readonly ComponentMethod CloseMethod =
            ComponentMethod.Named("close");

        /// <summary>
        /// Opens the menu in hamburger mode.
        /// </summary>
        public static ComponentRef<FusionMenu, TModel> Open<TModel>(
            this ComponentRef<FusionMenu, TModel> self)
            where TModel : class
            => self.EmitCall(OpenMethod);

        /// <summary>
        /// Closes the menu in hamburger mode.
        /// </summary>
        public static ComponentRef<FusionMenu, TModel> Close<TModel>(
            this ComponentRef<FusionMenu, TModel> self)
            where TModel : class
            => self.EmitCall(CloseMethod);
    }
}
