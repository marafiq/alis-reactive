using Alis.Reactive.Conformance.World;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class HouseholdContactsController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.HouseholdContacts)]
    public IActionResult OfAResident() => View("/Care/Admission/HouseholdContacts.cshtml", new HouseholdContacts());

    [HttpPost(HouseholdContactsEndpoints.Contacts)]
    public IActionResult Save([FromForm] HouseholdContacts contacts) => Content(contacts.WebPage ?? string.Empty, PlainText);
}

public sealed class HouseholdContacts
{
    public string? WebPage { get; set; }
}

public sealed class HouseholdContactsRules : ReactiveValidator<HouseholdContacts>
{
    public HouseholdContactsRules() => RuleFor(contacts => contacts.WebPage).Url().WithMessage("Give the family's web page.");
}

public static class HouseholdContactsEndpoints
{
    public const string Contacts = CarePages.HouseholdContacts + "/contacts";
}
