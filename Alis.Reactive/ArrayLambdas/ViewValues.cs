using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

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

    private static InvalidOperationException ReadThroughNull(MemberExpression member)
        => new($"'{Printed.Of(member)}' reads a member of a value that is null when the view renders.");

    [SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "The owner's exception, 2026-11-06: a value the view holds inside an array lambda is read along its chain of fields and properties when the view renders (D6.7, D18.1).")]
    private static object? Read(MemberExpression member, object? holder)
        => member.Member is FieldInfo field ? field.GetValue(holder) : ((PropertyInfo)member.Member).GetValue(holder);
}
