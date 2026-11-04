namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.SplitButton
{
    public sealed class FusionSplitButtonModel
    {
        public string? Kind { get; set; }

        public string? Note { get; set; }

        public bool Signed { get; set; }
    }

    public sealed class CareNoteSave
    {
        public string? Kind { get; set; }

        public string? Note { get; set; }

        public string Action { get; set; } = string.Empty;
    }

    public sealed class CareNoteSaved
    {
        public bool Saved { get; set; }

        public string Message { get; set; } = string.Empty;

        public string[] FinishedActions { get; set; } = [];
    }
}
