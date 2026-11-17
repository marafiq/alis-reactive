using System;
using System.Linq.Expressions;
using Alis.Reactive.Declarations;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.RequestsAndResponses;

internal static class ReplyValues
{
    internal static ReadOnly<int> Status(JsObject reply)
        => new(reply, ContractRecord<HttpResponse>.Name(response => response.Status), false);

    internal static ReadOnly<FailureOutcome> Outcome(JsObject reply)
        => new(reply, ContractRecord<HttpResponse>.Name(response => response.Outcome), false);

    internal static ResponseHeaders Headers(JsObject reply)
    {
        var headers = new ResponseHeaders();
        headers.Reach(new Reached(new GetExpression([new MemberStep(ContractRecord<HttpResponse>.Name(response => response.Headers))],
            PathMembers.Required, reply.Bound.Target), reply.Bound.Writer));
        return headers;
    }
}

internal sealed class ReplyBody<TValue> : ReadOnly<TValue>
{
    private readonly SuccessBody _kind;

    internal ReplyBody(JsObject reply, SuccessBody kind)
        : base(reply, ContractRecord<HttpResponse>.Name(response => response.Body), false)
    {
        _kind = kind;
    }

    internal override Schema.Expression Expression => new GetExpression([new MemberStep(ValueName)], PathMembers.Optional, base.Expression);

    internal override HeldForm Form
    {
        get
        {
            var writer = Owner.Bound.Writer;
            return _kind == SuccessBody.Json
                ? HeldForm.App(writer.Content.Json, writer.Content.RendersUnder)
                : _kind == SuccessBody.File
                    ? HeldForm.App(new AppJson(() => PlanJson.Options), writer.Content.RendersUnder)
                    : HeldForm.Live(writer.Content.Json, writer.Content.RendersUnder);
        }
    }

    private string ValueName => _kind == SuccessBody.Json ? ContractRecord<JsonResponseBody>.Name(body => body.Value)
        : _kind == SuccessBody.PlainText ? ContractRecord<TextResponseBody>.Name(body => body.Value)
        : _kind == SuccessBody.Html ? ContractRecord<HtmlResponseBody>.Name(body => body.Value)
        : ContractRecord<FileResponseBody>.Name(body => body.Value);
}

internal interface ISentRequestReply
{
    void SentBy(RequestPartsWriter parts);
}

internal sealed class SentRequestValue<TRequest>(JsObject reply, RequestPartsWriter parts)
    : ReadOnly<TRequest>(reply, ContractRecord<HttpResponse>.Name(response => response.Request), false)
{
    internal override Schema.Expression Expression
        => parts.SendsAnyInJson ? throw JsonReadBack("The request, read whole, sends members") : base.Expression;

    internal override TypedSource<TResult> ReadPast<TResult>(Expression<Func<TRequest, TResult>> path)
    {
        var member = RequestMember.Of(path, Owner.Bound.Writer.Content.Json.App);
        if (parts.SendsInJson(member.Name)) throw JsonReadBack($"'{path}' is sent");
        return new Computed<TResult>(new GetExpression(member.Path, PathMembers.Optional, base.Expression), Form);
    }

    private static InvalidOperationException JsonReadBack(string read)
        => new($"{read} in the request's JSON body, and a reply reads back only a member the request sent in its path, query or form data; " +
            "read the value from its source, or send the member in the path, query or form data.");
}
