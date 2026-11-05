using System.Globalization;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Patterns.RoomOffer;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Patterns;

[Area("Sandbox")]
[Route("Sandbox/Patterns/RoomOffer")]
public class RoomOfferController : Controller
{
    private static readonly CultureInfo Dollars = CultureInfo.GetCultureInfo("en-US");

    [HttpGet("")]
    public IActionResult Index()
        => View("~/Areas/Sandbox/Views/Patterns/RoomOffer/Index.cshtml", new RoomOfferModel());

    [HttpPost("Send")]
    public IActionResult Send([FromBody] RoomOfferModel? offer)
    {
        if (offer == null) return BadRequest();

        var rate = offer.Room.MonthlyRate.ToString("C", Dollars);
        var deposit = offer.Deposit.ToString("C", Dollars);
        return Ok(new RoomOfferSent(
            $"Offer sent for the {offer.Room.Wing} wing, floor {offer.Room.Floor}, at {rate} a month with a {deposit} deposit."));
    }
}
