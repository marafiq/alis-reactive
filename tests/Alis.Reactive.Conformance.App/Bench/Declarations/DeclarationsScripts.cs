namespace Alis.Reactive.Conformance.App.Bench;

public interface IScriptedDeclarationsPage
{
    IReadOnlyList<string> Scripts { get; }
}

public static class DeclarationsScripts
{
    private const string Root = "/js/bench/declarations/";

    public const string PageTally = Root + "page-tally.js";
}
