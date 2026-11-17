using System;
using System.Collections.Generic;
using System.Text.Json;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.RichTextEditor;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a rich text editor bound to a member of its model.
/// </summary>
public static partial class FusionRichTextEditorHtml
{
    /// <summary>Renders a rich text editor for the field's member, holding the member's content.</summary>
    /// <param name="field">The input field the editor is rendered for.</param>
    /// <returns>The rendered editor, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionRichTextEditor, TModel> FusionRichTextEditor<TModel, TValue>(this InputField<TModel, TValue> field)
        => field.FusionRichTextEditor(_ => { });

    /// <summary>Renders a rich text editor for the field's member, holding the member's content, with what the view sets on EJ2's builder.</summary>
    /// <param name="field">The input field the editor is rendered for.</param>
    /// <param name="build">What the view sets on the editor.</param>
    /// <returns>The rendered editor, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionRichTextEditor, TModel> FusionRichTextEditor<TModel, TValue>(this InputField<TModel, TValue> field, Action<RichTextEditorBuilder> build)
    {
        var editor = RichTextEditorOf(field).Value(TextOf(field.ValueJson)).HtmlAttributes(new Dictionary<string, object> { ["name"] = field.Name });
        (build ?? throw new ArgumentNullException(nameof(build)))(editor);
        return RenderedEditor(field, editor);
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

    private static partial RichTextEditorBuilder RichTextEditorOf<TModel, TValue>(InputField<TModel, TValue> field);

    private static partial Rendered<FusionRichTextEditor, TModel> RenderedEditor<TModel, TValue>(InputField<TModel, TValue> field, RichTextEditorBuilder editor);
}
