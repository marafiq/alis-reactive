using System;
using System.Linq.Expressions;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.TypeKnowledge;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.TriggersAndPipelines;

/// <summary>
/// The parts of a request with no body: the URL's placeholders, its query and its headers.
/// </summary>
public sealed class QueryParts<TRequest>
{
    private readonly RequestPartsWriter _parts;
    private readonly int _stage;

    internal QueryParts(RequestPartsWriter parts, int stage)
    {
        _parts = parts;
        _stage = stage;
    }

    internal QueryParts<TRequest> PlacedLiteral<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => new(_parts, _parts.Literal(_stage, member, value, _parts.FillsAPlaceholder(member)));

    internal QueryParts<TRequest> PlacedSource<TMember, TValue>(Expression<Func<TRequest, TMember>> member, ITypedSource<TValue> value) => new(_parts, _parts.Source(_stage, member, value, _parts.FillsAPlaceholder(member)));

    internal QueryParts<TRequest> PlacedInput<TMember, TValue>(Expression<Func<TRequest, TMember>> member, BoundInput<TValue> input)
        => new(_parts, _parts.Input(_stage, member, input.Id, stage => _parts.Source(stage, member, input.Value, _parts.FillsAPlaceholder(member))));

    internal QueriedParts<TRequest> Submitted(RequestForm<TRequest> form) => new(_parts, _parts.Submits(_stage, form.ElementId, ClassNames.ModelKey(typeof(TRequest))));

    internal QueriedParts<TRequest> Sent(RequestContainer<TRequest> container) => new(_parts, _parts.Sends(_stage, container.ElementId, ClassNames.ModelKey(typeof(TRequest))));

    /// <summary>Fills the URL's placeholder named like a member of the request with what a source reads when the request is sent.</summary>
    /// <remarks>A value that cannot be read, is empty or holds a slash stops the request before it is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public QueryParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value)
    => new(_parts, _parts.Source(_stage, member, value, true));

    /// <summary>Fills the URL's placeholder named like a member of the request with a literal.</summary>
    /// <remarks>A value that cannot be read, is empty or holds a slash stops the request before it is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public QueryParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue>> member, TValue value)
    => new(_parts, _parts.Literal(_stage, member, value, true));

    /// <summary>Fills the URL's placeholder named like a member of the request with what a source reads when the request is sent.</summary>
    /// <remarks>A value that cannot be read, is empty or holds a slash stops the request before it is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public QueryParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
    => new(_parts, _parts.Source(_stage, member, value, true));

    /// <summary>Sends a member in the URL's query, under the member's C# path, as a source reads it when the request is sent.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public QueryParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value)
    => new(_parts, _parts.Source(_stage, member, value, false));

    /// <summary>Sends a member in the URL's query, under the member's C# path, as a literal.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public QueryParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue>> member, TValue value)
    => new(_parts, _parts.Literal(_stage, member, value, false));

    /// <summary>Sends a member in the URL's query, under the member's C# path, as a source reads it when the request is sent.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public QueryParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
    => new(_parts, _parts.Source(_stage, member, value, false));

    /// <summary>Sends a header with a fixed text.</summary>
    /// <remarks><c>Content-Type</c>, <c>Accept</c> and the headers the framework or the browser writes are refused where the view renders.</remarks>
    /// <param name="name">The header's name.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public QueryParts<TRequest> Header(string name, string value)
    => new(_parts, _parts.Header(_stage, name, value));

    /// <summary>Sends a header whose text a source reads when the request is sent; one that cannot be read is not sent.</summary>
    /// <remarks><c>Content-Type</c>, <c>Accept</c> and the headers the framework or the browser writes are refused where the view renders.</remarks>
    /// <param name="name">The header's name.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public QueryParts<TRequest> Header<TValue>(string name, ITypedSource<TValue> value)
    => new(_parts, _parts.Header(_stage, name, value));

    /// <summary>Starts a condition on the parts its branch sends, read once when the request is sent.</summary>
    /// <remarks>When the condition is not true, each part the branch writes is left out, as if it were never gathered, and the endpoint binds the member's start. The URL, the method and the body's format take no condition, so a branch takes no body and no container: the request names them before its first condition.</remarks>
    /// <param name="condition">The condition; the branch's parts are sent only when it is true.</param>
    /// <returns>The condition, which takes its branch with <c>Then</c>.</returns>
    public PartsGuard<QueriedParts<TRequest>> When(TypedSource<bool> condition) => new(_parts, _stage, condition.Expression, stage => new QueriedParts<TRequest>(_parts, stage));
}

/// <summary>
/// The parts of a request with no body after it named the container it sends: it takes no second.
/// </summary>
public sealed class QueriedParts<TRequest>
{
    private readonly RequestPartsWriter _parts;
    private readonly int _stage;

    internal QueriedParts(RequestPartsWriter parts, int stage)
    {
        _parts = parts;
        _stage = stage;
    }

    internal QueriedParts<TRequest> PlacedLiteral<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => new(_parts, _parts.Literal(_stage, member, value, _parts.FillsAPlaceholder(member)));

    internal QueriedParts<TRequest> PlacedSource<TMember, TValue>(Expression<Func<TRequest, TMember>> member, ITypedSource<TValue> value) => new(_parts, _parts.Source(_stage, member, value, _parts.FillsAPlaceholder(member)));

    internal QueriedParts<TRequest> PlacedInput<TMember, TValue>(Expression<Func<TRequest, TMember>> member, BoundInput<TValue> input)
        => new(_parts, _parts.Input(_stage, member, input.Id, stage => _parts.Source(stage, member, input.Value, _parts.FillsAPlaceholder(member))));

    /// <summary>Fills the URL's placeholder named like a member of the request with what a source reads when the request is sent.</summary>
    /// <remarks>A value that cannot be read, is empty or holds a slash stops the request before it is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public QueriedParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => new(_parts, _parts.Source(_stage, member, value, true));

    /// <summary>Fills the URL's placeholder named like a member of the request with a literal.</summary>
    /// <remarks>A value that cannot be read, is empty or holds a slash stops the request before it is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public QueriedParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => new(_parts, _parts.Literal(_stage, member, value, true));

    /// <summary>Fills the URL's placeholder named like a member of the request with what a source reads when the request is sent.</summary>
    /// <remarks>A value that cannot be read, is empty or holds a slash stops the request before it is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public QueriedParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => new(_parts, _parts.Source(_stage, member, value, true));

    /// <summary>Sends a member in the URL's query, under the name the endpoint binds it by, as a source reads it when the request is sent.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public QueriedParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => new(_parts, _parts.Source(_stage, member, value, false));

    /// <summary>Sends a member in the URL's query, under the name the endpoint binds it by, as a literal.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public QueriedParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => new(_parts, _parts.Literal(_stage, member, value, false));

    /// <summary>Sends a member in the URL's query, under the name the endpoint binds it by, as a source reads it when the request is sent.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public QueriedParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => new(_parts, _parts.Source(_stage, member, value, false));

    /// <summary>Sends a header with a fixed text.</summary>
    /// <remarks><c>Content-Type</c>, <c>Accept</c> and the headers the framework or the browser writes are refused where the view renders.</remarks>
    /// <param name="name">The header's name.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public QueriedParts<TRequest> Header(string name, string value) => new(_parts, _parts.Header(_stage, name, value));

    /// <summary>Sends a header whose text a source reads when the request is sent; one that cannot be read is not sent.</summary>
    /// <remarks><c>Content-Type</c>, <c>Accept</c> and the headers the framework or the browser writes are refused where the view renders.</remarks>
    /// <param name="name">The header's name.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public QueriedParts<TRequest> Header<TValue>(string name, ITypedSource<TValue> value) => new(_parts, _parts.Header(_stage, name, value));

    /// <summary>Starts a condition on the parts its branch sends, read once when the request is sent.</summary>
    /// <remarks>When the condition is not true, each part the branch writes is left out, as if it were never gathered, and the endpoint binds the member's start. The URL, the method and the body's format take no condition, so a branch takes no body and no container: the request names them before its first condition.</remarks>
    /// <param name="condition">The condition; the branch's parts are sent only when it is true.</param>
    /// <returns>The condition, which takes its branch with <c>Then</c>.</returns>
    public PartsGuard<QueriedParts<TRequest>> When(TypedSource<bool> condition) => new(_parts, _stage, condition.Expression, stage => new QueriedParts<TRequest>(_parts, stage));
}

/// <summary>
/// The parts of a request with a body: the URL's placeholders, its query, its headers and one body.
/// </summary>
public sealed class BodyParts<TRequest>
{
    private readonly RequestPartsWriter _parts;
    private readonly int _stage;

    internal BodyParts(RequestPartsWriter parts, int stage)
    {
        _parts = parts;
        _stage = stage;
    }

    internal BodiedParts<TRequest> PlacedLiteral<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => new(_parts, _parts.Multipart(_stage, stage => _parts.PlacedLiteral(stage, member, value)));

    internal BodiedParts<TRequest> PlacedSource<TMember, TValue>(Expression<Func<TRequest, TMember>> member, ITypedSource<TValue> value) => new(_parts, _parts.Multipart(_stage, stage => _parts.PlacedSource(stage, member, value)));

    internal BodiedParts<TRequest> PlacedInput<TMember, TValue>(Expression<Func<TRequest, TMember>> member, BoundInput<TValue> input)
        => new(_parts, _parts.Multipart(_stage, stage => _parts.Input(stage, member, input.Id, placed => _parts.PlacedSource(placed, member, input.Value))));

    internal BodiedParts<TRequest> Submitted(RequestForm<TRequest> form) => new(_parts, _parts.Multipart(_parts.Submits(_stage, form.ElementId, ClassNames.ModelKey(typeof(TRequest))), _ => { }));

    internal BodiedParts<TRequest> Sent(RequestContainer<TRequest> container) => new(_parts, _parts.Multipart(_parts.Sends(_stage, container.ElementId, ClassNames.ModelKey(typeof(TRequest))), _ => { }));

    /// <summary>Fills the URL's placeholder named like a member of the request with what a source reads when the request is sent.</summary>
    /// <remarks>A value that cannot be read, is empty or holds a slash stops the request before it is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public BodyParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => new(_parts, _parts.Source(_stage, member, value, true));

    /// <summary>Fills the URL's placeholder named like a member of the request with a literal.</summary>
    /// <remarks>A value that cannot be read, is empty or holds a slash stops the request before it is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public BodyParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => new(_parts, _parts.Literal(_stage, member, value, true));

    /// <summary>Fills the URL's placeholder named like a member of the request with what a source reads when the request is sent.</summary>
    /// <remarks>A value that cannot be read, is empty or holds a slash stops the request before it is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public BodyParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => new(_parts, _parts.Source(_stage, member, value, true));

    /// <summary>Sends a member in the URL's query, under the name the endpoint binds it by, as a source reads it when the request is sent.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public BodyParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => new(_parts, _parts.Source(_stage, member, value, false));

    /// <summary>Sends a member in the URL's query, under the name the endpoint binds it by, as a literal.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public BodyParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => new(_parts, _parts.Literal(_stage, member, value, false));

    /// <summary>Sends a member in the URL's query, under the name the endpoint binds it by, as a source reads it when the request is sent.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public BodyParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => new(_parts, _parts.Source(_stage, member, value, false));

    /// <summary>Sends a header with a fixed text.</summary>
    /// <remarks><c>Content-Type</c>, <c>Accept</c> and the headers the framework or the browser writes are refused where the view renders.</remarks>
    /// <param name="name">The header's name.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public BodyParts<TRequest> Header(string name, string value) => new(_parts, _parts.Header(_stage, name, value));

    /// <summary>Sends a header whose text a source reads when the request is sent; one that cannot be read is not sent.</summary>
    /// <remarks><c>Content-Type</c>, <c>Accept</c> and the headers the framework or the browser writes are refused where the view renders.</remarks>
    /// <param name="name">The header's name.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public BodyParts<TRequest> Header<TValue>(string name, ITypedSource<TValue> value) => new(_parts, _parts.Header(_stage, name, value));

    /// <summary>Sends a JSON body: one object whose members the lambda sets.</summary>
    /// <param name="body">The body's members.</param>
    /// <returns>The request's parts, which take no second body.</returns>
    public BodiedParts<TRequest> Json(Action<JsonBody<TRequest>> body) => new(_parts, _parts.Json(_stage, stage => body(new JsonBody<TRequest>(_parts, stage))));

    /// <summary>Sends a JSON body: one object whose members the lambda sets, with the framework inputs of a container.</summary>
    /// <param name="container">The container whose framework inputs the request sends; it becomes the request's container, where its failures show.</param>
    /// <param name="body">The body's members.</param>
    /// <returns>The request's parts, which take no second body.</returns>
    public BodiedParts<TRequest> Json(RequestContainer<TRequest> container, Action<JsonBody<TRequest>> body)
        => new(_parts, _parts.Json(_parts.Sends(_stage, container.ElementId, ClassNames.ModelKey(typeof(TRequest))), stage => body(new JsonBody<TRequest>(_parts, stage))));

    /// <summary>Sends a URL-encoded body whose fields the lambda sets.</summary>
    /// <param name="body">The body's members.</param>
    /// <returns>The request's parts, which take no second body.</returns>
    public BodiedParts<TRequest> UrlEncoded(Action<UrlEncodedBody<TRequest>> body) => new(_parts, _parts.UrlEncoded(_stage, stage => body(new UrlEncodedBody<TRequest>(_parts, stage))));

    /// <summary>Sends a URL-encoded body whose fields the lambda sets, with a native form's submission set.</summary>
    /// <param name="form">The native form whose submission set the request sends; it becomes the request's container, where its failures show.</param>
    /// <param name="body">The body's members.</param>
    /// <returns>The request's parts, which take no second body.</returns>
    public BodiedParts<TRequest> UrlEncoded(RequestForm<TRequest> form, Action<UrlEncodedBody<TRequest>> body)
        => new(_parts, _parts.UrlEncoded(_parts.Submits(_stage, form.ElementId, ClassNames.ModelKey(typeof(TRequest))), stage => body(new UrlEncodedBody<TRequest>(_parts, stage))));

    /// <summary>Sends a URL-encoded body whose fields the lambda sets, with the framework inputs of a container.</summary>
    /// <param name="container">The container whose framework inputs the request sends; it becomes the request's container, where its failures show.</param>
    /// <param name="body">The body's members.</param>
    /// <returns>The request's parts, which take no second body.</returns>
    public BodiedParts<TRequest> UrlEncoded(RequestContainer<TRequest> container, Action<UrlEncodedBody<TRequest>> body)
        => new(_parts, _parts.UrlEncoded(_parts.Sends(_stage, container.ElementId, ClassNames.ModelKey(typeof(TRequest))), stage => body(new UrlEncodedBody<TRequest>(_parts, stage))));

    /// <summary>Sends a multipart body whose fields and files the lambda sets.</summary>
    /// <param name="body">The body's members.</param>
    /// <returns>The request's parts, which take no second body.</returns>
    public BodiedParts<TRequest> Multipart(Action<MultipartBody<TRequest>> body) => new(_parts, _parts.Multipart(_stage, stage => body(new MultipartBody<TRequest>(_parts, stage))));

    /// <summary>Sends a multipart body whose fields and files the lambda sets, with a native form's submission set.</summary>
    /// <param name="form">The native form whose submission set the request sends; it becomes the request's container, where its failures show.</param>
    /// <param name="body">The body's members.</param>
    /// <returns>The request's parts, which take no second body.</returns>
    public BodiedParts<TRequest> Multipart(RequestForm<TRequest> form, Action<MultipartBody<TRequest>> body)
        => new(_parts, _parts.Multipart(_parts.Submits(_stage, form.ElementId, ClassNames.ModelKey(typeof(TRequest))), stage => body(new MultipartBody<TRequest>(_parts, stage))));

    /// <summary>Sends a multipart body whose fields and files the lambda sets, with the framework inputs of a container.</summary>
    /// <param name="container">The container whose framework inputs the request sends; it becomes the request's container, where its failures show.</param>
    /// <param name="body">The body's members.</param>
    /// <returns>The request's parts, which take no second body.</returns>
    public BodiedParts<TRequest> Multipart(RequestContainer<TRequest> container, Action<MultipartBody<TRequest>> body)
        => new(_parts, _parts.Multipart(_parts.Sends(_stage, container.ElementId, ClassNames.ModelKey(typeof(TRequest))), stage => body(new MultipartBody<TRequest>(_parts, stage))));

    /// <summary>Sends a text as the whole body.</summary>
    /// <param name="text">The text sent as the whole body.</param>
    /// <returns>The request's parts, which take no member of a body.</returns>
    public WholeBodyParts<TRequest> Text(string text) => new(_parts, _parts.Text(_stage, text));

    /// <summary>Sends the text a source reads when the request is sent as the whole body, or an empty body when the source reads nothing or holds null.</summary>
    /// <param name="text">The text sent as the whole body.</param>
    /// <returns>The request's parts, which take no member of a body.</returns>
    public WholeBodyParts<TRequest> Text(ITypedSource<string?> text) => new(_parts, _parts.Text(_stage, text));

    /// <summary>Sends one file as the whole body, under the file's own media type.</summary>
    /// <param name="file">The file sent.</param>
    /// <returns>The request's parts, which take no member of a body.</returns>
    public WholeBodyParts<TRequest> File(BrowserFile file) => new(_parts, _parts.File(_stage, file));

    /// <summary>Starts a condition on the parts its branch sends, read once when the request is sent.</summary>
    /// <remarks>When the condition is not true, each part the branch writes is left out, as if it were never gathered, and the endpoint binds the member's start. The URL, the method and the body's format take no condition, so a branch takes no body and no container: a request that names no body before its first condition sends its members as multipart, as <c>Set</c> does.</remarks>
    /// <param name="condition">The condition; the branch's parts are sent only when it is true.</param>
    /// <returns>The condition, which takes its branch with <c>Then</c>.</returns>
    public PartsGuard<BodiedParts<TRequest>> When(TypedSource<bool> condition)
        => new(_parts, _parts.Multipart(_stage, _ => { }), condition.Expression, stage => new BodiedParts<TRequest>(_parts, stage));
}

/// <summary>
/// The parts of a request after its body is written: it takes no second body.
/// </summary>
public sealed class BodiedParts<TRequest>
{
    private readonly RequestPartsWriter _parts;
    private readonly int _stage;

    internal BodiedParts(RequestPartsWriter parts, int stage)
    {
        _parts = parts;
        _stage = stage;
    }

    internal BodiedParts<TRequest> PlacedLiteral<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => new(_parts, _parts.PlacedLiteral(_stage, member, value));

    internal BodiedParts<TRequest> PlacedSource<TMember, TValue>(Expression<Func<TRequest, TMember>> member, ITypedSource<TValue> value) => new(_parts, _parts.PlacedSource(_stage, member, value));

    internal BodiedParts<TRequest> PlacedInput<TMember, TValue>(Expression<Func<TRequest, TMember>> member, BoundInput<TValue> input)
        => new(_parts, _parts.Input(_stage, member, input.Id, stage => _parts.PlacedSource(stage, member, input.Value)));

    /// <summary>Fills the URL's placeholder named like a member of the request with what a source reads when the request is sent.</summary>
    /// <remarks>A value that cannot be read, is empty or holds a slash stops the request before it is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public BodiedParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => new(_parts, _parts.Source(_stage, member, value, true));

    /// <summary>Fills the URL's placeholder named like a member of the request with a literal.</summary>
    /// <remarks>A value that cannot be read, is empty or holds a slash stops the request before it is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public BodiedParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => new(_parts, _parts.Literal(_stage, member, value, true));

    /// <summary>Fills the URL's placeholder named like a member of the request with what a source reads when the request is sent.</summary>
    /// <remarks>A value that cannot be read, is empty or holds a slash stops the request before it is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public BodiedParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => new(_parts, _parts.Source(_stage, member, value, true));

    /// <summary>Sends a member in the URL's query, under the name the endpoint binds it by, as a source reads it when the request is sent.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public BodiedParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => new(_parts, _parts.Source(_stage, member, value, false));

    /// <summary>Sends a member in the URL's query, under the name the endpoint binds it by, as a literal.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public BodiedParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => new(_parts, _parts.Literal(_stage, member, value, false));

    /// <summary>Sends a member in the URL's query, under the name the endpoint binds it by, as a source reads it when the request is sent.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public BodiedParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => new(_parts, _parts.Source(_stage, member, value, false));

    /// <summary>Sends a header with a fixed text.</summary>
    /// <remarks><c>Content-Type</c>, <c>Accept</c> and the headers the framework or the browser writes are refused where the view renders.</remarks>
    /// <param name="name">The header's name.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public BodiedParts<TRequest> Header(string name, string value) => new(_parts, _parts.Header(_stage, name, value));

    /// <summary>Sends a header whose text a source reads when the request is sent; one that cannot be read is not sent.</summary>
    /// <remarks><c>Content-Type</c>, <c>Accept</c> and the headers the framework or the browser writes are refused where the view renders.</remarks>
    /// <param name="name">The header's name.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public BodiedParts<TRequest> Header<TValue>(string name, ITypedSource<TValue> value) => new(_parts, _parts.Header(_stage, name, value));

    /// <summary>Starts a condition on the parts its branch sends, read once when the request is sent.</summary>
    /// <remarks>When the condition is not true, each part the branch writes is left out, as if it were never gathered, and the endpoint binds the member's start. The URL, the method and the body's format take no condition, so a branch takes no body and no container: the request names them before its first condition.</remarks>
    /// <param name="condition">The condition; the branch's parts are sent only when it is true.</param>
    /// <returns>The condition, which takes its branch with <c>Then</c>.</returns>
    public PartsGuard<BodiedParts<TRequest>> When(TypedSource<bool> condition) => new(_parts, _stage, condition.Expression, stage => new BodiedParts<TRequest>(_parts, stage));
}

/// <summary>
/// The parts of a request whose whole body is a text or a file: it takes no member of a body.
/// </summary>
public sealed class WholeBodyParts<TRequest>
{
    private readonly RequestPartsWriter _parts;
    private readonly int _stage;

    internal WholeBodyParts(RequestPartsWriter parts, int stage)
    {
        _parts = parts;
        _stage = stage;
    }

    /// <summary>Fills the URL's placeholder named like a member of the request with what a source reads when the request is sent.</summary>
    /// <remarks>A value that cannot be read, is empty or holds a slash stops the request before it is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public WholeBodyParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => new(_parts, _parts.Source(_stage, member, value, true));

    /// <summary>Fills the URL's placeholder named like a member of the request with a literal.</summary>
    /// <remarks>A value that cannot be read, is empty or holds a slash stops the request before it is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public WholeBodyParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => new(_parts, _parts.Literal(_stage, member, value, true));

    /// <summary>Fills the URL's placeholder named like a member of the request with what a source reads when the request is sent.</summary>
    /// <remarks>A value that cannot be read, is empty or holds a slash stops the request before it is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public WholeBodyParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => new(_parts, _parts.Source(_stage, member, value, true));

    /// <summary>Sends a member in the URL's query, under the name the endpoint binds it by, as a source reads it when the request is sent.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public WholeBodyParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => new(_parts, _parts.Source(_stage, member, value, false));

    /// <summary>Sends a member in the URL's query, under the name the endpoint binds it by, as a literal.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public WholeBodyParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => new(_parts, _parts.Literal(_stage, member, value, false));

    /// <summary>Sends a member in the URL's query, under the name the endpoint binds it by, as a source reads it when the request is sent.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public WholeBodyParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => new(_parts, _parts.Source(_stage, member, value, false));

    /// <summary>Sends a header with a fixed text.</summary>
    /// <remarks><c>Content-Type</c>, <c>Accept</c> and the headers the framework or the browser writes are refused where the view renders.</remarks>
    /// <param name="name">The header's name.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public WholeBodyParts<TRequest> Header(string name, string value) => new(_parts, _parts.Header(_stage, name, value));

    /// <summary>Sends a header whose text a source reads when the request is sent; one that cannot be read is not sent.</summary>
    /// <remarks><c>Content-Type</c>, <c>Accept</c> and the headers the framework or the browser writes are refused where the view renders.</remarks>
    /// <param name="name">The header's name.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public WholeBodyParts<TRequest> Header<TValue>(string name, ITypedSource<TValue> value) => new(_parts, _parts.Header(_stage, name, value));

    /// <summary>Starts a condition on the parts its branch sends, read once when the request is sent.</summary>
    /// <remarks>When the condition is not true, each part the branch writes is left out, as if it were never gathered, and the endpoint binds the member's start. The URL, the method and the body's format take no condition, so a branch takes no body and no container: the request names them before its first condition.</remarks>
    /// <param name="condition">The condition; the branch's parts are sent only when it is true.</param>
    /// <returns>The condition, which takes its branch with <c>Then</c>.</returns>
    public PartsGuard<WholeBodyParts<TRequest>> When(TypedSource<bool> condition) => new(_parts, _stage, condition.Expression, stage => new WholeBodyParts<TRequest>(_parts, stage));
}

/// <summary>
/// The members of a JSON body.
/// </summary>
public sealed class JsonBody<TRequest>
{
    private readonly RequestPartsWriter _parts;
    private readonly int _stage;

    internal JsonBody(RequestPartsWriter parts, int stage)
    {
        _parts = parts;
        _stage = stage;
    }

    /// <summary>Sends a member of the request in the body, as a source reads it when the request is sent.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The body's members.</returns>
    public JsonBody<TRequest> Set<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => new(_parts, _parts.BodySource(_stage, member, value));

    /// <summary>Sends a member of the request in the body, as a literal.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The body's members.</returns>
    public JsonBody<TRequest> Set<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => new(_parts, _parts.BodyLiteral(_stage, member, value));

    /// <summary>Sends a member of the request in the body, as a source reads it when the request is sent.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The body's members.</returns>
    public JsonBody<TRequest> Set<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => new(_parts, _parts.BodySource(_stage, member, value));

    /// <summary>Sends one input of the request's own view in the body, under a member's name.</summary>
    /// <remarks>When an input and a set member send one name, the input's value is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="input">The input of the request's own view whose value is sent.</param>
    /// <returns>The body's members.</returns>
    public JsonBody<TRequest> Input<TMember, TValue>(Expression<Func<TRequest, TMember>> member, BoundInput<TValue> input)
        where TValue : TMember
        => new(_parts, _parts.Input(_stage, member, input.Id, stage => _parts.BodySource(stage, member, input.Value)));

    /// <summary>Sends one input of the request's own view in the body, under a member's name.</summary>
    /// <remarks>When an input and a set member send one name, the input's value is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="input">The input of the request's own view whose value is sent.</param>
    /// <returns>The body's members.</returns>
    public JsonBody<TRequest> Input<TValue>(Expression<Func<TRequest, TValue?>> member, BoundInput<TValue> input)
        where TValue : struct
        => new(_parts, _parts.Input(_stage, member, input.Id, stage => _parts.BodySource(stage, member, input.Value)));

    /// <summary>Starts a condition on the parts its branch sends, read once when the request is sent.</summary>
    /// <remarks>When the condition is not true, each member the branch writes is left out, as if it were never gathered, and the endpoint binds the member's start.</remarks>
    /// <param name="condition">The condition; the branch's parts are sent only when it is true.</param>
    /// <returns>The condition, which takes its branch with <c>Then</c>.</returns>
    public PartsGuard<JsonBody<TRequest>> When(TypedSource<bool> condition) => new(_parts, _stage, condition.Expression, stage => new JsonBody<TRequest>(_parts, stage));
}

/// <summary>
/// The fields of a URL-encoded body.
/// </summary>
public sealed class UrlEncodedBody<TRequest>
{
    private readonly RequestPartsWriter _parts;
    private readonly int _stage;

    internal UrlEncodedBody(RequestPartsWriter parts, int stage)
    {
        _parts = parts;
        _stage = stage;
    }

    /// <summary>Sends a member of the request in the body, as a source reads it when the request is sent.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The body's members.</returns>
    public UrlEncodedBody<TRequest> Set<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => new(_parts, _parts.BodySource(_stage, member, value));

    /// <summary>Sends a member of the request in the body, as a literal.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The body's members.</returns>
    public UrlEncodedBody<TRequest> Set<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => new(_parts, _parts.BodyLiteral(_stage, member, value));

    /// <summary>Sends a member of the request in the body, as a source reads it when the request is sent.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The body's members.</returns>
    public UrlEncodedBody<TRequest> Set<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => new(_parts, _parts.BodySource(_stage, member, value));

    /// <summary>Sends one input of the request's own view in the body, under a member's name.</summary>
    /// <remarks>When an input and a set member send one name, the input's value is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="input">The input of the request's own view whose value is sent.</param>
    /// <returns>The body's members.</returns>
    public UrlEncodedBody<TRequest> Input<TMember, TValue>(Expression<Func<TRequest, TMember>> member, BoundInput<TValue> input)
        where TValue : TMember
        => new(_parts, _parts.Input(_stage, member, input.Id, stage => _parts.BodySource(stage, member, input.Value)));

    /// <summary>Sends one input of the request's own view in the body, under a member's name.</summary>
    /// <remarks>When an input and a set member send one name, the input's value is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="input">The input of the request's own view whose value is sent.</param>
    /// <returns>The body's members.</returns>
    public UrlEncodedBody<TRequest> Input<TValue>(Expression<Func<TRequest, TValue?>> member, BoundInput<TValue> input)
        where TValue : struct
        => new(_parts, _parts.Input(_stage, member, input.Id, stage => _parts.BodySource(stage, member, input.Value)));

    /// <summary>Starts a condition on the parts its branch sends, read once when the request is sent.</summary>
    /// <remarks>When the condition is not true, each member the branch writes is left out, as if it were never gathered, and the endpoint binds the member's start.</remarks>
    /// <param name="condition">The condition; the branch's parts are sent only when it is true.</param>
    /// <returns>The condition, which takes its branch with <c>Then</c>.</returns>
    public PartsGuard<UrlEncodedBody<TRequest>> When(TypedSource<bool> condition) => new(_parts, _stage, condition.Expression, stage => new UrlEncodedBody<TRequest>(_parts, stage));
}

/// <summary>
/// The fields and files of a multipart body.
/// </summary>
public sealed partial class MultipartBody<TRequest>
{
    private readonly RequestPartsWriter _parts;
    private readonly int _stage;

    internal MultipartBody(RequestPartsWriter parts, int stage)
    {
        _parts = parts;
        _stage = stage;
    }

    /// <summary>Sends a member of the request in the body, as a source reads it when the request is sent.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The body's members.</returns>
    public MultipartBody<TRequest> Set<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => new(_parts, _parts.BodySource(_stage, member, value));

    /// <summary>Sends a member of the request in the body, as a literal.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The body's members.</returns>
    public MultipartBody<TRequest> Set<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => new(_parts, _parts.BodyLiteral(_stage, member, value));

    /// <summary>Sends a member of the request in the body, as a source reads it when the request is sent.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The body's members.</returns>
    public MultipartBody<TRequest> Set<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => new(_parts, _parts.BodySource(_stage, member, value));

    /// <summary>Sends one input of the request's own view in the body, under a member's name.</summary>
    /// <remarks>When an input and a set member send one name, the input's value is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="input">The input of the request's own view whose value is sent.</param>
    /// <returns>The body's members.</returns>
    public MultipartBody<TRequest> Input<TMember, TValue>(Expression<Func<TRequest, TMember>> member, BoundInput<TValue> input)
        where TValue : TMember
        => new(_parts, _parts.Input(_stage, member, input.Id, stage => _parts.BodySource(stage, member, input.Value)));

    /// <summary>Sends one input of the request's own view in the body, under a member's name.</summary>
    /// <remarks>When an input and a set member send one name, the input's value is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="input">The input of the request's own view whose value is sent.</param>
    /// <returns>The body's members.</returns>
    public MultipartBody<TRequest> Input<TValue>(Expression<Func<TRequest, TValue?>> member, BoundInput<TValue> input)
        where TValue : struct
        => new(_parts, _parts.Input(_stage, member, input.Id, stage => _parts.BodySource(stage, member, input.Value)));

    /// <summary>Starts a condition on the parts its branch sends, read once when the request is sent.</summary>
    /// <remarks>When the condition is not true, each member the branch writes is left out, as if it were never gathered, and the endpoint binds the member's start.</remarks>
    /// <param name="condition">The condition; the branch's parts are sent only when it is true.</param>
    /// <returns>The condition, which takes its branch with <c>Then</c>.</returns>
    public PartsGuard<MultipartBody<TRequest>> When(TypedSource<bool> condition) => new(_parts, _stage, condition.Expression, stage => new MultipartBody<TRequest>(_parts, stage));
}
