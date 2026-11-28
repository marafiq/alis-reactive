namespace Alis.Reactive.Conformance.World;

public static partial class CarePages
{
    public const string RoomIntake = Admission + "room-intake";
    public const string RoomIntakeOfAnUnboundRoom = RoomIntake + "?Room=2a";
    public const string RoomIntakeOfRoom12 = Admission + "room-intake/room-12";
}
