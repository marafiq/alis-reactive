using System;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a slider bound to a member of its model.
/// </summary>
public static class FusionSliderHtml
{
    /// <summary>Renders a slider for the field's member, starting at the member's value.</summary>
    /// <remarks>A member that holds a list of numbers renders a range slider, with a handle for each end.</remarks>
    /// <param name="field">The input field the slider is rendered for.</param>
    /// <returns>The rendered slider, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionSlider<TValue>, TModel> FusionSlider<TModel, TValue>(this InputField<TModel, TValue> field)
        => throw new NotImplementedException();

    /// <summary>Renders a slider for the field's member, starting at the member's value, with what the view sets on EJ2's builder.</summary>
    /// <remarks>A member that holds a list of numbers renders a range slider, with a handle for each end.</remarks>
    /// <param name="field">The input field the slider is rendered for.</param>
    /// <param name="build">What the view sets on the slider.</param>
    /// <returns>The rendered slider, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionSlider<TValue>, TModel> FusionSlider<TModel, TValue>(this InputField<TModel, TValue> field, Action<SliderBuilder> build)
        => throw new NotImplementedException();
}
