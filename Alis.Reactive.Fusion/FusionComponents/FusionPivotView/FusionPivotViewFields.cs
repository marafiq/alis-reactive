using System;
using System.Linq.Expressions;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The door of a view to the fields a pivot view reads of its rows.
/// </summary>
public static class FusionPivotViewFields
{
    /// <summary>Starts the fields of a pivot view over a row class.</summary>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    /// <returns>The fields of the row class.</returns>
    public static FusionPivotViewFields<TRow> Of<TRow>() => new();
}

/// <summary>
/// The fields a pivot view reads of a row class.
/// </summary>
/// <typeparam name="TRow">The class of the rows.</typeparam>
public sealed class FusionPivotViewFields<TRow>
{
    internal FusionPivotViewFields()
    {
    }

    /// <summary>Names a field of the pivot as the app's JSON names the member, the name the pivot's rows hold.</summary>
    /// <remarks>Hand the name to the <c>Name</c> or <c>BaseField</c> of a row, column, value, filter or field mapping of the data source settings on EJ2's builder, to the field a format, sort, filter, drill, group or conditional format setting names, or to an excluded field; the pivot's render helper names it under the view's plan. The pivot reads each field as one member of a row, so a path past one member is refused where the view renders.</remarks>
    /// <typeparam name="TValue">The type of the member.</typeparam>
    /// <param name="member">The member, as <c>r =&gt; r.Ward</c>.</param>
    /// <returns>The field.</returns>
    public string Field<TValue>(Expression<Func<TRow, TValue>> member)
    {
        _ = member ?? throw new ArgumentNullException(nameof(member));
        return FusionPivotViewNames.Defer(names => names.Of(member));
    }
}
