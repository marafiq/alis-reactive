using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Alis.Reactive.Conformance.App.Care;
using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.Extensions.DependencyInjection;

namespace Alis.Reactive.Conformance.App;

public class TwinApplication : HttpApplication
{
    protected void Application_Start()
    {
        var services = new ServiceCollection();
        services.AddReactiveRequests(requests => requests.Antiforgery(AntiforgeryIn.Header));
        services.AddReactiveJson(json => json.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));
        services.AddReactiveFluentValidation(validation => validation.Add<RoomNumberingRules>());
        DependencyResolver.SetResolver(new TwinResolver(DependencyResolver.Current, services.BuildServiceProvider()));
        RouteTable.Routes.MapMvcAttributeRoutes();
    }

    protected void Application_BeginRequest()
    {
        var context = new HttpContextWrapper(Context);
        if (!RequestCulturesAndZones.Take(context))
        {
            CompleteRequest();
            return;
        }

        TwinWorlds.Open(context);
    }

    protected void Application_Error() => ErrorPage.Answer(Context);
}
