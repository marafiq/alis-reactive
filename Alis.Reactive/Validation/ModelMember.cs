using System;
using System.Linq.Expressions;
using Alis.Reactive.InputsAndIds;

namespace Alis.Reactive.Validation;

internal sealed class ModelMember<T, TValue>
{
    private readonly Func<T, TValue> _read;

    internal ModelMember(Expression<Func<T, TValue>> expression)
    {
        Name = MvcText.Of(expression);
        _read = ServerExpressions.Read(expression);
    }

    internal string Name { get; }

    internal TValue Read(T model) => _read(model);
}
