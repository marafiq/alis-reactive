using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.World;

public sealed class ReadyController : Controller
{
    [HttpGet(WorldEndpoints.Ready)]
    public ViewResult Ready() => View("/World/Ready.cshtml", new ReadyPage("North", HttpContext.RequestServices.GetService<RequestDefaults>() is not null));

    [HttpGet(ReadyEndpoints.Doses)]
    public IActionResult Doses() => Json(new List<ReadyDose>
    {
        new("North", true),
        new("North", false),
        new("South", true),
    });
}
