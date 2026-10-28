using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Framework.App.Components;

/// <summary>
/// A stamp its own script builds (stamp.ts): the render helper writes the stamp's element and, after it, the
/// script that constructs it, the way a vendor's render helper writes the script that builds its widget. No
/// plan reaches it; HTML loaded into a slot shows it built only when the slot runs the scripts it inserts.
/// </summary>
public static class StampHtml
{
    /// <summary>A stamp showing its text once its script has run.</summary>
    public static IHtmlContent Stamp(this IHtmlHelper html, string id, string text) =>
        new HtmlString(
            $"<span class=\"stamp\" id=\"{Encode(id)}\"></span>" +
            $"<script data-stamp-for=\"{Encode(id)}\" data-stamp-text=\"{Encode(text)}\">window.frameworkStamp.build(document.currentScript);</script>");

    private static string Encode(string value) => HtmlEncoder.Default.Encode(value);
}
