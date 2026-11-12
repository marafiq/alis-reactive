namespace Alis.Reactive.Conformance.World;

public static partial class CarePages
{
    public const string Stay = Admission + "stay";
    public const string Shift = Admission + "shift";
    public const string ShiftStartingAtSeven = Shift + "?starts=07:00:00";
    public const string ShiftStartingAtHalfPastTwo = Shift + "?starts=14:30:00";
    public const string ShiftStartingATickBeforeNoon = Shift + "?starts=11:59:59.9999999";
    public const string KinMoveIn = Admission + "kin-move-in";
    public const string Circle = Admission + "circle";
    public const string ReceptionWithANameSection = Admission + "reception/name-section";
    public const string ReceptionWithAPhoneSection = Admission + "reception/phone-section";
    public const string Lobby = Admission + "lobby";
    public const string SignInWithTwoNames = Admission + "sign-in/two-names";
    public const string SignInWithANickname = Admission + "sign-in/nickname";
    public const string KinCards = Admission + "kin-cards";
    public const string GivenName = Admission + "given-name";
    public const string MoveInWithKin = Admission + "move-in-with-kin";
}
