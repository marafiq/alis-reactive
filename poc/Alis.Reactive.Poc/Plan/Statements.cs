using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Plan;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(SetStatement), "set")]
[JsonDerivedType(typeof(DoStatement), "do")]
[JsonDerivedType(typeof(LetStatement), "let")]
[JsonDerivedType(typeof(IfStatement), "if")]
[JsonDerivedType(typeof(ListenStatement), "listen")]
internal abstract class Statement
{
    private protected Statement()
    {
    }
}

internal sealed class SetStatement : Statement
{
    internal SetStatement(Expression target, IReadOnlyList<PathStep> path, Presence members, Expression value, bool content)
    {
        Target = target;
        Path = path;
        Members = members;
        Value = value;
        Content = content;
    }

    public Expression Target { get; }

    public IReadOnlyList<PathStep> Path { get; }

    public Presence Members { get; }

    public Expression Value { get; }

    public bool Content { get; }
}

internal sealed class DoStatement : Statement
{
    internal DoStatement(Expression expr, bool awaits)
    {
        Expr = expr;
        Await = awaits;
    }

    public Expression Expr { get; }

    public bool Await { get; }
}

internal sealed class LetStatement : Statement
{
    internal LetStatement(string name, Expression expr, bool awaits)
    {
        Name = name;
        Expr = expr;
        Await = awaits;
    }

    public string Name { get; }

    public Expression Expr { get; }

    public bool Await { get; }

    public LetBinding Binds { get; internal set; } = LetBinding.Line;
}

internal sealed class IfStatement : Statement
{
    internal IfStatement(Expression when, IReadOnlyList<Statement> then)
    {
        When = when;
        Then = then;
        Else = [];
    }

    public Expression When { get; }

    public IReadOnlyList<Statement> Then { get; }

    public IReadOnlyList<Statement> Else { get; internal set; }
}

internal sealed class ListenStatement : Statement
{
    internal ListenStatement(Expression target, string @event, Listening listen, Release release, IReadOnlyList<PathStep> payload, ReadPresence members, Policy policy, string payloadName, string argumentsName, IReadOnlyList<Statement> body)
    {
        Object = target;
        Event = @event;
        Listen = listen;
        Release = release;
        Payload = payload;
        Members = members;
        Policy = policy;
        PayloadName = payloadName;
        ArgumentsName = argumentsName;
        Do = body;
    }

    public Expression Object { get; }

    public string Event { get; }

    public Listening Listen { get; }

    public Release Release { get; }

    public IReadOnlyList<PathStep> Payload { get; }

    public ReadPresence Members { get; }

    public Policy Policy { get; }

    public string PayloadName { get; }

    public string ArgumentsName { get; }

    public IReadOnlyList<Statement> Do { get; }
}
