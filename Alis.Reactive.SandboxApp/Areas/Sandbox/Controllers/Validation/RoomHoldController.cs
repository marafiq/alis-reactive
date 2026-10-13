using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Validation
{
    /// <summary>
    /// Room holds: the coordinator holds a room for a prospective resident. The server refuses a room
    /// another family already holds; that refusal belongs to no field the coordinator typed in. The
    /// reservations office also refuses a prospect who already holds a room.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Validation/RoomHold")]
    public class RoomHoldController : Controller
    {
        private readonly RoomHoldValidator _holdRules;
        private readonly ReservationsOfficeValidator _reservationsOffice;

        public RoomHoldController(RoomHoldValidator holdRules, ReservationsOfficeValidator reservationsOffice)
        {
            _holdRules = holdRules;
            _reservationsOffice = reservationsOffice;
        }

        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/Validation/RoomHold/Index.cshtml", new RoomHoldModel());

        [HttpPost("Hold")]
        public async Task<IActionResult> Hold([FromBody] RoomHoldModel? hold)
        {
            if (hold == null) return BadRequest();

            // The availability rule asks the hold book asynchronously, so the server validates asynchronously;
            // the reservations office's rules are its own validator's, which only the server runs.
            var failures = (await _holdRules.ValidateAsync(hold)).Errors
                .Concat(_reservationsOffice.Validate(hold).Errors)
                .ToList();
            if (failures.Count > 0)
            {
                var errors = failures
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());
                return BadRequest(new { errors });
            }

            return Ok(new RoomHoldConfirmation($"{hold.RoomCode} is held for {hold.ProspectName}."));
        }
    }
}
