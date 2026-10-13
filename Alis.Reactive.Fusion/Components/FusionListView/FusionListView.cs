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
#nullable disable annotations
    /// <summary>
    /// The Syncfusion ListView, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionListView(plan, "resident-list", b =&gt; ...)</c> and target it with
    /// <c>var residents = p.Component&lt;FusionListView&gt;("resident-list")</c>, then call a member as its own
    /// statement, for example <c>residents.SelectItem("Ada")</c>.
    /// </remarks>
    public sealed class FusionListView : FusionComponent
    {
        /// <summary>Selects the item with this text.</summary>
        /// <param name="item">The text of the item to select.</param>
        public void SelectItem(Arg<string> item) => Call(item);

        /// <summary>Clears the selection of the item with this text.</summary>
        /// <param name="item">The text of the item to clear.</param>
        public void UnselectItem(Arg<string> item) => Call(item);

        /// <summary>Checks every item of a checkbox list.</summary>
        public void CheckAllItems() => Call();

        /// <summary>Unchecks every item of a checkbox list.</summary>
        public void UncheckAllItems() => Call();

        /// <summary>Raised when an item is selected.</summary>
        public TypedEvent<FusionListViewSelectArgs> Select =>
            Event<FusionListViewSelectArgs>();
    }

    /// <summary>Payload of <see cref="FusionListView.Select"/>.</summary>
    public class FusionListViewSelectArgs : JsObject
    {
        /// <summary>Text of the selected item.</summary>
        public ReadOnly<string> Text => ReadOnly<string>();

        /// <summary>Zero-based index of the selected item when the user selected it.</summary>
        public ReadOnly<int?> Index => ReadOnly<int?>();

        /// <summary>Whether user interaction made the selection.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();

        /// <summary>Checkbox state in a checkbox list.</summary>
        public ReadOnly<bool?> IsChecked => ReadOnly<bool?>();

        /// <summary>Set to <see langword="true"/> with <c>args.Cancel.Set(true)</c> to stop the action.</summary>
        public Prop<bool> Cancel => Prop<bool>();
    }
#nullable restore annotations

    /// <summary>
    /// Creates a FusionListView with Syncfusion MVC builder-owned static configuration.
    /// </summary>
    public static class FusionListViewHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion ListView and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <typeparam name="TModel">The view model the plan is authored for.</typeparam>
        /// <param name="html">The view's HTML helper.</param>
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
