using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Plugins.ArrayManager;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Plugins
{
    /// <summary>
    /// Ward desk: Birch ward's census read through the page's array plugin, the memory-care ward beside
    /// it, tonight's staffing asked for with the census, and a visitor pass preview.
    /// URL: /Sandbox/Plugins/ArrayManager.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Plugins/ArrayManager")]
    public class ArrayManagerController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/Plugins/ArrayManager/Index.cshtml", new WardDeskModel());

        /// <summary>
        /// Birch ward: five residents fill its five beds. Edith Morales, first on the list, is in hospital
        /// and Samuel Price on leave, so the round starts with Thomas Reed.
        /// </summary>
        [HttpGet("Residents")]
        public IActionResult Residents() => Ok(new WardCensusResponse
        {
            Ward = "Birch Ward",
            Residents = new object[]
            {
                new { name = "Edith Morales", status = "in hospital",  condition = "stable", careHours = 3 },
                new { name = "Thomas Reed",   status = "in residence", condition = "stable", careHours = 4 },
                new { name = "Dorothy Lane",  status = "in residence", condition = "stable", careHours = 6 },
                new { name = "Samuel Price",  status = "on leave",     condition = "stable", careHours = 2 },
                new { name = "Martha Cole",   status = "in residence", condition = "stable", careHours = 5 },
            },
        });

        /// <summary>The memory-care ward: three residents in its five beds, Walter Cobb critical.</summary>
        [HttpGet("MemoryWard")]
        public IActionResult MemoryWard() => Ok(new WardCensusResponse
        {
            Ward = "Memory-Care Ward",
            Residents = new object[]
            {
                new { name = "Edith Lane",  status = "in residence", condition = "stable",   careHours = 6 },
                new { name = "Walter Cobb", status = "in residence", condition = "critical", careHours = 8 },
                new { name = "Iris Moreno", status = "in hospital",  condition = "stable",   careHours = 4 },
            },
        });

        /// <summary>
        /// Plans tonight's staffing from the ward's census (the <c>X-Ward-Census</c> header) and the
        /// residents in residence (<c>?inResidence=</c>): an aide for every two residents in residence,
        /// and a bed held for each resident away. A request missing either is answered with what to send.
        /// </summary>
        [HttpGet("Staffing")]
        public IActionResult Staffing(
            [FromQuery] int? inResidence,
            [FromHeader(Name = "X-Ward-Census")] int? census)
        {
            if (inResidence is not int present || census is not int onCensus)
                return Ok(new WardStaffingPlan { Plan = "Send the ward's census and the residents in residence to plan tonight's staffing." });

            var aides = (present + 1) / 2;
            var held = onCensus - present;
            return Ok(new WardStaffingPlan
            {
                Plan = $"Tonight: {aides} aides for {present} residents in residence; {held} beds held for residents away.",
            });
        }
    }
}
