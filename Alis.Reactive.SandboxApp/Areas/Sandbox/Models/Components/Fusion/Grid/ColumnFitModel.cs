namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>The med pass sheet page: the grid holds tonight's doses, so the page has no fields of its own.</summary>
    public class MedPassSheetViewModel
    {
    }

    /// <summary>One dose of the evening medication pass, one row of the sheet.</summary>
    public class MedPassDose
    {
        public int DoseId { get; set; }
        public string Room { get; set; } = "";
        public string ResidentName { get; set; } = "";

        /// <summary>The medication as the pharmacy labels it: <c>Donepezil hydrochloride 10 mg tablet</c>.</summary>
        public string Medication { get; set; } = "";
        public string Dose { get; set; } = "";

        /// <summary>When the dose is due: <c>8:00 PM</c>.</summary>
        public string Due { get; set; } = "";
    }
}
