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
#nullable disable annotations
    /// <summary>
    /// The Syncfusion Tooltip, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionTooltip(plan, "care-tip", b =&gt; ...)</c> and target it with
    /// <c>p.Component&lt;FusionTooltip&gt;("care-tip")</c>; every declared member works with <c>Set</c>,
    /// <c>Call</c> and <c>Get</c>. Its open and close events are <see cref="Opened"/> and <see cref="Closed"/>,
    /// because <see cref="Open"/> and <see cref="Close"/> are its methods.
    /// </remarks>
    public sealed class FusionTooltip : FusionComponent
    {
        /// <summary>Shows the tooltip.</summary>
        public void Open() => Call();

        /// <summary>Hides the tooltip.</summary>
        public void Close() => Call();

        /// <summary>Re-renders the tooltip for its current target.</summary>
        public void Refresh() => Call();

        /// <summary>Raised before the tooltip opens.</summary>
        public TypedEvent<FusionTooltipBeforeOpenArgs> BeforeOpen =>
            Event<FusionTooltipBeforeOpenArgs>("beforeOpen");

        /// <summary>Raised before the tooltip closes.</summary>
        public TypedEvent<FusionTooltipBeforeCloseArgs> BeforeClose =>
            Event<FusionTooltipBeforeCloseArgs>("beforeClose");

        /// <summary>Raised after the tooltip opens (the JavaScript <c>open</c> event).</summary>
        public TypedEvent<FusionTooltipOpenedArgs> Opened =>
            Event<FusionTooltipOpenedArgs>("open");

        /// <summary>Raised after the tooltip closes (the JavaScript <c>close</c> event).</summary>
        public TypedEvent<FusionTooltipClosedArgs> Closed =>
            Event<FusionTooltipClosedArgs>("close");

        /// <summary>Raised before the tooltip renders its content.</summary>
        public TypedEvent<FusionTooltipBeforeRenderArgs> BeforeRender =>
            Event<FusionTooltipBeforeRenderArgs>("beforeRender");
    }

    /// <summary>Payload of <see cref="FusionTooltip.BeforeOpen"/>.</summary>
    public class FusionTooltipBeforeOpenArgs : JsObject
    {
        /// <summary>Set to <see langword="true"/> with <c>p.Payload(args).Set(a =&gt; a.Cancel, true)</c> to stop the action.</summary>
        public Prop<bool> Cancel => Prop<bool>();
    }

    /// <summary>Payload of <see cref="FusionTooltip.BeforeClose"/>.</summary>
    public class FusionTooltipBeforeCloseArgs : JsObject
    {
        /// <summary>Set to <see langword="true"/> with <c>p.Payload(args).Set(a =&gt; a.Cancel, true)</c> to stop the action.</summary>
        public Prop<bool> Cancel => Prop<bool>();
    }

    /// <summary>Payload of <see cref="FusionTooltip.Opened"/>; carries no data.</summary>
    public class FusionTooltipOpenedArgs : JsObject
    {
    }

    /// <summary>Payload of <see cref="FusionTooltip.Closed"/>; carries no data.</summary>
    public class FusionTooltipClosedArgs : JsObject
    {
    }

    /// <summary>Payload of <see cref="FusionTooltip.BeforeRender"/>.</summary>
    public class FusionTooltipBeforeRenderArgs : JsObject
    {
        /// <summary>Set to <see langword="true"/> with <c>p.Payload(args).Set(a =&gt; a.Cancel, true)</c> to stop the action.</summary>
        public Prop<bool> Cancel => Prop<bool>();
    }
#nullable restore annotations

    /// <summary>
    /// Renders the component for a plan, ready for <c>.Reactive(...)</c>.
    /// </summary>
    public static class FusionTooltipHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion Tooltip and carries its controlled component ID into the Reactive Plan.
        /// </summary>
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
