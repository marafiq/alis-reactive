
namespace Alis.Reactive.Framework.App.Rows;

public sealed class ContactSheet
{
    public string? Resident { get; set; }

    public string? Wing { get; set; }

    public List<Contact> Contacts { get; set; } = new();

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

public sealed class Contact
{
    public string? Name { get; set; }

    public string? Relationship { get; set; }

    public string? Phone { get; set; }
}

public sealed class ContactRemoval
{
    public List<Contact> Contacts { get; set; } = new();

    public int Remove { get; set; }
}

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

public sealed class SavedContacts
{
    public string Summary { get; set; } = "";
}
