using System;
using System.Collections.Generic;
using System.Text.Json;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a one-time code input bound to a member of text of its model.
/// </summary>
public static partial class FusionOtpInputHtml
{
    /// <summary>Renders a one-time code input for the field's member, starting at the member's text.</summary>
    /// <param name="field">The input field the input is rendered for.</param>
    /// <returns>The rendered input, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionOtpInput, TModel> FusionOtpInput<TModel>(this InputField<TModel, string?> field)
        => field.FusionOtpInput(_ => { });

    /// <summary>Renders a one-time code input for the field's member, starting at the member's text, with what the view sets on EJ2's builder.</summary>
    /// <param name="field">The input field the input is rendered for.</param>
    /// <param name="build">What the view sets on the input.</param>
    /// <returns>The rendered input, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionOtpInput, TModel> FusionOtpInput<TModel>(this InputField<TModel, string?> field, Action<OtpInputBuilder> build)
    {
        var box = OtpInputOf(field).Value(TextOf(field.ValueJson)).HtmlAttributes(new Dictionary<string, object> { ["name"] = field.Name });
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

    private static partial OtpInputBuilder OtpInputOf<TModel>(InputField<TModel, string?> field);

    private static partial Rendered<FusionOtpInput, TModel> RenderedBox<TModel>(InputField<TModel, string?> field, OtpInputBuilder box);
}
