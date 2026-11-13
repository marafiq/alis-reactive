using System;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a text box bound to a member of its model.
/// </summary>
public static class FusionTextBoxHtml
{
    /// <summary>Renders a text box for the field's member, holding the member's text.</summary>
    /// <param name="field">The input field the box is rendered for.</param>
    /// <returns>The rendered box, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionTextBox, TModel> FusionTextBox<TModel, TValue>(this InputField<TModel, TValue> field)
        => throw new NotImplementedException();

    /// <summary>Renders a text box for the field's member, holding the member's text, with what the view sets on EJ2's builder.</summary>
    /// <param name="field">The input field the box is rendered for.</param>
    /// <param name="build">What the view sets on the box.</param>
    /// <returns>The rendered box, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionTextBox, TModel> FusionTextBox<TModel, TValue>(this InputField<TModel, TValue> field, Action<TextBoxBuilder> build)
        => throw new NotImplementedException();
}
