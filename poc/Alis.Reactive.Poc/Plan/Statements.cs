using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Plan;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(SetStatement), "set")]
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
