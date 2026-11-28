using System.Text.Json.Serialization;

namespace Alis.Reactive.ValuesAndSources;

internal sealed class EvaluatedValue
{
    internal EvaluatedValue()
    {
    }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(Kind))]
[JsonDerivedType(typeof(NothingValueRead))]
[JsonDerivedType(typeof(HeldValueRead))]
internal abstract class ValueRead
{
    private protected ValueRead()
    {
    }

    public abstract string Kind { get; }
}

internal sealed class NothingValueRead : ValueRead
{
    internal NothingValueRead()
    {
    }

    public override string Kind => "nothing";
}

internal sealed class HeldValueRead : ValueRead
{
    internal HeldValueRead(EvaluatedValue value)
    {
        Value = value;
    }

    public override string Kind => "held";

    public EvaluatedValue Value { get; }
}
