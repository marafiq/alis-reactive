using System.Globalization;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>Respite stay: the server prices the days picked and reserves the stay.</summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/DateRangePicker")]
    public class DateRangePickerController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Fusion/DateRangePicker/Index.cshtml", new DateRangePickerModel());
        }

        /// <summary>Prices the stay: the nights between the day Eleanor arrives and the day she leaves.</summary>
        [HttpGet("Quote")]
        public IActionResult Quote([FromQuery] RespiteStayDates stay)
        {
            if (stay.Arrives is not { } arrives || stay.Leaves is not { } leaves) return BadRequest();

            return Ok(new RespiteQuote($"{Day(arrives)} to {Day(leaves)}: {Priced(arrives, leaves)}."));
        }

        /// <summary>Reserves the respite stay for the days picked.</summary>
        [HttpPost("Reserve")]
        public IActionResult Reserve([FromBody] RespiteStayDates? stay)
        {
            var period = stay is { Arrives: { } arrives, Leaves: { } leaves } ? new[] { arrives, leaves } : null;
            var result = new StayReservationValidator().Validate(new DateRangePickerModel { StayPeriod = period });
            if (!result.IsValid)
                return BadRequest(new { errors = result.ErrorsByField });

            return Ok(new StayReservation(
                $"Eleanor Voss is booked into Garden Suite 3 from {Day(period![0])} to {Day(period[1])}, {period[1].Year}: {Priced(period[0], period[1])}."));
        }

        private static string Day(DateTime day) => day.ToString("dddd, MMMM d", CultureInfo.InvariantCulture);

        private static string Priced(DateTime arrives, DateTime leaves)
        {
            var nights = (leaves.Date - arrives.Date).Days;
            return string.Format(CultureInfo.InvariantCulture, "{0} nights at ${1:N0} a night, ${2:N0}",
                nights, DateRangePickerModel.NightlyRate, nights * DateRangePickerModel.NightlyRate);
        }
    }
}
