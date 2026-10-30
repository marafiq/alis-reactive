using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Forms;

/// <summary>
/// A form is an element: a request sends the inputs inside the form it names, and none outside it, and each form
/// shows what it holds: a message beside the field it names, and one no field shows in a summary of its own.
/// </summary>
public sealed class FormsController : Controller
{
    public IActionResult Outing() => View(new OutingPage());

    /// <summary>A page whose request sends its form's inputs but checks no form, so no form is named.</summary>
    public IActionResult Unnamed() => View(new OutingPage());

    /// <summary>Binds the outing as any endpoint does and says what it bound.</summary>
    [HttpPost]
    public IActionResult Bound([FromBody] OutingPage outing) => Json(new FormsEcho
    {
        Summary = $"resident {outing.Resident ?? "none"}, companion {outing.Companion ?? "none"}, note {outing.Note ?? "none"}",
    });

    public IActionResult FrontDesk() => View(new FrontDeskPage());

    /// <summary>Refuses every visit: whom the visitor came to see is the server's to ask for, and no field shows it.</summary>
    [HttpPost]
    public IActionResult LogVisit() =>
        BadRequest(new { errors = new Dictionary<string, string[]> { ["Resident"] = ["Say whom the visitor came to see."] } });

    /// <summary>Refuses every parcel: who signs for it is the server's to ask for, and no field shows it.</summary>
    [HttpPost]
    public IActionResult LogParcel() =>
        BadRequest(new { errors = new Dictionary<string, string[]> { ["SignedBy"] = ["Say who signs for the parcel."] } });

    public IActionResult Stay() => View(new StayPage());

    /// <summary>The guest's line, a partial of the stay's own model, loaded into the stay's form when a guest is picked.</summary>
    public IActionResult GuestLine() => PartialView("GuestLine", new StayPage());

    /// <summary>Checks the guest against the arrivals the desk expects, by the member's binding name.</summary>
    [HttpPost]
    public IActionResult CheckGuest([FromBody] StayPage stay) => stay.Guest is "Ada" or "Bo"
        ? Json(new FormsEcho { Summary = $"{stay.Guest} is expected" })
        : BadRequest(new { errors = new Dictionary<string, string[]> { ["Guest"] = ["No guest of that name is expected."] } });

    /// <summary>Refuses every stay: when it starts is the server's to ask for, and no field shows it.</summary>
    [HttpPost]
    public IActionResult BookStay() =>
        BadRequest(new { errors = new Dictionary<string, string[]> { ["StartsOn"] = ["Say when the stay starts."] } });
}
