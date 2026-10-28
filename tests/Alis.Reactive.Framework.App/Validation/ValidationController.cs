using Alis.Reactive.FluentValidator;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Validation;

/// <summary>A form whose rules are written once and checked on both sides: the browser what it can, the server all.</summary>
public sealed class ValidationController : Controller
{
    public IActionResult Visit() => View(new VisitBooking());

    public IActionResult Checks() => View(new CheckedForm());

    public IActionResult Household() => View(new Household());

    public IActionResult Escort() => View(new EscortRequest());

    public IActionResult EscortRefused() => View(new EscortRequest());

    /// <summary>Checks the escort request with the same rules the page carries; 400 with every message by field, or saved.</summary>
    [HttpPost("/Validation/Escort")]
    public IActionResult SaveEscort([FromBody] EscortRequest request, [FromServices] EscortRules rules)
    {
        var result = rules.Validate(request);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = "saved" });
    }

    /// <summary>Checks the household with the same rules the page carries; 400 with every message by field, or saved.</summary>
    [HttpPost("/Validation/Household")]
    public IActionResult SaveHousehold([FromBody] Household household, [FromServices] HouseholdRules rules)
    {
        var result = rules.Validate(household);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = "saved" });
    }

    /// <summary>Checks the form with the same rules the page carries; 400 with every message by field, or saved.</summary>
    [HttpPost("/Validation/Checks")]
    public IActionResult SaveChecks([FromBody] CheckedForm form, [FromServices] CheckRules rules)
    {
        var result = rules.Validate(form);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = "saved" });
    }

    /// <summary>Checks the booking with the same rules the page carries, and answers 400 with every message by field.</summary>
    [HttpPost("/Validation/Book")]
    public IActionResult Book([FromBody] VisitBooking booking, [FromServices] VisitRules rules)
    {
        var result = rules.Validate(booking);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = $"booked {booking.Visitor} for {booking.Guests} guest(s)" });
    }
}
