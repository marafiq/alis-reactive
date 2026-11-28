using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class BadgeDeskController : Controller
{
    private const string PlainText = "text/plain";
    private const string Markup = "text/html";
    private const string TheResident = "Ada Hart";

    [HttpGet(CarePages.BadgeDesk)]
    public IActionResult ForAResident() => View("/Care/Admission/BadgeDesk.cshtml", new BadgeIntake(TheResident, false));

    [HttpPost(BadgeDeskEndpoints.Admissions)]
    public IActionResult Admit([FromForm] BadgeAdmission admission) => Content($"Admitted {admission.Resident}", PlainText);

    [HttpGet(CareEndpoints.Badge)]
    public IActionResult Badge() => Content($"<p>Badge for {TheResident}</p>", Markup);
}

public sealed record BadgeIntake(string Resident, bool PrintBadge);

public sealed class BadgeAdmission
{
    public string Resident { get; set; } = "";
}

public static class BadgeDeskEndpoints
{
    public const string Admissions = CarePages.Area + "admission/badge-desk/admissions";
}

public static class BadgeDesks
{
    public static readonly AppEvent Admit = AppEvent.Declare();
}
