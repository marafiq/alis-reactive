using System;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a text area bound to a member of its model.
/// </summary>
public static class FusionTextAreaHtml
{
    /// <summary>Renders a text area for the field's member, holding the member's text.</summary>
    /// <param name="field">The input field the area is rendered for.</param>
    /// <returns>The rendered area, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionTextArea, TModel> FusionTextArea<TModel, TValue>(this InputField<TModel, TValue> field)
        => throw new NotImplementedException();

    /// <summary>Renders a text area for the field's member, holding the member's text, with what the view sets on EJ2's builder.</summary>
    /// <param name="field">The input field the area is rendered for.</param>
    /// <param name="build">What the view sets on the area.</param>
    /// <returns>The rendered area, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionTextArea, TModel> FusionTextArea<TModel, TValue>(this InputField<TModel, TValue> field, Action<TextAreaBuilder> build)
        => throw new NotImplementedException();
}
