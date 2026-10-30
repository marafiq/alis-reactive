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

    /// <summary>An escort request whose rules sit under a condition the browser cannot read, the one the address names.</summary>
    public IActionResult EscortRefused(EscortUnreadable unreadable = EscortUnreadable.LengthOfText)
    {
        ViewData["Unreadable"] = unreadable;
        return View(new EscortRequest());
    }

    public IActionResult RulesReadBack() => View(new VisitBooking());

    public IActionResult RuleSetRefused(RuleSetMistake mistake) => View(new RuleSetRefusalsPage { Mistake = mistake });

    public IActionResult Notice() => View(new Notice());

    public IActionResult Bounds() => View(new BoundsForm());

    /// <summary>The bounds page, checked by a range written with its bounds the wrong way round.</summary>
    public IActionResult BoundsRefused() => View(new BoundsForm());

    public IActionResult Sameness() => View(new SamenessForm());

    public IActionResult Guest() => View(new RegisteredGuest());

    public IActionResult UnregisteredGuest() => View(new UnregisteredGuest());

    public IActionResult Incident() => View(new IncidentReport());

    public IActionResult Outing() => View(new ResidentOuting());

    /// <summary>The outing signed out from a card whose sign-out button carries a plan of its own.</summary>
    public IActionResult OutingCard() => View(new ResidentOuting());

    /// <summary>Signs the resident out, or refuses with every message by the member's binding name.</summary>
    [HttpPost("/Validation/Outing")]
    public IActionResult SignOutForAnOuting([FromBody] ResidentOuting outing)
    {
        var errors = new Dictionary<string, string[]>();
        if (outing.Resident is not ("Ada Lovelace" or "Tom Kilburn"))
            errors[nameof(ResidentOuting.Resident)] = ["No resident of that name lives here."];
        if (string.IsNullOrWhiteSpace(outing.BackBy))
            errors[nameof(ResidentOuting.BackBy)] = ["Say when the resident will be back."];
        return errors.Count > 0
            ? BadRequest(new { errors })
            : Json(new Booked { Summary = $"{outing.Resident} signed out with {outing.Companion}" });
    }

    /// <summary>Checks the report by its rules; 400 with every message by field, or filed with the photos it bound.</summary>
    [HttpPost("/Validation/Incident")]
    public IActionResult FileIncident([FromForm] IncidentReport report, [FromServices] IncidentReportRules rules)
    {
        var result = rules.Validate(report);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = "filed with " + string.Join(", ", report.Photos!.Select(photo => photo.FileName)) });
    }

    public IActionResult Unphotographed() => View(new UnphotographedReport());

    /// <summary>Checks the report by its rules; 400 with every message by field, or filed with no photos.</summary>
    [HttpPost("/Validation/Unphotographed")]
    public IActionResult FileUnphotographed([FromForm] UnphotographedReport report, [FromServices] UnphotographedReportRules rules)
    {
        var result = rules.Validate(report);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = "filed without photos" });
    }

    public IActionResult Ride() => View(new RideRequest());

    /// <summary>Checks the ride by its rules; 400 with every message by field, or booked.</summary>
    [HttpPost("/Validation/Ride")]
    public IActionResult BookRide([FromBody] RideRequest ride, [FromServices] RideRequestRules rules)
    {
        var result = rules.Validate(ride);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = $"booked from wing {ride.Wing} at {ride.PickUp:hh\\:mm}" });
    }

    public IActionResult GuestMeal() => View(new GuestMealOrder());

    /// <summary>Checks the meal by its rules; 400 with every message by field, or ordered from the menu it bound.</summary>
    [HttpPost("/Validation/GuestMeal")]
    public IActionResult OrderGuestMeal([FromBody] GuestMealOrder meal, [FromServices] GuestMealRules rules)
    {
        var result = rules.Validate(meal);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = $"ordered from the {meal.Menu} menu" });
    }

    public IActionResult Handover() => View(new HandoverNote());

    /// <summary>A handover whose one form a second request validates by another validator, the one the address names.</summary>
    public IActionResult HandoverRefused(HandoverMistake mistake)
    {
        ViewData["Mistake"] = mistake;
        return View(new HandoverNote());
    }

    /// <summary>Checks the draft by the handover's rules; 400 with every message by field, or saved.</summary>
    [HttpPost("/Validation/Handover/Draft")]
    public IActionResult SaveHandoverDraft([FromBody] HandoverNote note, [FromServices] HandoverRules rules)
    {
        var result = rules.Validate(note);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = $"draft saved for {note.Resident}" });
    }

    /// <summary>Checks the handover by the same rules; 400 with every message by field, or handed over.</summary>
    [HttpPost("/Validation/Handover")]
    public IActionResult HandOver([FromBody] HandoverNote note, [FromServices] HandoverRules rules)
    {
        var result = rules.Validate(note);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = $"handed over for {note.Resident}" });
    }

    /// <summary>Checks the guest by the generic rules registered for it; 400 with every message by field, or saved.</summary>
    [HttpPost("/Validation/Guest")]
    public IActionResult SaveGuest([FromBody] RegisteredGuest guest, [FromServices] GuestNameRules<RegisteredGuest> rules)
    {
        var result = rules.Validate(guest);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = $"saved {guest.Name}" });
    }

    /// <summary>Checks the guest by a rule that reads through a null, and says what the validator said of it.</summary>
    [HttpPost("/Validation/GuestUnguarded")]
    public IActionResult SaveGuestUnguarded([FromBody] RegisteredGuest guest, [FromServices] UnguardedGuestRules rules)
    {
        try
        {
            rules.Validate(guest);
            return Json(new Booked { Summary = "saved" });
        }
        catch (NullReferenceException unguarded)
        {
            return Json(new Booked { Summary = unguarded.Message });
        }
    }

    /// <summary>Checks the guest by a rule that waits, without waiting for it, and says what the validator said of it.</summary>
    [HttpPost("/Validation/GuestUnwaited")]
    public IActionResult SaveGuestUnwaited([FromBody] RegisteredGuest guest, [FromServices] WaitingGuestRules rules)
    {
        try
        {
            rules.Validate(guest);
            return Json(new Booked { Summary = "saved" });
        }
        catch (InvalidOperationException refused)
        {
            return Json(new Booked { Summary = refused.Message });
        }
    }

    /// <summary>Checks the comparisons with the same rules the page carries; 400 with every message by field, or saved.</summary>
    [HttpPost("/Validation/Sameness")]
    public IActionResult SaveSameness([FromBody] SamenessForm form, [FromServices] SamenessRules rules)
    {
        var result = rules.Validate(form);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = "saved" });
    }

    /// <summary>Checks the bounds with the same rules the page carries; 400 with every message by field, or saved.</summary>
    [HttpPost("/Validation/Bounds")]
    public IActionResult SaveBounds([FromBody] BoundsForm form, [FromServices] BoundsRules rules)
    {
        var result = rules.Validate(form);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = "saved" });
    }

    /// <summary>Checks the notice with the same rules the page carries, one of which waits; 400 with every message by field, or saved.</summary>
    [HttpPost("/Validation/Notice")]
    public async Task<IActionResult> SaveNotice([FromBody] Notice notice, [FromServices] NoticeRules rules)
    {
        var result = await rules.ValidateAsync(notice);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = "saved" });
    }

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

    /// <summary>Checks the visit and answers with every failure as a line of text, as a log would write it.</summary>
    [HttpPost("/Validation/Book/Failures")]
    public IActionResult BookFailures([FromBody] VisitBooking booking, [FromServices] VisitRules rules) =>
        Json(new Booked { Summary = string.Join("; ", rules.Validate(booking).Errors) });
}
