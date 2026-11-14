namespace Alis.Reactive.Conformance.App.Care;

public interface IScriptedResidentsPage
{
    IReadOnlyList<string> Scripts { get; }
}

public static class ResidentsScripts
{
    private const string Root = "/js/care/residents/";

    public const string ServingStationWithAPagedList = Root + "serving-station-with-a-paged-list.js";
}
