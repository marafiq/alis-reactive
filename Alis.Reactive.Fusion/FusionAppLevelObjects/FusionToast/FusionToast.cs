using Alis.Reactive.Declarations;
using Alis.Reactive.Fusion.FusionComponents;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.Fusion.FusionAppLevelObjects;

/// <summary>
/// The page's FusionToast: EJ2's Toast, made once on the element the layout renders, which a view fills and shows.
/// </summary>
/// <remarks>
/// A view reaches it with no id, <c>pipeline.Component&lt;FusionToast&gt;()</c>, sets its members and calls
/// <see cref="Show"/>. Its title and content show as text, markup included.
/// </remarks>
[Mirrors("@syncfusion/ej2-notifications", "Toast")]
public sealed class FusionToast : FusionComponent, IAppLevelComponent
{
    /// <summary>The id of the toast's element on every page.</summary>
    public const string ElementId = "alisFusionToast";

    /// <summary>Gets the id of the toast's element, <see cref="ElementId"/>.</summary>
    public string DefaultId => ElementId;

    /// <summary>Gets the toast's title, shown as text; null until set.</summary>
    public Property<string?> Title => Property<string?>();

    /// <summary>Gets the toast's content, shown as text; null until set.</summary>
    public Property<string?> Content => Property<string?>();

    /// <summary>Gets the milliseconds a toast shows before it hides itself, 5000 unless set; 0 keeps it until it is closed.</summary>
    public Property<int> TimeOut => Property<int>();

    /// <summary>Gets whether a toast shows its close button.</summary>
    public Property<bool> ShowCloseButton => Property<bool>();

    /// <summary>Gets whether a toast shows the bar of the time it has left.</summary>
    public Property<bool> ShowProgressBar => Property<bool>();

    /// <summary>Gets the toast's tone, written as EJ2's class for it.</summary>
    public Property<FusionToastTone> CssClass => Property<FusionToastTone>();

    /// <summary>Shows a toast with the title, content, tone, time and buttons as last set, the newest on top.</summary>
    public void Show() => Method();

    /// <summary>Hides the newest toast.</summary>
    public void Hide() => Method();
}
