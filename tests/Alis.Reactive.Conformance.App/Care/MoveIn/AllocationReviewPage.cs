using Alis.Reactive.Conformance.App.Components;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class AllocationReviewPage
{
    public string DraftId { get; set; } = string.Empty;

    public string? Reference { get; set; }

    public ConformanceStart ComponentStart { get; set; } = new();

    public string BedOnFile { get; set; } = string.Empty;

    public IReadOnlyList<string> Reviews { get; set; } = [];
}
