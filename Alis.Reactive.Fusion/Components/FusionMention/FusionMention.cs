using System;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
#if NET48
using System.Web;
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
#endif
using Syncfusion.EJ2;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion Mention, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionMention(plan, "care-mention", b =&gt; ...)</c> and target it with
    /// <c>p.Component&lt;FusionMention&gt;("care-mention")</c>; every declared member works with <c>Set</c>,
    /// <c>Call</c> and <c>Get</c>, for example <c>.Call(c =&gt; c.Search, "No", 96, 160)</c>.
    /// </remarks>
    public sealed class FusionMention : FusionComponent
    {
        /// <summary>Filters the suggestions by a text and shows them at a position, in pixels.</summary>
        public ComponentMethod<string, int, int> Search => Method<string, int, int>("search");

        /// <summary>Opens the suggestion popup.</summary>
        public ComponentMethod ShowPopup => Method("showPopup");

        /// <summary>Closes the suggestion popup.</summary>
        public ComponentMethod HidePopup => Method("hidePopup");

        /// <summary>Raised when the user picks a suggestion.</summary>
        public TypedEvent<FusionMentionChangeArgs> Change =>
            Event<FusionMentionChangeArgs>("change");

        /// <summary>Raised after the suggestion popup opens.</summary>
        public TypedEvent<FusionMentionPopupArgs> Opened =>
            Event<FusionMentionPopupArgs>("opened");

        /// <summary>Raised after the suggestion popup closes.</summary>
        public TypedEvent<FusionMentionPopupArgs> Closed =>
            Event<FusionMentionPopupArgs>("closed");
    }

    /// <summary>Payload of <see cref="FusionMention.Change"/>.</summary>
    public class FusionMentionChangeArgs
    {
        /// <summary>The picked suggestion's value.</summary>
        public string? Value { get; set; }

        /// <summary>Whether user interaction made the pick.</summary>
        public bool IsInteracted { get; set; }
    }

    /// <summary>Payload of <see cref="FusionMention.Opened"/> and <see cref="FusionMention.Closed"/>; carries no data.</summary>
    public class FusionMentionPopupArgs
    {
    }

    public static class FusionMentionHtmlExtensions
    {
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionMention, TModel> FusionMention<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            string elementId,
            Action<MentionBuilder> build)
            where TModel : class
        {
            var hostId = elementId + "-host";
            var builder = html.EJS().Mention(hostId);
            build(builder);

#if NET48
            var content = new HtmlString(
                RenderBridge(elementId, hostId, builder.model.Target)
                + builder.Render().ToHtmlString());
#else
            var content = new HtmlContentBuilder();
            content.AppendHtml(RenderBridge(elementId, hostId, builder.model.Target));
            content.AppendHtml(builder.Render());
#endif

            return plan.Render<global::Alis.Reactive.Fusion.Components.FusionMention>(elementId, content);
        }

        public static MentionBuilder Fields<TItem>(
            this MentionBuilder builder,
            Expression<Func<TItem, object?>> text,
            Expression<Func<TItem, object?>> value)
        {
            return builder.Fields(new MentionFieldSettings
            {
                Text = Field.Of(text).Name,
                Value = Field.Of(value).Name
            });
        }

        private static string RenderBridge(string elementId, string hostId, string targetSelector)
        {
            var encodedElementId = JavaScriptEncoder.Default.Encode(elementId);
            var encodedHostId = JavaScriptEncoder.Default.Encode(hostId);
            var encodedTargetSelector = JavaScriptEncoder.Default.Encode(targetSelector);

            return "<span id=\"" + HtmlEncoder.Default.Encode(elementId) + "\" hidden></span>"
                + "<script>(function(){document.addEventListener('DOMContentLoaded',function(){"
                + "var bridge=document.getElementById('" + encodedElementId + "');"
                + "var host=document.getElementById('" + encodedHostId + "');"
                + "var target=document.querySelector('" + encodedTargetSelector + "');"
                + "var source=(host&&host.ej2_instances)?host:target;"
                + "if(bridge&&source&&source.ej2_instances){bridge.ej2_instances=source.ej2_instances;}"
                + "});})();</script>";
        }
    }
}
