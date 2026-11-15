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

internal sealed class SentRequestValue<TRequest>(JsObject reply)
    : ReadOnly<TRequest>(reply, ContractRecord<HttpResponse>.Name(response => response.Request), false)
{
    internal override TypedSource<TResult> ReadPast<TResult>(LambdaExpression path)
        => new Computed<TResult>(new GetExpression(new RequestMember(path).Path, PathMembers.Optional, Expression), Form);
}
