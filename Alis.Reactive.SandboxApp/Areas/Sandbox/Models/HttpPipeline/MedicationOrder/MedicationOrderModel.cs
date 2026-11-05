namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class MedicationOrderModel
    {
        public string? ResidentName { get; set; }
        public List<MedicationOrderLine> Lines { get; set; } = new();
    }

    public class MedicationOrderLine
    {
        public string? Sku { get; set; }
        public int Quantity { get; set; }
    }

    public sealed record MedicationOrderConfirmation(string Confirmation);
}
