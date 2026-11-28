namespace Alis.Reactive.Conformance.World;

public sealed partial class MoveInChange
{
    public string? CurrentAssessment { get; set; }

    public bool? RoomHandoffLocked { get; set; }

    public bool? RoomDirectoryAvailable { get; set; }

    public bool RoomSearchConnectionRestored { get; set; }
}
