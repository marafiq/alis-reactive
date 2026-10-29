using System.Text.Encodings.Web;
using Alis.Reactive.Framework.App.Components;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Framework.App.Rendering;

/// <summary>
/// A button with a plan of its own that renders the plan beside its markup when the markup is written, as a render
/// helper may: ASP.NET Core writes a view's HTML content when the page is sent, after every view has run.
/// </summary>
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
    /// <summary>A button that writes "recorded" into the element named, on a plan of its own.</summary>
    public static RecordButton<TModel> RecordButton<TModel>(this IHtmlHelper<TModel> html, string id, string text, string stateId)
        where TModel : class
    {
        var plan = html.ReactivePlan<TModel>();
        var button = plan.Button(id, text).Reactive(click => click.Click, p => p.Element(stateId).Text.Set("recorded"));
        return new RecordButton<TModel>(html, plan, button);
    }
}
