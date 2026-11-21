using System;
using System.Linq.Expressions;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The door of a view to the fields of a multi-column combo box's columns.
/// </summary>
public static class FusionMultiColumnComboBoxColumns
{
    /// <summary>Starts the fields of the columns of a multi-column combo box over a row class.</summary>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    /// <returns>The fields of the row class.</returns>
    public static FusionMultiColumnComboBoxColumns<TRow> Of<TRow>() => new();
}

/// <summary>
/// The fields of the columns of a multi-column combo box over a row class.
/// </summary>
/// <typeparam name="TRow">The class of the rows.</typeparam>
public sealed class FusionMultiColumnComboBoxColumns<TRow>
{
    internal FusionMultiColumnComboBoxColumns()
    {
    }

    /// <summary>Names a column's field as the app's JSON names the member, the name the box's rows hold.</summary>
    /// <remarks>Hand the name to the column's <c>Field</c> on EJ2's builder; the box's render helper names it under the view's plan.</remarks>
    /// <typeparam name="TValue">The type of the member.</typeparam>
    /// <param name="member">The member, as <c>r =&gt; r.BedCode</c>.</param>
    /// <returns>The field.</returns>
    public string Field<TValue>(Expression<Func<TRow, TValue>> member)
    {
        _ = member ?? throw new ArgumentNullException(nameof(member));
        return FusionMultiColumnComboBoxHtml.Defer(names => names.Of(member));
    }
}
