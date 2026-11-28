using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Alis.Reactive.Conformance.App.World;

internal sealed class ExportedActivities(CollectorChanges changes) : Collection<Activity>
{
    private readonly Lock _held = new();

    internal IReadOnlyList<Activity> Now()
    {
        lock (_held) return [.. Items];
    }

    protected override void InsertItem(int index, Activity item)
    {
        lock (_held) base.InsertItem(index, item);
        changes.Changed();
    }
}
