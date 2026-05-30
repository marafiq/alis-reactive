namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public sealed class FusionProgressButtonModel
    {
    }

    public sealed class FusionProgressButtonEchoRequest
    {
        public string Content { get; set; } = string.Empty;

        public bool Disabled { get; set; }

        public string CssClass { get; set; } = string.Empty;

        public bool ProgressEnabled { get; set; }
    }

    public sealed class FusionProgressButtonEchoResponse
    {
        public string Content { get; set; } = string.Empty;

        public bool Disabled { get; set; }

        public string CssClass { get; set; } = string.Empty;

        public bool ProgressEnabled { get; set; }

        public string Summary { get; set; } = string.Empty;
    }
}
