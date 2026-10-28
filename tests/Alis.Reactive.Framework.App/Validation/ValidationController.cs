using Alis.Reactive.FluentValidator;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Validation;

/// <summary>A form whose rules are written once and checked on both sides: the browser what it can, the server all.</summary>
public sealed class ValidationController : Controller
{
    public IActionResult Visit() => View(new VisitBooking());

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
