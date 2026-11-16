using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Alis.Reactive.Execution;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.OwnershipAndState;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using Alis.Reactive.ValuesAndSources;
using Expression = Alis.Reactive.Schema.Expression;

namespace Alis.Reactive.RequestsAndResponses;

internal sealed class RequestPartsWriter
{
    private const string TokenHeader = "RequestVerificationToken";
    private const string TokenField = "__RequestVerificationToken";

    private readonly BlockWriter _writer;
    private readonly RequestUrlParts _url;
    private readonly List<Expression> _query = [];
    private readonly List<Statement> _captures = [];
    private readonly List<Expression> _headers = [];
    private readonly Dictionary<string, Expression> _path = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _members = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _headerNames = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<Func<(IReadOnlyList<string> Names, JsonLeaf Leaf)>> _pathJson = [];
    private readonly List<Expression> _pathFields = [];
    private readonly List<(IReadOnlyList<string> Names, JsonLeaf Leaf)> _json = [];
    private readonly HashSet<string> _jsonMembers = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<Expression> _fields = [];
    private Body _body = Body.None;
    private Expression _whole = Nothing;
    private bool _closed;
    private int _stage;

    internal RequestPartsWriter(BlockWriter writer, string url)
    {
        _writer = writer;
        _url = new RequestUrlParts(url);
    }

    private enum Body
    {
        None,
        Json,
        UrlEncoded,
        Multipart,
        Text,
        File,
    }

    private static RecordExpression Nothing => new ContractRecord<NothingValueRead>().Set(read => read.Kind, "nothing").Written;

    internal int Literal<TValue>(int stage, LambdaExpression member, TValue value, bool path)
    {
        Open(stage);
        var sent = RequestValues.Literal(value, _writer, member.ToString());
        Add(member, (sent.Value, sent.Form), path);
        if (path) _pathJson.Add(() => (JsonNames(member), JsonBodyValues.Literal(value, _writer, member)));
        return _stage;
    }

    internal int Source<TValue>(int stage, LambdaExpression member, ITypedSource<TValue> value, bool path)
    {
        Open(stage);
        var sent = RequestValues.Source(value, _writer, member.ToString(), _captures.Add);
        Add(member, (sent.Value, sent.Form), path);
        if (path) _pathJson.Add(() => (JsonNames(member), JsonBodyValues.Captured<TValue>(sent.Captured, Sources.FormOf(value), _writer, member)));
        return _stage;
    }

    internal int Json(int stage, Action<int> members) => Opened(stage, Body.Json, members);

    internal int UrlEncoded(int stage, Action<int> members) => Opened(stage, Body.UrlEncoded, members);

    internal int Multipart(int stage, Action<int> members) => Opened(stage, Body.Multipart, members);

    internal int Text(int stage, string text)
    {
        Open(stage);
        _body = Body.Text;
        _whole = RequestValues.Held(ContractRecord<TextSentBody>.Literal(text ?? throw new ArgumentNullException(nameof(text))));
        return _stage;
    }

    internal int Text(int stage, ITypedSource<string?> text)
    {
        Open(stage);
        _body = Body.Text;
        _whole = RequestValues.Read(RequestValues.Captured(Sources.Of(text), _writer, _captures.Add));
        return _stage;
    }

    internal int File(int stage, BrowserFile file)
    {
        Open(stage);
        _body = Body.File;
        _whole = RequestValues.Read(RequestValues.Captured(file.Bound.Target, _writer, _captures.Add));
        return _stage;
    }

    internal int BodyLiteral<TValue>(int stage, LambdaExpression member, TValue value)
    {
        Open(stage);
        if (_body == Body.Json) AddJson(member, JsonBodyValues.Literal(value, _writer, member));
        else AddField(member, RequestValues.Literal(value, _writer, member.ToString()));
        return _stage;
    }

    internal int BodySource<TValue>(int stage, LambdaExpression member, ITypedSource<TValue> value)
    {
        Open(stage);
        if (_body == Body.Json)
        {
            AddJson(member, JsonBodyValues.Source(value, _writer, member, _captures.Add));
            return _stage;
        }

        var sent = RequestValues.Source(value, _writer, member.ToString(), _captures.Add);
        AddField(member, (sent.Value, sent.Form));
        return _stage;
    }

    internal bool FillsAPlaceholder(LambdaExpression member)
    {
        var named = new RequestMember(member);
        return named.TopLevel && _url.Has(named.Name);
    }

    internal int PlacedLiteral<TValue>(int stage, LambdaExpression member, TValue value)
        => FillsAPlaceholder(member) ? Literal(stage, member, value, true) : BodyLiteral(stage, member, value);

    internal int PlacedSource<TValue>(int stage, LambdaExpression member, ITypedSource<TValue> value)
        => FillsAPlaceholder(member) ? Source(stage, member, value, true) : BodySource(stage, member, value);

    internal int FilePart(int stage, LambdaExpression member, BrowserFile file)
    {
        Open(stage);
        var captured = RequestValues.Captured(file.Bound.Target, _writer, _captures.Add);
        AddFile(member, RequestValues.ReadUnlessNull(captured, new ListExpression([captured])));
        return _stage;
    }

    internal int FileParts(int stage, LambdaExpression member, Expression files)
    {
        Open(stage);
        var captured = RequestValues.Captured(files, _writer, _captures.Add);
        AddFile(member, RequestValues.Read(captured, new OpExpression(Operator.ToArray, [captured])));
        return _stage;
    }

    internal int Header(int stage, string name, string value)
    {
        Open(stage);
        HeaderName(name);
        HeaderNames.Value(name, value);
        _headers.Add(HeaderRecord(name, RequestValues.Held(ContractRecord<SentHeader>.Literal(value)), new TextScalarWireForm()));
        return _stage;
    }

    internal int Header<TValue>(int stage, string name, ITypedSource<TValue> value)
    {
        Open(stage);
        HeaderName(name);
        if (!Sources.FormOf(value).WritesAsAScalar<TValue>())
            throw new InvalidOperationException($"Header '{name}' holds a list or an object; send a scalar source, or send the member in the query or body.");
        var sent = RequestValues.Source(value, _writer, name, _captures.Add);
        _headers.Add(HeaderRecord(name, sent.Value, (ScalarWireForm)sent.Form));
        return _stage;
    }

    internal bool SendsInJson(string member)
        => _body == Body.Json && (_jsonMembers.Any(name => member.Equals(name, StringComparison.OrdinalIgnoreCase) || member.StartsWith(name + ".", StringComparison.OrdinalIgnoreCase)) ||
            _path.ContainsKey(member));

    internal bool SendsAnyInJson => _body == Body.Json && (_jsonMembers.Count > 0 || _path.Count > 0);

    internal void Complete()
    {
        foreach (var part in _url.Parts)
        {
            if (part.Member.Length > 0 && !_path.ContainsKey(part.Member))
                throw new InvalidOperationException($"The URL '{_url.Url}' has the placeholder {part.Member}, and the request has no member named {part.Member}; add the member, or take the placeholder out of the URL.");
        }

        foreach (var capture in _captures) _writer.Write(capture);
        _closed = true;
    }

    internal RecordExpression Send(RequestMethod method, SuccessBody success, bool respondedJson)
    {
        var token = Token(method);
        var url = new ContractRecord<RequestUrl>()
            .Set(sent => sent.Parts, new ListExpression(_url.Parts.Select(part => part.Member.Length > 0
                ? _path[part.Member]
                : new ContractRecord<TextPathPart>().Set(text => text.Kind, "text").Set(text => text.Text, part.Text).Written).ToArray()))
            .Set(sent => sent.Query, _url.Query).Written;
        var accept = success == SuccessBody.Json ? "application/json" : success == SuccessBody.PlainText ? "text/*" : success == SuccessBody.Html ? "text/html" : "*/*";
        return new ContractRecord<HttpSend>()
            .Set(sent => sent.Method, method)
            .Set(sent => sent.Url, url)
            .Set(sent => sent.Query, new ContractRecord<SentQuery>().Set(query => query.Fields, new ListExpression(_query.ToArray())).Set(query => query.Inputs, NoInputs).Written)
            .Set(sent => sent.Body, SentBody(token.InField))
            .Set(sent => sent.Headers, new ListExpression([.. _headers, HeaderRecord("Accept", RequestValues.Held(ContractRecord<SentHeader>.Literal(accept)), new TextScalarWireForm())]))
            .Set(sent => sent.Antiforgery, token.Record)
            .Set(sent => sent.Retry, new ContractRecord<NoneRetryPolicy>().Set(retry => retry.Kind, "none").Written)
            .Set(sent => sent.Busy, new ContractRecord<NoneBusy>().Set(busy => busy.Kind, "none").Written)
            .Set(sent => sent.RespondedJson, respondedJson)
            .Set(sent => sent.Contracts, new ListExpression([])).Written;
    }

    private static RecordExpression NoInputs => new ContractRecord<SentInputs>()
        .Set(sent => sent.Container, new ContractRecord<NoneContainerCapture>().Set(container => container.Kind, "none").Written)
        .Set(sent => sent.Individual, new ListExpression([])).Written;

    private (RecordExpression Record, bool InField) Token(RequestMethod method)
    {
        var none = (new ContractRecord<NoneAntiforgery>().Set(token => token.Kind, "none").Written, false);
        var unsafeMethod = method == RequestMethod.Post || method == RequestMethod.Put || method == RequestMethod.Delete;
        if (!unsafeMethod) return none;
        var page = _writer.Content.Antiforgery;
        var request = method.Text + " " + _url.Url;
        if (page.Choice == AntiforgeryChoice.NotChosen)
        {
            throw new InvalidOperationException(
                $"'{request}' sends no antiforgery token, since the app chose none: call Antiforgery(AntiforgeryIn.Header), Antiforgery(AntiforgeryIn.FormField) or NoAntiforgery() in AddReactiveRequests.");
        }

        var sendsToken = page.Choice is AntiforgeryChoice.Header or AntiforgeryChoice.Field && _url.OnOrigin(page.Origin);
        if (!sendsToken) return none;
        if (page.Choice == AntiforgeryChoice.Header) return (TokenRecord(page.Token, new ContractRecord<HeaderTokenPlace>().Set(place => place.Kind, "header").Set(place => place.Name, TokenHeader).Written), false);
        if (_body is Body.Json or Body.Text or Body.File)
        {
            throw new InvalidOperationException(
                $"'{request}' sends a {BodyName} body, and the app sends the antiforgery token in the form field {TokenField}, which no host reads in that body; " +
                "send a URL-encoded or multipart body, or call Antiforgery(AntiforgeryIn.Header) in AddReactiveRequests.");
        }

        return (TokenRecord(page.Token, new ContractRecord<FieldTokenPlace>().Set(place => place.Kind, "field").Set(place => place.Name, TokenField).Written), true);
    }

    private string BodyName => _body == Body.Json ? "JSON" : _body == Body.Text ? "text" : "file";

    private static RecordExpression TokenRecord(string value, RecordExpression place)
        => new ContractRecord<TokenAntiforgery>().Set(token => token.Kind, "token").Set(token => token.Value, value).Set(token => token.Place, place).Written;

    private RecordExpression SentBody(bool tokenInField)
    {
        if (_body == Body.None && !tokenInField) return new ContractRecord<NoneSentBody>().Set(body => body.Kind, "none").Written;
        if (_body == Body.Json)
        {
            return new ContractRecord<JsonSentBody>().Set(body => body.Kind, "json")
                .Set(body => body.Value, JsonBodyValues.Object([.. _pathJson.Select(copy => copy()), .. _json])).Set(body => body.Inputs, NoInputs).Written;
        }

        if (_body == Body.Multipart)
        {
            return new ContractRecord<MultipartSentBody>().Set(body => body.Kind, "multipart")
                .Set(body => body.Parts, new ListExpression([.. _pathFields.Select(FieldPart), .. _fields])).Set(body => body.Inputs, NoInputs).Written;
        }

        if (_body == Body.Text) return new ContractRecord<TextSentBody>().Set(body => body.Kind, "text").Set(body => body.Value, _whole).Set(body => body.Inputs, NoInputs).Written;
        if (_body == Body.File) return new ContractRecord<FileSentBody>().Set(body => body.Kind, "file").Set(body => body.Value, _whole).Set(body => body.Inputs, NoInputs).Written;
        var fields = _body == Body.UrlEncoded ? [.. _pathFields, .. _fields] : new List<Expression>();
        return new ContractRecord<UrlEncodedSentBody>().Set(body => body.Kind, "urlEncoded").Set(body => body.Fields, new ListExpression(fields)).Set(body => body.Inputs, NoInputs).Written;
    }

    private int Opened(int stage, Body body, Action<int> members)
    {
        Open(stage);
        _body = body;
        members(_stage);
        return _stage;
    }

    private void Add(LambdaExpression member, (Expression Value, WireForm Form) sent, bool path)
    {
        var named = Named(member);
        if (path)
        {
            if (!named.TopLevel || !_url.Has(named.Name))
                throw new InvalidOperationException($"Path member '{member}' names no top-level placeholder; use Query for this member, or name a top-level member that fills the URL.");
            if (member.ReturnType == typeof(DateTime) || member.ReturnType == typeof(DateTime?) || member.ReturnType == typeof(DateTimeOffset) || member.ReturnType == typeof(DateTimeOffset?))
                throw new InvalidOperationException($"'{member}' is a date, and a date cannot fill the placeholder {named.Name}: IIS refuses a time in a path segment. Name the member so it goes in the query.");
            if (sent.Form is not ScalarWireForm)
                throw new InvalidOperationException($"Path member '{member}' holds a list or an object; send it in the query and fill the placeholder with a scalar member.");
            _path.Add(named.Name, new ContractRecord<MemberPathPart>().Set(part => part.Kind, "member")
                .Set(part => part.Name, named.Name).Set(part => part.Value, sent.Value).Set(part => part.Form, WireRecords.Of(sent.Form)).Written);
            _pathFields.Add(FieldRecord(member, named, sent));
            return;
        }

        _query.Add(FieldRecord(member, named, sent));
    }

    private void AddField(LambdaExpression member, (Expression Value, WireForm Form) sent)
    {
        var field = FieldRecord(member, Named(member), sent);
        _fields.Add(_body == Body.Multipart ? FieldPart(field) : field);
    }

    private void AddJson(LambdaExpression member, JsonLeaf leaf)
    {
        _jsonMembers.Add(Named(member).Name);
        _json.Add((JsonNames(member), leaf));
    }

    private void AddFile(LambdaExpression member, Expression value)
    {
        var named = Named(member);
        _fields.Add(new ContractRecord<FileMultipartPart>().Set(part => part.Kind, "file").Set(part => part.Name, named.Name).Set(part => part.Value, value).Written);
    }

    private RequestMember Named(LambdaExpression member)
    {
        var named = new RequestMember(member);
        if (_members.Any(name => name.Equals(named.Name, StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith(named.Name + ".", StringComparison.OrdinalIgnoreCase) || named.Name.StartsWith(name + ".", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Request member '{named.Name}' overlaps a member already supplied; send each member once, and send either an object or its members.");
        }

        _members.Add(named.Name);
        return named;
    }

    private IReadOnlyList<string> JsonNames(LambdaExpression member)
        => MemberPath.TryOf(member, _writer.Content.Json.BodyForm, out var path) ? path.BodyNames : throw MemberPath.NoMemberPath(member);

    private static RecordExpression FieldRecord(LambdaExpression member, RequestMember named, (Expression Value, WireForm Form) sent)
        => new ContractRecord<SentField>().Set(field => field.Name, named.Name).Set(field => field.Path, new ListExpression(named.Path.Select(step => ContractRecord<SentField>.Literal(step)).ToArray()))
            .Set(field => field.Value, sent.Value).Set(field => field.Form, WireRecords.Of(sent.Form))
            .Set(field => field.WhenNothing, RequestValues.Absence(member.ReturnType)).Written;

    private static RecordExpression FieldPart(Expression field)
        => new ContractRecord<FieldMultipartPart>().Set(part => part.Kind, "field").Set(part => part.Field, field).Written;

    private void HeaderName(string name)
    {
        HeaderNames.Request(name);
        if (!_headerNames.Add(name))
            throw new InvalidOperationException($"Header '{name}' is supplied twice; send each header name once, ignoring case.");
    }

    private static RecordExpression HeaderRecord(string name, Expression value, ScalarWireForm form)
        => new ContractRecord<SentHeader>().Set(header => header.Name, name).Set(header => header.Value, value).Set(header => header.Form, WireRecords.Of(form)).Written;

    private void Open(int stage)
    {
        if (_closed)
            throw new InvalidOperationException("The request's parts are already written; write every part in its request callback.");
        if (stage != _stage) throw new InvalidOperationException("A request's parts stage is already used; write its parts in one chain.");
        _stage++;
    }
}
