using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class AssignedDosesController : Controller
{
    private const string JsonText = "application/json";

    [HttpGet(CarePages.AssignedDosesWithAnUnprioritisedDose)]
    public IActionResult WithAnUnprioritisedDose() => View("/Care/Residents/AssignedDoses.cshtml", new AssignedDoses("Morning", AssignedDosesEndpoints.Doses));

    [HttpGet(CarePages.AssignedDosesWithEveryDosePrioritised)]
    public IActionResult WithEveryDosePrioritised() => View("/Care/Residents/AssignedDoses.cshtml", new AssignedDoses("Evening", AssignedDosesEndpoints.DosesAllPrioritised));

    [HttpGet(AssignedDosesEndpoints.Doses)]
    public IActionResult Doses() => Content(
        """[{"name":"A","priority":2,"carer":{"name":"Ada"},"ward":"East"},{"name":"B","priority":null,"carer":null,"ward":"West"},{"name":"C","priority":3,"carer":{"name":"Ben"}}]""",
        JsonText);

    [HttpGet(AssignedDosesEndpoints.DosesAllPrioritised)]
    public IActionResult DosesAllPrioritised() => Content(
        """[{"name":"A","priority":2,"carer":{"name":"Ada"},"ward":"East"},{"name":"B","priority":4,"carer":null,"ward":"West"},{"name":"C","priority":3,"carer":{"name":"Ben"}}]""",
        JsonText);
}

public sealed record AssignedDoses(string Round, string Doses);

public sealed class AssignedDose
{
    public string Name { get; set; } = "";

    public int? Priority { get; set; }

    public AssignedCarer? Carer { get; set; }

    public string? Ward { get; set; }
}

public sealed class AssignedCarer
{
    public string Name { get; set; } = "";
}

public static class AssignedDosesEndpoints
{
    private const string Root = CarePages.Area + "residents/assigned-doses/";

    public const string Doses = Root + "doses";

    public const string DosesAllPrioritised = Root + "doses/all-prioritised";
}
