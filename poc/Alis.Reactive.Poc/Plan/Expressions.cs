using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Plan;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(LiteralExpression), "lit")]
[JsonDerivedType(typeof(ObjectExpression), "obj")]
[JsonDerivedType(typeof(RefExpression), "ref")]
[JsonDerivedType(typeof(GetExpression), "get")]
[JsonDerivedType(typeof(DataExpression), "data")]
[JsonDerivedType(typeof(CallExpression), "call")]
[JsonDerivedType(typeof(FnExpression), "fn")]
[JsonDerivedType(typeof(AnswerExpression), "answer")]
[JsonDerivedType(typeof(OperatorExpression), "op")]
internal abstract class Expression
{
    private protected Expression()
    {
    }
}

internal sealed class LiteralExpression : Expression
{
    internal LiteralExpression(JsonElement value)
    {
        Value = value;
    }

    public JsonElement Value { get; }
}

internal sealed class ObjectExpression : Expression
{
    internal ObjectExpression(string id)
    {
        Id = id;
    }

    public string Id { get; }
}

internal sealed class RefExpression : Expression
{
    internal RefExpression(string name)
    {
        Name = name;
    }

    public string Name { get; }
}

internal sealed class GetExpression : Expression
{
    internal GetExpression(Expression target, IReadOnlyList<PathStep> path, ReadPresence members)
    {
        Target = target;
        Path = path;
        Members = members;
    }

    public Expression Target { get; }

    public IReadOnlyList<PathStep> Path { get; }

    public ReadPresence Members { get; }
}

internal sealed class DataExpression : Expression
{
    internal DataExpression(Expression target, IReadOnlyList<DataStep> path)
    {
        Target = target;
        Path = path;
    }

    public Expression Target { get; }

    public IReadOnlyList<DataStep> Path { get; }
}

internal sealed class CallExpression : Expression
{
    internal CallExpression(Expression target, IReadOnlyList<PathStep> path, ReadPresence members, IReadOnlyList<Expression> args)
    {
        Target = target;
        Path = path;
        Members = members;
        Args = args;
    }

    public Expression Target { get; }

    public IReadOnlyList<PathStep> Path { get; }

    public ReadPresence Members { get; }

    public IReadOnlyList<Expression> Args { get; }
}

internal sealed class FnExpression : Expression
{
    internal FnExpression(IReadOnlyList<string> parameters, Expression body)
    {
        Params = parameters;
        Body = body;
    }

    public IReadOnlyList<string> Params { get; }

    public Expression Body { get; }
}

internal sealed class AnswerExpression : Expression
{
    internal AnswerExpression(string arguments, Expression body)
    {
        Arguments = arguments;
        Body = body;
    }

    public string Arguments { get; }

    public Expression Body { get; }
}

internal sealed class OperatorExpression : Expression
{
    internal OperatorExpression(Operator op, IReadOnlyList<Expression> args)
    {
        Op = op;
        Args = args;
    }

    public Operator Op { get; }

    public IReadOnlyList<Expression> Args { get; }
}
