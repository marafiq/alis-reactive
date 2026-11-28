using System;
using System.Text.Json;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Buttons;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a switch bound to a member of its model.
/// </summary>
public static partial class FusionSwitchHtml
{
    /// <summary>Renders a switch for the field's member, on when the member holds true.</summary>
    /// <param name="field">The input field the switch is rendered for.</param>
    /// <returns>The rendered switch, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionSwitch, TModel> FusionSwitch<TModel>(this InputField<TModel, bool> field)
        => field.FusionSwitch(_ => { });

    /// <summary>Renders a switch for the field's member, on when the member holds true, with what the view sets on EJ2's builder.</summary>
    /// <param name="field">The input field the switch is rendered for.</param>
    /// <param name="build">What the view sets on the switch.</param>
    /// <returns>The rendered switch, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionSwitch, TModel> FusionSwitch<TModel>(this InputField<TModel, bool> field, Action<SwitchBuilder> build)
    {
        var toggle = SwitchOf(field).Checked(HoldsTrue(field.ValueJson)).Name(field.Name);
        (build ?? throw new ArgumentNullException(nameof(build)))(toggle);
        return RenderedSwitch(field, toggle);
    }

    private static bool HoldsTrue(string valueJson)
    {
        using var held = JsonDocument.Parse(valueJson);
        return held.RootElement.ValueKind == JsonValueKind.True;
    }

    private static partial SwitchBuilder SwitchOf<TModel>(InputField<TModel, bool> field);

    private static partial Rendered<FusionSwitch, TModel> RenderedSwitch<TModel>(InputField<TModel, bool> field, SwitchBuilder toggle);
}
