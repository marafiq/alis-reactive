namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class MedPassSheetViewModel
    {
    }

    public class MedPassDose
    {
        public int DoseId { get; set; }
        public string Room { get; set; } = "";
        public string ResidentName { get; set; } = "";

        public string Medication { get; set; } = "";
        public string Dose { get; set; } = "";

        public string Due { get; set; } = "";
    }
}
