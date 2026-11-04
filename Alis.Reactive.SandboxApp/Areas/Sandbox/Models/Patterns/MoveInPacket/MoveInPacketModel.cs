
namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Patterns.MoveInPacket
{
    public class MoveInPacketModel
    {
        public string ResidentId { get; set; } = "";

        public string ResidentName { get; set; } = "";

        public string FamilyPhone { get; set; } = "";

        public string CareLevel { get; set; } = "";

        public string Mobility { get; set; } = "";

        public bool HasAllergies { get; set; }

        public string[] Allergies { get; set; } = [];

        public string[] Diet { get; set; } = [];

        public string CareNotes { get; set; } = "";

        public string CarePlan { get; set; } = "";

        public DateTime? MoveInDay { get; set; }

        public DateTime? MedicationTime { get; set; }

        public DateTime? PhysicianVisit { get; set; }

        public DateTime[]? TrialStay { get; set; }

        public string Community { get; set; } = "";

        public string Insurance { get; set; } = "";

        public string Physician { get; set; } = "";

        public decimal MonthlyRate { get; set; }

        public bool TextFamilyUpdates { get; set; }
    }

    public sealed record PacketCommunity(string Code, string Name)
    {
        public static readonly PacketCommunity[] All =
        [
            new("main", "Maple Grove Main Campus"),
            new("cottages", "The Cottages at Maple Grove"),
            new("west", "Maple Grove West")
        ];
    }

    public sealed record PacketInsurer(string Code, string Name, string Kind)
    {
        public static readonly PacketInsurer[] All =
        [
            new("medicare", "Medicare", "Government"),
            new("bluecross", "Blue Cross", "Private"),
            new("aetna", "Aetna", "Private")
        ];
    }

    public sealed record PacketDiet(string Code, string Name)
    {
        public static readonly PacketDiet[] All =
        [
            new("low-sodium", "Low sodium"),
            new("diabetic", "Diabetic"),
            new("soft", "Soft foods"),
            new("vegetarian", "Vegetarian")
        ];
    }

    public class MoveInPacketValidator : ReactiveValidator<MoveInPacketModel>
    {
        public MoveInPacketValidator()
        {
            RuleFor(x => x.ResidentName).NotEmpty().WithMessage("Enter the resident's name.");
        }
    }

    public sealed record PacketReceipt(string Status, string Resident, string Care, string Schedule, string Billing);
}
