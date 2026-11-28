using System;
using System.Collections.Generic;
using System.Text.Json;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a text box bound to a member of its model.
/// </summary>
public static partial class FusionTextBoxHtml
{
    /// <summary>Renders a text box for the field's member, holding the member's text.</summary>
    /// <param name="field">The input field the box is rendered for.</param>
    /// <returns>The rendered box, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionTextBox, TModel> FusionTextBox<TModel, TValue>(this InputField<TModel, TValue> field)
        => field.FusionTextBox(_ => { });

    /// <summary>Renders a text box for the field's member, holding the member's text, with what the view sets on EJ2's builder.</summary>
    /// <param name="field">The input field the box is rendered for.</param>
    /// <param name="build">What the view sets on the box.</param>
    /// <returns>The rendered box, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionTextBox, TModel> FusionTextBox<TModel, TValue>(this InputField<TModel, TValue> field, Action<TextBoxBuilder> build)
    {
        var box = TextBoxOf(field).Value(TextOf(field.ValueJson)).HtmlAttributes(new Dictionary<string, object> { ["name"] = field.Name });
        (build ?? throw new ArgumentNullException(nameof(build)))(box);
        return RenderedBox(field, box);
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

    private static partial TextBoxBuilder TextBoxOf<TModel, TValue>(InputField<TModel, TValue> field);

    private static partial Rendered<FusionTextBox, TModel> RenderedBox<TModel, TValue>(InputField<TModel, TValue> field, TextBoxBuilder box);
}
