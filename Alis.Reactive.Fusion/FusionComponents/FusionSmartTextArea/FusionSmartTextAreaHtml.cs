using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.RenderingAndDelivery;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a smart text area bound to a member of its model.
/// </summary>
public static partial class FusionSmartTextAreaHtml
{
    /// <summary>Renders a smart text area for the field's member, starting at the member's text.</summary>
    /// <param name="field">The input field the area is rendered for.</param>
    /// <returns>The rendered area, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionSmartTextArea, TModel> FusionSmartTextArea<TModel, TValue>(this InputField<TModel, TValue> field)
        => field.FusionSmartTextArea(_ => { });

    /// <summary>Renders a smart text area for the field's member, starting at the member's text, with what the view sets on it.</summary>
    /// <param name="field">The input field the area is rendered for.</param>
    /// <param name="options">What the view sets on the area: who is typing, the phrases, the endpoint and where a suggestion shows.</param>
    /// <returns>The rendered area, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionSmartTextArea, TModel> FusionSmartTextArea<TModel, TValue>(this InputField<TModel, TValue> field, Action<FusionSmartTextAreaOptions> options)
    {
        var set = new FusionSmartTextAreaOptions();
        (options ?? throw new ArgumentNullException(nameof(options)))(set);
        using var value = JsonDocument.Parse(field.ValueJson);
        var start = field.Json(new SmartTextAreaStart(value.RootElement, set));
        return Area(field, new Dictionary<string, string> { ["id"] = field.Id, ["name"] = field.Name, ["data-fusion-smart-text-area"] = start });
    }

    private static partial Rendered<FusionSmartTextArea, TModel> Area<TModel, TValue>(InputField<TModel, TValue> field, Dictionary<string, string> attributes);
}

internal sealed class SmartTextAreaStart
{
    internal SmartTextAreaStart(JsonElement value, FusionSmartTextAreaOptions options)
    {
        Value = value;
        UserRole = options.UserRole;
        UserPhrases = options.UserPhrases;
        SuggestionEndpoint = options.SuggestionEndpoint.Length == 0 ? null : options.SuggestionEndpoint;
        SuggestionMode = options.SuggestionMode;
        CssClass = options.CssClass;
        Rows = options.Rows;
    }

    [JsonPropertyName("value")]
    public JsonElement Value { get; }

    [JsonPropertyName("userRole")]
    public string UserRole { get; }

    [JsonPropertyName("UserPhrases")]
    public IReadOnlyList<string> UserPhrases { get; }

    [JsonPropertyName("suggestionEndpoint")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SuggestionEndpoint { get; }

    [JsonPropertyName("showSuggestionOnPopup")]
    public FusionSmartSuggestionMode SuggestionMode { get; }

    [JsonPropertyName("cssClass")]
    public string CssClass { get; }

    [JsonPropertyName("rows")]
    public int Rows { get; }
}
