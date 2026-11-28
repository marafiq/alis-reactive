using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using Expression = Alis.Reactive.Schema.Expression;

namespace Alis.Reactive.ValuesAndSources;

internal sealed class ContractRecord<TContract>
{
    private readonly List<RecordField> _fields = [];

    internal ContractRecord<TContract> Set<TValue>(Expression<Func<TContract, TValue>> member, Expression value)
    {
        _fields.Add(new RecordField(Name(member), value));
        return this;
    }

    internal ContractRecord<TContract> Set<TValue>(Expression<Func<TContract, TValue>> member, TValue value)
        => Set(member, Literal(value));

    internal RecordExpression Written => new(_fields.ToArray());

    internal static string Name<TValue>(Expression<Func<TContract, TValue>> member)
        => MemberPath.NameOfField(member, PlanJson.Options);

    internal static LitExpression Literal<TValue>(TValue value)
        => new(JsonSerializer.SerializeToElement(value, PlanJson.Options));
}
