using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.RequestsAndResponses;

internal sealed class HttpSend
{
    internal HttpSend(RequestMethod method, RequestUrl url, SentQuery query, SentBody body, IReadOnlyList<SentHeader> headers, Antiforgery antiforgery, RetryPolicy retry, Busy busy, bool respondedJson, IReadOnlyList<RequestContract> contracts)
    {
        Method = method;
        Url = url;
        Query = query;
        Body = body;
        Headers = headers;
        Antiforgery = antiforgery;
        Retry = retry;
        Busy = busy;
        RespondedJson = respondedJson;
        Contracts = contracts;
    }

    public RequestMethod Method { get; }

    public RequestUrl Url { get; }

    public SentQuery Query { get; }

    public SentBody Body { get; }

    public IReadOnlyList<SentHeader> Headers { get; }

    public Antiforgery Antiforgery { get; }

    public RetryPolicy Retry { get; }

    public Busy Busy { get; }

    public bool RespondedJson { get; }

    public IReadOnlyList<RequestContract> Contracts { get; }
}

internal sealed class RequestUrl
{
    internal RequestUrl(IReadOnlyList<PathPart> parts, string query)
    {
        Parts = parts;
        Query = query;
    }

    public IReadOnlyList<PathPart> Parts { get; }

    public string Query { get; }
}

internal sealed class SentQuery
{
    internal SentQuery(IReadOnlyList<SentField> fields, SentInputs inputs)
    {
        Fields = fields;
        Inputs = inputs;
    }

    public IReadOnlyList<SentField> Fields { get; }

    public SentInputs Inputs { get; }
}

internal sealed class SentField
{
    internal SentField(string name, IReadOnlyList<PathStep> path, ValueRead value, WireForm form, WhenNothing whenNothing, bool when)
    {
        Name = name;
        Path = path;
        Value = value;
        Form = form;
        WhenNothing = whenNothing;
        When = when;
    }

    public string Name { get; }

    public IReadOnlyList<PathStep> Path { get; }

    public ValueRead Value { get; }

    public WireForm Form { get; }

    public WhenNothing WhenNothing { get; }

    public bool When { get; }
}

internal sealed class SentHeader
{
    internal SentHeader(string name, ValueRead value, ScalarWireForm form, bool when)
    {
        Name = name;
        Value = value;
        Form = form;
        When = when;
    }

    public string Name { get; }

    public ValueRead Value { get; }

    public ScalarWireForm Form { get; }

    public bool When { get; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(Kind))]
[JsonDerivedType(typeof(TextPathPart))]
[JsonDerivedType(typeof(MemberPathPart))]
internal abstract class PathPart
{
    private protected PathPart()
    {
    }

    public abstract string Kind { get; }
}

internal sealed class TextPathPart : PathPart
{
    internal TextPathPart(string text)
    {
        Text = text;
    }

    public override string Kind => "text";

    public string Text { get; }
}

internal sealed class MemberPathPart : PathPart
{
    internal MemberPathPart(string name, ValueRead value, ScalarWireForm form)
    {
        Name = name;
        Value = value;
        Form = form;
    }

    public override string Kind => "member";

    public string Name { get; }

    public ValueRead Value { get; }

    public ScalarWireForm Form { get; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(Kind))]
[JsonDerivedType(typeof(NoneSentBody))]
[JsonDerivedType(typeof(JsonSentBody))]
[JsonDerivedType(typeof(UrlEncodedSentBody))]
[JsonDerivedType(typeof(MultipartSentBody))]
[JsonDerivedType(typeof(TextSentBody))]
[JsonDerivedType(typeof(FileSentBody))]
internal abstract class SentBody
{
    private protected SentBody()
    {
    }

    public abstract string Kind { get; }
}

internal sealed class NoneSentBody : SentBody
{
    internal NoneSentBody()
    {
    }

    public override string Kind => "none";
}

internal sealed class JsonSentBody : SentBody
{
    internal JsonSentBody(EvaluatedValue value, IReadOnlyList<GuardedMember> guarded, IReadOnlyList<SentEnum> enums, SentInputs inputs)
    {
        Value = value;
        Guarded = guarded;
        Enums = enums;
        Inputs = inputs;
    }

    public override string Kind => "json";

    public EvaluatedValue Value { get; }

    public IReadOnlyList<GuardedMember> Guarded { get; }

    public IReadOnlyList<SentEnum> Enums { get; }

    public SentInputs Inputs { get; }
}

internal sealed class SentEnum
{
    internal SentEnum(string name, IReadOnlyList<string> path, JsonForm form)
    {
        Name = name;
        Path = path;
        Form = form;
    }

    public string Name { get; }

    public IReadOnlyList<string> Path { get; }

    public JsonForm Form { get; }
}

internal sealed class GuardedMember
{
    internal GuardedMember(IReadOnlyList<string> path, bool when)
    {
        Path = path;
        When = when;
    }

    public IReadOnlyList<string> Path { get; }

    public bool When { get; }
}

internal sealed class UrlEncodedSentBody : SentBody
{
    internal UrlEncodedSentBody(IReadOnlyList<SentField> fields, SentInputs inputs)
    {
        Fields = fields;
        Inputs = inputs;
    }

    public override string Kind => "urlEncoded";

    public IReadOnlyList<SentField> Fields { get; }

    public SentInputs Inputs { get; }
}

internal sealed class MultipartSentBody : SentBody
{
    internal MultipartSentBody(IReadOnlyList<MultipartPart> parts, SentInputs inputs)
    {
        Parts = parts;
        Inputs = inputs;
    }

    public override string Kind => "multipart";

    public IReadOnlyList<MultipartPart> Parts { get; }

    public SentInputs Inputs { get; }
}

internal sealed class TextSentBody : SentBody
{
    internal TextSentBody(ValueRead value, SentInputs inputs)
    {
        Value = value;
        Inputs = inputs;
    }

    public override string Kind => "text";

    public ValueRead Value { get; }

    public SentInputs Inputs { get; }
}

internal sealed class FileSentBody : SentBody
{
    internal FileSentBody(ValueRead value, SentInputs inputs)
    {
        Value = value;
        Inputs = inputs;
    }

    public override string Kind => "file";

    public ValueRead Value { get; }

    public SentInputs Inputs { get; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(Kind))]
[JsonDerivedType(typeof(FieldMultipartPart))]
[JsonDerivedType(typeof(FileMultipartPart))]
internal abstract class MultipartPart
{
    private protected MultipartPart()
    {
    }

    public abstract string Kind { get; }
}

internal sealed class FieldMultipartPart : MultipartPart
{
    internal FieldMultipartPart(SentField field)
    {
        Field = field;
    }

    public override string Kind => "field";

    public SentField Field { get; }
}

internal sealed class FileMultipartPart : MultipartPart
{
    internal FileMultipartPart(string name, ValueRead value, bool when)
    {
        Name = name;
        Value = value;
        When = when;
    }

    public override string Kind => "file";

    public string Name { get; }

    public ValueRead Value { get; }

    public bool When { get; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(Kind))]
[JsonDerivedType(typeof(NoneAntiforgery))]
[JsonDerivedType(typeof(TokenAntiforgery))]
internal abstract class Antiforgery
{
    private protected Antiforgery()
    {
    }

    public abstract string Kind { get; }
}

internal sealed class NoneAntiforgery : Antiforgery
{
    internal NoneAntiforgery()
    {
    }

    public override string Kind => "none";
}

internal sealed class TokenAntiforgery : Antiforgery
{
    internal TokenAntiforgery(string value, TokenPlace place)
    {
        Value = value;
        Place = place;
    }

    public override string Kind => "token";

    public string Value { get; }

    public TokenPlace Place { get; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(Kind))]
[JsonDerivedType(typeof(HeaderTokenPlace))]
[JsonDerivedType(typeof(FieldTokenPlace))]
internal abstract class TokenPlace
{
    private protected TokenPlace()
    {
    }

    public abstract string Kind { get; }
}

internal sealed class HeaderTokenPlace : TokenPlace
{
    internal HeaderTokenPlace(string name)
    {
        Name = name;
    }

    public override string Kind => "header";

    public string Name { get; }
}

internal sealed class FieldTokenPlace : TokenPlace
{
    internal FieldTokenPlace(string name)
    {
        Name = name;
    }

    public override string Kind => "field";

    public string Name { get; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(Kind))]
[JsonDerivedType(typeof(NoneRetryPolicy))]
[JsonDerivedType(typeof(StandardRetryPolicy))]
internal abstract class RetryPolicy
{
    private protected RetryPolicy()
    {
    }

    public abstract string Kind { get; }
}

internal sealed class NoneRetryPolicy : RetryPolicy
{
    internal NoneRetryPolicy()
    {
    }

    public override string Kind => "none";
}

internal sealed class StandardRetryPolicy : RetryPolicy
{
    internal StandardRetryPolicy(int attempts, int firstWaitMilliseconds)
    {
        Attempts = attempts;
        FirstWaitMilliseconds = firstWaitMilliseconds;
    }

    public override string Kind => "standard";

    public int Attempts { get; }

    public int FirstWaitMilliseconds { get; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(Kind))]
[JsonDerivedType(typeof(NoneBusy))]
[JsonDerivedType(typeof(PageBusy))]
[JsonDerivedType(typeof(TargetsBusy))]
internal abstract class Busy
{
    private protected Busy()
    {
    }

    public abstract string Kind { get; }
}

internal sealed class NoneBusy : Busy
{
    internal NoneBusy()
    {
    }

    public override string Kind => "none";
}

internal sealed class PageBusy : Busy
{
    internal PageBusy()
    {
    }

    public override string Kind => "page";
}

internal sealed class TargetsBusy : Busy
{
    internal TargetsBusy(IReadOnlyList<BusyTarget> values)
    {
        Values = values;
    }

    public override string Kind => "targets";

    public IReadOnlyList<BusyTarget> Values { get; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(Kind))]
[JsonDerivedType(typeof(MissingBusyTarget))]
[JsonDerivedType(typeof(ElementBusyTarget))]
[JsonDerivedType(typeof(ComponentBusyTarget))]
internal abstract class BusyTarget
{
    private protected BusyTarget()
    {
    }

    public abstract string Kind { get; }
}

internal sealed class MissingBusyTarget : BusyTarget
{
    internal MissingBusyTarget(string id)
    {
        Id = id;
    }

    public override string Kind => "missing";

    public string Id { get; }
}

internal sealed class ElementBusyTarget : BusyTarget
{
    internal ElementBusyTarget(string id)
    {
        Id = id;
    }

    public override string Kind => "element";

    public string Id { get; }
}

internal sealed class ComponentBusyTarget : BusyTarget
{
    internal ComponentBusyTarget(ValueRead value)
    {
        Value = value;
    }

    public override string Kind => "component";

    public ValueRead Value { get; }
}
