namespace Alis.Reactive.Conformance.App.Components;

public sealed class ConformanceStart
{
    public string Text { get; init; } = "";

    public string? Value { get; init; }

    public decimal Amount { get; init; }

    public OuterStart Outer { get; init; } = new();

    public string ReadOnlyText { get; init; } = "";

    public string? CurrentItem { get; init; }
}
