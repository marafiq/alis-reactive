namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.AppLevel.PharmacyDelivery
{
    public class PharmacyDeliveryModel
    {
        public string? ReceivedBy { get; set; } = "Nurse Patel";
    }

    public class DeliveryReceiptValidator : ReactiveValidator<PharmacyDeliveryModel>
    {
        public DeliveryReceiptValidator()
        {
            RuleFor(x => x.ReceivedBy).NotEmpty().WithMessage("Say who received the delivery.");
        }
    }

    public class DeliverySlipModel
    {
        public string Pharmacy { get; set; } = "Garden Pharmacy";

        public int Items { get; set; } = 12;
    }

    public sealed record ManifestSummary(string Summary);

    public sealed record ReturnResult(string Confirmation);

    public sealed class DeliveryReceipt
    {
        public string? ReceivedBy { get; set; }
    }

    public sealed record DeliveryMarked(string Confirmation);
}
