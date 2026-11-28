#if NET10_0_OR_GREATER
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Alis.Reactive.RequestsAndResponses;

internal static partial class PageMembers
{
    private static readonly EmptyModelMetadataProvider Metadata = new();

    internal static partial IReadOnlyCollection<string> Of<TPage>()
        => [.. Metadata.GetMetadataForType(typeof(TPage)).Properties.Select(property => property.PropertyName).OfType<string>()];
}
#endif
