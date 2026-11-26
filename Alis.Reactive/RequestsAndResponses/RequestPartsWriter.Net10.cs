#if NET10_0_OR_GREATER
namespace Alis.Reactive.RequestsAndResponses;

internal sealed partial class RequestPartsWriter
{
    private static partial bool ReadsTheQueryBesideABodyThatBindsMembers() => false;
}
#endif
