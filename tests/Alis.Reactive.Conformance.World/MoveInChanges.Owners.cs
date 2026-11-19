namespace Alis.Reactive.Conformance.World;

public static partial class MoveInChanges
{
    public static Seed TheAssessmentIsSuperseded(string reference) => new() { MoveIn = new MoveInChange { CurrentAssessment = reference } };

    public static Seed TheRoomHandoffIsLocked() => new() { MoveIn = new MoveInChange { RoomHandoffLocked = true } };

    public static Seed TheRoomSearchConnectionIsRestored() => new() { MoveIn = new MoveInChange { RoomSearchConnectionRestored = true } };

    public static Seed TheRoomDirectoryIsAvailable(bool available) => new() { MoveIn = new MoveInChange { RoomDirectoryAvailable = available } };
}
