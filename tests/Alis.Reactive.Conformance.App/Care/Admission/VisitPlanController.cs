using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class VisitPlanController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.VisitPlan)]
    public IActionResult Plan() => View("/Care/Admission/VisitPlan.cshtml", new VisitPlan());

    [HttpGet(CarePages.OpenVisitPlan)]
    public IActionResult OpenPlan() => View("/Care/Admission/OpenVisitPlan.cshtml", new OpenVisitPlan());

    [HttpPost(VisitPlanEndpoints.Plans)]
    public IActionResult Save([FromForm] VisitPlan plan) => Content($"{plan.Resident}: {plan.VisitDays.Count} visit days", PlainText);

    [HttpPost(VisitPlanEndpoints.OpenPlans)]
    public IActionResult SaveOpen([FromForm] OpenVisitPlan plan) => Content($"{plan.Resident}: {plan.VisitDays.Count} visit days", PlainText);
}

public enum VisitDay
{
    Mon,
    Tue,
    Wed,
}

public sealed class VisitPlan
{
    public string? Resident { get; set; }

    public List<VisitDay> VisitDays { get; set; } = [];
}

public sealed class OpenVisitPlan
{
    public string? Resident { get; set; }

    public List<VisitDay?> VisitDays { get; set; } = [];
}

public static class VisitPlanEndpoints
{
    public const string Plans = CarePages.VisitPlan + "/plans";

    public const string OpenPlans = CarePages.OpenVisitPlan + "/plans";
}
