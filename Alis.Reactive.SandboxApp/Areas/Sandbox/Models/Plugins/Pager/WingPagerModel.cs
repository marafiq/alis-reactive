namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Plugins.Pager
{
    public class WingPagerModel
    {
        public string Message { get; set; } = "Room 12 needs a hand";

        public string Sender { get; set; } = "Nurse Patel";

        public string? AccessCode { get; set; }
    }

    public sealed class PagerPlugin : Plugin
    {
        public PagerPlugin()
            : base("pager")
        {
        }

        public Property<string> Channel => Property<string>();

        public WriteOnly<string> AccessCode => WriteOnly<string>();

        public ReadOnly<bool> PriorityUnlocked => ReadOnly<bool>();

        public ReadOnly<int> Delivered => ReadOnly<int>();

        public PagerStatus Status => Object<PagerStatus>();

        public Objects<PagerDevice> Devices => Objects<PagerDevice>();

        public TypedSource<int> Estimate(Arg<int> count, Arg<bool> urgent) => Method<int>(count, urgent);

        public void Send(Arg<string> message) => MethodAsync(message);

        public TypedSource<string> SendWithReceipt(Arg<string> message) => MethodAsync<string>(message);
    }

    public sealed class PagerStatus : JsObject
    {
        public ReadOnly<bool> Online => ReadOnly<bool>();

        public ReadOnly<int> Queued => ReadOnly<int>();

        public TypedSource<string> Label() => Method<string>();
    }

    public sealed class PagerDevice : JsObject
    {
        public ReadOnly<string> Name => ReadOnly<string>();

        public ReadOnly<int> Battery => ReadOnly<int>();
    }

    public sealed class InitialsPlugin : Plugin
    {
        public InitialsPlugin()
            : base("initials")
        {
        }

        public TypedSource<string> Of(Arg<string> fullName) => Method<string>(fullName, name: "");
    }
}
