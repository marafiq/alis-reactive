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
    /// The Syncfusion Sidebar, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionSidebar(plan, "resident-sidebar", b =&gt; ...)</c>, reach it with
    /// <c>var sidebar = p.Component&lt;FusionSidebar&gt;("resident-sidebar");</c> and use its members directly:
    /// <c>sidebar.Toggle();</c> or <c>p.When(sidebar.IsOpen).Truthy()</c>.
    /// </remarks>
    public sealed class FusionSidebar : FusionComponent
    {
        /// <summary>Whether the sidebar is open.</summary>
        public Prop<bool> IsOpen => Prop<bool>();

        /// <summary>Opens the sidebar.</summary>
        public void Show() => Call();

        /// <summary>Closes the sidebar.</summary>
        public void Hide() => Call();

        /// <summary>Opens the sidebar when closed and closes it when open.</summary>
        public void Toggle() => Call();

        /// <summary>Raised when the sidebar opens.</summary>
        public TypedEvent<FusionSidebarTransitionArgs> Open =>
            Event<FusionSidebarTransitionArgs>();

        /// <summary>Raised when the sidebar closes.</summary>
        public TypedEvent<FusionSidebarTransitionArgs> Close =>
            Event<FusionSidebarTransitionArgs>();
    }

    /// <summary>Payload of <see cref="FusionSidebar.Open"/> and <see cref="FusionSidebar.Close"/>.</summary>
    public class FusionSidebarTransitionArgs : JsObject
    {
        /// <summary>Whether user interaction caused the transition.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }
#nullable restore annotations

    /// <summary>
    /// Renders the component for a plan, ready for <c>.Reactive(...)</c>.
    /// </summary>
    public static class FusionSidebarHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion Sidebar and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <param name="html">The view's HTML helper.</param>
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
