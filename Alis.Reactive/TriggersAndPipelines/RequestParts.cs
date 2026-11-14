using System;
using System.Linq.Expressions;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.TriggersAndPipelines;

/// <summary>
/// The parts of a request with no body: the URL's placeholders, its query and its headers.
/// </summary>
public sealed class QueryParts<TRequest>
{
    internal QueryParts()
    {
    }

    /// <summary>Fills the URL's placeholder named like a member of the request with what a source reads when the request is sent.</summary>
    /// <remarks>A value that cannot be read, is empty or holds a slash stops the request before it is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public QueryParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => throw new NotImplementedException();

    /// <summary>Fills the URL's placeholder named like a member of the request with a literal.</summary>
    /// <remarks>A value that cannot be read, is empty or holds a slash stops the request before it is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public QueryParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => throw new NotImplementedException();

    /// <summary>Fills the URL's placeholder named like a member of the request with what a source reads when the request is sent.</summary>
    /// <remarks>A value that cannot be read, is empty or holds a slash stops the request before it is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public QueryParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotImplementedException();

    /// <summary>Sends a member in the URL's query, under the name the endpoint binds it by, as a source reads it when the request is sent.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public QueryParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => throw new NotImplementedException();

    /// <summary>Sends a member in the URL's query, under the name the endpoint binds it by, as a literal.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public QueryParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => throw new NotImplementedException();

    /// <summary>Sends a member in the URL's query, under the name the endpoint binds it by, as a source reads it when the request is sent.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public QueryParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotImplementedException();

    /// <summary>Sends a header with a fixed text.</summary>
    /// <remarks><c>Content-Type</c>, <c>Accept</c> and the headers the framework or the browser writes are refused where the view renders.</remarks>
    /// <param name="name">The header's name.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public QueryParts<TRequest> Header(string name, string value) => throw new NotImplementedException();

    /// <summary>Sends a header whose text a source reads when the request is sent; one that cannot be read is not sent.</summary>
    /// <remarks><c>Content-Type</c>, <c>Accept</c> and the headers the framework or the browser writes are refused where the view renders.</remarks>
    /// <param name="name">The header's name.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public QueryParts<TRequest> Header<TValue>(string name, ITypedSource<TValue> value) => throw new NotImplementedException();

    /// <summary>Starts a condition on the parts its branch sends, read once when the request is sent.</summary>
    /// <remarks>When the condition is not true, each part the branch writes is left out, as if it were never gathered, and the endpoint binds the member's start. The URL, the method and the body's format take no condition, so a branch takes no body and no container: the request names them before its first condition.</remarks>
    /// <param name="condition">The condition; the branch's parts are sent only when it is true.</param>
    /// <returns>The condition, which takes its branch with <c>Then</c>.</returns>
    public PartsGuard<QueriedParts<TRequest>> When(TypedSource<bool> condition) => throw new NotImplementedException();
}

/// <summary>
/// The parts of a request with no body after it named the container it sends: it takes no second.
/// </summary>
public sealed class QueriedParts<TRequest>
{
    internal QueriedParts()
    {
    }

    /// <summary>Fills the URL's placeholder named like a member of the request with what a source reads when the request is sent.</summary>
    /// <remarks>A value that cannot be read, is empty or holds a slash stops the request before it is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public QueriedParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => throw new NotImplementedException();

    /// <summary>Fills the URL's placeholder named like a member of the request with a literal.</summary>
    /// <remarks>A value that cannot be read, is empty or holds a slash stops the request before it is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public QueriedParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => throw new NotImplementedException();

    /// <summary>Fills the URL's placeholder named like a member of the request with what a source reads when the request is sent.</summary>
    /// <remarks>A value that cannot be read, is empty or holds a slash stops the request before it is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public QueriedParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotImplementedException();

    /// <summary>Sends a member in the URL's query, under the name the endpoint binds it by, as a source reads it when the request is sent.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public QueriedParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => throw new NotImplementedException();

    /// <summary>Sends a member in the URL's query, under the name the endpoint binds it by, as a literal.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public QueriedParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => throw new NotImplementedException();

    /// <summary>Sends a member in the URL's query, under the name the endpoint binds it by, as a source reads it when the request is sent.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public QueriedParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotImplementedException();

    /// <summary>Sends a header with a fixed text.</summary>
    /// <remarks><c>Content-Type</c>, <c>Accept</c> and the headers the framework or the browser writes are refused where the view renders.</remarks>
    /// <param name="name">The header's name.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public QueriedParts<TRequest> Header(string name, string value) => throw new NotImplementedException();

    /// <summary>Sends a header whose text a source reads when the request is sent; one that cannot be read is not sent.</summary>
    /// <remarks><c>Content-Type</c>, <c>Accept</c> and the headers the framework or the browser writes are refused where the view renders.</remarks>
    /// <param name="name">The header's name.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public QueriedParts<TRequest> Header<TValue>(string name, ITypedSource<TValue> value) => throw new NotImplementedException();

    /// <summary>Starts a condition on the parts its branch sends, read once when the request is sent.</summary>
    /// <remarks>When the condition is not true, each part the branch writes is left out, as if it were never gathered, and the endpoint binds the member's start. The URL, the method and the body's format take no condition, so a branch takes no body and no container: the request names them before its first condition.</remarks>
    /// <param name="condition">The condition; the branch's parts are sent only when it is true.</param>
    /// <returns>The condition, which takes its branch with <c>Then</c>.</returns>
    public PartsGuard<QueriedParts<TRequest>> When(TypedSource<bool> condition) => throw new NotImplementedException();
}

/// <summary>
/// The parts of a request with a body: the URL's placeholders, its query, its headers and one body.
/// </summary>
public sealed class BodyParts<TRequest>
{
    internal BodyParts()
    {
    }

    /// <summary>Fills the URL's placeholder named like a member of the request with what a source reads when the request is sent.</summary>
    /// <remarks>A value that cannot be read, is empty or holds a slash stops the request before it is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public BodyParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => throw new NotImplementedException();

    /// <summary>Fills the URL's placeholder named like a member of the request with a literal.</summary>
    /// <remarks>A value that cannot be read, is empty or holds a slash stops the request before it is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public BodyParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => throw new NotImplementedException();

    /// <summary>Fills the URL's placeholder named like a member of the request with what a source reads when the request is sent.</summary>
    /// <remarks>A value that cannot be read, is empty or holds a slash stops the request before it is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public BodyParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotImplementedException();

    /// <summary>Sends a member in the URL's query, under the name the endpoint binds it by, as a source reads it when the request is sent.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public BodyParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => throw new NotImplementedException();

    /// <summary>Sends a member in the URL's query, under the name the endpoint binds it by, as a literal.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public BodyParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => throw new NotImplementedException();

    /// <summary>Sends a member in the URL's query, under the name the endpoint binds it by, as a source reads it when the request is sent.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public BodyParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotImplementedException();

    /// <summary>Sends a header with a fixed text.</summary>
    /// <remarks><c>Content-Type</c>, <c>Accept</c> and the headers the framework or the browser writes are refused where the view renders.</remarks>
    /// <param name="name">The header's name.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public BodyParts<TRequest> Header(string name, string value) => throw new NotImplementedException();

    /// <summary>Sends a header whose text a source reads when the request is sent; one that cannot be read is not sent.</summary>
    /// <remarks><c>Content-Type</c>, <c>Accept</c> and the headers the framework or the browser writes are refused where the view renders.</remarks>
    /// <param name="name">The header's name.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public BodyParts<TRequest> Header<TValue>(string name, ITypedSource<TValue> value) => throw new NotImplementedException();

    /// <summary>Sends a JSON body: one object whose members the lambda sets.</summary>
    /// <param name="body">The body's members.</param>
    /// <returns>The request's parts, which take no second body.</returns>
    public BodiedParts<TRequest> Json(Action<JsonBody<TRequest>> body) => throw new NotImplementedException();

    /// <summary>Sends a JSON body: one object whose members the lambda sets, with the framework inputs of a container.</summary>
    /// <param name="container">The container whose framework inputs the request sends; it becomes the request's container, where its failures show.</param>
    /// <param name="body">The body's members.</param>
    /// <returns>The request's parts, which take no second body.</returns>
    public BodiedParts<TRequest> Json(RequestContainer<TRequest> container, Action<JsonBody<TRequest>> body) => throw new NotImplementedException();

    /// <summary>Sends a URL-encoded body whose fields the lambda sets.</summary>
    /// <param name="body">The body's members.</param>
    /// <returns>The request's parts, which take no second body.</returns>
    public BodiedParts<TRequest> UrlEncoded(Action<UrlEncodedBody<TRequest>> body) => throw new NotImplementedException();

    /// <summary>Sends a URL-encoded body whose fields the lambda sets, with a native form's submission set.</summary>
    /// <param name="form">The native form whose submission set the request sends; it becomes the request's container, where its failures show.</param>
    /// <param name="body">The body's members.</param>
    /// <returns>The request's parts, which take no second body.</returns>
    public BodiedParts<TRequest> UrlEncoded(RequestForm<TRequest> form, Action<UrlEncodedBody<TRequest>> body) => throw new NotImplementedException();

    /// <summary>Sends a URL-encoded body whose fields the lambda sets, with the framework inputs of a container.</summary>
    /// <param name="container">The container whose framework inputs the request sends; it becomes the request's container, where its failures show.</param>
    /// <param name="body">The body's members.</param>
    /// <returns>The request's parts, which take no second body.</returns>
    public BodiedParts<TRequest> UrlEncoded(RequestContainer<TRequest> container, Action<UrlEncodedBody<TRequest>> body) => throw new NotImplementedException();

    /// <summary>Sends a multipart body whose fields and files the lambda sets.</summary>
    /// <param name="body">The body's members.</param>
    /// <returns>The request's parts, which take no second body.</returns>
    public BodiedParts<TRequest> Multipart(Action<MultipartBody<TRequest>> body) => throw new NotImplementedException();

    /// <summary>Sends a multipart body whose fields and files the lambda sets, with a native form's submission set.</summary>
    /// <param name="form">The native form whose submission set the request sends; it becomes the request's container, where its failures show.</param>
    /// <param name="body">The body's members.</param>
    /// <returns>The request's parts, which take no second body.</returns>
    public BodiedParts<TRequest> Multipart(RequestForm<TRequest> form, Action<MultipartBody<TRequest>> body) => throw new NotImplementedException();

    /// <summary>Sends a multipart body whose fields and files the lambda sets, with the framework inputs of a container.</summary>
    /// <param name="container">The container whose framework inputs the request sends; it becomes the request's container, where its failures show.</param>
    /// <param name="body">The body's members.</param>
    /// <returns>The request's parts, which take no second body.</returns>
    public BodiedParts<TRequest> Multipart(RequestContainer<TRequest> container, Action<MultipartBody<TRequest>> body) => throw new NotImplementedException();

    /// <summary>Sends a text as the whole body.</summary>
    /// <param name="text">The text sent as the whole body.</param>
    /// <returns>The request's parts, which take no member of a body.</returns>
    public WholeBodyParts<TRequest> Text(string text) => throw new NotImplementedException();

    /// <summary>Sends the text a source reads when the request is sent as the whole body.</summary>
    /// <param name="text">The text sent as the whole body.</param>
    /// <returns>The request's parts, which take no member of a body.</returns>
    public WholeBodyParts<TRequest> Text(ITypedSource<string?> text) => throw new NotImplementedException();

    /// <summary>Sends one file as the whole body, under the file's own media type.</summary>
    /// <param name="file">The file sent.</param>
    /// <returns>The request's parts, which take no member of a body.</returns>
    public WholeBodyParts<TRequest> File(BrowserFile file) => throw new NotImplementedException();

    /// <summary>Starts a condition on the parts its branch sends, read once when the request is sent.</summary>
    /// <remarks>When the condition is not true, each part the branch writes is left out, as if it were never gathered, and the endpoint binds the member's start. The URL, the method and the body's format take no condition, so a branch takes no body and no container: the request names them before its first condition.</remarks>
    /// <param name="condition">The condition; the branch's parts are sent only when it is true.</param>
    /// <returns>The condition, which takes its branch with <c>Then</c>.</returns>
    public PartsGuard<BodiedParts<TRequest>> When(TypedSource<bool> condition) => throw new NotImplementedException();
}

/// <summary>
/// The parts of a request after its body is written: it takes no second body.
/// </summary>
public sealed class BodiedParts<TRequest>
{
    internal BodiedParts()
    {
    }

    /// <summary>Fills the URL's placeholder named like a member of the request with what a source reads when the request is sent.</summary>
    /// <remarks>A value that cannot be read, is empty or holds a slash stops the request before it is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public BodiedParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => throw new NotImplementedException();

    /// <summary>Fills the URL's placeholder named like a member of the request with a literal.</summary>
    /// <remarks>A value that cannot be read, is empty or holds a slash stops the request before it is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public BodiedParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => throw new NotImplementedException();

    /// <summary>Fills the URL's placeholder named like a member of the request with what a source reads when the request is sent.</summary>
    /// <remarks>A value that cannot be read, is empty or holds a slash stops the request before it is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public BodiedParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotImplementedException();

    /// <summary>Sends a member in the URL's query, under the name the endpoint binds it by, as a source reads it when the request is sent.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public BodiedParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => throw new NotImplementedException();

    /// <summary>Sends a member in the URL's query, under the name the endpoint binds it by, as a literal.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public BodiedParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => throw new NotImplementedException();

    /// <summary>Sends a member in the URL's query, under the name the endpoint binds it by, as a source reads it when the request is sent.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public BodiedParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotImplementedException();

    /// <summary>Sends a header with a fixed text.</summary>
    /// <remarks><c>Content-Type</c>, <c>Accept</c> and the headers the framework or the browser writes are refused where the view renders.</remarks>
    /// <param name="name">The header's name.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public BodiedParts<TRequest> Header(string name, string value) => throw new NotImplementedException();

    /// <summary>Sends a header whose text a source reads when the request is sent; one that cannot be read is not sent.</summary>
    /// <remarks><c>Content-Type</c>, <c>Accept</c> and the headers the framework or the browser writes are refused where the view renders.</remarks>
    /// <param name="name">The header's name.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public BodiedParts<TRequest> Header<TValue>(string name, ITypedSource<TValue> value) => throw new NotImplementedException();

    /// <summary>Starts a condition on the parts its branch sends, read once when the request is sent.</summary>
    /// <remarks>When the condition is not true, each part the branch writes is left out, as if it were never gathered, and the endpoint binds the member's start. The URL, the method and the body's format take no condition, so a branch takes no body and no container: the request names them before its first condition.</remarks>
    /// <param name="condition">The condition; the branch's parts are sent only when it is true.</param>
    /// <returns>The condition, which takes its branch with <c>Then</c>.</returns>
    public PartsGuard<BodiedParts<TRequest>> When(TypedSource<bool> condition) => throw new NotImplementedException();
}

/// <summary>
/// The parts of a request whose whole body is a text or a file: it takes no member of a body.
/// </summary>
public sealed class WholeBodyParts<TRequest>
{
    internal WholeBodyParts()
    {
    }

    /// <summary>Fills the URL's placeholder named like a member of the request with what a source reads when the request is sent.</summary>
    /// <remarks>A value that cannot be read, is empty or holds a slash stops the request before it is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public WholeBodyParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => throw new NotImplementedException();

    /// <summary>Fills the URL's placeholder named like a member of the request with a literal.</summary>
    /// <remarks>A value that cannot be read, is empty or holds a slash stops the request before it is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public WholeBodyParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => throw new NotImplementedException();

    /// <summary>Fills the URL's placeholder named like a member of the request with what a source reads when the request is sent.</summary>
    /// <remarks>A value that cannot be read, is empty or holds a slash stops the request before it is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public WholeBodyParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotImplementedException();

    /// <summary>Sends a member in the URL's query, under the name the endpoint binds it by, as a source reads it when the request is sent.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public WholeBodyParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => throw new NotImplementedException();

    /// <summary>Sends a member in the URL's query, under the name the endpoint binds it by, as a literal.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public WholeBodyParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => throw new NotImplementedException();

    /// <summary>Sends a member in the URL's query, under the name the endpoint binds it by, as a source reads it when the request is sent.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public WholeBodyParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotImplementedException();

    /// <summary>Sends a header with a fixed text.</summary>
    /// <remarks><c>Content-Type</c>, <c>Accept</c> and the headers the framework or the browser writes are refused where the view renders.</remarks>
    /// <param name="name">The header's name.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public WholeBodyParts<TRequest> Header(string name, string value) => throw new NotImplementedException();

    /// <summary>Sends a header whose text a source reads when the request is sent; one that cannot be read is not sent.</summary>
    /// <remarks><c>Content-Type</c>, <c>Accept</c> and the headers the framework or the browser writes are refused where the view renders.</remarks>
    /// <param name="name">The header's name.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public WholeBodyParts<TRequest> Header<TValue>(string name, ITypedSource<TValue> value) => throw new NotImplementedException();

    /// <summary>Starts a condition on the parts its branch sends, read once when the request is sent.</summary>
    /// <remarks>When the condition is not true, each part the branch writes is left out, as if it were never gathered, and the endpoint binds the member's start. The URL, the method and the body's format take no condition, so a branch takes no body and no container: the request names them before its first condition.</remarks>
    /// <param name="condition">The condition; the branch's parts are sent only when it is true.</param>
    /// <returns>The condition, which takes its branch with <c>Then</c>.</returns>
    public PartsGuard<WholeBodyParts<TRequest>> When(TypedSource<bool> condition) => throw new NotImplementedException();
}

/// <summary>
/// The members of a JSON body.
/// </summary>
public sealed class JsonBody<TRequest>
{
    internal JsonBody()
    {
    }

    /// <summary>Sends a member of the request in the body, as a source reads it when the request is sent.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The body's members.</returns>
    public JsonBody<TRequest> Set<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => throw new NotImplementedException();

    /// <summary>Sends a member of the request in the body, as a literal.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The body's members.</returns>
    public JsonBody<TRequest> Set<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => throw new NotImplementedException();

    /// <summary>Sends a member of the request in the body, as a source reads it when the request is sent.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The body's members.</returns>
    public JsonBody<TRequest> Set<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotImplementedException();

    /// <summary>Sends one input of the request's own view in the body, under a member's name.</summary>
    /// <remarks>When an input and a set member send one name, the input's value is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="input">The input of the request's own view whose value is sent.</param>
    /// <returns>The body's members.</returns>
    public JsonBody<TRequest> Input<TMember, TValue>(Expression<Func<TRequest, TMember>> member, BoundInput<TValue> input)
        where TValue : TMember
        => throw new NotImplementedException();

    /// <summary>Sends one input of the request's own view in the body, under a member's name.</summary>
    /// <remarks>When an input and a set member send one name, the input's value is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="input">The input of the request's own view whose value is sent.</param>
    /// <returns>The body's members.</returns>
    public JsonBody<TRequest> Input<TValue>(Expression<Func<TRequest, TValue?>> member, BoundInput<TValue> input)
        where TValue : struct
        => throw new NotImplementedException();

    /// <summary>Starts a condition on the parts its branch sends, read once when the request is sent.</summary>
    /// <remarks>When the condition is not true, each member the branch writes is left out, as if it were never gathered, and the endpoint binds the member's start.</remarks>
    /// <param name="condition">The condition; the branch's parts are sent only when it is true.</param>
    /// <returns>The condition, which takes its branch with <c>Then</c>.</returns>
    public PartsGuard<JsonBody<TRequest>> When(TypedSource<bool> condition) => throw new NotImplementedException();
}

/// <summary>
/// The fields of a URL-encoded body.
/// </summary>
public sealed class UrlEncodedBody<TRequest>
{
    internal UrlEncodedBody()
    {
    }

    /// <summary>Sends a member of the request in the body, as a source reads it when the request is sent.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The body's members.</returns>
    public UrlEncodedBody<TRequest> Set<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => throw new NotImplementedException();

    /// <summary>Sends a member of the request in the body, as a literal.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The body's members.</returns>
    public UrlEncodedBody<TRequest> Set<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => throw new NotImplementedException();

    /// <summary>Sends a member of the request in the body, as a source reads it when the request is sent.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The body's members.</returns>
    public UrlEncodedBody<TRequest> Set<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotImplementedException();

    /// <summary>Sends one input of the request's own view in the body, under a member's name.</summary>
    /// <remarks>When an input and a set member send one name, the input's value is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="input">The input of the request's own view whose value is sent.</param>
    /// <returns>The body's members.</returns>
    public UrlEncodedBody<TRequest> Input<TMember, TValue>(Expression<Func<TRequest, TMember>> member, BoundInput<TValue> input)
        where TValue : TMember
        => throw new NotImplementedException();

    /// <summary>Sends one input of the request's own view in the body, under a member's name.</summary>
    /// <remarks>When an input and a set member send one name, the input's value is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="input">The input of the request's own view whose value is sent.</param>
    /// <returns>The body's members.</returns>
    public UrlEncodedBody<TRequest> Input<TValue>(Expression<Func<TRequest, TValue?>> member, BoundInput<TValue> input)
        where TValue : struct
        => throw new NotImplementedException();

    /// <summary>Starts a condition on the parts its branch sends, read once when the request is sent.</summary>
    /// <remarks>When the condition is not true, each member the branch writes is left out, as if it were never gathered, and the endpoint binds the member's start.</remarks>
    /// <param name="condition">The condition; the branch's parts are sent only when it is true.</param>
    /// <returns>The condition, which takes its branch with <c>Then</c>.</returns>
    public PartsGuard<UrlEncodedBody<TRequest>> When(TypedSource<bool> condition) => throw new NotImplementedException();
}

/// <summary>
/// The fields and files of a multipart body.
/// </summary>
public sealed partial class MultipartBody<TRequest>
{
    internal MultipartBody()
    {
    }

    /// <summary>Sends a member of the request in the body, as a source reads it when the request is sent.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The body's members.</returns>
    public MultipartBody<TRequest> Set<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => throw new NotImplementedException();

    /// <summary>Sends a member of the request in the body, as a literal.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The body's members.</returns>
    public MultipartBody<TRequest> Set<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => throw new NotImplementedException();

    /// <summary>Sends a member of the request in the body, as a source reads it when the request is sent.</summary>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The body's members.</returns>
    public MultipartBody<TRequest> Set<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotImplementedException();

    /// <summary>Sends one input of the request's own view in the body, under a member's name.</summary>
    /// <remarks>When an input and a set member send one name, the input's value is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="input">The input of the request's own view whose value is sent.</param>
    /// <returns>The body's members.</returns>
    public MultipartBody<TRequest> Input<TMember, TValue>(Expression<Func<TRequest, TMember>> member, BoundInput<TValue> input)
        where TValue : TMember
        => throw new NotImplementedException();

    /// <summary>Sends one input of the request's own view in the body, under a member's name.</summary>
    /// <remarks>When an input and a set member send one name, the input's value is sent.</remarks>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="input">The input of the request's own view whose value is sent.</param>
    /// <returns>The body's members.</returns>
    public MultipartBody<TRequest> Input<TValue>(Expression<Func<TRequest, TValue?>> member, BoundInput<TValue> input)
        where TValue : struct
        => throw new NotImplementedException();

    /// <summary>Starts a condition on the parts its branch sends, read once when the request is sent.</summary>
    /// <remarks>When the condition is not true, each member the branch writes is left out, as if it were never gathered, and the endpoint binds the member's start.</remarks>
    /// <param name="condition">The condition; the branch's parts are sent only when it is true.</param>
    /// <returns>The condition, which takes its branch with <c>Then</c>.</returns>
    public PartsGuard<MultipartBody<TRequest>> When(TypedSource<bool> condition) => throw new NotImplementedException();
}
