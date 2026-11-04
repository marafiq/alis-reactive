using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Inputs;

public sealed class InputsController : Controller
{
    public IActionResult Admit() => View(new AdmitPage());

    [HttpPost]
    public IActionResult Admit([FromBody] AdmitPage admitted) => Json(new AdmitEcho
    {
        Summary = string.Join(", ",
            admitted.Name,
            $"{admitted.Age + 1} next year",
            $"weighs {(admitted.Weight is decimal weight ? weight.ToString("0.0", CultureInfo.InvariantCulture) : "unknown")}",
            $"admitted {admitted.Admitted.ToString("dddd yyyy-MM-dd", CultureInfo.InvariantCulture)}",
            $"arrives {admitted.Arrives.ToUniversalTime().ToString("HH:mm", CultureInfo.InvariantCulture)} UTC"),
    });

    public IActionResult Booking() => View(new BookingPage());

    [HttpPost]
    public IActionResult Booking([FromForm] BookingPage booking) => Json(Booked(booking));

    [HttpPost("/Inputs/Booking/Json")]
    public IActionResult BookingJson([FromBody] BookingPage booking) => Json(Booked(booking));

    private BookingEcho Booked(BookingPage booking) => new()
    {
        Summary = string.Join(", ",
            Bound("day", nameof(BookingPage.Day), booking.Day.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)),
            Bound("called", nameof(BookingPage.Called), booking.Called.ToString("yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture)),
            Bound("guests", nameof(BookingPage.Guests), booking.Guests.ToString(CultureInfo.InvariantCulture)),
            booking.Tags is { Count: > 0 } tags ? "tags " + string.Join(" and ", tags.Select(tag => $"'{tag}'")) : "no tags"),
    };

    private string Bound(string label, string member, string value)
        => ModelState.TryGetValue(member, out var entry) && entry.Errors.Count > 0
            ? $"{label} '{entry.AttemptedValue}' not bound"
            : $"{label} {value}";

    public IActionResult Arrival(string? note = null, bool noPass = false) => View(new ArrivalPage { Note = note, Pass = noPass ? null : "P-7" });

    [HttpPost]
    public IActionResult Arrival([FromBody] ArrivalPage arrival) => Json(new ArrivalEcho
    {
        Summary = string.Join(", ",
            $"visitor {arrival.Visitor ?? "none"}",
            $"room {arrival.Room ?? "none"}",
            $"note {arrival.Note ?? "none"}",
            $"pass {arrival.Pass ?? "none"}"),
    });

    public IActionResult Room() => View(new Ward.Room());

    [HttpPost]
    public IActionResult Room([FromBody] Ward.Room room) => Json(new RoomEcho
    {
        Summary = $"bed {room.Bed}, note {room.Note ?? "none"}, phone {room.Contact.Phone ?? "none"}",
    });

    public IActionResult Depart(DepartRendering rendering) => View(new DepartPage { Rendering = rendering });

    [HttpPost]
    public IActionResult Depart([FromBody] DepartPage departed) => Json(new DepartEcho { Summary = $"visitor {departed.Visitor ?? "none"}" });

    public IActionResult Contacts() => View(new ContactsPage
    {
        Primary = new ContactCard { Phones = new() { "555 0100" } },
        Alternate = new ContactCard { Phones = new() { "555 0199" } },
    });

    [HttpPost]
    public IActionResult Contacts([FromBody] ContactsPage contacts) => Json(new ContactsEcho
    {
        Summary = $"primary {Said(contacts.Primary)}, alternate {Said(contacts.Alternate)}",
    });

    [HttpPost("/Inputs/Contacts/Posted")]
    public IActionResult ContactsPosted([FromForm] ContactsPage contacts)
        => Content($"Posted: primary {Said(contacts.Primary)}, alternate {Said(contacts.Alternate)}");

    private static string Said(ContactCard card) => $"{card.Name ?? "none"} ({card.Phones.FirstOrDefault() ?? "none"})";

    public IActionResult Circle() => View(new CirclePage());

    [HttpPost]
    public IActionResult Circle([FromBody] CirclePage circle) => Json(new CircleEcho
    {
        Summary = circle.Members.Count == 0
            ? "no members"
            : string.Join(", ", circle.Members.Select((member, position) => $"member {position} {member?.Name ?? "none"}")),
    });

    public IActionResult Care() => View(new CarePage { Level = CareLevel.Residential, PreviousLevel = CareLevel.Residential });

    [HttpPost]
    public IActionResult Care([FromBody] CarePage care, [FromServices] CareRules rules)
    {
        var result = rules.Validate(care);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new CareEcho
        {
            Summary = string.Join(", ",
                $"level {care.Level} ({(int)care.Level})",
                $"previously {(care.PreviousLevel is CareLevel previous ? $"{previous} ({(int)previous})" : "none")}",
                $"on the {care.Shift} shift",
                $"equipment {care.Equipment ?? "none"}",
                $"keyworker {care.Keyworker ?? "none"}"),
        });
    }
}
