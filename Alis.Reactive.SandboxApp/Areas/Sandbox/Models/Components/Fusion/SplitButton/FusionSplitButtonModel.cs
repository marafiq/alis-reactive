namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public sealed class FusionSplitButtonModel
    {
    }

    public sealed class FusionSplitButtonEchoRequest
    {
        public string Content { get; set; } = string.Empty;

        public bool Disabled { get; set; }

        public string CssClass { get; set; } = string.Empty;
    }

    public sealed class FusionSplitButtonEchoResponse
    {
        public string Content { get; set; } = string.Empty;

        public bool Disabled { get; set; }

        public string CssClass { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;
    }
}
