using Alis.Reactive.Conformance.App.Care;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.World;

internal sealed class TwinWorld
{
    private const string NoticeFile = "notice.txt";

    internal bool ResolverLeftUnset { get; private set; }

    internal TwinHostOrders HostOrders { get; } = new();

    internal ShiftRosterBook ShiftRoster { get; } = ShiftRosterBook.Open();

    internal CarePlanReviewBook CarePlanReview { get; } = CarePlanReviewBook.Open();

    internal DateTime? DateOfBirth { get; private set; }

    internal void Take(Seed seed, string noticeFolder)
    {
        if (seed.ResolverLeftUnset is true) ResolverLeftUnset = true;
        if (seed.DateOfBirth is { } born) DateOfBirth = born;
        if (seed.Notice is not { } notice) return;

        Directory.CreateDirectory(noticeFolder);
        File.WriteAllText(Path.Combine(noticeFolder, NoticeFile), notice);
    }
}
