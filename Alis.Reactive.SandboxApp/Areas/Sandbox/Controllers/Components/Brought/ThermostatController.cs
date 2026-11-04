using System.Collections.Generic;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought;
using Alis.Reactive.SandboxApp.BroughtComponents;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Brought
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/Thermostat")]
    public class ThermostatController : Controller
    {
        private const string ViewBase = "~/Areas/Sandbox/Views/Components/Brought/Thermostat/";

        [HttpGet("")]
        public IActionResult Index() => View(ViewBase + "Index.cshtml", new RoomComfortModel());

        [HttpGet("Policy")]
        public IActionResult Policy() => Ok(new ComfortPolicy { Min = 64, Max = 72, Night = 66 });

        [HttpGet("NightPlan")]
        public IActionResult NightPlan() => Ok(new ComfortPlan
        {
            Target = 66,
            Mode = ThermostatMode.Heat,
            Nights = new List<NightSetting>
            {
                new NightSetting { Day = "Weeknights", Target = 66 },
                new NightSetting { Day = "Weekends", Target = 67 },
            },
        });

        [HttpPost("Save")]
        public IActionResult Save([FromBody] RoomComfortModel? comfort)
        {
            if (comfort == null) return BadRequest();
            return Ok(new RoomComfortSaved { Confirmation = $"Margaret's room set to {comfort.Target}°F." });
        }
    }
}
