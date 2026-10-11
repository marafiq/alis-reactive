using System;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif
using Syncfusion.EJ2;
using Syncfusion.EJ2.Buttons;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion RadioButton, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionRadioButton(plan, "companion-suite", b =&gt; ...)</c> and target it with
    /// <c>p.Component&lt;FusionRadioButton&gt;("companion-suite")</c>; every declared member works with <c>Set</c>,
    /// <c>Call</c> and <c>Get</c>, for example <c>.Set(c =&gt; c.Checked, true).Call(c =&gt; c.DataBind)</c> or
    /// <c>.Get(c =&gt; c.GetSelectedValue)</c>.
    /// </remarks>
    public sealed class FusionRadioButton : FusionComponent
    {
        /// <summary>Whether the radio button is checked.</summary>
        public ComponentProperty<bool> Checked => Property<bool>("checked");

        /// <summary>Whether the button is disabled.</summary>
        public ComponentProperty<bool> Disabled => Property<bool>("disabled");

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public ComponentMethod DataBind => Method("dataBind");

        /// <summary>The value of the checked radio button in the group.</summary>
        public ComponentFunction<string> GetSelectedValue => Function<string>("getSelectedValue");

        /// <summary>Clicks the radio button.</summary>
        public ComponentMethod Click => Method("click");

        /// <summary>Moves focus to the radio button.</summary>
        public ComponentMethod FocusIn => Method("focusIn");

        /// <summary>Raised when the radio button becomes checked.</summary>
        public TypedEvent<FusionRadioButtonChangeArgs> Change =>
            Event<FusionRadioButtonChangeArgs>("change");
    }

    /// <summary>Payload of <see cref="FusionRadioButton.Change"/>.</summary>
    public class FusionRadioButtonChangeArgs
    {
        /// <summary>Value of the checked radio button.</summary>
        public string Value { get; set; } = string.Empty;
    }

    /// <summary>
    /// Creates a FusionRadioButton with Syncfusion MVC builder-owned initial render.
    /// </summary>
    public static class FusionRadioButtonHtmlExtensions
    {
        /// <summary>
        /// Renders one Syncfusion RadioButton with a stable component id.
        /// </summary>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionRadioButton, TModel> FusionRadioButton<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            string elementId,
            Action<RadioButtonBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().RadioButton(elementId);
            build(builder);

            return plan.Render<global::Alis.Reactive.Fusion.Components.FusionRadioButton>(elementId, builder.Render());
        }
    }
}
