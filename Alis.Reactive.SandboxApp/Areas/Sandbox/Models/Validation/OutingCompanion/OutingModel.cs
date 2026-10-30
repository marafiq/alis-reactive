namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// A resident's outing. A companion joins it only when the coordinator adds one: the companion's
    /// fields, and their rules, arrive with the companion and leave with them.
    /// </summary>
    public class OutingModel
    {
        public string? ResidentName { get; set; }
        public string? Destination { get; set; }
        public OutingCompanion? Companion { get; set; }
    }

    public class OutingCompanion
    {
        public string? Name { get; set; }
        public string? Phone { get; set; }
    }

    /// <summary>What the server says once the outing is saved, or the companion confirmed.</summary>
    public sealed record OutingConfirmation(string Confirmation);
}
