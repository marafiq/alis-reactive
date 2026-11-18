namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInDesk
{
    private const string TheAllocationsReceipts = "ALLOC";
    private const string TheAllocationsReviews = "AR";

    internal ReceiptPage ReceiptOf(string draftId, string reference)
    {
        lock (_gate)
        {
            var review = _world.Reviews.FirstOrDefault(found => found.DraftId == draftId && ReferenceOfTheReceiptOf(found) == reference);
            return new ReceiptPage { Reference = reference, Text = review is null ? "No receipt of that reference is on file." : TheReceiptOf(review) };
        }
    }

    private static string ReferenceOfTheReceiptOf(ReviewState review)
        => review.Kind == ReviewKind.Allocation ? TheAllocationsReceipts + review.Id[TheAllocationsReviews.Length..] : review.Id;

    private string TheReceiptOf(ReviewState review) => review.Kind == ReviewKind.Allocation
        ? $"{review.Reference}: prepare {review.Message} for {ResidentOf(review.DraftId)}."
        : $"Identity review {review.Id}: {review.Message}.";

    private string ResidentOf(string draftId)
    {
        var residentId = DraftOf(draftId)?.Data.ResidentId;
        return _world.Residents.FirstOrDefault(found => found.Id == residentId)?.Identity.Name ?? string.Empty;
    }
}
