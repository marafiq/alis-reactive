using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Alis.Reactive.Execution;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.OwnershipAndState;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using Alis.Reactive.Validation;
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
    private Expression _container = NoContainer;
    private readonly List<Expression> _individual = [];
    private readonly HashSet<string> _inputMembers = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Action> _setMembers = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<Expression, Func<Expression>> _checkedEntries = [];
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

    internal string? ContainerId { get; private set; }

    internal bool SendsAContainer => ContainerId is not null;

    internal int Sends(int stage, string containerId, string model) => Contains(stage, containerId, ModelPlugin.Sends(_writer.Content, containerId, model));

    internal int Submits(int stage, string formId, string model) => Contains(stage, formId, ModelPlugin.Submits(_writer.Content, formId, model));

    internal int Input(int stage, LambdaExpression member, string inputId, Func<int, int> placed)
    {
        var name = new RequestMember(member).Name;
        if (_inputMembers.Contains(name))
            throw new InvalidOperationException($"Request member '{name}' overlaps a member already supplied; send each member once, and send either an object or its members.");
        if (_setMembers.TryGetValue(name, out var dropped))
        {
            dropped();
            _setMembers.Remove(name);
            _members.Remove(name);
        }

        var stageAfter = placed(stage);
        _inputMembers.Add(name);
        _individual.Add(ModelPlugin.Input(_writer.Content, inputId, name));
        return stageAfter;
    }

    internal int Literal<TValue>(int stage, LambdaExpression member, TValue value, bool path)
    {
        if (SentByAnInput(member)) return Skipped(stage);
        Open(stage);
        var sent = RequestValues.Literal(value, _writer, member.ToString());
        Add(member, (sent.Value, sent.Form), path);
        if (path) _pathJson.Add(() => (JsonNames(member), JsonBodyValues.Literal(value, _writer, member)));
        return _stage;
    }

    internal int Source<TValue>(int stage, LambdaExpression member, ITypedSource<TValue> value, bool path)
    {
        if (SentByAnInput(member)) return Skipped(stage);
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
        if (SentByAnInput(member)) return Skipped(stage);
        Open(stage);
        if (_body == Body.Json) AddJson(member, JsonBodyValues.Literal(value, _writer, member));
        else AddField(member, RequestValues.Literal(value, _writer, member.ToString()));
        return _stage;
    }

    internal int BodySource<TValue>(int stage, LambdaExpression member, ITypedSource<TValue> value)
    {
        if (SentByAnInput(member)) return Skipped(stage);
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
        _headers.Add(LiteralHeader(name, value));
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

    internal RequestCheckParts CheckParts(RefExpression argument, RequestMethod method, (RefExpression Body, RefExpression Failures) names)
    {
        var sendsABody = _body != Body.None;
        var inputs = new GetExpression([new MemberStep(sendsABody ? "body" : "query"), new MemberStep("inputs")], PathMembers.Required, argument);
        var sentValue = _body == Body.Json ? new GetExpression([new MemberStep("body"), new MemberStep("value")], PathMembers.Required, argument) : (Expression)Literals.Text(null);
        var sentFields = new List<Expression>([.. _pathFields, .. sendsABody ? _fields : _query]);
        var entries = new ListExpression(sentFields.Where(_checkedEntries.ContainsKey).Select(field => _checkedEntries[field]()).ToArray());
        return new RequestCheckParts("'" + method.Text + " " + _url.Url + "'", CheckedFormat(method), inputs, sentValue, entries, ContainerId, names);
    }

    internal RecordExpression Send(RequestMethod method, SuccessBody success, bool respondedJson, SendDefaults defaults)
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
            .Set(sent => sent.Query, new ContractRecord<SentQuery>().Set(query => query.Fields, new ListExpression(_query.ToArray())).Set(query => query.Inputs, _body == Body.None ? Inputs : NoInputs).Written)
            .Set(sent => sent.Body, SentBody(token.InField))
            .Set(sent => sent.Headers, new ListExpression([.. DefaultHeaders(defaults), .. _headers, LiteralHeader("Accept", accept)]))
            .Set(sent => sent.Antiforgery, token.Record)
            .Set(sent => sent.Retry, defaults.Retry)
            .Set(sent => sent.Busy, defaults.Busy)
            .Set(sent => sent.RespondedJson, respondedJson)
            .Set(sent => sent.Contracts, defaults.Contracts).Written;
    }

    private static RecordExpression NoContainer => new ContractRecord<NoneContainerCapture>().Set(container => container.Kind, "none").Written;

    private IEnumerable<Expression> DefaultHeaders(SendDefaults defaults)
        => defaults.Headers.Where(header => !_headerNames.Contains(header.Name)).Select(header => LiteralHeader(header.Name, header.Value));

    private static RecordExpression LiteralHeader(string name, string value)
        => HeaderRecord(name, RequestValues.Held(ContractRecord<SentHeader>.Literal(value)), new TextScalarWireForm());

    private static RecordExpression NoInputs => new ContractRecord<SentInputs>()
        .Set(sent => sent.Container, NoContainer)
        .Set(sent => sent.Individual, new ListExpression([])).Written;

    private RecordExpression Inputs => new ContractRecord<SentInputs>()
        .Set(sent => sent.Container, _container)
        .Set(sent => sent.Individual, new ListExpression(_individual.ToArray())).Written;

    private int Contains(int stage, string id, Expression container)
    {
        Open(stage);
        ContainerId = id;
        _container = container;
        return _stage;
    }

    private BodyFormat CheckedFormat(RequestMethod method) => _body switch
    {
        Body.Json => BodyFormat.Json,
        Body.UrlEncoded => BodyFormat.UrlEncoded,
        Body.Multipart => BodyFormat.Multipart,
        Body.Text => BodyFormat.PlainText,
        Body.File => BodyFormat.File,
        _ => method == RequestMethod.Post || method == RequestMethod.Put || method == RequestMethod.Delete ? BodyFormat.None : BodyFormat.UrlEncoded,
    };

    private void Checks(Expression field, LambdaExpression member, Expression value)
        => _checkedEntries[field] = () => new RecordExpression([
            new RecordField("path", new ListExpression(JsonNames(member).Select(name => (Expression)Literals.Text(name)).ToArray())),
            new RecordField("value", value),
        ]);

    private bool SentByAnInput(LambdaExpression member) => _inputMembers.Contains(new RequestMember(member).Name);

    private int Skipped(int stage)
    {
        Open(stage);
        return _stage;
    }

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
                .Set(body => body.Value, JsonBodyValues.Object([.. _pathJson.Select(copy => copy()), .. _json])).Set(body => body.Inputs, Inputs).Written;
        }

        if (_body == Body.Multipart)
        {
            return new ContractRecord<MultipartSentBody>().Set(body => body.Kind, "multipart")
                .Set(body => body.Parts, new ListExpression([.. _pathFields.Select(FieldPart), .. _fields])).Set(body => body.Inputs, Inputs).Written;
        }

        if (_body == Body.Text) return new ContractRecord<TextSentBody>().Set(body => body.Kind, "text").Set(body => body.Value, _whole).Set(body => body.Inputs, Inputs).Written;
        if (_body == Body.File) return new ContractRecord<FileSentBody>().Set(body => body.Kind, "file").Set(body => body.Value, _whole).Set(body => body.Inputs, Inputs).Written;
        var fields = _body == Body.UrlEncoded ? [.. _pathFields, .. _fields] : new List<Expression>();
        return new ContractRecord<UrlEncodedSentBody>().Set(body => body.Kind, "urlEncoded").Set(body => body.Fields, new ListExpression(fields)).Set(body => body.Inputs, Inputs).Written;
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
            var pathField = FieldRecord(member, named, sent);
            _pathFields.Add(pathField);
            Checks(pathField, member, sent.Value);
            var pathJsonAt = _pathJson.Count;
            _setMembers[named.Name] = () =>
            {
                _path.Remove(named.Name);
                _pathFields.Remove(pathField);
                if (pathJsonAt < _pathJson.Count) _pathJson.RemoveAt(pathJsonAt);
            };
            return;
        }

        var queried = FieldRecord(member, named, sent);
        _query.Add(queried);
        Checks(queried, member, sent.Value);
        _setMembers[named.Name] = () => _query.Remove(queried);
    }

    private void AddField(LambdaExpression member, (Expression Value, WireForm Form) sent)
    {
        var named = Named(member);
        var field = FieldRecord(member, named, sent);
        var placed = _body == Body.Multipart ? FieldPart(field) : field;
        _fields.Add(placed);
        Checks(placed, member, sent.Value);
        _setMembers[named.Name] = () => _fields.Remove(placed);
    }

    private void AddJson(LambdaExpression member, JsonLeaf leaf)
    {
        var named = Named(member).Name;
        var entry = (JsonNames(member), leaf);
        _jsonMembers.Add(named);
        _json.Add(entry);
        _setMembers[named] = () =>
        {
            _json.Remove(entry);
            _jsonMembers.Remove(named);
        };
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

internal sealed class SendDefaults(IReadOnlyList<(string Name, string Value)> headers, RecordExpression retry, RecordExpression busy, ListExpression contracts)
{
    internal IReadOnlyList<(string Name, string Value)> Headers { get; } = headers;

    internal RecordExpression Retry { get; } = retry;

    internal RecordExpression Busy { get; } = busy;

    internal ListExpression Contracts { get; } = contracts;
}
