using System;
using Alis.Reactive.Execution;
using Alis.Reactive.Schema;
using Expression = Alis.Reactive.Schema.Expression;

namespace Alis.Reactive.Declarations;

internal abstract class Binding
{
    private protected Binding()
    {
    }

    internal abstract Expression Target { get; }

    internal abstract BlockWriter Writer { get; }

    internal abstract PathMembers Reads(bool optional);
}

internal sealed class NotReached : Binding
{
    internal static readonly NotReached Instance = new();

    private NotReached()
    {
    }

    internal override Expression Target => throw ADeclarationAlone();

    internal override BlockWriter Writer => throw ADeclarationAlone();

    internal override PathMembers Reads(bool optional) => throw ADeclarationAlone();

    private static InvalidOperationException ADeclarationAlone()
        => new("A declaration made with new is no object of the page. A pipeline reaches one: pipeline.Component<T>(id), or the payload an event hands its pipeline.");
}

internal sealed class Reached : Binding
{
    private readonly Func<Expression> _target;
    private readonly bool _mayBeAbsent;

    internal Reached(Expression target, BlockWriter writer, bool mayBeAbsent = false)
        : this(() => target, writer, mayBeAbsent)
    {
    }

    internal Reached(Func<Expression> target, BlockWriter writer, bool mayBeAbsent)
    {
        _target = target;
        Writer = writer;
        _mayBeAbsent = mayBeAbsent;
    }

    internal override Expression Target => _target();

    internal override BlockWriter Writer { get; }

    internal override PathMembers Reads(bool optional)
        => _mayBeAbsent
            ? optional ? PathMembers.Optional : PathMembers.RequiredIfHeld
            : optional ? PathMembers.OptionalMember : PathMembers.Required;
}
