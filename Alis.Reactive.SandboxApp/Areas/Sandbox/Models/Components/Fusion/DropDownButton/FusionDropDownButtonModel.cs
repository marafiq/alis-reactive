namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public sealed class FusionDropDownButtonModel
    {
    }

    public sealed class FusionDropDownButtonEchoRequest
    {
        public string Content { get; set; } = string.Empty;

        public bool Disabled { get; set; }

        public string CssClass { get; set; } = string.Empty;
    }

    public sealed class FusionDropDownButtonEchoResponse
    {
        public string Content { get; set; } = string.Empty;

        public bool Disabled { get; set; }

        public string CssClass { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;
    }
}
