namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public sealed class ButtonModel
    {
    }

    public sealed class ButtonEchoRequest
    {
        public string Content { get; set; } = string.Empty;

        public bool Disabled { get; set; }

        public string CssClass { get; set; } = string.Empty;

        public bool IsPrimary { get; set; }

        public bool IsToggle { get; set; }
    }

    public sealed class ButtonEchoResponse
    {
        public string Content { get; set; } = string.Empty;

        public bool Disabled { get; set; }

        public string CssClass { get; set; } = string.Empty;

        public bool IsPrimary { get; set; }

        public bool IsToggle { get; set; }

        public string Summary { get; set; } = string.Empty;
    }
}
