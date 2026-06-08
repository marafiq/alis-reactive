using Alis.Reactive.Builders.Conditions;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Reads and changes rendered <see cref="FusionCarousel"/> slide state from a Reactive Plan pipeline.
    /// </summary>
    public static class FusionCarouselExtensions
    {
        private static readonly FusionCarousel Component = new FusionCarousel();

        private static readonly ComponentProperty<int> SelectedIndexProperty =
            ComponentProperty<int>.Named("selectedIndex");

        private static readonly ComponentMethod NextMethod =
            ComponentMethod.Named("next");

        private static readonly ComponentMethod PreviousMethod =
            ComponentMethod.Named("prev");

        /// <summary>
        /// Reads the current slide index.
        /// </summary>
        public static TypedComponentSource<int> SelectedIndex<TModel>(
            this ComponentRef<FusionCarousel, TModel> self)
            where TModel : class
            => self.Read(SelectedIndexProperty);

        /// <summary>
        /// Advances to the next slide.
        /// </summary>
        public static ComponentRef<FusionCarousel, TModel> Next<TModel>(
            this ComponentRef<FusionCarousel, TModel> self)
            where TModel : class
            => self.EmitCall(NextMethod);

        /// <summary>
        /// Moves to the previous slide.
        /// </summary>
        public static ComponentRef<FusionCarousel, TModel> Previous<TModel>(
            this ComponentRef<FusionCarousel, TModel> self)
            where TModel : class
            => self.EmitCall(PreviousMethod);
    }
}
