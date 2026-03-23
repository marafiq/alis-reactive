namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Validation.Contract
{
    public class ResidentModel
    {
        // Basic
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? ConfirmEmail { get; set; }

        // Care
        public string? CareLevel { get; set; }
        public int? MemoryAssessmentScore { get; set; }
        public string? PhysicianName { get; set; }

        // Veteran
        public bool IsVeteran { get; set; }
        public string? VeteranId { get; set; }

        // Emergency
        public bool HasEmergencyContact { get; set; }
        public string? EmergencyName { get; set; }
        public string? EmergencyPhone { get; set; }
        public string? ReasonForNoContact { get; set; }

        // Address
        public string? AddressType { get; set; }
        public ResidentAddress Address { get; set; } = new();
    }

    public class ResidentAddress
    {
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? ZipCode { get; set; }
    }
}
