using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Menu;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// The staff navigation on the nurses' station tablet. The med tech is signed in unless the tablet was
    /// switched to the business office; each page the navigation opens answers with what it says tonight.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/FusionMenu")]
    public class MenuController : Controller
    {
        // What each page says tonight, by the id of the menu item that opens it.
        private static readonly IReadOnlyDictionary<string, string> PageSummaries = new Dictionary<string, string>
        {
            ["nav-census"] = "42 residents are on the census tonight; 3 are out on leave.",
            ["nav-room-board"] = "Maple wing has 2 rooms open tonight.",
            ["nav-care-notes"] = "5 care notes were written this shift.",
            ["nav-med-pass"] = "Evening med pass: 18 of 24 residents done.",
            ["nav-prn-log"] = "2 as-needed doses were given this shift.",
            ["nav-invoices"] = "October invoices go out on the 1st.",
            ["nav-payments"] = "4 payments were posted today.",
            ["nav-help"] = "For help with the tablet, call the nurse manager at ext. 4410."
        };

        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index([FromQuery] string? staff)
        {
            var businessOffice = staff == StaffMember.BusinessOffice.Key;
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Menu/Index.cshtml",
                new FusionMenuModel
                {
                    SignedIn = businessOffice ? StaffMember.BusinessOffice : StaffMember.MedTech,
                    SwitchTo = businessOffice ? StaffMember.MedTech : StaffMember.BusinessOffice
                });
        }

        [HttpGet("Page")]
        public IActionResult OpenPage([FromQuery] string? page) =>
            page != null && PageSummaries.TryGetValue(page, out var summary)
                ? Ok(new StaffPage { Summary = summary })
                : NotFound();
    }
}
