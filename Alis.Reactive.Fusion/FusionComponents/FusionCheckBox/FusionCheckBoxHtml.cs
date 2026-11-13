using System;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Buttons;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a check box bound to a member of its model.
/// </summary>
public static class FusionCheckBoxHtml
{
    /// <summary>Renders a check box for the field's member, checked when the member holds true.</summary>
    /// <param name="field">The input field the box is rendered for.</param>
    /// <returns>The rendered box, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionCheckBox, TModel> FusionCheckBox<TModel>(this InputField<TModel, bool> field)
        => throw new NotImplementedException();

    /// <summary>Renders a check box for the field's member, checked when the member holds true, with what the view sets on EJ2's builder.</summary>
    /// <param name="field">The input field the box is rendered for.</param>
    /// <param name="build">What the view sets on the box.</param>
    /// <returns>The rendered box, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionCheckBox, TModel> FusionCheckBox<TModel>(this InputField<TModel, bool> field, Action<CheckBoxBuilder> build)
        => throw new NotImplementedException();
}
