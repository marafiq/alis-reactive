using System.Globalization;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Validation
{
    /// <summary>
    /// Room rate sheet: the administrator names a room and sets its monthly rate. The confirmation
    /// repeats what the server received, so a room only reaches the sheet named and priced.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Validation/RoomRate")]
    public class RoomRateController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/Validation/RoomRate/Index.cshtml", new RoomRateModel());

        [HttpPost("Add")]
        public IActionResult Add([FromBody] RoomRateModel? room)
        {
            if (room == null) return BadRequest();

            var result = new RoomRateValidator().Validate(room);
            if (!result.IsValid)
            {
                var errors = result.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());
                return BadRequest(new { errors });
            }

            var rate = room.MonthlyRate.ToString("#,##0.00", CultureInfo.InvariantCulture);
            // The validator requires the community fee.
            var communityFee = room.CommunityFee!.Value;
            var fee = communityFee == 0m
                ? "community fee waived"
                : $"community fee ${communityFee.ToString("#,##0.00", CultureInfo.InvariantCulture)}";
            return Ok(new RoomRateConfirmation($"{room.RoomName} added to the rate sheet at ${rate} a month, {fee}."));
        }
    }
}
