using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/DateRangePicker")]
    public class DateRangePickerController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Fusion/DateRangePicker/Index.cshtml", new DateRangePickerModel());
        }

        /// <summary>Reserves the respite stay for the chosen stay period.</summary>
        [HttpPost("Reserve")]
        public IActionResult Reserve([FromBody] DateRangePickerModel? stay)
        {
            if (stay == null) return BadRequest();

            var result = new StayReservationValidator().Validate(stay);
            if (!result.IsValid)
                return BadRequest(new { errors = result.ErrorsByField });

            var period = stay.StayPeriod!;
            var nights = (period[period.Length - 1].Date - period[0].Date).Days;
            return Ok(new StayReservation($"Respite stay reserved for {nights} nights."));
        }
    }
}
