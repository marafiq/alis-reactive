namespace Alis.Reactive.PlanTests.Support;

/// <summary>A resident's vitals: the members whose values convert on their way in, a maybe-number, a day, an instant and a list.</summary>
public sealed class Vitals
{
    public int? Weight { get; set; }

    public DateTime MeasuredOn { get; set; }

    public DateTimeOffset RecordedAt { get; set; }

    public List<string> Notes { get; set; } = new();
}
