using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Inputs;

/// <summary>Inputs bound to a model's members: gathered, converted, validated (capabilities I and P4).</summary>
public sealed class InputsController : Controller
{
    public IActionResult Admit() => View(new AdmitPage());

    /// <summary>Binds the posted admission as any endpoint does, and says what it bound.</summary>
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

    /// <summary>
    /// Binds the posted booking from its form fields as any endpoint does, and says what it bound: each member's
    /// value, or the text it was sent when that text does not bind to the member's type.
    /// </summary>
    [HttpPost]
    public IActionResult Booking([FromForm] BookingPage booking) => Json(Booked(booking));

    /// <summary>Binds the booking from a JSON body as any endpoint does, and says what it bound.</summary>
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

    private string Bound(string label, string member, string value) =>
        ModelState.TryGetValue(member, out var entry) && entry.Errors.Count > 0
            ? $"{label} '{entry.AttemptedValue}' not bound"
            : $"{label} {value}";

    /// <summary>A visitor's arrival, the desk's pass carried hidden, unless the desk gave none; a note the address gives starts the page.</summary>
    public IActionResult Arrival(string? note = null, bool noPass = false) => View(new ArrivalPage { Note = note, Pass = noPass ? null : "P-7" });

    /// <summary>Binds the posted arrival as any endpoint does, and says what it bound.</summary>
    [HttpPost]
    public IActionResult Arrival([FromBody] ArrivalPage arrival) => Json(new ArrivalEcho
    {
        Summary = string.Join(", ",
            $"visitor {arrival.Visitor ?? "none"}",
            $"room {arrival.Room ?? "none"}",
            $"note {arrival.Note ?? "none"}",
            $"pass {arrival.Pass ?? "none"}"),
    });

    /// <summary>A room on the ward, its inputs labelled by the ids the framework gives them.</summary>
    public IActionResult Room() => View(new Ward.Room());

    /// <summary>Binds the posted room as any endpoint does, and says what it bound.</summary>
    [HttpPost]
    public IActionResult Room([FromBody] Ward.Room room) => Json(new RoomEcho
    {
        Summary = $"bed {room.Bed}, note {room.Note ?? "none"}, phone {room.Contact.Phone ?? "none"}",
    });

    /// <summary>A visitor signing out, the visitor's field rendered a second time as the address asks.</summary>
    public IActionResult Depart(DepartRendering rendering) => View(new DepartPage { Rendering = rendering });

    /// <summary>Binds the visitor who signed out, and says whom.</summary>
    [HttpPost]
    public IActionResult Depart([FromBody] DepartPage departed) => Json(new DepartEcho { Summary = $"visitor {departed.Visitor ?? "none"}" });

    /// <summary>A resident's contacts: the contact card rendered for the primary contact and for the alternate.</summary>
    public IActionResult Contacts() => View(new ContactsPage
    {
        Primary = new ContactCard { Phones = new() { "555 0100" } },
        Alternate = new ContactCard { Phones = new() { "555 0199" } },
    });

    /// <summary>Binds the contacts as any endpoint does, and says what it bound for each member.</summary>
    [HttpPost]
    public IActionResult Contacts([FromBody] ContactsPage contacts) => Json(new ContactsEcho
    {
        Summary = $"primary {Said(contacts.Primary)}, alternate {Said(contacts.Alternate)}",
    });

    /// <summary>Binds the contacts a plain form posted, by the names of its inputs, and says what it bound.</summary>
    [HttpPost("/Inputs/Contacts/Posted")]
    public IActionResult ContactsPosted([FromForm] ContactsPage contacts) =>
        Content($"Posted: primary {Said(contacts.Primary)}, alternate {Said(contacts.Alternate)}");

    private static string Said(ContactCard card) => $"{card.Name ?? "none"} ({card.Phones.FirstOrDefault() ?? "none"})";

    /// <summary>A resident's care, each choice starting on an answer the rules accept.</summary>
    public IActionResult Care() => View(new CarePage { Level = CareLevel.Residential, PreviousLevel = CareLevel.Residential });

    /// <summary>Checks the care by the rules the page carries, and says what it bound: each level as its member and its number.</summary>
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
