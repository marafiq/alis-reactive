using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Alis.Reactive.Plan;
using LinqExpression = System.Linq.Expressions.Expression;
using PlanExpression = Alis.Reactive.Plan.Expression;

namespace Alis.Reactive.Conditions;

[SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "The owner's choice, 2026-11-06: the .NET members a list's lambda translates are one closed table keyed by the member the expression tree carries, built from typed lambdas (D6.5, D18.1).")]
internal static class TranslatedMembers
{
    internal interface ITranslator
    {
        PlanExpression Read(LinqExpression operand);

        FnExpression Lambda(LambdaExpression inner);

        FnExpression EveryItem();
    }

    private delegate PlanExpression Translation(LinqExpression term, IReadOnlyList<LinqExpression> operands, ITranslator read, string argument);

    private static readonly Dictionary<(Module Module, int Token), Translation> Table = new()
    {
        [Property((string text) => text.Length)] = (_, operands, read, _) => ListForms.Length(read.Read(operands[0])),
        [Method((string text) => text.Trim())] = Unary(Operator.Trim),
        [Method((string text) => text.TrimStart())] = Unary(Operator.TrimStart),
        [Method((string text) => text.TrimEnd())] = Unary(Operator.TrimEnd),
        [Method((string text) => text.TrimStart('a', 'b'))] = Unary(Operator.TrimStart),
        [Method((string text) => text.TrimEnd('a', 'b'))] = Unary(Operator.TrimEnd),
        [Method((string text) => text.ToUpperInvariant())] = Unary(Operator.UpperInvariant),
        [Method((string text) => text.ToLowerInvariant())] = Unary(Operator.LowerInvariant),
        [Method((string text) => text.Contains(""))] = Binary(Operator.Contains),
        [Method((string text) => text.StartsWith("", StringComparison.Ordinal))] = Ordinal(Operator.StartsWith),
        [Method((string text) => text.EndsWith("", StringComparison.Ordinal))] = Ordinal(Operator.EndsWith),
        [Method((string text) => text.StartsWith(""))] = ByCulture,
        [Method((string text) => text.EndsWith(""))] = ByCulture,
        [Static(() => string.IsNullOrEmpty(""))] = Unary(Operator.NullOrEmpty),
        [Static(() => string.IsNullOrWhiteSpace(""))] = (term, operands, read, argument) => Op(Operator.NullOrEmpty, Unary(Operator.Trim)(term, operands, read, argument)),
        [Method((IEnumerable<int> items) => items.Any())] = (_, operands, read, _) => ListForms.Some(read.Read(operands[0]), read.EveryItem()),
        [Method((IEnumerable<int> items) => items.Any(item => true))] = (_, operands, read, _) => ListForms.Some(read.Read(operands[0]), read.Lambda((LambdaExpression)operands[1])),
        [Method((IEnumerable<int> items) => items.All(item => true))] = (_, operands, read, _) => ListForms.Every(read.Read(operands[0]), read.Lambda((LambdaExpression)operands[1])),
        [Method((IEnumerable<int> items) => items.Count())] = (_, operands, read, _) => ListForms.Length(read.Read(operands[0])),
        [Method((IEnumerable<int> items) => items.Count(item => true))] = (_, operands, read, _) => ListForms.Length(ListForms.Filter(read.Read(operands[0]), read.Lambda((LambdaExpression)operands[1]))),
    };

    internal static PlanExpression? Of(LinqExpression term, ITranslator read, string argument) => term switch
    {
        MethodCallExpression call when Table.TryGetValue(Key(call.Method), out var translate) => translate(call, Operands(call), read, argument),
        MemberExpression { Expression: { } holder } member when Table.TryGetValue(Key(member.Member), out var translate) => translate(member, [holder], read, argument),
        _ => null,
    };

    internal static ArgumentException NoBrowserForm(LinqExpression term, string argument)
        => new($"'{ViewValues.Printed(term)}' has no browser form in a list's lambda; use a member the framework translates, or move the work to the server.", argument);

    private static Translation Unary(Operator op) => (term, operands, read, argument) =>
    {
        var paramsArrayIsEmpty = operands.Skip(1).All(operand => operand is NewArrayExpression { Expressions.Count: 0 });
        if (!paramsArrayIsEmpty) throw NoBrowserForm(term, argument);
        return Op(op, read.Read(operands[0]));
    };

    private static Translation Binary(Operator op) => (_, operands, read, _) => Op(op, read.Read(operands[0]), read.Read(operands[1]));

    private static Translation Ordinal(Operator op) => (term, operands, read, argument) =>
    {
        var comparison = ViewValues.Of(operands[2], argument);
        if (comparison is not StringComparison.Ordinal)
            throw new ArgumentException($"'{ViewValues.Printed(term)}' compares text by {comparison}; a list's lambda compares text by StringComparison.Ordinal.", argument);
        return Op(op, read.Read(operands[0]), read.Read(operands[1]));
    };

    private static PlanExpression ByCulture(LinqExpression term, IReadOnlyList<LinqExpression> operands, ITranslator read, string argument)
        => throw new ArgumentException($"'{ViewValues.Printed(term)}' compares by the culture in .NET, which the browser cannot match; pass StringComparison.Ordinal, such as d.Ward.StartsWith(\"N\", StringComparison.Ordinal).", argument);

    private static LinqExpression[] Operands(MethodCallExpression call)
        => call.Object is null ? [.. call.Arguments] : [call.Object, .. call.Arguments];

    private static OperatorExpression Op(Operator op, params PlanExpression[] args) => new(op, args);

    private static (Module Module, int Token) Key(MemberInfo member) => (member.Module, member.MetadataToken);

    private static (Module Module, int Token) Method<T, TResult>(Expression<Func<T, TResult>> call) => Key(((MethodCallExpression)call.Body).Method);

    private static (Module Module, int Token) Static<TResult>(Expression<Func<TResult>> call) => Key(((MethodCallExpression)call.Body).Method);

    private static (Module Module, int Token) Property<T, TResult>(Expression<Func<T, TResult>> read) => Key(((MemberExpression)read.Body).Member);
}
