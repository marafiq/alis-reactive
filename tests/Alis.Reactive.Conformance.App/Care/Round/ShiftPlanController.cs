using System.Globalization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ShiftPlanController : Controller
{
    private const string PlainText = "text/plain";
    private const string NoStart = "none";

    [HttpGet(CarePages.ShiftPlan)]
    public IActionResult OfTheRota() => View("/Care/Round/ShiftPlan.cshtml", new ShiftPlan());

    [HttpPost(CareEndpoints.ShiftPlanShifts)]
    public IActionResult Save([FromForm] ShiftPlan shift)
    {
        var validation = this.Validate<ShiftPlan, ShiftPlanRules>(shift);
        if (!validation.IsValid) return validation.ToErrorResponse();
        var starts = shift.Starts is { } start ? start.ToString("HH:mm", CultureInfo.InvariantCulture) : NoStart;
        return Content(starts + ", " + shift.Length.ToString("c", CultureInfo.InvariantCulture), PlainText);
    }
}

public sealed class ShiftPlan
{
    public TimeOnly? Starts { get; set; }

    public TimeSpan Length { get; set; }
}

public sealed class ShiftPlanRules : ReactiveValidator<ShiftPlan>
{
    public ShiftPlanRules()
    {
        RuleFor(shift => shift.Starts).GreaterThanOrEqualTo(new TimeOnly(7, 0)).WithMessage("Shifts start at seven or later.");
        RuleFor(shift => shift.Length).LessThanOrEqualTo(TimeSpan.FromHours(23)).WithMessage("A shift lasts 23 hours at most.");
    }
}
