using System;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The door of a view to a numeric text box bound to a member of its model.
/// </summary>
public static class FusionNumericTextBoxHtml
{
    /// <summary>Renders a numeric text box for the field's member, holding the member's number, with what the view sets on EJ2's builder.</summary>
    /// <param name="field">The input field the box is rendered for.</param>
    /// <param name="build">What the view sets on the box.</param>
    /// <returns>The rendered box, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionNumericTextBox<TValue>, TModel> FusionNumericTextBox<TModel, TValue>(this InputField<TModel, TValue> field, Action<NumericTextBoxBuilder> build)
        => throw new NotImplementedException();
}
