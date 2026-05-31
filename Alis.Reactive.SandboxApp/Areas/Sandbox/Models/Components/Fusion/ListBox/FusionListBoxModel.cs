namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public sealed class FusionListBoxModel
    {
    }

    public sealed class ListBoxResidentItem
    {
        public string Value { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;
    }

    public sealed class FusionListBoxEchoRequest
    {
        public string[]? Value { get; set; }
    }

    public sealed class FusionListBoxEchoResponse
    {
        public string ValueSummary { get; set; } = string.Empty;

        public string CountSummary { get; set; } = string.Empty;
    }
}
