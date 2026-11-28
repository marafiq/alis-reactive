#if NET10_0_OR_GREATER
namespace Alis.Reactive.TriggersAndPipelines;

internal static class RequestHost
{
    internal const bool RespondedJson = false;

    internal static string NoRequestsCall(string request)
        => $"'{request}' is rendered in an app that never called AddReactiveRequests; call services.AddReactiveRequests(requests => ..) once at startup.";
}
#endif
