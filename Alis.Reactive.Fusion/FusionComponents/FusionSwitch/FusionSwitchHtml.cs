using System;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Buttons;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a switch bound to a member of its model.
/// </summary>
public static class FusionSwitchHtml
{
    /// <summary>Renders a switch for the field's member, on when the member holds true.</summary>
    /// <param name="field">The input field the switch is rendered for.</param>
    /// <returns>The rendered switch, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionSwitch, TModel> FusionSwitch<TModel>(this InputField<TModel, bool> field)
        => throw new NotImplementedException();

    /// <summary>Renders a switch for the field's member, on when the member holds true, with what the view sets on EJ2's builder.</summary>
    /// <param name="field">The input field the switch is rendered for.</param>
    /// <param name="build">What the view sets on the switch.</param>
    /// <returns>The rendered switch, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionSwitch, TModel> FusionSwitch<TModel>(this InputField<TModel, bool> field, Action<SwitchBuilder> build)
        => throw new NotImplementedException();
}
