using System;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a masked text box bound to a member of its model.
/// </summary>
public static class FusionMaskedTextBoxHtml
{
    /// <summary>Renders a masked text box for the field's member, holding the member's value.</summary>
    /// <param name="field">The input field the box is rendered for.</param>
    /// <returns>The rendered box, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionMaskedTextBox, TModel> FusionMaskedTextBox<TModel, TValue>(this InputField<TModel, TValue> field)
        => throw new NotImplementedException();

    /// <summary>Renders a masked text box for the field's member, holding the member's value, with what the view sets on EJ2's builder.</summary>
    /// <param name="field">The input field the box is rendered for.</param>
    /// <param name="build">What the view sets on the box, its mask among them.</param>
    /// <returns>The rendered box, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionMaskedTextBox, TModel> FusionMaskedTextBox<TModel, TValue>(this InputField<TModel, TValue> field, Action<MaskedTextBoxBuilder> build)
        => throw new NotImplementedException();
}
