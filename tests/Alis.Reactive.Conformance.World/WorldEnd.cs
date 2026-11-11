using System.Collections.Generic;

namespace Alis.Reactive.Conformance.World;

public sealed class WorldEnd
{
    public IList<string> GatesStillParked { get; set; } = new List<string>();
}
