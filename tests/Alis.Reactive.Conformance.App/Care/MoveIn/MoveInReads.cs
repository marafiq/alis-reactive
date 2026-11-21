using Alis.Reactive.Conformance.App.World;

namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInReads(CurrentWorld world)
{
    private MoveInDesk Desk => world.Data.Owns(MoveInDesk.Opened);
}
