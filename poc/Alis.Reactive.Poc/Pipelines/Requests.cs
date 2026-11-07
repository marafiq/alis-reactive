using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using Alis.Reactive.Plan;
using Alis.Reactive.Values;
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
        var (path, search) = RequestUrls.Split(url);
        var placeholder = RequestUrls.Pieces(path, nameof(url)).FirstOrDefault(piece => piece.Placeholder);
        if (placeholder is not null)
        {
            throw new ArgumentException(
                $"The URL '{url}' has the placeholder {placeholder.Text}, which a request that sets no members cannot fill; take the placeholder out of the URL.", nameof(url));
        }

        var send = new HttpSend(HttpMethod.Get, [new TextPart(path)], search, [], BodyKind.Html);
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

    private GetExpression Member(string name) => new(_response, [PathStep.Member(PlanJson.Name(name))], ReadPresence.Required);
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
        if (url.Contains('#')) throw new ArgumentException($"The URL '{url}' holds a fragment, which a request does not send.", nameof(url));
        if (IsAPagePath(url) || IsAnAbsoluteWebUri(url)) return url;
        throw new ArgumentException("A request's URL is a page path, such as /Cards/Card, or an absolute http or https URI.", nameof(url));
    }

    internal static (string Path, string Search) Split(string url)
    {
        var mark = url.IndexOf('?');
        return mark < 0 ? (url, "") : (url.Substring(0, mark), url.Substring(mark));
    }

    internal static IReadOnlyList<UrlPiece> Pieces(string path, string argument)
    {
        var pieces = new List<UrlPiece>();
        var at = 0;
        while (at < path.Length)
        {
            var open = path.IndexOf('{', at);
            var close = path.IndexOf('}', at);
            if (open < 0 && close < 0)
            {
                pieces.Add(new UrlPiece(path.Substring(at), placeholder: false));
                break;
            }

            if (open < 0 || close < open) throw Unpaired(path, argument);
            if (open > at) pieces.Add(new UrlPiece(path.Substring(at, open - at), placeholder: false));
            var name = path.Substring(open + 1, close - open - 1);
            if (name.Length == 0 || name.IndexOfAny(['{', '/']) >= 0) throw Unpaired(path, argument);
            pieces.Add(new UrlPiece(name, placeholder: true));
            at = close + 1;
        }

        return pieces;
    }

    private static ArgumentException Unpaired(string path, string argument)
        => new($"The URL '{path}' holds a brace without its pair; a placeholder is a member's name in braces, as {{name}}.", argument);

    private static bool IsAPagePath(string url) => url is ['/', not '/', ..] or ['/'];

    private static bool IsAnAbsoluteWebUri(string url)
        => Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}

internal sealed class UrlPiece
{
    internal UrlPiece(string text, bool placeholder)
    {
        Text = text;
        Placeholder = placeholder;
    }

    internal string Text { get; }

    internal bool Placeholder { get; }
}

public sealed class RequestMembers<TRequest>
{
    private readonly PlanBuild _build;
    private readonly List<SentMember> _members = [];

    internal RequestMembers(PlanBuild build)
    {
        _build = build;
    }

    internal IReadOnlyList<SentMember> Members => _members;

    public RequestMembers<TRequest> Set(Expression<Func<TRequest, string>> member, ITypedSource<string> value)
        => Add(member, Sources.Of(value).Expression);

    public RequestMembers<TRequest> Set(Expression<Func<TRequest, string>> member, string value)
        => Add(member, Literal.Of(value, _build));

    private RequestMembers<TRequest> Add(Expression<Func<TRequest, string>> member, Plan.Expression value)
    {
        var name = TopLevelName(member);
        if (_members.Any(each => string.Equals(each.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException($"The request sets '{name}' twice; set each member once.", nameof(member));
        }

        _members.Add(new SentMember(name, value));
        return this;
    }

    private static string TopLevelName(Expression<Func<TRequest, string>> member)
    {
        var readsTheRequest = member.Body is MemberExpression { Expression: ParameterExpression };
        if (!readsTheRequest) throw new ArgumentException($"'{member}' is not a member of the request itself; a request sets its own members, as r => r.Name.", nameof(member));
        return member.Body.ToString().Substring(member.Parameters[0].Name!.Length + 1);
    }
}

internal sealed class SentMember
{
    internal SentMember(string name, Plan.Expression value)
    {
        Name = name;
        Value = value;
    }

    internal string Name { get; }

    internal Plan.Expression Value { get; }
}

public sealed class SentRequest<TModel>
{
    private readonly PlanBuild _build;
    private readonly RefExpression _response;
    private bool _routed;

    internal SentRequest(PlanBuild build, RefExpression response)
    {
        _build = build;
        _response = response;
    }

    public void Response(Action<Routes<TModel>> routes)
    {
        if (_routed) throw new InvalidOperationException("A request takes one Response: write each route inside it.");
        if (!_build.Follows(_response))
        {
            throw new InvalidOperationException(
                "Response routes the request it follows: write it in the same sentence, before any other step, as pipeline.Get<TRequest>(url, request => ..).Response(response => ..).");
        }

        _routed = true;
        var declared = new Routes<TModel>(_build, _response);
        routes(declared);
        declared.Write();
    }

    internal static SentRequest<TModel> Get<TRequest>(PlanBuild build, string url, Action<RequestMembers<TRequest>> members)
    {
        var (path, search) = RequestUrls.Split(RequestUrls.Checked(url));
        var set = new RequestMembers<TRequest>(build);
        members(set);
        var pieces = RequestUrls.Pieces(path, nameof(url));
        var placeholders = pieces.Where(piece => piece.Placeholder).ToList();
        foreach (var placeholder in placeholders)
        {
            var filled = set.Members.Any(member => string.Equals(member.Name, placeholder.Text, StringComparison.OrdinalIgnoreCase));
            if (!filled)
            {
                throw new ArgumentException(
                    $"The URL '{url}' has the placeholder {placeholder.Text}, and the request has no member named {placeholder.Text}; add the member, or take the placeholder out of the URL.", nameof(url));
            }
        }

        var parts = pieces.Select(piece => piece.Placeholder ? ValuePart(piece.Text, set.Members, build) : TextPart(piece.Text, build)).ToList<Plan.Expression>();
        var query = set.Members
            .Where(member => !placeholders.Any(placeholder => string.Equals(placeholder.Text, member.Name, StringComparison.OrdinalIgnoreCase)))
            .Select(member => Record(
                (nameof(QueryField.Name), Literal.Of(member.Name, build)),
                (nameof(QueryField.Value), member.Value),
                (nameof(QueryField.WhenNothing), Literal.Of(WhenNothing.Empty.Text, build))))
            .ToList<Plan.Expression>();
        var send = Record(
            (nameof(HttpSend.Method), Literal.Of(HttpMethod.Get.Text, build)),
            (nameof(HttpSend.Path), new ListExpression(parts)),
            (nameof(HttpSend.Search), Literal.Of(search, build)),
            (nameof(HttpSend.Query), new ListExpression(query)),
            (nameof(HttpSend.Success), Literal.Of(BodyKind.Json.Text, build)));
        var response = build.AwaitedResult(new CallExpression(new ObjectExpression(build.Http()), [PathStep.Member("send")], ReadPresence.Required, [send]));
        return new SentRequest<TModel>(build, response);
    }

    private static RecordExpression ValuePart(string placeholder, IReadOnlyList<SentMember> members, PlanBuild build)
    {
        var member = members.First(each => string.Equals(each.Name, placeholder, StringComparison.OrdinalIgnoreCase));
        return Kind(PathPart.ValueKind, (nameof(Plan.ValuePart.Name), Literal.Of(member.Name, build)), (nameof(Plan.ValuePart.Value), member.Value));
    }

    private static RecordExpression TextPart(string text, PlanBuild build)
        => Kind(PathPart.TextKind, (nameof(Plan.TextPart.Text), Literal.Of(text, build)));

    private static RecordExpression Kind(string kind, params (string Member, Plan.Expression Value)[] fields)
        => new([new RecordField(PathPart.Discriminator, new LiteralExpression(JsonSerializer.SerializeToElement(kind))), .. Fields(fields)]);

    private static RecordExpression Record(params (string Member, Plan.Expression Value)[] fields) => new(Fields(fields));

    private static List<RecordField> Fields((string Member, Plan.Expression Value)[] fields)
        => [.. fields.Select(field => new RecordField(PlanJson.Name(field.Member), field.Value))];
}

public sealed class Routes<TModel>
{
    private readonly PlanBuild _build;
    private readonly RefExpression _response;
    private readonly List<(int Status, Action Steps)> _errors = [];
    private Action? _success;
    private Action? _otherwise;

    internal Routes(PlanBuild build, RefExpression response)
    {
        _build = build;
        _response = response;
    }

    public Routes<TModel> OnSuccess<TBody>(Action<ResponseBody<TBody>, Pipeline<TModel>> success)
    {
        if (_success is not null) throw new InvalidOperationException("A request has one success route.");
        _success = () => success(Body<TBody>(), new Pipeline<TModel>(_build));
        return this;
    }

    public Routes<TModel> OnError<TBody>(int status, Action<ResponseBody<TBody>, Pipeline<TModel>> error)
    {
        var notAnError = status is < 300 or > 599;
        if (notAnError) throw new ArgumentOutOfRangeException(nameof(status), $"An error route is for a status from 300 to 599, not {status}.");
        if (_errors.Any(each => each.Status == status)) throw new InvalidOperationException($"A request has one route for the status {status}.");
        _errors.Add((status, () => error(Body<TBody>(), new Pipeline<TModel>(_build))));
        return this;
    }

    public Routes<TModel> OnError(Action<Pipeline<TModel>> otherwise)
    {
        if (_otherwise is not null) throw new InvalidOperationException("A request has one route for every other outcome.");
        _otherwise = () => otherwise(new Pipeline<TModel>(_build));
        return this;
    }

    internal void Write()
    {
        var arms = new List<(Plan.Expression When, Action Steps)>();
        if (_success is { } success) arms.Add((Outcome(HttpOutcome.Success), success));
        foreach (var (status, steps) in _errors)
        {
            arms.Add((new OperatorExpression(Operator.And, [Outcome(HttpOutcome.Status), new OperatorExpression(Operator.Eq, [Member(nameof(HttpResponse.Status)), Literal.Of(status, _build)])]), steps));
        }

        var successIsRouted = _success is not null;
        if (!successIsRouted && _otherwise is { } unsucceeded) arms.Add((Outcome(Operator.Neq, HttpOutcome.Success), unsucceeded));
        if (arms.Count == 0) return;
        var branching = _build.When(arms[0].When, arms[0].Steps);
        foreach (var (when, steps) in arms.Skip(1)) _build.ElseIf(branching, when, steps);
        if (successIsRouted && _otherwise is { } otherwise) _build.Else(branching, otherwise);
    }

    private ResponseBody<TBody> Body<TBody>() => new(Member(nameof(HttpResponse.Body)), _build);

    private OperatorExpression Outcome(HttpOutcome outcome) => Outcome(Operator.Eq, outcome);

    private OperatorExpression Outcome(Operator compared, HttpOutcome outcome)
        => new(compared, [Member(nameof(HttpResponse.Outcome)), Literal.Of(outcome.Text, _build)]);

    private GetExpression Member(string name) => new(_response, [PathStep.Member(PlanJson.Name(name))], ReadPresence.Required);
}

public sealed class ResponseBody<TBody> : TypedSource<TBody>
{
    private readonly Plan.Expression _read;

    internal ResponseBody(Plan.Expression read, PlanBuild build)
    {
        _read = read;
        Build = build;
    }

    internal override Plan.Expression Expression => Build.InScope(_read);

    internal override PlanBuild Build { get; }

    public TypedSource<TRead> Read<TRead>(Expression<Func<TBody, TRead>> path)
        => DataPast(path, nameof(path));
}
