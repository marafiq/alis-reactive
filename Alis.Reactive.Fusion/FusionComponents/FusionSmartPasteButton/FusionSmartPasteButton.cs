using Alis.Reactive.Declarations;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A smart paste button: EJ2's SmartPasteButton, which fills its form's fields from the text a person copied, as the
/// app's endpoint reads it.
/// </summary>
[Mirrors("@syncfusion/ej2-buttons", "SmartPasteButton")]
public sealed class FusionSmartPasteButton : FusionComponent
{
    /// <summary>Gets whether the button is disabled.</summary>
    public Property<bool> Disabled => Property<bool>();

    /// <summary>Clicks the button, as a person does.</summary>
    public void Click() => Method();

    /// <summary>Moves the keyboard's focus to the button.</summary>
    public void FocusIn() => Method();
}

/// <summary>
/// What a view sets on a smart paste button it renders.
/// </summary>
public sealed class FusionSmartPasteButtonOptions
{
    /// <summary>Gets or sets the button's text.</summary>
    public string Content { get; set; } = "";

    /// <summary>Gets or sets the URL of the app's endpoint that reads the copied text into the form's fields.</summary>
    /// <remarks>A button with no endpoint is refused when the view renders it.</remarks>
    public string AiAssistEndpoint { get; set; } = "";

    /// <summary>Gets or sets the CSS classes of the button's element, separated by spaces.</summary>
    public string CssClass { get; set; } = "";

    /// <summary>Gets or sets whether the button shows as the form's primary button.</summary>
    public bool IsPrimary { get; set; }
}
