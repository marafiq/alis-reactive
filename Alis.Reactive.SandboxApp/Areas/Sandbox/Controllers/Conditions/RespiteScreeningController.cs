using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.RespiteScreening;
using Alis.Reactive.SandboxApp.BroughtComponents;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Conditions;

[Area("Sandbox")]
[Route("Sandbox/Conditions/RespiteScreening")]
public class RespiteScreeningController : Controller
{
    [HttpGet("")]
    public IActionResult Index() =>
        View("~/Areas/Sandbox/Views/Conditions/RespiteScreening/Index.cshtml", new RespiteScreeningModel());

    // The Garden wing expects Margaret Hale: 62 or over, from its opening day, assisted living at $5,000;
    // it offers the garden-view room and allows one pet. Its records: a veteran, no previous stay, this
    // season's flu shot given, a diabetic diet.
    [HttpGet("Policy")]
    public IActionResult Policy() => Ok(new WingPolicy
    {
        ExpectedResident = "Margaret Hale",
        MinimumAge = 62,
        OpensOn = new DateTime(2026, 7, 1),
        VeteranOnRecord = true,
        CareLevel = CareLevel.Assisted,
        MonthlyRate = 5000.00m,
        RoomOffered = "Garden view",
        PetLimit = 1,
        LastStayOnRecord = null,
        FluShotOnRecord = true,
        DietOnRecord = Diet.Diabetic,
    });
}
