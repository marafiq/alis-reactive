namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.AIAssistView
{
    public sealed class AIAssistViewModel
    {
        public string ResidentName { get; init; } = "";

        public string Room { get; init; } = "";

        public string CareLevel { get; init; } = "";

        public IReadOnlyList<string> SavedNotes { get; init; } = [];

        public HandoffNote NightHandoff { get; init; } = new();
    }

    public sealed class HandoffNote
    {
        public string Question { get; init; } = "";

        public string Answer { get; init; } = "";
    }

    public sealed class AssistantLink
    {
        public string Ask { get; set; } = "";
    }

    public sealed class AssistantQuestion
    {
        public string? Prompt { get; init; }

        public string[] Suggestions { get; init; } = [];
    }

    public sealed class AssistantAnswer
    {
        public string Answer { get; init; } = "";
    }

    public sealed class StoppedAnswer
    {
        public int QuestionIndex { get; init; }
    }

    public sealed class NurseQuestion
    {
        public string? Question { get; init; }
    }

    public sealed class AssistantStatus
    {
        public string Summary { get; init; } = "";
    }
}
