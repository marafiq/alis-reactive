namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// The resident roster: the rows load once, sorted by name in the browser, and the roster narrows to
    /// the active residents from the rows already on screen. No inputs.
    /// </summary>
    public class ArrayGridModel
    {
    }

    /// <summary>The roster as the server sends it, in no particular order.</summary>
    public sealed class RosterResponse
    {
        public RosterResident[] Residents { get; set; } = Array.Empty<RosterResident>();
    }

    /// <summary>One row of the roster.</summary>
    public sealed class RosterResident
    {
        public string Name { get; set; } = "";
        public string Status { get; set; } = "";
        public int Age { get; set; }
        public int Balance { get; set; }
    }
}
