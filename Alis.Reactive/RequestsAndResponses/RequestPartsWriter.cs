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
    private readonly List<SentMember> _sent = [];
    private readonly List<Statement> _captures = [];
    private readonly List<Expression> _headers = [];
    private readonly Dictionary<string, Expression> _path = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _members = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _headerNames = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<Func<(IReadOnlyList<JsonStep> Path, JsonLeaf Leaf)>> _pathJson = [];
    private readonly List<Expression> _pathFields = [];
    private readonly List<(IReadOnlyList<JsonStep> Path, JsonLeaf Leaf)> _json = [];
    private readonly HashSet<string> _jsonMembers = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<Expression> _fields = [];
    private readonly List<(Expression Field, RequestMember Member, bool Guarded, bool LeftWhenItReadsNothing)> _positioned = [];
    private Body _body = Body.None;
    private Expression _whole = Nothing;
    private Expression _container = NoContainer;
    private readonly List<Expression> _individual = [];
    private readonly HashSet<string> _inputMembers = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Action> _setMembers = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<Expression, Func<Expression>> _checkedEntries = [];
    private readonly Stack<RefExpression> _guards = new();
    private readonly Dictionary<IReadOnlyList<JsonStep>, RefExpression> _jsonGuards = [];
    private readonly Dictionary<string, RefExpression> _headerGuards = new(StringComparer.OrdinalIgnoreCase);
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

    internal string Url => _url.Url;

    internal IReadOnlyList<RequestUrlParts.Part> UrlParts => _url.Parts;

    internal string OwnQuery => _url.Query;

    internal IReadOnlyList<SentMember> SentMembers => _sent;

    internal bool SendsAHeader => _headers.Count > 0;

    internal bool SendsAnInput => _individual.Count > 0;

    private Expression When => _guards.Count == 0 ? Literal(true) : _guards.Peek();

    internal int Sends(int stage, string containerId, string model) => Contains(stage, containerId, ModelPlugin.Sends(_writer.Content, containerId, model));

    internal int Submits(int stage, string formId, string model) => Contains(stage, formId, ModelPlugin.Submits(_writer.Content, formId, model));

    internal int Input<TRequest, TMember>(int stage, Expression<Func<TRequest, TMember>> member, string inputId, Func<int, int> placed)
    {
        var name = Member(member).Name;
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
        var captured = ModelPlugin.Input(_writer.Content, inputId, name);
        _individual.Add(_guards.Count == 0 ? captured : new OpExpression(Operator.Cond, [_guards.Peek(), captured, LeftOutInput]));
        return stageAfter;
    }

    internal int Guarded(int stage, Expression condition, Action<int> branch)
    {
        Open(stage);
        var held = new OpExpression(Operator.Cond, [condition, Literal(true), Literal(false)]);
        var guard = _writer.NextBinding("partCondition");
        _captures.Add(new LetStatement(guard.Name, false, LetBinds.Line, _guards.Count == 0 ? held : new OpExpression(Operator.And, [_guards.Peek(), held])));
        _guards.Push(guard);
        branch(_stage);
        _guards.Pop();
        return _stage;
    }

    internal int Literal<TRequest, TValue>(int stage, Expression<Func<TRequest, TValue>> member, TValue value, bool path) => Literal(stage, Member(member), value, path);

    internal int Source<TRequest, TMember, TValue>(int stage, Expression<Func<TRequest, TMember>> member, ITypedSource<TValue> value, bool path)
        => Source(stage, Member(member), value, path);

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
        _whole = RequestValues.Held(new OpExpression(Operator.Coalesce, [RequestValues.Captured(Sources.Of(text), _writer, Capture), Literals.Text("")]));
        return _stage;
    }

    internal int File(int stage, BrowserFile file)
    {
        Open(stage);
        _body = Body.File;
        _whole = RequestValues.Read(RequestValues.Captured(file.Bound.Target, _writer, Capture));
        return _stage;
    }

    internal int BodyLiteral<TRequest, TValue>(int stage, Expression<Func<TRequest, TValue>> member, TValue value) => BodyLiteral(stage, Member(member), value);

    internal int BodySource<TRequest, TMember, TValue>(int stage, Expression<Func<TRequest, TMember>> member, ITypedSource<TValue> value)
        => BodySource(stage, Member(member), value);

    internal bool FillsAPlaceholder<TRequest, TMember>(Expression<Func<TRequest, TMember>> member) => FillsAPlaceholder(Member(member));

    internal int PlacedLiteral<TRequest, TValue>(int stage, Expression<Func<TRequest, TValue>> member, TValue value)
    {
        var named = Member(member);
        return FillsAPlaceholder(named) ? Literal(stage, named, value, true) : BodyLiteral(stage, named, value);
    }

    internal int PlacedSource<TRequest, TMember, TValue>(int stage, Expression<Func<TRequest, TMember>> member, ITypedSource<TValue> value)
    {
        var named = Member(member);
        return FillsAPlaceholder(named) ? Source(stage, named, value, true) : BodySource(stage, named, value);
    }

    internal int FilePart<TRequest, TFile>(int stage, Expression<Func<TRequest, TFile>> member, BrowserFile file)
    {
        Open(stage);
        var captured = RequestValues.Captured(file.Bound.Target, _writer, Capture);
        AddFile(Member(member), RequestValues.ReadUnlessNull(captured, new ListExpression([captured])));
        return _stage;
    }

    internal int FileParts<TRequest, TFiles>(int stage, Expression<Func<TRequest, TFiles>> member, Expression files)
    {
        Open(stage);
        var captured = RequestValues.Captured(files, _writer, Capture);
        AddFile(Member(member), RequestValues.Read(captured, new OpExpression(Operator.ToArray, [captured])));
        return _stage;
    }

    internal int Header(int stage, string name, string value)
    {
        Open(stage);
        HeaderName(name);
        HeaderNames.Value(name, value);
        _headers.Add(HeaderRecord(name, RequestValues.Held(ContractRecord<SentHeader>.Literal(value)), new TextScalarWireForm(), When));
        return _stage;
    }

    internal int Header<TValue>(int stage, string name, ITypedSource<TValue> value)
    {
        Open(stage);
        HeaderName(name);
        if (!Sources.FormOf(value).WritesAsAScalar<TValue>())
            throw new InvalidOperationException($"Header '{name}' holds a list or an object; send a scalar source, or send the member in the query or body.");
        var sent = RequestValues.Source(value, _writer, name, Capture);
        _headers.Add(HeaderRecord(name, sent.Value, (ScalarWireForm)sent.Form, When));
        return _stage;
    }

    internal bool SendsInJson(string member)
        => _body == Body.Json && (_jsonMembers.Any(name => RequestMember.Holds(name, member)) || _path.ContainsKey(member));

    internal bool SendsAnyInJson => _body == Body.Json && (_jsonMembers.Count > 0 || _path.Count > 0);

    internal void Complete()
    {
        foreach (var part in _url.Parts)
        {
            if (part.Member.Length > 0 && !_path.ContainsKey(part.Member))
                throw new InvalidOperationException($"The URL '{_url.Url}' has the placeholder {part.Member}, and the request has no member named {part.Member}; add the member, or take the placeholder out of the URL.");
        }

        RefuseAnItemLeftWithNoField(_query, "query");
        RefuseAnItemLeftWithNoField(_fields, "form data");
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
            .Set(sent => sent.Headers, new ListExpression([.. DefaultHeaders(defaults), .. _headers, LiteralHeader("Accept", accept, Literal(true))]))
            .Set(sent => sent.Antiforgery, token.Record)
            .Set(sent => sent.Retry, defaults.Retry)
            .Set(sent => sent.Busy, defaults.Busy)
            .Set(sent => sent.RespondedJson, respondedJson)
            .Set(sent => sent.Contracts, defaults.Contracts).Written;
    }

    private static RecordExpression NoContainer => new ContractRecord<NoneContainerCapture>().Set(container => container.Kind, "none").Written;

    private IEnumerable<Expression> DefaultHeaders(SendDefaults defaults)
        => defaults.Headers.Where(header => !_headerNames.Contains(header.Name) || _headerGuards.ContainsKey(header.Name))
            .Select(header => LiteralHeader(header.Name, header.Value, _headerGuards.TryGetValue(header.Name, out var guard) ? Unless(guard) : Literal(true)));

    private static RecordExpression LiteralHeader(string name, string value, Expression when)
        => HeaderRecord(name, RequestValues.Held(ContractRecord<SentHeader>.Literal(value)), new TextScalarWireForm(), when);

    private static OpExpression Unless(RefExpression guard) => new(Operator.Cond, [guard, Literal(false), Literal(true)]);

    private static LitExpression Literal(bool value) => ContractRecord<SentField>.Literal(value);

    private static RecordExpression LeftOutInput => new ContractRecord<LeftOutInputCapture>().Set(capture => capture.Kind, "leftOut").Written;

    private static OpExpression ReadsNothing => new(Operator.NullableValue, [ContractRecord<ValueRead>.Literal<string?>(null)]);

    private void Capture(LetStatement capture)
        => _captures.Add(_guards.Count == 0 ? capture : new LetStatement(capture.Name, capture.Await, capture.Binds, new OpExpression(Operator.Cond, [_guards.Peek(), capture.Expr, ReadsNothing])));

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

    private void Checks(Expression field, RequestMember member, Expression value)
        => _checkedEntries[field] = () => new RecordExpression([
            new RecordField("path", new ListExpression(BodySteps(member).Select(step => (Expression)ContractRecord<SentField>.Literal(step.Step)).ToArray())),
            new RecordField("value", value),
        ]);

    private Expression Guarded(Expression read) => _guards.Count == 0 ? read : new OpExpression(Operator.Cond, [_guards.Peek(), read, Nothing]);

    private bool SentByAnInput(RequestMember member) => _inputMembers.Contains(member.Name);

    private int Literal<TValue>(int stage, RequestMember member, TValue value, bool path)
    {
        if (SentByAnInput(member)) return Skipped(stage);
        Open(stage);
        var (held, form) = RequestValues.LiteralHeld(value, _writer, member.Lambda.ToString());
        Add(member, (RequestValues.Held(held), form), path, held, value is null);
        if (path) _pathJson.Add(() => (BodySteps(member), JsonBodyValues.Literal(value, _writer, member.Lambda)));
        return _stage;
    }

    private int Source<TValue>(int stage, RequestMember member, ITypedSource<TValue> value, bool path)
    {
        if (SentByAnInput(member)) return Skipped(stage);
        Open(stage);
        var sent = RequestValues.Source(value, _writer, member.Lambda.ToString(), path ? _captures.Add : Capture);
        Add(member, (sent.Value, sent.Form), path, Sources.Of(value), true);
        if (path) _pathJson.Add(() => (BodySteps(member), JsonBodyValues.Captured<TValue>(sent.Captured, Sources.FormOf(value), _writer, member)));
        return _stage;
    }

    private int BodyLiteral<TValue>(int stage, RequestMember member, TValue value)
    {
        if (SentByAnInput(member)) return Skipped(stage);
        Open(stage);
        if (_body == Body.Json) AddJson(member, JsonBodyValues.Literal(value, _writer, member.Lambda));
        else AddField(member, RequestValues.Literal(value, _writer, member.Lambda.ToString()), value is null);
        return _stage;
    }

    private int BodySource<TValue>(int stage, RequestMember member, ITypedSource<TValue> value)
    {
        if (SentByAnInput(member)) return Skipped(stage);
        Open(stage);
        if (_body == Body.Json)
        {
            AddJson(member, JsonBodyValues.Source(value, _writer, member, Capture));
            return _stage;
        }

        var sent = RequestValues.Source(value, _writer, member.Lambda.ToString(), Capture);
        AddField(member, (sent.Value, sent.Form), true);
        return _stage;
    }

    private bool FillsAPlaceholder(RequestMember member) => member.TopLevel && _url.Has(member.Name);

    private RequestMember Member<TRequest, TMember>(Expression<Func<TRequest, TMember>> member) => RequestMember.Of(member, _writer.Content.Json.App);

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
            IReadOnlyList<(IReadOnlyList<JsonStep> Path, JsonLeaf Leaf)> members = [.. _pathJson.Select(copy => copy()), .. _json];
            return new ContractRecord<JsonSentBody>().Set(body => body.Kind, "json")
                .Set(body => body.Value, JsonBodyValues.Object(members))
                .Set(body => body.Guarded, new ListExpression(GuardedMembers().ToArray()))
                .Set(body => body.Enums, new ListExpression(members.SelectMany(member => member.Leaf.Enums.Select(sent => (Expression)SentEnumRecord(sent.Name, member.Path, sent.Form))).ToArray()))
                .Set(body => body.Inputs, Inputs).Written;
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

    private IEnumerable<Expression> GuardedMembers()
    {
        var members = _json.Select(member => member.Path).ToArray();
        var guarded = members.Where(_jsonGuards.ContainsKey).ToArray();
        var holders = new List<IReadOnlyList<JsonStep>>();
        foreach (var holder in guarded.SelectMany(names => Enumerable.Range(1, names.Count - 1).Select(length => names.Take(length).ToArray())))
        {
            var aListItemKeepsItsPosition = holder[^1].IsIndex;
            if (!aListItemKeepsItsPosition && !holders.Any(known => known.SequenceEqual(holder))) holders.Add(holder);
        }

        foreach (var holder in holders)
        {
            var held = members.Where(names => names.Count > holder.Count && holder.SequenceEqual(names.Take(holder.Count))).ToArray();
            if (!held.All(_jsonGuards.ContainsKey)) continue;
            var whens = held.Select(names => (Expression)_jsonGuards[names]).Distinct().ToArray();
            yield return GuardedRecord(holder, whens.Skip(1).Aggregate(whens[0], (either, next) => new OpExpression(Operator.Or, [either, next])));
        }

        foreach (var names in guarded) yield return GuardedRecord(names, _jsonGuards[names]);
        foreach (var (path, leaf) in _json)
        {
            foreach (var when in leaf.SentWhen) yield return GuardedRecord(path, when);
        }
    }

    private static RecordExpression SentEnumRecord(string name, IReadOnlyList<JsonStep> path, JsonForm form)
        => new ContractRecord<SentEnum>().Set(sent => sent.Name, name).Set(sent => sent.Path, TextPath(path)).Set(sent => sent.Form, WireRecords.Of(form)).Written;

    private static RecordExpression GuardedRecord(IReadOnlyList<JsonStep> path, Expression when)
        => new ContractRecord<GuardedMember>().Set(member => member.Path, TextPath(path)).Set(member => member.When, when).Written;

    private static ListExpression TextPath(IReadOnlyList<JsonStep> path) => new(path.Select(step => (Expression)Literals.Text(step.Name)).ToArray());

    private int Opened(int stage, Body body, Action<int> members)
    {
        Open(stage);
        _body = body;
        members(_stage);
        return _stage;
    }

    private void Add(RequestMember named, (Expression Value, WireForm Form) sent, bool path, Expression read, bool canReadNothing)
    {
        Named(named);
        var member = named.Lambda;
        var sentMember = new SentMember(member.ToString(), named.Name, path, member.ReturnType, sent.Form, read, _guards.Count > 0);
        if (path)
        {
            if (!named.TopLevel || !_url.Has(named.Name))
                throw new InvalidOperationException($"Path member '{member}' names no top-level placeholder; use Query for this member, or name a top-level member that fills the URL.");
            if (_guards.Count > 0)
                throw new InvalidOperationException($"'{member}' fills a placeholder of the URL '{_url.Url}' under a condition on the request's parts, and the URL takes no condition: a request that differs in its URL is a second request under the pipeline's When.");
            if (member.ReturnType == typeof(DateTime) || member.ReturnType == typeof(DateTime?) || member.ReturnType == typeof(DateTimeOffset) || member.ReturnType == typeof(DateTimeOffset?))
                throw new InvalidOperationException($"'{member}' is a date, and a date cannot fill the placeholder {named.Name}: IIS refuses a time in a path segment. Name the member so it goes in the query.");
            if (sent.Form is not ScalarWireForm)
                throw new InvalidOperationException($"Path member '{member}' holds a list or an object; send it in the query and fill the placeholder with a scalar member.");
            _path.Add(named.Name, new ContractRecord<MemberPathPart>().Set(part => part.Kind, "member")
                .Set(part => part.Name, named.Name).Set(part => part.Value, sent.Value).Set(part => part.Form, WireRecords.Of(sent.Form)).Written);
            var pathField = FieldRecord(named, sent, Literal(true));
            _pathFields.Add(pathField);
            Checks(pathField, named, sent.Value);
            var pathJsonAt = _pathJson.Count;
            _sent.Add(sentMember);
            _setMembers[named.Name] = () =>
            {
                _path.Remove(named.Name);
                _pathFields.Remove(pathField);
                if (pathJsonAt < _pathJson.Count) _pathJson.RemoveAt(pathJsonAt);
                _sent.Remove(sentMember);
            };
            return;
        }

        var queried = FieldRecord(named, sent, When);
        _query.Add(queried);
        Positioned(queried, named, canReadNothing);
        Checks(queried, named, Guarded(sent.Value));
        _sent.Add(sentMember);
        _setMembers[named.Name] = () =>
        {
            _query.Remove(queried);
            _sent.Remove(sentMember);
        };
    }

    private void AddField(RequestMember named, (Expression Value, WireForm Form) sent, bool canReadNothing)
    {
        Named(named);
        var field = FieldRecord(named, sent, When);
        var placed = _body == Body.Multipart ? FieldPart(field) : field;
        _fields.Add(placed);
        Positioned(placed, named, canReadNothing);
        Checks(placed, named, Guarded(sent.Value));
        _setMembers[named.Name] = () => _fields.Remove(placed);
    }

    private void Positioned(Expression field, RequestMember member, bool canReadNothing)
        => _positioned.Add((field, member, _guards.Count > 0, canReadNothing && SentForms.Absence(member.Lambda.ReturnType) == WhenNothing.Leave));

    private void RefuseAnItemLeftWithNoField(List<Expression> sent, string place)
    {
        var members = _positioned.Where(member => sent.Contains(member.Field)).ToArray();
        var alwaysSent = members.Where(member => !member.Guarded && !member.LeftWhenItReadsNothing).Select(member => member.Member.Name).ToArray();
        foreach (var member in members.Where(member => member.Guarded || member.LeftWhenItReadsNothing))
        {
            foreach (var item in member.Member.ItemsPastAPosition())
            {
                var itemSendsAField = alwaysSent.Any(name => RequestMember.Holds(item, name));
                if (!itemSendsAField) throw AnItemLeftWithNoField(member.Member.Name, place, LeftOutBy(member.Guarded, member.LeftWhenItReadsNothing), item);
            }
        }
    }

    private static string LeftOutBy(bool guarded, bool leftWhenItReadsNothing)
    {
        if (!guarded) return "from a value it leaves out when it reads nothing";
        return leftWhenItReadsNothing ? "under a condition, from a value it leaves out when it reads nothing" : "under a condition, which leaves it out where the condition fails";
    }

    private static InvalidOperationException AnItemLeftWithNoField(string member, string place, string leftOutBy, string item)
        => new($"The request sends '{member}' in its {place} past a list position {leftOutBy}, and no member of '{item}' is always sent, so the item would send no field and the endpoint's list would end before it on both hosts; send beside it a member of the item that is always sent, a literal, a text or a nullable member outside every condition, or send the list in a JSON body.");

    private void AddJson(RequestMember member, JsonLeaf leaf)
    {
        var named = Named(member).Name;
        var entry = (BodySteps(member), leaf);
        if (_guards.Count > 0 && entry.Item1[^1].IsIndex)
        {
            throw new InvalidOperationException(
                $"'{member.Name}' is a position of a list in the request's JSON body set under a condition on its parts, and a JSON list leaves out no position: the items after it would move into its place. Set the list's items with no condition, or set a member of the item under the condition.");
        }

        _jsonMembers.Add(named);
        _json.Add(entry);
        if (_guards.Count > 0) _jsonGuards.Add(entry.Item1, _guards.Peek());
        _setMembers[named] = () =>
        {
            _json.Remove(entry);
            _jsonGuards.Remove(entry.Item1);
            _jsonMembers.Remove(named);
        };
    }

    private void AddFile(RequestMember member, Expression value)
    {
        var named = Named(member);
        _fields.Add(new ContractRecord<FileMultipartPart>().Set(part => part.Kind, "file").Set(part => part.Name, named.Name).Set(part => part.Value, value).Set(part => part.When, When).Written);
    }

    private RequestMember Named(RequestMember named)
    {
        if (_members.Any(name => RequestMember.Holds(name, named.Name) || RequestMember.Holds(named.Name, name)))
        {
            throw new InvalidOperationException($"Request member '{named.Name}' overlaps a member already supplied; send each member once, and send either an object or its members.");
        }

        _members.Add(named.Name);
        return named;
    }

    private IReadOnlyList<JsonStep> BodySteps(RequestMember member) => member.BodySteps(_writer.Content.Json.BodyForm);

    private static RecordExpression FieldRecord(RequestMember named, (Expression Value, WireForm Form) sent, Expression when)
        => new ContractRecord<SentField>().Set(field => field.Name, named.Name).Set(field => field.Path, new ListExpression(named.Path.Select(step => ContractRecord<SentField>.Literal(step)).ToArray()))
            .Set(field => field.Value, sent.Value).Set(field => field.Form, WireRecords.Of(sent.Form))
            .Set(field => field.WhenNothing, SentForms.Absence(named.Lambda.ReturnType)).Set(field => field.When, when).Written;

    private static RecordExpression FieldPart(Expression field)
        => new ContractRecord<FieldMultipartPart>().Set(part => part.Kind, "field").Set(part => part.Field, field).Written;

    private void HeaderName(string name)
    {
        HeaderNames.Request(name);
        if (!_headerNames.Add(name))
            throw new InvalidOperationException($"Header '{name}' is supplied twice; send each header name once, ignoring case.");
        if (_guards.Count > 0) _headerGuards[name] = _guards.Peek();
    }

    private static RecordExpression HeaderRecord(string name, Expression value, ScalarWireForm form, Expression when)
        => new ContractRecord<SentHeader>().Set(header => header.Name, name).Set(header => header.Value, value).Set(header => header.Form, WireRecords.Of(form))
            .Set(header => header.When, when).Written;

    private void Open(int stage)
    {
        if (_closed)
            throw new InvalidOperationException("The request's parts are already written; write every part in its request callback.");
        if (stage != _stage) throw new InvalidOperationException("A request's parts stage is already used; write its parts in one chain.");
        _stage++;
    }
}

internal sealed class SentMember(string written, string name, bool inPath, Type valueType, WireForm form, Expression read, bool guarded)
{
    internal string Written { get; } = written;

    internal string Name { get; } = name;

    internal bool InPath { get; } = inPath;

    internal Type ValueType { get; } = valueType;

    internal WireForm Form { get; } = form;

    internal WhenNothing WhenNothing => SentForms.Absence(ValueType);

    internal Expression Read { get; } = read;

    internal bool Guarded { get; } = guarded;
}

internal sealed class SendDefaults(IReadOnlyList<(string Name, string Value)> headers, RecordExpression retry, RecordExpression busy, ListExpression contracts)
{
    internal IReadOnlyList<(string Name, string Value)> Headers { get; } = headers;

    internal RecordExpression Retry { get; } = retry;

    internal RecordExpression Busy { get; } = busy;

    internal ListExpression Contracts { get; } = contracts;
}
