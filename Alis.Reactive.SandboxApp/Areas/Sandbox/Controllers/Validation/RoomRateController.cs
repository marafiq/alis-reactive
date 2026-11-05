using System.Globalization;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Validation
{
    [Area("Sandbox")]
    [Route("Sandbox/Validation/RoomRate")]
    public class RoomRateController : Controller
    {
        private readonly RoomRateValidator _validator;

        public RoomRateController(RoomRateValidator validator)
        {
            _validator = validator;
        }

        [HttpGet("")]
        public IActionResult Index()
            => View("~/Areas/Sandbox/Views/Validation/RoomRate/Index.cshtml", new RoomRateModel());

        [HttpPost("Add")]
        public IActionResult Add([FromBody] RoomRateModel? room)
        {
            if (room == null) return BadRequest();

            var result = _validator.Validate(room);
            if (!result.IsValid)
                return BadRequest(new { errors = result.ErrorsByField });

            var rate = room.MonthlyRate.ToString("#,##0.00", CultureInfo.InvariantCulture);
            var communityFee = room.CommunityFee!.Value;
            var fee = communityFee == 0m
                ? "community fee waived"
                : $"community fee ${communityFee.ToString("#,##0.00", CultureInfo.InvariantCulture)}";
            return Ok(new RoomRateConfirmation($"{room.RoomName} added to the rate sheet at ${rate} a month, {fee}."));
        }
    }
}
