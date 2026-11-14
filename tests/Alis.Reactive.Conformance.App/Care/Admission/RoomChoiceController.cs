using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RoomChoiceController : Controller
{
    [HttpGet(CarePages.RoomChoiceUnderReplace)]
    public IActionResult UnderReplace() => View("/Care/Admission/RoomChoice.cshtml", new RoomChoice(HandlerPolicy.Replace, TheWards(), null));

    [HttpGet(CarePages.RoomChoiceUnderOverlap)]
    public IActionResult UnderOverlap() => View("/Care/Admission/RoomChoice.cshtml", new RoomChoice(HandlerPolicy.Overlap, TheWards(), null));

    [HttpGet(CareEndpoints.RoomChoiceRooms)]
    public async Task<IActionResult> Rooms(RoomChoiceQuery query)
    {
        await World.Reach(GateName.Of(LanesGate.Respond), Request.Path);
        return PartialView("/Care/Admission/RoomChoiceRooms.cshtml", new RoomChoiceRooms(RoomsOf(query.Ward)));
    }

    private WorldData World => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data;

    private static List<SelectListItem> TheWards() => [new SelectListItem("East", "East"), new SelectListItem("West", "West")];

    private static IReadOnlyList<string> RoomsOf(string? ward) => ward switch
    {
        "East" => ["E1", "E2"],
        "West" => ["W1", "W2", "W3"],
        _ => [],
    };
}

public sealed record RoomChoice(HandlerPolicy Policy, IReadOnlyList<SelectListItem> Wards, string? Ward);

public sealed class RoomChoiceQuery
{
    public string? Ward { get; set; }
}

public sealed record RoomChoiceRooms(IReadOnlyList<string> Rooms);
