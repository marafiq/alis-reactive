using Alis.Reactive.Declarations;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>A live EJ2 Query supplied by the application for local filtering.</summary>
[Mirrors("@syncfusion/ej2-data", "Query")]
public sealed class FusionDropDownListQuery : JsObject
{
    /// <summary>Copies the configured query and its existing operations.</summary>
    /// <returns>A new query that can be changed independently.</returns>
    public FusionDropDownListQuery Clone() => ReturnedObject<FusionDropDownListQuery>();

    /// <summary>Keeps the first rows.</summary>
    /// <param name="count">The number of rows to retain.</param>
    /// <returns>This live query with the operation applied.</returns>
    public MethodResult<FusionDropDownListQuery> Take(Arg<int> count) => Method<FusionDropDownListQuery>([count]);

    /// <summary>Skips the first rows.</summary>
    /// <param name="count">The number of rows to skip.</param>
    /// <returns>This live query with the operation applied.</returns>
    public MethodResult<FusionDropDownListQuery> Skip(Arg<int> count) => Method<FusionDropDownListQuery>([count]);

    /// <summary>Restricts the query to the specified range.</summary>
    /// <param name="start">The inclusive starting index.</param>
    /// <param name="end">The exclusive ending index.</param>
    /// <returns>This live query with the operation applied.</returns>
    public MethodResult<FusionDropDownListQuery> Range(Arg<int> start, Arg<int> end) => Method<FusionDropDownListQuery>([start, end]);

    /// <summary>Selects a page from the query.</summary>
    /// <param name="pageIndex">The one-based page index.</param>
    /// <param name="pageSize">The number of rows per page.</param>
    /// <returns>This live query with the operation applied.</returns>
    public MethodResult<FusionDropDownListQuery> Page(Arg<int> pageIndex, Arg<int> pageSize) => Method<FusionDropDownListQuery>([pageIndex, pageSize]);
}
