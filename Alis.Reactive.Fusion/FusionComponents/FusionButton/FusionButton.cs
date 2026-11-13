using Alis.Reactive.Declarations;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A button: EJ2's Button, which shows text, an icon or both.
/// </summary>
/// <remarks>Call <see cref="DataBind"/> after writing its properties, so the button shows them now.</remarks>
[Mirrors("@syncfusion/ej2-buttons", "Button")]
public sealed class FusionButton : FusionComponent
{
    /// <summary>Gets the text the button shows.</summary>
    public Property<string> Content => Property<string>();

    /// <summary>Gets the CSS classes of the button's icon.</summary>
    public Property<string> IconCss => Property<string>();

    /// <summary>Gets the CSS classes of the button's element.</summary>
    public Property<string> CssClass => Property<string>();

    /// <summary>Gets whether the button is disabled.</summary>
    public Property<bool> Disabled => Property<bool>();

    /// <summary>Gets whether the button shows as the primary one.</summary>
    public Property<bool> IsPrimary => Property<bool>();

    /// <summary>Gets whether a click toggles the button's active state.</summary>
    public Property<bool> IsToggle => Property<bool>();

    /// <summary>Gets where the button's icon sits beside its text.</summary>
    public Property<FusionButtonIconPosition> IconPosition => Property<FusionButtonIconPosition>();

    /// <summary>Applies the changes written to the button's properties now.</summary>
    public void DataBind() => Method();

    /// <summary>Clicks the button, as a person does.</summary>
    public void Click() => Method();

    /// <summary>Moves the keyboard's focus to the button.</summary>
    public void FocusIn() => Method();
}
