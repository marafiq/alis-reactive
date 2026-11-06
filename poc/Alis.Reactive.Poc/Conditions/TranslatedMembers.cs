using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
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

        bool ReadsTheItem(LinqExpression node);

        object? ViewValue(LinqExpression node);

        LiteralExpression Literal(object? value, Type type);
    }

    private delegate PlanExpression Translation(LinqExpression term, IReadOnlyList<LinqExpression> operands, ITranslator read, string argument);

    [SuppressMessage("Globalization", "CA1304:Specify CultureInfo", Justification = "The table names the members a lambda translates by typed lambdas it never runs; ToUpper() and ToLower() are keyed as the view writes them.")]
    [SuppressMessage("Globalization", "CA1311:Specify a culture or use an invariant version", Justification = "The table names the members a lambda translates by typed lambdas it never runs; ToUpper() and ToLower() are keyed as the view writes them.")]
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
        [Method((string text) => text.ToUpper())] = (_, operands, read, _) => Op(UpperUnder(CultureInfo.CurrentCulture), read.Read(operands[0])),
        [Method((string text) => text.ToLower())] = (_, operands, read, _) => Op(LowerUnder(CultureInfo.CurrentCulture), read.Read(operands[0])),
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
        [Property((List<int> items) => items.Count)] = (_, operands, read, _) => ListForms.Length(read.Read(operands[0])),
        [Property((IReadOnlyCollection<int> items) => items.Count)] = (_, operands, read, _) => ListForms.Length(read.Read(operands[0])),
        [Property((ICollection<int> items) => items.Count)] = (_, operands, read, _) => ListForms.Length(read.Read(operands[0])),
        [Method((IEnumerable<int> values) => values.Contains(0))] = FixedValues(arrayOnly: true),
        [Method((List<int> values) => values.Contains(0))] = FixedValues(arrayOnly: false),
#if NET
        [Method((string text) => text.Contains('a'))] = Binary(Operator.Contains),
        [Method((string[] values) => values.Contains(""))] = FixedValues(arrayOnly: true),
        [Method((DayOfWeek[] values) => values.Contains(DayOfWeek.Monday))] = FixedValues(arrayOnly: true),
#endif
    };

#if NET
    private static readonly (Module Module, int Token) SpanConversion = Key(((MethodCallExpression)((MethodCallExpression)((Expression<Func<string[], bool>>)(values => values.Contains(""))).Body).Arguments[0]).Method);
#endif

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

    private static Translation FixedValues(bool arrayOnly) => (term, operands, read, argument) =>
    {
        var values = Unspanned(operands[0]);
        var item = operands[1];
        var printed = $"{ViewValues.Printed(values)}.Contains({ViewValues.Printed(item)})";
        var comparerIsDefault = operands.Count < 3 || operands[2] is ConstantExpression { Value: null };
        if (!comparerIsDefault)
            throw new ArgumentException($"'{printed}' passes an equality comparer, which the browser does not have; look up without it, or move the work to the server.", argument);
        if (values.Type == typeof(string)) return Op(Operator.Contains, read.Read(values), read.Read(item));
        if (read.ReadsTheItem(values))
            throw new ArgumentException($"'{printed}' looks for a value in a list the item holds; use Any, such as d.Hours.Any(hour => hour == 8).", argument);
        var held = read.ViewValue(values);
        if (held is null) throw new ArgumentException($"'{printed}' looks in a list that is null when the view renders.", argument);
        if (arrayOnly && held is not Array)
            throw new ArgumentException($"'{printed}' looks in a collection that can compare by its own rules, as a HashSet's comparer does; copy it into an array before the lambda, such as var wards = Model.Wards.ToArray();.", argument);
        var list = read.Literal(held, values.Type);
        var holdsAnObject = list.Value.EnumerateArray().Any(value => value.ValueKind is JsonValueKind.Object or JsonValueKind.Array);
        if (holdsAnObject)
            throw new ArgumentException($"'{printed}' looks for an object, and the browser compares objects by reference; look for a member, such as names.Contains(d.Carer.Name).", argument);
        return Op(Operator.Includes, list, read.Read(item));
    };

    private static Operator UpperUnder(CultureInfo culture)
    {
        var keepsTheDotlessI = string.Equals("\u0131".ToUpper(culture), "\u0131", StringComparison.Ordinal);
        if (keepsTheDotlessI) return Operator.UpperInvariant;
        var dotsTheI = string.Equals("i".ToUpper(culture), "\u0130", StringComparison.Ordinal);
        return dotsTheI ? Operator.UpperTurkic : Operator.Upper;
    }

    private static Operator LowerUnder(CultureInfo culture)
    {
        var keepsTheDottedI = string.Equals("\u0130".ToLower(culture), "\u0130", StringComparison.Ordinal);
        if (keepsTheDottedI) return Operator.LowerInvariant;
        var undotsTheI = string.Equals("I".ToLower(culture), "\u0131", StringComparison.Ordinal);
        return undotsTheI ? Operator.LowerTurkic : Operator.Lower;
    }

    private static LinqExpression Unspanned(LinqExpression values)
    {
#if NET
        if (values is MethodCallExpression { Arguments.Count: 1 } span && Key(span.Method) == SpanConversion) values = span.Arguments[0];
#endif
        while (values is UnaryExpression { NodeType: ExpressionType.Convert } cast) values = cast.Operand;
        return values;
    }

    private static LinqExpression[] Operands(MethodCallExpression call)
        => call.Object is null ? [.. call.Arguments] : [call.Object, .. call.Arguments];

    private static OperatorExpression Op(Operator op, params PlanExpression[] args) => new(op, args);

    private static (Module Module, int Token) Key(MemberInfo member) => (member.Module, member.MetadataToken);

    private static (Module Module, int Token) Method<T, TResult>(Expression<Func<T, TResult>> call) => Key(((MethodCallExpression)call.Body).Method);

    private static (Module Module, int Token) Static<TResult>(Expression<Func<TResult>> call) => Key(((MethodCallExpression)call.Body).Method);

    private static (Module Module, int Token) Property<T, TResult>(Expression<Func<T, TResult>> read) => Key(((MemberExpression)read.Body).Member);
}
