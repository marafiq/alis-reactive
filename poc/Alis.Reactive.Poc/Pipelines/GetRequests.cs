using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Components;
using Alis.Reactive.Plan;
using Alis.Reactive.Requests;
using Alis.Reactive.Writing;

namespace Alis.Reactive.Pipelines;

public sealed class NoModel
{
    private NoModel()
    {
    }
}

public sealed class GetRequest<TModel, TRequest>
{
    private readonly RoutedRequest _request;
    private bool _written;

    internal GetRequest(RoutedRequest request)
    {
        _request = request;
    }

    public GetRequest<TModel, TRequest, Response<TRequest, TBody>> OnSuccess<TBody>(Action<Response<TRequest, TBody>, Pipeline<TModel>> success)
        => Succeeds(BodyKind.Json, new Response<TRequest, TBody>(), success);

    public GetRequest<TModel, TRequest> OnError<TError>(int status, Action<Response<TRequest, TError>, Pipeline<TModel>> error)
    {
        Write();
        _request.Fails(status, BodyKind.Json, new Response<TRequest, TError>(), error);
        return new(_request);
    }

    public GetRequest<TModel, TRequest> OnError(Action<Failure, Pipeline<TModel>> failed)
    {
        Write();
        _request.CatchesAll(failed);
        return new(_request);
    }

    internal GetRequest<TModel, TRequest, TReply> Succeeds<TReply>(BodyKind kind, TReply reply, Action<TReply, Pipeline<TModel>> success)
        where TReply : JsObject
    {
        Write();
        _request.Succeeds(kind, reply, success);
        return new(_request);
    }

    private void Write()
    {
        _request.Takes(_written);
        _written = true;
    }
}

public sealed class GetRequest<TModel, TRequest, TReply>
{
    private readonly RoutedRequest _request;
    private bool _written;

    internal GetRequest(RoutedRequest request)
    {
        _request = request;
    }

    public GetRequest<TModel, TRequest, TReply> OnError<TError>(int status, Action<Response<TRequest, TError>, Pipeline<TModel>> error)
    {
        Write();
        _request.Fails(status, BodyKind.Json, new Response<TRequest, TError>(), error);
        return new(_request);
    }

    public GetRequest<TModel, TRequest, TReply> OnError(Action<Failure, Pipeline<TModel>> failed)
    {
        Write();
        _request.CatchesAll(failed);
        return new(_request);
    }

    private void Write()
    {
        _request.Takes(_written);
        _written = true;
    }
}

internal sealed class RoutedRequest
{
    private readonly PlanBuild _build;
    private readonly Func<BodyKind, Plan.Expression> _send;
    private readonly RequestStatement _statement;
    private bool _caughtAll;

    internal RoutedRequest(PlanBuild build, Func<BodyKind, Plan.Expression> send)
    {
        _build = build;
        _send = send;
        _statement = build.Request(send(BodyKind.None));
    }

    internal void Takes(bool written)
    {
        if (written)
        {
            throw new InvalidOperationException(
                "A request is written as one chain, each door on the stage the door before it returned, and this stage took a door already: write pipeline.Get<TRequest>(url, request => ..).OnSuccess<TBody>((reply, pipeline) => ..).OnError((failed, pipeline) => ..).");
        }

        if (!_build.Follows(_statement))
        {
            throw new InvalidOperationException(
                "A route belongs to the request it follows: write it in the same sentence, before any other step, as pipeline.Get<TRequest>(url, request => ..).OnSuccess<TBody>((reply, pipeline) => ..) or pipeline.Get(url).Into(id).");
        }
    }

    internal void Succeeds<TReply, TModel>(BodyKind kind, TReply reply, Action<TReply, Pipeline<TModel>> success)
        where TReply : JsObject
    {
        _statement.Send = _send(kind);
        _statement.Success = new Route(kind, Routed(reply, success));
    }

    internal void Fails<TReply, TModel>(int status, BodyKind kind, TReply reply, Action<TReply, Pipeline<TModel>> error)
        where TReply : JsObject
    {
        var notAnError = status is < 300 or > 599;
        if (notAnError) throw new ArgumentOutOfRangeException(nameof(status), $"An error route is for a status from 300 to 599, not {status}.");
        if (_statement.Statuses.Any(each => each.Status == status)) throw new InvalidOperationException($"A request has one route for the status {status}.");
        _statement.Statuses = [.. _statement.Statuses, new StatusRoute(status, kind, Routed(reply, error))];
    }

    internal void CatchesAll<TModel>(Action<Failure, Pipeline<TModel>> failed)
    {
        if (_caughtAll) throw new InvalidOperationException("A request has one route for every other outcome.");
        _caughtAll = true;
        _statement.Otherwise = new OtherwiseRoute(Routed(new Failure(), failed));
    }

    private IReadOnlyList<Statement> Routed<TReply, TModel>(TReply reply, Action<TReply, Pipeline<TModel>> route)
        where TReply : JsObject
    {
        reply.Bind(new Binding(new RefExpression(_statement.Reply), _build, ReadPresence.Required));
        return _build.Route(_statement, () => route(reply, new Pipeline<TModel>(_build)));
    }
}
