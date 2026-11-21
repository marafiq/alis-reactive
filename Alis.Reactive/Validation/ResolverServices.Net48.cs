#if NET48
using System;
using System.Web.Mvc;

namespace Alis.Reactive.Validation;

internal sealed class ResolverServices : IServiceProvider
{
    public object? GetService(Type serviceType) => DependencyResolver.Current.GetService(serviceType);
}
#endif
