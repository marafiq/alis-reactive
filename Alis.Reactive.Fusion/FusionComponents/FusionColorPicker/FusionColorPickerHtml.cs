using System;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a color picker bound to a member of its model.
/// </summary>
public static class FusionColorPickerHtml
{
    /// <summary>Renders a color picker for the field's member, holding the member's color.</summary>
    /// <param name="field">The input field the picker is rendered for.</param>
    /// <returns>The rendered picker, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionColorPicker, TModel> FusionColorPicker<TModel, TValue>(this InputField<TModel, TValue> field)
        => throw new NotImplementedException();

    /// <summary>Renders a color picker for the field's member, holding the member's color, with what the view sets on EJ2's builder.</summary>
    /// <param name="field">The input field the picker is rendered for.</param>
    /// <param name="build">What the view sets on the picker.</param>
    /// <returns>The rendered picker, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionColorPicker, TModel> FusionColorPicker<TModel, TValue>(this InputField<TModel, TValue> field, Action<ColorPickerBuilder> build)
        => throw new NotImplementedException();
}
