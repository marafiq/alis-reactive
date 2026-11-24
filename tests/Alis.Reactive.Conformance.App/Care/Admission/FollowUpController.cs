using System.Globalization;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class FollowUpController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.FollowUp)]
    public IActionResult FollowUp() => View("/Care/Admission/FollowUp.cshtml", new FollowUp { Due = new DateTime(2026, 12, 1) });

    [HttpPost(FollowUpEndpoints.FollowUps)]
    public IActionResult Save([FromForm] FollowUp followUp) => Answer(followUp);

    [HttpPost(FollowUpEndpoints.Referrals)]
    public IActionResult Refer([FromBody] FollowUp followUp) => Answer(followUp);

    private ContentResult Answer(FollowUp followUp) => Content($"{followUp.Reason}: due {followUp.Due.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)}", PlainText);
}

public sealed class FollowUp
{
    public string? Reason { get; set; }

    public DateTime Due { get; set; }
}

public static class FollowUpEndpoints
{
    public const string FollowUps = CarePages.FollowUp + "/follow-ups";

    public const string Referrals = CarePages.FollowUp + "/referrals";
}
