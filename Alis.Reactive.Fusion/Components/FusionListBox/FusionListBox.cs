using System;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif
using Syncfusion.EJ2;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion ListBox, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionListBox(plan, "dining-companions", b =&gt; ...)</c> and target it with
    /// <c>var companions = p.Component&lt;FusionListBox&gt;("dining-companions")</c>, then write or call a member as
    /// its own statement, for example <c>companions.SelectItems(new[] { "carey" }, true, true)</c>.
    /// </remarks>
    public sealed class FusionListBox : FusionComponent
    {
        /// <summary>The selected values.</summary>
        public Property<string[]> Value => Property<string[]>();

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public void DataBind() => Method();

        /// <summary>Selects (<see langword="true"/>) or clears the items; the third argument says the first holds values, not texts.</summary>
        /// <param name="items">The items, by text, or by value when <paramref name="isValue"/> is <see langword="true"/>.</param>
        /// <param name="state"><see langword="true"/> to select the items; <see langword="false"/> to clear them.</param>
        /// <param name="isValue"><see langword="true"/> when <paramref name="items"/> holds values, not texts.</param>
        public void SelectItems(Arg<string[]> items, Arg<bool> state, Arg<bool> isValue) => Method(items, state, isValue);

        /// <summary>Selects (<see langword="true"/>) or clears every item.</summary>
        /// <param name="state"><see langword="true"/> to select every item; <see langword="false"/> to clear them all.</param>
        public void SelectAll(Arg<bool> state) => Method(state);

        /// <summary>Enables (<see langword="true"/>) or disables the items; the third argument says the first holds values, not texts.</summary>
        /// <param name="items">The items, by text, or by value when <paramref name="isValue"/> is <see langword="true"/>.</param>
        /// <param name="enable"><see langword="true"/> to enable the items; <see langword="false"/> to disable them.</param>
        /// <param name="isValue"><see langword="true"/> when <paramref name="items"/> holds values, not texts.</param>
        public void EnableItems(Arg<string[]> items, Arg<bool> enable, Arg<bool> isValue) => Method(items, enable, isValue);

        /// <summary>Raised when the selection changes.</summary>
        public Event<FusionListBoxChangeArgs> Change =>
            Event<FusionListBoxChangeArgs>();
    }

    /// <summary>Payload of <see cref="FusionListBox.Change"/>.</summary>
    public class FusionListBoxChangeArgs : JsObject
    {
        /// <summary>Selected values after the change.</summary>
        public ReadOnly<string[]> Value => ReadOnly<string[]>();
    }

    /// <summary>
    /// Creates a FusionListBox with Syncfusion MVC builder-owned static configuration.
    /// </summary>
    public static class FusionListBoxHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion ListBox and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <typeparam name="TModel">The view model the plan is authored for.</typeparam>
        /// <param name="html">The view's HTML helper.</param>
        /// <param name="plan">Reactive Plan receiving list box event wiring.</param>
        /// <param name="elementId">Controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the component before rendering.</param>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionListBox, TModel> FusionListBox<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            string elementId,
            Action<ListBoxBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().ListBox(elementId);
            build(builder);

            return plan.Render<global::Alis.Reactive.Fusion.Components.FusionListBox>(elementId, builder.Render());
        }
    }
}
