using Alis.Reactive.Conformance.World;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class HomeLeadController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.HomeLead)]
    public IActionResult OfTheHome() => View("/Care/Admission/HomeLead.cshtml", new HomeLead());

    [HttpPost(HomeLeadEndpoints.Leads)]
    public IActionResult Save([FromBody] HomeLead home) => Content($"{home.Lead.Name}, {home.Lead.Phone}", PlainText);
}

public sealed class HomeLead
{
    public HomeLeadPerson Lead { get; set; } = new() { Name = "Ruth" };
}

public sealed class HomeLeadPerson
{
    public string Name { get; set; } = "";

    public string? Phone { get; set; }
}

public sealed class HomeLeadRules : ReactiveValidator<HomeLead>
{
    public HomeLeadRules() => RuleFor(home => home.Lead.Name).NotEmpty().WithMessage("Lead name is required.");
}

public static class HomeLeadEndpoints
{
    public const string Leads = CarePages.HomeLead + "/leads";
}
