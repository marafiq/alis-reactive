using System;
using System.Linq.Expressions;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Names a grid's columns by the members of its row class, so the compiler checks each column's field.
    /// </summary>
    /// <remarks>
    /// <code>
    /// var columns = FusionGridColumns.Of&lt;ResidentCareItem&gt;();
    /// new GridColumn { Field = columns.Field(r =&gt; r.OpenTasks), HeaderText = "Open Tasks" }
    /// </code>
    /// </remarks>
    public static class FusionGridColumns
    {
        /// <summary>The column names of a grid whose rows are <typeparamref name="TRow"/>.</summary>
        /// <typeparam name="TRow">Your row class, the one the grid is rendered with.</typeparam>
        public static FusionGridColumns<TRow> Of<TRow>() where TRow : class => new FusionGridColumns<TRow>();
    }

    /// <summary>The column names of a grid whose rows are <typeparamref name="TRow"/>.</summary>
    /// <typeparam name="TRow">Your row class.</typeparam>
    public sealed class FusionGridColumns<TRow> where TRow : class
    {
        internal FusionGridColumns()
        {
        }

        /// <summary>
        /// The field of the column that shows a member of the row: its name as the row's JSON carries it
        /// (<c>r =&gt; r.OpenTasks</c> is <c>"openTasks"</c>, <c>r =&gt; r.MRN</c> is <c>"mrn"</c>,
        /// <c>r =&gt; r.Address.City</c> is <c>"address.city"</c>).
        /// </summary>
        /// <typeparam name="TValue">The type of the member.</typeparam>
        /// <param name="member">The member, for example <c>r =&gt; r.OpenTasks</c>.</param>
        public string Field<TValue>(Expression<Func<TRow, TValue>> member) => global::Alis.Reactive.Field.Of(member).Name;
    }
}
