using System.Globalization;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Framework.App.Components;

/// <summary>
/// A text entry for a model member (entry.ts): the input element keeps the entry as <c>entry</c>, takes
/// listeners through <c>on</c> and <c>off</c>, and is committed when the user leaves it after a change.
/// Whatever the member's type, the entry holds text; the framework reads it under the member's shape.
/// </summary>
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

    /// <summary>Raised when the user leaves the entry after changing it.</summary>
    public Event<EntryCommitted> Committed => Event<EntryCommitted>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Committed);
}

public sealed class EntryCommitted : JsObject
{
    public ReadOnly<string> Value => ReadOnly<string>();
}

public static class EntryHtml
{
    /// <summary>An entry for the member the input field binds, showing the member's value as text.</summary>
    public static RenderedComponent<Entry, TModel> Entry<TModel, TProp>(this InputBoundField<TModel, TProp> field)
        where TModel : class
    {
        var model = field.Helper.ViewData.Model;
        var value = model == null ? "" : Text(field.Expression.Compile()(model));
        return field.Render<Entry>(new HtmlString(
            $"<input class=\"entry\" id=\"{HtmlEncoder.Default.Encode(field.ElementId)}\" type=\"text\" value=\"{HtmlEncoder.Default.Encode(value)}\">"));
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
