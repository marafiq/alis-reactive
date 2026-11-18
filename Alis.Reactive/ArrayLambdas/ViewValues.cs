using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using Alis.Reactive.Conditions;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.TypeKnowledge;

namespace Alis.Reactive.ArrayLambdas;

internal static class ViewValues
{
    internal static object? Of(Expression node) => node switch
    {
        ConstantExpression constant => constant.Value,
        MemberExpression { Expression: null } shared => Read(shared, null),
        MemberExpression { Expression: { } holder } member => Read(member, Of(holder) ?? throw ReadThroughNull(member)),
        _ => throw Computed(node),
    };

    internal static InvalidOperationException Computed(Expression node)
        => new($"'{Printed.Of(node)}' computes a value inside a list's lambda; compute it into a local before the lambda, such as var ward = Model.Ward.ToUpper();.");

    internal static Type WrittenAs(Expression member, Expression held)
    {
        var compilerFoldedTheValueToANumber = !ComparedTypes.IsNumber(member.Type) && ComparedTypes.IsNumber(held.Type);
        return compilerFoldedTheValueToANumber ? member.Type : held.Type;
    }

    internal static object? Held(object? value, Type heldAs, Type writtenAs, JsonSerializerOptions options)
    {
        if (value is null || heldAs == writtenAs) return value;
        if (value is int code && ComparedTypes.IsChar(writtenAs)) return Convert.ToChar(code, CultureInfo.InvariantCulture);
        return JsonSerializer.SerializeToElement(value, heldAs, options).Deserialize(writtenAs, options);
    }

    internal static string InTerm(LambdaExpression term, AppJson json)
    {
        var item = term.Parameters[0];
        if (!Reads(item, term.Body)) return JsonSerializer.Serialize(Of(term.Body), term.Body.Type, json.LiveForm);
        if (term.Body is not BinaryExpression { NodeType: ExpressionType.Equal or ExpressionType.NotEqual or ExpressionType.LessThan or ExpressionType.LessThanOrEqual or ExpressionType.GreaterThan or ExpressionType.GreaterThanOrEqual } comparison)
            throw HoldsNoViewValue(term);
        var left = Unconverted(comparison.Left);
        var right = Unconverted(comparison.Right);
        var leftReadsTheItem = Reads(item, left);
        if (leftReadsTheItem == Reads(item, right)) throw HoldsNoViewValue(term);
        var member = leftReadsTheItem ? left : right;
        var held = leftReadsTheItem ? right : left;
        var writtenAs = WrittenAs(member, held);
        var value = Held(Of(held), held.Type, writtenAs, json.LiveForm);
        var written = OwnFormOf(item, member, term, json.LiveForm)?.Written(value, writtenAs) ?? JsonSerializer.SerializeToElement(value, writtenAs, json.LiveForm);
        var orders = comparison.NodeType is ExpressionType.LessThan or ExpressionType.LessThanOrEqual or ExpressionType.GreaterThan or ExpressionType.GreaterThanOrEqual;
        RefuseAnOrderingOfText(Printed.Of(term.Body), written, member.Type, orders);
        return written.GetRawText();
    }

    internal static void RefuseAnOrderingOfText(string term, JsonElement written, Type member, bool orders)
    {
        var orderedAsText = orders && written.ValueKind == JsonValueKind.String && !ComparedTypes.IsChar(member) && !Keys.IsDateOrTime(member);
        if (!orderedAsText) return;
        var cure = ComparedTypes.IsBinaryFraction(member)
            ? "order it on the server."
            : "compare it with == or !=, or order on the server.";
        throw new InvalidOperationException($"'{term}' orders a value the app's JSON writes as text, which does not order as the value does in .NET; " + cure);
    }

    private static WrittenMember? OwnFormOf(ParameterExpression item, Expression member, LambdaExpression term, JsonSerializerOptions options)
        => member is MemberExpression read && MemberPath.TryOf(read, item, term, options, out var path) && path.Member.HoldsItsOwnForm ? path.Member : null;

    private static InvalidOperationException HoldsNoViewValue(LambdaExpression term)
        => new($"'{Printed.Of(term.Body)}' compares no member of the item with a value the view holds; pass a comparison such as m => m.Beds > Model.MinSpare, or a term that reads no member of the item.");

    private static InvalidOperationException ReadThroughNull(MemberExpression member)
        => new($"'{Printed.Of(member)}' reads a member of a value that is null when the view renders.");

    private static bool Reads(ParameterExpression item, Expression node) => new ItemLambda.ItemFinder([item]).Finds(node);

    private static Expression Unconverted(Expression node)
        => node is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked, Method: null } conversion ? Unconverted(conversion.Operand) : node;

    [SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "The owner's exception, 2026-11-06: a value the view holds inside an array lambda is read along its chain of fields and properties when the view renders (D6.7, D18.1).")]
    private static object? Read(MemberExpression member, object? holder)
        => member.Member is FieldInfo field ? field.GetValue(holder) : ((PropertyInfo)member.Member).GetValue(holder);
}
