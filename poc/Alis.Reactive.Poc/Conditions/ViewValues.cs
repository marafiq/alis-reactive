using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace Alis.Reactive.Conditions;

internal static class ViewValues
{
    internal static object? Of(Expression node, string argument)
    {
        switch (node)
        {
            case ConstantExpression constant:
                return constant.Value;
            case MemberExpression { Expression: null } shared:
                return Read(shared, null);
            case MemberExpression { Expression: { } holder } member:
                var held = Of(holder, argument);
                if (held is null) throw new ArgumentException($"'{Printed(member)}' reads a member of a value that is null when the view renders.", argument);
                return Read(member, held);
            default:
                throw Computed(node, argument);
        }
    }

    internal static ArgumentException Computed(Expression node, string argument)
        => new($"'{Printed(node)}' computes a value inside a list's lambda; compute it into a local before the lambda, such as var ward = Model.Ward.ToUpper();.", argument);

    internal static string Printed(Expression node)
    {
        var text = node.ToString();
        var start = text.IndexOf("value(", StringComparison.Ordinal);
        while (start >= 0)
        {
            var end = text.IndexOf(").", start, StringComparison.Ordinal);
            if (end < 0) return text;
            text = text.Remove(start, end + 2 - start);
            start = text.IndexOf("value(", start, StringComparison.Ordinal);
        }

        return text;
    }

    [SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "The owner's exception, 2026-11-06: a value the view holds is read along its chain of fields and properties when the view renders (D6.7, D18.1).")]
    private static object? Read(MemberExpression member, object? holder)
        => member.Member is FieldInfo field ? field.GetValue(holder) : ((PropertyInfo)member.Member).GetValue(holder);
}
