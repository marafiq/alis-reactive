using System.Globalization;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.VitalsAlert;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Conditions;

/// <summary>
/// Walter Cho's heart rate check on the evening round. A reading is recorded at 72 bpm until the nurse
/// enters another. A critical reading pages the rapid response team; a high one alerts the charge nurse,
/// Dana Ortiz. Each answer names the heart rate it was sent.
/// </summary>
[Area("Sandbox")]
[Route("Sandbox/Conditions/VitalsAlert")]
public class VitalsAlertController : Controller
{
    [HttpGet("")]
    public IActionResult Index() =>
        View("~/Areas/Sandbox/Views/Conditions/VitalsAlert/Index.cshtml", new VitalsAlertModel { HeartRate = 72 });

    [HttpPost("RapidResponse")]
    public IActionResult RapidResponse([FromBody] HeartRateReading reading) =>
        Ok(new HeartRateAlert("Rapid response team paged",
            $"The rapid response team is on its way to room 118: Walter Cho's heart rate is {Bpm(reading)} bpm."));

    [HttpPost("ChargeNurse")]
    public IActionResult ChargeNurse([FromBody] HeartRateReading reading) =>
        Ok(new HeartRateAlert("Charge nurse alerted",
            $"Dana Ortiz will recheck Walter Cho within 15 minutes: his heart rate is {Bpm(reading)} bpm."));

    private static string Bpm(HeartRateReading reading) => reading.HeartRate.ToString("0", CultureInfo.InvariantCulture);
}
