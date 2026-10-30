using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Patterns.MoveInPacket
{
    /// <summary>
    /// The move-in packet an admissions coordinator completes for a new resident: one answer from each kind
    /// of field the community's forms use.
    /// </summary>
    /// <remarks>
    /// A day, a time, a visit or a stay not yet chosen has no value a default could stand for, so those
    /// members are nullable and their pickers open empty; every other member starts empty.
    /// </remarks>
    public class MoveInPacketModel
    {
        /// <summary>The resident's record number, carried by the packet and never typed.</summary>
        public string ResidentId { get; set; } = "";

        public string ResidentName { get; set; } = "";

        /// <summary>The family's phone, its ten digits without the mask's punctuation.</summary>
        public string FamilyPhone { get; set; } = "";

        /// <summary>The care level's code: independent, assisted or memory.</summary>
        public string CareLevel { get; set; } = "";

        /// <summary>How the resident gets about: unaided, walker or wheelchair.</summary>
        public string Mobility { get; set; } = "";

        public bool HasAllergies { get; set; }

        public string[] Allergies { get; set; } = [];

        /// <summary>The codes of the diets the kitchen follows for her.</summary>
        public string[] Diet { get; set; } = [];

        public string CareNotes { get; set; } = "";

        /// <summary>The care plan as the editor writes it, in HTML.</summary>
        public string CarePlan { get; set; } = "";

        public DateTime? MoveInDay { get; set; }

        /// <summary>The time of her morning medication; only its time of day matters.</summary>
        public DateTime? MedicationTime { get; set; }

        public DateTime? PhysicianVisit { get; set; }

        /// <summary>The first and last day of her trial stay.</summary>
        public DateTime[]? TrialStay { get; set; }

        /// <summary>The code of the community she moves into.</summary>
        public string Community { get; set; } = "";

        /// <summary>The code of the insurer that pays her care.</summary>
        public string Insurance { get; set; } = "";

        /// <summary>The attending physician, by name.</summary>
        public string Physician { get; set; } = "";

        public decimal MonthlyRate { get; set; }

        /// <summary>The family is texted when anything in her care changes.</summary>
        public bool TextFamilyUpdates { get; set; }
    }

    /// <summary>A community of Maple Grove a resident moves into.</summary>
    public sealed record PacketCommunity(string Code, string Name)
    {
        public static readonly PacketCommunity[] All =
        [
            new("main", "Maple Grove Main Campus"),
            new("cottages", "The Cottages at Maple Grove"),
            new("west", "Maple Grove West")
        ];
    }

    /// <summary>An insurer that pays a resident's care.</summary>
    public sealed record PacketInsurer(string Code, string Name, string Kind)
    {
        public static readonly PacketInsurer[] All =
        [
            new("medicare", "Medicare", "Government"),
            new("bluecross", "Blue Cross", "Private"),
            new("aetna", "Aetna", "Private")
        ];
    }

    /// <summary>A diet the kitchen follows.</summary>
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

    /// <summary>What a packet cannot go without: the resident it is for.</summary>
    public class MoveInPacketValidator : ReactiveValidator<MoveInPacketModel>
    {
        public MoveInPacketValidator()
        {
            RuleFor(x => x.ResidentName).NotEmpty().WithMessage("Enter the resident's name.");
        }
    }

    /// <summary>
    /// What the side that took the packet received, one sentence for each part of it: the resident, her
    /// care, her schedule and her billing.
    /// </summary>
    public sealed record PacketReceipt(string Status, string Resident, string Care, string Schedule, string Billing);
}
