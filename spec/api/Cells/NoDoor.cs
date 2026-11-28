using System;
using Alis.Reactive.Values;

namespace Cells;

public static class NoDoor
{
    public static void Position(string door) => throw new NotSupportedException(door);

    public static TypedSource<string?> Source(string door) => throw new NotSupportedException(door);
}
