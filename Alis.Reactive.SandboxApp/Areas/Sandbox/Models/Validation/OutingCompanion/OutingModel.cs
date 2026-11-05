namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
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

    public sealed record OutingConfirmation(string Confirmation);
}
