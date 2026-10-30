using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.MedicationRound;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Conditions;

[Area("Sandbox")]
[Route("Sandbox/Conditions/MedicationRound")]
public class MedicationRoundController : Controller
{
    private static readonly DateTime RoundDate = new(2026, 6, 2);

    [HttpGet("")]
    public IActionResult Index()
    {
        return View("~/Areas/Sandbox/Views/Conditions/MedicationRound/Index.cshtml", new MedicationRoundModel
        {
            RoundDate = RoundDate,
            Doses = DosesDue()
        });
    }

    // The Maple wing's 08:00 doses on the round's day, as the eMAR lists them.
    private static List<RoundDose> DosesDue() =>
    [
        new()
        {
            Resident = "Margaret Hale", Room = "214", Medication = "Metoprolol 25 mg tablet",
            OnWing = true, Pulse = 60, Temperature = 98.2, MinutesSinceLastDose = 1440, Status = "Due",
            Order = new RoundDoseOrder { SignedBy = "Dr. Okafor", StopDate = new DateTime(2026, 9, 1), Route = "By mouth" },
            Allergies = "Penicillin"
        },
        new()
        {
            Resident = "Walter Brooks", Room = "216", Medication = "Digoxin 125 mcg tablet",
            OnWing = true, Pulse = 59, Temperature = 100.4, MinutesSinceLastDose = 1440, Status = "Due",
            Order = new RoundDoseOrder { SignedBy = "Dr. Okafor", StopDate = new DateTime(2026, 6, 3), Route = "By mouth" }
        },
        new()
        {
            Resident = "Evelyn Hart", Room = "218", Medication = "Carvedilol 6.25 mg tablet",
            OnWing = true, Pulse = 100, Temperature = 100.5, MinutesSinceLastDose = 240, Status = "Due",
            Order = new RoundDoseOrder { SignedBy = "Dr. Patel", StopDate = new DateTime(2026, 7, 15), Route = "By mouth" },
            Allergies = ""
        },
        new()
        {
            Resident = "Robert Miles", Room = "220", Medication = "Insulin glargine 20 units",
            OnWing = true, TimeCritical = true, Pulse = 101, Temperature = 100.3, MinutesSinceLastDose = 1440, Status = "Due",
            Order = new RoundDoseOrder { SignedBy = "Dr. Patel", StopDate = new DateTime(2026, 8, 1), Route = "Under the skin" }
        },
        new()
        {
            Resident = "Rose Delgado", Room = "222", Medication = "Oxycodone 5 mg tablet, as needed for pain",
            OnWing = true, Pulse = 72, Temperature = 98.6, MinutesSinceLastDose = 239, Status = "Due",
            Order = new RoundDoseOrder { StopDate = new DateTime(2026, 6, 9) },
            Allergies = "Codeine", ForPain = true
        },
        new()
        {
            Resident = "Harold Kim", Room = "224", Medication = "Acetaminophen 650 mg, as needed for pain",
            OnWing = true, Pulse = 88, Temperature = 99.1, MinutesSinceLastDose = 240, Status = "Due",
            Allergies = "Latex", ForPain = true, PainScore = 0
        },
        new()
        {
            Resident = "Agnes Moore", Room = "226", Medication = "Carbidopa-levodopa 25/100 mg tablet",
            TimeCritical = true, Pulse = 76, Temperature = 97.9, MinutesSinceLastDose = 300, Status = "Due",
            Order = new RoundDoseOrder { SignedBy = "Dr. Okafor", StopDate = new DateTime(2026, 6, 1), Route = "By mouth" }
        },
        new()
        {
            Resident = "Ida Bell", Room = "228", Medication = "Lisinopril 10 mg tablet",
            Pulse = 80, Temperature = 98.0, MinutesSinceLastDose = 1440, Status = "Due",
            Order = new RoundDoseOrder { SignedBy = "Dr. Patel", StopDate = new DateTime(2026, 8, 15), Route = "By mouth" }
        },
        new()
        {
            Resident = "Frank Ortiz", Room = "230", Medication = "Warfarin 2 mg tablet",
            OnWing = true, Pulse = 70, Temperature = 98.4, MinutesSinceLastDose = 1440, Status = "On hold",
            Order = new RoundDoseOrder { SignedBy = "Dr. Patel", StopDate = new DateTime(2026, 7, 1), Route = "By mouth" },
            Allergies = "Aspirin"
        },
        new()
        {
            Resident = "Clara Nguyen", Room = "232", Medication = "Levothyroxine 50 mcg tablet",
            OnWing = true, Pulse = 66, Temperature = 95.9, MinutesSinceLastDose = 1440, Status = "Not available",
            Order = new RoundDoseOrder { SignedBy = "Dr. Okafor", StopDate = new DateTime(2026, 10, 1), Route = "By mouth" }
        },
        new()
        {
            Resident = "Henry Walsh", Room = "234", Medication = "Furosemide 20 mg tablet",
            OnWing = true, Pulse = 84, Temperature = 96.0, MinutesSinceLastDose = 1440, Status = "Due",
            Order = new RoundDoseOrder { SignedBy = "Dr. Patel", StopDate = new DateTime(2026, 6, 2), Route = "By mouth" }
        }
    ];
}
