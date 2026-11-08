using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using Alis.Reactive.Plan;
using Alis.Reactive.Types;
using Alis.Reactive.Values;
using Alis.Reactive.Writing;

namespace Alis.Reactive.Pipelines;

public sealed class Request
{
    private readonly PlanBuild _build;
    private readonly RequestStatement _request;
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
        _request = build.Request(argument);
    }

    public void Into(string elementId)
    {
        Slots.RequireAnId(elementId);
        if (_routed) throw new InvalidOperationException("A request loads one slot: write one Into.");
        if (!_build.Follows(_request))
        {
            throw new InvalidOperationException(
                "Into is the route of the request it follows: write it in the same sentence, before any other step, as pipeline.Get(url).Into(id).");
        }

        _routed = true;
        var slot = _build.Slot(elementId);
        _request.Success = new Route(_build.Route(_request, () =>
            _build.Write(new DoStatement(new CallExpression(new ObjectExpression(slot), [PathStep.Member("load")], ReadPresence.Required, [Replies.Body(_build, _request)]), awaits: false))));
    }
}

internal static class Replies
{
    internal static GetExpression Body(PlanBuild build, RequestStatement request)
        => new(build.Reply(request), [PathStep.Member(PlanJson.Name(nameof(HttpResponse.Body)))], ReadPresence.Required);
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

    public RequestMembers<TRequest> Set<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value)
        => Add(member, WireForms.Of<TValue>(_build.Values, member.ToString()), Sources.Of(value).Expression);

    public RequestMembers<TRequest> Set<TValue>(Expression<Func<TRequest, TValue>> member, TValue value)
        => Add(member, WireForms.Of<TValue>(_build.Values, member.ToString()), LiteralOf(value));

    public RequestMembers<TRequest> Set<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => Add(member, WireForms.Of<TValue?>(_build.Values, member.ToString()), Sources.Of(value).Expression);

    private LiteralExpression LiteralOf<TValue>(TValue value)
        => value is Enum named ? Literal.Of(named.ToString(), _build) : Literal.Of(value, _build);

    private RequestMembers<TRequest> Add(LambdaExpression member, SentForm form, Plan.Expression value)
    {
        var name = TopLevelName(member);
        if (_members.Any(each => string.Equals(each.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException($"The request sets '{name}' twice; set each member once.", nameof(member));
        }

        _members.Add(new SentMember(name, value, form));
        return this;
    }

    private static string TopLevelName(LambdaExpression member)
    {
        var readsTheRequest = member.Body is MemberExpression { Expression: ParameterExpression };
        if (!readsTheRequest) throw new ArgumentException($"'{member}' is not a member of the request itself; a request sets its own members, as r => r.Name.", nameof(member));
        return member.Body.ToString().Substring(member.Parameters[0].Name!.Length + 1);
    }
}

internal sealed class SentMember
{
    internal SentMember(string name, Plan.Expression value, SentForm form)
    {
        Name = name;
        Value = value;
        Form = form;
    }

    internal string Name { get; }

    internal Plan.Expression Value { get; }

    internal SentForm Form { get; }
}

public sealed class SentRequest<TModel>
{
    private readonly PlanBuild _build;
    private readonly RequestStatement _request;
    private bool _routed;

    internal SentRequest(PlanBuild build, RequestStatement request)
    {
        _build = build;
        _request = request;
    }

    public void Response(Action<Routes<TModel>> routes)
    {
        if (_routed) throw new InvalidOperationException("A request takes one Response: write each route inside it.");
        if (!_build.Follows(_request))
        {
            throw new InvalidOperationException(
                "Response routes the request it follows: write it in the same sentence, before any other step, as pipeline.Get<TRequest>(url, request => ..).Response(response => ..).");
        }

        _routed = true;
        var declared = new Routes<TModel>(_build, _request);
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

        foreach (var member in set.Members.Where(member => placeholders.Any(placeholder => string.Equals(placeholder.Text, member.Name, StringComparison.OrdinalIgnoreCase))))
        {
            if (!WireForms.FillsAPathSegment(member.Form.Form)) throw HeldOutOfThePath(url, member);
        }

        var parts = pieces.Select(piece => piece.Placeholder ? ValuePart(piece.Text, set.Members, build) : TextPart(piece.Text, build)).ToList<Plan.Expression>();
        var query = set.Members
            .Where(member => !placeholders.Any(placeholder => string.Equals(placeholder.Text, member.Name, StringComparison.OrdinalIgnoreCase)))
            .Select(member => Record(
                (nameof(QueryField.Name), Literal.Of(member.Name, build)),
                (nameof(QueryField.Value), member.Value),
                (nameof(QueryField.Form), FormOf(member.Form.Form)),
                (nameof(QueryField.WhenNothing), Literal.Of(member.Form.WhenNothing.Text, build))))
            .ToList<Plan.Expression>();
        var send = Record(
            (nameof(HttpSend.Method), Literal.Of(HttpMethod.Get.Text, build)),
            (nameof(HttpSend.Path), new ListExpression(parts)),
            (nameof(HttpSend.Search), Literal.Of(search, build)),
            (nameof(HttpSend.Query), new ListExpression(query)),
            (nameof(HttpSend.Success), Literal.Of(BodyKind.Json.Text, build)));
        return new SentRequest<TModel>(build, build.Request(send));
    }

    private static RecordExpression ValuePart(string placeholder, IReadOnlyList<SentMember> members, PlanBuild build)
    {
        var member = members.First(each => string.Equals(each.Name, placeholder, StringComparison.OrdinalIgnoreCase));
        return Kind(
            PathPart.ValueKind,
            (nameof(Plan.ValuePart.Name), Literal.Of(member.Name, build)),
            (nameof(Plan.ValuePart.Value), member.Value),
            (nameof(Plan.ValuePart.Form), FormOf(member.Form.Form)));
    }

    private static LiteralExpression FormOf(WireForm form) => new(JsonSerializer.SerializeToElement(form, PlanJson.Options));

    private static ArgumentException HeldOutOfThePath(string url, SentMember member)
    {
        var why = member.Form.Form is ListForm
            ? "which one path segment cannot hold; take it out of the URL, and the request sends each item in the query"
            : "which a URL's path does not carry, since IIS on .NET Framework refuses a time in a path segment; take it out of the URL, and the request sends it in the query";
        return new ArgumentException($"The URL '{url}' fills its placeholder {member.Name} with {WireForms.Noun(member.Form.Form)}, {why}.", nameof(url));
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
    private readonly RequestStatement _request;
    private readonly List<(int Status, Action Steps)> _errors = [];
    private Action? _success;
    private Action? _otherwise;

    internal Routes(PlanBuild build, RequestStatement request)
    {
        _build = build;
        _request = request;
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
        if (_success is { } success) _request.Success = new Route(_build.Route(_request, success));
        _request.Statuses = [.. _errors.Select(error => new StatusRoute(error.Status, _build.Route(_request, error.Steps)))];
        if (_otherwise is { } otherwise) _request.Otherwise = new Route(_build.Route(_request, otherwise));
    }

    private ResponseBody<TBody> Body<TBody>() => new(_request, _build);
}

public sealed class ResponseBody<TBody> : TypedSource<TBody>
{
    private readonly RequestStatement _request;

    internal ResponseBody(RequestStatement request, PlanBuild build)
    {
        _request = request;
        Build = build;
    }

    internal override Plan.Expression Expression => Replies.Body(Build, _request);

    internal override PlanBuild Build { get; }

    public TypedSource<TRead> Read<TRead>(Expression<Func<TBody, TRead>> path)
        => DataPast(path, nameof(path));
}
