using System;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif
using Syncfusion.EJ2;
using Syncfusion.EJ2.Navigations;

namespace Alis.Reactive.Fusion.Components
{
#nullable disable annotations
    /// <summary>
    /// The Syncfusion Carousel, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionCarousel(plan, "activity-carousel", b =&gt; ...)</c> and target it with
    /// <c>p.Component&lt;FusionCarousel&gt;("activity-carousel")</c>; every declared member works with <c>Set</c>,
    /// <c>Call</c> and <c>Get</c>, for example <c>.Call(c =&gt; c.Next)</c> or <c>.Get(c =&gt; c.SelectedIndex)</c>.
    /// </remarks>
    public sealed class FusionCarousel : FusionComponent
    {
        /// <summary>The zero-based index of the shown slide.</summary>
        public Prop<int> SelectedIndex => Prop<int>();

        /// <summary>Shows the next slide.</summary>
        public void Next() => Call();

        /// <summary>Shows the previous slide.</summary>
        public void Prev() => Call();

        /// <summary>Raised before the slide changes.</summary>
        public TypedEvent<FusionCarouselSlideChangingArgs> SlideChanging =>
            Event<FusionCarouselSlideChangingArgs>("slideChanging");

        /// <summary>Raised after the slide changes.</summary>
        public TypedEvent<FusionCarouselSlideChangedArgs> SlideChanged =>
            Event<FusionCarouselSlideChangedArgs>("slideChanged");
    }

    /// <summary>Payload of <see cref="FusionCarousel.SlideChanging"/>.</summary>
    public class FusionCarouselSlideChangingArgs : JsObject
    {
        /// <summary>Index of the slide shown now.</summary>
        public ReadOnly<int> CurrentIndex => ReadOnly<int>();

        /// <summary>Index of the slide about to show.</summary>
        public ReadOnly<int> NextIndex => ReadOnly<int>();

        /// <summary>Whether a swipe started the change.</summary>
        public ReadOnly<bool> IsSwiped => ReadOnly<bool>();

        /// <summary><c>"Next"</c> or <c>"Previous"</c>.</summary>
        public ReadOnly<string> SlideDirection => ReadOnly<string>();

        /// <summary>Set to <see langword="true"/> with <c>p.Payload(args).Set(a =&gt; a.Cancel, true)</c> to stop the action.</summary>
        public Prop<bool> Cancel => Prop<bool>();
    }

    /// <summary>Payload of <see cref="FusionCarousel.SlideChanged"/>.</summary>
    public class FusionCarouselSlideChangedArgs : JsObject
    {
        /// <summary>Index of the slide shown now.</summary>
        public ReadOnly<int> CurrentIndex => ReadOnly<int>();

        /// <summary>Index of the slide shown before.</summary>
        public ReadOnly<int> PreviousIndex => ReadOnly<int>();

        /// <summary>Whether a swipe made the change.</summary>
        public ReadOnly<bool> IsSwiped => ReadOnly<bool>();

        /// <summary><c>"Next"</c> or <c>"Previous"</c>.</summary>
        public ReadOnly<string> SlideDirection => ReadOnly<string>();
    }
#nullable restore annotations

    /// <summary>
    /// Renders the component for a plan, ready for <c>.Reactive(...)</c>.
    /// </summary>
    public static class FusionCarouselHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion Carousel and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <param name="plan">Reactive Plan receiving carousel event wiring.</param>
        /// <param name="elementId">Controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the component before rendering.</param>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionCarousel, TModel> FusionCarousel<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            string elementId,
            Action<CarouselBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().Carousel(elementId);
            build(builder);

            return plan.Render<global::Alis.Reactive.Fusion.Components.FusionCarousel>(elementId, builder.Render());
        }
    }
}
