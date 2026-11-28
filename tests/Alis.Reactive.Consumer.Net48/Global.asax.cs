using System.Web.Mvc;
using System.Web.Routing;
using Alis.Reactive.RenderingAndDelivery;
using Microsoft.Extensions.DependencyInjection;

namespace Alis.Reactive.Consumer.Net48
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            var services = new ServiceCollection();
            services.AddReactiveJson(json => { });
            DependencyResolver.SetResolver(new ContainerResolver(DependencyResolver.Current, services.BuildServiceProvider()));
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}
