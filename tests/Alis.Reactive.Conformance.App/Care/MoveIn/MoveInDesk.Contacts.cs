using System.Globalization;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInDesk
{
    private const string HelensDraft = "D100";
    private const string AContactsId = "CT";

    internal ContactsVerdict SaveContacts(string draftId, int version, string coordinatorId, Guid operation, ContactEdit edit)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft) return new ContactsVerdict();
            if (version != draft.Version) return new ContactsVerdict { Stale = StaleAt(draft) };

            var rows = edit.Contacts.Rows.Select(AsStored).ToList();
            if (edit.NewContact is { } added)
            {
                if (rows.Count >= _world.Rules.MaximumContacts) return new ContactsVerdict { TooMany = true };

                var joined = AsStored(added);
                joined.Id = NextContactIdOf(draft);
                rows.Add(joined);
            }

            return new ContactsVerdict { Saved = KeepContacts(draft, rows, coordinatorId, operation) };
        }
    }

    internal ContactsVerdict RemoveContact(string draftId, string contactId, int version, string coordinatorId, Guid operation, ContactEdit edit)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft) return new ContactsVerdict();
            if (version != draft.Version) return new ContactsVerdict { Stale = StaleAt(draft) };
            if (draft.Data.Contacts.Rows.All(row => row.Id != contactId)) return new ContactsVerdict();

            var payer = draft.Data.Payer;
            if (payer.ContactId == contactId)
            {
                var explanation = new InstructionReference(true, "EXPLAIN-" + payer.Primary.PolicyNumber);
                return new ContactsVerdict { DependedOn = new PayerDependency(new PayerDependencyDetail(new DependentPayer(contactId, payer.Primary.PolicyNumber, explanation))) };
            }

            var rows = edit.Contacts.Rows.Where(row => row.Id != contactId).Select(AsStored).ToList();
            return new ContactsVerdict { Saved = KeepContacts(draft, rows, coordinatorId, operation) };
        }
    }

    internal string? PayerExplanationOf(string draftId, string contactId, string policyNumber)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft) return null;
            if (draft.Data.Contacts.Rows.FirstOrDefault(row => row.Id == contactId) is not { } contact) return null;

            var payer = draft.Data.Payer;
            var dependsOnTheContact = payer.ContactId == contactId && payer.Primary.PolicyNumber == policyNumber;
            return dependsOnTheContact ? $"{contact.Name} is the authorised payer contact for {policyNumber}." : null;
        }
    }

    partial void TakeForTheContacts(MoveInChange change)
    {
        TakeForTheContactAuthority(change);
        if (DraftOf(HelensDraft) is not { } draft) return;

        if (change.FourContacts == true) draft.Data.Contacts.Rows.AddRange([CaraWard(), DanWard()]);
        if (change.ContactWithNoId is { } nameless)
        {
            foreach (var row in draft.Data.Contacts.Rows.Where(row => row.Id == nameless)) row.Id = string.Empty;
        }

        if (change.PayersContact is not { } payer) return;

        draft.Data.Payer.ContactId = payer;
        foreach (var policy in _world.Policies.Where(policy => policy.Number == draft.Data.Payer.Primary.PolicyNumber)) policy.AuthorisedContactId = payer;
    }

    static partial void KeepTheContacts(MoveInPage saved, MoveInPage sent)
        => saved.Contacts = new ContactSection { Rows = [.. sent.Contacts.Rows.Select(AsStored)] };

    private SaveReply KeepContacts(DraftState draft, List<ContactInput> rows, string coordinatorId, Guid operation)
    {
        draft.Data.Contacts = new ContactSection { Rows = rows };
        draft.Version++;
        draft.SavedBy = coordinatorId;
        draft.SavedAt = _world.Clock.Now;
        var reference = "CONTACTS" + draft.Version.ToString(CultureInfo.InvariantCulture);
        Record(draft, operation, "contacts", reference);
        return new SaveReply(new SaveReceipt(draft.Id, draft.Version, reference, LabelOfTheCoordinator(draft.SavedBy), draft.SavedAt), new ReceiptReview(false, string.Empty));
    }

    private static string NextContactIdOf(DraftState draft)
    {
        var last = draft.Data.Contacts.Rows.Select(NumberOf).DefaultIfEmpty().Max();
        return AContactsId + (last + 1).ToString(CultureInfo.InvariantCulture);
    }

    private static int NumberOf(ContactInput row)
        => row.Id.StartsWith(AContactsId, StringComparison.Ordinal) && int.TryParse(row.Id.AsSpan(AContactsId.Length), NumberStyles.None, CultureInfo.InvariantCulture, out var number) ? number : 0;

    private static ContactInput AsStored(ContactInput sent) => new()
    {
        Id = sent.Id,
        Name = sent.Name,
        Relationship = sent.Relationship,
        Email = sent.Email,
        Phone = sent.Phone,
        AlternatePhone = sent.AlternatePhone,
        Role = sent.Role,
        Authority = sent.Role == AuthorityRole.Ordinary || sent.Authority is not { } authority ? null : Copied(authority),
    };

    private static ContactInput CaraWard() => new()
    {
        Id = "CT3",
        Name = "Cara Ward",
        Relationship = ContactRelationship.Friend,
        Email = "cara@example.test",
        Phone = "5185550301",
        AlternatePhone = "5185550302",
        Role = AuthorityRole.Ordinary,
    };

    private static ContactInput DanWard() => new()
    {
        Id = "CT4",
        Name = "Dan Ward",
        Relationship = ContactRelationship.Son,
        Email = "dan@example.test",
        Phone = "5185550401",
        AlternatePhone = "5185550402",
        Role = AuthorityRole.Ordinary,
    };
}

internal sealed class ContactsVerdict
{
    internal SaveReply? Saved { get; init; }

    internal VersionConflict? Stale { get; init; }

    internal PayerDependency? DependedOn { get; init; }

    internal bool TooMany { get; init; }
}
