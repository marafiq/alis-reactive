using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Alis.Reactive.Execution;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using Alis.Reactive.ValuesAndSources;
using Expression = Alis.Reactive.Schema.Expression;

namespace Alis.Reactive.RequestsAndResponses;

internal sealed class RequestPartsWriter
{
    private readonly BlockWriter _writer;
    private readonly RequestUrlParts _url;
    private readonly List<Expression> _query = [];
    private readonly List<Statement> _captures = [];
    private readonly List<Expression> _headers = [];
    private readonly Dictionary<string, Expression> _path = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _members = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _headerNames = new(StringComparer.OrdinalIgnoreCase);
    private bool _closed;
    private int _stage;

    internal RequestPartsWriter(BlockWriter writer, string url)
    {
        _writer = writer;
        _url = new RequestUrlParts(url);
    }

    internal int Literal<TValue>(int stage, LambdaExpression member, TValue value, bool path)
    {
        Open(stage);
        Add(member, RequestValues.Literal(value, _writer, member.ToString()), path);
        return _stage;
    }

    internal int Source<TValue>(int stage, LambdaExpression member, ITypedSource<TValue> value, bool path)
    {
        Open(stage);
        Add(member, RequestValues.Source(value, _writer, member.ToString(), _captures.Add), path);
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
        var inputs = new ContractRecord<SentInputs>()
            .Set(sent => sent.Container, new ContractRecord<NoneContainerCapture>().Set(container => container.Kind, "none").Written)
            .Set(sent => sent.Individual, new ListExpression([])).Written;
        var url = new ContractRecord<RequestUrl>()
            .Set(sent => sent.Parts, new ListExpression(_url.Parts.Select(part => part.Member.Length > 0
                ? _path[part.Member]
                : new ContractRecord<TextPathPart>().Set(text => text.Kind, "text").Set(text => text.Text, part.Text).Written).ToArray()))
            .Set(sent => sent.Query, _url.Query).Written;
        var accept = success == SuccessBody.Json ? "application/json" : success == SuccessBody.PlainText ? "text/*" : success == SuccessBody.Html ? "text/html" : "*/*";
        return new ContractRecord<HttpSend>()
            .Set(sent => sent.Method, method)
            .Set(sent => sent.Url, url)
            .Set(sent => sent.Query, new ContractRecord<SentQuery>().Set(query => query.Fields, new ListExpression(_query.ToArray())).Set(query => query.Inputs, inputs).Written)
            .Set(sent => sent.Body, new ContractRecord<NoneSentBody>().Set(body => body.Kind, "none").Written)
            .Set(sent => sent.Headers, new ListExpression([.. _headers, HeaderRecord("Accept", RequestValues.Held(ContractRecord<SentHeader>.Literal(accept)), new TextScalarWireForm())]))
            .Set(sent => sent.Antiforgery, new ContractRecord<NoneAntiforgery>().Set(token => token.Kind, "none").Written)
            .Set(sent => sent.Retry, new ContractRecord<NoneRetryPolicy>().Set(retry => retry.Kind, "none").Written)
            .Set(sent => sent.Busy, new ContractRecord<NoneBusy>().Set(busy => busy.Kind, "none").Written)
            .Set(sent => sent.RespondedJson, respondedJson)
            .Set(sent => sent.Contracts, new ListExpression([])).Written;
    }

    private void Add(LambdaExpression member, (Expression Value, WireForm Form) sent, bool path)
    {
        var named = new RequestMember(member);
        if (_members.Any(name => name.Equals(named.Name, StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith(named.Name + ".", StringComparison.OrdinalIgnoreCase) || named.Name.StartsWith(name + ".", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Request member '{named.Name}' overlaps a member already supplied; send each member once, and send either an object or its members.");
        }

        _members.Add(named.Name);
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
            return;
        }

        _query.Add(new ContractRecord<SentField>().Set(field => field.Name, named.Name).Set(field => field.Path, new ListExpression(named.Path.Select(step => ContractRecord<SentField>.Literal(step)).ToArray()))
            .Set(field => field.Value, sent.Value).Set(field => field.Form, WireRecords.Of(sent.Form))
            .Set(field => field.WhenNothing, RequestValues.Absence(member.ReturnType)).Written);
    }

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
