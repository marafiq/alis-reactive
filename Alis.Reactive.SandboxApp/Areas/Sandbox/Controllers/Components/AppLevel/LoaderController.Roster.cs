using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.AppLevel
{
    /// <summary>
    /// Morning roster: checking a resident in and refreshing the roster each answer with the roster's
    /// rows, drawn by the server, which replace the rows the loader was covering.
    /// </summary>
    public partial class LoaderController
    {
        private const string RosterRowsView = "~/Areas/Sandbox/Views/Components/AppLevel/Loader/_RosterRows.cshtml";

        private static readonly IReadOnlyList<(int Id, string Name)> MorningRoster =
        [
            (1, "Margaret Hill"),
            (2, "Harold Benson"),
        ];

        [HttpGet("Roster")]
        public IActionResult Roster() =>
            View("~/Areas/Sandbox/Views/Components/AppLevel/Loader/Roster.cshtml",
                new ResidentRosterPageModel { Roster = RosterWithCheckedIn() });

        [HttpPost("Roster/CheckIn")]
        public IActionResult RosterCheckIn(int residentId) =>
            PartialView(RosterRowsView, RosterWithCheckedIn(residentId));

        [HttpGet("Roster/Rows")]
        public IActionResult RosterRows() => PartialView(RosterRowsView, RosterWithCheckedIn());

        [HttpGet("Roster/CareSummary")]
        public IActionResult RosterCareSummary() =>
            Ok(new RosterCareSummary("Margaret Hill: two medications due at noon."));

        private static ResidentRoster RosterWithCheckedIn(params int[] checkedInIds) =>
            new(MorningRoster
                .Select(resident => new RosterResident(resident.Id, resident.Name, checkedInIds.Contains(resident.Id)))
                .ToList());
    }
}
