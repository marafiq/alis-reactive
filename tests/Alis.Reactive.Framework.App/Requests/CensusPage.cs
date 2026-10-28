using Alis.Reactive.Framework.App.Components;

namespace Alis.Reactive.Framework.App.Requests;

/// <summary>
/// The census the desk takes: what a query string and a form post carry for a list, for a value the
/// address leaves out and for a list it leaves out; what a JSON body carries for blank text; and a query
/// member whose items have no text form.
/// </summary>
public sealed class CensusPage;

/// <summary>A wing and the wings the census asks about, as the address gives them.</summary>
public sealed class CensusQuery
{
    public string? Wing { get; set; }

    public List<string>? Wings { get; set; }
}

/// <summary>Labels the census keeps, as a JSON body carries them.</summary>
public sealed class CensusLabels
{
    public List<string?>? Labels { get; set; }
}

/// <summary>The cards a slate holds, asked about in a query.</summary>
public sealed class CensusCards
{
    public IReadOnlyList<SlateCard>? Cards { get; set; }
}
