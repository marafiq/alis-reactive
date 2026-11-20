using System.Text;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInDesk
{
    private bool _metadataUnavailable;

    partial void TakeForTheDocuments(MoveInChange change)
    {
        if (change.ConsentRevisionChanged && _world.Documents.Find(d => d.DraftId == HelensDraft && d.Id == "DOC1") is { } consent)
        {
            consent.Revision++;
            consent.Caption = "Reviewed";
        }
        if (change.DocumentMetadataUnavailable) _metadataUnavailable = true;
        if (change.PacketSealed) _packetSealed = true;
        if (change.PacketServiceUnavailable) _packetUnavailable = true;
    }

    internal SignaturePage? SignatureOf(string draftId)
    {
        lock (_gate)
        {
            var saved = _world.Documents.Where(d => d.DraftId == draftId && d.Scan == DocumentScan.Accepted);
            var selected = saved.LastOrDefault(d => d.Kind == DocumentKind.Signature) ?? saved.FirstOrDefault(d => d.Kind == DocumentKind.Consent);
            if (selected is null || DraftOf(draftId) is not { } draft || !CoordinatorServes(draft.Data.CoordinatorId, draft.Data.Location.CommunityId)) return null;
            return new SignaturePage(draftId, selected.Id, selected.Name, selected.ContentType == "text/plain" ? Encoding.UTF8.GetString(selected.Bytes) : selected.ContentType);
        }
    }

    internal (int Status, DocumentMetadata? Metadata, string? Caption) InspectDocument(string draftId, string documentId)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft || _world.Documents.Find(d => d.DraftId == draftId && d.Id == documentId) is not { } document) return (404, null, null);
            if (!CoordinatorServes(draft.Data.CoordinatorId, draft.Data.Location.CommunityId)) return (403, null, null);
            if (_metadataUnavailable) return (503, null, null);
            return (200, MetadataOf(document), document.Caption);
        }
    }

    internal (int Status, DocumentReply? Reply, string Reason) ReplaceSignature(string draftId, string documentId, string? etag, SignatureRequest sent, byte[] bytes, string? coordinator, Guid operation)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft || _world.Documents.Find(d => d.DraftId == draftId && d.Id == documentId) is not { } source)
                return (404, null, "The document was not found.");
            if (DocumentAuthor(draft, coordinator, operation) is { } refusal) return (refusal.Status, null, refusal.Text);
            if (draft.Status != DraftStatus.Open) return (410, null, "This draft is closed.");
            var signature = _world.Documents.Find(d => d.DraftId == draftId && d.Kind == DocumentKind.Signature);
            if (etag != $"\"{source.Revision}\"" || signature is not null && signature.Id != documentId)
                return (409, null, "The document changed. Inspect the current version.");
            if (source.Kind is not (DocumentKind.Consent or DocumentKind.Signature) || sent.Size != bytes.LongLength || bytes.Length is < 1 or > 1048576
                || !sent.Name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) || sent.Type != "text/plain" || _world.Rules.QuarantinedFileContents.Contains(Encoding.UTF8.GetString(bytes)))
            {
                return (422, null, "The signature file was refused.");
            }

            if (signature is null)
            {
                signature = new DocumentState { Id = $"DOC{_world.Documents.Count + 1}", Kind = DocumentKind.Signature, DraftId = draftId };
                _world.Documents.Add(signature);
                draft.Data.Documents.Ids.Add(signature.Id);
            }
            var revision = source.Revision + 1;
            signature.Name = sent.Name;
            signature.ContentType = sent.Type;
            signature.LastModified = sent.LastModified;
            signature.Size = bytes.LongLength;
            signature.Bytes = bytes;
            signature.Caption = sent.Caption is { } caption ? Uri.UnescapeDataString(caption) : source.Caption;
            signature.Revision = revision;
            signature.Scan = DocumentScan.Accepted;
            signature.UploadedBy = draft.Data.CoordinatorId;
            return (200, new(new(draftId, new([MetadataOf(signature)], new(false, string.Empty)))), string.Empty);
        }
    }

    private static DocumentMetadata MetadataOf(DocumentState file) => new(file.Id, file.Name, file.ContentType, file.Bytes.LongLength, file.Revision, file.LastModified);
}

internal sealed partial class MoveInReads
{
    internal SignaturePage? SignatureOf(string draftId) => Desk.SignatureOf(draftId);
}

public sealed record SignaturePage(string DraftId, string DocumentId, string Name, string Contents);
