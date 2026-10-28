using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.Framework.App.Inputs;

/// <summary>A resident's contacts: one card of its own model for each of two members, the primary contact and the alternate.</summary>
public sealed class ContactsPage
{
    public ContactCard Primary { get; set; } = new();

    public ContactCard Alternate { get; set; } = new();
}

/// <summary>A contact card: a partial of its own model, rendered for a member of the contacts page, so its inputs bind under that member.</summary>
public sealed class ContactCard
{
    public string? Name { get; set; }

    public List<string?> Phones { get; set; } = new();
}

/// <summary>A card's rules, checked where the card's inputs bind: a name, and every phone number it lists.</summary>
public sealed class ContactCardRules : ReactiveValidator<ContactCard>
{
    public ContactCardRules()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name the contact.");
        RuleForEach(x => x.Phones).NotEmpty().WithMessage("Give a phone number.");
    }
}

/// <summary>What the endpoint bound, in its own words.</summary>
public sealed class ContactsEcho
{
    public string Summary { get; set; } = "";
}
