namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.OtpInput
{
    public sealed class OtpInputModel
    {
        public string? VisitCode { get; set; }
    }

    public sealed class VisitCodeCheck
    {
        public string Code { get; set; } = string.Empty;

        public string PreviousCode { get; set; } = string.Empty;
    }

    public sealed class VisitCodeChecked
    {
        public string Message { get; set; } = string.Empty;
    }

    public sealed class WalkInCodeIssued
    {
        public string Code { get; set; } = string.Empty;
    }
}
