namespace Alis.Reactive.Conformance.App.Components;

public sealed class ItemStart
{
    public string Name { get; init; } = "";

    public bool Active { get; init; }

    public int Value { get; init; }

    public int? OptionalCount { get; init; }

    public int? OptionalScore { get; init; }

    public bool LacksName { get; init; }

    public bool LacksActive { get; init; }
}
