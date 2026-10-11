using System.Text.Json.Serialization;
#if NET48
using System.Web;
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
#endif
using Syncfusion.EJ2;
using Syncfusion.EJ2.Notifications;

namespace Alis.Reactive.Fusion.AppLevel
{
    /// <summary>
    /// The page's Syncfusion Toast, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// One toast exists per page, rendered by <c>@Html.FusionToast()</c> in the layout, and is referenced
    /// without an ID. Set what the next toast says, then show it:
    /// <code>
    /// p.Component&lt;FusionToast&gt;()
    ///     .Set(c =&gt; c.Content, "Care plan saved")
    ///     .Set(c =&gt; c.Tone, FusionToastTone.Success)
    ///     .Call(c =&gt; c.Show)
    /// </code>
    /// </remarks>
    public sealed class FusionToast : FusionComponent, IAppLevelComponent
    {
        /// <summary>Well-known layout element ID for the toast.</summary>
        public const string ElementId = "alisFusionToast";

        /// <inheritdoc />
        public string DefaultId => ElementId;

        /// <summary>The heading of the next toast.</summary>
        public ComponentProperty<string> Title => Property<string>("title");

        /// <summary>The message of the next toast.</summary>
        public ComponentProperty<string> Content => Property<string>("content");

        /// <summary>Milliseconds the next toast stays before it hides; 0 keeps it until closed.</summary>
        public ComponentProperty<int> TimeOut => Property<int>("timeOut");

        /// <summary>Whether the next toast shows a close button.</summary>
        public ComponentProperty<bool> ShowCloseButton => Property<bool>("showCloseButton");

        /// <summary>Whether the next toast shows its remaining time as a progress bar.</summary>
        public ComponentProperty<bool> ShowProgressBar => Property<bool>("showProgressBar");

        /// <summary>The look of the next toast: success, warning, danger or info.</summary>
        public ComponentProperty<FusionToastTone> Tone => Property<FusionToastTone>("cssClass");

        /// <summary>Shows a toast with the current title, content and settings.</summary>
        public ComponentMethod Show => Method("show");

        /// <summary>Hides the toast.</summary>
        public ComponentMethod Hide => Method("hide");
    }

    /// <summary>The look of a <see cref="FusionToast"/>, by Syncfusion's toast classes.</summary>
    public enum FusionToastTone
    {
        /// <summary>A completed action.</summary>
        [JsonStringEnumMemberName("e-toast-success")]
        Success,

        /// <summary>Something that needs attention.</summary>
        [JsonStringEnumMemberName("e-toast-warning")]
        Warning,

        /// <summary>A failure.</summary>
        [JsonStringEnumMemberName("e-toast-danger")]
        Danger,

        /// <summary>Information.</summary>
        [JsonStringEnumMemberName("e-toast-info")]
        Info
    }

    /// <summary>Renders the page's <see cref="FusionToast"/>.</summary>
    public static class FusionToastHtmlExtensions
    {
        /// <summary>Renders the toast at the bottom right of the page; call it once in the layout.</summary>
#if NET48
        public static IHtmlString FusionToast(this HtmlHelper html)
#else
        public static IHtmlContent FusionToast(this IHtmlHelper html)
#endif
        {
            return html.EJS().Toast(AppLevel.FusionToast.ElementId)
                .Target("body")
                .Position(new ToastToastPosition { X = "Right", Y = "Bottom" })
                .NewestOnTop(true)
                .ShowCloseButton(true)
                .Render();
        }
    }
}
