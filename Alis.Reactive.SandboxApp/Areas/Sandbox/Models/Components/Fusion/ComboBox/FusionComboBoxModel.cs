namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.ComboBox
{
    /// <summary>
    /// A nurse's 08:00 medication pass on the Maple wing: the resident is chosen by scanning their
    /// wristband or by typing part of their last name, and the page shows their room and allergies.
    /// </summary>
    public sealed class FusionComboBoxModel
    {
        /// <summary>The code the cart's scanner reads off a resident's wristband, such as R-1042.</summary>
        public string? Wristband { get; set; }

        /// <summary>The resident whose medications are being given, by resident id.</summary>
        public string? Resident { get; set; }

        /// <summary>The residents on the round, in the order the nurse visits them.</summary>
        public IReadOnlyList<RoundResident> Round { get; init; } = [];
    }

    /// <summary>A resident on the round: the id the picker keeps and the name it shows, last name first.</summary>
    public sealed class RoundResident
    {
        public string Id { get; init; } = "";

        public string Name { get; init; } = "";
    }

    /// <summary>Asks for the chart of the resident the picker holds.</summary>
    public sealed class DoseCheckQuery
    {
        public string? ResidentId { get; set; }
    }

    /// <summary>What the nurse checks before giving a dose: where the resident is and what they react to.</summary>
    public sealed class DoseCheck
    {
        public string Room { get; init; } = "";

        public string Allergies { get; init; } = "";
    }

    /// <summary>The medications given to the resident the picker holds, at their place on the round.</summary>
    public sealed class MedicationPassRecord
    {
        public string? Resident { get; set; }

        /// <summary>The resident's place on the round, from zero, as the picker reads it.</summary>
        public int? RoundPosition { get; set; }
    }

    /// <summary>The line the pass shows after a dose is recorded.</summary>
    public sealed class MedicationPassStatus
    {
        public string Message { get; init; } = "";
    }
}
