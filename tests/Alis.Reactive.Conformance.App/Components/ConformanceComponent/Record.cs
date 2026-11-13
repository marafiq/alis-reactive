using System;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class Record
{
    public string Name { get; set; } = null!;

    public string Code { get; set; } = "C0";

    public int Count { get; set; }

    public DateTime Day { get; set; }

    public Detail Detail { get; set; } = new();

    public int MultiWord { get; set; }

    public double Ratio { get; set; }
}
