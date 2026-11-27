using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RoomChoicesController : Controller
{
    [HttpGet(CarePages.RoomReferral)]
    public IActionResult BookedReferral() => View("/Care/AdmissionClinical/RoomAllocation.cshtml", new RoomAllocation(Assign: true, InitiallyChosen: false, Cleaning: false, InitiallyDisabled: false));

    [HttpGet(CarePages.RoomReferralReleased)]
    public IActionResult ReleasedReferral() => View("/Care/AdmissionClinical/RoomAllocation.cshtml", new RoomAllocation(Assign: false, InitiallyChosen: true, Cleaning: false, InitiallyDisabled: false));

    [HttpGet(CarePages.RoomCleaning)]
    public IActionResult Cleaning() => View("/Care/AdmissionClinical/RoomAllocation.cshtml", new RoomAllocation(Assign: false, InitiallyChosen: false, Cleaning: true, InitiallyDisabled: false));

    [HttpGet(CarePages.RoomCleaned)]
    public IActionResult ReleasedFromCleaning() => View("/Care/AdmissionClinical/RoomAllocation.cshtml", new RoomAllocation(Assign: true, InitiallyChosen: false, Cleaning: false, InitiallyDisabled: true));

    [HttpGet(CarePages.RespiteIncluded)]
    public IActionResult RespiteAlternative() => View("/Care/AdmissionClinical/RoomAlternatives.cshtml", new RoomAlternatives(IncludeRespite: true));

    [HttpGet(CarePages.RespiteIndependent)]
    public IActionResult RespiteStay() => View("/Care/AdmissionClinical/RoomAlternatives.cshtml", new RoomAlternatives(IncludeRespite: false));

    [HttpGet(CarePages.AdmissionRoomBooking)]
    public IActionResult Booking() => View("/Care/AdmissionClinical/RoomBooking.cshtml", new AdmissionRoomBooking(Contract: false));

    [HttpGet(CarePages.RoomContractBooking)]
    public IActionResult ContractBooking() => View("/Care/AdmissionClinical/RoomBooking.cshtml", new AdmissionRoomBooking(Contract: true));

    [HttpGet(CarePages.RoomCard)]
    public IActionResult ResidentRoomCard() => View("/Care/AdmissionClinical/RoomCard.cshtml", new RoomCard());

    [HttpGet(CarePages.RoomAvailabilityHeld)]
    public IActionResult RoomUnderCleaning() => View("/Care/AdmissionClinical/RoomAvailability.cshtml", new RoomAvailability(Held: true));

    [HttpGet(CarePages.RoomAvailabilityOpen)]
    public IActionResult AvailableRoom() => View("/Care/AdmissionClinical/RoomAvailability.cshtml", new RoomAvailability(Held: false));

    [HttpGet(CarePages.RoomVisit)]
    public IActionResult Provisional() => View("/Care/AdmissionClinical/RoomVisit.cshtml", new RoomVisit(RestoreChoice: false));

    [HttpGet(CarePages.RoomNextVisit)]
    public IActionResult NextVisit() => View("/Care/AdmissionClinical/RoomVisit.cshtml", new RoomVisit(RestoreChoice: true));

    [HttpGet(CarePages.RoomPreview)]
    public IActionResult Preview() => View("/Care/AdmissionClinical/RoomPreview.cshtml", new RoomPreview());

    [HttpGet(CarePages.RoomCardPrinting)]
    public IActionResult PrintCard() => View("/Care/AdmissionClinical/RoomCardPrinting.cshtml", new RoomCardPrinting());

    [HttpGet(CarePages.SavedArabicRoomCard)]
    public IActionResult ArabicEdition() => View("/Care/AdmissionClinical/SavedRoomCard.cshtml", new SavedRoomCard(Arabic: true));

    [HttpGet(CarePages.SavedEnglishRoomCard)]
    public IActionResult EnglishEdition() => View("/Care/AdmissionClinical/SavedRoomCard.cshtml", new SavedRoomCard(Arabic: false));

    [HttpGet(CarePages.RoomInventory)]
    public IActionResult AvailableInventory() => View("/Care/AdmissionClinical/RoomInventory.cshtml", new RoomInventory("Rose Alvarez"));

    [HttpPost(CarePages.AdmissionRoomBooking)]
    public IActionResult ConfirmBooking([FromForm] string? bed) => View("/Care/AdmissionClinical/RoomBookingReceipt.cshtml",
        new RoomBookingReceipt(bed switch
        {
            "garden" => "Garden wing reserved",
            "contract" => "Contract bed reserved",
            _ => "Room code awaiting review"
        }));
}

public sealed record RoomAllocation(bool Assign, bool InitiallyChosen, bool Cleaning, bool InitiallyDisabled);
public sealed record RoomAlternatives(bool IncludeRespite);
public sealed record AdmissionRoomBooking(bool Contract);
public sealed record RoomBookingReceipt(string Description);
public sealed record RoomInventory(string Resident);
public sealed record RoomCard;
public sealed record RoomAvailability(bool Held);
public sealed record RoomVisit(bool RestoreChoice);
public sealed record RoomPreview;
public sealed record RoomCardPrinting;
public sealed record SavedRoomCard(bool Arabic);
