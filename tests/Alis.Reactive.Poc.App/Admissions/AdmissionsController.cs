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

    public IActionResult LatePrefixRead() => View(new IntakeForm());

    public IActionResult ResidentTwice() => View(new IntakeForm());

    public IActionResult TaggedBadge() => View(new BadgeForm());

    public IActionResult Allergies() => View(new IntakeForm { Allergies = ["penicillin", "latex"] });

    public IActionResult Aids() => View(new IntakeForm { Aids = ["walking frame", "hearing aid"] });

    public IActionResult FirstVisitor() => View(new IntakeForm { Visitors = [new IntakeForm.Kin { Name = "Ada Byron" }] });

    public IActionResult VisitorsByForeach() => View(new IntakeForm { Visitors = [new IntakeForm.Kin { Name = "Ada Byron" }] });

    public IActionResult ShortenedResident() => View(new IntakeForm());

    public IActionResult PrimaryVisitor() => View(new IntakeForm { Visitors = [new IntakeForm.Kin { Name = "Ada Byron" }] });

    public IActionResult Preferences() => View(new IntakeForm { ResidentName = "Helen Park", Preferences = new() { ["visiting"] = "afternoons" } });
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

    public List<string> Allergies { get; set; } = [];

    public string[] Aids { get; set; } = [];

    public List<Kin> Visitors { get; set; } = [];

    public Dictionary<string, string> Preferences { get; set; } = new();

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
