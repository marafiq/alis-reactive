using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Plan;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(SetStatement), "set")]
[JsonDerivedType(typeof(DoStatement), "do")]
[JsonDerivedType(typeof(LetStatement), "let")]
internal abstract class Statement
{
    private protected Statement()
    {
    }
}

internal sealed class SetStatement : Statement
{
    internal SetStatement(Expression target, IReadOnlyList<PathStep> path, Presence members, Expression value)
    {
        Target = target;
        Path = path;
        Members = members;
        Value = value;
    }

    public Expression Target { get; }

    public IReadOnlyList<PathStep> Path { get; }

    public Presence Members { get; }

    public Expression Value { get; }
}

internal sealed class DoStatement : Statement
{
    internal DoStatement(Expression expr)
    {
        Expr = expr;
    }

    public Expression Expr { get; }
}

internal sealed class LetStatement : Statement
{
    internal LetStatement(string name, Expression expr)
    {
        Name = name;
        Expr = expr;
    }

    public string Name { get; }

    public Expression Expr { get; }
}
