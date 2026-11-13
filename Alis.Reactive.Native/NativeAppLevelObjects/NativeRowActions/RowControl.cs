using System;

namespace Alis.Reactive.Native.NativeAppLevelObjects;

/// <summary>
/// One row's control of a row action: a button, or a link whose address is the action's request.
/// </summary>
/// <remarks>The control's id and, for a link, its address are the action's; a view sets its classes.</remarks>
public sealed partial class RowControl
{
    internal RowControl()
    {
    }

    /// <summary>Sets the CSS classes of the control's element.</summary>
    /// <param name="css">The class names, separated by spaces.</param>
    /// <returns>The control.</returns>
    public RowControl CssClass(string css) => throw new NotImplementedException();
}
