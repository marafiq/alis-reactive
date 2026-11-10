using Alis.Reactive.Components;
using Alis.Reactive.Values;

namespace Cells;

public sealed partial class CellBoard
{
    public void Mark(Arg<string?> text) => Method([text]);
}
