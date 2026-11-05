using Alis.Reactive.SandboxApp.BroughtComponents;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.AdmissionDecision;

public class AdmissionDecisionModel
{
    public const string SheetId = "intake";

    public bool DirectorApproved { get; set; }

    public string Bed { get; set; } = "";

    public Applicant Applicant { get; set; } = new Applicant
    {
        Name = "Margaret Hale",
        Age = 62,
        MoveInDate = new DateTime(2026, 7, 1),
        Veteran = true,
        CareLevel = CareLevel.Assisted,
        MonthlyRate = 5000.00m,
        PetCount = 1,
        Contact = new ApplicantContact { Phone = "555-0101", Relation = "Daughter" },
        Allergies = ["Penicillin"],
    };

    public int WaitlistPosition { get; set; } = 2;
}

public static class Beds
{
    public const string Garden = "Garden 214";
    public const string Maple = "Maple 118";
}
