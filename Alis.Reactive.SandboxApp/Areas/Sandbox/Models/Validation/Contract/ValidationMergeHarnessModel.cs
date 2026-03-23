namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ValidationMergeHarnessModel
    {
        public ValidationMergeRootSection Root { get; set; } = new();
        public NestedSection Nested { get; set; } = new()
        {
            Address = new ValidationAddress(),
            Delivery = new DeliveryNote()
        };
    }

    public class ValidationMergeRootSection
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public decimal? Amount { get; set; }
    }
}
