using System;
using System.Collections.Generic;
using System.Text.Json;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a color picker bound to a member of its model.
/// </summary>
public static partial class FusionColorPickerHtml
{
    /// <summary>Renders a color picker for the field's member, holding the member's color.</summary>
    /// <param name="field">The input field the picker is rendered for.</param>
    /// <returns>The rendered picker, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionColorPicker, TModel> FusionColorPicker<TModel, TValue>(this InputField<TModel, TValue> field)
        => field.FusionColorPicker(_ => { });

    /// <summary>Renders a color picker for the field's member, holding the member's color, with what the view sets on EJ2's builder.</summary>
    /// <param name="field">The input field the picker is rendered for.</param>
    /// <param name="build">What the view sets on the picker.</param>
    /// <returns>The rendered picker, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionColorPicker, TModel> FusionColorPicker<TModel, TValue>(this InputField<TModel, TValue> field, Action<ColorPickerBuilder> build)
    {
        var picker = ColorPickerOf(field).Value(TextOf(field.ValueJson)).HtmlAttributes(new Dictionary<string, object> { ["name"] = field.Name });
        (build ?? throw new ArgumentNullException(nameof(build)))(picker);
        return RenderedPicker(field, picker);
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

    private static partial ColorPickerBuilder ColorPickerOf<TModel, TValue>(InputField<TModel, TValue> field);

    private static partial Rendered<FusionColorPicker, TModel> RenderedPicker<TModel, TValue>(InputField<TModel, TValue> field, ColorPickerBuilder picker);
}
