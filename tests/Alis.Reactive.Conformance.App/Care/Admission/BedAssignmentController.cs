using Alis.Reactive.Conformance.World;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class BedAssignmentController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.BedAssignment)]
    public IActionResult OfABed() => View("/Care/Admission/BedAssignment.cshtml", new BedAssignmentPage(SendsTheWard: false));

    [HttpGet(CarePages.BedAssignmentInAWard)]
    public IActionResult OfABedInAWard() => View("/Care/Admission/BedAssignment.cshtml", new BedAssignmentPage(SendsTheWard: true));

    [HttpPost(BedAssignmentEndpoints.Assignments)]
    public IActionResult Assign([FromForm] BedPlacement assignment) => Content($"{assignment.Ward}, bed {assignment.Bed}", PlainText);
}

public sealed record BedAssignmentPage(bool SendsTheWard);

public sealed class BedPlacement
{
    public string? Ward { get; set; }

    public int Bed { get; set; }
}

public sealed class BedAssignmentRules : ReactiveValidator<BedPlacement>
{
    public BedAssignmentRules() => RuleFor(assignment => assignment.Ward).NotEmpty().WithMessage("Ward is required.");
}

public static class BedAssignmentEndpoints
{
    public const string Assignments = CarePages.BedAssignment + "/assignments";
}
