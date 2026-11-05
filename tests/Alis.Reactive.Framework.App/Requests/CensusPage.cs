using Alis.Reactive.Framework.App.Components;

namespace Alis.Reactive.Framework.App.Requests;

public sealed class CensusPage;

public sealed class CensusQuery
{
    public string? Wing { get; set; }

    public List<string>? Wings { get; set; }
}

public sealed class CensusLabels
{
    public List<string?>? Labels { get; set; }
}

public sealed class CensusCards
{
    public IReadOnlyList<SlateCard>? Cards { get; set; }
}
