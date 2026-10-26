using System.Globalization;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought;
using Alis.Reactive.SandboxApp.BroughtComponents;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Brought
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/IntakeSheet")]
    public class IntakeSheetController : Controller
    {
        private const string ViewBase = "~/Areas/Sandbox/Views/Components/Brought/IntakeSheet/";

        [HttpGet("")]
        public IActionResult Index() => View(ViewBase + "Index.cshtml", new MoveInNoticeModel());

        // The waiting list's record of a resident, for the wing the query names: Margaret Hale is second
        // on it, Harold Benson fifth.
        [HttpGet("WaitingList/{resident}")]
        public IActionResult WaitingList(string resident, [FromQuery] string wing)
        {
            var entry = resident == "harold"
                ? new WaitingListEntry
                {
                    Wing = $"{wing} wing",
                    Position = 5,
                    Applicant = new Applicant
                    {
                        Name = "Harold Benson",
                        Age = 71,
                        MoveInDate = new DateTime(2026, 7, 15),
                        Veteran = false,
                        CareLevel = CareLevel.MemoryCare,
                        MonthlyRate = 6200.00m,
                        RoomPreference = "Maple 118",
                        PetCount = 0,
                        PriorStayEnded = new DateTime(2025, 12, 31),
                        FluShot = true,
                        Diet = Diet.Pureed,
                        Contact = new ApplicantContact { Phone = "555-0142", Relation = "Son" },
                        Allergies = ["Latex", "Codeine"],
                    },
                }
                : new WaitingListEntry
                {
                    Wing = $"{wing} wing",
                    Position = 2,
                    Applicant = new MoveInNoticeModel().Applicant,
                };
            return Ok(entry);
        }

        // The desk's receipt reads back everything that reached it: the address, each header, and the body.
        [HttpPost("Notice/{wing}/{desk}/{applicant}/{age:int}/{phone}/{days:int}/{position:int}/{relation}")]
        public IActionResult Notice(
            string wing, string desk, string applicant, int age, string phone, int days, int position, string relation,
            [FromBody] NoticeRequest? body)
        {
            if (body == null) return BadRequest();

            string Header(string name) => Request.Headers[name].ToString();
            var headers = string.Join(" · ",
                Header("X-Wing"), Header("X-Applicant"), Header("X-Phone"), Header("X-Summary"), Header("X-Position"),
                Header("X-Source"), Header("X-Notice"), Header("X-Relation"), Header("X-Code"), Header("X-Years"));
            var received = string.Join(" · ",
                body.Name, body.ContactPhone, body.Summary,
                body.ListPosition?.ToString(CultureInfo.InvariantCulture) ?? "none",
                body.WingName, body.Source, body.Notice, body.ContactRelation,
                string.Join("+", body.Allergies), string.Join("+", body.Services), body.Code,
                body.CareLevel.ToString(), body.MoveInDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                body.Veteran ? "veteran" : "civilian",
                $"{body.Facts.Relation} {body.Facts.Phone}",
                body.Years.ToString(CultureInfo.InvariantCulture));

            return Ok(new NoticeReceipt
            {
                Route = $"{wing}/{desk}/{applicant}/{age}/{phone}/{days}/{position}/{relation}",
                Headers = headers,
                Body = received,
            });
        }
    }
}
