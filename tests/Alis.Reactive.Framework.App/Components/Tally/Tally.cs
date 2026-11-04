using System.Text.Json.Serialization;

namespace Alis.Reactive.Framework.App.Components;

public sealed class Tally : Plugin
{
    public Tally()
        : base("tally")
    {
    }

    public ReadOnly<int> Total => ReadOnly<int>();

    public Property<int> Mark => Property<int>();

    public ReadOnly<TallyEntry> Last => ReadOnly<TallyEntry>();

    public ReadOnly<int?> LastDay => ReadOnly<int?>();

    public void Add(Arg<int> amount) => Method(amount);

    public TypedSource<bool> HasPassedTheMark() => Method<bool>();

    public void AddEntry(Arg<TallyEntry> entry) => Method(entry);
}

public sealed class TallyEntry
{
    [JsonPropertyName("amount")]
    public int Count { get; set; }

    [JsonPropertyName("added_on")]
    public DateTime AddedOn { get; set; }
}
