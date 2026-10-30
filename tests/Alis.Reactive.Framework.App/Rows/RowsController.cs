using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Rows;

/// <summary>
/// A list edited row by row (ADR-0003): one list partial in a slot, which the desk adds a contact to or removes one
/// from by posting the list, and which the server renders again with its rows numbered from 0, with no gap. The
/// page's rules check every row on the page when the sheet is saved, whoever rendered it.
/// </summary>
public sealed class RowsController : Controller
{
    /// <summary>Ada's sheet, with the first <paramref name="contacts"/> contacts on her file.</summary>
    public IActionResult Contacts(int contacts = 2) => View(ContactSheet.OnFile(contacts));

    /// <summary>The list as the desk posted it, with a blank contact after the rest.</summary>
    [HttpPost]
    public IActionResult AddContact([FromBody] ContactSheet sheet)
    {
        sheet.Contacts.Add(new Contact());
        return PartialView("ContactRows", sheet);
    }

    /// <summary>The list as the desk posted it, without the contact at the position it names.</summary>
    [HttpPost]
    public IActionResult RemoveContact([FromBody] ContactRemoval removal)
    {
        removal.Contacts.RemoveAt(removal.Remove);
        return PartialView("ContactRows", new ContactSheet { Contacts = removal.Contacts });
    }

    /// <summary>Binds the sheet from a JSON body, checks it by the sheet's rules, and says whom it saved.</summary>
    [HttpPost]
    public IActionResult SaveContacts([FromBody] ContactSheet sheet, [FromServices] ContactSheetRules rules) => Saved(sheet, rules);

    /// <summary>Binds the sheet from a form post, as a plain HTML form posts it, and checks and answers the same way.</summary>
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
