using System.Collections.Generic;

namespace Alis.Reactive.Conformance.World;

public sealed class Seed
{
    public IList<string> Residents { get; set; } = new List<string>();

    public string? Word { get; set; }

    public int? CachedRoundsPlanVersion { get; set; }

    public string? ComponentText { get; set; }

    public static Seed Resident(string name) => new Seed().AndResident(name);

    public static Seed TheWord(string word) => new() { Word = word };

    public static Seed CachedRoundsOfPlanVersion(int version) => new() { CachedRoundsPlanVersion = version };

    public static Seed TheComponentsText(string text) => new() { ComponentText = text };

    public Seed AndResident(string name)
    {
        Residents.Add(name);
        return this;
    }
}
