using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Rows;

public sealed class RowsController : Controller
{
    public IActionResult Contacts(int contacts = 2) => View(ContactSheet.OnFile(contacts));

    [HttpPost]
    public IActionResult AddContact([FromBody] ContactSheet sheet)
    {
        sheet.Contacts.Add(new Contact());
        return PartialView("ContactRows", sheet);
    }

    [HttpPost]
    public IActionResult RemoveContact([FromBody] ContactRemoval removal)
    {
        removal.Contacts.RemoveAt(removal.Remove);
        return PartialView("ContactRows", new ContactSheet { Contacts = removal.Contacts });
    }

    [HttpPost]
    public IActionResult SaveContacts([FromBody] ContactSheet sheet, [FromServices] ContactSheetRules rules) => Saved(sheet, rules);

    [HttpPost]
    public IActionResult PostContacts([FromForm] ContactSheet sheet, [FromServices] ContactSheetRules rules) => Saved(sheet, rules);

    private IActionResult Saved(ContactSheet sheet, ContactSheetRules rules)
    {
        var result = rules.Validate(sheet);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        var saved = sheet.Contacts.Count == 0
            ? "no contacts"
            : string.Join(", ", sheet.Contacts.Select(contact => contact?.Name ?? "a gap"));
        return Json(new SavedContacts { Summary = $"{sheet.Resident}'s contacts: {saved}" });
    }
}
