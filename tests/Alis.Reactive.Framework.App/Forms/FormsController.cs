using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Forms;

public sealed class FormsController : Controller
{
    public IActionResult Outing() => View(new OutingPage());

    public IActionResult Unnamed() => View(new OutingPage());

    [HttpPost]
    public IActionResult Bound([FromBody] OutingPage outing) => Json(new FormsEcho
    {
        Summary = $"resident {outing.Resident ?? "none"}, companion {outing.Companion ?? "none"}, note {outing.Note ?? "none"}",
    });

    public IActionResult Lunch() => View(new LunchPage());

    [HttpPost]
    public IActionResult BookLunch([FromBody] LunchPage lunch) => Json(new FormsEcho
    {
        Summary = $"lunch for {lunch.Resident ?? "no one"}, companion {lunch.Companion ?? "none"}",
    });

    public IActionResult FrontDesk() => View(new FrontDeskPage());

    [HttpPost]
    public IActionResult LogVisit()
        => BadRequest(new { errors = new Dictionary<string, string[]> { ["Resident"] = ["Say whom the visitor came to see."] } });

    [HttpPost]
    public IActionResult LogParcel()
        => BadRequest(new { errors = new Dictionary<string, string[]> { ["SignedBy"] = ["Say who signs for the parcel."] } });

    public IActionResult Stay() => View(new StayPage());

    public IActionResult GuestLine() => PartialView("GuestLine", new StayPage());

    [HttpPost]
    public IActionResult CheckGuest([FromBody] StayPage stay) => stay.Guest is "Ada" or "Bo"
        ? Json(new FormsEcho { Summary = $"{stay.Guest} is expected" })
        : BadRequest(new { errors = new Dictionary<string, string[]> { ["Guest"] = ["No guest of that name is expected."] } });

    [HttpPost]
    public IActionResult BookStay()
        => BadRequest(new { errors = new Dictionary<string, string[]> { ["StartsOn"] = ["Say when the stay starts."] } });
}
