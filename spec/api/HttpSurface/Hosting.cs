using System;
using Alis.Reactive.Pipelines;
using Microsoft.Extensions.DependencyInjection;

namespace Alis.Reactive.Hosting;

public static class RequestServices
{
    public static IServiceCollection AddReactiveRequests(this IServiceCollection services, Action<RequestDefaults> requests)
        => throw new NotSupportedException();
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public sealed class ValidateAntiForgeryHeaderAttribute : Attribute
{
}
