namespace Alis.Reactive.Conformance.World;

public static partial class CarePages
{
    public const string BedsideRecord = Record + "bedside";
    public const string RoomFreeRecordWithTheRoomFree = Record + "room-free/free";
    public const string RoomFreeRecordWithTheRoomTaken = Record + "room-free/taken";
    public const string CareNoteRecordWithANote = Record + "care-note/noted";
    public const string CareNoteRecordWithNoNote = Record + "care-note/none";
    public const string FluidsChart = Record + "fluids-chart";
    public const string FluidsChartLabelledByArity = Record + "fluids-chart/labelled-by-arity";
    public const string ChartCards = Record + "chart-cards";
    public const string ResidentNameCardWithMarkupInTheName = Record + "resident-name-card/markup";
    public const string ResidentNameCardWithAPlainName = Record + "resident-name-card/plain";
}
