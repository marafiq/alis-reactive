using System;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.InPlaceEditor;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to an in-place editor bound to a member of its model.
/// </summary>
public static partial class FusionInPlaceEditorHtml
{
    /// <summary>Renders an in-place editor for the field's member, starting at the member's value.</summary>
    /// <param name="field">The input field the editor is rendered for.</param>
    /// <returns>The rendered editor, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionInPlaceEditor<TValue>, TModel> FusionInPlaceEditor<TModel, TValue>(this InputField<TModel, TValue> field)
        => field.FusionInPlaceEditor(_ => { });

    /// <summary>Renders an in-place editor for the field's member, starting at the member's value, with what the view sets on EJ2's builder.</summary>
    /// <param name="field">The input field the editor is rendered for.</param>
    /// <param name="build">What the view sets on the editor, its type among them.</param>
    /// <returns>The rendered editor, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionInPlaceEditor<TValue>, TModel> FusionInPlaceEditor<TModel, TValue>(this InputField<TModel, TValue> field, Action<InPlaceEditorBuilder> build)
    {
        var editor = InPlaceEditorOf(field).Name(field.Name).Value(FusionData.AsWritten(field.ValueJson));
        (build ?? throw new ArgumentNullException(nameof(build)))(editor);
        return RenderedEditor(field, editor);
    }

    private static partial InPlaceEditorBuilder InPlaceEditorOf<TModel, TValue>(InputField<TModel, TValue> field);

    private static partial Rendered<FusionInPlaceEditor<TValue>, TModel> RenderedEditor<TModel, TValue>(InputField<TModel, TValue> field, InPlaceEditorBuilder editor);
}
