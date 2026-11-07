using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using Alis.Reactive.Plan;
using Alis.Reactive.Types;
using LinqExpression = System.Linq.Expressions.Expression;
using PlanExpression = Alis.Reactive.Plan.Expression;

namespace Alis.Reactive.Conditions;

internal sealed class LambdaCondition : TranslatedMembers.ITranslator
{
    private static readonly Dictionary<ExpressionType, (Operator Op, bool Orders)> Comparisons = new()
    {
        [ExpressionType.Equal] = (Operator.Eq, false),
        [ExpressionType.NotEqual] = (Operator.Neq, false),
        [ExpressionType.LessThan] = (Operator.Lt, true),
        [ExpressionType.LessThanOrEqual] = (Operator.Lte, true),
        [ExpressionType.GreaterThan] = (Operator.Gt, true),
        [ExpressionType.GreaterThanOrEqual] = (Operator.Gte, true),
    };

    private static readonly Dictionary<Type, (int Bytes, bool Signed)> WholeNumbers = new()
    {
        [typeof(sbyte)] = (1, true),
        [typeof(sbyte?)] = (1, true),
        [typeof(byte)] = (1, false),
        [typeof(byte?)] = (1, false),
        [typeof(short)] = (2, true),
        [typeof(short?)] = (2, true),
        [typeof(ushort)] = (2, false),
        [typeof(ushort?)] = (2, false),
        [typeof(int)] = (4, true),
        [typeof(int?)] = (4, true),
        [typeof(uint)] = (4, false),
        [typeof(uint?)] = (4, false),
        [typeof(long)] = (8, true),
        [typeof(long?)] = (8, true),
        [typeof(ulong)] = (8, false),
        [typeof(ulong?)] = (8, false),
    };

    private static readonly HashSet<Type> Fractions =
    [
        typeof(float), typeof(float?), typeof(double), typeof(double?), typeof(decimal), typeof(decimal?),
    ];

    private static readonly HashSet<Type> Chars = [typeof(char), typeof(char?)];

    private static readonly HashSet<Type> OtherScalars = [typeof(string), typeof(bool), typeof(bool?), typeof(Guid), typeof(Guid?)];

    private static readonly HashSet<Type> DatesAndTimes =
    [
        typeof(DateTime), typeof(DateTime?), typeof(DateTimeOffset), typeof(DateTimeOffset?), typeof(TimeSpan), typeof(TimeSpan?),
#if NET
        typeof(DateOnly), typeof(DateOnly?), typeof(TimeOnly), typeof(TimeOnly?),
#endif
    ];

    private readonly LambdaExpression _lambda;
    private readonly IReadOnlyDictionary<ParameterExpression, PlanExpression> _scope;
    private readonly DataStarts _starts;
    private readonly JsonSerializerOptions _values;
    private readonly string _argument;
    private readonly Func<string> _itemName;

    private LambdaCondition(LambdaExpression lambda, IReadOnlyDictionary<ParameterExpression, PlanExpression> scope, DataStarts starts, string argument, Func<string> itemName)
    {
        _lambda = lambda;
        _scope = scope;
        _starts = starts;
        _values = starts.Values;
        _argument = argument;
        _itemName = itemName;
    }

    internal static PlanExpression Of(LambdaExpression lambda, PlanExpression item, DataStarts starts, string argument, Func<string> itemName)
    {
        var scope = new Dictionary<ParameterExpression, PlanExpression> { [lambda.Parameters[0]] = item };
        return new LambdaCondition(lambda, scope, starts, argument, itemName).Term(lambda.Body);
    }

    public PlanExpression Read(LinqExpression operand)
    {
        var read = Unconverted(operand);
        if (!ReadsTheItem(read)) return new LiteralExpression(Written(ViewValues.Of(read, _argument), read.Type));
        if (read is ParameterExpression parameter && _scope.TryGetValue(parameter, out var bound)) return bound;
        if (read is UnaryExpression { NodeType: ExpressionType.ArrayLength } length) return ListForms.Length(Read(length.Operand));
        if (read is BinaryExpression { NodeType: ExpressionType.Coalesce } coalesce) return Coalesced(coalesce);
        if (read is ConditionalExpression conditional) return Op(Operator.Cond, Term(conditional.Test), Term(conditional.IfTrue), Term(conditional.IfFalse));
        var translated = TranslatedMembers.Of(read, this, _argument);
        if (translated is not null) return translated;
        var root = RootOf(read);
        if (root is null || !_scope.TryGetValue(root, out var holder)) throw TranslatedMembers.NoBrowserForm(read, _argument);
        var steps = JsonPath.Steps(read, root, _starts, _lambda) ?? throw TranslatedMembers.NoBrowserForm(read, _argument);
        return new DataExpression(holder, steps);
    }

    public FnExpression Lambda(LambdaExpression inner)
    {
        var item = _itemName();
        var scope = new Dictionary<ParameterExpression, PlanExpression>(_scope.Count + 1);
        foreach (var bound in _scope) scope[bound.Key] = bound.Value;
        scope[inner.Parameters[0]] = new RefExpression(item);
        return new FnExpression([item], new LambdaCondition(inner, scope, _starts, _argument, _itemName).Term(inner.Body));
    }

    public FnExpression EveryItem() => ListForms.EveryItem(_itemName());

    public object? ViewValue(LinqExpression node) => ViewValues.Of(Unconverted(node), _argument);

    public LiteralExpression Literal(object? value, Type type) => new(Written(value, type));

    private PlanExpression Term(LinqExpression node) => node switch
    {
        BinaryExpression { NodeType: ExpressionType.AndAlso } both => Op(Operator.And, Term(both.Left), Term(both.Right)),
        BinaryExpression { NodeType: ExpressionType.OrElse } either => Op(Operator.Or, Term(either.Left), Term(either.Right)),
        BinaryExpression comparison when Comparisons.TryGetValue(comparison.NodeType, out var compared) => Compared(comparison, compared.Op, compared.Orders),
        UnaryExpression { NodeType: ExpressionType.Not } negation when negation.Type == typeof(bool) => Op(Operator.Not, Term(negation.Operand)),
        _ => Read(node),
    };

    private OperatorExpression Coalesced(BinaryExpression coalesce)
    {
        if (coalesce.Conversion is not null) throw Refused(coalesce, "converts its left side by a user-defined conversion, which the browser does not have; coalesce values of one type, or move the work to the server.");
        return Op(Operator.Coalesce, Term(coalesce.Left), Term(coalesce.Right));
    }

    private OperatorExpression Compared(BinaryExpression comparison, Operator op, bool orders)
    {
        var left = Unconverted(comparison.Left);
        var right = Unconverted(comparison.Right);
        var leftReadsTheItem = ReadsTheItem(left);
        var rightReadsTheItem = ReadsTheItem(right);
        if (!leftReadsTheItem && !rightReadsTheItem) throw ViewValues.Computed(comparison, _argument);
        if (DatesAndTimes.Contains(left.Type) || DatesAndTimes.Contains(right.Type)) throw Refused(comparison, "compares dates or times, whose text neither orders nor equals as the instant does; compare them on the server.");
        if (leftReadsTheItem && rightReadsTheItem)
        {
            RequireComparableMembers(comparison, left, right, orders);
            return Op(op, Term(left), Term(right));
        }

        var member = leftReadsTheItem ? left : right;
        var memberSide = leftReadsTheItem ? comparison.Left : comparison.Right;
        var held = leftReadsTheItem ? right : left;
        var heldSide = leftReadsTheItem ? comparison.Right : comparison.Left;
        var compilerFoldedTheConstant = IsConverted(memberSide) && !IsConverted(heldSide);
        var written = Written(ViewValues.Of(held, _argument), compilerFoldedTheConstant ? member.Type : held.Type);
        RequireComparableValue(comparison, written, member.Type, orders);
        var literal = new LiteralExpression(written);
        return leftReadsTheItem ? Op(op, Term(member), literal) : Op(op, literal, Term(member));
    }

    private void RequireComparableMembers(BinaryExpression comparison, LinqExpression left, LinqExpression right, bool orders)
    {
        if (orders)
        {
            var orderable = (IsNumber(left.Type) && IsNumber(right.Type)) || (Chars.Contains(left.Type) && Chars.Contains(right.Type));
            if (!orderable) throw Refused(comparison, "orders values the browser cannot order as .NET does; order numbers or a char, or order on the server.");
            return;
        }

        var bothScalars = IsScalar(left.Type) && IsScalar(right.Type);
        var enumsCompared = IsNumber(comparison.Left.Type) && IsConverted(comparison.Left) && IsConverted(comparison.Right) && left.Type == right.Type;
        if (!bothScalars && !enumsCompared) throw ComparesObjects(comparison);
    }

    private void RequireComparableValue(BinaryExpression comparison, JsonElement written, Type member, bool orders)
    {
        if (written.ValueKind is JsonValueKind.Object or JsonValueKind.Array) throw ComparesObjects(comparison);
        var orderedAsText = orders && written.ValueKind == JsonValueKind.String && !Chars.Contains(member);
        if (orderedAsText) throw Refused(comparison, "orders a value the app's JSON writes as text, which does not order as the value does in .NET; compare it with == or !=, or order on the server.");
    }

    private JsonElement Written(object? value, Type type)
    {
        var held = value is int code && Chars.Contains(type) ? Convert.ToChar(code) : value;
        return JsonSerializer.SerializeToElement(held, _values.GetTypeInfo(type));
    }

    private LinqExpression Unconverted(LinqExpression node)
    {
        while (node is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } conversion)
        {
            if (Narrows(conversion.Operand.Type, conversion.Type))
                throw Refused(conversion, "casts a number to a narrower one, which changes the value as C# computes it and not as the browser does; compare the member itself.");
            node = conversion.Operand;
        }

        return node;
    }

    public bool ReadsTheItem(LinqExpression node)
    {
        var finder = new ItemFinder(_scope);
        finder.Visit(node);
        return finder.Found;
    }

    private ArgumentException ComparesObjects(BinaryExpression comparison)
        => Refused(comparison, "compares objects, and the browser compares objects by reference; compare a member of each, such as d => d.Carer.Name == lead.");

    private ArgumentException Refused(LinqExpression term, string reason) => new($"'{ViewValues.Printed(term)}' {reason}", _argument);

    private static bool Narrows(Type from, Type to)
    {
        if (!WholeNumbers.TryGetValue(to, out var whole)) return false;
        if (Fractions.Contains(from)) return true;
        if (!WholeNumbers.TryGetValue(from, out var cast)) return false;
        var holdsEvery = cast.Signed == whole.Signed ? whole.Bytes >= cast.Bytes : !cast.Signed && whole.Bytes > cast.Bytes;
        return !holdsEvery;
    }

    private static bool IsConverted(LinqExpression side) => side is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked };

    private static bool IsNumber(Type type) => WholeNumbers.ContainsKey(type) || Fractions.Contains(type);

    private static bool IsScalar(Type type) => IsNumber(type) || Chars.Contains(type) || OtherScalars.Contains(type);

    private static OperatorExpression Op(Operator op, params PlanExpression[] args) => new(op, args);

    private static ParameterExpression? RootOf(LinqExpression node)
    {
        while (node is MemberExpression { Expression: { } holder }) node = holder;
        return node as ParameterExpression;
    }

    private sealed class ItemFinder(IReadOnlyDictionary<ParameterExpression, PlanExpression> scope) : ExpressionVisitor
    {
        internal bool Found { get; private set; }

        protected override LinqExpression VisitParameter(ParameterExpression node)
        {
            Found |= scope.ContainsKey(node);
            return node;
        }
    }
}
