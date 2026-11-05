using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.AppLevel
{
    public partial class LoaderController
    {
        private const string RosterRowsView = "~/Areas/Sandbox/Views/Components/AppLevel/Loader/_RosterRows.cshtml";

        private static readonly IReadOnlyList<(int Id, string Name)> MorningResidents =
        [
            (1, "Margaret Hill"),
            (2, "Harold Benson"),
        ];

        [HttpGet("Roster")]
        public IActionResult Roster()
            => View("~/Areas/Sandbox/Views/Components/AppLevel/Loader/Roster.cshtml",
                new ResidentRosterPageModel { Roster = RosterWithCheckedIn() });

        [HttpPost("Roster/CheckIn/{residentId}")]
        public IActionResult RosterCheckIn(int residentId) => PartialView(RosterRowsView, RosterWithCheckedIn(residentId));

        [HttpGet("Roster/Rows")]
        public IActionResult RosterRows() => PartialView(RosterRowsView, RosterWithCheckedIn());

        [HttpGet("Roster/CareSummary")]
        public IActionResult RosterCareSummary() => Ok(new RosterCareSummary("Margaret Hill: two medications due at noon."));

        private static MorningRosterRows RosterWithCheckedIn(params int[] checkedInIds)
            => new(MorningResidents
                .Select(resident => new MorningRosterResident(resident.Id, resident.Name, checkedInIds.Contains(resident.Id)))
                .ToList());
    }
}
