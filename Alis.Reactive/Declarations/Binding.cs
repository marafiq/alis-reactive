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

    internal virtual Expression WriteTarget => Target;

    internal virtual Expression Arguments
        => throw new InvalidOperationException("An argument at a position is what a listener or a callback receives: declare it on an event's payload or a callback's arguments.");

    internal abstract BlockWriter Writer { get; }

    internal abstract PathMembers Reads(bool optional);

    internal virtual bool IsAnEventsPayload => false;

    internal virtual bool MayBeAbsent => false;
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

    internal override bool MayBeAbsent => _mayBeAbsent;

    internal override PathMembers Reads(bool optional)
        => _mayBeAbsent
            ? optional ? PathMembers.Optional : PathMembers.RequiredIfHeld
            : optional ? PathMembers.OptionalMember : PathMembers.Required;
}

internal sealed class Received(BlockWriter writer, string payload = "e", string arguments = "arguments") : Binding
{
    private readonly int _listenDepth = writer.ListenDepth;

    internal override Expression Target { get; } = new RefExpression(payload);

    internal override Expression WriteTarget
    {
        get
        {
            if (Writer.ListenDepth > _listenDepth)
                throw new InvalidOperationException("A listen cannot write the payload of the event around it: the vendor read that payload when its listener returned. Write it before the listen step.");
            if (Writer.PathAwaited)
                throw new InvalidOperationException("A write to the event's payload after an awaited step reaches no vendor: write it before the first request, Confirm or async method.");
            return Target;
        }
    }

    internal override Expression Arguments { get; } = new RefExpression(arguments);

    internal override BlockWriter Writer { get; } = writer;

    internal override PathMembers Reads(bool optional) => optional ? PathMembers.OptionalMember : PathMembers.Required;

    internal override bool IsAnEventsPayload => true;
}

internal sealed class AnswerArguments : Binding
{
    internal AnswerArguments(BlockWriter writer, string name)
    {
        Writer = writer;
        Arguments = new RefExpression(name);
        Target = new GetExpression([new IndexStep(0)], PathMembers.Required, Arguments);
    }

    internal override Expression Target { get; }

    internal override Expression Arguments { get; }

    internal override BlockWriter Writer { get; }

    internal override PathMembers Reads(bool optional) => optional ? PathMembers.OptionalMember : PathMembers.Required;
}
