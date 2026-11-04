using System.Globalization;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Framework.App.Components;

public sealed class Entry : Component, IInputComponent
{
    public static ComponentVendor EntryVendor { get; } = ComponentVendor.Define(
        "entry",
        objectPath: "entry",
        subscribe: "on",
        unsubscribe: "off",
        payload: VendorEventPayload.Argument);

    public override ComponentVendor Vendor => EntryVendor;

    public Property<string> Value => Property<string>();

    public Event<EntryCommitted> Committed => Event<EntryCommitted>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Committed);
}

public sealed class EntryCommitted : JsObject
{
    public ReadOnly<string> Value => ReadOnly<string>();
}

public static class EntryHtml
{
    public static RenderedComponent<Entry, TModel> Entry<TModel, TProp>(this InputBoundField<TModel, TProp> field)
        where TModel : class
        => field.Render<Entry>(Markup(field, "text"));

    public static RenderedComponent<Entry, TModel> HiddenEntry<TModel, TProp>(this InputBoundField<TModel, TProp> field)
        where TModel : class
        => field.Render<Entry>(Markup(field, "hidden"));

    private static HtmlString Markup<TModel, TProp>(InputBoundField<TModel, TProp> field, string type)
        where TModel : class
    {
        var model = field.Helper.ViewData.Model;
        var value = model == null ? "" : Text(field.Expression.Compile()(model));
        return new HtmlString(
            $"<input class=\"entry\" id=\"{HtmlEncoder.Default.Encode(field.ElementId)}\" name=\"{HtmlEncoder.Default.Encode(field.BindingPath)}\" type=\"{type}\" value=\"{HtmlEncoder.Default.Encode(value)}\">");
    }

    private static string Text(object? value) => value switch
    {
        null => "",
        DateTime day => day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        DateTimeOffset moment => moment.ToString("o", CultureInfo.InvariantCulture),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? "",
    };
}
