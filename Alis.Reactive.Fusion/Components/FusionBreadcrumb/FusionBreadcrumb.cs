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
    /// The Syncfusion Breadcrumb, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionBreadcrumb(plan, "care-breadcrumb", b =&gt; ...)</c> and target it with
    /// <c>p.Component&lt;FusionBreadcrumb&gt;("care-breadcrumb")</c>; every declared member works with <c>Set</c>,
    /// <c>Call</c> and <c>Get</c>. Syncfusion applies property writes on its next tick; follow a write with
    /// <c>.Call(c =&gt; c.DataBind)</c>: <c>.Set(c =&gt; c.ActiveItem, "/plans").Call(c =&gt; c.DataBind)</c>.
    /// </remarks>
    public sealed class FusionBreadcrumb : FusionComponent
    {
        /// <summary>The URL of the active item.</summary>
        public Prop<string> ActiveItem => Prop<string>();

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public void DataBind() => Call();

        /// <summary>Raised when the user clicks an item.</summary>
        public TypedEvent<FusionBreadcrumbItemClickArgs> ItemClick =>
            Event<FusionBreadcrumbItemClickArgs>("itemClick");
    }

    /// <summary>Payload of <see cref="FusionBreadcrumb.ItemClick"/>.</summary>
    public class FusionBreadcrumbItemClickArgs : JsObject
    {
        /// <summary>The clicked item.</summary>
        public ReadOnly<FusionBreadcrumbItem> Item => ReadOnly<FusionBreadcrumbItem>();
    }
#nullable restore annotations

    /// <summary>A breadcrumb item as the <see cref="FusionBreadcrumb.ItemClick"/> payload reports it.</summary>
    public class FusionBreadcrumbItem
    {
        /// <summary>The item text.</summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>The item ID.</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>The item URL.</summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>The item icon classes.</summary>
        public string? IconCss { get; set; }

        /// <summary>Whether the item is disabled.</summary>
        public bool Disabled { get; set; }
    }

    /// <summary>
    /// Renders the component for a plan, ready for <c>.Reactive(...)</c>.
    /// </summary>
    public static class FusionBreadcrumbHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion Breadcrumb and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <param name="plan">Reactive Plan receiving breadcrumb event wiring.</param>
        /// <param name="elementId">Controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the component before rendering.</param>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionBreadcrumb, TModel> FusionBreadcrumb<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            string elementId,
            Action<BreadcrumbBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().Breadcrumb(elementId);
            build(builder);

            return plan.Render<global::Alis.Reactive.Fusion.Components.FusionBreadcrumb>(elementId, builder.Render());
        }
    }
}
