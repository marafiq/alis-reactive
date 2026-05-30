namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public sealed class FusionDropDownTreeModel
    {
        public string[]? ResidentIds { get; set; }
    }

    public sealed class DropDownTreeResidentNode
    {
        public string Value { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

        public string? ParentValue { get; set; }

        public bool HasChildren { get; set; }

        public bool Expanded { get; set; }
    }

    public sealed class FusionDropDownTreeEchoRequest
    {
        public string[]? Value { get; set; }

        public string? Text { get; set; }
    }

    public sealed class FusionDropDownTreeEchoResponse
    {
        public string ValueSummary { get; set; } = string.Empty;

        public string? Text { get; set; }

        public string Summary { get; set; } = string.Empty;
    }
}
