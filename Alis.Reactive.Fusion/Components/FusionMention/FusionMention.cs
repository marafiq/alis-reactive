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
    /// Render it with <c>Html.FusionMention(plan, "care-mention", b =&gt; ...)</c>, reach it with
    /// <c>var mention = p.Component&lt;FusionMention&gt;("care-mention");</c> and use its members directly:
    /// <c>mention.Search("No", 96, 160);</c>.
    /// </remarks>
    public sealed class FusionMention : FusionComponent
    {
        /// <summary>Filters the suggestions by a text and shows them at a position, in pixels.</summary>
        /// <param name="text">The text the suggestions are filtered by.</param>
        /// <param name="positionX">The popup's horizontal position, in pixels.</param>
        /// <param name="positionY">The popup's vertical position, in pixels.</param>
        public void Search(Arg<string> text, Arg<int> positionX, Arg<int> positionY) => Method(text, positionX, positionY);

        /// <summary>Opens the suggestion popup.</summary>
        public void ShowPopup() => Method();

        /// <summary>Closes the suggestion popup.</summary>
        public void HidePopup() => Method();

        /// <summary>Raised when the user picks a suggestion.</summary>
        public Event<FusionMentionChangeArgs> Change =>
            Event<FusionMentionChangeArgs>();

        /// <summary>Raised after the suggestion popup opens.</summary>
        public Event<FusionMentionPopupArgs> Opened =>
            Event<FusionMentionPopupArgs>();

        /// <summary>Raised after the suggestion popup closes.</summary>
        public Event<FusionMentionPopupArgs> Closed =>
            Event<FusionMentionPopupArgs>();
    }

    /// <summary>Payload of <see cref="FusionMention.Change"/>.</summary>
    public class FusionMentionChangeArgs : JsObject
    {
        /// <summary>The picked suggestion's value.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();

        /// <summary>Whether user interaction made the pick; always true, as EJ2 raises Change only for a pick made by key or click (mention.js:1231, 1553).</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }

    /// <summary>Payload of <see cref="FusionMention.Opened"/> and <see cref="FusionMention.Closed"/>; carries no data.</summary>
    public class FusionMentionPopupArgs : JsObject
    {
    }

    /// <summary>Renders a <see cref="FusionMention"/> for a plan and maps its fields.</summary>
    public static class FusionMentionHtmlExtensions
    {
        /// <summary>Renders a Syncfusion Mention with the element ID the view chooses.</summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <param name="html">The view's HTML helper.</param>
        /// <param name="plan">The plan the component's events are wired into.</param>
        /// <param name="elementId">The component's element ID, chosen by the view.</param>
        /// <param name="build">Configures the Syncfusion builder before rendering.</param>
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
            var content = new HtmlString(RenderBridge(elementId, hostId) + builder.Render().ToHtmlString());
#else
            var content = new HtmlContentBuilder();
            content.AppendHtml(RenderBridge(elementId, hostId));
            content.AppendHtml(builder.Render());
#endif

            return plan.Render<global::Alis.Reactive.Fusion.Components.FusionMention>(elementId, content);
        }

        /// <summary>Maps display text and value fields using typed item expressions.</summary>
        /// <typeparam name="TItem">The item class the data source holds.</typeparam>
        /// <param name="builder">The Syncfusion builder being configured.</param>
        /// <param name="text">The item member shown as each suggestion's text.</param>
        /// <param name="value">The item member used as each suggestion's value.</param>
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

        // A Mention with a target turns its own element into the suggestion popup and renames it when the
        // popup first opens, so the component's element is a hidden span: the framework's mention code
        // (run on alis:mount, like an app's widget code) gives it the Mention's instance.
        private static string RenderBridge(string elementId, string hostId)
        {
            return "<span id=\"" + HtmlEncoder.Default.Encode(elementId) + "\" hidden data-alis-mention=\""
                + HtmlEncoder.Default.Encode(hostId) + "\"></span>";
        }
    }
}
