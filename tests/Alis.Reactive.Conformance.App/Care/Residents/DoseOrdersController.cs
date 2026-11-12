using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class DoseOrdersController : Controller
{
    [HttpGet(CarePages.DoseOrdersWithAnUnprioritisedDose)]
    public IActionResult WithAnUnprioritisedDose() => View("/Care/Residents/DoseOrders.cshtml", new DoseOrders("Morning", DoseOrdersEndpoints.Doses));

    [HttpGet(CarePages.DoseOrdersWithEveryDosePrioritised)]
    public IActionResult WithEveryDosePrioritised() => View("/Care/Residents/DoseOrders.cshtml", new DoseOrders("Evening", DoseOrdersEndpoints.DosesAllPrioritised));

    [HttpGet(DoseOrdersEndpoints.Doses)]
    public IActionResult Doses() => Json(TheDoses(null));

    [HttpGet(DoseOrdersEndpoints.DosesAllPrioritised)]
    public IActionResult DosesAllPrioritised() => Json(TheDoses(3));

    private static List<OrderedDose> TheDoses(int? priorityOfD) => new List<OrderedDose>
    {
        new("A", 5, 2, false, "b"),
        new("B", 1, 4, true, "B"),
        new("C", 5, 1, true, "a"),
        new("D", 3, priorityOfD, false, "A"),
    };
}

public sealed record DoseOrders(string Round, string Doses);

public sealed record OrderedDose(string Name, int Milligrams, int? Priority, bool Urgent, string Code);

public static class DoseOrdersEndpoints
{
    private const string Root = CarePages.Area + "residents/dose-orders/";

    public const string Doses = Root + "doses";

    public const string DosesAllPrioritised = Root + "doses/all-prioritised";
}
