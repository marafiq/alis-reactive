namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Patterns.MoveInContacts
{
    public class MoveInContactsModel
    {
        public ResidentName Resident { get; set; } = new ResidentName();

        public List<MoveInContact> Contacts { get; set; } = new List<MoveInContact> { new MoveInContact(), new MoveInContact() };

        public MoveInContact NextOfKin { get; set; } = new MoveInContact();

        public MoveInContact Alternate { get; set; } = new MoveInContact();
    }

    public class ResidentName
    {
        public string? First { get; set; }

        public string? Last { get; set; }
    }

    public class MoveInContact
    {
        public string? Name { get; set; }

        public string? Phone { get; set; }
    }

    public sealed record ContactsSaved(string Confirmation);
}
