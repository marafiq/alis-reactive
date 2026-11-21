using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInDesk
{
    internal bool HoldsRevoked(string reference)
    {
        lock (_gate) return _world.Rules.RevokedAuthorityReferences.Contains(reference);
    }

    internal IReadOnlyList<string> AdmittedResidents()
    {
        lock (_gate) return [.. _world.Residents.Where(resident => resident.ActiveAdmissionId is not null).Select(resident => resident.Id)];
    }

    internal MoveInPage AuthorityToEnter(string draftId, string contactId)
    {
        lock (_gate)
        {
            var saved = DraftOf(draftId)?.Data.Contacts.Rows ?? [];
            var found = saved.FindIndex(row => row.Id == contactId);
            var position = found < 0 ? saved.Count : found;
            var onFile = found < 0 ? null : saved[found].Authority;
            var entered = onFile is null ? new AuthorityInput { StartsOn = _world.Clock.Today } : Copied(onFile);
            var rows = Enumerable.Range(0, position).Select(_ => new ContactInput()).Append(new ContactInput { Id = contactId, Authority = entered }).ToList();
            return new MoveInPage { DraftId = draftId, Contacts = new ContactSection { Rows = rows } };
        }
    }

    internal AuthorityOnFilePage AuthorityOnFile(string draftId, string contactId)
    {
        lock (_gate)
        {
            var contact = DraftOf(draftId)?.Data.Contacts.Rows.FirstOrDefault(row => row.Id == contactId);
            return contact is { Authority: { } authority }
                ? new AuthorityOnFilePage { Contact = contact.Name, Role = contact.Role, Authority = Copied(authority) }
                : new AuthorityOnFilePage { Contact = contact?.Name ?? string.Empty };
        }
    }

    private void TakeForTheContactAuthority(MoveInChange change)
    {
        if (change.RevokedAuthority is { } revoked) _world.Rules.RevokedAuthorityReferences.Add(revoked);
    }

    private static AuthorityInput Copied(AuthorityInput authority)
        => new() { Reference = authority.Reference, StartsOn = authority.StartsOn, EndsOn = authority.EndsOn, Consent = authority.Consent };
}
