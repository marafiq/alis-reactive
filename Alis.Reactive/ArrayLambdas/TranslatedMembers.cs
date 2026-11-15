using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using Alis.Reactive.Conditions;
using Alis.Reactive.Schema;
using LinqExpression = System.Linq.Expressions.Expression;
using PlanExpression = Alis.Reactive.Schema.Expression;

namespace Alis.Reactive.ArrayLambdas;

[SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "The owner's choice, 2026-11-06: the .NET members a list's lambda reads through are one closed table keyed by the definition the expression tree carries, its module and metadata token, built from typed lambdas and never listed or invoked (D6.5, D6.12, D18.1).")]
internal static partial class TranslatedMembers
{
    private static readonly HashSet<MemberKey> SpanConversions = [];

    private static readonly Dictionary<MemberKey, Translation> Table = Written();

    private static readonly Dictionary<StringComparison, string> ComparisonNames = new()
    {
        [StringComparison.CurrentCulture] = "StringComparison.CurrentCulture",
        [StringComparison.CurrentCultureIgnoreCase] = "StringComparison.CurrentCultureIgnoreCase",
        [StringComparison.InvariantCulture] = "StringComparison.InvariantCulture",
        [StringComparison.InvariantCultureIgnoreCase] = "StringComparison.InvariantCultureIgnoreCase",
        [StringComparison.OrdinalIgnoreCase] = "StringComparison.OrdinalIgnoreCase",
    };

    private static readonly HashSet<MemberKey> ExactDecimalConversions =
    [
        Conversion((sbyte value) => (decimal)value),
        Conversion((byte value) => (decimal)value),
        Conversion((short value) => (decimal)value),
        Conversion((ushort value) => (decimal)value),
        Conversion((int value) => (decimal)value),
        Conversion((uint value) => (decimal)value),
        Conversion((long value) => (decimal)value),
        Conversion((ulong value) => (decimal)value),
        Conversion((char value) => (decimal)value),
    ];

    private delegate PlanExpression Translation(LinqExpression term, IReadOnlyList<LinqExpression> operands, ItemLambda read);

    internal static bool IsAnExactDecimalConversion(UnaryExpression conversion) => ExactDecimalConversions.Contains(Key(conversion.Method!));

    internal static bool TryTranslating(LinqExpression term, ItemLambda read, [NotNullWhen(true)] out PlanExpression? translated)
    {
        translated = term switch
        {
            MethodCallExpression call when Table.TryGetValue(Key(call.Method), out var translate) => translate(call, Operands(call), read),
            MemberExpression { Expression: { } holder } member when Table.TryGetValue(Key(member.Member), out var translate) => translate(member, [holder], read),
            _ => null,
        };
        return translated is not null;
    }

    [SuppressMessage("Globalization", "CA1304:Specify CultureInfo", Justification = "The table names the members a lambda translates by typed lambdas it never runs; ToUpper() and ToLower() are keyed as the view writes them.")]
    [SuppressMessage("Globalization", "CA1311:Specify a culture or use an invariant version", Justification = "The table names the members a lambda translates by typed lambdas it never runs; ToUpper() and ToLower() are keyed as the view writes them.")]
    private static Dictionary<MemberKey, Translation> Written()
    {
        var table = new Dictionary<MemberKey, Translation>
        {
            [Property((string text) => text.Length)] = (_, operands, read) => ListForms.Length(read.Term(operands[0])),
            [Method((string text) => text.Trim())] = Unary(Operator.Trim),
            [Method((string text) => text.TrimStart('a', 'b'))] = Unary(Operator.TrimStart),
            [Method((string text) => text.TrimEnd('a', 'b'))] = Unary(Operator.TrimEnd),
            [Method((string text) => text.ToUpperInvariant())] = Unary(Operator.UpperInvariant),
            [Method((string text) => text.ToLowerInvariant())] = Unary(Operator.LowerInvariant),
            [Method((string text) => text.ToUpper())] = (_, operands, read) => new OpExpression(UpperUnder(read.Form.RendersUnder), [read.Term(operands[0])]),
            [Method((string text) => text.ToLower())] = (_, operands, read) => new OpExpression(LowerUnder(read.Form.RendersUnder), [read.Term(operands[0])]),
            [Method((string text) => text.Contains(""))] = Binary(Operator.Contains),
            [Method((string text) => text.StartsWith("", StringComparison.Ordinal))] = Ordinal(Operator.StartsWith),
            [Method((string text) => text.EndsWith("", StringComparison.Ordinal))] = Ordinal(Operator.EndsWith),
            [Method((string text) => text.StartsWith(""))] = ByTheCulture(),
            [Method((string text) => text.EndsWith(""))] = ByTheCulture(),
            [Static(() => string.IsNullOrEmpty(""))] = Unary(Operator.NullOrEmpty),
            [Static(() => string.IsNullOrWhiteSpace(""))] = (_, operands, read) => new OpExpression(Operator.NullOrEmpty, [new OpExpression(Operator.Trim, [read.Term(operands[0])])]),
            [Method((IEnumerable<int> items) => items.Any())] = (_, operands, read) => ListForms.AnyOf(read.Term(operands[0]), read.EveryInnerItem()),
            [Method((IEnumerable<int> items) => items.Any(item => true))] = (_, operands, read) => ListForms.AnyOf(read.Term(operands[0]), read.Inner((LambdaExpression)operands[1])),
            [Method((IEnumerable<int> items) => items.All(item => true))] = (_, operands, read) => ListForms.AllOf(read.Term(operands[0]), read.Inner((LambdaExpression)operands[1])),
            [Method((IEnumerable<int> items) => items.Count())] = (_, operands, read) => ListForms.Length(read.Term(operands[0])),
            [Method((IEnumerable<int> items) => items.Count(item => true))] = (_, operands, read) => ListForms.Length(ListForms.Filter(read.Term(operands[0]), read.Inner((LambdaExpression)operands[1]))),
            [Property((List<int> items) => items.Count)] = (_, operands, read) => ListForms.Length(read.Term(operands[0])),
            [Property((IReadOnlyCollection<int> items) => items.Count)] = (_, operands, read) => ListForms.Length(read.Term(operands[0])),
            [Property((ICollection<int> items) => items.Count)] = (_, operands, read) => ListForms.Length(read.Term(operands[0])),
            [Property((int? value) => value.HasValue)] = (_, operands, read) => HasAValue(operands[0], read),
            [Property((int? value) => value!.Value)] = (_, operands, read) => new OpExpression(Operator.NullableValue, [read.Term(operands[0])]),
            [Method((IEnumerable<int> values) => values.Contains(0))] = Lookup(inAnyCollection: false),
            [Method((IEnumerable<int> values) => values.Contains(0, null))] = Lookup(inAnyCollection: false),
            [Method((List<int> values) => values.Contains(0))] = Lookup(inAnyCollection: true),
            [Method((HashSet<int> values) => values.Contains(0))] = Lookup(inAnyCollection: false),
        };
        AddTheHostsOwn(table, SpanConversions);
        return table;
    }

    static partial void AddTheHostsOwn(Dictionary<MemberKey, Translation> table, HashSet<MemberKey> spanConversions);

    private static Translation Unary(Operator op) => (term, operands, read) =>
    {
        var passesNoCharacters = operands.Skip(1).All(operand => operand is NewArrayExpression { NodeType: ExpressionType.NewArrayInit, Expressions.Count: 0 });
        if (!passesNoCharacters) throw ItemLambda.NoBrowserForm(term);
        return new OpExpression(op, [read.Term(operands[0])]);
    };

    private static Translation Binary(Operator op) => (_, operands, read) => new OpExpression(op, [read.Term(operands[0]), read.Term(operands[1])]);

    private static Translation Ordinal(Operator op) => (term, operands, read) =>
    {
        if (read.ReadsAnItem(operands[2])) throw ItemLambda.NoBrowserForm(term);
        var comparison = (StringComparison)ViewValues.Of(operands[2])!;
        if (comparison != StringComparison.Ordinal) throw Refused(term, $"compares text by {ComparisonNames[comparison]}; a list's lambda compares text by StringComparison.Ordinal.");
        return new OpExpression(op, [read.Term(operands[0]), read.Term(operands[1])]);
    };

    private static Translation ByTheCulture()
        => (term, _, _) => throw Refused(term, "compares by the culture in .NET, which the browser cannot match; pass StringComparison.Ordinal, such as d.Ward.StartsWith(\"N\", StringComparison.Ordinal).");

    private static OpExpression HasAValue(LinqExpression nullable, ItemLambda read)
    {
        var noValue = read.Form.Beside(null, nullable.Type);
        return new OpExpression(Operator.Neq, [Keys.Of(nullable.Type, read.Term(nullable)), Keys.Of(nullable.Type, noValue)]);
    }

    private static Translation Lookup(bool inAnyCollection) => (term, operands, read) =>
    {
        var values = Unspanned(operands[0]);
        var item = operands[1];
        var comparerIsTheDefault = operands.Count < 3 || operands[2] is ConstantExpression { Value: null };
        LinqExpression[] passed = comparerIsTheDefault ? [item] : [item, operands[2]];
        var printed = Printed.Of(values) + ".Contains(" + string.Join(", ", passed.Select(Printed.Of)) + ")";
        if (!comparerIsTheDefault) throw Refused(printed, "passes an equality comparer, which the browser does not have; look up without it, or move the work to the server.");
        var looksInText = values.Type == typeof(string);
        if (looksInText) return new OpExpression(Operator.Contains, [read.Term(values), read.Term(item)]);
        if (read.ReadsAnItem(values)) throw Refused(printed, "looks for a value in a list the item holds; use Any, such as d.Hours.Any(hour => hour == 8).");
        var held = ViewValues.Of(values) ?? throw Refused(printed, "looks in a list that is null when the view renders.");
        var comparesByItsOwnRules = !inAnyCollection && held is not Array;
        if (comparesByItsOwnRules)
            throw Refused(printed, "looks in a collection that can compare by its own rules, as a HashSet's comparer does; copy it into an array before the lambda, such as var wards = Model.Wards.ToArray();.");
        var list = read.Form.Beside(held, values.Type);
        var written = list.Value.EnumerateArray().ToList();
        var holdsAnObject = written.Any(value => value.ValueKind is JsonValueKind.Object or JsonValueKind.Array);
        if (holdsAnObject) throw Refused(printed, "looks for an object, and the browser compares objects by reference; look for a member, such as names.Contains(d.Carer.Name).");
        var holdsAFractionAsText = ComparedTypes.IsFraction(item.Type) && written.Any(value => value.ValueKind == JsonValueKind.String);
        if (holdsAFractionAsText)
        {
            throw Refused(printed, "looks for a fraction the JSON writes as text, and the browser's lookup compares its text, which does not equal as the number does, as 0.10 and 0.1 or -0 and 0; " +
                "compare a decimal with == for each value, such as d => d.Copay == 0.10m || d.Copay == 0.25m, or look it up on the server.");
        }

        var looked = read.Term(item);
        if (!Keys.IsDateOrTime(item.Type)) return new OpExpression(Operator.Includes, [list, looked]);
        var key = Keys.Of(item.Type, looked);
        if (written.Count == 0) return new OpExpression(Operator.Neq, [key, key]);
        return written
            .Select(value => (PlanExpression)new OpExpression(Operator.Eq, [key, Keys.Of(item.Type, new LitExpression(value.Clone()))]))
            .Aggregate((either, or) => new OpExpression(Operator.Or, [either, or]));
    };

    private static Operator UpperUnder(CultureInfo culture)
    {
        var keepsTheDotlessI = string.Equals("ı".ToUpper(culture), "ı", StringComparison.Ordinal);
        if (keepsTheDotlessI) return Operator.UpperInvariant;
        var dotsTheI = string.Equals("i".ToUpper(culture), "İ", StringComparison.Ordinal);
        return dotsTheI ? Operator.UpperTurkic : Operator.Upper;
    }

    private static Operator LowerUnder(CultureInfo culture)
    {
        var keepsTheDottedI = string.Equals("İ".ToLower(culture), "İ", StringComparison.Ordinal);
        if (keepsTheDottedI) return Operator.LowerInvariant;
        var undotsTheI = string.Equals("I".ToLower(culture), "ı", StringComparison.Ordinal);
        return undotsTheI ? Operator.LowerTurkic : Operator.Lower;
    }

    private static LinqExpression Unspanned(LinqExpression values)
    {
        var spanned = values is MethodCallExpression { Arguments.Count: 1 } conversion && SpanConversions.Contains(Key(conversion.Method));
        if (spanned) values = ((MethodCallExpression)values).Arguments[0];
        while (values is UnaryExpression { NodeType: ExpressionType.Convert } cast) values = cast.Operand;
        return values;
    }

    private static LinqExpression[] Operands(MethodCallExpression call) => call.Object is null ? [.. call.Arguments] : [call.Object, .. call.Arguments];

    private static InvalidOperationException Refused(LinqExpression term, string reason) => Refused(Printed.Of(term), reason);

    private static InvalidOperationException Refused(string printed, string reason) => new($"'{printed}' {reason}");

    private static MemberKey Key(MemberInfo member) => new(member.Module, member.MetadataToken);

    private static MemberKey Method<T, TResult>(Expression<Func<T, TResult>> call) => Key(((MethodCallExpression)call.Body).Method);

    private static MemberKey Static<TResult>(Expression<Func<TResult>> call) => Key(((MethodCallExpression)call.Body).Method);

    private static MemberKey Property<T, TResult>(Expression<Func<T, TResult>> read) => Key(((MemberExpression)read.Body).Member);

    private static MemberKey Conversion<T, TResult>(Expression<Func<T, TResult>> cast) => Key(((UnaryExpression)cast.Body).Method!);

    private readonly record struct MemberKey(Module Module, int Token);
}
