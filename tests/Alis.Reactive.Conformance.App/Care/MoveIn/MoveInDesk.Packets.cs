namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInDesk
{
    private readonly List<(string Reference, PrintIntent Intent)> _printIntents = [];
    private bool _packetSealed;
    private bool _packetUnavailable;

    internal AdmissionPacketDesk? PacketDeskOf(string draftId)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft || !CoordinatorServes(draft.Data.CoordinatorId, draft.Data.Location.CommunityId)) return null;
            var note = _world.Acknowledgements.LastOrDefault(n => n.DraftId == draftId && n.Kind == "packet-note");
            var print = _printIntents.LastOrDefault(p => p.Intent.DraftId == draftId);
            return new(draftId, draft.AcceptedQuoteRevision,
                note is null ? "No packet note on file." : $"{note.Id} · {note.Text} · {new Uri(note.Reference).PathAndQuery}",
                print.Intent is null ? "No print intent on file." : $"{print.Reference} · {print.Intent.Signer} · revision {print.Intent.PacketRevision}");
        }
    }

    internal PacketRequirements? RequirementsOf(string draftId)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft || !CoordinatorServes(draft.Data.CoordinatorId, draft.Data.Location.CommunityId)) return null;
            var accepted = _world.Documents.Where(d => d.DraftId == draftId && draft.Data.Documents.Ids.Contains(d.Id) && d.Scan == DocumentScan.Accepted);
            var missing = draft.Data.Documents.RequiredKinds.Except(accepted.Select(d => d.Kind));
            return new(draftId, draft.AcceptedQuoteRevision, [.. missing]);
        }
    }

    internal (int Status, AdmissionPrintPacket? Packet) PacketOf(string draftId, int? revision, bool download)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { Quote: { } quote } draft) return (404, null);
            if (!CoordinatorServes(draft.Data.CoordinatorId, draft.Data.Location.CommunityId)) return (403, null);
            if (download && _packetUnavailable) return (503, null);
            if (revision != quote.Revision || draft.AcceptedQuoteRevision != quote.Revision) return (409, null);
            var room = draft.Data.Location;
            var community = CommunityOf(room.CommunityId);
            var building = BuildingAt(room.CommunityId, room.BuildingId);
            var apartment = building?.Apartments.Find(a => a.Id == room.ApartmentId);
            var bed = apartment?.Beds.Find(b => b.Id == room.BedId);
            var resident = _world.Residents.Find(r => r.Id == draft.Data.ResidentId);
            var documents = _world.Documents.Where(d => d.DraftId == draftId && draft.Data.Documents.Ids.Contains(d.Id) && d.Scan == DocumentScan.Accepted).ToList();
            if (resident is null || community is null || building is null || apartment is null || bed is null || !draft.Data.Documents.RequiredKinds.All(kind => documents.Any(d => d.Kind == kind))) return (409, null);
            var contents = FormattableString.Invariant($"Admission {draftId}\n{resident.Identity.Name}\n{community.Name} / {building.Name} / {apartment.Number} / {bed.Label}\nMonthly total: {quote.Pricing.MonthlyTotal:0.00} USD\nDeposit: {quote.Pricing.Deposit:0.00} USD\n");
            return (200, new AdmissionPrintPacket(draftId, quote.Revision, resident.Identity.Name, contents, [.. documents.Select(d => d.Name)]));
        }
    }

    internal (int Status, string Text) SavePacketNote(string draftId, string coordinatorId, string url, string text, string? coordinator, Guid operation)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft) return (404, "The draft was not found.");
            if (DocumentAuthor(draft, coordinator, operation) is { } refusal) return refusal;
            if (coordinator != coordinatorId) return (403, "This coordinator cannot save a packet note.");
            if (_packetSealed) return (422, "Packet is sealed.");
            if (string.IsNullOrWhiteSpace(text)) return (422, "Enter a packet note.");
            var reference = $"PN{100 + _world.Acknowledgements.Count(a => a.Kind == "packet-note")}";
            _world.Acknowledgements.Add(new AcknowledgementState { Id = reference, DraftId = draftId, CoordinatorId = coordinatorId, Kind = "packet-note", Reference = url, Text = text });
            Record(draft, draft.Data.OperationId, "packet-note", reference, coordinator);
            return (200, "Packet note saved");
        }
    }

    internal (int Status, SaveReply? Receipt) RequestPrint(string draftId, PrintIntent sent, string? coordinator, Guid operation)
    {
        lock (_gate)
        {
            var read = PacketOf(draftId, sent.PacketRevision, false);
            if (read.Packet is not { } packet) return (read.Status, null);
            var initials = string.Concat(packet.ResidentName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(part => part[0]));
            if (sent.Signer != initials || sent.DraftId != draftId) return (400, null);
            var draft = DraftOf(draftId)!;
            if (DocumentAuthor(draft, coordinator, operation) is { } refusal) return (refusal.Status, null);
            var reference = $"PRINT{100 + _world.History.Count(h => h.Action == "print-packet")}";
            _printIntents.Add((reference, sent));
            Record(draft, draft.Data.OperationId, "print-packet", reference, draft.Data.CoordinatorId);
            return (201, new(new(draftId, packet.Revision, reference, LabelOfTheCoordinator(draft.Data.CoordinatorId), _world.Clock.Now), new(false, string.Empty)));
        }
    }
}

internal sealed partial class MoveInReads
{
    internal AdmissionPacketDesk? PacketDeskOf(string draftId) => Desk.PacketDeskOf(draftId);
}

public sealed record AdmissionPacketDesk(string DraftId, int? AcceptedRevision, string Note, string Print);

public sealed record PacketRequirements(string DraftId, int? AcceptedRevision, IReadOnlyList<DocumentKind> MissingDocuments);

public interface IAdmissionPrintPacket
{
    string DraftId { get; }

    int Revision { get; }
}

public sealed record AdmissionPrintPacket(string DraftId, int Revision, string ResidentName, string Contents, List<string> Documents) : IAdmissionPrintPacket;

public sealed class AdmissionPacketQuery
{
    public string DraftId { get; set; } = string.Empty;

    public int? QuoteRevision { get; set; }

    public string Culture { get; set; } = string.Empty;

    public string CoordinatorId { get; set; } = string.Empty;
}

public sealed class PrintIntent
{
    public string DraftId { get; set; } = string.Empty;

    public int PacketRevision { get; set; }

    public string PageUrl { get; set; } = string.Empty;

    public string Signer { get; set; } = string.Empty;
}
