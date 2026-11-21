using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class IntakeAdmitController : Controller
{
    private const string PlainText = "text/plain";
    private const string Page = "/Care/Admission/IntakeAdmit.cshtml";
    private const string Token = "__RequestVerificationToken";
    private const string Ward = "Ward";
    private const string Code = "Code";

    [HttpGet(CarePages.IntakeAdmit)]
    public IActionResult WithTheWardInTheForm() => View(Page, new IntakeAdmission("", "intake"));

    [HttpGet(CarePages.IntakeAdmitWardApart)]
    public IActionResult WithTheWardApart() => View(Page, new IntakeAdmission("", null));

    [HttpPost(IntakeAdmissionEndpoints.Admissions)]
    public IActionResult Receive() => Content($"{Ward} {Sent(Ward)}, {Code} {Sent(Code)}, fields {Request.Form[Token].Count}", PlainText);

    private string Sent(string name) => Request.Form.TryGetValue(name, out var value) && value.ToString().Length > 0 ? value.ToString() : "none";
}

public sealed record IntakeAdmission(string Name, string? WardForm);

public static class IntakeAdmissionEndpoints
{
    public const string Admissions = CarePages.Area + "admission/intake-admit/admissions";
}

public static class IntakeAdmissions
{
    public static readonly AppEvent Admit = AppEvent.Declare();
}
