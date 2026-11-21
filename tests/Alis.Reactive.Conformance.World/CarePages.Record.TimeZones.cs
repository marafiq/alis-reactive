namespace Alis.Reactive.Conformance.World;

public static partial class CarePages
{
    public const string VisitBoard = Record + "visit-board";
    public const string VisitBoardInNewYork = VisitBoard + "?zone=America/New_York";
    public const string VisitBoardWithNewYorkDesk = VisitBoard + "/new-york-desk";
    public const string VisitBoardWithNewYorkDeskInNewYork = VisitBoardWithNewYorkDesk + "?zone=America/New_York";
}
