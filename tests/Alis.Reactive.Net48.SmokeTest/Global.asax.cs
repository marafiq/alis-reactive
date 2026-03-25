using System;
using System.Web.Mvc;
using System.Web.Routing;
using Alis.Reactive;
using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.Net48.SmokeTest
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            ReactivePlanConfig.UseValidationExtractor(
                new FluentValidationAdapter(type =>
                    (FluentValidation.IValidator)Activator.CreateInstance(type)));
        }
    }
}
