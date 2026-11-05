namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class TextBoxModel
    {
        public string? PreferredName { get; set; }

        public string? DietaryNote { get; set; }

        public string LegalName { get; set; } = string.Empty;
    }

    public sealed class ProfileSaveRequest
    {
        public string? PreferredName { get; set; }

        public string? DietaryNote { get; set; }
    }

    public sealed class ProfileSaveResponse
    {
        public string Confirmation { get; set; } = string.Empty;
    }
}
