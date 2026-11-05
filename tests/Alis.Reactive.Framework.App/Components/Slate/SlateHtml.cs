using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Framework.App.Components;

public static class SlateHtml
{
    public static RenderedComponent<Slate, TModel> Slate<TModel>(
        this ReactivePlan<TModel> plan,
        string id,
        string title,
        params string[] rows)
        where TModel : class
    {
        var markup = new StringBuilder()
            .Append("<div class=\"slate\" id=\"").Append(Encode(id))
            .Append("\" data-title=\"").Append(Encode(title))
            .Append("\" data-rows=\"").Append(Encode(string.Join("|", rows)))
            .Append("\"></div>");
        return plan.Render<Slate>(id, new HtmlString(markup.ToString()));
    }

    public static RenderedComponent<Slate, TModel> Slate<TModel>(
        this ReactivePlan<TModel> plan,
        string id,
        string title,
        DateTimeOffset startedAt,
        params string[] rows)
        where TModel : class
    {
        var markup = new StringBuilder()
            .Append("<div class=\"slate\" id=\"").Append(Encode(id))
            .Append("\" data-title=\"").Append(Encode(title))
            .Append("\" data-started=\"").Append(startedAt.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture))
            .Append("\" data-rows=\"").Append(Encode(string.Join("|", rows)))
            .Append("\"></div>");
        return plan.Render<Slate>(id, new HtmlString(markup.ToString()));
    }

    private static string Encode(string value) => HtmlEncoder.Default.Encode(value);
}
