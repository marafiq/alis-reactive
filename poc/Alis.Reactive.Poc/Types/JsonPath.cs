using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using Alis.Reactive.Plan;

namespace Alis.Reactive.Types;

internal static class JsonPath
{
    internal static IReadOnlyList<PathStep> Of(LambdaExpression path, JsonSerializerOptions options)
    {
        var steps = new List<PathStep>();
        var node = path.Body;
        while (node is MemberExpression { Expression: { } holder } access)
        {
            steps.Add(PathStep.Member(JsonName(access, holder.Type, options, path)));
            node = holder;
        }

        var readsTheValue = node == path.Parameters[0] && steps.Count > 0;
        if (!readsTheValue) throw new ArgumentException($"'{path}' is not a path of members of its value, such as n => n.Pager.Number.", nameof(path));
        steps.Reverse();
        return steps;
    }

    private static string JsonName(MemberExpression access, Type holder, JsonSerializerOptions options, LambdaExpression path)
    {
        foreach (var property in options.GetTypeInfo(holder).Properties)
        {
            var written = property.Get is not null;
            if (written && Equals(property.AttributeProvider, access.Member)) return property.Name;
        }

        throw new ArgumentException($"'{path}' reads a member of '{holder.FullName}' that the app's JSON options do not write, so the browser never holds it.", nameof(path));
    }
}
