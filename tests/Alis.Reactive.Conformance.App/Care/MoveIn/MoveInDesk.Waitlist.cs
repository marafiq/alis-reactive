using System.Globalization;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInDesk
{
    private readonly HashSet<string> _closedWaitlists = new(StringComparer.Ordinal);

    partial void TakeForTheWaitlist(MoveInChange change)
    {
        if (change.ClosedWaitlistCommunity is { } community) _closedWaitlists.Add(community);
    }

    internal (int Status, WaitlistTermsPage? Terms) OfferedWaitlistTerms(string draftId, string termsId, string? bedId)
    {
        lock (_gate)
        {
            var draft = DraftOf(draftId);
            var community = _world.Communities.FirstOrDefault(community => "WT-" + community.Id == termsId);
            if (draft is null || community is null) return (404, null);
            if (!_world.Coordinators.Any(person => person.Id == draft.Data.CoordinatorId && person.CommunityIds.Contains(community.Id))) return (403, null);
            if (bedId is null) return (200, new WaitlistTermsPage(string.Empty, null));

            var bed = community.Buildings.SelectMany(building => building.Apartments).SelectMany(apartment => apartment.Beds).FirstOrDefault(bed => bed.Id == bedId);
            if (bed is not { State: BedState.Held, Hold: { } hold, Holder: { } holder }) return (404, null);
            var zone = community.TimeZoneId.Length == 0 ? _world.Clock.TimeZoneId : community.TimeZoneId;
            var until = TimeZoneInfo.ConvertTime(hold.ExpiresAt, TimeZoneInfo.FindSystemTimeZoneById(zone));
            return (200, new WaitlistTermsPage(
                "Bed held by " + holder.Name + " until " + until.ToString("HH:mm", CultureInfo.InvariantCulture),
                new WaitlistPage { DraftId = draft.Id, ResidentId = draft.Data.ResidentId, CommunityId = community.Id, CareLevel = draft.Data.Care.Level }));
        }
    }

    internal WaitlistRecordPage WaitlistOnFile(string draftId)
    {
        lock (_gate)
        {
            var draft = DraftOf(draftId);
            var community = draft is null ? null : CommunityOf(draft.Data.Location.CommunityId);
            var entry = _world.Waitlist.FirstOrDefault(entry => entry.DraftId == draftId);
            var residentName = entry is null ? string.Empty : _world.Residents.Single(resident => resident.Id == entry.ResidentId).Identity.Name;
            var record = entry is null ? "No waitlist entry." : $"{entry.Id}: {residentName}, {CommunityOf(entry.CommunityId)?.Name}, {entry.CareLevel}, position {entry.Position.ToString(CultureInfo.InvariantCulture)}. {entry.Reason}";
            var state = community is null ? string.Empty : _closedWaitlists.Contains(community.Id) ? community.Name + " is closed to waitlist entries." : community.Name + " accepts waitlist entries.";
            return new WaitlistRecordPage(record, draft?.Status.ToString() ?? string.Empty, state);
        }
    }

    internal WaitlistVerdict JoinTheWaitlist(string draftId, WaitlistPage sent, string coordinator, Guid operation)
    {
        lock (_gate)
        {
            var draft = DraftOf(draftId);
            var community = CommunityOf(sent.CommunityId);
            if (draft is null || community is null) return new(404, null, MoveInErrors.Missing());
            if (draft.Status != DraftStatus.Open) return new(410, null, MoveInErrors.Closed());
            if (!_world.Coordinators.Any(person => person.Id == coordinator && person.CanAdmit && person.CommunityIds.Contains(community.Id)))
                return new(403, null, MoveInErrors.Forbidden());
            if (sent.ResidentId != draft.Data.ResidentId || sent.DraftId != draftId || !_world.CareLevels.Any(level => level.Level == sent.CareLevel))
                return new(400, null, MoveInErrors.Refused("waitlist-details", "The waitlist details do not match this draft."));
            if (_closedWaitlists.Contains(community.Id))
                return new(422, null, MoveInErrors.Refused("waitlist-closed", community.Name + " is closed to waitlist entries."));
            if (_world.Waitlist.Any(entry => entry.DraftId == draftId && entry.CommunityId == community.Id))
                return new(409, null, MoveInErrors.Refused("already-waitlisted", "This draft already has a waitlist entry."));
            var entry = new WaitlistState
            {
                Id = "W" + (100 + _world.Waitlist.Count).ToString(CultureInfo.InvariantCulture),
                DraftId = draftId,
                CommunityId = community.Id,
                CareLevel = sent.CareLevel,
                ResidentId = sent.ResidentId,
                Reason = sent.Reason,
                Position = 1 + _world.Waitlist.Count(other => other.CommunityId == community.Id && other.CareLevel == sent.CareLevel),
            };
            _world.Waitlist.Add(entry);
            Record(draft, operation, "waitlist", entry.Id);
            return new(201, entry, null);
        }
    }
}

internal sealed record WaitlistVerdict(int Status, WaitlistState? Entry, ErrorEnvelope? Error);
