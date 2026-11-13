using System;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a one-time code input bound to a member of text of its model.
/// </summary>
public static class FusionOtpInputHtml
{
    /// <summary>Renders a one-time code input for the field's member, starting at the member's text.</summary>
    /// <param name="field">The input field the input is rendered for.</param>
    /// <returns>The rendered input, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionOtpInput, TModel> FusionOtpInput<TModel>(this InputField<TModel, string?> field)
        => throw new NotImplementedException();

    /// <summary>Renders a one-time code input for the field's member, starting at the member's text, with what the view sets on EJ2's builder.</summary>
    /// <param name="field">The input field the input is rendered for.</param>
    /// <param name="build">What the view sets on the input.</param>
    /// <returns>The rendered input, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionOtpInput, TModel> FusionOtpInput<TModel>(this InputField<TModel, string?> field, Action<OtpInputBuilder> build)
        => throw new NotImplementedException();
}
