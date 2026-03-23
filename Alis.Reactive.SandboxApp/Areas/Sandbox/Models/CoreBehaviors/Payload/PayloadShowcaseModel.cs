namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.CoreBehaviors.Payload
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
}
