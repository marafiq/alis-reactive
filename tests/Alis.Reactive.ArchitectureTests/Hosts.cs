namespace Alis.Reactive.ArchitectureTests;

internal static class Hosts
{
    internal const string Net48 = "NET48";
    internal const string Net10 = "NET10_0_OR_GREATER";

    internal static readonly string[] SymbolsOfNet48 = [Net48];
    internal static readonly string[] SymbolsOfNet10 = [Net10, "NET6_0_OR_GREATER", "NET"];
}
