namespace Alis.Reactive.PlanTests.Support;

public sealed class Vitals
{
    public int? Weight { get; set; }

    public DateTime MeasuredOn { get; set; }

    public DateTimeOffset RecordedAt { get; set; }

    public List<string> Notes { get; set; } = new();
}
