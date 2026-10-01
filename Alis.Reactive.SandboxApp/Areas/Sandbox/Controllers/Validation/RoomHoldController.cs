using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Validation
{
    /// <summary>
    /// Room holds: the coordinator holds a room for a prospective resident. The server refuses a room
    /// another family already holds; that refusal belongs to no field the coordinator typed in.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Validation/RoomHold")]
    public class RoomHoldController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/Validation/RoomHold/Index.cshtml",
                new RoomHoldModel { RoomCode = RoomHoldValidator.HeldRoom });

        [HttpPost("Hold")]
        public IActionResult Hold([FromBody] RoomHoldModel? hold)
        {
            if (hold == null) return BadRequest();

            var result = new RoomHoldValidator().Validate(hold);
            if (!result.IsValid)
            {
                var errors = result.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());
                return BadRequest(new { errors });
            }

            return Ok(new RoomHoldConfirmation($"{hold.RoomCode} is held for {hold.ProspectName}."));
        }
    }
}
