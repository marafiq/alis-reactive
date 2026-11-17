using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Alis.Reactive.Consumer.Net48
{
    public class ContainerResolver : IDependencyResolver
    {
        private readonly IDependencyResolver _mvc;
        private readonly IServiceProvider _container;

        public ContainerResolver(IDependencyResolver mvc, IServiceProvider container)
        {
            _mvc = mvc;
            _container = container;
        }

        public object GetService(Type serviceType) => _container.GetService(serviceType) ?? _mvc.GetService(serviceType);

        public IEnumerable<object> GetServices(Type serviceType) => _container.GetServices(serviceType).Concat(_mvc.GetServices(serviceType));
    }
}
