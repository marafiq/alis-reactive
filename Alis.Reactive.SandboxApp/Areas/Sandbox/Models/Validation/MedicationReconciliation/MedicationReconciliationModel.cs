namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// The nurse's medication reconciliation: each medication line has its own rules, a controlled
    /// substance has more, each allergy line is checked, and at least one administration check is ticked.
    /// </summary>
    public class MedicationReconciliationModel
    {
        public string? ResidentName { get; set; }
        public List<MedicationLine> Medications { get; set; } = new();
        public bool HasAllergies { get; set; }
        public List<string?> Allergies { get; set; } = new();
        public List<string> AdministrationChecks { get; set; } = new();
    }

    public class MedicationLine
    {
        public string? Name { get; set; }
        public int? DoseMg { get; set; }

        /// <summary>The hours the dose is given, like 8,20.</summary>
        public string? Hours { get; set; }
        public bool Controlled { get; set; }

        /// <summary>The second nurse who verified a controlled substance count.</summary>
        public string? CountVerifiedBy { get; set; }
        public Prescriber Prescriber { get; set; } = new();
    }

    public class Prescriber
    {
        /// <summary>The prescriber's ten-digit NPI.</summary>
        public string? Npi { get; set; }
    }

    /// <summary>What the server says once the reconciliation is saved, or the MAR printed.</summary>
    public sealed record MedicationReconciliationConfirmation(string Confirmation);
}
