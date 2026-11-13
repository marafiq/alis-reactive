using System;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.RichTextEditor;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a rich text editor bound to a member of its model.
/// </summary>
public static class FusionRichTextEditorHtml
{
    /// <summary>Renders a rich text editor for the field's member, holding the member's content.</summary>
    /// <param name="field">The input field the editor is rendered for.</param>
    /// <returns>The rendered editor, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionRichTextEditor, TModel> FusionRichTextEditor<TModel, TValue>(this InputField<TModel, TValue> field)
        => throw new NotImplementedException();

    /// <summary>Renders a rich text editor for the field's member, holding the member's content, with what the view sets on EJ2's builder.</summary>
    /// <param name="field">The input field the editor is rendered for.</param>
    /// <param name="build">What the view sets on the editor.</param>
    /// <returns>The rendered editor, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionRichTextEditor, TModel> FusionRichTextEditor<TModel, TValue>(this InputField<TModel, TValue> field, Action<RichTextEditorBuilder> build)
        => throw new NotImplementedException();
}
