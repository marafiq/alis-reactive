using System.Web.Mvc;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.TriggersAndPipelines;

namespace Alis.Reactive.Conformance.App.World;

public sealed class ReadyController : Controller
{
    [HttpGet]
    [Address(WorldEndpoints.Ready)]
    public ActionResult Ready() => View("/World/Ready.cshtml", new ReadyPage("North", DependencyResolver.Current.GetService<RequestDefaults>() is not null));

    [HttpGet]
    [Address(ReadyEndpoints.Doses)]
    public ActionResult Doses() => this.JsonResponse(new List<ReadyDose>
    {
        new("North", true),
        new("North", false),
        new("South", true),
    });
}
