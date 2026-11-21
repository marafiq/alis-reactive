using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ResidentContactController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.ResidentContact)]
    public IActionResult OfAResident() => View("/Care/Admission/ResidentContact.cshtml", new ResidentContact());

    [HttpGet(ResidentContactEndpoints.ContactCard)]
    public IActionResult ContactCard() => PartialView("/Care/Admission/ResidentContactPerson.cshtml", new ResidentContactPerson());

    [HttpPost(ResidentContactEndpoints.Residents)]
    public IActionResult SaveResident([FromForm] ResidentContact resident)
    {
        if (string.IsNullOrEmpty(resident.Name)) ModelState.AddModelError(nameof(ResidentContact.Name), "Name the resident.");
        if (ModelState.IsValid) return Content("Resident saved", PlainText);
        return ValidationProblem(ModelState);
    }

    [HttpPost(ResidentContactEndpoints.Contacts)]
    public IActionResult SaveContact([FromForm] ResidentContactPerson contact)
    {
        if (string.IsNullOrEmpty(contact.Name)) ModelState.AddModelError(nameof(ResidentContactPerson.Name), "Name the contact.");
        if (ModelState.IsValid) return Content("Contact saved", PlainText);
        return ValidationProblem(ModelState);
    }
}

public sealed class ResidentContact
{
    public string? Name { get; set; }
}

public sealed class ResidentContactPerson
{
    public string? Name { get; set; }
}

public static class ResidentContactEndpoints
{
    public const string ContactCard = CarePages.Area + "admission/resident-contact/contact-card";

    public const string Residents = CarePages.Area + "admission/resident-contact/residents";

    public const string Contacts = CarePages.Area + "admission/resident-contact/contacts";
}

public static class ResidentContactButtons
{
    public static readonly AppEvent SaveResident = AppEvent.Declare();

    public static readonly AppEvent OpenContact = AppEvent.Declare();

    public static readonly AppEvent SaveContact = AppEvent.Declare();
}
