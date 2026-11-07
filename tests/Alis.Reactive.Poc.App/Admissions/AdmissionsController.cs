using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Poc.App.Admissions;

public sealed class AdmissionsController : Controller
{
    [HttpGet]
    public IActionResult Intake() => View(new IntakeForm { ResidentName = "Helen Park", Ward = "South", NextOfKin = new IntakeForm.Kin { Name = "Sam Park" } });

    [HttpPost]
    public IActionResult Intake(IntakeForm form) => View("Saved", form);

    public IActionResult WholeModel() => View(new IntakeForm());

    public IActionResult LatePrefix() => View(new IntakeForm());

    public IActionResult TaggedBadge() => View(new BadgeForm());
}

public abstract class AdmissionBase
{
    public string ResidentName { get; set; } = "";
}

public sealed class IntakeForm : AdmissionBase
{
    [JsonPropertyName("unit")]
    public string? Ward { get; set; }

    public Kin NextOfKin { get; set; } = new();

    public sealed class Kin
    {
        public string Name { get; set; } = "";
    }
}

public sealed class BadgeForm
{
    public Tagged<string> Badge { get; set; } = new();
}

public sealed class Tagged<TValue>
{
    public TValue? Value { get; set; }
}
