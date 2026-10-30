using System;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif
using Syncfusion.EJ2;
using Syncfusion.EJ2.SplitButtons;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion ProgressButton, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionProgressButton(plan, "send-report", b =&gt; ...)</c>, reach it with
    /// <c>var sendReport = p.Component&lt;FusionProgressButton&gt;("send-report");</c> and use its members directly.
    /// Syncfusion applies property writes on its next tick; follow writes with <see cref="DataBind"/>:
    /// <c>sendReport.Content.Set("Saved"); sendReport.DataBind();</c>.
    /// </remarks>
    public sealed class FusionProgressButton : FusionComponent
    {
        /// <summary>The button text.</summary>
        public Property<string> Content => Property<string>();

        /// <summary>Whether the button is disabled.</summary>
        public Property<bool> Disabled => Property<bool>();

        /// <summary>The CSS classes on the button.</summary>
        public Property<string> CssClass => Property<string>();

        /// <summary>Whether the button shows progress while it spins.</summary>
        public Property<bool> EnableProgress => Property<bool>();

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public void DataBind() => Method();

        /// <summary>Starts the progress from the beginning.</summary>
        public void Start() => Method();

        /// <summary>Starts the progress at a percentage.</summary>
        /// <param name="percent">The percent the progress starts at.</param>
        public void StartAt(Arg<double> percent) => Method(percent, name: "start");

        /// <summary>Completes the progress at once.</summary>
        public void ProgressComplete() => Method();

        /// <summary>Moves focus to the button.</summary>
        public void FocusIn() => Method();

        /// <summary>Raised when the progress starts.</summary>
        public Event<FusionProgressButtonProgressArgs> Begin =>
            Event<FusionProgressButtonProgressArgs>();

        /// <summary>Raised as the progress advances.</summary>
        public Event<FusionProgressButtonProgressArgs> Progress =>
            Event<FusionProgressButtonProgressArgs>();

        /// <summary>Raised when the progress completes.</summary>
        public Event<FusionProgressButtonProgressArgs> End =>
            Event<FusionProgressButtonProgressArgs>();
    }

    /// <summary>Payload of <see cref="FusionProgressButton.Begin"/>, <see cref="FusionProgressButton.Progress"/> and <see cref="FusionProgressButton.End"/>.</summary>
    public class FusionProgressButtonProgressArgs : JsObject
    {
        /// <summary>Progress completed, in percent.</summary>
        public ReadOnly<double> Percent => ReadOnly<double>();

        /// <summary>Time elapsed, in milliseconds.</summary>
        public ReadOnly<double> CurrentDuration => ReadOnly<double>();

        /// <summary>Progress step, in percent.</summary>
        public ReadOnly<double> Step => ReadOnly<double>();
    }

    /// <summary>
    /// Creates a FusionProgressButton with Syncfusion MVC builder-owned initial render.
    /// </summary>
    public static class FusionProgressButtonHtmlExtensions
    {
        /// <summary>
        /// Renders one Syncfusion ProgressButton with a stable component id.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <param name="html">The view's HTML helper.</param>
        /// <param name="plan">The plan the component's events are wired into.</param>
        /// <param name="elementId">The component's element ID, chosen by the view.</param>
        /// <param name="build">Configures the Syncfusion builder before rendering.</param>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionProgressButton, TModel> FusionProgressButton<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            string elementId,
            Action<ProgressButtonBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().ProgressButton(elementId);
            build(builder);

            return plan.Render<global::Alis.Reactive.Fusion.Components.FusionProgressButton>(elementId, builder.Render());
        }
    }
}
