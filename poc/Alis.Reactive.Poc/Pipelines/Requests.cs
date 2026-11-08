using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using Alis.Reactive.Components;
using Alis.Reactive.Plan;
using Alis.Reactive.Requests;
using Alis.Reactive.Types;
using Alis.Reactive.Values;
using Alis.Reactive.Writing;

namespace Alis.Reactive.Pipelines;

internal static class GetSends
{
    internal static Func<BodyKind, Plan.Expression> Of(string url)
    {
        var (path, search) = RequestUrls.Split(url);
        var placeholder = RequestUrls.Pieces(path, nameof(url)).FirstOrDefault(piece => piece.Placeholder);
        if (placeholder is not null)
        {
            throw new ArgumentException(
                $"The URL '{url}' has the placeholder {placeholder.Text}, which a request that sets no members cannot fill; take the placeholder out of the URL.", nameof(url));
        }

        return kind => new LiteralExpression(JsonSerializer.SerializeToElement(
            new HttpSend(HttpMethod.Get, [new TextPart(path)], search, [], [new SentHeader(SentRecords.Accept, JsonSerializer.SerializeToElement(kind.Accept), WireForm.Text)]),
            PlanJson.Options));
    }

    internal static Func<BodyKind, Plan.Expression> Of<TRequest>(PlanBuild build, string url, Action<QueryParts<TRequest>> request)
    {
        var (path, search) = RequestUrls.Split(RequestUrls.Checked(url));
        var parts = new QueryParts<TRequest>(build, url, RequestUrls.Pieces(path, nameof(url)));
        request(parts);
        return kind => parts.Sent(url, search, kind);
    }
}

internal static class Slots
{
    internal static void RequireAnId(string elementId)
    {
        if (string.IsNullOrWhiteSpace(elementId)) throw new ArgumentException("A slot is reached by the id of its element.", nameof(elementId));
    }

    internal static void Load(PlanBuild build, string elementId, ReadOnly<Markup> body)
    {
        var slot = build.Slot(elementId);
        build.Write(new DoStatement(new CallExpression(new ObjectExpression(slot), [PathStep.Member("load")], ReadPresence.Required, [Sources.Of(body).Expression]), awaits: false));
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

public sealed class QueryParts<TRequest>
{
    private readonly string _url;
    private readonly IReadOnlyList<UrlPiece> _pieces;
    private readonly List<SentMember> _path = [];
    private readonly HashSet<string> _placedBySet = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<SentMember> _query = [];
    private readonly List<SentMember> _headers = [];

    internal QueryParts(PlanBuild build, string url, IReadOnlyList<UrlPiece> pieces)
    {
        Build = build;
        _url = url;
        _pieces = pieces;
    }

    internal PlanBuild Build { get; }

    public QueryParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value)
        => InPath(member, Form<TValue>(member), Sources.Of(value).Expression);

    public QueryParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue>> member, TValue value)
        => InPath(member, Form<TValue>(member), LiteralOf(value));

    public QueryParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => InPath(member, Form<TValue?>(member), Sources.Of(value).Expression);

    public QueryParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value)
        => InQuery(member, Form<TValue>(member), Sources.Of(value).Expression);

    public QueryParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue>> member, TValue value)
        => InQuery(member, Form<TValue>(member), LiteralOf(value));

    public QueryParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => InQuery(member, Form<TValue?>(member), Sources.Of(value).Expression);

    public QueryParts<TRequest> Header(string name, string value)
    {
        RequestHeaders.RequireANewName(name, _headers);
        _headers.Add(new SentMember(name, Literal.Of(RequestHeaders.FixedValue(name, value), Build), new SentForm(WireForm.Text, WhenNothing.Leave)));
        return this;
    }

    public QueryParts<TRequest> Header<TValue>(string name, ITypedSource<TValue> value)
    {
        RequestHeaders.RequireANewName(name, _headers);
        var form = WireForms.OneHeaderValue<TValue>(Build.Values, name, nameof(value));
        _headers.Add(new SentMember(name, Sources.Of(value).Expression, new SentForm(form, WhenNothing.Leave)));
        return this;
    }

    internal QueryParts<TRequest> Placed(LambdaExpression member, SentForm form, Plan.Expression value)
    {
        var key = MemberKey(member);
        var fillsAPlaceholder = !key.Contains('.') && PlaceholderNamed(key) is not null;
        if (!fillsAPlaceholder) return InQuery(member, form, value);
        _placedBySet.Add(key);
        return InPath(member, form, value);
    }

    internal SentForm Form<TValue>(LambdaExpression member) => WireForms.Of<TValue>(Build.Values, member.ToString());

    internal LiteralExpression LiteralOf<TValue>(TValue value)
        => value is Enum named ? Literal.Of(named.ToString(), Build) : Literal.Of(value, Build);

    internal RecordExpression Sent(string url, string search, BodyKind success)
    {
        foreach (var placeholder in _pieces.Where(piece => piece.Placeholder))
        {
            if (Named(_path, placeholder.Text) is not null) continue;
            if (Named(_query, placeholder.Text) is { } queried)
            {
                throw new ArgumentException(
                    $"The URL '{url}' has the placeholder {placeholder.Text}, which Query does not fill; fill it with Path(r => r.{queried.Name}, value).", nameof(url));
            }

            throw new ArgumentException(
                $"The URL '{url}' has the placeholder {placeholder.Text}, and the request has no member named {placeholder.Text}; add the member, or take the placeholder out of the URL.", nameof(url));
        }

        foreach (var member in _path.Where(member => !WireForms.FillsAPathSegment(member.Form.Form)))
        {
            throw HeldOutOfThePath(url, member, _placedBySet.Contains(member.Name));
        }

        var path = _pieces.Select(piece => piece.Placeholder ? ValueRecord(Named(_path, piece.Text)!) : SentRecords.TextPart(piece.Text, Build)).ToList<Plan.Expression>();
        return SentRecords.Record(
            (nameof(HttpSend.Method), Literal.Of(HttpMethod.Get.Text, Build)),
            (nameof(HttpSend.Path), new ListExpression(path)),
            (nameof(HttpSend.Search), Literal.Of(search, Build)),
            (nameof(HttpSend.Query), new ListExpression([.. _query.Select(FieldRecord)])),
            (nameof(HttpSend.Headers), new ListExpression([AcceptRecord(success), .. _headers.Select(HeaderRecord)])));
    }

    private QueryParts<TRequest> InPath(LambdaExpression member, SentForm form, Plan.Expression value)
    {
        var key = MemberKey(member);
        if (key.Contains('.'))
        {
            throw new ArgumentException(
                $"'{member}' fills a placeholder, which only a member of the request itself fills, as r => r.Name; send {key} with Query.", nameof(member));
        }

        if (PlaceholderNamed(key) is null) throw new ArgumentException($"The URL '{_url}' has no placeholder {key}; add {{{key}}} to the URL, or send {key} with Query.", nameof(member));
        return Add(_path, new SentMember(key, value, form));
    }

    private QueryParts<TRequest> InQuery(LambdaExpression member, SentForm form, Plan.Expression value)
        => Add(_query, new SentMember(MemberKey(member), value, form));

    private QueryParts<TRequest> Add(List<SentMember> part, SentMember member)
    {
        if (Named(_path, member.Name) is not null || Named(_query, member.Name) is not null)
        {
            throw new ArgumentException($"The request sets '{member.Name}' twice; set each member once.", nameof(member));
        }

        part.Add(member);
        return this;
    }

    private UrlPiece? PlaceholderNamed(string key)
        => _pieces.FirstOrDefault(piece => piece.Placeholder && string.Equals(piece.Text, key, StringComparison.OrdinalIgnoreCase));

    private static SentMember? Named(List<SentMember> part, string key)
        => part.FirstOrDefault(member => string.Equals(member.Name, key, StringComparison.OrdinalIgnoreCase));

    private static string MemberKey(LambdaExpression member)
    {
        var node = member.Body;
        while (node is MemberExpression { Expression: { } holder }) node = holder;
        var readsTheRequest = member.Body is MemberExpression && node == member.Parameters[0];
        if (!readsTheRequest)
        {
            throw new ArgumentException(
                $"'{member}' is not a member path of the request; a request sends its members, as r => r.Name or r => r.NextOfKin.Name.", nameof(member));
        }

        return member.Body.ToString().Substring(member.Parameters[0].Name!.Length + 1);
    }

    private static ArgumentException HeldOutOfThePath(string url, SentMember member, bool placedBySet)
    {
        var why = member.Form.Form is ListForm
            ? "which one path segment cannot hold"
            : "which a URL's path does not carry, since IIS on .NET Framework refuses a time in a path segment";
        var cure = placedBySet
            ? member.Form.Form is ListForm ? "take it out of the URL, and the request sends each item in the query" : "take it out of the URL, and the request sends it in the query"
            : $"take it out of the URL and send {member.Name} with Query";
        return new ArgumentException($"The URL '{url}' fills its placeholder {member.Name} with {WireForms.Noun(member.Form.Form)}, {why}; {cure}.", nameof(url));
    }

    private RecordExpression ValueRecord(SentMember member)
        => SentRecords.Kind(
            PathPart.ValueKind,
            (nameof(Plan.ValuePart.Name), Literal.Of(member.Name, Build)),
            (nameof(Plan.ValuePart.Value), member.Value),
            (nameof(Plan.ValuePart.Form), SentRecords.FormOf(member.Form.Form)));

    private RecordExpression FieldRecord(SentMember member)
        => SentRecords.Record(
            (nameof(SentField.Name), Literal.Of(member.Name, Build)),
            (nameof(SentField.Value), member.Value),
            (nameof(SentField.Form), SentRecords.FormOf(member.Form.Form)),
            (nameof(SentField.WhenNothing), Literal.Of(member.Form.WhenNothing.Text, Build)));

    private RecordExpression AcceptRecord(BodyKind success)
        => SentRecords.Record(
            (nameof(SentHeader.Name), Literal.Of(SentRecords.Accept, Build)),
            (nameof(SentHeader.Value), Literal.Of(success.Accept, Build)),
            (nameof(SentHeader.Form), SentRecords.FormOf(WireForm.Text)));

    private RecordExpression HeaderRecord(SentMember header)
        => SentRecords.Record(
            (nameof(SentHeader.Name), Literal.Of(header.Name, Build)),
            (nameof(SentHeader.Value), header.Value),
            (nameof(SentHeader.Form), SentRecords.FormOf(header.Form.Form)));
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

internal static class SentRecords
{
    internal const string Accept = "Accept";

    internal static RecordExpression TextPart(string text, PlanBuild build)
        => Kind(PathPart.TextKind, (nameof(Plan.TextPart.Text), Literal.Of(text, build)));

    internal static LiteralExpression FormOf(WireForm form) => new(JsonSerializer.SerializeToElement(form, PlanJson.Options));

    internal static RecordExpression Kind(string kind, params (string Member, Plan.Expression Value)[] fields)
        => new([new RecordField(PathPart.Discriminator, new LiteralExpression(JsonSerializer.SerializeToElement(kind))), .. Fields(fields)]);

    internal static RecordExpression Record(params (string Member, Plan.Expression Value)[] fields) => new(Fields(fields));

    private static List<RecordField> Fields((string Member, Plan.Expression Value)[] fields)
        => [.. fields.Select(field => new RecordField(PlanJson.Name(field.Member), field.Value))];
}
