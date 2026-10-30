using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.Framework.App.Rows;

/// <summary>A resident's emergency contacts: a list the desk adds to and removes from, saved as a whole.</summary>
public sealed class ContactSheet
{
    public string? Resident { get; set; }

    /// <summary>The wing the resident lives on, which the desk keeps beside the sheet, not in it.</summary>
    public string? Wing { get; set; }

    public List<Contact> Contacts { get; set; } = new();

    /// <summary>
    /// Ada's sheet, on the garden wing, with the first <paramref name="contacts"/> of the contacts on her file: Bo, a
    /// friend, then Cy, her guardian.
    /// </summary>
    public static ContactSheet OnFile(int contacts) => new()
    {
        Resident = "Ada",
        Wing = "Garden",
        Contacts = new List<Contact>
        {
            new() { Name = "Bo", Relationship = "Friend", Phone = "555-0101" },
            new() { Name = "Cy", Relationship = "Guardian", Phone = "555-0102" },
        }.Take(contacts).ToList(),
    };
}

/// <summary>Someone the desk calls about the resident.</summary>
public sealed class Contact
{
    public string? Name { get; set; }

    public string? Relationship { get; set; }

    public string? Phone { get; set; }
}

/// <summary>The sheet as the desk posted it, and the position of the contact to take off it.</summary>
public sealed class ContactRemoval
{
    public List<Contact> Contacts { get; set; } = new();

    public int Remove { get; set; }
}

/// <summary>
/// The sheet's rules, written once for every contact it holds: each is named, by a name of their own (which only
/// the server checks), and a guardian has a phone the desk can call; on the memory care wing, every contact does.
/// </summary>
public sealed class ContactSheetRules : ReactiveValidator<ContactSheet>
{
    public ContactSheetRules()
    {
        RuleForEach(x => x.Contacts).ChildRules(contact =>
        {
            contact.RuleFor(x => x.Name).NotEmpty().WithMessage("Name each contact.");
            contact.RuleFor(x => x.Name).Must(name => name != "Front desk").WithMessage("Give the contact's own name, not the front desk.");
            contact.RuleFor(x => x.Phone).NotEmpty().WithMessage("Give the guardian a phone the desk can call.").When(x => x.Relationship == "Guardian");
        });
        When(x => x.Wing == "Memory care", () =>
            RuleForEach(x => x.Contacts).ChildRules(contact =>
                contact.RuleFor(x => x.Phone).NotEmpty().WithMessage("The memory care wing calls every contact: give each a phone.")));
    }
}

/// <summary>What the desk's endpoint saved, in its own words.</summary>
public sealed class SavedContacts
{
    public string Summary { get; set; } = "";
}
