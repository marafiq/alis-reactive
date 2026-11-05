namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.SmartComponents
{
    public sealed class SmartComponentsModel
    {
        public string? IncomingReferral { get; set; }

        public string? ResidentName { get; set; }

        public string? ReferredBy { get; set; }

        public string? CareLevel { get; set; }

        public string? Reason { get; set; }

        public string IntakeNote { get; set; } = string.Empty;

        public IReadOnlyList<IncomingReferral> Inbox { get; set; } = [];

        public IReadOnlyList<string> CareLevels { get; set; } = [];
    }

    public sealed class IncomingReferral
    {
        public string Id { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;
    }

    public sealed class ReferralLetterQuery
    {
        public string ReferralId { get; set; } = string.Empty;
    }

    public sealed class ReferralIntakeSave
    {
        public string ResidentName { get; set; } = string.Empty;

        public string CareLevel { get; set; } = string.Empty;

        public string? IntakeNote { get; set; }
    }

    public sealed class ReferralIntakeSaved
    {
        public string Message { get; set; } = string.Empty;
    }

    public sealed class SmartAssistRequest
    {
        public IReadOnlyList<SmartAssistMessage> Messages { get; set; } = [];
    }

    public sealed class SmartAssistMessage
    {
        public string Role { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;
    }
}
