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
    /// The Syncfusion Tab, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionTab(plan, "care-tabs", b =&gt; ...)</c> and target it with
    /// <c>p.Component&lt;FusionTab&gt;("care-tabs")</c>; every declared member works with <c>Set</c>,
    /// <c>Call</c> and <c>Get</c>, for example <c>.Call(c =&gt; c.Select, 2)</c> or <c>.Set(c =&gt; c.SelectedItem, 0)</c>.
    /// </remarks>
    public sealed class FusionTab : FusionComponent
    {
        /// <summary>The zero-based index of the selected tab.</summary>
        public ComponentProperty<int> SelectedItem => Property<int>("selectedItem");

        /// <summary>Selects the tab at a zero-based index.</summary>
        public ComponentMethod<int> Select => Method<int>("select");

        /// <summary>Hides (<see langword="true"/>) or shows (<see langword="false"/>) the tab at a zero-based index.</summary>
        public ComponentMethod<int, bool> HideTab => Method<int, bool>("hideTab");

        /// <summary>Raised after a tab is selected.</summary>
        public TypedEvent<FusionTabSelectedArgs> Selected =>
            Event<FusionTabSelectedArgs>("selected");
    }

    /// <summary>Payload of <see cref="FusionTab.Selected"/>.</summary>
    public class FusionTabSelectedArgs
    {
        /// <summary>Index of the selected tab.</summary>
        public int SelectedIndex { get; set; }

        /// <summary>Index of the previously selected tab.</summary>
        public int PreviousIndex { get; set; }

        /// <summary>Whether a swipe selected the tab.</summary>
        public bool IsSwiped { get; set; }
    }

    /// <summary>
    /// Renders the component for a plan, ready for <c>.Reactive(...)</c>.
    /// </summary>
    public static class FusionTabHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion Tab and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <param name="plan">Reactive Plan receiving tab event wiring.</param>
        /// <param name="elementId">Controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the component before rendering.</param>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionTab, TModel> FusionTab<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            string elementId,
            Action<TabBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().Tab(elementId);
            build(builder);

            return plan.Render<global::Alis.Reactive.Fusion.Components.FusionTab>(elementId, builder.Render());
        }
    }
}
