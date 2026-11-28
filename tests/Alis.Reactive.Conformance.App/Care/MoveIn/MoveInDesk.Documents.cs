using System.Text;

namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInDesk
{
    private readonly Dictionary<(string Draft, string Reference), string> _documentReceipts = [];

    internal const long MaximumDocumentBytes = 1048576;

    internal (int Status, UploadPolicy? Policy) UploadPolicyOf(string draftId, DocumentKind kind)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft) return (404, null);
            if (!CoordinatorServes(draft.Data.CoordinatorId, draft.Data.Location.CommunityId)) return (403, null);
            if (draft.Status != DraftStatus.Open || _world.Rules.DocumentUploadLocked) return (423, null);
            return Enum.IsDefined(kind) ? (200, new UploadPolicy(true, MaximumDocumentBytes, ["text/plain", "application/pdf"], 2)) : (400, null);
        }
    }

    private (int Status, string Text)? DocumentAuthor(DraftState draft, string? coordinator, Guid operation)
    {
        if (coordinator != draft.Data.CoordinatorId || !CoordinatorServes(coordinator, draft.Data.Location.CommunityId)) return (403, "This coordinator cannot change this draft.");
        if (operation == Guid.Empty || operation != draft.Data.OperationId) return (400, "Use the current draft operation.");
        return null;
    }

    private static bool HasDocumentExtension(DocumentState file) => file.ContentType switch
    {
        "text/plain" => file.Name.EndsWith(".txt", StringComparison.Ordinal),
        "application/pdf" => file.Name.EndsWith(".pdf", StringComparison.Ordinal),
        _ => false,
    };

    internal DocumentsPage? DocumentForm(string draftId)
    {
        lock (_gate)
        {
            return DraftOf(draftId) is { } draft ? new DocumentsPage { DraftId = draft.Id, Version = draft.Version, Caption = "Admission documents", Kind = DocumentKind.Consent } : null;
        }
    }

    internal IReadOnlyList<DocumentRegisterEntry> DocumentsOnFile(string draftId)
    {
        lock (_gate) return [.. _world.Documents.Where(d => d.DraftId == draftId).Select(d => new DocumentRegisterEntry(d.Id, d.Name, d.Bytes.Length, d.Kind, d.Caption))];
    }

    internal (int Status, DocumentReply? Reply, string Reason) AcceptDocuments(string draftId, DocumentsPage form, List<DocumentState> batch, string? coordinator, Guid operation)
    {
        lock (_gate)
        {
            var policy = UploadPolicyOf(draftId, form.Kind);
            if (policy.Policy is null) return (policy.Status, null, "Document uploads are unavailable.");
            var draft = DraftOf(draftId)!;
            if (DocumentAuthor(draft, coordinator, operation) is { } refusal) return (refusal.Status, null, refusal.Text);
            if (draft.Version != form.Version) return (409, null, "The draft version has changed.");
            if (batch.Count == 0 || batch.Any(file => file.Bytes.LongLength > policy.Policy.MaximumBytes || !policy.Policy.Types.Contains(file.ContentType) || !HasDocumentExtension(file)))
                return (400, null, "Choose supported admission files.");
            if (batch.Any(file => _world.Rules.QuarantinedFileContents.Contains(Encoding.UTF8.GetString(file.Bytes))))
                return (422, null, "A document failed its scan. No documents accepted.");
            var accepted = new List<DocumentMetadata>();
            foreach (var document in batch)
            {
                document.Id = $"DOC{_world.Documents.Count + 1}";
                document.DraftId = draftId;
                document.Scan = DocumentScan.Accepted;
                document.Revision = 1;
                document.UploadedBy = draft.Data.CoordinatorId;
                _world.Documents.Add(document);
                draft.Data.Documents.Ids.Add(document.Id);
                accepted.Add(new(document.Id, document.Name, document.ContentType, document.Size, document.Revision, document.LastModified));
            }
            var receipt = $"DOCUMENTS{_documentReceipts.Count + 1}";
            _documentReceipts.Add((draftId, receipt), $"{accepted.Count} documents received: {string.Join(", ", accepted.Select(d => d.Name))}.");
            return (201, new(new(draftId, new(accepted, new(true, receipt)))), string.Empty);
        }
    }
}

internal sealed partial class MoveInReads
{
    internal DocumentsPage? DocumentForm(string draftId) => Desk.DocumentForm(draftId);

    internal IReadOnlyList<DocumentRegisterEntry> DocumentsOnFile(string draftId) => Desk.DocumentsOnFile(draftId);
}

internal sealed record DocumentRegisterEntry(string Id, string Name, int Size, DocumentKind Kind, string Caption);
