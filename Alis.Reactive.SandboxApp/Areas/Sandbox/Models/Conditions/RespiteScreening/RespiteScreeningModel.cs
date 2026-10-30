using Alis.Reactive.SandboxApp.BroughtComponents;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.RespiteScreening;

/// <summary>
/// Journey: the admissions coordinator screens an applicant for a respite stay against the community's
/// rules, fact by fact, and against the Garden wing's policy. The facts live on an intake sheet the
/// sandbox wrote itself (BroughtComponents/IntakeSheet); each rule reads one of them.
/// </summary>
public class RespiteScreeningModel
{
    /// <summary>The id of the applicant's intake sheet on the page.</summary>
    public const string SheetId = "intake";

    /// <summary>The services the family asked for.</summary>
    public string[] Services { get; set; } = [];

    /// <summary>The applicant as the waiting list holds her: on the edge of most rules the page checks.</summary>
    public Applicant Applicant { get; set; } = new Applicant
    {
        Name = "Margaret Hale",
        Age = 62,
        MoveInDate = new DateTime(2026, 7, 1),
        Veteran = true,
        CareLevel = CareLevel.Assisted,
        MonthlyRate = 5000.00m,
        RoomPreference = null,
        PetCount = 1,
        PriorStayEnded = null,
        FluShot = null,
        Diet = null,
        Contact = new ApplicantContact { Phone = "555-0101", Relation = "Daughter" },
        Allergies = ["Penicillin"],
    };
}

/// <summary>The services a family can ask for, in the words the coordinator ticks.</summary>
public static class RespiteServices
{
    public const string Transport = "Transport";
    public const string Laundry = "Laundry";
    public const string Meals = "Meals";
}

/// <summary>The Garden wing's policy, from the Policy endpoint: what the wing expects of a respite applicant.</summary>
public sealed class WingPolicy
{
    public string ExpectedResident { get; set; } = "";

    public int MinimumAge { get; set; }

    public DateTime OpensOn { get; set; }

    /// <summary>What the VA record says of the applicant.</summary>
    public bool VeteranOnRecord { get; set; }

    public CareLevel CareLevel { get; set; }

    public decimal MonthlyRate { get; set; }

    /// <summary>The room the wing offers, or nothing when none is free.</summary>
    public string? RoomOffered { get; set; }

    /// <summary>How many pets the wing allows, or nothing when it has no limit.</summary>
    public int? PetLimit { get; set; }

    /// <summary>When the applicant's last stay ended, as the community's record has it.</summary>
    public DateTime? LastStayOnRecord { get; set; }

    /// <summary>Whether the clinic recorded this season's flu shot.</summary>
    public bool? FluShotOnRecord { get; set; }

    /// <summary>The diet the dietitian recorded.</summary>
    public Diet? DietOnRecord { get; set; }
}
