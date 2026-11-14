using System;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;

namespace Alis.Reactive.Native.NativeAppLevelObjects;

/// <summary>
/// The doors of a view to the row actions of a grid-like list: each declared once, rendered in every row.
/// </summary>
/// <remarks>
/// An action's steps run for an unmodified click with the primary button; a link's address is its block's one GET,
/// so a click with a modifier key, or with the middle button, opens it as the browser does.
/// </remarks>
public static class NativeRowActions
{
    /// <summary>Declares a row action whose control is a button: its steps run each time a person clicks one row's button.</summary>
    /// <param name="rows">The rows the action serves.</param>
    /// <param name="action">The steps the action runs, with the row whose button was clicked.</param>
    /// <typeparam name="TModel">The model of the view that declares the action.</typeparam>
    /// <typeparam name="TRow">The type of a row.</typeparam>
    /// <returns>The action, rendered in each row with <c>Render</c>.</returns>
    public static RowAction<TModel, TRow> NativeActionButton<TModel, TRow>(this Rows<TModel, TRow> rows, Action<RowActivated<TRow>, Pipeline<TModel>> action)
        => throw new NotImplementedException();

    /// <summary>Declares a row action whose control is a button, under a policy for a click that comes while the action's last run is in flight.</summary>
    /// <remarks>The policy spans every row: a click on any row's button meets the run a click on any other row started.</remarks>
    /// <param name="rows">The rows the action serves.</param>
    /// <param name="policy">What a click does while the run the last click started is in flight.</param>
    /// <param name="action">The steps the action runs, with the row whose button was clicked.</param>
    /// <typeparam name="TModel">The model of the view that declares the action.</typeparam>
    /// <typeparam name="TRow">The type of a row.</typeparam>
    /// <returns>The action, rendered in each row with <c>Render</c>.</returns>
    public static RowAction<TModel, TRow> NativeActionButton<TModel, TRow>(this Rows<TModel, TRow> rows, HandlerPolicy policy, Action<RowActivated<TRow>, Pipeline<TModel>> action)
        => throw new NotImplementedException();

    /// <summary>Declares a row action whose control is a link: its steps, one GET whose address the link holds, run each time a person clicks one row's link.</summary>
    /// <param name="rows">The rows the action serves.</param>
    /// <param name="action">The steps the action runs, with the row whose link was clicked: one GET.</param>
    /// <typeparam name="TModel">The model of the view that declares the action.</typeparam>
    /// <typeparam name="TRow">The type of a row.</typeparam>
    /// <returns>The action, rendered in each row with <c>Render</c>.</returns>
    public static RowAction<TModel, TRow> NativeActionLink<TModel, TRow>(this Rows<TModel, TRow> rows, Action<RowActivated<TRow>, Pipeline<TModel>> action)
        => throw new NotImplementedException();

    /// <summary>Declares a row action whose control is a link, under a policy for a click that comes while the action's last run is in flight.</summary>
    /// <remarks>The policy spans every row: a click on any row's link meets the run a click on any other row started.</remarks>
    /// <param name="rows">The rows the action serves.</param>
    /// <param name="policy">What a click does while the run the last click started is in flight.</param>
    /// <param name="action">The steps the action runs, with the row whose link was clicked: one GET.</param>
    /// <typeparam name="TModel">The model of the view that declares the action.</typeparam>
    /// <typeparam name="TRow">The type of a row.</typeparam>
    /// <returns>The action, rendered in each row with <c>Render</c>.</returns>
    public static RowAction<TModel, TRow> NativeActionLink<TModel, TRow>(this Rows<TModel, TRow> rows, HandlerPolicy policy, Action<RowActivated<TRow>, Pipeline<TModel>> action)
        => throw new NotImplementedException();
}
