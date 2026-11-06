using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.Plan;

namespace Alis.Reactive.Types;

internal static class JsonPath
{
    internal static IReadOnlyList<DataStep> Of(LambdaExpression path, DataStarts starts)
        => Steps(path.Body, path.Parameters[0], starts, path)
            ?? throw new ArgumentException($"'{path}' is not a path of members of its value, such as n => n.Pager.Number.", nameof(path));

    internal static IReadOnlyList<DataStep>? Steps(System.Linq.Expressions.Expression node, ParameterExpression value, DataStarts starts, LambdaExpression path)
    {
        var accesses = new List<(MemberExpression Access, Type Holder)>();
        while (node is MemberExpression { Expression: { } holder } access)
        {
            accesses.Add((access, holder.Type));
            node = holder;
        }

        var readsTheValue = node == value && accesses.Count > 0;
        if (!readsTheValue) return null;
        accesses.Reverse();
        return starts.Steps(accesses.ConvertAll(step => new DataLink(step.Holder, Written(step.Access, step.Holder, starts.Values, path))));
    }

    private static JsonPropertyInfo Written(MemberExpression access, Type holder, JsonSerializerOptions options, LambdaExpression path)
    {
        var declared = DeclaredName(access, holder, options, path);
        foreach (var property in options.GetTypeInfo(holder).Properties)
        {
            var written = property.Get is not null;
            if (written && property.Name == declared) return property;
        }

        throw NotWritten(path, holder);
    }

    private static string DeclaredName(MemberExpression access, Type holder, JsonSerializerOptions options, LambdaExpression path)
    {
        if (DeclaringClass(access) is { } declaring)
        {
            foreach (var property in options.GetTypeInfo(declaring).Properties)
            {
                if (Equals(property.AttributeProvider, access.Member)) return property.Name;
            }
        }

        throw NotWritten(path, holder);
    }

    [SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "The owner's exception, 2026-11-06: the class that declares a lambda's member is a type's identity, not a discovery of members (D18.1).")]
    private static Type? DeclaringClass(MemberExpression access) => access.Member.DeclaringType;

    private static ArgumentException NotWritten(LambdaExpression path, Type holder)
        => new($"'{path}' reads a member of '{holder.FullName}' that the app's JSON options do not write, so the browser never holds it.", nameof(path));
}
