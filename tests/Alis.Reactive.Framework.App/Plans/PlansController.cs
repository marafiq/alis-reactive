using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Plans;

/// <summary>How a page's plans compose: by the plan id they share, on their own, and found in loaded HTML.</summary>
public sealed class PlansController : Controller
{
    public IActionResult Visit() => View(new VisitPage());

    /// <summary>The room field, a partial of the visit's own model, loaded into the page's slot.</summary>
    public IActionResult RoomField() => PartialView("RoomField", new VisitPage());

    /// <summary>Binds the visit as any endpoint does and says whom and what it bound.</summary>
    [HttpPost("/Plans/Visits")]
    public IActionResult Visits([FromBody] VisitPage visit) => Said(
        $"visitor {visit.Visitor ?? "none"}, resident {visit.Resident ?? "none"}, " +
        $"room {visit.Room ?? "none"}, companion {visit.Companion.Name ?? "none"}");

    /// <summary>Refuses every visit for a member the page renders no field for.</summary>
    [HttpPost("/Plans/Visits/Unreasoned")]
    public IActionResult Unreasoned() =>
        BadRequest(new { errors = new Dictionary<string, string[]> { ["Reason"] = ["Say why the visit is booked."] } });

    public IActionResult Contacts() => View(new ContactsPage());

    /// <summary>Binds the contacts as any endpoint does and says whom it bound for each.</summary>
    [HttpPost("/Plans/Contacts")]
    public IActionResult SaveContacts([FromBody] ContactsPage contacts) => Said(
        $"next of kin {contacts.NextOfKin.Name ?? "none"}, alternate {contacts.Alternate.Name ?? "none"}");

    public IActionResult Noticeboard() => View(new NoticeboardPage());

    public IActionResult Notice() => PartialView("Notice", new Notice());

    /// <summary>
    /// A notice whose HTML the framework did not write: its plan script holds text that is not JSON, as a
    /// hand-edited or truncated response would.
    /// </summary>
    public IActionResult TornNotice() => Content(
        "<section><h2>A torn notice</h2>" +
        "<script type=\"application/json\" data-reactive-plan>{\"planId\":\"torn\",</script></section>",
        "text/html");

    /// <summary>The serviced page, its plan rendered with the request's services, or with services that hold no rules.</summary>
    public IActionResult Serviced(bool withoutRules = false)
    {
        ViewData["WithoutRules"] = withoutRules;
        return View(new ServicedPage());
    }

    [HttpPost("/Plans/Serviced")]
    public IActionResult SaveServiced([FromBody] ServicedPage page) => Said($"visitor {page.Visitor ?? "none"}");

    public IActionResult Refused(PlanMistake mistake) => View("Refusals", new PlanRefusalsPage { Mistake = mistake });

    private JsonResult Said(string summary) => Json(new Booked { Summary = summary });
}
