using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Plans;

/// <summary>
/// Every plan of a page stands alone: cards of one model that each start a plan, the plans found in loaded HTML, and
/// the mistakes a plan is refused for when it renders.
/// </summary>
public sealed class PlansController : Controller
{
    public IActionResult Contacts() => View(new ContactsPage());

    /// <summary>Binds the contacts as any endpoint does and says whom it bound for each.</summary>
    [HttpPost("/Plans/Contacts")]
    public IActionResult SaveContacts([FromBody] ContactsPage contacts) => Said(
        $"next of kin {contacts.NextOfKin.Name ?? "none"}, alternate {contacts.Alternate.Name ?? "none"}");

    public IActionResult Keyholders() => View(new KeyholdersPage());

    /// <summary>Refuses a keyholder whose relationship is not given, by the member's binding name.</summary>
    [HttpPost("/Plans/Keyholders")]
    public IActionResult SaveKeyholders([FromBody] KeyholdersPage keyholders) => keyholders.Primary.Relationship is null
        ? BadRequest(new { errors = new Dictionary<string, string[]> { ["Primary.Relationship"] = ["Say how the keyholder is related."] } })
        : Said($"keyholder {keyholders.Primary.Name}");

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

    public IActionResult Refused(PlanMistake mistake) => View("Refusals", new PlanRefusalsPage { Mistake = mistake });

    private JsonResult Said(string summary) => Json(new Booked { Summary = summary });
}
