namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public sealed class ButtonModel
    {
        public string ResidentName { get; set; } = "Eleanor Whitfield";
    }

    public sealed class ButtonCheckInRequest
    {
        public string Action { get; set; } = string.Empty;

        public bool Locked { get; set; }

        public string Priority { get; set; } = string.Empty;

        public bool Recommended { get; set; }

        public bool FollowUp { get; set; }
    }

    public sealed class ButtonCheckInResponse
    {
        public string Confirmation { get; set; } = string.Empty;
    }
}
