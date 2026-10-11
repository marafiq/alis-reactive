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
    /// Render it with <c>Html.FusionProgressButton(plan, "send-report", b =&gt; ...)</c> and target it with
    /// <c>p.Component&lt;FusionProgressButton&gt;("send-report")</c>; every declared member works with <c>Set</c>,
    /// <c>Call</c> and <c>Get</c>. Syncfusion applies property writes on its next tick; follow writes with
    /// <c>.Call(c =&gt; c.DataBind)</c>: <c>.Set(c =&gt; c.Content, "Saved").Call(c =&gt; c.DataBind)</c>.
    /// </remarks>
    public sealed class FusionProgressButton : FusionComponent
    {
        /// <summary>The button text.</summary>
        public ComponentProperty<string> Content => Property<string>("content");

        /// <summary>Whether the button is disabled.</summary>
        public ComponentProperty<bool> Disabled => Property<bool>("disabled");

        /// <summary>The CSS classes on the button.</summary>
        public ComponentProperty<string> CssClass => Property<string>("cssClass");

        /// <summary>Whether the button shows progress while it spins.</summary>
        public ComponentProperty<bool> EnableProgress => Property<bool>("enableProgress");

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public ComponentMethod DataBind => Method("dataBind");

        /// <summary>Starts the progress from the beginning.</summary>
        public ComponentMethod Start => Method("start");

        /// <summary>Starts the progress at a percentage.</summary>
        public ComponentMethod<double> StartAt => Method<double>("start");

        /// <summary>Completes the progress at once.</summary>
        public ComponentMethod ProgressComplete => Method("progressComplete");

        /// <summary>Moves focus to the button.</summary>
        public ComponentMethod FocusIn => Method("focusIn");

        /// <summary>Raised when the progress starts.</summary>
        public TypedEvent<FusionProgressButtonProgressArgs> Begin =>
            Event<FusionProgressButtonProgressArgs>("begin");

        /// <summary>Raised as the progress advances.</summary>
        public TypedEvent<FusionProgressButtonProgressArgs> Progress =>
            Event<FusionProgressButtonProgressArgs>("progress");

        /// <summary>Raised when the progress completes.</summary>
        public TypedEvent<FusionProgressButtonProgressArgs> End =>
            Event<FusionProgressButtonProgressArgs>("end");
    }

    /// <summary>Payload of <see cref="FusionProgressButton.Begin"/>, <see cref="FusionProgressButton.Progress"/> and <see cref="FusionProgressButton.End"/>.</summary>
    public class FusionProgressButtonProgressArgs
    {
        /// <summary>Progress completed, in percent.</summary>
        public double Percent { get; set; }

        /// <summary>Time elapsed, in milliseconds.</summary>
        public double CurrentDuration { get; set; }

        /// <summary>Progress step, in percent.</summary>
        public double Step { get; set; }
    }

    /// <summary>
    /// Creates a FusionProgressButton with Syncfusion MVC builder-owned initial render.
    /// </summary>
    public static class FusionProgressButtonHtmlExtensions
    {
        /// <summary>
        /// Renders one Syncfusion ProgressButton with a stable component id.
        /// </summary>
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
