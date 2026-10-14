using System.Text.Encodings.Web;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.AIAssistView;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>Margaret Hale's care-plan assistant: the care team's records answer each question.</summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/AIAssistView")]
    public class AIAssistViewController : Controller
    {
        private const string ChargeNurse = "Dana Ruiz, charge nurse on Maple";

        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/AIAssistView/Index.cshtml",
                new AIAssistViewModel
                {
                    ResidentName = "Margaret Hale",
                    Room = "Maple 214",
                    CareLevel = "Assisted living",
                    SavedNotes =
                    [
                        "Offer fluids every two hours while Margaret is awake.",
                        "Physical therapy is Tuesday and Thursday at 10 a.m.",
                        "Call her daughter Ellen after any fall, day or night."
                    ],
                    NightHandoff = new HandoffNote
                    {
                        Question = "What did the night shift hand off?",
                        Answer = "Priya Shah, night nurse: Margaret woke at 2 a.m. asking for Ellen and settled after warm milk. She drank two glasses of water overnight."
                    }
                });
        }

        // The answer comes from the care team's records; it ends by naming the common questions not asked.
        [HttpPost("Answer")]
        public IActionResult Answer([FromBody] AssistantQuestion question)
        {
            var prompt = question.Prompt ?? "";
            return Ok(new AssistantAnswer { Answer = FromCareRecords(prompt) + OtherQuestions(prompt, question.Suggestions) });
        }

        // The assistant stops preparing the answer; the coordinator counts questions from one.
        [HttpPost("StopAnswer")]
        public IActionResult StopAnswer([FromBody] StoppedAnswer stopped)
        {
            return Ok(new AssistantStatus
            {
                Summary = $"Stopped the answer to question {stopped.QuestionIndex + 1}. Edit it below and send it again."
            });
        }

        [HttpPost("AskNurse")]
        public IActionResult AskNurse([FromBody] NurseQuestion question)
        {
            if (string.IsNullOrWhiteSpace(question.Question))
                return BadRequest(new AssistantStatus { Summary = "Type the question for the charge nurse first." });

            return Ok(new AssistantStatus { Summary = $"Sent to {ChargeNurse}: “{question.Question}”" });
        }

        private static string FromCareRecords(string prompt)
        {
            var asked = prompt.ToLowerInvariant();
            if (asked.Contains("follow-up"))
                return "Margaret's follow-ups this week: a hydration check-in on Wednesday, her physical therapy re-evaluation on Thursday and a podiatry visit on Friday.";
            if (asked.Contains("podiatry"))
                return "Margaret's podiatry visit is Friday at 2 p.m. with Dr. Lin.";
            if (asked.Contains("fall"))
                return "Margaret's fall-risk interventions: a bed alarm at night, non-slip socks and her walker within reach.";
            if (asked.Contains("family"))
                return "Draft for Ellen Hale: your mother had a good week. She walked to lunch every day and joined Thursday's music hour.";
            if (asked.Contains("overnight"))
                return "Margaret drank two glasses of water overnight; the night nurse asks that fluids be offered again at breakfast.";
            return $"The care team has no notes on “{HtmlEncoder.Default.Encode(prompt)}” yet.";
        }

        // "You can also ask “A”, “B” or “C”." The answer is shown as HTML, so the questions are encoded.
        private static string OtherQuestions(string prompt, IEnumerable<string> suggestions)
        {
            var others = suggestions
                .Where(suggestion => suggestion != prompt)
                .Select(suggestion => $"“{HtmlEncoder.Default.Encode(suggestion)}”")
                .ToList();
            if (others.Count == 0)
                return "";

            var named = others.Count == 1
                ? others[0]
                : string.Join(", ", others.Take(others.Count - 1)) + " or " + others[^1];
            return $" You can also ask {named}.";
        }
    }
}
