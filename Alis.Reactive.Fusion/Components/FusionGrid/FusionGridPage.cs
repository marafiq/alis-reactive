using System.Collections.Generic;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// One page of a grid's rows, as EJ2's data manager reads a remote result: the rows, and how many
    /// rows there are in all. A response class implements it so a page writes it with
    /// <c>grid.Page&lt;Row&gt;().Set(answer)</c>.
    /// </summary>
    /// <typeparam name="TRow">The row class.</typeparam>
    public interface IFusionGridPage<out TRow>
    {
        /// <summary>The rows of this page.</summary>
        IReadOnlyList<TRow> Result { get; }

        /// <summary>How many rows there are across every page.</summary>
        int Count { get; }
    }
}
