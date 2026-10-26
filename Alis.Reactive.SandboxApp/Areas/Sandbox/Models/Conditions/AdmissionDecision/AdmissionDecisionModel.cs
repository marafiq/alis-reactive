using Alis.Reactive.SandboxApp.BroughtComponents;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.AdmissionDecision;

/// <summary>
/// Journey: the admissions director decides an applicant's respite stay. The rules combine, the decision
/// takes the first reason that fits, and where the waiting list or the director must be asked, the page
/// waits for the answer. The applicant's facts live on an intake sheet the sandbox wrote itself
/// (BroughtComponents/IntakeSheet), which also keeps her waiting-list number and counts how often it is asked.
/// </summary>
public class AdmissionDecisionModel
{
    /// <summary>The id of the applicant's intake sheet on the page.</summary>
    public const string SheetId = "intake";

    /// <summary>Whether the director has approved the stay.</summary>
    public bool DirectorApproved { get; set; }

    /// <summary>The bed offered, if any.</summary>
    public string Bed { get; set; } = "";

    /// <summary>Margaret Hale, second on the waiting list: of age, a veteran, assisted living.</summary>
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

    /// <summary>Where the waiting list places her when the page opens.</summary>
    public int WaitlistPosition { get; set; } = 2;
}

/// <summary>The beds the director can offer.</summary>
public static class Beds
{
    public const string Garden = "Garden 214";
    public const string Maple = "Maple 118";
}
