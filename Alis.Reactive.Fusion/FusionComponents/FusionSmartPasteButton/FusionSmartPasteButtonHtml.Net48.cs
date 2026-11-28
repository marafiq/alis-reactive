#if NET48
using System;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The door of a view to a smart paste button.
/// </summary>
public static partial class FusionSmartPasteButtonHtml
{
    /// <summary>Renders a smart paste button under the id the view gives it, with the page's antiforgery token.</summary>
    /// <remarks>A button whose <see cref="FusionSmartPasteButtonOptions.AiAssistEndpoint"/> is empty is refused when the view renders it.</remarks>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="elementId">The id the view gives the button's element.</param>
    /// <param name="options">What the view sets on the button.</param>
    /// <returns>The button's markup, encoded as HTML.</returns>
    public static IHtmlString FusionSmartPasteButton<TModel>(this HtmlHelper<TModel> html, string elementId, Action<FusionSmartPasteButtonOptions> options)
    {
        var set = Set(elementId, options);
        var button = new TagBuilder("button");
        button.MergeAttribute("id", elementId);
        button.MergeAttribute("type", "button");
        if (!string.IsNullOrEmpty(set.CssClass)) button.AddCssClass(set.CssClass);
        button.MergeAttribute("data-alis-smart-paste-button", ConstructionOptions(set, RequestToken(html.ViewContext.HttpContext)));
        button.SetInnerText(set.Content);
        return new MvcHtmlString(button.ToString(TagRenderMode.Normal));
    }

    private static string RequestToken(HttpContextBase context)
    {
        AntiForgery.GetTokens(context.Request.Cookies[AntiForgeryConfig.CookieName]?.Value, out var newCookie, out var formToken);
        if (newCookie is not null)
            context.Response.Cookies.Set(new HttpCookie(AntiForgeryConfig.CookieName, newCookie) { HttpOnly = true, Secure = AntiForgeryConfig.RequireSsl });
        return formToken;
    }
}
#endif
