using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RoundStaffController : Controller
{
    [HttpGet(CarePages.RoundStaffWithAChargeNurse)]
    public IActionResult WithAChargeNurse() => View("/Care/Round/RoundStaff.cshtml", new RoundStaff("Morning", "Charge nurse"));

    [HttpGet(CarePages.RoundStaffWithAStaffNurse)]
    public IActionResult WithAStaffNurse() => View("/Care/Round/RoundStaff.cshtml", new RoundStaff("Morning", "Staff nurse"));
}

public sealed record RoundStaff(string Round, string RoleOfAda);

public static class RoundStaffEvents
{
    public static readonly AppEvent Show = AppEvent.Declare();
}
