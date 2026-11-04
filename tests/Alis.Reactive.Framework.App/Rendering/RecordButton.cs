using System.Text.Encodings.Web;
using Alis.Reactive.Framework.App.Components;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Framework.App.Rendering;

public sealed class RecordButton<TModel> : IHtmlContent
    where TModel : class
{
    private readonly IHtmlHelper<TModel> _html;
    private readonly ReactivePlan<TModel> _plan;
    private readonly RenderedComponent<Button, TModel> _button;

    internal RecordButton(IHtmlHelper<TModel> html, ReactivePlan<TModel> plan, RenderedComponent<Button, TModel> button)
    {
        _html = html;
        _plan = plan;
        _button = button;
    }

    public void WriteTo(TextWriter writer, HtmlEncoder encoder)
    {
        _button.WriteTo(writer, encoder);
        _html.RenderPlan(_plan).WriteTo(writer, encoder);
    }
}

public static class RecordButtonHtml
{
    public static RecordButton<TModel> RecordButton<TModel>(this IHtmlHelper<TModel> html, string id, string text, string stateId)
        where TModel : class
    {
        var plan = html.ReactivePlan<TModel>();
        var button = plan.Button(id, text).Reactive(click => click.Click, p => p.Element(stateId).Text.Set("recorded"));
        return new RecordButton<TModel>(html, plan, button);
    }
}
