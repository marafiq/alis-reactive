namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    // Journey: a care coordinator updates a resident's profile card — the preferred name
    // they go by, and a dietary preference note kitchen staff read. The card opens with the
    // name on file, the coordinator edits it, and saves the profile to the resident record.
    public class TextBoxModel
    {
        public string? PreferredName { get; set; }

        public string? DietaryNote { get; set; }

        // The resident's full legal name on file — offered as a one-click fill for the
        // preferred-name field when a resident goes by their legal name.
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
