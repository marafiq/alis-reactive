namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
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

    public sealed record EmergencyContactsConfirmation(string Confirmation);
}
