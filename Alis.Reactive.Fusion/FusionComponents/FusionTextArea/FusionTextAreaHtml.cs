using System;
using System.Collections.Generic;
using System.Text.Json;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a text area bound to a member of its model.
/// </summary>
public static partial class FusionTextAreaHtml
{
    /// <summary>Renders a text area for the field's member, holding the member's text.</summary>
    /// <param name="field">The input field the area is rendered for.</param>
    /// <returns>The rendered area, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionTextArea, TModel> FusionTextArea<TModel, TValue>(this InputField<TModel, TValue> field)
        => field.FusionTextArea(_ => { });

    /// <summary>Renders a text area for the field's member, holding the member's text, with what the view sets on EJ2's builder.</summary>
    /// <param name="field">The input field the area is rendered for.</param>
    /// <param name="build">What the view sets on the area.</param>
    /// <returns>The rendered area, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionTextArea, TModel> FusionTextArea<TModel, TValue>(this InputField<TModel, TValue> field, Action<TextAreaBuilder> build)
    {
        var area = TextAreaOf(field).Value(TextOf(field.ValueJson)).HtmlAttributes(new Dictionary<string, object> { ["name"] = field.Name });
        (build ?? throw new ArgumentNullException(nameof(build)))(area);
        return RenderedArea(field, area);
    }

    private static string? TextOf(string valueJson)
    {
        using var held = JsonDocument.Parse(valueJson);
        var value = held.RootElement;
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Null => null,
            _ => value.GetRawText(),
        };
    }

    private static partial TextAreaBuilder TextAreaOf<TModel, TValue>(InputField<TModel, TValue> field);

    private static partial Rendered<FusionTextArea, TModel> RenderedArea<TModel, TValue>(InputField<TModel, TValue> field, TextAreaBuilder area);
}
