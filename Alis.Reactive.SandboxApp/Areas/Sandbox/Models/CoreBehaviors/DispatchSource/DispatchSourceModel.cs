namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class DispatchSourceModel
    {
        public string Name { get; set; } = "";
        public string Status { get; set; } = "";
        public DispatchSourceAddress Address { get; set; } = new DispatchSourceAddress();
    }

    public class DispatchSourceAddress
    {
        public string City { get; set; } = "";
    }

    public static class DispatchSourceEvents
    {
        public static readonly AppEvent DoTransfer = AppEvent.Declare();
        public static readonly AppEvent<DispatchSourceModel> Transfer = AppEvent.Declare<DispatchSourceModel>();
    }
}
