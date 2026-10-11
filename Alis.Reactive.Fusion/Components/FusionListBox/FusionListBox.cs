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
    /// <c>p.Component&lt;FusionListBox&gt;("dining-companions")</c>; every declared member works with <c>Set</c>,
    /// <c>Call</c> and <c>Get</c>, for example <c>.Call(c =&gt; c.SelectItems, new[] { "carey" }, true, true)</c>.
    /// </remarks>
    public sealed class FusionListBox : FusionComponent
    {
        /// <summary>The selected values.</summary>
        public ComponentProperty<string[]> Value => Property<string[]>("value");

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public ComponentMethod DataBind => Method("dataBind");

        /// <summary>Selects (<see langword="true"/>) or clears the items; the third argument says the first holds values, not texts.</summary>
        public ComponentMethod<string[], bool, bool> SelectItems => Method<string[], bool, bool>("selectItems");

        /// <summary>Selects (<see langword="true"/>) or clears every item.</summary>
        public ComponentMethod<bool> SelectAll => Method<bool>("selectAll");

        /// <summary>Enables (<see langword="true"/>) or disables the items; the third argument says the first holds values, not texts.</summary>
        public ComponentMethod<string[], bool, bool> EnableItems => Method<string[], bool, bool>("enableItems");

        /// <summary>Raised when the selection changes.</summary>
        public TypedEvent<FusionListBoxChangeArgs> Change =>
            Event<FusionListBoxChangeArgs>("change");
    }

    /// <summary>Payload of <see cref="FusionListBox.Change"/>.</summary>
    public class FusionListBoxChangeArgs
    {
        /// <summary>Selected values after the change.</summary>
        public string[]? Value { get; set; }
    }

    /// <summary>
    /// Creates a FusionListBox with Syncfusion MVC builder-owned static configuration.
    /// </summary>
    public static class FusionListBoxHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion ListBox and carries its controlled component ID into the Reactive Plan.
        /// </summary>
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
