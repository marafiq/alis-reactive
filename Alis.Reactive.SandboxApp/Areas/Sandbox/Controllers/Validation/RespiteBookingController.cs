using System.Globalization;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Validation
{
    /// <summary>
    /// Respite-stay booking: the coordinator picks a community, enters the daily rate and,
    /// when known, the expected number of nights and whether the community drives the resident
    /// in. The confirmation repeats what the server received, so a blank field that reached the
    /// server as zero or "no" shows up on the page.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Validation/RespiteBooking")]
    public class RespiteBookingController : Controller
    {
        private static readonly IReadOnlyDictionary<int, string> Communities = new Dictionary<int, string>
        {
            [1] = "Maple Grove",
            [2] = "Cedar Ridge",
            [3] = "Willow Creek",
        };

        [HttpGet("")]
        public IActionResult Index()
        {
            ViewBag.Communities = Communities
                .Select(community => new SelectListItem(community.Value, community.Key.ToString(CultureInfo.InvariantCulture)))
                .ToList();

            return View("~/Areas/Sandbox/Views/Validation/RespiteBooking/Index.cshtml", new RespiteBookingModel());
        }

        [HttpPost("Book")]
        public IActionResult Book([FromBody] RespiteBookingModel? booking)
        {
            if (booking == null) return BadRequest();

            var result = new RespiteBookingValidator().Validate(booking);
            if (!result.IsValid)
            {
                return FieldErrors(result.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray()));
            }

            // The validator requires the community and the daily rate.
            var communityId = booking.CommunityId!.Value;
            var dailyRate = booking.DailyRate!.Value;
            if (!Communities.TryGetValue(communityId, out var community))
            {
                var unknownCommunity = $"Community {communityId.ToString(CultureInfo.InvariantCulture)} does not exist.";
                return FieldErrors(new Dictionary<string, string[]> { [nameof(booking.CommunityId)] = new[] { unknownCommunity } });
            }

            return Ok(new RespiteBookingConfirmation(ConfirmationFor(booking, community, dailyRate)));
        }

        private BadRequestObjectResult FieldErrors(Dictionary<string, string[]> errors) => BadRequest(new { errors });

        private static string ConfirmationFor(RespiteBookingModel booking, string community, decimal dailyRate)
        {
            var rate = dailyRate.ToString("0.00", CultureInfo.InvariantCulture);
            var length = booking.ExpectedNights is { } nights
                ? $"{nights.ToString(CultureInfo.InvariantCulture)} nights"
                : "open-ended";
            var transport = booking.NeedsTransport switch
            {
                true => "transport arranged",
                false => "no transport",
                null => "transport to be decided",
            };

            return $"Respite stay booked for {booking.ResidentName} at {community}: ${rate} per day, {length}, {transport}.";
        }
    }
}
