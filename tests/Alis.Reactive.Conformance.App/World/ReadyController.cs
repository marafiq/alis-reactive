using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.World;

public sealed class ReadyController : Controller
{
    [HttpGet(WorldEndpoints.Ready)]
    public ViewResult Ready() => View("/World/Ready.cshtml", new ReadyPage("North"));

    [HttpGet(ReadyEndpoints.Doses)]
    public IActionResult Doses() => Json(new List<ReadyDose>
    {
        new("North", true),
        new("North", false),
        new("South", true),
    });
}

public sealed record ReadyPage(string Ward);

public sealed record ReadyDose(string Ward, bool Given);

public static class ReadyEndpoints
{
    public const string Doses = WorldEndpoints.Ready + "/doses";
}
