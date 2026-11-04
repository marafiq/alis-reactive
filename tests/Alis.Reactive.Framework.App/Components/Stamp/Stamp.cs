using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Framework.App.Components;

public static class StampHtml
{
    public static IHtmlContent Stamp(this IHtmlHelper html, string id, string text)
        => new HtmlString(
            $"<span class=\"stamp\" id=\"{Encode(id)}\"></span>" +
            $"<script data-stamp-for=\"{Encode(id)}\" data-stamp-text=\"{Encode(text)}\">window.frameworkStamp.build(document.currentScript);</script>");

    private static string Encode(string value) => HtmlEncoder.Default.Encode(value);
}
