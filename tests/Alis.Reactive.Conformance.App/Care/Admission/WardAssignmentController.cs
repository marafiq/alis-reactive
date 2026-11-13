using Alis.Reactive.Conformance.World;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WardAssignmentController : Controller
{
    private const string PlainText = "text/plain";
    private const string WardRequired = "Ward is required.";

    [HttpGet(CarePages.WardAssignment)]
    public IActionResult OfAResident() => View("/Care/Admission/WardAssignment.cshtml", new WardAssignmentPage());

    [HttpPost(WardAssignmentEndpoints.Assignments)]
    public IActionResult Assign([FromForm] WardAssignment assignment)
    {
        if (!string.IsNullOrEmpty(assignment.Ward)) return Content(assignment.Ward, PlainText);
        ModelState.AddModelError(nameof(WardAssignment.Ward), WardRequired);
        return ValidationProblem(ModelState);
    }
}

public sealed class WardAssignmentPage
{
    public string? Ward { get; set; }
}

public sealed class WardAssignment
{
    public string? Ward { get; set; }
}

public sealed class WardAssignmentRules : ReactiveValidator<WardAssignment>
{
    public WardAssignmentRules() => RuleFor(assignment => assignment.Ward).NotEmpty().WithMessage("Ward is required.").ServerOnly();
}

public static class WardAssignmentEndpoints
{
    public const string Assignments = CarePages.WardAssignment + "/assignments";
}
