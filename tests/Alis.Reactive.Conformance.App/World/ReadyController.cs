using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.World;

public sealed class ReadyController : Controller
{
    [HttpGet(WorldEndpoints.Ready)]
    public ViewResult Ready() => View("/World/Ready.cshtml", new ReadyPage());
}

public sealed class ReadyPage;
