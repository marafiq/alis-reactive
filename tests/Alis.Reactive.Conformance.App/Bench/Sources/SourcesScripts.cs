namespace Alis.Reactive.Conformance.App.Bench;

public interface IScriptedSourcesPage
{
    IReadOnlyList<string> Scripts { get; }
}

public static class SourcesScripts
{
    private const string Root = "/js/bench/sources/";

    public const string PageLog = Root + "page-log.js";
}
