using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInReads
{
    internal IReadOnlyList<SelectListItem> Relationships()
        => [.. Enum.GetValues<ContactRelationship>().Select(relationship => new SelectListItem(relationship.ToString(), relationship.ToString()))];

    internal ContactEdit NewContact() => new() { NewContact = new ContactInput() };

    internal IReadOnlyList<ContactOnFile> ContactsOnFile(MoveInPage page)
        => [.. page.Contacts.Rows.Select(contact => new ContactOnFile(contact, OnFile(contact, page.Payer)))];

    private static string OnFile(ContactInput contact, PayerInput payer)
    {
        var held = $"{contact.Name}, {contact.Relationship}, {contact.Phone}, {Held(contact)}";
        var isThePayersContact = contact.Id.Length > 0 && payer.ContactId == contact.Id;
        return isThePayersContact ? $"{held}, payer contact for {payer.Primary.PolicyNumber}" : held;
    }

    private static string Held(ContactInput contact) => contact.Role switch
    {
        AuthorityRole.PowerOfAttorney => $"power of attorney {contact.Authority?.Reference}".TrimEnd(),
        AuthorityRole.Guardian => $"guardian {contact.Authority?.Reference}".TrimEnd(),
        _ => "ordinary contact",
    };
}

internal sealed record ContactOnFile(ContactInput Contact, string Line);
