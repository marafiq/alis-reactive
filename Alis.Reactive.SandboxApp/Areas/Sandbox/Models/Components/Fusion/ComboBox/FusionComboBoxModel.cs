namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public sealed class FusionComboBoxModel
    {
        public string? Resident { get; set; }
    }

    public sealed class ComboBoxResidentItem
    {
        public string Value { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;
    }

    public sealed class FusionComboBoxEchoRequest
    {
        public string? Value { get; set; }

        public string? Text { get; set; }

        public int Index { get; set; }
    }

    public sealed class FusionComboBoxEchoResponse
    {
        public string? Value { get; set; }

        public string? Text { get; set; }

        public int Index { get; set; }

        public string Summary { get; set; } = string.Empty;
    }
}
