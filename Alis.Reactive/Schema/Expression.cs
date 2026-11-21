using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Schema;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(LitExpression), "lit")]
[JsonDerivedType(typeof(ObjExpression), "obj")]
[JsonDerivedType(typeof(RefExpression), "ref")]
[JsonDerivedType(typeof(GetExpression), "get")]
[JsonDerivedType(typeof(DataExpression), "data")]
[JsonDerivedType(typeof(AsExpression), "as")]
[JsonDerivedType(typeof(CallExpression), "call")]
[JsonDerivedType(typeof(FnExpression), "fn")]
[JsonDerivedType(typeof(AnswerExpression), "answer")]
[JsonDerivedType(typeof(RecordExpression), "record")]
[JsonDerivedType(typeof(ListExpression), "list")]
[JsonDerivedType(typeof(OpExpression), "op")]
[JsonDerivedType(typeof(SignalExpression), "signal")]
internal abstract class Expression
{
    private protected Expression()
    {
    }
}

internal sealed class LitExpression : Expression
{
    internal LitExpression(JsonElement value)
    {
        Value = value;
    }

    public JsonElement Value { get; }
}

internal sealed class ObjExpression : Expression
{
    internal ObjExpression(string id)
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
    internal GetExpression(IReadOnlyList<PathStep> path, PathMembers members, Expression target)
    {
        Path = path;
        Members = members;
        Target = target;
    }

    public IReadOnlyList<PathStep> Path { get; }

    public PathMembers Members { get; }

    public Expression Target { get; }
}

internal sealed class DataExpression : Expression
{
    internal DataExpression(IReadOnlyList<DataStep> path, Expression target)
    {
        Path = path;
        Target = target;
    }

    public IReadOnlyList<DataStep> Path { get; }

    public Expression Target { get; }
}

internal sealed class AsExpression : Expression
{
    internal AsExpression(Expression expr, Shape shape)
    {
        Expr = expr;
        Shape = shape;
    }

    public Expression Expr { get; }

    public Shape Shape { get; }
}

internal sealed class CallExpression : Expression
{
    internal CallExpression(IReadOnlyList<PathStep> path, PathMembers members, Expression target, IReadOnlyList<Expression> args)
    {
        Path = path;
        Members = members;
        Target = target;
        Args = args;
    }

    public IReadOnlyList<PathStep> Path { get; }

    public PathMembers Members { get; }

    public Expression Target { get; }

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

internal sealed class RecordExpression : Expression
{
    internal RecordExpression(IReadOnlyList<RecordField> fields)
    {
        Fields = fields;
    }

    public IReadOnlyList<RecordField> Fields { get; }
}

internal sealed class RecordField
{
    internal RecordField(string name, Expression value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; }

    public Expression Value { get; }
}

internal sealed class ListExpression : Expression
{
    internal ListExpression(IReadOnlyList<Expression> items)
    {
        Items = items;
    }

    public IReadOnlyList<Expression> Items { get; }
}

internal sealed class OpExpression : Expression
{
    internal OpExpression(Operator op, IReadOnlyList<Expression> args)
    {
        Op = op;
        Args = args;
    }

    public Operator Op { get; }

    public IReadOnlyList<Expression> Args { get; }
}

internal sealed class SignalExpression : Expression
{
    internal static readonly SignalExpression Instance = new();

    private SignalExpression()
    {
    }
}
