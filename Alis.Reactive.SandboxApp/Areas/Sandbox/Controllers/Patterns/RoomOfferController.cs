using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Patterns.RoomOffer;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Patterns;

/// <summary>
/// The room offer: the page fills the deposit, the floor and the rate from what the coordinator
/// chooses; the server sends the offer as MVC binds it, the room's members by their dotted names.
/// </summary>
[Area("Sandbox")]
[Route("Sandbox/Patterns/RoomOffer")]
public class RoomOfferController : Controller
{
    private static readonly CultureInfo Dollars = CultureInfo.GetCultureInfo("en-US");

    [HttpGet("")]
    public IActionResult Index() =>
        View("~/Areas/Sandbox/Views/Patterns/RoomOffer/Index.cshtml", new RoomOfferModel());

    // "Offer sent for the Oak wing, floor 2, at $4,800.00 a month with a $2,500.00 deposit."
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
