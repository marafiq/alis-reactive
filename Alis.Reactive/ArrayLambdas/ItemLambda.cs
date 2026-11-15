using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.Json;
using Alis.Reactive.Conditions;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using Alis.Reactive.TypeKnowledge;
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
        => Outermost(field, form).Fn(field, body => fieldHoldsNull ? body : new OpExpression(Operator.NonNull, [body]), takenAsHeld: _ => true);

    internal static FnExpression Totalling(LambdaExpression term, HeldForm form)
        => Outermost(term, form).Fn(term, body => body, takenAsHeld: member => !member.HasItsOwnConverter && ComparedTypes.IsDecimal(member.Contract.Type));

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

    private FnExpression Fn(LambdaExpression lambda, Func<PlanExpression, PlanExpression> shaped) => Fn(lambda, shaped, takenAsHeld: _ => false);

    private FnExpression Fn(LambdaExpression lambda, Func<PlanExpression, PlanExpression> shaped, Func<WrittenMember, bool> takenAsHeld)
    {
        var name = NextItem;
        var items = new Dictionary<ParameterExpression, string>(_items) { [lambda.Parameters[0]] = name };
        var body = new ItemLambda(_lambda, items, _form);
        if (body.ReadsItsOwnForm(lambda.Body, out var read) && takenAsHeld(read.Path.Member)) throw new NotImplementedException();
        return new FnExpression([name], shaped(body.Term(lambda.Body)));
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

    private DataExpression DataRead(MemberExpression member)
    {
        if (!ReadsAPath(member, out var root)) throw NoBrowserForm(member);
        var path = PathOf(member, root);
        if (path.Member.HoldsItsOwnForm)
        {
            throw new InvalidOperationException(
                $"'{Printed.Of(member)}' is written by {path.Member.WhatWritesItsOwnForm}, unlike {_form.BaseFormName}, so the browser does not hold the value .NET computes with; take it whole in a Select, or compute on the server.");
        }

        return Data(path, root);
    }

    private DataExpression Data(MemberPath path, ParameterExpression root) => new(path.DataSteps(), new RefExpression(_items[root]));

    private bool ReadsAPath(MemberExpression member, [NotNullWhen(true)] out ParameterExpression? root)
    {
        LinqExpression node = member;
        while (node is MemberExpression { Expression: { } holder })
        {
            if (_form.WritesAsAScalar(holder.Type))
            {
                root = null;
                return false;
            }

            node = holder;
        }

        root = node as ParameterExpression;
        return root is not null && _items.ContainsKey(root);
    }

    private MemberPath PathOf(MemberExpression member, ParameterExpression root)
    {
        if (!MemberPath.TryOf(member, root, _lambda, _form.DataOptions, out var path)) throw NoBrowserForm(member);
        path.RefuseAReadPastItsOwnConverter(Printed.Of(member));
        return path;
    }

    private bool ReadsItsOwnForm(LinqExpression node, [NotNullWhen(true)] out OwnFormRead? read)
    {
        read = null;
        if (node is not MemberExpression member || TranslatedMembers.Translates(member) || !ReadsAPath(member, out var root)) return false;
        var path = PathOf(member, root);
        read = path.Member.HoldsItsOwnForm ? new OwnFormRead(member, path, root) : null;
        return read is not null;
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
            RefuseAMemberInItsOwnFormBesideAnother(comparison, left, right);
            RequireComparableMembers(comparison, left.Type, right.Type, orders);
            return new OpExpression(op, [Keys.Of(left.Type, Read(left)), Keys.Of(right.Type, Read(right))]);
        }

        var member = leftReadsAnItem ? left : right;
        var held = leftReadsAnItem ? right : left;
        if (ReadsItsOwnForm(member, out var inItsOwnForm)) return ComparedInItsOwnForm(comparison, op, orders, inItsOwnForm, held, leftReadsAnItem);
        var writtenAs = WrittenAs(member, held);
        var written = TranslatedMembers.ReadsADayOfTheWeek(member) ? DayNumber(ViewValues.Of(held)) : _form.Beside(Held(ViewValues.Of(held), held.Type, writtenAs), writtenAs);
        RequireComparableValue(comparison, written.Value, member.Type, orders);
        var memberRead = Keys.Of(member.Type, Read(member));
        var value = Keys.Of(member.Type, written);
        return new OpExpression(op, leftReadsAnItem ? [memberRead, value] : [value, memberRead]);
    }

    private static Type WrittenAs(LinqExpression member, LinqExpression held)
    {
        var compilerFoldedTheValueToANumber = !ComparedTypes.IsNumber(member.Type) && ComparedTypes.IsNumber(held.Type);
        return compilerFoldedTheValueToANumber ? member.Type : held.Type;
    }

    private OpExpression ComparedInItsOwnForm(BinaryExpression comparison, Operator op, bool orders, OwnFormRead read, LinqExpression held, bool memberOnTheLeft)
    {
        var member = read.Member;
        var written = read.Path.Member;
        var unlikeTheBase = $"'{Printed.Of(member)}', which {written.WhatWritesItsOwnForm} writes unlike {_form.BaseFormName}";
        if (orders) throw Refused(comparison, $"orders {unlikeTheBase}, so the browser does not order it as .NET does; order it on the server.");
        if (!_form.WritesAsAScalar(member.Type)) throw Refused(comparison, $"compares {unlikeTheBase}, and .NET compares the object it holds; compare it on the server.");
        if (Keys.IsDateOrTime(member.Type)) throw Refused(comparison, "compares a date or a time its own converter writes, whose text the converter decides; compare it on the server.");
        var value = written.Written(Held(ViewValues.Of(held), held.Type, WrittenAs(member, held)));
        var writtenAsText = value.ValueKind == JsonValueKind.String;
        if (writtenAsText && ComparedTypes.IsBinaryFraction(member.Type))
            throw Refused(comparison, "compares a float or double the JSON writes as text, whose text does not equal as the number does, as -0 and 0 or NaN and NaN; compare it on the server.");
        if (writtenAsText && ComparedTypes.IsDecimal(member.Type) && written.HasItsOwnConverter)
            throw Refused(comparison, "compares a decimal its own converter writes as text, whose text the converter decides; compare it on the server.");
        var data = Data(read.Path, read.Root);
        var literal = new LitExpression(value);
        PlanExpression[] operands = memberOnTheLeft ? [data, literal] : [literal, data];
        var comparedAsDecimals = ComparedTypes.IsDecimal(member.Type) && !written.HasItsOwnConverter;
        if (!comparedAsDecimals) return new OpExpression(op, operands);
        var equal = new OpExpression(Operator.EqDecimal, operands);
        return op == Operator.Eq ? equal : new OpExpression(Operator.Not, [equal]);
    }

    private void RefuseAMemberInItsOwnFormBesideAnother(BinaryExpression comparison, LinqExpression left, LinqExpression right)
    {
        if (!ReadsItsOwnForm(left, out var read) && !ReadsItsOwnForm(right, out read)) return;
        throw Refused(comparison, $"compares two members, and {read.Path.Member.WhatWritesItsOwnForm} writes '{Printed.Of(read.Member)}' unlike {_form.BaseFormName}, so the browser does not compare them as .NET does; compare them on the server.");
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

    private sealed record OwnFormRead(MemberExpression Member, MemberPath Path, ParameterExpression Root);

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
