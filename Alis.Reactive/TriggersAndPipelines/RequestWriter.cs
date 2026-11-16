using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Declarations;
using Alis.Reactive.Execution;
using Alis.Reactive.OwnershipAndState;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Schema;
using Alis.Reactive.Validation;

namespace Alis.Reactive.TriggersAndPipelines;

internal sealed class RequestWriter<TModel>
{
    private readonly BlockWriter _writer;
    private readonly RequestPartsWriter _parts;
    private readonly RequestMethod _method;
    private readonly RefExpression _argument;
    private readonly RefExpression _reply;
    private readonly bool _enteredAfterAwait;
    private readonly BlockWriter.Place _place;
    private readonly List<StatusRoute> _statuses = [];
    private IReadOnlyList<Statement> _loading = [];
    private IReadOnlyList<Statement> _finally = [];
    private IReadOnlyList<Statement> _chained = [];
    private Route _success = new(SuccessBody.None, []);
    private OtherwiseRoute _otherwise = new(OtherwiseBody.None, []);
    private bool _hasLoading;
    private bool _hasFinally;
    private bool _hasOtherwise;
    private IReadOnlyList<Statement> _check = [];
    private bool _namesItsCheck;
    private (RefExpression Body, RefExpression Failures)? _checkNames;
    private int _stage;

    private RequestWriter(BlockWriter writer, RequestPartsWriter parts, RequestMethod method, Func<RequestWriter<TModel>, IReadOnlyList<Statement>> defaultCheck)
    {
        _writer = writer;
        _parts = parts;
        _method = method;
        _argument = writer.NextBinding("argument");
        _reply = writer.RequestReply();
        _enteredAfterAwait = writer.PathAwaited;
        _check = defaultCheck(this);
        _place = writer.Placed(Written());
    }

    internal static RequestWriter<TModel> Open<TRequest>(BlockWriter writer, string url, RequestMethod method, Action<QueryParts<TRequest>> parts)
        => Open(writer, url, method, request => parts(new QueryParts<TRequest>(request, 0)), DefaultCheck<TRequest>);

    internal static RequestWriter<TModel> Open<TRequest>(BlockWriter writer, string url, RequestMethod method, Action<BodyParts<TRequest>> parts)
        => Open(writer, url, method, request => parts(new BodyParts<TRequest>(request, 0)), DefaultCheck<TRequest>);

    private static RequestWriter<TModel> Open(BlockWriter writer, string url, RequestMethod method, Action<RequestPartsWriter> parts, Func<RequestWriter<TModel>, IReadOnlyList<Statement>> defaultCheck)
    {
        if (writer.Content.Antiforgery.Choice == AntiforgeryChoice.NoRequestsCall) throw new InvalidOperationException(RequestHost.NoRequestsCall(method.Text + " " + url));
        var request = new RequestPartsWriter(writer, url);
        parts(request);
        request.Complete();
        return new RequestWriter<TModel>(writer, request, method, defaultCheck);
    }

    internal int Validate<TRequest, TValidator>(int stage, string ruleSet)
        where TValidator : ReactiveValidator<TRequest>
    {
        Continue(stage);
        NamesItsCheck();
        _check = RequestChecks.Named<TRequest, TValidator>(_writer, CheckParts, ruleSet);
        return Changed();
    }

    internal int NoValidation(int stage)
    {
        Continue(stage);
        NamesItsCheck();
        _check = [];
        return Changed();
    }

    internal int Success<TReply>(int stage, SuccessBody kind, TReply reply, Action<TReply, Pipeline<TModel>> body) where TReply : JsObject
    {
        Continue(stage);
        _success = new Route(kind, Route(reply, body));
        return Changed();
    }

    internal int Status<TReply>(int stage, int status, ErrorBody kind, TReply reply, Action<TReply, Pipeline<TModel>> body) where TReply : JsObject
    {
        Continue(stage);
        if (status is < 300 or > 599)
            throw new InvalidOperationException($"OnError status {status} is outside 300 to 599; name an error status in that range.");
        if (_statuses.Any(route => route.Status == status))
            throw new InvalidOperationException($"OnError already names status {status}; write one route for that status.");
        _statuses.Add(new StatusRoute(status, kind, Route(reply, body)));
        return Changed();
    }

    internal int Otherwise<TReply>(int stage, OtherwiseBody kind, TReply reply, Action<TReply, Pipeline<TModel>> body) where TReply : JsObject
    {
        Continue(stage);
        if (_hasOtherwise) throw new InvalidOperationException("OnError already has a catch-all route; write one catch-all for the request.");
        _hasOtherwise = true;
        _otherwise = new OtherwiseRoute(kind, Route(reply, body));
        return Changed();
    }

    internal int Loading(int stage, Action<Pipeline<TModel>> body)
    {
        Continue(stage);
        if (_hasLoading) throw new InvalidOperationException("WhileLoading is already written for this request; put its steps in one block.");
        _hasLoading = true;
        _loading = _writer.Branch(_enteredAfterAwait, () => body(new Pipeline<TModel>(_writer))).Block;
        return Changed();
    }

    internal int Finally(int stage, Action<Pipeline<TModel>> body)
    {
        Continue(stage);
        if (_hasFinally) throw new InvalidOperationException("Finally is already written for this request; put its steps in one block.");
        _hasFinally = true;
        _finally = _writer.Branch(true, () => body(new Pipeline<TModel>(_writer))).Block;
        return Changed();
    }

    internal void Chained<TReply>(int stage, TReply reply, Action<TReply, Pipeline<TModel>> body)
    {
        Continue(stage);
        _chained = _writer.Branch(true, () => body(reply, new Pipeline<TModel>(_writer))).Block;
        Changed();
    }

    private IReadOnlyList<Statement> Route<TReply>(TReply reply, Action<TReply, Pipeline<TModel>> body) where TReply : JsObject
    {
        reply.Reach(new Reached(_reply, _writer));
        if (reply is ISentRequestReply sent) sent.SentBy(_parts);
        return _writer.Branch(true, () => body(reply, new Pipeline<TModel>(_writer))).Block;
    }

    private static IReadOnlyList<Statement> DefaultCheck<TRequest>(RequestWriter<TModel> request) => RequestChecks.Default<TRequest>(request._writer, request.CheckParts);

    private RequestCheckParts CheckParts()
    {
        _checkNames ??= (_writer.NextBinding("checked"), _writer.NextBinding("failures"));
        return _parts.CheckParts(_argument, _method, _checkNames.Value);
    }

    private void NamesItsCheck()
    {
        if (_namesItsCheck) throw new InvalidOperationException("The request already names its check; write one Validate or NoValidation for the request.");
        _namesItsCheck = true;
    }

    private IReadOnlyList<Statement> Check() => _check;

    private void Continue(int stage)
    {
        if (stage != _stage || !_writer.Replaced(_place, Written())) throw BrokenSentence();
        _stage++;
    }

    private int Changed()
    {
        if (!_writer.Replaced(_place, Written())) throw BrokenSentence();
        return _stage;
    }

    private RequestStatement Written()
        => new(_argument.Name, _reply.Name, _parts.Send(_method, _success.Kind, RequestHost.RespondedJson),
            Check(), [new Layer(_loading, _finally)], _success, _statuses.ToArray(), _otherwise, _chained);

    private static InvalidOperationException BrokenSentence()
        => new("A request's next door continues its current stage before any other step of its block; write the request in one sentence.");
}
