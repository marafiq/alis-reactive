namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Patterns.MoveInContacts
{
    // Journey: the admissions coordinator records a new resident's contacts on the move-in packet. The
    // resident's name is nested, the emergency contacts are items of a list each rendered by the same
    // row partial, and the next of kin is a card of its own rendered under a field prefix. Every input
    // carries the id the framework derives and the name the server binds, and the packet saves as JSON
    // and as a form alike.
    public class MoveInContactsModel
    {
        /// <summary>The resident's name.</summary>
        public ResidentName Resident { get; set; } = new ResidentName();

        /// <summary>The emergency contacts, in order.</summary>
        public List<MoveInContact> Contacts { get; set; } = new List<MoveInContact> { new MoveInContact(), new MoveInContact() };

        /// <summary>The next of kin.</summary>
        public MoveInContact NextOfKin { get; set; } = new MoveInContact();

        /// <summary>An alternate contact, recorded on the alternate page.</summary>
        public MoveInContact Alternate { get; set; } = new MoveInContact();
    }

    /// <summary>A resident's name.</summary>
    public class ResidentName
    {
        public string? First { get; set; }

        public string? Last { get; set; }
    }

    /// <summary>A contact on the packet.</summary>
    public class MoveInContact
    {
        public string? Name { get; set; }

        public string? Phone { get; set; }
    }

    /// <summary>The confirmation the server returns once a packet or a contact is saved.</summary>
    public sealed record ContactsSaved(string Confirmation);
}
