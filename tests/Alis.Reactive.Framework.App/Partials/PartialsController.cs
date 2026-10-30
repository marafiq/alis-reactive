using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Partials;

/// <summary>A page and its partials composing by the member an input binds: an editor template, a partial of a base class, a generic model.</summary>
public sealed class PartialsController : Controller
{
    public IActionResult Desk() => View(new DeskPage());

    /// <summary>Binds the desk as any endpoint does and checks it by the desk's rules; then says whom it bound.</summary>
    [HttpPost]
    public IActionResult SaveDesk([FromBody] DeskPage desk, [FromServices] DeskRules rules)
    {
        var result = rules.Validate(desk);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Said($"visitor {desk.Visitor ?? "none"}, next of kin {desk.NextOfKin.Name ?? "none"}");
    }

    /// <summary>Checks the next of kin against the resident's file, so a message names the member as the desk binds it (NextOfKin.Name).</summary>
    [HttpPost]
    public IActionResult CheckKin([FromBody] DeskPage desk, [FromServices] KinOnFileRules rules)
    {
        var result = rules.Validate(desk);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Said($"next of kin {desk.NextOfKin.Name} is on file");
    }

    /// <summary>The reception: the desk's record, whose next of kin is loaded into the page when the visitor names one.</summary>
    public IActionResult Reception() => View(new DeskPage());

    /// <summary>The reception with two slots that each load the next of kin's template, so the page holds its input twice.</summary>
    public IActionResult DoubleBooked() => View(new DeskPage());

    /// <summary>
    /// The next of kin's editor template, loaded into the reception's slot: rendered for the desk's NextOfKin member,
    /// as EditorFor renders it, so its input binds as NextOfKin.Name.
    /// </summary>
    public IActionResult NextOfKin()
    {
        ViewData.TemplateInfo.HtmlFieldPrefix = nameof(DeskPage.NextOfKin);
        return PartialView("EditorTemplates/KinCard", new KinCard());
    }

    public IActionResult Admission(bool showsRoom = false) => View(new AdmissionPage { ShowsRoom = showsRoom });

    /// <summary>The room's line, loaded into the admission's slot: a partial of the admission's own model.</summary>
    public IActionResult RoomLine() => PartialView("RoomLine", new AdmissionPage());

    /// <summary>Binds the admission as any endpoint does, checks it by the admission's rules, and says where the resident goes.</summary>
    [HttpPost]
    public IActionResult Admit([FromBody] AdmissionPage admission, [FromServices] AdmissionRules rules)
    {
        var result = rules.Validate(admission);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Said($"{admission.Resident ?? "no one"} admitted to room {admission.Room}");
    }

    public IActionResult Lobby() => View(new LobbyPage());

    /// <summary>Binds the lobby's record as any endpoint does, checks it by the sign-in rules, and says whom it signed in.</summary>
    [HttpPost]
    public IActionResult SignIn([FromBody] LobbyPage lobby, [FromServices] LobbyRules rules)
    {
        var result = rules.Validate(lobby);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Said($"visitor {lobby.Visitor} signed in to see {lobby.Resident ?? "no one"}");
    }

    public IActionResult Fluids() => View(new ChartEntry<FluidsReading>());

    /// <summary>Binds the chart entry as any endpoint does and says what was charted, and by whom.</summary>
    [HttpPost]
    public IActionResult ChartFluids([FromBody] ChartEntry<FluidsReading> entry) => Said(
        $"{entry.Reading.Millilitres?.ToString() ?? "no"} ml, initials {entry.Initials ?? "none"}");

    public IActionResult Refused(PartialMistake mistake) => View("Refused", new PartialRefusalsPage { Mistake = mistake });

    public IActionResult Handover() => View(new HandoverPage());

    /// <summary>Refuses every handover whose keyholder's relationship is not given, by the member's binding name.</summary>
    [HttpPost]
    public IActionResult HandOver([FromBody] HandoverPage handover) => handover.Keyholder.Relationship is null
        ? BadRequest(new { errors = new Dictionary<string, string[]> { ["Keyholder.Relationship"] = ["Say how the keyholder is related."] } })
        : Said($"room {handover.Room} handed to {handover.Keyholder.Name}");

    public IActionResult Escort() => View(new EscortPage());

    /// <summary>Refuses the escort a line sends: which shift they cover is the server's to ask for, by the member's binding name.</summary>
    [HttpPost]
    public IActionResult CheckEscort([FromBody] EscortPage escort)
    {
        var line = escort.Driver.Name is null ? nameof(EscortPage.Aide) : nameof(EscortPage.Driver);
        return BadRequest(new { errors = new Dictionary<string, string[]> { [$"{line}.Shift"] = [$"Say which shift the {line.ToLowerInvariant()} covers."] } });
    }

    private JsonResult Said(string summary) => Json(new PartialsEcho { Summary = summary });
}
