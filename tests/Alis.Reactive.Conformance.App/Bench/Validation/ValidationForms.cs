using System.Collections.Generic;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class ConformanceForm
{
    public string? Code { get; set; }

    public bool IsStrict { get; set; }
}

public sealed class ConformanceEntry
{
    public string? Code { get; set; }
}

public sealed class ConformanceEntries
{
    public List<ConformanceEntry> Entries { get; set; } = [];
}

public sealed class ConformanceFirst
{
    public ConformanceEntry First { get; set; } = new();
}
