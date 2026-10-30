using System;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif
using Syncfusion.EJ2;
using Syncfusion.EJ2.Popups;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion Tooltip, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionTooltip(plan, "care-tip", b =&gt; ...)</c>, reach it with
    /// <c>var careTip = p.Component&lt;FusionTooltip&gt;("care-tip");</c> and use its members directly:
    /// <c>careTip.Open();</c>. Its open and close events are <see cref="Opened"/> and <see cref="Closed"/>,
    /// because <see cref="Open"/> and <see cref="Close"/> are its methods.
    /// </remarks>
    public sealed class FusionTooltip : FusionComponent
    {
        /// <summary>Shows the tooltip.</summary>
        public void Open() => Method();

        /// <summary>Hides the tooltip.</summary>
        public void Close() => Method();

        /// <summary>Re-renders the tooltip for its current target.</summary>
        public void Refresh() => Method();

        /// <summary>Raised before the tooltip opens.</summary>
        public Event<FusionTooltipBeforeOpenArgs> BeforeOpen =>
            Event<FusionTooltipBeforeOpenArgs>();

        /// <summary>Raised before the tooltip closes.</summary>
        public Event<FusionTooltipBeforeCloseArgs> BeforeClose =>
            Event<FusionTooltipBeforeCloseArgs>();

        /// <summary>Raised after the tooltip opens.</summary>
        public Event<FusionTooltipAfterOpenArgs> AfterOpen =>
            Event<FusionTooltipAfterOpenArgs>();

        /// <summary>Raised after the tooltip closes.</summary>
        public Event<FusionTooltipAfterCloseArgs> AfterClose =>
            Event<FusionTooltipAfterCloseArgs>();

        /// <summary>Raised before the tooltip renders its content.</summary>
        public Event<FusionTooltipBeforeRenderArgs> BeforeRender =>
            Event<FusionTooltipBeforeRenderArgs>();
    }

    /// <summary>Payload of <see cref="FusionTooltip.BeforeOpen"/>.</summary>
    public class FusionTooltipBeforeOpenArgs : JsObject
    {
        /// <summary>Set to <see langword="true"/> with <c>args.Cancel.Set(true)</c> to stop the action.</summary>
        public Property<bool> Cancel => Property<bool>();
    }

    /// <summary>Payload of <see cref="FusionTooltip.BeforeClose"/>.</summary>
    public class FusionTooltipBeforeCloseArgs : JsObject
    {
        /// <summary>Set to <see langword="true"/> with <c>args.Cancel.Set(true)</c> to stop the action.</summary>
        public Property<bool> Cancel => Property<bool>();
    }

    /// <summary>Payload of <see cref="FusionTooltip.AfterOpen"/>; carries no data.</summary>
    public class FusionTooltipAfterOpenArgs : JsObject
    {
    }

    /// <summary>Payload of <see cref="FusionTooltip.AfterClose"/>; carries no data.</summary>
    public class FusionTooltipAfterCloseArgs : JsObject
    {
    }

    /// <summary>Payload of <see cref="FusionTooltip.BeforeRender"/>.</summary>
    public class FusionTooltipBeforeRenderArgs : JsObject
    {
        /// <summary>Set to <see langword="true"/> with <c>args.Cancel.Set(true)</c> to stop the action.</summary>
        public Property<bool> Cancel => Property<bool>();
    }

    /// <summary>
    /// Renders the component for a plan, ready for <c>.Reactive(...)</c>.
    /// </summary>
    public static class FusionTooltipHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion Tooltip and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <param name="html">The view's HTML helper.</param>
        /// <param name="plan">Reactive Plan receiving tooltip event wiring.</param>
        /// <param name="elementId">Controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the component before rendering.</param>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionTooltip, TModel> FusionTooltip<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            string elementId,
            Action<TooltipBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().Tooltip(elementId);
            build(builder);

            return plan.Render<global::Alis.Reactive.Fusion.Components.FusionTooltip>(elementId, builder.Render());
        }
    }
}
