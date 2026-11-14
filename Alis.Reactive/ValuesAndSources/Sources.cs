using Alis.Reactive.Schema;

namespace Alis.Reactive.ValuesAndSources;

internal interface IPlanSource
{
    Expression Expression { get; }

    HeldForm Form { get; }
}

internal static class Sources
{
    internal static Expression Of<T>(ITypedSource<T> source) => ((IPlanSource)source).Expression;

    internal static HeldForm FormOf<T>(ITypedSource<T> source) => ((IPlanSource)source).Form;
}
