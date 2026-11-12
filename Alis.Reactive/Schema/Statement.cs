using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Schema;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(SetStatement), "set")]
[JsonDerivedType(typeof(DoStatement), "do")]
[JsonDerivedType(typeof(LetStatement), "let")]
[JsonDerivedType(typeof(IfStatement), "if")]
[JsonDerivedType(typeof(AllStatement), "all")]
[JsonDerivedType(typeof(RequestStatement), "request")]
[JsonDerivedType(typeof(ListenStatement), "listen")]
[JsonDerivedType(typeof(StopStatement), "stop")]
internal abstract class Statement
{
    private protected Statement()
    {
    }
}

internal sealed class SetStatement : Statement
{
    internal SetStatement(IReadOnlyList<PathStep> path, SetMembers members, bool content, Expression target, Expression value)
    {
        Path = path;
        Members = members;
        Content = content;
        Target = target;
        Value = value;
    }

    public IReadOnlyList<PathStep> Path { get; }

    public SetMembers Members { get; }

    public bool Content { get; }

    public Expression Target { get; }

    public Expression Value { get; }
}

internal sealed class DoStatement : Statement
{
    internal DoStatement(bool awaits, Expression expr)
    {
        Await = awaits;
        Expr = expr;
    }

    public bool Await { get; }

    public Expression Expr { get; }
}

internal sealed class LetStatement : Statement
{
    internal LetStatement(string name, bool awaits, LetBinds binds, Expression expr)
    {
        Name = name;
        Await = awaits;
        Binds = binds;
        Expr = expr;
    }

    public string Name { get; }

    public bool Await { get; }

    public LetBinds Binds { get; }

    public Expression Expr { get; }
}

[JsonConverter(typeof(ChoiceConverter<LetBinds>))]
internal sealed class LetBinds : IChoice
{
    internal static readonly LetBinds Line = new("line");
    internal static readonly LetBinds Read = new("read");

    private LetBinds(string text)
    {
        Text = text;
    }

    public string Text { get; }
}

internal sealed class IfStatement : Statement
{
    internal IfStatement(Expression when, IReadOnlyList<Statement> then, IReadOnlyList<Statement> otherwise)
    {
        When = when;
        Then = then;
        Else = otherwise;
    }

    public Expression When { get; }

    public IReadOnlyList<Statement> Then { get; }

    public IReadOnlyList<Statement> Else { get; }
}

internal sealed class AllStatement : Statement
{
    internal AllStatement(IReadOnlyList<IReadOnlyList<Statement>> blocks)
    {
        Blocks = blocks;
    }

    public IReadOnlyList<IReadOnlyList<Statement>> Blocks { get; }
}

internal sealed class RequestStatement : Statement
{
    internal RequestStatement(
        string argument,
        string reply,
        Expression send,
        IReadOnlyList<Statement> check,
        IReadOnlyList<Layer> layers,
        Route success,
        IReadOnlyList<StatusRoute> statuses,
        OtherwiseRoute otherwise,
        IReadOnlyList<Statement> chained)
    {
        Argument = argument;
        Reply = reply;
        Send = send;
        Check = check;
        Layers = layers;
        Success = success;
        Statuses = statuses;
        Otherwise = otherwise;
        Chained = chained;
    }

    public string Argument { get; }

    public string Reply { get; }

    public Expression Send { get; }

    public IReadOnlyList<Statement> Check { get; }

    public IReadOnlyList<Layer> Layers { get; }

    public Route Success { get; }

    public IReadOnlyList<StatusRoute> Statuses { get; }

    public OtherwiseRoute Otherwise { get; }

    public IReadOnlyList<Statement> Chained { get; }
}

internal sealed class ListenStatement : Statement
{
    internal ListenStatement(
        string @event,
        Listening listen,
        Release release,
        IReadOnlyList<PathStep> payload,
        PathMembers members,
        Policy policy,
        string payloadName,
        string argumentsName,
        Expression target,
        IReadOnlyList<Statement> body)
    {
        Event = @event;
        Listen = listen;
        Release = release;
        Payload = payload;
        Members = members;
        Policy = policy;
        PayloadName = payloadName;
        ArgumentsName = argumentsName;
        Object = target;
        Do = body;
    }

    public string Event { get; }

    public Listening Listen { get; }

    public Release Release { get; }

    public IReadOnlyList<PathStep> Payload { get; }

    public PathMembers Members { get; }

    public Policy Policy { get; }

    public string PayloadName { get; }

    public string ArgumentsName { get; }

    public Expression Object { get; }

    public IReadOnlyList<Statement> Do { get; }
}

internal sealed class StopStatement : Statement
{
    internal StopStatement(string reason)
    {
        Reason = reason;
    }

    public string Reason { get; }
}
