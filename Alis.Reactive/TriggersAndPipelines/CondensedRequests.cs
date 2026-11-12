using System;
using System.Linq.Expressions;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.TriggersAndPipelines;

/// <summary>
/// The condensed doors of a request, each of which writes what the request's full form writes.
/// </summary>
public static class CondensedRequests
{
    /// <summary>Sends a member of the request, as a source reads it when the request is sent, wherever the request carries it.</summary>
    /// <remarks>A member named like a placeholder of the URL fills it. Any other goes in the query of a request with no body, and in the body of a POST, a PUT or a DELETE, which sends it as multipart unless the request names another body.</remarks>
    /// <param name="request">The request's parts.</param>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public static QueryParts<TRequest> Set<TRequest, TValue>(this QueryParts<TRequest> request, Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value)
        => throw new NotImplementedException();

    /// <summary>Sends a member of the request, as a literal, wherever the request carries it.</summary>
    /// <remarks>A member named like a placeholder of the URL fills it. Any other goes in the query of a request with no body, and in the body of a POST, a PUT or a DELETE, which sends it as multipart unless the request names another body.</remarks>
    /// <param name="request">The request's parts.</param>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public static QueryParts<TRequest> Set<TRequest, TValue>(this QueryParts<TRequest> request, Expression<Func<TRequest, TValue>> member, TValue value)
        => throw new NotImplementedException();

    /// <summary>Sends a member of the request, as a source reads it when the request is sent, wherever the request carries it.</summary>
    /// <remarks>A member named like a placeholder of the URL fills it. Any other goes in the query of a request with no body, and in the body of a POST, a PUT or a DELETE, which sends it as multipart unless the request names another body.</remarks>
    /// <param name="request">The request's parts.</param>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public static QueryParts<TRequest> Set<TRequest, TValue>(this QueryParts<TRequest> request, Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotImplementedException();

    /// <summary>Runs a pipeline when the server answers 2xx, without reading the body.</summary>
    /// <param name="request">The request.</param>
    /// <param name="success">The steps to run.</param>
    /// <returns>The request, carrying its reply's type: it takes no second success route.</returns>
    public static GetRequest<TModel, TRequest, Response<TRequest>> OnSuccess<TModel, TRequest>(this GetRequest<TModel, TRequest> request, Action<Pipeline<TModel>> success)
        => throw new NotImplementedException();

    /// <summary>Loads the HTML of a 2xx answer into an element, replacing what the element held.</summary>
    /// <remarks>What the element held is released first: its plans stop and its components leave. An answer that is not HTML runs the catch-all route.</remarks>
    /// <param name="request">The request.</param>
    /// <param name="elementId">The id of the element the HTML is loaded into.</param>
    /// <returns>The request, carrying its reply's type: it takes no second success route.</returns>
    public static GetRequest<TModel, TRequest, Response<TRequest, Markup>> Into<TModel, TRequest>(this GetRequest<TModel, TRequest> request, string elementId)
        => throw new NotImplementedException();

    /// <summary>Sends a native form's submission set as the browser builds it: a GET's in its query, any other request's as multipart fields and files.</summary>
    /// <param name="request">The request's parts.</param>
    /// <param name="form">The native form whose submission set the request sends; it becomes the request's container, where its failures show.</param>
    /// <returns>The request's parts, which take no second container.</returns>
    public static QueriedParts<TRequest> Form<TRequest>(this QueryParts<TRequest> request, RequestForm<TRequest> form) => throw new NotImplementedException();

    /// <summary>Sends the framework inputs inside a container: a GET's in its query, any other request's as multipart fields and files.</summary>
    /// <param name="request">The request's parts.</param>
    /// <param name="container">The container whose framework inputs the request sends; it becomes the request's container, where its failures show.</param>
    /// <returns>The request's parts, which take no second container.</returns>
    public static QueriedParts<TRequest> Inputs<TRequest>(this QueryParts<TRequest> request, RequestContainer<TRequest> container) => throw new NotImplementedException();

    /// <summary>Sends a member of the request, as a source reads it when the request is sent, wherever the request carries it.</summary>
    /// <remarks>A member named like a placeholder of the URL fills it. Any other goes in the query of a request with no body, and in the body of a POST, a PUT or a DELETE, which sends it as multipart unless the request names another body.</remarks>
    /// <param name="request">The request's parts.</param>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public static QueriedParts<TRequest> Set<TRequest, TValue>(this QueriedParts<TRequest> request, Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value)
        => throw new NotImplementedException();

    /// <summary>Sends a member of the request, as a literal, wherever the request carries it.</summary>
    /// <remarks>A member named like a placeholder of the URL fills it. Any other goes in the query of a request with no body, and in the body of a POST, a PUT or a DELETE, which sends it as multipart unless the request names another body.</remarks>
    /// <param name="request">The request's parts.</param>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public static QueriedParts<TRequest> Set<TRequest, TValue>(this QueriedParts<TRequest> request, Expression<Func<TRequest, TValue>> member, TValue value)
        => throw new NotImplementedException();

    /// <summary>Sends a member of the request, as a source reads it when the request is sent, wherever the request carries it.</summary>
    /// <remarks>A member named like a placeholder of the URL fills it. Any other goes in the query of a request with no body, and in the body of a POST, a PUT or a DELETE, which sends it as multipart unless the request names another body.</remarks>
    /// <param name="request">The request's parts.</param>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public static QueriedParts<TRequest> Set<TRequest, TValue>(this QueriedParts<TRequest> request, Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotImplementedException();

    /// <summary>Sends one input of the request's own view under a member's name, placed as a set member is.</summary>
    /// <remarks>When an input and a set member send one name, the input's value is sent.</remarks>
    /// <param name="request">The request's parts.</param>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="input">The input of the request's own view whose value is sent.</param>
    /// <returns>The request's parts.</returns>
    public static QueriedParts<TRequest> Input<TRequest, TMember, TValue>(this QueriedParts<TRequest> request, Expression<Func<TRequest, TMember>> member, BoundInput<TValue> input)
        where TValue : TMember
        => throw new NotImplementedException();

    /// <summary>Sends one input of the request's own view under a member's name, placed as a set member is.</summary>
    /// <remarks>When an input and a set member send one name, the input's value is sent.</remarks>
    /// <param name="request">The request's parts.</param>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="input">The input of the request's own view whose value is sent.</param>
    /// <returns>The request's parts.</returns>
    public static QueriedParts<TRequest> Input<TRequest, TValue>(this QueriedParts<TRequest> request, Expression<Func<TRequest, TValue?>> member, BoundInput<TValue> input)
        where TValue : struct
        => throw new NotImplementedException();

    /// <summary>Sends one input of the request's own view under a member's name, placed as a set member is.</summary>
    /// <remarks>When an input and a set member send one name, the input's value is sent.</remarks>
    /// <param name="request">The request's parts.</param>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="input">The input of the request's own view whose value is sent.</param>
    /// <returns>The request's parts.</returns>
    public static QueryParts<TRequest> Input<TRequest, TMember, TValue>(this QueryParts<TRequest> request, Expression<Func<TRequest, TMember>> member, BoundInput<TValue> input)
        where TValue : TMember
        => throw new NotImplementedException();

    /// <summary>Sends one input of the request's own view under a member's name, placed as a set member is.</summary>
    /// <remarks>When an input and a set member send one name, the input's value is sent.</remarks>
    /// <param name="request">The request's parts.</param>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="input">The input of the request's own view whose value is sent.</param>
    /// <returns>The request's parts.</returns>
    public static QueryParts<TRequest> Input<TRequest, TValue>(this QueryParts<TRequest> request, Expression<Func<TRequest, TValue?>> member, BoundInput<TValue> input)
        where TValue : struct
        => throw new NotImplementedException();

    /// <summary>Sends a member of the request, as a source reads it when the request is sent, wherever the request carries it.</summary>
    /// <remarks>A member named like a placeholder of the URL fills it. Any other goes in the query of a request with no body, and in the body of a POST, a PUT or a DELETE, which sends it as multipart unless the request names another body.</remarks>
    /// <param name="request">The request's parts.</param>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public static BodiedParts<TRequest> Set<TRequest, TValue>(this BodyParts<TRequest> request, Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value)
        => throw new NotImplementedException();

    /// <summary>Sends a member of the request, as a literal, wherever the request carries it.</summary>
    /// <remarks>A member named like a placeholder of the URL fills it. Any other goes in the query of a request with no body, and in the body of a POST, a PUT or a DELETE, which sends it as multipart unless the request names another body.</remarks>
    /// <param name="request">The request's parts.</param>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public static BodiedParts<TRequest> Set<TRequest, TValue>(this BodyParts<TRequest> request, Expression<Func<TRequest, TValue>> member, TValue value)
        => throw new NotImplementedException();

    /// <summary>Sends a member of the request, as a source reads it when the request is sent, wherever the request carries it.</summary>
    /// <remarks>A member named like a placeholder of the URL fills it. Any other goes in the query of a request with no body, and in the body of a POST, a PUT or a DELETE, which sends it as multipart unless the request names another body.</remarks>
    /// <param name="request">The request's parts.</param>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public static BodiedParts<TRequest> Set<TRequest, TValue>(this BodyParts<TRequest> request, Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotImplementedException();

    /// <summary>Sends a native form's submission set as the browser builds it: a GET's in its query, any other request's as multipart fields and files.</summary>
    /// <param name="request">The request's parts.</param>
    /// <param name="form">The native form whose submission set the request sends; it becomes the request's container, where its failures show.</param>
    /// <returns>The request's parts, which take no second container.</returns>
    public static BodiedParts<TRequest> Form<TRequest>(this BodyParts<TRequest> request, RequestForm<TRequest> form) => throw new NotImplementedException();

    /// <summary>Sends the framework inputs inside a container: a GET's in its query, any other request's as multipart fields and files.</summary>
    /// <param name="request">The request's parts.</param>
    /// <param name="container">The container whose framework inputs the request sends; it becomes the request's container, where its failures show.</param>
    /// <returns>The request's parts, which take no second container.</returns>
    public static BodiedParts<TRequest> Inputs<TRequest>(this BodyParts<TRequest> request, RequestContainer<TRequest> container) => throw new NotImplementedException();

    /// <summary>Sends one input of the request's own view under a member's name, placed as a set member is.</summary>
    /// <remarks>When an input and a set member send one name, the input's value is sent.</remarks>
    /// <param name="request">The request's parts.</param>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="input">The input of the request's own view whose value is sent.</param>
    /// <returns>The request's parts.</returns>
    public static BodiedParts<TRequest> Input<TRequest, TMember, TValue>(this BodyParts<TRequest> request, Expression<Func<TRequest, TMember>> member, BoundInput<TValue> input)
        where TValue : TMember
        => throw new NotImplementedException();

    /// <summary>Sends one input of the request's own view under a member's name, placed as a set member is.</summary>
    /// <remarks>When an input and a set member send one name, the input's value is sent.</remarks>
    /// <param name="request">The request's parts.</param>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="input">The input of the request's own view whose value is sent.</param>
    /// <returns>The request's parts.</returns>
    public static BodiedParts<TRequest> Input<TRequest, TValue>(this BodyParts<TRequest> request, Expression<Func<TRequest, TValue?>> member, BoundInput<TValue> input)
        where TValue : struct
        => throw new NotImplementedException();

    /// <summary>Sends a member of the request, as a source reads it when the request is sent, wherever the request carries it.</summary>
    /// <remarks>A member named like a placeholder of the URL fills it. Any other goes in the query of a request with no body, and in the body of a POST, a PUT or a DELETE, which sends it as multipart unless the request names another body.</remarks>
    /// <param name="request">The request's parts.</param>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public static BodiedParts<TRequest> Set<TRequest, TValue>(this BodiedParts<TRequest> request, Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value)
        => throw new NotImplementedException();

    /// <summary>Sends a member of the request, as a literal, wherever the request carries it.</summary>
    /// <remarks>A member named like a placeholder of the URL fills it. Any other goes in the query of a request with no body, and in the body of a POST, a PUT or a DELETE, which sends it as multipart unless the request names another body.</remarks>
    /// <param name="request">The request's parts.</param>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public static BodiedParts<TRequest> Set<TRequest, TValue>(this BodiedParts<TRequest> request, Expression<Func<TRequest, TValue>> member, TValue value)
        => throw new NotImplementedException();

    /// <summary>Sends a member of the request, as a source reads it when the request is sent, wherever the request carries it.</summary>
    /// <remarks>A member named like a placeholder of the URL fills it. Any other goes in the query of a request with no body, and in the body of a POST, a PUT or a DELETE, which sends it as multipart unless the request names another body.</remarks>
    /// <param name="request">The request's parts.</param>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="value">The value sent: a literal written where the view renders, or a source read when the request is sent.</param>
    /// <returns>The request's parts.</returns>
    public static BodiedParts<TRequest> Set<TRequest, TValue>(this BodiedParts<TRequest> request, Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotImplementedException();

    /// <summary>Sends one input of the request's own view under a member's name, placed as a set member is.</summary>
    /// <remarks>When an input and a set member send one name, the input's value is sent.</remarks>
    /// <param name="request">The request's parts.</param>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="input">The input of the request's own view whose value is sent.</param>
    /// <returns>The request's parts.</returns>
    public static BodiedParts<TRequest> Input<TRequest, TMember, TValue>(this BodiedParts<TRequest> request, Expression<Func<TRequest, TMember>> member, BoundInput<TValue> input)
        where TValue : TMember
        => throw new NotImplementedException();

    /// <summary>Sends one input of the request's own view under a member's name, placed as a set member is.</summary>
    /// <remarks>When an input and a set member send one name, the input's value is sent.</remarks>
    /// <param name="request">The request's parts.</param>
    /// <param name="member">The member of the request, as a member path.</param>
    /// <param name="input">The input of the request's own view whose value is sent.</param>
    /// <returns>The request's parts.</returns>
    public static BodiedParts<TRequest> Input<TRequest, TValue>(this BodiedParts<TRequest> request, Expression<Func<TRequest, TValue?>> member, BoundInput<TValue> input)
        where TValue : struct
        => throw new NotImplementedException();

    /// <summary>Saves the body of a 2xx answer as the browser's download.</summary>
    /// <param name="request">The request.</param>
    /// <returns>The request, carrying its reply's type: it takes no second success route.</returns>
    public static GetRequest<TModel, TRequest, Response<TRequest, ResponseFile>> Download<TModel, TRequest>(this GetRequest<TModel, TRequest> request)
        => throw new NotImplementedException();

    /// <summary>Runs a pipeline when the server answers 2xx, without reading the body.</summary>
    /// <param name="request">The request.</param>
    /// <param name="success">The steps to run.</param>
    /// <returns>The request, carrying its reply's type: it takes no second success route.</returns>
    public static HeadRequest<TModel, TRequest, Response<TRequest>> OnSuccess<TModel, TRequest>(this HeadRequest<TModel, TRequest> request, Action<Pipeline<TModel>> success)
        => throw new NotImplementedException();

    /// <summary>Runs a pipeline when the server answers 2xx, without reading the body.</summary>
    /// <param name="request">The request.</param>
    /// <param name="success">The steps to run.</param>
    /// <returns>The request, carrying its reply's type: it takes no second success route.</returns>
    public static OptionsRequest<TModel, TRequest, Response<TRequest>> OnSuccess<TModel, TRequest>(this OptionsRequest<TModel, TRequest> request, Action<Pipeline<TModel>> success)
        => throw new NotImplementedException();

    /// <summary>Loads the HTML of a 2xx answer into an element, replacing what the element held.</summary>
    /// <remarks>What the element held is released first: its plans stop and its components leave. An answer that is not HTML runs the catch-all route.</remarks>
    /// <param name="request">The request.</param>
    /// <param name="elementId">The id of the element the HTML is loaded into.</param>
    /// <returns>The request, carrying its reply's type: it takes no second success route.</returns>
    public static OptionsRequest<TModel, TRequest, Response<TRequest, Markup>> Into<TModel, TRequest>(this OptionsRequest<TModel, TRequest> request, string elementId)
        => throw new NotImplementedException();

    /// <summary>Saves the body of a 2xx answer as the browser's download.</summary>
    /// <param name="request">The request.</param>
    /// <returns>The request, carrying its reply's type: it takes no second success route.</returns>
    public static OptionsRequest<TModel, TRequest, Response<TRequest, ResponseFile>> Download<TModel, TRequest>(this OptionsRequest<TModel, TRequest> request)
        => throw new NotImplementedException();

    /// <summary>Runs a pipeline when the server answers 2xx, without reading the body.</summary>
    /// <param name="request">The request.</param>
    /// <param name="success">The steps to run.</param>
    /// <returns>The request, carrying its reply's type: it takes no second success route.</returns>
    public static PostRequest<TModel, TRequest, Response<TRequest>> OnSuccess<TModel, TRequest>(this PostRequest<TModel, TRequest> request, Action<Pipeline<TModel>> success)
        => throw new NotImplementedException();

    /// <summary>Loads the HTML of a 2xx answer into an element, replacing what the element held.</summary>
    /// <remarks>What the element held is released first: its plans stop and its components leave. An answer that is not HTML runs the catch-all route.</remarks>
    /// <param name="request">The request.</param>
    /// <param name="elementId">The id of the element the HTML is loaded into.</param>
    /// <returns>The request, carrying its reply's type: it takes no second success route.</returns>
    public static PostRequest<TModel, TRequest, Response<TRequest, Markup>> Into<TModel, TRequest>(this PostRequest<TModel, TRequest> request, string elementId)
        => throw new NotImplementedException();

    /// <summary>Saves the body of a 2xx answer as the browser's download.</summary>
    /// <param name="request">The request.</param>
    /// <returns>The request, carrying its reply's type: it takes no second success route.</returns>
    public static PostRequest<TModel, TRequest, Response<TRequest, ResponseFile>> Download<TModel, TRequest>(this PostRequest<TModel, TRequest> request)
        => throw new NotImplementedException();

    /// <summary>Runs a pipeline when the server answers 2xx, without reading the body.</summary>
    /// <param name="request">The request.</param>
    /// <param name="success">The steps to run.</param>
    /// <returns>The request, carrying its reply's type: it takes no second success route.</returns>
    public static PutRequest<TModel, TRequest, Response<TRequest>> OnSuccess<TModel, TRequest>(this PutRequest<TModel, TRequest> request, Action<Pipeline<TModel>> success)
        => throw new NotImplementedException();

    /// <summary>Loads the HTML of a 2xx answer into an element, replacing what the element held.</summary>
    /// <remarks>What the element held is released first: its plans stop and its components leave. An answer that is not HTML runs the catch-all route.</remarks>
    /// <param name="request">The request.</param>
    /// <param name="elementId">The id of the element the HTML is loaded into.</param>
    /// <returns>The request, carrying its reply's type: it takes no second success route.</returns>
    public static PutRequest<TModel, TRequest, Response<TRequest, Markup>> Into<TModel, TRequest>(this PutRequest<TModel, TRequest> request, string elementId)
        => throw new NotImplementedException();

    /// <summary>Saves the body of a 2xx answer as the browser's download.</summary>
    /// <param name="request">The request.</param>
    /// <returns>The request, carrying its reply's type: it takes no second success route.</returns>
    public static PutRequest<TModel, TRequest, Response<TRequest, ResponseFile>> Download<TModel, TRequest>(this PutRequest<TModel, TRequest> request)
        => throw new NotImplementedException();

    /// <summary>Runs a pipeline when the server answers 2xx, without reading the body.</summary>
    /// <param name="request">The request.</param>
    /// <param name="success">The steps to run.</param>
    /// <returns>The request, carrying its reply's type: it takes no second success route.</returns>
    public static DeleteRequest<TModel, TRequest, Response<TRequest>> OnSuccess<TModel, TRequest>(this DeleteRequest<TModel, TRequest> request, Action<Pipeline<TModel>> success)
        => throw new NotImplementedException();

    /// <summary>Loads the HTML of a 2xx answer into an element, replacing what the element held.</summary>
    /// <remarks>What the element held is released first: its plans stop and its components leave. An answer that is not HTML runs the catch-all route.</remarks>
    /// <param name="request">The request.</param>
    /// <param name="elementId">The id of the element the HTML is loaded into.</param>
    /// <returns>The request, carrying its reply's type: it takes no second success route.</returns>
    public static DeleteRequest<TModel, TRequest, Response<TRequest, Markup>> Into<TModel, TRequest>(this DeleteRequest<TModel, TRequest> request, string elementId)
        => throw new NotImplementedException();

    /// <summary>Saves the body of a 2xx answer as the browser's download.</summary>
    /// <param name="request">The request.</param>
    /// <returns>The request, carrying its reply's type: it takes no second success route.</returns>
    public static DeleteRequest<TModel, TRequest, Response<TRequest, ResponseFile>> Download<TModel, TRequest>(this DeleteRequest<TModel, TRequest> request)
        => throw new NotImplementedException();

    /// <summary>Reaches the page's notice, to show a text as a success, an error, a warning or information.</summary>
    /// <param name="pipeline">The pipeline the step is written in.</param>
    /// <returns>The notice.</returns>
    public static Notice Notice<TModel>(this Pipeline<TModel> pipeline) => throw new NotImplementedException();
}
