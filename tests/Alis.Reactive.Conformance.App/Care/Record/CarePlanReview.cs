using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;

namespace Alis.Reactive.Conformance.App.Care;

public sealed record CarePlanReview(string Resident, int Reviews);

public sealed record PlanReviewer(string Name);

public static class CarePlanReviewEndpoints
{
    public const string Reviewed = CarePages.CarePlanReview + "/reviewed";
}

public static class CarePlanReviews
{
    public static readonly AppEvent MarkReviewed = AppEvent.Declare();

    public static readonly AppEvent MarkReviewedAndShowTheReviewer = AppEvent.Declare();
}

internal sealed partial class CarePlanReviewBook
{
    private readonly object _gate = new();
    private int _reviews;

    internal static CarePlanReviewBook Open() => new();

    internal int Reviews
    {
        get
        {
            lock (_gate) return _reviews;
        }
    }

    internal void Review()
    {
        lock (_gate) _reviews++;
    }
}
