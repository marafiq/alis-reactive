using System.Collections.Generic;

namespace Alis.Reactive.Net48.SmokeTest.Models
{
    public class ResidentIntakeModel
    {
        // Personal Info
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DateOfBirth { get; set; }

        // Placement
        public string FacilityId { get; set; }
        public string UnitId { get; set; }
        public string CareLevel { get; set; }
        public string AdmissionDate { get; set; }
        public string MonthlyRate { get; set; }

        // Medical
        public bool RequiresMedicationManagement { get; set; }
        public string PrimaryPhysician { get; set; }
        public string CognitiveAssessmentDate { get; set; }

        // Emergency Contact
        public string EmergencyContactName { get; set; }
        public string EmergencyContactPhone { get; set; }

        // Facility-specific
        public string RoomPreference { get; set; }
        public string DepositAmount { get; set; }
    }

    public class LookupItem
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public LookupItem() { }
        public LookupItem(string id, string name) { Id = id; Name = name; }
    }

    public class FacilitiesResponse
    {
        public List<LookupItem> Facilities { get; set; }
    }

    public class CareLevelsResponse
    {
        public List<LookupItem> Levels { get; set; }
    }

    public class UnitsResponse
    {
        public List<LookupItem> Units { get; set; }
    }

    public class ConfirmationResponse
    {
        public string Number { get; set; }
    }
}
