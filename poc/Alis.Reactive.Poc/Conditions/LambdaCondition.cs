using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using Alis.Reactive.Plan;
using Alis.Reactive.Types;
using LinqExpression = System.Linq.Expressions.Expression;
using PlanExpression = Alis.Reactive.Plan.Expression;

namespace Alis.Reactive.Conditions;

internal enum LambdaUse
{
    Computes,
    Takes,
    SumsDecimal,
}

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

    private readonly LambdaExpression _lambda;
    private readonly IReadOnlyDictionary<ParameterExpression, PlanExpression> _scope;
    private readonly IReadOnlyDictionary<ParameterExpression, HeldForm> _forms;
    private readonly DataStarts _starts;
    private readonly JsonSerializerOptions _values;
    private readonly HeldForm _optionsForm;
    private readonly string _argument;
    private readonly Func<string> _itemName;

    private LambdaCondition(LambdaExpression lambda, IReadOnlyDictionary<ParameterExpression, PlanExpression> scope, IReadOnlyDictionary<ParameterExpression, HeldForm> forms, DataStarts starts, string argument, Func<string> itemName)
    {
        _lambda = lambda;
        _scope = scope;
        _forms = forms;
        _starts = starts;
        _values = starts.Values;
        _optionsForm = HeldForm.OfTheOptions(starts.Values);
        _argument = argument;
        _itemName = itemName;
    }

    internal static PlanExpression Of(LambdaExpression lambda, PlanExpression item, HeldForm itemForm, DataStarts starts, string argument, Func<string> itemName, LambdaUse use)
        => Taken(lambda, item, itemForm, starts, argument, itemName, use).Body;

    internal static (PlanExpression Body, HeldForm Form) Taken(LambdaExpression lambda, PlanExpression item, HeldForm itemForm, DataStarts starts, string argument, Func<string> itemName, LambdaUse use)
    {
        var parameter = lambda.Parameters[0];
        var scope = new Dictionary<ParameterExpression, PlanExpression> { [parameter] = item };
        var forms = new Dictionary<ParameterExpression, HeldForm> { [parameter] = itemForm };
        var condition = new LambdaCondition(lambda, scope, forms, starts, argument, itemName);
        var bodyIsARead = lambda.Body is MemberExpression or ParameterExpression;
        return use switch
        {
            LambdaUse.Takes when bodyIsARead => (condition.Read(lambda.Body, TakesItsOwnForm), condition.FormOf(lambda.Body)),
            LambdaUse.SumsDecimal when bodyIsARead => (condition.Read(lambda.Body, IsDecimalText), condition._optionsForm),
            _ => (condition.Term(lambda.Body), condition._optionsForm),
        };
    }

    public PlanExpression Read(LinqExpression operand) => Read(operand, TakesNoOwnForm);

    private PlanExpression Read(LinqExpression operand, Func<HeldForm, bool> takesItsOwnForm)
    {
        var read = Unconverted(operand);
        if (!ReadsTheItem(read)) return new LiteralExpression(Written(ViewValues.Of(read, _argument), read.Type));
        if (read is ParameterExpression parameter && _scope.TryGetValue(parameter, out var bound))
        {
            var held = _forms[parameter];
            if (held.IsOwn && !takesItsOwnForm(held)) throw ComputesWith(held.Read, held);
            return bound;
        }

        if (read is UnaryExpression { NodeType: ExpressionType.ArrayLength } length) return ListForms.Length(Read(length.Operand));
        if (read is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } valueOf) return Op(Operator.NullableValue, Read(valueOf.Operand));
        if (read is BinaryExpression { NodeType: ExpressionType.Coalesce } coalesce) return Coalesced(coalesce);
        if (read is ConditionalExpression conditional) return Op(Operator.Cond, Term(conditional.Test), Term(conditional.IfTrue), Term(conditional.IfFalse));
        var translated = TranslatedMembers.Of(read, this, _argument);
        if (translated is not null) return translated;
        var root = RootOf(read);
        if (root is null || !_scope.TryGetValue(root, out var holder)) throw TranslatedMembers.NoBrowserForm(read, _argument);
        var links = JsonPath.Links(read, root, _forms[root], _starts, _lambda, _argument) ?? throw TranslatedMembers.NoBrowserForm(read, _argument);
        var form = HeldForm.Of(links[links.Count - 1], _values);
        if (form.IsOwn && !takesItsOwnForm(form)) throw ComputesWith(ViewValues.Printed(read), form);
        return new DataExpression(holder, _starts.Steps(links));
    }

    public FnExpression Lambda(LambdaExpression inner)
    {
        var item = _itemName();
        var scope = new Dictionary<ParameterExpression, PlanExpression>(_scope.Count + 1);
        foreach (var bound in _scope) scope[bound.Key] = bound.Value;
        scope[inner.Parameters[0]] = new RefExpression(item);
        var forms = new Dictionary<ParameterExpression, HeldForm>(_forms.Count + 1);
        foreach (var held in _forms) forms[held.Key] = held.Value;
        forms[inner.Parameters[0]] = _optionsForm;
        return new FnExpression([item], new LambdaCondition(inner, scope, forms, _starts, _argument, _itemName).Term(inner.Body));
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
        if (ComparedTypes.IsDateOrTime(left.Type) || ComparedTypes.IsDateOrTime(right.Type)) throw Refused(comparison, "compares dates or times, whose text neither orders nor equals as the instant does; compare them on the server.");
        if (leftReadsTheItem && rightReadsTheItem)
        {
            RequireNoOwnForm(comparison, left);
            RequireNoOwnForm(comparison, right);
            RequireComparableMembers(comparison, left, right, orders);
            return Op(op, Term(left), Term(right));
        }

        var member = leftReadsTheItem ? left : right;
        var memberSide = leftReadsTheItem ? comparison.Left : comparison.Right;
        var held = leftReadsTheItem ? right : left;
        var heldSide = leftReadsTheItem ? comparison.Right : comparison.Left;
        var compilerFoldedTheConstant = IsConverted(memberSide) && !IsConverted(heldSide);
        var writtenAs = compilerFoldedTheConstant ? member.Type : held.Type;
        var form = FormOf(member);
        if (form.IsOwn) RequireComparableInItsOwnForm(comparison, member, memberSide, form, orders);
        var written = form.Write(Held(ViewValues.Of(held, _argument), writtenAs), writtenAs);
        RequireComparableValue(comparison, written, member.Type, orders);
        var literal = new LiteralExpression(written);
        var memberRead = form.IsOwn ? Read(member, TakesItsOwnForm) : Term(member);
        return leftReadsTheItem ? Op(op, memberRead, literal) : Op(op, literal, memberRead);
    }

    private HeldForm FormOf(LinqExpression read)
    {
        if (read is ParameterExpression parameter && _forms.TryGetValue(parameter, out var held)) return held;
        if (read is not MemberExpression member || TranslatedMembers.Translates(member)) return _optionsForm;
        if (RootOf(read) is not { } root || !_scope.ContainsKey(root)) return _optionsForm;
        var links = JsonPath.Links(read, root, _forms[root], _starts, _lambda, _argument);
        return links is null ? _optionsForm : HeldForm.Of(links[links.Count - 1], _values).For(ViewValues.Printed(read));
    }

    private void RequireNoOwnForm(BinaryExpression comparison, LinqExpression member)
    {
        var form = FormOf(member);
        if (!form.IsOwn) return;
        throw Refused(comparison, $"compares two members, and {form.Cause} writes '{ViewValues.Printed(member)}' unlike the app's options, so the browser does not compare them as .NET does; compare them on the server.");
    }

    private void RequireComparableInItsOwnForm(BinaryExpression comparison, LinqExpression member, LinqExpression memberSide, HeldForm form, bool orders)
    {
        if (orders) throw Refused(comparison, $"orders '{ViewValues.Printed(member)}', which {form.Cause} writes unlike the app's options, so the browser does not order it as .NET does; order it on the server.");
        if (!ComparedTypes.IsScalar(memberSide.Type)) throw Refused(comparison, $"compares '{ViewValues.Printed(member)}', which {form.Cause} writes unlike the app's options, and .NET compares the object it holds; compare it on the server.");
    }

    private void RequireComparableMembers(BinaryExpression comparison, LinqExpression left, LinqExpression right, bool orders)
    {
        if (orders)
        {
            var orderable = (ComparedTypes.IsNumber(left.Type) && ComparedTypes.IsNumber(right.Type)) || (ComparedTypes.IsChar(left.Type) && ComparedTypes.IsChar(right.Type));
            if (!orderable) throw Refused(comparison, "orders values the browser cannot order as .NET does; order numbers or a char, or order on the server.");
            return;
        }

        var bothScalars = ComparedTypes.IsScalar(left.Type) && ComparedTypes.IsScalar(right.Type);
        var enumsCompared = ComparedTypes.IsNumber(comparison.Left.Type) && IsConverted(comparison.Left) && IsConverted(comparison.Right) && left.Type == right.Type;
        if (!bothScalars && !enumsCompared) throw ComparesObjects(comparison);
    }

    private void RequireComparableValue(BinaryExpression comparison, JsonElement written, Type member, bool orders)
    {
        if (written.ValueKind is JsonValueKind.Object or JsonValueKind.Array) throw ComparesObjects(comparison);
        var fractionAsText = written.ValueKind == JsonValueKind.String && ComparedTypes.IsFraction(member);
        if (fractionAsText) throw Refused(comparison, "compares a fraction the JSON writes as text, whose text neither equals nor orders as the number does, as 0.10 and 0.1; compare it on the server.");
        var orderedAsText = orders && written.ValueKind == JsonValueKind.String && !ComparedTypes.IsChar(member);
        if (orderedAsText) throw Refused(comparison, "orders a value the app's JSON writes as text, which does not order as the value does in .NET; compare it with == or !=, or order on the server.");
    }

    private JsonElement Written(object? value, Type type) => _optionsForm.Write(Held(value, type), type);

    private static object? Held(object? value, Type type) => value is int code && ComparedTypes.IsChar(type) ? Convert.ToChar(code) : value;

    private LinqExpression Unconverted(LinqExpression node)
    {
        while (node is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } conversion)
        {
            if (ComparedTypes.Narrows(conversion.Operand.Type, conversion.Type))
                throw Refused(conversion, "casts a number to a narrower one, which changes the value as C# computes it and not as the browser does; compare the member itself.");
            if (conversion.Method is { } method && !TranslatedMembers.IsAnExactDecimalConversion(method)) throw ConvertedUnlikeTheBrowser(conversion);
            var takesTheValue = conversion is { IsLifted: true, IsLiftedToNull: false, Method: null };
            if (takesTheValue && ReadsTheItem(conversion.Operand)) return conversion;
            if (takesTheValue && ViewValues.Of(conversion.Operand, _argument) is null)
                throw new ArgumentException($"'{ViewValues.Printed(conversion.Operand)}' is null when the view renders, and the cast takes its value, which .NET refuses; compare the nullable itself, such as d => d.Priority == limit.", _argument);
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

    private ArgumentException ComputesWith(string member, HeldForm form)
        => new($"'{member}' is written by {form.Cause}, unlike the app's options, so the browser does not hold the value .NET computes with; take it whole in a Select, or compute on the server.", _argument);

    private ArgumentException ComparesObjects(BinaryExpression comparison)
        => Refused(comparison, "compares objects, and the browser compares objects by reference; compare a member of each, such as d => d.Carer.Name == lead.");

    private ArgumentException Refused(LinqExpression term, string reason) => new($"'{ViewValues.Printed(term)}' {reason}", _argument);

    private ArgumentException ConvertedUnlikeTheBrowser(UnaryExpression conversion)
    {
        var operand = ViewValues.Printed(conversion.Operand);
        var castsADecimal = ComparedTypes.IsDecimal(conversion.Type) || ComparedTypes.IsDecimal(conversion.Operand.Type);
        if (castsADecimal) return new ArgumentException($"'{operand}' is cast to or from decimal, which rounds as C# computes it and not as the browser does; keep the member's own type, or move the work to the server.", _argument);
        return new ArgumentException($"'{operand}' is converted by a user-defined conversion, which the browser does not have; compare a member it reads, such as d => d.Seal.Tag == \"S4\", or move the work to the server.", _argument);
    }

    private static bool TakesNoOwnForm(HeldForm _) => false;

    private static bool TakesItsOwnForm(HeldForm _) => true;

    private static bool IsDecimalText(HeldForm form) => form.By == HeldBy.ItsNumberHandling;

    private static bool IsConverted(LinqExpression side) => side is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked };

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
