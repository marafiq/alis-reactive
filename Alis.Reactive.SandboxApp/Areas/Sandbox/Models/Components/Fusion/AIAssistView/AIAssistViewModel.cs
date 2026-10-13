namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public sealed class AIAssistViewModel
    {
    }

    /// <summary>The query of <c>GET AIAssistView/Reply</c>: the prompt the coordinator sent.</summary>
    public sealed class AIAssistReplyQuery
    {
        public string? Prompt { get; init; }
    }

    /// <summary>The care team's answer to a prompt.</summary>
    public sealed class AIAssistReply
    {
        public string Answer { get; init; } = "";
    }
}
