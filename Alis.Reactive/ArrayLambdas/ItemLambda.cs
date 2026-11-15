using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.Json;
using Alis.Reactive.Conditions;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using Alis.Reactive.ValuesAndSources;
using LinqExpression = System.Linq.Expressions.Expression;
using PlanExpression = Alis.Reactive.Schema.Expression;

namespace Alis.Reactive.ArrayLambdas;

internal sealed class ItemLambda
{
    private const string Item = "item";
    private const string Left = "left";
    private const string Right = "right";

    private static readonly Dictionary<ExpressionType, (Operator Op, bool Orders)> Comparisons = new()
    {
        [ExpressionType.Equal] = (Operator.Eq, false),
        [ExpressionType.NotEqual] = (Operator.Neq, false),
        [ExpressionType.LessThan] = (Operator.Lt, true),
        [ExpressionType.LessThanOrEqual] = (Operator.Lte, true),
        [ExpressionType.GreaterThan] = (Operator.Gt, true),
        [ExpressionType.GreaterThanOrEqual] = (Operator.Gte, true),
    };

    private readonly LambdaExpression _lambda;
    private readonly Dictionary<ParameterExpression, string> _items;
    private readonly HeldForm _form;

    private ItemLambda(LambdaExpression lambda, Dictionary<ParameterExpression, string> items, HeldForm form)
    {
        _lambda = lambda;
        _items = items;
        _form = form;
    }

    internal static FnExpression Of(LambdaExpression lambda, HeldForm form) => Outermost(lambda, form).Fn(lambda, body => body);

    internal static FnExpression Selecting(LambdaExpression field, HeldForm form, bool fieldHoldsNull)
        => Outermost(field, form).Fn(field, body => fieldHoldsNull ? body : new OpExpression(Operator.NonNull, [body]));

    internal static FnExpression EveryItem() => new([Item + "1"], new LitExpression(JsonSerializer.SerializeToElement(true)));

    internal static FnExpression Comparing(LambdaExpression key, HeldForm form, Operator comparison, bool descending)
        => Outermost(key, form).Sort(key, comparison, descending);

    internal HeldForm Form => _form;

    internal FnExpression Inner(LambdaExpression inner) => Fn(inner, body => body);

    internal FnExpression EveryInnerItem() => new([NextItem], new LitExpression(JsonSerializer.SerializeToElement(true)));

    internal static InvalidOperationException NoBrowserForm(LinqExpression term)
        => Refused(term, "has no browser form in a list's lambda; use a member the framework translates, or move the work to the server.");

    private static ItemLambda Outermost(LambdaExpression lambda, HeldForm form) => new(lambda, [], form);

    private string NextItem => Item + (_items.Count + 1).ToString(CultureInfo.InvariantCulture);

    private FnExpression Fn(LambdaExpression lambda, Func<PlanExpression, PlanExpression> shaped)
    {
        var name = NextItem;
        var items = new Dictionary<ParameterExpression, string>(_items) { [lambda.Parameters[0]] = name };
        return new FnExpression([name], shaped(new ItemLambda(_lambda, items, _form).Term(lambda.Body)));
    }

    private FnExpression Sort(LambdaExpression key, Operator comparison, bool descending)
    {
        var depth = (_items.Count + 1).ToString(CultureInfo.InvariantCulture);
        var left = Left + depth;
        var right = Right + depth;
        var leftKey = KeyOf(key, left);
        var rightKey = KeyOf(key, right);
        return new FnExpression([left, right], new OpExpression(comparison, descending ? [rightKey, leftKey] : [leftKey, rightKey]));
    }

    private PlanExpression KeyOf(LambdaExpression key, string item)
    {
        var items = new Dictionary<ParameterExpression, string>(_items) { [key.Parameters[0]] = item };
        return Keys.Of(key.Body.Type, new ItemLambda(_lambda, items, _form).Term(key.Body));
    }

    internal PlanExpression Term(LinqExpression node) => node switch
    {
        BinaryExpression { NodeType: ExpressionType.AndAlso } both => new OpExpression(Operator.And, [Term(both.Left), Term(both.Right)]),
        BinaryExpression { NodeType: ExpressionType.OrElse } either => new OpExpression(Operator.Or, [Term(either.Left), Term(either.Right)]),
        BinaryExpression comparison when Comparisons.TryGetValue(comparison.NodeType, out var compared) => Compared(comparison, compared.Op, compared.Orders),
        UnaryExpression { NodeType: ExpressionType.Not } negation when negation.Type == typeof(bool) => new OpExpression(Operator.Not, [Term(negation.Operand)]),
        _ => Read(node),
    };

    private PlanExpression Read(LinqExpression node)
    {
        var read = Unconverted(node);
        if (!ReadsAnItem(read)) return _form.Beside(ViewValues.Of(read), read.Type);
        if (TranslatedMembers.TryTranslating(read, this, out var translated)) return translated;
        return read switch
        {
            ParameterExpression item when _items.TryGetValue(item, out var name) => new RefExpression(name),
            UnaryExpression { NodeType: ExpressionType.ArrayLength } length => ListForms.Length(Read(length.Operand)),
            UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } valueOf => new OpExpression(Operator.NullableValue, [Read(valueOf.Operand)]),
            BinaryExpression { NodeType: ExpressionType.Coalesce } coalesce => Coalesced(coalesce),
            ConditionalExpression choice => new OpExpression(Operator.Cond, [Term(choice.Test), Term(choice.IfTrue), Term(choice.IfFalse)]),
            MemberExpression member => DataRead(member),
            _ => throw NoBrowserForm(read),
        };
    }

    private PlanExpression DataRead(MemberExpression member)
    {
        LinqExpression node = member;
        while (node is MemberExpression { Expression: { } holder })
        {
            if (_form.WritesAsAScalar(holder.Type)) throw NoBrowserForm(member);
            node = holder;
        }

        var readsAnItem = node is ParameterExpression root && _items.ContainsKey(root);
        if (!readsAnItem || !MemberPath.TryOf(member, (ParameterExpression)node, _lambda, _form.DataOptions, out var path)) throw NoBrowserForm(member);
        path.RefuseAReadPastItsOwnConverter(Printed.Of(member));
        if (path.EndsInItsOwnForm) throw new NotImplementedException();
        return new DataExpression(path.DataSteps(), new RefExpression(_items[(ParameterExpression)node]));
    }

    private OpExpression Coalesced(BinaryExpression coalesce)
    {
        if (coalesce.Conversion is not null)
            throw Refused(coalesce, "converts its left side by a user-defined conversion, which the browser does not have; coalesce values of one type, or move the work to the server.");
        return new OpExpression(Operator.Coalesce, [Term(coalesce.Left), Term(coalesce.Right)]);
    }

    private OpExpression Compared(BinaryExpression comparison, Operator op, bool orders)
    {
        var left = Unconverted(comparison.Left);
        var right = Unconverted(comparison.Right);
        var leftReadsAnItem = ReadsAnItem(left);
        var rightReadsAnItem = ReadsAnItem(right);
        if (!leftReadsAnItem && !rightReadsAnItem) throw ViewValues.Computed(comparison);
        if (leftReadsAnItem && rightReadsAnItem)
        {
            RequireComparableMembers(comparison, left.Type, right.Type, orders);
            return new OpExpression(op, [Keys.Of(left.Type, Read(left)), Keys.Of(right.Type, Read(right))]);
        }

        var member = leftReadsAnItem ? left : right;
        var held = leftReadsAnItem ? right : left;
        var compilerFoldedTheValueToANumber = !ComparedTypes.IsNumber(member.Type) && ComparedTypes.IsNumber(held.Type);
        var writtenAs = compilerFoldedTheValueToANumber ? member.Type : held.Type;
        var written = TranslatedMembers.ReadsADayOfTheWeek(member) ? DayNumber(ViewValues.Of(held)) : _form.Beside(Held(ViewValues.Of(held), held.Type, writtenAs), writtenAs);
        RequireComparableValue(comparison, written.Value, member.Type, orders);
        var memberRead = Keys.Of(member.Type, Read(member));
        var value = Keys.Of(member.Type, written);
        return new OpExpression(op, leftReadsAnItem ? [memberRead, value] : [value, memberRead]);
    }

    private void RequireComparableMembers(BinaryExpression comparison, Type left, Type right, bool orders)
    {
        if (orders)
        {
            var orderable = (ComparedTypes.IsNumber(left) && ComparedTypes.IsNumber(right)) || (ComparedTypes.IsChar(left) && ComparedTypes.IsChar(right)) || Keys.KeyedAlike(left, right);
            if (!orderable) throw Refused(comparison, "orders values the browser cannot order as .NET does; order numbers, a char, or a date or a time, or order on the server.");
            return;
        }

        if (!_form.WritesAsAScalar(left) || !_form.WritesAsAScalar(right)) throw ComparesObjects(comparison);
    }

    private static void RequireComparableValue(BinaryExpression comparison, JsonElement written, Type member, bool orders)
    {
        if (written.ValueKind is JsonValueKind.Object or JsonValueKind.Array) throw ComparesObjects(comparison);
        var orderedAsText = orders && written.ValueKind == JsonValueKind.String && !ComparedTypes.IsChar(member) && !Keys.IsDateOrTime(member);
        if (!orderedAsText) return;
        var cure = ComparedTypes.IsBinaryFraction(member)
            ? "order it on the server."
            : "compare it with == or !=, or order on the server.";
        throw Refused(comparison, "orders a value the app's JSON writes as text, which does not order as the value does in .NET; " + cure);
    }

    private object? Held(object? value, Type heldAs, Type writtenAs)
    {
        if (value is null || heldAs == writtenAs) return value;
        if (value is int code && ComparedTypes.IsChar(writtenAs)) return Convert.ToChar(code, CultureInfo.InvariantCulture);
        return JsonSerializer.SerializeToElement(value, heldAs, _form.DataOptions).Deserialize(writtenAs, _form.DataOptions);
    }

    private static LitExpression DayNumber(object? day) => new(JsonSerializer.SerializeToElement(day is null ? (int?)null : Convert.ToInt32(day, CultureInfo.InvariantCulture)));

    private LinqExpression Unconverted(LinqExpression node)
    {
        while (node is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } conversion)
        {
            if (ComparedTypes.Narrows(conversion.Operand.Type, conversion.Type))
                throw Refused(conversion, "casts a number to a narrower one, which changes the value as C# computes it and not as the browser does; compare the member itself.");
            if (conversion.Method is not null && !TranslatedMembers.IsAnExactDecimalConversion(conversion)) throw ConvertedUnlikeTheBrowser(conversion);
            var takesTheValue = conversion is { IsLifted: true, IsLiftedToNull: false, Method: null };
            if (takesTheValue && ReadsAnItem(conversion.Operand)) return conversion;
            if (takesTheValue && ViewValues.Of(conversion.Operand) is null)
            {
                throw new InvalidOperationException(
                    $"'{Printed.Of(conversion.Operand)}' is null when the view renders, and the cast takes its value, which .NET refuses; compare the nullable itself, such as d => d.Priority == limit.");
            }

            node = conversion.Operand;
        }

        return node;
    }

    internal bool ReadsAnItem(LinqExpression node)
    {
        var finder = new ItemFinder(_items);
        finder.Visit(node);
        return finder.Found;
    }

    private static InvalidOperationException ConvertedUnlikeTheBrowser(UnaryExpression conversion)
    {
        var operand = Printed.Of(conversion.Operand);
        var castsADecimal = ComparedTypes.IsDecimal(conversion.Type) || ComparedTypes.IsDecimal(conversion.Operand.Type);
        if (castsADecimal) return new($"'{operand}' is cast to or from decimal, which rounds as C# computes it and not as the browser does; keep the member's own type, or move the work to the server.");
        return new($"'{operand}' is converted by a user-defined conversion, which the browser does not have; compare a member it reads, such as d => d.Seal.Tag == \"S4\", or move the work to the server.");
    }

    private static InvalidOperationException ComparesObjects(BinaryExpression comparison)
        => Refused(comparison, "compares objects, and the browser compares objects by reference; compare a member of each, such as d => d.Carer.Name == lead.");

    private static InvalidOperationException Refused(LinqExpression term, string reason) => new($"'{Printed.Of(term)}' {reason}");

    private sealed class ItemFinder(Dictionary<ParameterExpression, string> items) : ExpressionVisitor
    {
        internal bool Found { get; private set; }

        protected override LinqExpression VisitParameter(ParameterExpression node)
        {
            Found |= items.ContainsKey(node);
            return node;
        }
    }
}
