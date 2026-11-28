#if NET48
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Alis.Reactive.RequestsAndResponses;

internal static partial class PageMembers
{
    private static readonly EmptyModelMetadataProvider Metadata = new();

    internal static partial IReadOnlyCollection<string> Of<TPage>()
        => Metadata.GetMetadataForType(null, typeof(TPage)).Properties.Select(property => property.PropertyName).ToList();
}
#endif
