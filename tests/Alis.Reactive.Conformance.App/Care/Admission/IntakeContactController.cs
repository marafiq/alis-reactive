using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class IntakeContactController : Controller
{
    private const string PlainText = "text/plain";
    private const string Name = nameof(ContactIntake.Name);
    private const string Phone = nameof(ContactIntake.Phone);

    [HttpGet(CarePages.IntakeContact)]
    public IActionResult OfAnApplicant() => View("/Care/Admission/IntakeContact.cshtml", new ContactIntake("", ""));

    [HttpPost(IntakeContactEndpoints.Contacts)]
    public IActionResult Receive()
    {
        var received = new List<string>();
        if (Request.Form.ContainsKey(Name)) received.Add($"{Name} {Request.Form[Name]}".TrimEnd());
        if (Request.Form.ContainsKey(Phone)) received.Add($"{Phone} {Request.Form[Phone]}".TrimEnd());
        return Content(string.Join(", ", received), PlainText);
    }
}

public sealed record ContactIntake(string Name, string Phone);

public static class IntakeContactEndpoints
{
    public const string Contacts = CarePages.Area + "admission/intake-contact/contacts";
}

public static class IntakeContacts
{
    public static readonly AppEvent SendContact = AppEvent.Declare();

    public static readonly AppEvent SendAll = AppEvent.Declare();
}
