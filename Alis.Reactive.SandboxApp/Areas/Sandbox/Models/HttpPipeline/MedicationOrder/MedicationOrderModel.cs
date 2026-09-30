namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>A medication refill order for one resident, one line per medication.</summary>
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

    /// <summary>The order confirmation the server returns once the order is saved.</summary>
    public sealed record MedicationOrderConfirmation(string Confirmation);
}
