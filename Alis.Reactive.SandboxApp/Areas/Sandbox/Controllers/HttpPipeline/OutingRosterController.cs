using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.HttpPipeline
{
    /// <summary>
    /// Garden outing: four residents, three of whom use a mobility aid; nobody uses a mobility scooter. No
    /// aid chosen means every resident. The bus has two boarding-help spaces and books only aids it knows.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/HttpPipeline/OutingRoster")]
    public class OutingRosterController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/HttpPipeline/OutingRoster/Index.cshtml", new OutingRosterModel());

        [HttpGet("Residents")]
        public IActionResult Residents([FromQuery] string[]? mobilityAids)
        {
            var residents = new[]
            {
                (Name: "Helen Park", Aid: (string?)"WALKER"),
                (Name: "Arthur Bell", Aid: (string?)"WHEELCHAIR"),
                (Name: "Rosa Diaz", Aid: (string?)null),
                (Name: "Frank Moore", Aid: (string?)"CANE"),
            };
            var aidChosen = mobilityAids is { Length: > 0 };
            var joining = residents.Where(r => !aidChosen || mobilityAids!.Contains(r.Aid)).Select(r => r.Name).ToList();
            var listed = joining.Count == 0 ? "No resident uses the chosen aids." : string.Join(", ", joining);
            return Ok(new OutingRosterAnswer(listed));
        }

        [HttpPost("Book")]
        public IActionResult Book([FromForm] OutingRosterModel booking)
        {
            var aids = booking.MobilityAids ?? [];
            var unknownAids = aids.Where(code => AidName(code) is null).ToList();
            if (unknownAids.Count > 0)
                return BadRequest(new OutingBookingRefusal($"Unknown mobility aid: '{unknownAids[0]}'."));
            if (aids.Length > 2)
                return BadRequest(new OutingBookingRefusal("The bus has two boarding-help spaces: choose at most two aids."));

            var confirmation = aids.Length > 0
                ? $"Bus booked with boarding help for: {string.Join(", ", aids.Select(AidName))}."
                : "Bus booked. No boarding help needed.";
            return Ok(new OutingBookingAnswer(confirmation));
        }

        private static string? AidName(string? code) => code switch
        {
            "WALKER" => "Walker",
            "WHEELCHAIR" => "Wheelchair",
            "CANE" => "Cane",
            "SCOOTER" => "Mobility scooter",
            _ => null,
        };
    }
}
