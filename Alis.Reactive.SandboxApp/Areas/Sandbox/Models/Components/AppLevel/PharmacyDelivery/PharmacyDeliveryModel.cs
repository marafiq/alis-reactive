namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.AppLevel.PharmacyDelivery
{
    // Journey: the night nurse receives the evening pharmacy delivery. The slip opens in the page's
    // drawer, signing for it shows the page's toast, a damaged item is returned only after the nurse
    // confirms, the manifest check covers its card with the page's loader while the pharmacy answers,
    // and the delivery is marked from a link that sends who received it.
    public class PharmacyDeliveryModel
    {
        /// <summary>Who received the delivery.</summary>
        public string? ReceivedBy { get; set; } = "Nurse Patel";
    }

    /// <summary>The delivery slip that opens in the drawer.</summary>
    public class DeliverySlipModel
    {
        public string Pharmacy { get; set; } = "Garden Pharmacy";

        public int Items { get; set; } = 12;
    }

    /// <summary>What the pharmacy says about the manifest.</summary>
    public sealed record ManifestSummary(string Summary);

    /// <summary>The confirmation the server returns once a damaged item is returned.</summary>
    public sealed record ReturnResult(string Confirmation);

    /// <summary>What the Mark delivered link sends: who received the delivery.</summary>
    public sealed class DeliveryReceipt
    {
        public string? ReceivedBy { get; set; }
    }

    /// <summary>The confirmation the server returns once the delivery is marked.</summary>
    public sealed record DeliveryMarked(string Confirmation);
}
