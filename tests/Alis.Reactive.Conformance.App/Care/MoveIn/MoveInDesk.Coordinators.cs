namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInDesk
{
    private readonly Dictionary<string, (string Draft, string Coordinator)> _lineMembers = new(StringComparer.Ordinal);

    internal bool JoinLine(string connection, string draftId, string coordinatorId)
    {
        lock (_gate)
        {
            if (!MayJoinLine(draftId, coordinatorId)) return false;
            _lineMembers[connection] = (draftId, coordinatorId);
            return true;
        }
    }

    internal void LeaveLine(string connection)
    {
        lock (_gate) _lineMembers.Remove(connection);
    }

    internal CoordinatorLineReply? ReadLine(string connection, string draftId)
    {
        lock (_gate)
        {
            if (!_lineMembers.TryGetValue(connection, out var member) || member.Draft != draftId || !MayJoinLine(draftId, member.Coordinator)) return null;
            return LineOf(draftId);
        }
    }

    internal (CoordinatorMessage? Message, CoordinatorLineReply? Snapshot, List<string> Recipients, string Error) PostToLine(
        string connection, string draftId, string coordinatorId, string text)
    {
        lock (_gate)
        {
            if (!_lineMembers.TryGetValue(connection, out var member) || member != (draftId, coordinatorId) || !MayJoinLine(draftId, coordinatorId))
                return (null, null, [], "Membership revoked.");
            if (string.IsNullOrWhiteSpace(text) || text.Length < 3 || text is "test" or "spam")
                return (null, null, [], "Enter at least three characters and avoid test or spam.");
            var message = new CoordinatorMessage
            {
                Id = $"MSG{_world.Messages.Count + 1}",
                Sequence = _world.Messages.Count(item => item.DraftId == draftId) + 1,
                DraftId = draftId,
                CoordinatorId = coordinatorId,
                Text = text,
                SentAt = _world.Clock.Now,
            };
            _world.Messages.Add(message);
            var recipients = _lineMembers.Where(pair => pair.Value.Draft == draftId && MayJoinLine(draftId, pair.Value.Coordinator))
                .Select(pair => pair.Key).ToList();
            return (message, LineOf(draftId), recipients, string.Empty);
        }
    }

    private bool MayJoinLine(string draftId, string coordinatorId)
        => DraftOf(draftId) is { Status: DraftStatus.Open } draft && draft.Data.CoordinatorId == coordinatorId
            && CoordinatorServes(coordinatorId, draft.Data.Location.CommunityId);

    private CoordinatorLineReply LineOf(string draftId)
    {
        var community = DraftOf(draftId)!.Data.Location.CommunityId;
        var people = _world.Coordinators.Where(person => person.Online && person.CommunityIds.Contains(community)).Select(person => new PersonLabel(person.Id, person.Name)).ToList();
        var messages = _world.Messages.Where(message => message.DraftId == draftId).ToList();
        return new CoordinatorLineReply(new CoordinatorLine(messages.Count, people, messages));
    }

    internal string? WaterOnFile(string draftId)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft || !CoordinatorServes(draft.Data.CoordinatorId, draft.Data.Location.CommunityId)) return null;
            var placement = draft.Data.Location;
            var bed = BuildingAt(placement.CommunityId, placement.BuildingId)?.Apartments.Find(a => a.Id == placement.ApartmentId)?.Beds.Find(b => b.Id == placement.BedId);
            return bed is null ? null : $"{bed.Label} bed: {(bed.NeedsWater ? "water requested" : "no water requested")}";
        }
    }

    internal (int Status, string Instruction) PrepareWard(string draftId, WardPreparation preparation, string? coordinator, Guid operation)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft) return (404, "Draft not found.");
            if (coordinator != preparation.CoordinatorId || !MayJoinLine(draftId, preparation.CoordinatorId)) return (403, "Membership revoked.");
            if (operation == Guid.Empty || operation != draft.Data.OperationId) return (400, "Use the current draft operation.");
            if (preparation.Message != "Bed 4 needs water" || preparation.BedId != "BED4") return (400, "Select Bed 4 for this request.");
            var apartments = CommunityOf(draft.Data.Location.CommunityId)!.Buildings.SelectMany(building => building.Apartments);
            foreach (var apartment in apartments)
            {
                if (apartment.Beds.Find(item => item.Id == preparation.BedId) is not { } bed) continue;
                if (bed.State != BedState.Available) return (409, "Bed 4 is closed.");
                bed.NeedsWater = true;
                return (200, $"Water requested for apartment {apartment.Number}, {bed.Label} bed");
            }
            return (400, "Select a bed in this community.");
        }
    }
}
