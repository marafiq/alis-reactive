using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using Alis.Reactive.Schema;
using Alis.Reactive.TypeKnowledge;

namespace Alis.Reactive.PathsAndWire;

internal sealed partial class MemberPath
{
    private readonly IReadOnlyList<MemberLink> _links;

    private MemberPath(IReadOnlyList<MemberLink> links)
    {
        _links = links;
    }

    internal string BindingName => string.Join(".", _links.Select(link => link.ClrName));

    internal partial IReadOnlyList<PathStep> BodyPath { get; }

    internal IReadOnlyList<PathStep> DataPath => [.. _links.Select(link => new MemberStep(link.Written.Name))];

    internal IReadOnlyList<DataStep> DataSteps() => DataStarts.Steps([.. _links.Select(link => link.Written)]);

    internal bool EndsInItsOwnForm => _links[_links.Count - 1].Written.HoldsItsOwnForm;

    internal void RefuseAReadPastItsOwnConverter(string read)
    {
        var converted = _links.Take(_links.Count - 1).FirstOrDefault(link => link.Written.HasItsOwnConverter);
        if (converted is null) return;
        throw new InvalidOperationException(
            $"'{read}' reads past '{converted.Text}', which its own JSON converter writes, so the browser holds what the converter writes, not its members; " +
            $"read '{converted.Text}' itself, or read past it on the server.");
    }

    internal static bool TryOf(LambdaExpression path, JsonSerializerOptions options, [NotNullWhen(true)] out MemberPath? members)
        => TryOf(path.Body, path.Parameters[0], path, options, out members);

    internal static bool TryOf(System.Linq.Expressions.Expression read, ParameterExpression root, LambdaExpression lambda, JsonSerializerOptions options, [NotNullWhen(true)] out MemberPath? members)
    {
        var accesses = new List<MemberLink.Access>();
        var node = read;
        while (node is MemberExpression { Expression: { } holder } access)
        {
            accesses.Add(new MemberLink.Access(access, holder.ToString(), options.GetTypeInfo(holder.Type)));
            node = holder;
        }

        var readsItsValue = accesses.Count > 0 && node == root;
        accesses.Reverse();
        members = readsItsValue ? new MemberPath([.. accesses.Select(access => MemberLink.Of(access, lambda))]) : null;
        return members is not null;
    }

    private sealed class MemberLink
    {
        private MemberLink(string clrName, string text, WrittenMember written)
        {
            ClrName = clrName;
            Text = text;
            Written = written;
        }

        internal string ClrName { get; }

        internal string Text { get; }

        internal WrittenMember Written { get; }

        internal static MemberLink Of(Access access, LambdaExpression path)
            => new(access.Member.ToString().Substring(access.HolderText.Length + 1), access.Member.ToString(), WrittenMember.Of(access.Member, access.Holder, path));

        internal sealed record Access(MemberExpression Member, string HolderText, System.Text.Json.Serialization.Metadata.JsonTypeInfo Holder);
    }
}
