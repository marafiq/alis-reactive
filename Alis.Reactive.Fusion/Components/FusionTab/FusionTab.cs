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
    /// Render it with <c>Html.FusionTab(plan, "care-tabs", b =&gt; ...)</c>, reach it with
    /// <c>var careTabs = p.Component&lt;FusionTab&gt;("care-tabs");</c> and use its members directly:
    /// <c>careTabs.Select(2);</c> or <c>careTabs.SelectedItem.Set(0);</c>.
    /// </remarks>
    public sealed class FusionTab : FusionComponent
    {
        /// <summary>The zero-based index of the selected tab.</summary>
        public Prop<int> SelectedItem => Prop<int>();

        /// <summary>Selects the tab at a zero-based index.</summary>
        /// <param name="index">The zero-based index of the tab to select.</param>
        public void Select(Arg<int> index) => Call(index);

        /// <summary>Hides (<see langword="true"/>) or shows (<see langword="false"/>) the tab at a zero-based index.</summary>
        /// <param name="index">The zero-based index of the tab.</param>
        /// <param name="value"><see langword="true"/> to hide the tab; <see langword="false"/> to show it.</param>
        public void HideTab(Arg<int> index, Arg<bool> value) => Call(index, value);

        /// <summary>Raised after a tab is selected.</summary>
        public TypedEvent<FusionTabSelectedArgs> Selected =>
            Event<FusionTabSelectedArgs>();
    }

    /// <summary>Payload of <see cref="FusionTab.Selected"/>.</summary>
    public class FusionTabSelectedArgs : JsObject
    {
        /// <summary>Index of the selected tab.</summary>
        public ReadOnly<int> SelectedIndex => ReadOnly<int>();

        /// <summary>Index of the previously selected tab.</summary>
        public ReadOnly<int> PreviousIndex => ReadOnly<int>();

        /// <summary>Whether a swipe selected the tab.</summary>
        public ReadOnly<bool> IsSwiped => ReadOnly<bool>();
    }

    /// <summary>
    /// Renders the component for a plan, ready for <c>.Reactive(...)</c>.
    /// </summary>
    public static class FusionTabHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion Tab and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <param name="html">The view's HTML helper.</param>
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
