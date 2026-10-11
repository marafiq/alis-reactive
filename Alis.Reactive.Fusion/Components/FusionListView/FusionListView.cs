using System;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif
using Syncfusion.EJ2;
using Syncfusion.EJ2.Lists;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion ListView, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionListView(plan, "resident-list", b =&gt; ...)</c> and target it with
    /// <c>p.Component&lt;FusionListView&gt;("resident-list")</c>; every declared member works with <c>Set</c>,
    /// <c>Call</c> and <c>Get</c>, for example <c>.Call(c =&gt; c.SelectItem, "Ada")</c>.
    /// </remarks>
    public sealed class FusionListView : FusionComponent
    {
        /// <summary>Selects the item with this text.</summary>
        public ComponentMethod<string> SelectItem => Method<string>("selectItem");

        /// <summary>Clears the selection of the item with this text.</summary>
        public ComponentMethod<string> UnselectItem => Method<string>("unselectItem");

        /// <summary>Checks every item of a checkbox list.</summary>
        public ComponentMethod CheckAllItems => Method("checkAllItems");

        /// <summary>Unchecks every item of a checkbox list.</summary>
        public ComponentMethod UncheckAllItems => Method("uncheckAllItems");

        /// <summary>Raised when an item is selected.</summary>
        public TypedEvent<FusionListViewSelectArgs> Select =>
            Event<FusionListViewSelectArgs>("select");
    }

    /// <summary>Payload of <see cref="FusionListView.Select"/>.</summary>
    public class FusionListViewSelectArgs
    {
        /// <summary>Text of the selected item.</summary>
        public string? Text { get; set; }

        /// <summary>Zero-based index of the selected item when the user selected it.</summary>
        public int? Index { get; set; }

        /// <summary>Whether user interaction made the selection.</summary>
        public bool IsInteracted { get; set; }

        /// <summary>Checkbox state in a checkbox list.</summary>
        public bool? IsChecked { get; set; }

        /// <summary>Set to <see langword="true"/> with <c>p.Payload(args).Set(a =&gt; a.Cancel, true)</c> to stop the action.</summary>
        public bool Cancel { get; set; }
    }

    /// <summary>
    /// Creates a FusionListView with Syncfusion MVC builder-owned static configuration.
    /// </summary>
    public static class FusionListViewHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion ListView and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <param name="plan">Reactive Plan receiving list view event wiring.</param>
        /// <param name="elementId">Controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the component before rendering.</param>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionListView, TModel> FusionListView<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            string elementId,
            Action<ListViewBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().ListView(elementId);
            build(builder);

            return plan.Render<global::Alis.Reactive.Fusion.Components.FusionListView>(elementId, builder.Render());
        }
    }
}
