using Alis.Reactive.SandboxApp.BroughtComponents;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.RespiteScreening;

public class RespiteScreeningModel
{
    public const string SheetId = "intake";

    public string[] Services { get; set; } = [];

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

public static class RespiteServices
{
    public const string Transport = "Transport";
    public const string Laundry = "Laundry";
    public const string Meals = "Meals";
}

public sealed class WingPolicy
{
    public string ExpectedResident { get; set; } = "";

    public int MinimumAge { get; set; }

    public DateTime OpensOn { get; set; }

    public bool VeteranOnRecord { get; set; }

    public CareLevel CareLevel { get; set; }

    public decimal MonthlyRate { get; set; }

    public string? RoomOffered { get; set; }

    public int? PetLimit { get; set; }

    public DateTime? LastStayOnRecord { get; set; }

    public bool? FluShotOnRecord { get; set; }

    public Diet? DietOnRecord { get; set; }
}
