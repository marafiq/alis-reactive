namespace Alis.Reactive.Conformance.App.Components;

public sealed class RaiseStart
{
    public string Button { get; init; } = "";

    public string? NullableText { get; init; }

    public string? OptionalText { get; init; }

    public string? Text { get; init; }

    public bool NoArgument { get; init; }

    public bool Flag { get; init; }

    public int Count { get; init; }

    public bool EmptyRecord { get; init; }

    public RecordStart? Record { get; init; }
}
