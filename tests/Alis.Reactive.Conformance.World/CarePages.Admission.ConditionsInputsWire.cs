namespace Alis.Reactive.Conformance.World;

public static partial class CarePages
{
    public const string Stay = Admission + "stay";
    public const string Shift = Admission + "shift";
    public const string ShiftStartingAtSeven = Shift + "?starts=07:00:00";
    public const string ShiftStartingAtHalfPastTwo = Shift + "?starts=14:30:00";
    public const string ShiftStartingATickBeforeNoon = Shift + "?starts=11:59:59.9999999";
    public const string OpenDayBooking = Admission + "open-day-booking";
    public const string OpenDays = Admission + "open-days";
    public const string HandoverDesk = Admission + "handover-desk";
    public const string NoonStart = Admission + "noon-start";
    public const string NoonStartAtTwelve = NoonStart + "?starts=12:00";
    public const string NoonStartATickAfterTwelve = NoonStart + "?starts=12:00:00.0000001";
    public const string NoonStartAtTwelveToTheTick = NoonStart + "?starts=12:00:00.0000000";
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
