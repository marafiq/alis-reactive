using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Alis.Reactive.Schema;

namespace Alis.Reactive.RequestsAndResponses;

internal sealed class RequestMember
{
    internal RequestMember(LambdaExpression member)
    {
        var names = new List<string>();
        var node = member.Body;
        while (node is MemberExpression { Expression: { } holder })
        {
            names.Add(node.ToString().Substring(holder.ToString().Length + 1));
            node = holder;
        }

        if (names.Count == 0 || node != member.Parameters[0])
            throw new InvalidOperationException($"'{member}' names no request member; name a member path, such as r => r.Address.Ward.");
        names.Reverse();
        Name = string.Join(".", names);
        TopLevel = names.Count == 1;
        Path = names.Select(name => (PathStep)new MemberStep(name)).ToArray();
    }

    internal string Name { get; }
    internal bool TopLevel { get; }
    internal IReadOnlyList<PathStep> Path { get; }
}
