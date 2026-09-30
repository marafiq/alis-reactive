namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>A resident's intake: whether they have emergency contacts, and each contact's details.</summary>
    public class EmergencyContactsModel
    {
        public string? ResidentName { get; set; }
        public bool HasContacts { get; set; }
        public List<EmergencyContact> Contacts { get; set; } = new();
    }

    public class EmergencyContact
    {
        public string? Name { get; set; }
        public string? Phone { get; set; }
    }

    /// <summary>The intake confirmation the server returns once the contacts are saved.</summary>
    public sealed record EmergencyContactsConfirmation(string Confirmation);
}
