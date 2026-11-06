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
    internal delegate PlanExpression Reader(LinqExpression operand);

    private delegate PlanExpression Translation(LinqExpression term, IReadOnlyList<LinqExpression> operands, Reader read, string argument);

    private static readonly Dictionary<MemberInfo, Translation> Table = new()
    {
        [Property((string text) => text.Length)] = (_, operands, read, _) => new GetExpression(read(operands[0]), [PathStep.Member("length")], ReadPresence.RequiredIfHeld),
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
    };

    internal static PlanExpression? Of(LinqExpression term, Reader read, string argument) => term switch
    {
        MethodCallExpression call when Table.TryGetValue(call.Method, out var translate) => translate(call, Operands(call), read, argument),
        MemberExpression { Expression: { } holder } member when Table.TryGetValue(member.Member, out var translate) => translate(member, [holder], read, argument),
        _ => null,
    };

    internal static ArgumentException NoBrowserForm(LinqExpression term, string argument)
        => new($"'{ViewValues.Printed(term)}' has no browser form in a list's lambda; use a member the framework translates, or move the work to the server.", argument);

    private static Translation Unary(Operator op) => (term, operands, read, argument) =>
    {
        var paramsArrayIsEmpty = operands.Skip(1).All(operand => operand is NewArrayExpression { Expressions.Count: 0 });
        if (!paramsArrayIsEmpty) throw NoBrowserForm(term, argument);
        return Op(op, read(operands[0]));
    };

    private static Translation Binary(Operator op) => (_, operands, read, _) => Op(op, read(operands[0]), read(operands[1]));

    private static Translation Ordinal(Operator op) => (term, operands, read, argument) =>
    {
        var comparison = ViewValues.Of(operands[2], argument);
        if (comparison is not StringComparison.Ordinal)
            throw new ArgumentException($"'{ViewValues.Printed(term)}' compares text by {comparison}; a list's lambda compares text by StringComparison.Ordinal.", argument);
        return Op(op, read(operands[0]), read(operands[1]));
    };

    private static PlanExpression ByCulture(LinqExpression term, IReadOnlyList<LinqExpression> operands, Reader read, string argument)
        => throw new ArgumentException($"'{ViewValues.Printed(term)}' compares by the culture in .NET, which the browser cannot match; pass StringComparison.Ordinal, such as d.Ward.StartsWith(\"N\", StringComparison.Ordinal).", argument);

    private static LinqExpression[] Operands(MethodCallExpression call)
        => call.Object is null ? [.. call.Arguments] : [call.Object, .. call.Arguments];

    private static OperatorExpression Op(Operator op, params PlanExpression[] args) => new(op, args);

    private static MethodInfo Method<T, TResult>(Expression<Func<T, TResult>> call) => ((MethodCallExpression)call.Body).Method;

    private static MethodInfo Static<TResult>(Expression<Func<TResult>> call) => ((MethodCallExpression)call.Body).Method;

    private static MemberInfo Property<T, TResult>(Expression<Func<T, TResult>> read) => ((MemberExpression)read.Body).Member;
}
