using System.Globalization;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Native
{
    /// <summary>
    /// Care level change request: the server decides per resident whether a physician must sign
    /// off, renders that decision into a hidden field, and confirms what the submitted form said.
    /// </summary>
    public partial class NativeHiddenFieldController
    {
        private static readonly IReadOnlyDictionary<int, bool> PhysicianSignOffByResident = new Dictionary<int, bool>
        {
            [1042] = false,
            [1043] = true,
        };

        private static readonly string[] CareLevels = { "Independent", "Assisted Living", "Memory Care" };

        [HttpGet("CareLevelChange")]
        public IActionResult CareLevelChange(int residentId = 1042)
        {
            if (!PhysicianSignOffByResident.TryGetValue(residentId, out var requiresSignOff)) return NotFound();

            ViewBag.CareLevels = CareLevels.Select(level => new SelectListItem(level, level)).ToList();
            var request = new CareLevelChangeModel { ResidentId = residentId, RequiresPhysicianSignOff = requiresSignOff };
            return View("~/Areas/Sandbox/Views/Components/Native/NativeHiddenField/CareLevelChange.cshtml", request);
        }

        [HttpPost("CareLevelChange/Submit")]
        public IActionResult SubmitCareLevelChange([FromBody] CareLevelChangeModel? request)
        {
            if (request == null) return BadRequest();

            var signOff = request.RequiresPhysicianSignOff ? "physician sign-off required" : "no physician sign-off needed";
            var resident = request.ResidentId.ToString(CultureInfo.InvariantCulture);
            return Ok(new CareLevelChangeConfirmation(
                $"Change to {request.RequestedCareLevel} requested for resident #{resident}; {signOff}."));
        }
    }
}
