using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Validation;

public sealed class ValidationController : Controller
{
    public IActionResult Visit() => View(new VisitBooking());

    public IActionResult Checks() => View(new CheckedForm());

    public IActionResult Household() => View(new Household());

    public IActionResult Escort() => View(new EscortRequest());

    public IActionResult EscortRefused(EscortUnreadable unreadable = EscortUnreadable.LengthOfText)
    {
        ViewData["Unreadable"] = unreadable;
        return View(new EscortRequest());
    }

    public IActionResult RulesReadBack() => View(new VisitBooking());

    public IActionResult RuleSetRefused(RuleSetMistake mistake) => View(new RuleSetRefusalsPage { Mistake = mistake });

    public IActionResult Notice() => View(new Notice());

    public IActionResult Bounds() => View(new BoundsForm());

    public IActionResult BoundsRefused() => View(new BoundsForm());

    public IActionResult Sameness() => View(new SamenessForm());

    public IActionResult Guest() => View(new RegisteredGuest());

    public IActionResult UnregisteredGuest() => View(new UnregisteredGuest());

    public IActionResult Incident() => View(new IncidentReport());

    public IActionResult Outing() => View(new ResidentOuting());

    public IActionResult OutingNotes(bool placedTwice = false)
    {
        ViewData["PlacedTwice"] = placedTwice;
        return View(new ResidentOuting());
    }

    public IActionResult OutingCard() => View(new ResidentOuting());

    public IActionResult LateReturn() => View(new ResidentReturn());

    [HttpPost("/Validation/LateReturn")]
    public IActionResult SignBackIn([FromBody] ResidentReturn residentReturn, [FromServices] ResidentReturnRules rules)
    {
        var result = rules.Validate(residentReturn);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = $"{residentReturn.Resident} signed back in" });
    }

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

    [HttpPost("/Validation/Incident")]
    public IActionResult FileIncident([FromForm] IncidentReport report, [FromServices] IncidentReportRules rules)
    {
        var result = rules.Validate(report);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = "filed with " + string.Join(", ", report.Photos!.Select(photo => photo.FileName)) });
    }

    public IActionResult Unphotographed() => View(new UnphotographedReport());

    [HttpPost("/Validation/Unphotographed")]
    public IActionResult FileUnphotographed([FromForm] UnphotographedReport report, [FromServices] UnphotographedReportRules rules)
    {
        var result = rules.Validate(report);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = "filed without photos" });
    }

    public IActionResult Ride() => View(new RideRequest());

    [HttpPost("/Validation/Ride")]
    public IActionResult BookRide([FromBody] RideRequest ride, [FromServices] RideRequestRules rules)
    {
        var result = rules.Validate(ride);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = $"booked from wing {ride.Wing} at {ride.PickUp:hh\\:mm}" });
    }

    public IActionResult GuestMeal() => View(new GuestMealOrder());

    [HttpPost("/Validation/GuestMeal")]
    public IActionResult OrderGuestMeal([FromBody] GuestMealOrder meal, [FromServices] GuestMealRules rules)
    {
        var result = rules.Validate(meal);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = $"ordered from the {meal.Menu} menu" });
    }

    public IActionResult Handover() => View(new HandoverNote());

    public IActionResult HandoverRefused(HandoverMistake mistake)
    {
        ViewData["Mistake"] = mistake;
        return View(new HandoverNote());
    }

    [HttpPost("/Validation/Handover/Draft")]
    public IActionResult SaveHandoverDraft([FromBody] HandoverNote note, [FromServices] HandoverRules rules)
    {
        var result = rules.Validate(note);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = $"draft saved for {note.Resident}" });
    }

    [HttpPost("/Validation/Handover")]
    public IActionResult HandOver([FromBody] HandoverNote note, [FromServices] HandoverRules rules)
    {
        var result = rules.Validate(note);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = $"handed over for {note.Resident}" });
    }

    [HttpPost("/Validation/Guest")]
    public IActionResult SaveGuest([FromBody] RegisteredGuest guest, [FromServices] GuestNameRules<RegisteredGuest> rules)
    {
        var result = rules.Validate(guest);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = $"saved {guest.Name}" });
    }

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

    [HttpPost("/Validation/Sameness")]
    public IActionResult SaveSameness([FromBody] SamenessForm form, [FromServices] SamenessRules rules)
    {
        var result = rules.Validate(form);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = "saved" });
    }

    [HttpPost("/Validation/Bounds")]
    public IActionResult SaveBounds([FromBody] BoundsForm form, [FromServices] BoundsRules rules)
    {
        var result = rules.Validate(form);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = "saved" });
    }

    [HttpPost("/Validation/Notice")]
    public async Task<IActionResult> SaveNotice([FromBody] Notice notice, [FromServices] NoticeRules rules)
    {
        var result = await rules.ValidateAsync(notice);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = "saved" });
    }

    [HttpPost("/Validation/Escort")]
    public IActionResult SaveEscort([FromBody] EscortRequest request, [FromServices] EscortRules rules)
    {
        var result = rules.Validate(request);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = "saved" });
    }

    [HttpPost("/Validation/Household")]
    public IActionResult SaveHousehold([FromBody] Household household, [FromServices] HouseholdRules rules)
    {
        var result = rules.Validate(household);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = "saved" });
    }

    [HttpPost("/Validation/Checks")]
    public IActionResult SaveChecks([FromBody] CheckedForm form, [FromServices] CheckRules rules)
    {
        var result = rules.Validate(form);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = "saved" });
    }

    [HttpPost("/Validation/Book")]
    public IActionResult Book([FromBody] VisitBooking booking, [FromServices] VisitRules rules)
    {
        var result = rules.Validate(booking);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = $"booked {booking.Visitor} for {booking.Guests} guest(s)" });
    }

    [HttpPost("/Validation/Book/Failures")]
    public IActionResult BookFailures([FromBody] VisitBooking booking, [FromServices] VisitRules rules)
        => Json(new Booked { Summary = string.Join("; ", rules.Validate(booking).Errors) });

    public IActionResult Discharge(string? stage)
    {
        ViewData["Stage"] = stage;
        return View(new DischargeSummary());
    }

    public IActionResult DischargeRefused() => View(new DischargeSummary());

    [HttpPost("/Validation/Discharge")]
    public IActionResult SaveDischarge([FromBody] DischargeSummary summary, [FromServices] DischargeRules rules)
    {
        var result = rules.Validate(summary);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = $"saved for {summary.Resident}" });
    }

    [HttpPost("/Validation/Discharge/{stage}")]
    public IActionResult SaveDischargeStage(string stage, [FromBody] DischargeSummary summary, [FromServices] DischargeRules rules)
    {
        var result = rules.Validate(summary, stage);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = $"{stage} saved for {summary.Resident}" });
    }

    [HttpPost("/Validation/DischargeUndeclared")]
    public IActionResult SaveDischargeUndeclared([FromBody] DischargeSummary summary, [FromServices] DischargeRules rules)
    {
        try
        {
            rules.Validate(summary, "Drafts");
            return Json(new Booked { Summary = "saved" });
        }
        catch (InvalidOperationException refused)
        {
            return Json(new Booked { Summary = refused.Message });
        }
    }

    public IActionResult Transfer() => View(new WardTransfer());

    [HttpPost("/Validation/Transfer")]
    public async Task<IActionResult> SaveTransfer([FromBody] WardTransfer transfer, [FromServices] TransferRules rules, CancellationToken cancellation)
    {
        var result = await rules.ValidateAsync(transfer, cancellation);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = $"{transfer.Resident} moved to {transfer.Ward}" });
    }

    public IActionResult LinenRound() => View(new LinenRound());

    [HttpPost("/Validation/LinenRound")]
    public IActionResult SaveLinenRound([FromBody] LinenRound round, [FromServices] LinenRoundRules rules)
    {
        var result = rules.Validate(round);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = "saved" });
    }

    public IActionResult MedicationRound() => View(new MedicationRound());

    [HttpPost("/Validation/MedicationRound")]
    public IActionResult SaveMedicationRound([FromBody] MedicationRound round, [FromServices] MedicationRoundRules rules)
    {
        var result = rules.Validate(round);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = "saved" });
    }

    public IActionResult RoomMove() => View(new RoomMove());

    [HttpPost("/Validation/RoomMove")]
    public async Task<IActionResult> MoveRoom([FromBody] RoomMove move, [FromServices] RoomMoveRules rules, CancellationToken cancellation)
    {
        var result = await rules.ValidateAsync(move, cancellation);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Booked { Summary = $"{move.Resident} moved to {move.Room}" });
    }

    [HttpPost("/Validation/RoomMoveWithoutWaiting")]
    public IActionResult MoveRoomWithoutWaiting([FromBody] RoomMove move, [FromServices] RoomMoveRules rules)
    {
        try
        {
            rules.Validate(move);
            return Json(new Booked { Summary = "saved" });
        }
        catch (InvalidOperationException refused)
        {
            return Json(new Booked { Summary = refused.Message });
        }
    }

    [HttpPost("/Validation/TransferWithoutWaiting")]
    public IActionResult SaveTransferWithoutWaiting([FromBody] WardTransfer transfer, [FromServices] TransferRules rules)
    {
        try
        {
            rules.Validate(transfer);
            return Json(new Booked { Summary = "saved" });
        }
        catch (InvalidOperationException refused)
        {
            return Json(new Booked { Summary = refused.Message });
        }
    }
}
