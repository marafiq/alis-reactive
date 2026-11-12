using System;

namespace Alis.Reactive.Conformance.App.Components;

[Flags]
public enum CareDays
{
    None = 0,
    Mon = 1,
    Tue = 2,
    Wed = 4,
    Thu = 8,
    Fri = 16,
    Sat = 32,
    Sun = 64,
    Weekend = Sat | Sun,
}
