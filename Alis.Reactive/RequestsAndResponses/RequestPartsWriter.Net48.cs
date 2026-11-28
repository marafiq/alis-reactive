#if NET48
namespace Alis.Reactive.RequestsAndResponses;

internal sealed partial class RequestPartsWriter
{
    private static partial bool ReadsTheQueryBesideABodyThatBindsMembers() => true;
}
#endif
