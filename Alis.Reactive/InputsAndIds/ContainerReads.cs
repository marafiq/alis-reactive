using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.OwnershipAndState;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using Alis.Reactive.TypeKnowledge;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.InputsAndIds;

internal static class ContainerReads
{
    internal static TypedSource<TValue> Of<TModel, TValue>(PlanContent plan, string id, Expression<Func<TModel, TValue>> member)
    {
        var steps = MemberLambda.StepsOf(member, MvcText.Of(member)) ?? throw MemberPath.NoMemberPath(member);
        if (steps.Count == 0) throw MemberPath.NoMemberPath(member);
        var form = HeldForm.Live(plan.Json, plan.RendersUnder);
        var read = ModelPlugin.Sent(plan, id, ClassNames.ModelKey(typeof(TModel)));
        var members = new List<WrittenMember>();
        WrittenMember? previous = null;
        var previousText = "";
        foreach (var step in steps)
        {
            if (previous is { HasItsOwnConverter: true })
            {
                throw new InvalidOperationException(
                    $"'{member.Body}' reads past '{previousText}', which its own JSON converter writes, so the browser holds what the converter writes, not its members; read '{previousText}' itself, or read past it on the server.");
            }

            if (step.Node is MemberExpression access)
            {
                var written = WrittenMember.Of(access, form.DataOptions.GetTypeInfo(access.Expression!.Type), member);
                members.Add(written);
                previous = written;
                previousText = access.ToString();
                form = written.HoldsItsOwnForm ? form.WrittenBy(written, previousText) : form;
                continue;
            }

            read = PastMembers(read, members);
            var holder = step.Node is MethodCallExpression call ? call.Object! : ((BinaryExpression)step.Node).Left;
            var contract = form.DataOptions.GetTypeInfo(holder.Type);
            read = contract.Kind switch
            {
                JsonTypeInfoKind.Enumerable => new GetExpression([new IndexStep(int.Parse(step.Text, NumberStyles.None, CultureInfo.InvariantCulture))], PathMembers.Optional, read),
                JsonTypeInfoKind.Dictionary => new DataExpression([new DataStep(step.Text, [], NothingAbsence.Instance)], read),
                _ => throw MemberPath.NoMemberPath(member),
            };
            previous = null;
        }

        read = PastMembers(read, members);
        return form.Own is { Member.HasItsOwnConverter: false }
            ? new Computed<TValue>(form.IntoLive<TValue>(read), form.Computed)
            : new Computed<TValue>(read, form);
    }

    private static Schema.Expression PastMembers(Schema.Expression read, List<WrittenMember> members)
    {
        if (members.Count == 0) return read;
        var past = new DataExpression(DataStarts.Steps(members), read);
        members.Clear();
        return past;
    }
}
