using System;
using System.Linq.Expressions;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The door of a view to the fields of a grid's columns.
/// </summary>
public static class FusionGridColumns
{
    /// <summary>Starts the fields of the columns of a grid over a row class.</summary>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    /// <returns>The fields of the row class.</returns>
    public static FusionGridColumns<TRow> Of<TRow>() => throw new NotImplementedException();
}

/// <summary>
/// The fields of the columns of a grid over a row class.
/// </summary>
/// <typeparam name="TRow">The class of the rows.</typeparam>
public sealed class FusionGridColumns<TRow>
{
    internal FusionGridColumns()
    {
    }

    /// <summary>Names a column's field as the app's JSON names the member, the name the grid's rows hold.</summary>
    /// <remarks>Hand the name to the column's <c>Field</c> on EJ2's builder.</remarks>
    /// <typeparam name="TValue">The type of the member.</typeparam>
    /// <param name="member">The member, as <c>r =&gt; r.RoomNumber</c>.</param>
    /// <returns>The field.</returns>
    public string Field<TValue>(Expression<Func<TRow, TValue>> member) => throw new NotImplementedException();
}
