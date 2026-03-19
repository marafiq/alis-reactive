using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ComponentGatherModel
    {
        // Native scalar
        public string? ResidentName { get; set; }            // NativeTextBox
        public string? CareNotes { get; set; }               // NativeTextArea
        public bool HasAllergies { get; set; }                // NativeCheckBox
        public string? MobilityLevel { get; set; }            // NativeDropDown
        public string? CareLevel { get; set; }                // NativeRadioGroup

        // Native array
        public string[]? Allergies { get; set; }              // NativeCheckList

        // Fusion scalar
        public decimal MonthlyRate { get; set; }              // FusionNumericTextBox
        public string? FacilityId { get; set; }               // FusionDropDownList
        public string? PhysicianName { get; set; }            // FusionAutoComplete
        public DateTime? AdmissionDate { get; set; }          // FusionDatePicker
        public DateTime? MedicationTime { get; set; }         // FusionTimePicker
        public DateTime? AppointmentTime { get; set; }        // FusionDateTimePicker
        public DateTime? StayStart { get; set; }              // FusionDateRangePicker
        public string? InsuranceProvider { get; set; }        // FusionMultiColumnComboBox
        public string? PhoneNumber { get; set; }              // FusionInputMask
        public string? CarePlan { get; set; }                 // FusionRichTextEditor
        public bool ReceiveNotifications { get; set; }        // FusionSwitch

        // Fusion array
        public string[]? DietaryRestrictions { get; set; }    // FusionMultiSelect
    }

    // ── Response DTO for typed OnSuccess<T> echo binding ──

    public class GatherEchoResponse
    {
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
        public string? StayStart { get; set; }
        public string? InsuranceProvider { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CarePlan { get; set; }
        public bool ReceiveNotifications { get; set; }
        public string[]? DietaryRestrictions { get; set; }
        public int FieldCount { get; set; }
    }

    // ── Item types for data sources ──

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
