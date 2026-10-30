namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Plugins.Pager
{
    // Journey: the night desk pages the wing's staff through the pager system the community already
    // runs. The pager is JavaScript the page registers by name (Scripts/sandbox-plugins.ts) and declares
    // here with every kind of member a plugin can have; initials is a plugin that is itself a function.
    public class WingPagerModel
    {
        /// <summary>The message to page.</summary>
        public string Message { get; set; } = "Room 12 needs a hand";

        /// <summary>Who signs the page.</summary>
        public string Sender { get; set; } = "Nurse Patel";

        /// <summary>The code that unlocks priority paging.</summary>
        public string? AccessCode { get; set; }
    }

    /// <summary>The wing's pager system, registered by the page's script as <c>pager</c>.</summary>
    public sealed class PagerPlugin : Plugin
    {
        public PagerPlugin()
            : base("pager")
        {
        }

        /// <summary>The channel pages go out on; read and written.</summary>
        public Property<string> Channel => Property<string>();

        /// <summary>The code that unlocks priority paging. Written only: the pager never gives it back.</summary>
        public WriteOnly<string> AccessCode => WriteOnly<string>();

        /// <summary>Whether priority paging is unlocked.</summary>
        public ReadOnly<bool> PriorityUnlocked => ReadOnly<bool>();

        /// <summary>How many pages have been delivered.</summary>
        public ReadOnly<int> Delivered => ReadOnly<int>();

        /// <summary>The system's status: an object with members of its own.</summary>
        public PagerStatus Status => Object<PagerStatus>();

        /// <summary>The pagers on the wing: a list of objects with members of their own.</summary>
        public Objects<PagerDevice> Devices => Objects<PagerDevice>();

        /// <summary>How many seconds a page takes to reach a number of pagers, faster when urgent.</summary>
        /// <param name="count">How many pagers.</param>
        /// <param name="urgent">Whether the page is urgent.</param>
        public TypedSource<int> Estimate(Arg<int> count, Arg<bool> urgent) => Method<int>(count, urgent);

        /// <summary>Sends a page: a promise the steps after it wait for; <see cref="Delivered"/> counts it once it settles.</summary>
        /// <param name="message">The message.</param>
        public void Send(Arg<string> message) => MethodAsync(message);

        /// <summary>Sends a page and returns its receipt once delivered: a promise the first step that uses it waits for.</summary>
        /// <param name="message">The message.</param>
        public TypedSource<string> SendWithReceipt(Arg<string> message) => MethodAsync<string>(message);
    }

    /// <summary>The pager system's <c>status</c>.</summary>
    public sealed class PagerStatus : JsObject
    {
        /// <summary>Whether the system is online.</summary>
        public ReadOnly<bool> Online => ReadOnly<bool>();

        /// <summary>How many pages are waiting.</summary>
        public ReadOnly<int> Queued => ReadOnly<int>();

        /// <summary>The status in words.</summary>
        public TypedSource<string> Label() => Method<string>();
    }

    /// <summary>One of the pager system's <c>devices</c>.</summary>
    public sealed class PagerDevice : JsObject
    {
        /// <summary>Who carries it.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();

        /// <summary>Its battery, in percent.</summary>
        public ReadOnly<int> Battery => ReadOnly<int>();
    }

    /// <summary>A person's initials, registered by the page's script as <c>initials</c>: the plugin is itself the function.</summary>
    public sealed class InitialsPlugin : Plugin
    {
        public InitialsPlugin()
            : base("initials")
        {
        }

        /// <summary>The initials of a name.</summary>
        /// <param name="fullName">The name.</param>
        public TypedSource<string> Of(Arg<string> fullName) => Method<string>(fullName, name: "");
    }
}
