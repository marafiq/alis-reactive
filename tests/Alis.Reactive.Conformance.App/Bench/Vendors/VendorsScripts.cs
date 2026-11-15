namespace Alis.Reactive.Conformance.App.Bench;

public interface IScriptedVendorsPage
{
    IReadOnlyList<string> Scripts { get; }
}

public static class VendorsScripts
{
    private const string Root = "/js/bench/vendors/";

    public const string BenchClock = Root + "bench-clock.js";

    public const string PageTally = Root + "page-tally.js";
}
