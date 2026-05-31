namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public sealed class FusionCheckBoxModel
    {
        public bool ConsentAccepted { get; set; }

        public bool ReviewNeeded { get; set; }
    }

    public sealed class FusionCheckBoxEchoRequest
    {
        public bool Checked { get; set; }

        public bool Indeterminate { get; set; }

        public bool Disabled { get; set; }
    }

    public sealed class FusionCheckBoxEchoResponse
    {
        public bool Checked { get; set; }

        public bool Indeterminate { get; set; }

        public bool Disabled { get; set; }

        public string Summary { get; set; } = string.Empty;
    }
}
