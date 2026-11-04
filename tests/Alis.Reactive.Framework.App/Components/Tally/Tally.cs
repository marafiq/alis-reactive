namespace Alis.Reactive.Framework.App.Components;

public sealed class Tally : Plugin
{
    public Tally()
        : base("tally")
    {
    }

    public ReadOnly<int> Total => ReadOnly<int>();

    public Property<int> Mark => Property<int>();

    public void Add(Arg<int> amount) => Method(amount);

    public TypedSource<bool> HasPassedTheMark() => Method<bool>();
}
