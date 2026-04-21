using System;
using System.Collections.Generic;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    // --- Use Case A: mixed-form view model (Index.cshtml owns one plan on ResidentProfile) ---

    public class ResidentProfile
    {
        public string? Name { get; set; }
        public string? CareLevelId { get; set; }
        public DateTime? AdmissionDate { get; set; }
        public decimal MonthlyRate { get; set; }
        public string? Nickname { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Allergies { get; set; }
    }

    // --- Use Case B: per-card standalone nested models (each partial view owns its own plan) ---

    public class DateOfBirthQuickEdit
    {
        public string? ResidentId { get; set; }
        public DateTime? Value { get; set; }
    }

    public class CareLevelQuickEdit
    {
        public string? ResidentId { get; set; }
        public string? Value { get; set; }
    }

    public class MonthlyRateQuickEdit
    {
        public string? ResidentId { get; set; }
        public decimal Value { get; set; }
    }

    public class NicknameQuickEdit
    {
        public string? ResidentId { get; set; }
        public string? Value { get; set; }
    }

    public class CancelDemoQuickEdit
    {
        public string? ResidentId { get; set; }
        public string? Value { get; set; }
    }

    public class AllergiesQuickEdit
    {
        // MultiSelect inner type — value is an array of allergy codes.
        public string? ResidentId { get; set; }
        public string[]? Value { get; set; }
    }

    public class LastAdmissionQuickEdit
    {
        // DateTimePicker inner type — value is date + time.
        public string? ResidentId { get; set; }
        public DateTime? Value { get; set; }
    }

    public class MedicalRecordNumberQuickEdit
    {
        // Mask inner type — format-constrained string (e.g. MRN pattern).
        public string? ResidentId { get; set; }
        public string? Value { get; set; }
    }

    // --- API-surface coverage cards (demo-only; no commit endpoint) ---

    public class InPlaceEditorControlPanelModel
    {
        // Demo: Text inner, proves SetValue/Enable/Disable/Save/Focus/Validate mutations and the Value() read.
        public string? Value { get; set; }
    }

    public class InPlaceEditorEventTracerModel
    {
        // Demo: Text inner with SF ValidationRules so Validating fires alongside the lifecycle events.
        public string? Value { get; set; }
    }

    // --- Lookup DTOs (shared by §A and §B) ---

    public class InPlaceEditorCareLevelOption
    {
        public string Id { get; set; } = "";
        public string Label { get; set; } = "";
    }

    public class AllergyOption
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
    }

    // --- Commit-response / error DTOs (OnSuccess<T> / OnError<T> targets) ---

    public class InPlaceEditorUpdateResponse
    {
        public string DisplayValue { get; set; } = "";
        public bool Saved { get; set; }
    }

    public class InPlaceEditorCommitError
    {
        public string Message { get; set; } = "";
        public Dictionary<string, string>? FieldErrors { get; set; }
    }
}
