using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// Evening rounds: the nurse on East wing picks a resident on the board and records a task done or a fall,
    /// or brings in what the chart says now; each answer names the resident by key. Every page starts from the
    /// evening's board, and an answer works from the resident the nurse sent, keeping nothing.
    /// </summary>
    public partial class GridController
    {
        [HttpGet("KeyedUpdate"), ActionName("KeyedUpdate")]
        public IActionResult EveningRoundsPage() =>
            View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/KeyedUpdate.cshtml",
                new KeyedUpdateModel { Nurse = "Dana Cole, RN", Wing = "East" });

        [HttpGet("KeyedUpdateBoard")]
        public IActionResult KeyedUpdateBoard() => Ok(EveningRoundsBoard());

        [HttpPost("KeyedUpdateTaskDone")]
        public IActionResult KeyedUpdateTaskDone([FromBody] EveningRoundsRequest? request)
        {
            var picked = request?.Residents?.FirstOrDefault();
            if (picked == null)
                return BadRequest(EveningRoundsNobodyPicked());
            if (picked.OpenTasks == 0)
                return BadRequest(new EveningRoundsRefusal { Summary = $"{picked.ResidentName} has no open tasks left." });

            var left = picked.OpenTasks - 1;
            return Ok(new EveningRoundsTaskCount
            {
                ResidentId = picked.ResidentId,
                OpenTasks = left,
                Summary = $"{picked.ResidentName} has {left} open {(left == 1 ? "task" : "tasks")} left."
            });
        }

        [HttpPost("KeyedUpdateFall")]
        public IActionResult KeyedUpdateFall([FromBody] EveningRoundsRequest? request)
        {
            var picked = request?.Residents?.FirstOrDefault();
            if (picked == null)
                return BadRequest(EveningRoundsNobodyPicked());

            return Ok(new EveningRoundsFallRisk
            {
                ResidentId = picked.ResidentId,
                FallRisk = "High",
                Summary = $"Fall recorded for {picked.ResidentName}; fall risk is now High."
            });
        }

        [HttpPost("KeyedUpdateChart")]
        public IActionResult KeyedUpdateChart([FromBody] EveningRoundsRequest? request)
        {
            var picked = request?.Residents?.FirstOrDefault();
            if (picked == null)
                return BadRequest(EveningRoundsNobodyPicked());

            // Ruth Adler's room lost its heating this afternoon; she moved to E-214, therapy raised her fall
            // risk and wound care added orders. Nobody else's chart changed since the board was printed.
            return Ok(picked.ResidentId == 7303
                ? new EveningRoundsChart
                {
                    Resident = new EveningRoundsResident { ResidentId = 7303, ResidentName = "Ruth Adler", Room = "E-214", FallRisk = "High", OpenTasks = 3 },
                    Summary = "Ruth Adler's chart: moved to E-214 this afternoon, High fall risk, 3 open tasks."
                }
                : new EveningRoundsChart { Resident = picked, Summary = $"Nothing new on {picked.ResidentName}'s chart." });
        }

        private static EveningRoundsRefusal EveningRoundsNobodyPicked() =>
            new() { Summary = "Pick a resident on the board first." };

        private static List<EveningRoundsResident> EveningRoundsBoard() => new()
        {
            new EveningRoundsResident { ResidentId = 7301, ResidentName = "Martha Collins", Room = "E-201", FallRisk = "Low", OpenTasks = 2 },
            new EveningRoundsResident { ResidentId = 7302, ResidentName = "Joseph Rivera", Room = "E-203", FallRisk = "Moderate", OpenTasks = 3 },
            new EveningRoundsResident { ResidentId = 7303, ResidentName = "Ruth Adler", Room = "E-205", FallRisk = "Moderate", OpenTasks = 1 },
            new EveningRoundsResident { ResidentId = 7304, ResidentName = "Victor Nguyen", Room = "E-207", FallRisk = "Low", OpenTasks = 0 },
            new EveningRoundsResident { ResidentId = 7305, ResidentName = "Lillian Brooks", Room = "E-209", FallRisk = "High", OpenTasks = 4 }
        };
    }
}
