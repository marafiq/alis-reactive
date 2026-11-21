#if NET48
namespace Alis.Reactive.TriggersAndPipelines;

internal static class RequestHost
{
    internal const bool RespondedJson = true;

    internal static string NoRequestsCall(string request)
        => $"'{request}' is rendered in an app that never called AddReactiveRequests or set MVC's resolver; call services.AddReactiveRequests(requests => ..) once at startup " +
            "and set the resolver over the container with DependencyResolver.SetResolver in Application_Start.";
}
#endif
