using System.Text.Json;
using Alis.Reactive.Plan;
using Alis.Reactive.Writing;

namespace Alis.Reactive.Values;

public abstract class TypedSource<T>
{
    private protected TypedSource()
    {
    }

    internal abstract Expression Expression { get; }
}

internal static class Literal
{
    internal static Expression Of<T>(T value, PlanBuild build)
        => new LiteralExpression(JsonSerializer.SerializeToElement(value, build.Values));
}
