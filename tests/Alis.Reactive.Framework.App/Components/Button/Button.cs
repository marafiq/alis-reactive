using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Framework.App.Components;

/// <summary>
/// A plain <c>&lt;button&gt;</c>: the element is the object, reached through the core's native vendor, and a
/// click is its event. The gesture that starts most framework pipelines.
/// </summary>
public sealed class Button : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public Event Click => Event();
}

public static class ButtonHtml
{
    /// <summary>A button with its text, for a page's plan to react to.</summary>
    public static RenderedComponent<Button, TModel> Button<TModel>(this ReactivePlan<TModel> plan, string id, string text)
        where TModel : class =>
        plan.Render<Button>(id, new HtmlString(
            $"<button type=\"button\" id=\"{HtmlEncoder.Default.Encode(id)}\">{HtmlEncoder.Default.Encode(text)}</button>"));
}
