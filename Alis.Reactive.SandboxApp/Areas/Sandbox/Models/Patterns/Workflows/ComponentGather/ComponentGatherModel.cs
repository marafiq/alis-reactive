namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ComponentGatherModel
    {
        public string? ResidentId { get; set; }
        public string? FormToken { get; set; }
        public string? ResidentName { get; set; }
        public string? CareNotes { get; set; }
        public bool HasAllergies { get; set; }
        public string? MobilityLevel { get; set; }
        public string? CareLevel { get; set; }
        public string[]? Allergies { get; set; }
        public decimal MonthlyRate { get; set; }
        public string? FacilityId { get; set; }
        public string? PhysicianName { get; set; }
        public DateTime? AdmissionDate { get; set; }
        public DateTime? MedicationTime { get; set; }
        public DateTime? AppointmentTime { get; set; }
        public DateTime[]? StayPeriod { get; set; }
        public string? InsuranceProvider { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CarePlan { get; set; }
        public bool ReceiveNotifications { get; set; }
        public string[]? DietaryRestrictions { get; set; }
    }

    public class GatherEchoResponse
    {
        public string? ResidentId { get; set; }
        public string? FormToken { get; set; }
        public string? ResidentName { get; set; }
        public string? CareNotes { get; set; }
        public bool HasAllergies { get; set; }
        public string? MobilityLevel { get; set; }
        public string? CareLevel { get; set; }
        public string[]? Allergies { get; set; }
        public decimal MonthlyRate { get; set; }
        public string? FacilityId { get; set; }
        public string? PhysicianName { get; set; }
        public string? AdmissionDate { get; set; }
        public string? MedicationTime { get; set; }
        public string? AppointmentTime { get; set; }
        public string[]? StayPeriod { get; set; }
        public string? InsuranceProvider { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CarePlan { get; set; }
        public bool ReceiveNotifications { get; set; }
        public string[]? DietaryRestrictions { get; set; }
        public int FieldCount { get; set; }
    }

    public class GatherFacilityItem
    {
        public string Value { get; set; } = "";
        public string Text { get; set; } = "";
    }

    public class GatherPhysicianItem
    {
        public string Value { get; set; } = "";
        public string Text { get; set; } = "";
    }

    public class GatherInsuranceItem
    {
        public string Value { get; set; } = "";
        public string Text { get; set; } = "";
        public string Category { get; set; } = "";
    }

    public class GatherDietaryItem
    {
        public string Value { get; set; } = "";
        public string Text { get; set; } = "";
    }
}
