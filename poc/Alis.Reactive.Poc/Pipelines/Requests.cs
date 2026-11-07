using System;
using System.Text.Json;
using Alis.Reactive.Plan;
using Alis.Reactive.Writing;

namespace Alis.Reactive.Pipelines;

public sealed class Request
{
    private readonly PlanBuild _build;
    private readonly RefExpression _response;
    private bool _routed;

    internal Request(PlanBuild build, string url)
    {
        _build = build;
        var send = new HttpSend(HttpMethod.Get, url, BodyKind.Html);
        var argument = new LiteralExpression(JsonSerializer.SerializeToElement(send, PlanJson.Options));
        _response = build.AwaitedResult(new CallExpression(new ObjectExpression(build.Http()), [PathStep.Member("send")], ReadPresence.Required, [argument]));
    }

    public void Into(string elementId)
    {
        Slots.RequireAnId(elementId);
        if (_routed) throw new InvalidOperationException("A request loads one slot: write one Into.");
        if (!_build.Follows(_response))
        {
            throw new InvalidOperationException(
                "Into is the route of the request it follows: write it in the same sentence, before any other step, as pipeline.Get(url).Into(id).");
        }

        _routed = true;
        var succeeded = new OperatorExpression(Operator.Eq, [Member(nameof(HttpResponse.Outcome)), new LiteralExpression(JsonSerializer.SerializeToElement(HttpOutcome.Success.Text))]);
        var load = new DoStatement(new CallExpression(new ObjectExpression(_build.Slot(elementId)), [PathStep.Member("load")], ReadPresence.Required, [Member(nameof(HttpResponse.Body))]), awaits: false);
        _build.When(succeeded, () => _build.Write(load));
    }

    private GetExpression Member(string name) => new(_response, [PathStep.Member(PlanJson.Options.PropertyNamingPolicy!.ConvertName(name))], ReadPresence.Required);
}

internal static class Slots
{
    internal static void RequireAnId(string elementId)
    {
        if (string.IsNullOrWhiteSpace(elementId)) throw new ArgumentException("A slot is reached by the id of its element.", nameof(elementId));
    }

    internal static void Unload(PlanBuild build, string elementId)
    {
        RequireAnId(elementId);
        build.Write(new DoStatement(new CallExpression(new ObjectExpression(build.Slot(elementId)), [PathStep.Member("unload")], ReadPresence.Required, []), awaits: false));
    }
}

internal static class RequestUrls
{
    internal static string Checked(string url)
    {
        if (IsAPagePath(url) || IsAnAbsoluteWebUri(url)) return url;
        throw new ArgumentException("A request's URL is a page path, such as /Cards/Card, or an absolute http or https URI.", nameof(url));
    }

    private static bool IsAPagePath(string url) => url is ['/', not '/', ..] or ['/'];

    private static bool IsAnAbsoluteWebUri(string url)
        => Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
