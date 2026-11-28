using Alis.Reactive.Schema;

namespace Alis.Reactive.ValuesAndSources;

internal sealed class Computed<T> : TypedSource<T>
{
    internal Computed(Expression expression, HeldForm form)
    {
        Expression = expression;
        Form = form;
    }

    internal override Expression Expression { get; }

    internal override HeldForm Form { get; }
}
