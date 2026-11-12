using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class LobbyController : Controller
{
    [HttpGet(CarePages.Lobby)]
    public IActionResult OfTheHome() => View("/Care/Admission/Lobby.cshtml", new LobbyResident { Room = "12" });
}

public class LobbyPerson
{
    public string? Name { get; set; }
}

public sealed class LobbyResident : LobbyPerson
{
    public required string Room { get; init; }
}

public static class Lobbies
{
    public static readonly AppEvent Greet = AppEvent.Declare();
}
