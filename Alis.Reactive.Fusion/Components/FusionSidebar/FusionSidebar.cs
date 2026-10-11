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
    /// <summary>
    /// The Syncfusion Sidebar, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionSidebar(plan, "resident-sidebar", b =&gt; ...)</c> and target it with
    /// <c>p.Component&lt;FusionSidebar&gt;("resident-sidebar")</c>; every declared member works with <c>Set</c>,
    /// <c>Call</c> and <c>Get</c>, for example <c>.Call(c =&gt; c.Toggle)</c> or <c>p.When(sidebar.Get(c =&gt; c.IsOpen)).Truthy()</c>.
    /// </remarks>
    public sealed class FusionSidebar : FusionComponent
    {
        /// <summary>Whether the sidebar is open.</summary>
        public ComponentProperty<bool> IsOpen => Property<bool>("isOpen");

        /// <summary>Opens the sidebar.</summary>
        public ComponentMethod Show => Method("show");

        /// <summary>Closes the sidebar.</summary>
        public ComponentMethod Hide => Method("hide");

        /// <summary>Opens the sidebar when closed and closes it when open.</summary>
        public ComponentMethod Toggle => Method("toggle");

        /// <summary>Raised when the sidebar opens.</summary>
        public TypedEvent<FusionSidebarTransitionArgs> Open =>
            Event<FusionSidebarTransitionArgs>("open");

        /// <summary>Raised when the sidebar closes.</summary>
        public TypedEvent<FusionSidebarTransitionArgs> Close =>
            Event<FusionSidebarTransitionArgs>("close");
    }

    /// <summary>Payload of <see cref="FusionSidebar.Open"/> and <see cref="FusionSidebar.Close"/>.</summary>
    public class FusionSidebarTransitionArgs
    {
        /// <summary>Whether user interaction caused the transition.</summary>
        public bool IsInteracted { get; set; }
    }

    /// <summary>
    /// Renders the component for a plan, ready for <c>.Reactive(...)</c>.
    /// </summary>
    public static class FusionSidebarHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion Sidebar and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <param name="plan">Reactive Plan receiving sidebar event wiring.</param>
        /// <param name="elementId">Controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the component before rendering.</param>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionSidebar, TModel> FusionSidebar<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            string elementId,
            Action<SidebarBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().Sidebar(elementId);
            build(builder);

            return plan.Render<global::Alis.Reactive.Fusion.Components.FusionSidebar>(elementId, builder.Render());
        }
    }
}
