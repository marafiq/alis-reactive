using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// Monthly census: the business office reviews a closed month's census and downloads it for the state
    /// report and the month's billing. September and October are closed; November is still open and cannot
    /// be reported. Nothing is kept between requests.
    /// </summary>
    public partial class GridController
    {
        [HttpGet("RosterExport"), ActionName("RosterExport")]
        public IActionResult MonthlyCensusPage() =>
            View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/RosterExport.cshtml",
                new RosterExportModel { Month = "2026-10" });

        [HttpGet("RosterExportCensus")]
        public IActionResult RosterExportCensus([FromQuery] MonthlyCensusQuery query) =>
            query.Month switch
            {
                "2026-09" => Ok(MonthlyCensusOf("September 2026", SeptemberCensus())),
                "2026-10" => Ok(MonthlyCensusOf("October 2026", OctoberCensus())),
                "2026-11" => Conflict(new MonthlyCensus { Summary = "November's census is still open; it closes on November 30." }),
                _ => BadRequest(new MonthlyCensus { Summary = "Choose a month to report." })
            };

        private static MonthlyCensus MonthlyCensusOf(string month, List<MonthlyCensusResident> residents) => new()
        {
            Residents = residents,
            Summary = $"{month}: {residents.Count} residents, {residents.Sum(resident => resident.CensusDays)} census days."
        };

        // Samuel Ortiz moved out on September 14.
        private static List<MonthlyCensusResident> SeptemberCensus() => new()
        {
            new MonthlyCensusResident { ResidentId = 7401, ResidentName = "Dorothy Fleming", Suite = "201", CareLevel = "Assisted Living", CensusDays = 30 },
            new MonthlyCensusResident { ResidentId = 7402, ResidentName = "Harold Chen", Suite = "204", CareLevel = "Memory Care", CensusDays = 30 },
            new MonthlyCensusResident { ResidentId = 7403, ResidentName = "Lucia Romero", Suite = "207", CareLevel = "Independent", CensusDays = 30 },
            new MonthlyCensusResident { ResidentId = 7404, ResidentName = "Margaret Hale", Suite = "210", CareLevel = "Assisted Living", CensusDays = 30 },
            new MonthlyCensusResident { ResidentId = 7405, ResidentName = "Samuel Ortiz", Suite = "213", CareLevel = "Skilled Nursing", CensusDays = 14 },
            new MonthlyCensusResident { ResidentId = 7406, ResidentName = "Vera Lindqvist", Suite = "216", CareLevel = "Memory Care", CensusDays = 30 }
        };

        // Theodore Banks moved into Samuel Ortiz's suite on October 12.
        private static List<MonthlyCensusResident> OctoberCensus() => new()
        {
            new MonthlyCensusResident { ResidentId = 7401, ResidentName = "Dorothy Fleming", Suite = "201", CareLevel = "Assisted Living", CensusDays = 31 },
            new MonthlyCensusResident { ResidentId = 7402, ResidentName = "Harold Chen", Suite = "204", CareLevel = "Memory Care", CensusDays = 31 },
            new MonthlyCensusResident { ResidentId = 7403, ResidentName = "Lucia Romero", Suite = "207", CareLevel = "Independent", CensusDays = 31 },
            new MonthlyCensusResident { ResidentId = 7404, ResidentName = "Margaret Hale", Suite = "210", CareLevel = "Assisted Living", CensusDays = 31 },
            new MonthlyCensusResident { ResidentId = 7407, ResidentName = "Theodore Banks", Suite = "213", CareLevel = "Skilled Nursing", CensusDays = 20 },
            new MonthlyCensusResident { ResidentId = 7406, ResidentName = "Vera Lindqvist", Suite = "216", CareLevel = "Memory Care", CensusDays = 31 }
        };
    }
}
