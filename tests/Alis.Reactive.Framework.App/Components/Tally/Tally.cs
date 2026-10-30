namespace Alis.Reactive.Framework.App.Components;

/// <summary>
/// A page-wide tally (tally.ts): a plugin the page registers by name, with no element. A plugin an app
/// brings offers properties and methods; the framework's own plugins alone raise events.
/// </summary>
public sealed class Tally : Plugin
{
    public Tally()
        : base("tally")
    {
    }

    public ReadOnly<int> Total => ReadOnly<int>();

    /// <summary>The mark the tally is counting towards.</summary>
    public Property<int> Mark => Property<int>();

    public void Add(Arg<int> amount) => Method(amount);

    public TypedSource<bool> HasPassedTheMark() => Method<bool>();
}
