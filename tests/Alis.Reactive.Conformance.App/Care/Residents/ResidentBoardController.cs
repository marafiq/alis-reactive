using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ResidentBoardController : Controller
{
    [HttpGet(CarePages.ResidentBoard)]
    public IActionResult Board() => View("/Care/Residents/ResidentBoard.cshtml", new ResidentBoard("North wing"));

    [HttpGet(ResidentBoardEndpoints.Residents)]
    public IActionResult Residents() => NoContent();

    [HttpGet(ResidentBoardEndpoints.Staff)]
    public IActionResult Staff() => NoContent();

    [HttpGet(CareEndpoints.ResidentBoardAllergies)]
    public async Task<IActionResult> Allergies()
    {
        await HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data.Reach(GateName.Of(LanesGate.Settle), Request.Path);
        return NoContent();
    }
}

public sealed record ResidentBoard(string Wing);

public static class ResidentBoardEvents
{
    public static readonly AppEvent Load = AppEvent.Declare();

    public static readonly AppEvent LoadWithoutAllergies = AppEvent.Declare();
}

public static class ResidentBoardEndpoints
{
    private const string Root = CarePages.Area + "residents/board/";

    public const string Residents = Root + "residents";

    public const string Staff = Root + "staff";
}
