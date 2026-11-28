using System;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class Linked
{
    public DateTime Day { get; set; }

    public Linked? Previous { get; set; }
}
