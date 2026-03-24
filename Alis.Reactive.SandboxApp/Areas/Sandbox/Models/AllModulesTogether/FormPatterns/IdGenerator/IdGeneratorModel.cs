namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class IdGeneratorModel
    {
        public string? Name { get; set; }
        public decimal Amount { get; set; }
        public string? Status { get; set; }
        public IdGeneratorAddress? Address { get; set; }
    }

    public class IdGeneratorAddress
    {
        public string? City { get; set; }
        public int PostalCode { get; set; }
    }

    public class IdGeneratorJsonResponse
    {
        public string? Summary { get; set; }
    }
}
