namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class MedicationReconciliationModel
    {
        public string? ResidentName { get; set; }
        public List<MedicationLine> Medications { get; set; } = new();
        public bool HasAllergies { get; set; }
        public List<string?> Allergies { get; set; } = new();
        public List<string> AdministrationChecks { get; set; } = new();
    }

    public static class Pharmacy
    {
        private static readonly string[] Allergens = { "Penicillin", "Latex", "Sulfa", "Codeine", "Aspirin" };

        public static bool Lists(string? allergen) =>
            Array.Exists(Allergens, listed => string.Equals(listed, allergen?.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public class MedicationLine
    {
        public string? Name { get; set; }
        public int? DoseMg { get; set; }

        public string? Hours { get; set; }
        public bool Controlled { get; set; }

        public string? CountVerifiedBy { get; set; }
        public Prescriber Prescriber { get; set; } = new();
    }

    public class Prescriber
    {
        public string? Npi { get; set; }
    }

    public sealed record MedicationReconciliationConfirmation(string Confirmation);
}
