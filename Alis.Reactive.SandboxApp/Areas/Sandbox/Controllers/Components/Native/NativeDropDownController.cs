using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Native
{
    /// <summary>
    /// Van booking: the front desk books the community van for a resident's trip. The van is out at
    /// 10:00 AM (the dialysis run), so a booking then is offered the next free time.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/NativeDropDown")]
    public class NativeDropDownController : Controller
    {
        private const string VanOut = "10:00 AM";
        private const string NextFree = "11:00 AM";

        private static readonly IReadOnlyDictionary<string, string> Residents = new Dictionary<string, string>
        {
            ["margaret"] = "Margaret Hale",
            ["harold"] = "Harold Benson",
            ["ruth"] = "Ruth Coleman",
        };

        private static readonly IReadOnlyList<string> Trips = new[] { "Medical appointment", "Pharmacy pickup", "Shopping", "Family outing" };
        private static readonly IReadOnlyList<string> Departures = new[] { "9:00 AM", VanOut, NextFree, "1:30 PM" };
        private static readonly IReadOnlyList<string> Escorts = new[] { "No escort", "Family member", "Nurse aide" };

        [HttpGet("")]
        public IActionResult Index()
        {
            ViewBag.Residents = Residents.Select(resident => new SelectListItem(resident.Value, resident.Key)).ToArray();
            ViewBag.Trips = Options(Trips);
            ViewBag.Departures = Options(Departures);
            ViewBag.Escorts = Options(Escorts);
            return View("~/Areas/Sandbox/Views/Components/Native/NativeDropDown/Index.cshtml", new NativeDropDownModel { Escort = "No escort" });
        }

        [HttpPost("Book")]
        public IActionResult Book([FromBody] NativeDropDownModel? booking)
        {
            if (booking == null) return BadRequest();

            var result = new VanBookingValidator().Validate(booking);
            if (!result.IsValid) return BadRequest(new { errors = result.ErrorsByField });

            if (!Residents.TryGetValue(booking.ResidentId!, out var resident)) return BadRequest();
            if (booking.Departure == VanOut) return Conflict(new VanBusy { NextFree = NextFree });

            var escort = booking.Escort switch
            {
                "Nurse aide" => "with a nurse aide",
                "Family member" => "with a family member",
                _ => "no escort",
            };
            return Ok(new VanBooked { Message = $"Van booked for {resident}: {booking.Trip} at {booking.Departure}, {escort}." });
        }

        private static SelectListItem[] Options(IEnumerable<string> texts) =>
            texts.Select(text => new SelectListItem(text, text)).ToArray();
    }
}
