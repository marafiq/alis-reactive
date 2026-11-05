using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Validation
{
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
        public IActionResult Index()
            => View("~/Areas/Sandbox/Views/Validation/RoomHold/Index.cshtml", new RoomHoldModel());

        [HttpPost("Hold")]
        public async Task<IActionResult> Hold([FromBody] RoomHoldModel? hold)
        {
            if (hold == null) return BadRequest();

            var rules = new InlineValidator<RoomHoldModel>();
            rules.Include(_holdRules);
            rules.Include(_reservationsOffice);
            var result = await rules.ValidateAsync(hold);
            if (!result.IsValid) return BadRequest(new { errors = result.ErrorsByField });

            return Ok(new RoomHoldConfirmation($"{hold.RoomCode} is held for {hold.ProspectName}."));
        }
    }
}
