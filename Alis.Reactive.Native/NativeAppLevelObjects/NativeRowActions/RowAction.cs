using System;

namespace Alis.Reactive.Native.NativeAppLevelObjects;

/// <summary>
/// An action a view declares once over a list's rows and renders in each row with that row's values.
/// </summary>
/// <typeparam name="TModel">The model of the view that declares the action.</typeparam>
/// <typeparam name="TRow">The type of a row.</typeparam>
public sealed class RowAction<TModel, TRow>
{
    internal RowAction()
    {
    }

    /// <summary>Renders the action's control for one row, with its text.</summary>
    /// <param name="row">The row the control acts on.</param>
    /// <param name="text">The control's text.</param>
    /// <returns>The control, which writes its markup where the view puts it.</returns>
    public RowControl Render(TRow row, string text) => throw new NotImplementedException();
}
