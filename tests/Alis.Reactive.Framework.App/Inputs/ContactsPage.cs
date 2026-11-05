
namespace Alis.Reactive.Framework.App.Inputs;

public sealed class ContactsPage
{
    public ContactCard Primary { get; set; } = new();

    public ContactCard Alternate { get; set; } = new();
}

public sealed class ContactCard
{
    public string? Name { get; set; }

    public List<string?> Phones { get; set; } = new();
}

public sealed class ContactCardRules : ReactiveValidator<ContactCard>
{
    public ContactCardRules()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name the contact.");
        RuleForEach(x => x.Phones).NotEmpty().WithMessage("Give a phone number.");
    }
}

public sealed class ContactsEcho
{
    public string Summary { get; set; } = "";
}
