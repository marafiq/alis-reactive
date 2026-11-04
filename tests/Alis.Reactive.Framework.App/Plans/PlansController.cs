using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Plans;

public sealed class PlansController : Controller
{
    public IActionResult Contacts() => View(new ContactsPage());

    [HttpPost("/Plans/Contacts")]
    public IActionResult SaveContacts([FromBody] ContactsPage contacts) => Said(
        $"next of kin {contacts.NextOfKin.Name ?? "none"}, alternate {contacts.Alternate.Name ?? "none"}");

    public IActionResult Keyholders() => View(new KeyholdersPage());

    [HttpPost("/Plans/Keyholders")]
    public IActionResult SaveKeyholders([FromBody] KeyholdersPage keyholders) => keyholders.Primary.Relationship is null
        ? BadRequest(new { errors = new Dictionary<string, string[]> { ["Primary.Relationship"] = ["Say how the keyholder is related."] } })
        : Said($"keyholder {keyholders.Primary.Name}");

    public IActionResult Noticeboard() => View(new NoticeboardPage());

    public IActionResult Notice() => PartialView("Notice", new Notice());

    public IActionResult TornNotice() => Content(
        "<section><h2>A torn notice</h2>" +
        "<script type=\"application/json\" data-reactive-plan=\"torn\">{\"planId\":\"torn\",</script></section>",
        "text/html");

    public IActionResult Refused(PlanMistake mistake) => View("Refusals", new PlanRefusalsPage { Mistake = mistake });

    private JsonResult Said(string summary) => Json(new Booked { Summary = summary });
}
