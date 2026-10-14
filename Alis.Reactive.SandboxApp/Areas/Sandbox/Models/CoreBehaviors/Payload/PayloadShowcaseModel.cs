namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class PayloadShowcaseModel
    {
        public int IntValue { get; set; }
        public long LongValue { get; set; }
        public double DoubleValue { get; set; }
        public string? StringValue { get; set; }
        public bool BoolValue { get; set; }
        public AddressModel? Address { get; set; }
    }

    public class AddressModel
    {
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? Zip { get; set; }
    }

    /// <summary>The document events the PayloadShowcase page raises and hears.</summary>
    public static class PayloadShowcaseEvents
    {
        public static readonly AppEvent<PayloadShowcaseModel> PayloadTest = AppEvent.Named<PayloadShowcaseModel>("payload-test");
    }
}
