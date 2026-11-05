using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Partials;

public sealed class PartialsController : Controller
{
    public IActionResult Desk() => View(new DeskPage());

    [HttpPost]
    public IActionResult SaveDesk([FromBody] DeskPage desk, [FromServices] DeskRules rules)
    {
        var result = rules.Validate(desk);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Said($"visitor {desk.Visitor ?? "none"}, next of kin {desk.NextOfKin.Name ?? "none"}");
    }

    [HttpPost]
    public IActionResult CheckKin([FromBody] DeskPage desk, [FromServices] KinOnFileRules rules)
    {
        var result = rules.Validate(desk);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Said($"next of kin {desk.NextOfKin.Name} is on file");
    }

    public IActionResult Reception() => View(new DeskPage());

    public IActionResult DoubleBooked() => View(new DeskPage());

    public IActionResult NextOfKin()
    {
        ViewData.TemplateInfo.HtmlFieldPrefix = nameof(DeskPage.NextOfKin);
        return PartialView("EditorTemplates/KinCard", new KinCard());
    }

    public IActionResult Admission(bool showsRoom = false) => View(new AdmissionPage { ShowsRoom = showsRoom });

    public IActionResult RoomLine() => PartialView("RoomLine", new AdmissionPage());

    [HttpPost]
    public IActionResult Admit([FromBody] AdmissionPage admission, [FromServices] AdmissionRules rules)
    {
        var result = rules.Validate(admission);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Said($"{admission.Resident ?? "no one"} admitted to room {admission.Room}");
    }

    public IActionResult Lobby() => View(new LobbyPage());

    [HttpPost]
    public IActionResult SignIn([FromBody] LobbyPage lobby, [FromServices] LobbyRules rules)
    {
        var result = rules.Validate(lobby);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Said($"visitor {lobby.Visitor} signed in to see {lobby.Resident ?? "no one"}");
    }

    public IActionResult Fluids() => View(new ChartEntry<FluidsReading>());

    [HttpPost]
    public IActionResult ChartFluids([FromBody] ChartEntry<FluidsReading> entry) => Said(
        $"{entry.Reading.Millilitres?.ToString() ?? "no"} ml, initials {entry.Initials ?? "none"}");

    public IActionResult Refused(PartialMistake mistake) => View("Refused", new PartialRefusalsPage { Mistake = mistake });

    public IActionResult Sitting() => View(new SittingPage());

    [HttpPost]
    public IActionResult BookSitting([FromBody] SittingPage sitting) => sitting.NextOfKin.Name is "Bo" or "Cy"
        ? Said($"{sitting.Visitor ?? "no one"} sits with the resident beside {sitting.NextOfKin.Name}")
        : BadRequest(new { errors = new Dictionary<string, string[]> { ["NextOfKin.Name"] = ["That name is not on the resident's file."] } });

    public IActionResult Handover() => View(new HandoverPage());

    [HttpPost]
    public IActionResult HandOver([FromBody] HandoverPage handover) => handover.Keyholder.Relationship is null
        ? BadRequest(new { errors = new Dictionary<string, string[]> { ["Keyholder.Relationship"] = ["Say how the keyholder is related."] } })
        : Said($"room {handover.Room} handed to {handover.Keyholder.Name}");

    public IActionResult Escort() => View(new EscortPage());

    [HttpPost]
    public IActionResult CheckEscort([FromBody] EscortPage escort)
    {
        var line = escort.Driver.Name is null ? nameof(EscortPage.Aide) : nameof(EscortPage.Driver);
        return BadRequest(new { errors = new Dictionary<string, string[]> { [$"{line}.Shift"] = [$"Say which shift the {line.ToLowerInvariant()} covers."] } });
    }

    public IActionResult Concierge() => View(new ConciergePage());

    [HttpPost]
    public IActionResult SaveConcierge([FromBody] ConciergePage visit, [FromServices] ConciergeRules rules)
    {
        var result = rules.Validate(visit);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Said($"visitor {visit.Visitor ?? "none"}, next of kin {visit.NextOfKin.Name ?? "none"}");
    }

    [HttpPost]
    public IActionResult NoteKin([FromBody] KinNote note) => Said($"next of kin {note.NextOfKin.Name ?? "none"}");

    public IActionResult Changeover() => View(new ChangeoverPage());

    public IActionResult Corrections() => View(new CorrectionsPage());

    [HttpPost]
    public IActionResult CorrectRoom([FromBody] RoomCorrection correction) => Said($"room {correction.Value ?? "none"}");

    [HttpPost]
    public IActionResult CorrectPhone([FromBody] PhoneCorrection correction) => Said($"phone {correction.Value ?? "none"}");

    public IActionResult Bookings() => View(new BookingsPage());

    [HttpPost]
    public IActionResult BookVisit([FromBody] BookingsPage booking, [FromServices] BookingRules rules)
    {
        var result = rules.Validate(booking);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Said($"visitor {booking.Visitor ?? "none"}, next of kin {booking.NextOfKin.Name ?? "none"}");
    }

    private JsonResult Said(string summary) => Json(new PartialsEcho { Summary = summary });
}
