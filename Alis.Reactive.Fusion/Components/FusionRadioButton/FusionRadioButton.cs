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
    /// Render it with <c>Html.FusionRadioButton(plan, "companion-suite", b =&gt; ...)</c>, reach it with
    /// <c>var companionSuite = p.Component&lt;FusionRadioButton&gt;("companion-suite");</c> and use its members
    /// directly: <c>companionSuite.Checked.Set(true); companionSuite.DataBind();</c> or
    /// <c>p.When(companionSuite.GetSelectedValue()).Eq("companion")</c>.
    /// </remarks>
    public sealed class FusionRadioButton : FusionComponent
    {
        /// <summary>Whether the radio button is checked.</summary>
        public Property<bool> Checked => Property<bool>();

        /// <summary>Whether the button is disabled.</summary>
        public Property<bool> Disabled => Property<bool>();

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public void DataBind() => Method();

        /// <summary>The value of the checked radio button in the group.</summary>
        public TypedSource<string> GetSelectedValue() => Method<string>();

        /// <summary>Clicks the radio button.</summary>
        public void Click() => Method();

        /// <summary>Moves focus to the radio button.</summary>
        public void FocusIn() => Method();

        /// <summary>Raised when the radio button becomes checked.</summary>
        public Event<FusionRadioButtonChangeArgs> Change =>
            Event<FusionRadioButtonChangeArgs>();
    }

    /// <summary>Payload of <see cref="FusionRadioButton.Change"/>.</summary>
    public class FusionRadioButtonChangeArgs : JsObject
    {
        /// <summary>Value of the checked radio button.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();
    }

    /// <summary>
    /// Creates a FusionRadioButton with Syncfusion MVC builder-owned initial render.
    /// </summary>
    public static class FusionRadioButtonHtmlExtensions
    {
        /// <summary>
        /// Renders one Syncfusion RadioButton with a stable component id.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <param name="html">The view's HTML helper.</param>
        /// <param name="plan">The plan the component's events are wired into.</param>
        /// <param name="elementId">The component's element ID, chosen by the view.</param>
        /// <param name="build">Configures the Syncfusion builder before rendering.</param>
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
