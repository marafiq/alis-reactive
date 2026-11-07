using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Poc.App.Visits;

public sealed class VisitsController : Controller
{
    public IActionResult Planner() => View(new VisitsPage());

    public IActionResult PlannedDays() => View(new VisitsPage());

    public IActionResult FamilyVisits() => View(new VisitsPage());
}

public sealed record VisitsPage;
