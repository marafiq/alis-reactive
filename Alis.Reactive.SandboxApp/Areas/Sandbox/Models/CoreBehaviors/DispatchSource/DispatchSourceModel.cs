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

    /// <summary>The document events the DispatchSource page raises and hears.</summary>
    public static class DispatchSourceEvents
    {
        public static readonly AppEvent DoTransfer = AppEvent.Named("do-transfer");
        public static readonly AppEvent<DispatchSourceModel> Transfer = AppEvent.Named<DispatchSourceModel>("transfer");
    }
}
