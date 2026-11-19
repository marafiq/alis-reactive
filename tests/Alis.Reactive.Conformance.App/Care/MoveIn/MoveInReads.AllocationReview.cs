using Alis.Reactive.Conformance.App.Components;

namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInReads
{
    private const string TheReviewsDay = "2026-11-18";

    internal AllocationReviewPage AllocationReviewOf(MoveInPage page) => new()
    {
        DraftId = page.DraftId,
        Reference = page.Review.Reference,
        BedOnFile = Desk.BedOnFileOf(page.Location.BedId),
        Reviews = Desk.AllocationReviewsOf(page.DraftId),
        ComponentStart = ReviewPanelOf(page),
    };

    internal ConformanceStart ReviewPanelOf(MoveInPage page) => new()
    {
        Text = "Service review",
        Value = "DIN",
        Options = ["none", "DIN", "PHA", "TRA"],
        Count = 2,
        Amount = 240.00m,
        Enabled = Desk.MayAdmit(page.CoordinatorId),
        Outer = new OuterStart { InnerText = TheDeskOf(page.Location.CommunityId) },
        CheckEndpoint = MoveInEndpoints.ReviewCheckOf(page.DraftId),
        ReadOnlyText = page.Location.CommunityId,
        CurrentItem = "Dining",
        Items =
        [
            new ItemStart { Name = "Dining", Value = 2, Active = true },
            new ItemStart { Name = "Pharmacy support", Value = 1, Active = true },
            new ItemStart { Name = "Transport", Value = 0, Active = false },
        ],
        Records =
        [
            new RecordStart { Name = "Dining", Code = RecordCode.Text("DIN"), Count = 2, Day = TheReviewsDay, Detail = new DetailStart { Name = "Meals" } },
            new RecordStart { Name = "Pharmacy support", Code = RecordCode.Text("PHA"), Count = 1, Day = TheReviewsDay, Detail = new DetailStart { Name = "Medication help" } },
            new RecordStart { Name = "Transport", Code = RecordCode.Text("TRA"), Count = 0, Day = TheReviewsDay, Detail = new DetailStart { Name = "Travel" } },
        ],
        Record = new RecordStart { Name = "Service review", Code = RecordCode.Text("Q3"), Count = 2, Day = TheReviewsDay, Detail = new DetailStart { Name = "Capacity checked" } },
    };

    private string TheDeskOf(string communityId) => Desk.SiteOf(communityId) is { Length: > 0 } site ? site + " allocation desk" : "Allocation desk";
}
