#if NET48
using System;
using System.Web;
using System.Web.Mvc;
using Confirm = Alis.Reactive.Native.NativeAppLevelObjects.NativeConfirm;

namespace Alis.Reactive.Native.NativeAppLevelObjects;

/// <summary>
/// The door of a layout to the page's confirm.
/// </summary>
public static class NativeConfirmHtml
{
    private const string TitleId = "alis-confirm-title";
    private const string MessageId = "alis-confirm-message";

    /// <summary>Renders the page's confirm, hidden, once in the layout, with the app's heading and button texts.</summary>
    /// <remarks>Focus starts on the cancel button when a question shows, and Escape answers no.</remarks>
    /// <param name="html">The layout's HTML helper.</param>
    /// <param name="title">The heading every question shows.</param>
    /// <param name="ok">The text of the button that answers yes.</param>
    /// <param name="cancel">The text of the button that answers no.</param>
    /// <returns>The confirm's markup.</returns>
    public static IHtmlString NativeConfirm(this HtmlHelper html, string title, string ok, string cancel)
    {
        var dialog = new TagBuilder("dialog");
        dialog.MergeAttribute("id", Confirm.ElementId);
        dialog.MergeAttribute("class", "alis-confirm");
        dialog.MergeAttribute("aria-labelledby", TitleId);
        dialog.MergeAttribute("aria-describedby", MessageId);
        var heading = new TagBuilder("h2");
        heading.MergeAttribute("id", TitleId);
        heading.SetInnerText(title ?? throw new ArgumentNullException(nameof(title)));
        var message = new TagBuilder("p");
        message.MergeAttribute("id", MessageId);
        var no = new TagBuilder("button");
        no.MergeAttribute("type", "button");
        no.MergeAttribute("class", "alis-confirm__cancel");
        no.MergeAttribute("autofocus", "autofocus");
        no.SetInnerText(cancel ?? throw new ArgumentNullException(nameof(cancel)));
        var yes = new TagBuilder("button");
        yes.MergeAttribute("type", "button");
        yes.MergeAttribute("class", "alis-confirm__ok");
        yes.SetInnerText(ok ?? throw new ArgumentNullException(nameof(ok)));
        dialog.InnerHtml = heading.ToString(TagRenderMode.Normal) + message.ToString(TagRenderMode.Normal) + no.ToString(TagRenderMode.Normal) + yes.ToString(TagRenderMode.Normal);
        return new HtmlString(dialog.ToString(TagRenderMode.Normal));
    }
}
#endif
