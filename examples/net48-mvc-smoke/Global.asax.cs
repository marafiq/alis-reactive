using System.Web.Mvc;
using System.Web.Routing;
using Alis.Reactive;
using Microsoft.Extensions.DependencyInjection;

namespace Net48MvcSmoke
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            var services = new ServiceCollection();
            services.AddReactiveFluentValidation(validation => validation.AddFromAssemblyContaining<MvcApplication>());
            var provider = services.BuildServiceProvider();
            DependencyResolver.SetResolver(provider.GetService, provider.GetServices);
        }
    }
}
