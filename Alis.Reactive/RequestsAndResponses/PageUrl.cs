using System;
using System.Linq.Expressions;
using Alis.Reactive.Declarations;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.RequestsAndResponses;

/// <summary>
/// The page's own URL, read as the action that rendered the page bound it.
/// </summary>
/// <typeparam name="TPage">The type the page's action binds its URL to.</typeparam>
public sealed class PageUrl<TPage> : JsObject
{
    private PageUrl()
    {
    }

    /// <summary>Reads one member of the page's URL: a placeholder's from the path, any other from the query by its C# path.</summary>
    /// <remarks>
    /// A key is compared ignoring case. A member whose key the URL lacks, or whose text does not convert to the
    /// member's type, reads as a new <typeparamref name="TPage"/> would hold it.
    /// </remarks>
    /// <param name="member">The member to read, as a member path.</param>
    /// <returns>A source of the member's value.</returns>
    public TypedSource<TValue> Read<TValue>(Expression<Func<TPage, TValue>> member) => throw new NotImplementedException();
}
