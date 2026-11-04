namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ArrayGridModel
    {
    }

    public sealed class RosterResponse
    {
        public RosterResident[] Residents { get; set; } = Array.Empty<RosterResident>();
    }

    public sealed class RosterResident
    {
        public string Name { get; set; } = "";
        public string Status { get; set; } = "";
        public int Age { get; set; }
        public int Balance { get; set; }
    }
}
