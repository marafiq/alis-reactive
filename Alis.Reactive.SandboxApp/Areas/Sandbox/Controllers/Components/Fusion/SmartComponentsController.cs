using System.Text.RegularExpressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.SmartComponents;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// Maple Grove's referral intake: today's inbox of referral letters, the save of a referral, and the
    /// answers an AI service would give Syncfusion's smart components. The sandbox reaches no AI service, so
    /// <see cref="Paste"/> reads the letter's labelled lines and <see cref="Suggest"/> completes two openings.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/SmartComponents")]
    public class SmartComponentsController : Controller
    {
        private const string LetterView = "~/Areas/Sandbox/Views/Components/Fusion/SmartComponents/_ReferralLetter.cshtml";

        private static readonly IReadOnlyList<IncomingReferral> Inbox =
        [
            new() { Id = "ref-3101", Summary = "St. Mary's Hospital: Margaret Hale" },
            new() { Id = "ref-3102", Summary = "Riverside Medical Group: Arthur Bell" }
        ];

        // Each inbox referral's letter, by the referral's ID.
        private static readonly IReadOnlyDictionary<string, string> Letters = new Dictionary<string, string>
        {
            ["ref-3101"] = "St. Mary's Hospital, Discharge Planning\n"
                + "Patient: Margaret Hale\n"
                + "Referring physician: Dr. Lena Ortiz\n"
                + "Level of care: Memory Care\n"
                + "Reason for referral: Confusion at night since her hip surgery; she needs a secure unit.\n",
            ["ref-3102"] = "Riverside Medical Group\n"
                + "Patient: Arthur Bell\n"
                + "Referring physician: Dr. Samuel Kim\n"
                + "Level of care: Assisted Living\n"
                + "Reason for referral: Lives alone and missed his medications twice this month.\n"
        };

        private static readonly IReadOnlyList<string> CareLevels = ["Independent Living", "Assisted Living", "Memory Care"];

        // The letter's label for each field of the referral form, by the field's name.
        private static readonly IReadOnlyDictionary<string, string> LetterLabels = new Dictionary<string, string>
        {
            ["ResidentName"] = "Patient",
            ["ReferredBy"] = "Referring physician",
            ["CareLevel"] = "Level of care",
            ["Reason"] = "Reason for referral"
        };

        // What the stand-in suggests after a note's last word.
        private static readonly IReadOnlyDictionary<string, string> Completions = new Dictionary<string, string>
        {
            ["family"] = " tour scheduled for Thursday",
            ["metformin"] = " 500 mg with breakfast"
        };

        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/SmartComponents/Index.cshtml",
                new SmartComponentsModel { Inbox = Inbox, CareLevels = CareLevels });
        }

        [HttpGet("Letter")]
        public IActionResult Letter([FromQuery] ReferralLetterQuery query) =>
            Letters.TryGetValue(query.ReferralId, out var letter) ? PartialView(LetterView, letter) : NotFound();

        [HttpPost("Save")]
        public IActionResult Save([FromBody] ReferralIntakeSave referral)
        {
            var words = referral.IntakeNote.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
            var note = words == 0 ? "no intake note" : $"a {words}-word intake note";
            return Ok(new ReferralIntakeSaved
            {
                Message = $"Saved {referral.ResidentName}'s referral for {referral.CareLevel} with {note}."
            });
        }

        // Smart Paste asks for each form field by name; the answer gives each a value from the letter.
        [HttpPost("Paste")]
        public IActionResult Paste([FromBody] SmartAssistRequest request)
        {
            var fields = Regex.Matches(Instructions(request), @"FIELD (\w+)\^\^\^").Select(match => match.Groups[1].Value);
            var letter = UserText(request);
            var lines = fields.Select(field => $"FIELD {field}^^^{FromLetter(letter, field)}");
            return Content(string.Join("\n", lines), "text/plain");
        }

        // The smart text area asks what the coordinator will type at the cursor (^^^).
        [HttpPost("Suggest")]
        public IActionResult Suggest([FromBody] SmartAssistRequest request)
        {
            var typed = UserText(request).Split("^^^")[0].TrimEnd();
            var lastWord = typed.Split(' ').Last().ToLowerInvariant();
            return Content(
                Completions.TryGetValue(lastWord, out var completion) ? $"OK:[{completion}]" : "NO_PREDICTION",
                "text/plain");
        }

        private static string Instructions(SmartAssistRequest request) =>
            request.Messages.FirstOrDefault(message => message.Role == "system")?.Content ?? string.Empty;

        // The last user message, after its USER_DATA: or USER_TEXT: label.
        private static string UserText(SmartAssistRequest request)
        {
            var content = request.Messages.LastOrDefault(message => message.Role == "user")?.Content ?? string.Empty;
            var label = Regex.Match(content, "USER_(DATA|TEXT): ");
            return label.Success ? content.Substring(label.Index + label.Length) : content;
        }

        private static string FromLetter(string letter, string field)
        {
            if (!LetterLabels.TryGetValue(field, out var label)) return "NO_DATA";
            var line = Regex.Match(letter, "^" + Regex.Escape(label) + @":\s*(.+)$", RegexOptions.Multiline);
            return line.Success ? line.Groups[1].Value.Trim() : "NO_DATA";
        }
    }
}
