using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.Plan;

namespace Alis.Reactive.Types;

internal sealed class DataPath
{
    internal DataPath(IReadOnlyList<DataStep> steps, HeldForm form)
    {
        Steps = steps;
        Form = form;
    }

    internal IReadOnlyList<DataStep> Steps { get; }

    internal HeldForm Form { get; }
}

internal static class JsonPath
{
    internal static DataPath Of(LambdaExpression path, HeldForm valueForm, DataStarts starts, string argument)
    {
        var links = Links(path.Body, path.Parameters[0], valueForm, starts, path, argument)
            ?? throw new ArgumentException($"'{path}' is not a path of members of its value, such as n => n.Pager.Number.", argument);
        return new DataPath(starts.Steps(links), HeldForm.Of(links[links.Count - 1], starts.Values).For(path.Body.ToString()));
    }

    internal static IReadOnlyList<DataLink>? Links(System.Linq.Expressions.Expression node, ParameterExpression value, HeldForm valueForm, DataStarts starts, LambdaExpression path, string argument)
    {
        var read = node;
        var accesses = new List<(MemberExpression Access, Type Holder)>();
        while (node is MemberExpression { Expression: { } holder } access)
        {
            accesses.Add((access, holder.Type));
            node = holder;
        }

        var readsTheValue = node == value && accesses.Count > 0;
        if (!readsTheValue) return null;
        if (valueForm.By == HeldBy.ItsConverter) throw ReadsPast(read, value, argument);
        accesses.Reverse();
        var links = new List<DataLink>(accesses.Count);
        foreach (var (access, holder) in accesses)
        {
            var readsPastItsConverter = links.Count > 0 && links[links.Count - 1].Member.CustomConverter is not null;
            if (readsPastItsConverter) throw ReadsPast(read, (MemberExpression)access.Expression!, argument);
            links.Add(new DataLink(holder, Written(access, holder, starts.Values, path, argument)));
        }

        return links;
    }

    private static JsonPropertyInfo Written(MemberExpression access, Type holder, JsonSerializerOptions options, LambdaExpression path, string argument)
    {
        var declared = DeclaredName(access, holder, options, path, argument);
        foreach (var property in options.GetTypeInfo(holder).Properties)
        {
            var written = property.Get is not null;
            if (written && property.Name == declared) return property;
        }

        throw NotWritten(path, holder, argument);
    }

    private static string DeclaredName(MemberExpression access, Type holder, JsonSerializerOptions options, LambdaExpression path, string argument)
    {
        if (DeclaringClass(access) is { } declaring)
        {
            foreach (var property in options.GetTypeInfo(declaring).Properties)
            {
                if (Equals(property.AttributeProvider, access.Member)) return property.Name;
            }
        }

        throw NotWritten(path, holder, argument);
    }

    internal static string? DeclaringClassName(MemberExpression access) => DeclaringClass(access)?.FullName;

    [SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "The owner's exception, 2026-11-06: the class that declares a lambda's member is a type's identity, not a discovery of members (D18.1).")]
    private static Type? DeclaringClass(MemberExpression access) => access.Member.DeclaringType;

    private static ArgumentException ReadsPast(System.Linq.Expressions.Expression read, System.Linq.Expressions.Expression member, string argument)
        => new($"'{read}' reads past '{member}', which its own JSON converter writes, so the browser holds what the converter writes, not its members; read '{member}' itself, or read past it on the server.", argument);

    private static ArgumentException NotWritten(LambdaExpression path, Type holder, string argument)
        => new($"'{path}' reads a member of '{holder.FullName}' that the app's JSON options do not write, so the browser never holds it.", argument);
}
