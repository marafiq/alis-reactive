namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public sealed class FusionRadioButtonModel
    {
        public string RoomType { get; set; } = string.Empty;
    }

    public sealed class FusionRadioButtonEchoRequest
    {
        public string SelectedValue { get; set; } = string.Empty;

        public bool PrivateChecked { get; set; }

        public bool SharedChecked { get; set; }

        public bool SharedDisabled { get; set; }
    }

    public sealed class FusionRadioButtonEchoResponse
    {
        public string SelectedValue { get; set; } = string.Empty;

        public bool PrivateChecked { get; set; }

        public bool SharedChecked { get; set; }

        public bool SharedDisabled { get; set; }

        public string Summary { get; set; } = string.Empty;
    }
}
