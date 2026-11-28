#if NET48
using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.RequestsAndResponses;
using Microsoft.Extensions.DependencyInjection;

namespace Alis.Reactive.RenderingAndDelivery;

/// <summary>
/// The startup call that registers the JSON options an MVC 5 app writes its data with.
/// </summary>
public static class JsonServices
{
    /// <summary>Registers the app's JSON options, once at startup.</summary>
    /// <remarks>
    /// The options start as ASP.NET Core MVC's start, camel-case names, so one setting writes the same data on both hosts:
    /// <c>services.AddReactiveJson(json =&gt; json.Converters.Add(new JsonStringEnumConverter()));</c>. Each view and each
    /// <c>JsonResponse</c> reads them through MVC's resolver, set over the app's container with
    /// <c>DependencyResolver.SetResolver</c>; a view that reads them in an app without either is refused, naming both.
    /// </remarks>
    /// <param name="services">The app's services.</param>
    /// <param name="json">Sets the options the app chooses.</param>
    /// <returns>The app's services.</returns>
    public static IServiceCollection AddReactiveJson(this IServiceCollection services, Action<JsonSerializerOptions> json)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };
        json(options);
        options.MakeReadOnly();
        return services.AddSingleton(new RegisteredJson(options));
    }
}
#endif
