using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.OwnershipAndState;
using Alis.Reactive.Schema;
using Alis.Reactive.TypeKnowledge;
using Alis.Reactive.ValuesAndSources;
using PlanExpression = Alis.Reactive.Schema.Expression;

namespace Alis.Reactive.RequestsAndResponses;

internal static class PageUrlReads
{
    internal static TypedSource<TValue> Read<TPage, TValue>(Expression<Func<TPage, TValue>> member, PlanContent plan, Func<string, PlanExpression> textOf)
    {
        var live = plan.Json.LiveForm;
        var written = WrittenAlong(member, live);
        var name = MvcText.Of(member);
        var leaf = live.GetTypeInfo(typeof(TValue));
        if (leaf.Kind != JsonTypeInfoKind.None) throw HoldsMoreThanOneValue(member, name, leaf);
        if (EnumRows.IsEnum(typeof(TValue))) throw IsAnEnum(member, name);
        var startWritten = StartOf(member, written);
        var shape = InputShapes.Of(leaf, startWritten, NumberCulture.Invariant, name, plan.Json);
        var start = new LitExpression(AsTheShapeHoldsIt(startWritten, shape));
        var read = new OpExpression(Operator.Coalesce, [new AsExpression(textOf(name), shape), start]);
        return new Computed<TValue>(read, HeldForm.PageText(plan.Json, plan.RendersUnder, read, start));
    }

    private static IReadOnlyList<WrittenMember> WrittenAlong<TPage, TValue>(Expression<Func<TPage, TValue>> member, JsonSerializerOptions live)
    {
        var written = new List<WrittenMember>();
        var node = member.Body;
        while (node is MemberExpression { Expression: { } holder } access)
        {
            var holderContract = live.GetTypeInfo(holder.Type);
            if (holderContract.Kind != JsonTypeInfoKind.Object) throw NotAMemberPath(member);
            written.Insert(0, WrittenMember.Of(access, holderContract, member));
            node = holder;
        }

        if (node != member.Parameters[0] || written.Count == 0) throw NotAMemberPath(member);
        return written;
    }

    private static JsonElement StartOf(LambdaExpression member, IReadOnlyList<WrittenMember> written)
    {
        if (DataStarts.StartOf(written) is StartAbsence start) return start.Value;
        throw new InvalidOperationException(
            $"'{member}' reads a member the app's JSON options make only from the data the endpoint is sent: an abstract class, or a class they bind by the type the data names. A member the page's URL lacks has no start its plan can carry: read a member of a class they make from an empty object.");
    }

    private static JsonElement AsTheShapeHoldsIt(JsonElement start, Shape shape)
    {
        if (shape is not NumberShape || start.ValueKind != JsonValueKind.String) return start;
        using var number = JsonDocument.Parse(start.GetString()!);
        return number.RootElement.Clone();
    }

    private static InvalidOperationException NotAMemberPath(LambdaExpression member)
        => new($"'{member}' reads the page's URL by more than its members: a member of the page's URL is read by a member path, as page => page.Ward or page => page.Stay.Ward.");

    private static InvalidOperationException HoldsMoreThanOneValue(LambdaExpression member, string name, JsonTypeInfo leaf)
        => new($"'{member}' reads {name}, which the app's JSON options write as {WrittenAs(leaf)}: a member of the page's URL holds one value, so read a member that holds one.");

    private static InvalidOperationException IsAnEnum(LambdaExpression member, string name)
        => new($"'{member}' reads {name}, an enum the page's URL holds by its name or its number, which the browser does not read as the enum's value: read a member that holds text, or read it on the server.");

    private static string WrittenAs(JsonTypeInfo leaf) => leaf.Kind switch
    {
        JsonTypeInfoKind.Enumerable => "a list",
        JsonTypeInfoKind.Dictionary => "a dictionary",
        _ => "an object",
    };
}
