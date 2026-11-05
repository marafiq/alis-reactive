namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.ComboBox
{
    public sealed class FusionComboBoxModel
    {
        public string? Wristband { get; set; }

        public string? Resident { get; set; }

        public IReadOnlyList<RoundResident> Round { get; init; } = [];
    }

    public sealed class RoundResident
    {
        public string Id { get; init; } = "";

        public string Name { get; init; } = "";
    }

    public sealed class DoseCheckQuery
    {
        public string? ResidentId { get; set; }
    }

    public sealed class DoseCheck
    {
        public string Room { get; init; } = "";

        public string Allergies { get; init; } = "";
    }

    public sealed class MedicationPassRecord
    {
        public string? Resident { get; set; }

        public int? RoundPosition { get; set; }
    }

    public sealed class MedicationPassStatus
    {
        public string Message { get; init; } = "";
    }
}
