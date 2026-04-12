namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class DispatchSourceModel
    {
        public string Name { get; set; } = "";
        public string Status { get; set; } = "";
        public DispatchSourceAddress Address { get; set; }
    }

    public class DispatchSourceAddress
    {
        public string City { get; set; } = "";
    }
}
