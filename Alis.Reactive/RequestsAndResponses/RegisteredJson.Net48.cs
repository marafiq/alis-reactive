#if NET48
using System;
using System.Text.Json;
using System.Web.Mvc;

namespace Alis.Reactive.RequestsAndResponses;

internal sealed class RegisteredJson
{
    internal RegisteredJson(JsonSerializerOptions options)
    {
        Options = options;
    }

    internal JsonSerializerOptions Options { get; }

    internal static JsonSerializerOptions ReadBy(string reader)
        => DependencyResolver.Current.GetService<RegisteredJson>() is { } registered
            ? registered.Options
            : throw new InvalidOperationException(
                $"'{reader}' reads the app's JSON options in an app that never called AddReactiveJson or set MVC's resolver; call services.AddReactiveJson(json => ..) once at startup " +
                "and set the resolver over the container with DependencyResolver.SetResolver in Application_Start.");
}
#endif
