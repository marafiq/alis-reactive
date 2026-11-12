using System;
using Alis.Reactive.Execution;
using Expression = Alis.Reactive.Schema.Expression;

namespace Alis.Reactive.Declarations;

internal abstract class Binding
{
    private protected Binding()
    {
    }

    internal abstract Expression Target { get; }

    internal abstract BlockWriter Writer { get; }
}

internal sealed class NotReached : Binding
{
    internal static readonly NotReached Instance = new();

    private NotReached()
    {
    }

    internal override Expression Target => throw ADeclarationAlone();

    internal override BlockWriter Writer => throw ADeclarationAlone();

    private static InvalidOperationException ADeclarationAlone()
        => new("A declaration made with new is no object of the page. A pipeline reaches one: pipeline.Component<T>(id), or the payload an event hands its pipeline.");
}

internal sealed class Reached : Binding
{
    internal Reached(Expression target, BlockWriter writer)
    {
        Target = target;
        Writer = writer;
    }

    internal override Expression Target { get; }

    internal override BlockWriter Writer { get; }
}
