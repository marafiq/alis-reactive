using System.Globalization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class VisitBookingsController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(RefusalPages.VisitBooking)]
    public IActionResult Book() => View("/Refusals/ArrayLambdas/VisitBooking.cshtml", new VisitBooking());

    [HttpPost(VisitBookingEndpoints.Visits)]
    public IActionResult Save([FromForm] VisitBooking visit) => Content("Booked " + visit.At.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), PlainText);
}

public sealed class VisitBooking
{
    public DateTime At { get; set; }
}

public sealed class VisitValidator : ReactiveValidator<VisitBooking>
{
    public VisitValidator() => RuleFor(v => v.At).GreaterThan(new DateTime(2026, 11, 1)).WithMessage("Book a visit after November 1.");
}

public static class VisitBookingEndpoints
{
    public const string Visits = RefusalPages.VisitBooking + "/visits";
}
