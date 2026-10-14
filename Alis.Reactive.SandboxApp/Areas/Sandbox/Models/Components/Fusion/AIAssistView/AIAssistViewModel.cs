namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.AIAssistView
{
    /// <summary>
    /// The resident a care coordinator asks the community's assistant about: who she is, the notes her care
    /// team saved, and what the night shift handed off this morning.
    /// </summary>
    public sealed class AIAssistViewModel
    {
        public string ResidentName { get; init; } = "";

        public string Room { get; init; } = "";

        public string CareLevel { get; init; } = "";

        /// <summary>The notes the care team saved for the resident, shown in the assistant's Notes view.</summary>
        public IReadOnlyList<string> SavedNotes { get; init; } = [];

        /// <summary>The night shift's handoff, which the coordinator can bring into the conversation.</summary>
        public HandoffNote NightHandoff { get; init; } = new();
    }

    /// <summary>A handoff written as the question it answers and the answer the shift left.</summary>
    public sealed class HandoffNote
    {
        public string Question { get; init; } = "";

        public string Answer { get; init; } = "";
    }

    /// <summary>The link from a resident's chart, which may carry a question to ask on arrival: <c>?ask=...</c>; empty asks nothing.</summary>
    public sealed class AssistantLink
    {
        public string Ask { get; set; } = "";
    }

    /// <summary>The body of <c>POST AIAssistView/Answer</c>: the question asked and the common questions the assistant offers.</summary>
    public sealed class AssistantQuestion
    {
        public string? Prompt { get; init; }

        public string[] Suggestions { get; init; } = [];
    }

    /// <summary>The care team's answer to a question.</summary>
    public sealed class AssistantAnswer
    {
        public string Answer { get; init; } = "";
    }

    /// <summary>The body of <c>POST AIAssistView/StopAnswer</c>: which question of the conversation was stopped, from zero.</summary>
    public sealed class StoppedAnswer
    {
        public int QuestionIndex { get; init; }
    }

    /// <summary>The body of <c>POST AIAssistView/AskNurse</c>: the question for the charge nurse.</summary>
    public sealed class NurseQuestion
    {
        public string? Question { get; init; }
    }

    /// <summary>What the server says about a stopped answer or a question sent to the charge nurse.</summary>
    public sealed class AssistantStatus
    {
        public string Summary { get; init; } = "";
    }
}
