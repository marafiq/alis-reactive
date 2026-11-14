using System;
using System.Linq.Expressions;
using Alis.Reactive.Declarations;
using Alis.Reactive.Execution;
using Alis.Reactive.Plugins;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Schema;
using Alis.Reactive.ValuesAndSources;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.TriggersAndPipelines;

/// <summary>
/// The steps the browser runs when a handler starts: each door writes one step, in the order the view writes them.
/// </summary>
/// <remarks>
/// A step runs after the step before it has finished, a request and an awaited method included. A step that
/// meets something the page lacks, such as an element or a required member, stops the pipeline there.
/// </remarks>
/// <typeparam name="TModel">The model of the view that writes the pipeline.</typeparam>
public sealed class Pipeline<TModel>
{
    private readonly BlockWriter _writer;

    internal Pipeline(BlockWriter writer)
    {
        _writer = writer;
    }

    /// <summary>Reaches the component an element holds, to read, set, call and listen to its declared members.</summary>
    /// <param name="id">The id the view gave the component's element.</param>
    /// <typeparam name="TComponent">The component's declaration.</typeparam>
    /// <returns>The component.</returns>
    public TComponent Component<TComponent>(string id)
        where TComponent : Component, new()
    {
        var component = new TComponent();
        var vendor = component.Vendor;
        _writer.Content.Uses(vendor.Name, vendor.Grammar);
        _writer.Content.Declares(id, new ElementLocator(id, vendor.Name));
        component.Reach(new Reached(new ObjExpression(id), _writer));
        return component;
    }

    /// <summary>Reaches an app-level object on the element its declaration fixes, to read, set, call and listen to its declared members.</summary>
    /// <remarks>An element the page lacks, or a member its object lacks, stops the pipeline that reaches it.</remarks>
    /// <typeparam name="TComponent">The object's declaration.</typeparam>
    /// <returns>The object.</returns>
    public TComponent Component<TComponent>()
        where TComponent : Component, IAppLevelComponent, new()
        => throw new NotImplementedException();

    /// <summary>Reaches a plugin the page registers, to read, set, call and listen to its declared members.</summary>
    /// <typeparam name="TPlugin">The plugin's declaration.</typeparam>
    /// <returns>The plugin.</returns>
    public TPlugin Plugin<TPlugin>()
        where TPlugin : Plugin, new()
        => throw new NotImplementedException();

    /// <summary>Names the input a member of the view's model binds, for a request to send or a step to reach its component.</summary>
    /// <param name="member">The member the input binds, as a member path.</param>
    /// <returns>The input, typed by the member's value.</returns>
    public BoundInput<TValue> Input<TValue>(Expression<Func<TModel, TValue>> member) => throw new NotImplementedException();

    /// <summary>Raises an app event that carries no payload.</summary>
    /// <remarks>Each handler of the event runs up to its first awaited step before the step after this one runs.</remarks>
    /// <param name="appEvent">The event, as the app declares it.</param>
    public void Dispatch(AppEvent appEvent) => throw new NotImplementedException();

    /// <summary>Raises an app event with a payload the view writes.</summary>
    /// <typeparam name="TPayload">The class of the payload.</typeparam>
    /// <param name="appEvent">The event, as the app declares it.</param>
    /// <param name="payload">The payload, written into the plan where the view renders.</param>
    public void Dispatch<TPayload>(AppEvent<TPayload> appEvent, TPayload payload) => throw new NotImplementedException();

    /// <summary>Raises an app event with a payload read when the step runs.</summary>
    /// <typeparam name="TPayload">The class of the payload.</typeparam>
    /// <param name="appEvent">The event, as the app declares it.</param>
    /// <param name="payload">The source the payload is read from.</param>
    public void Dispatch<TPayload>(AppEvent<TPayload> appEvent, TypedSource<TPayload> payload) => throw new NotImplementedException();

    /// <summary>Starts a condition, read when the pipeline reaches it.</summary>
    /// <param name="condition">The condition; its branch runs only when it is true.</param>
    /// <returns>The condition, which takes its branch with <c>Then</c>.</returns>
    public Guard<TModel> When(TypedSource<bool> condition) => throw new NotImplementedException();

    /// <summary>Sends a GET request with no member.</summary>
    /// <param name="url">The URL: a path on the page's origin, or an absolute http or https URI.</param>
    /// <returns>The request, which takes its routes.</returns>
    public GetRequest<TModel, NoModel> Get(string url) => throw new NotImplementedException();

    /// <summary>Sends a GET request whose members fill the URL's placeholders and its query.</summary>
    /// <param name="url">The URL: a path on the page's origin, or an absolute http or https URI, with a placeholder in braces for each member its path takes.</param>
    /// <param name="request">The request's parts.</param>
    /// <typeparam name="TRequest">The type the endpoint binds the request to.</typeparam>
    /// <returns>The request, which takes its routes.</returns>
    public GetRequest<TModel, TRequest> Get<TRequest>(string url, Action<QueryParts<TRequest>> request) => throw new NotImplementedException();

    /// <summary>Sends a HEAD request with no member.</summary>
    /// <param name="url">The URL: a path on the page's origin, or an absolute http or https URI.</param>
    /// <returns>The request, which takes its routes.</returns>
    public HeadRequest<TModel, NoModel> Head(string url) => throw new NotImplementedException();

    /// <summary>Sends a HEAD request whose members fill the URL's placeholders and its query.</summary>
    /// <param name="url">The URL: a path on the page's origin, or an absolute http or https URI, with a placeholder in braces for each member its path takes.</param>
    /// <param name="request">The request's parts.</param>
    /// <typeparam name="TRequest">The type the endpoint binds the request to.</typeparam>
    /// <returns>The request, which takes its routes.</returns>
    public HeadRequest<TModel, TRequest> Head<TRequest>(string url, Action<QueryParts<TRequest>> request) => throw new NotImplementedException();

    /// <summary>Sends an OPTIONS request with no member.</summary>
    /// <param name="url">The URL: a path on the page's origin, or an absolute http or https URI.</param>
    /// <returns>The request, which takes its routes.</returns>
    public OptionsRequest<TModel, NoModel> Options(string url) => throw new NotImplementedException();

    /// <summary>Sends an OPTIONS request whose members fill the URL's placeholders and its query.</summary>
    /// <param name="url">The URL: a path on the page's origin, or an absolute http or https URI, with a placeholder in braces for each member its path takes.</param>
    /// <param name="request">The request's parts.</param>
    /// <typeparam name="TRequest">The type the endpoint binds the request to.</typeparam>
    /// <returns>The request, which takes its routes.</returns>
    public OptionsRequest<TModel, TRequest> Options<TRequest>(string url, Action<QueryParts<TRequest>> request) => throw new NotImplementedException();

    /// <summary>Sends a POST request with no member.</summary>
    /// <param name="url">The URL: a path on the page's origin, or an absolute http or https URI.</param>
    /// <returns>The request, which takes its routes.</returns>
    public PostRequest<TModel, NoModel> Post(string url) => throw new NotImplementedException();

    /// <summary>Sends a POST request whose members fill the URL's placeholders, its query and its body.</summary>
    /// <param name="url">The URL: a path on the page's origin, or an absolute http or https URI, with a placeholder in braces for each member its path takes.</param>
    /// <param name="request">The request's parts.</param>
    /// <typeparam name="TRequest">The type the endpoint binds the request to.</typeparam>
    /// <returns>The request, which takes its routes.</returns>
    public PostRequest<TModel, TRequest> Post<TRequest>(string url, Action<BodyParts<TRequest>> request) => throw new NotImplementedException();

    /// <summary>Sends a PUT request with no member.</summary>
    /// <param name="url">The URL: a path on the page's origin, or an absolute http or https URI.</param>
    /// <returns>The request, which takes its routes.</returns>
    public PutRequest<TModel, NoModel> Put(string url) => throw new NotImplementedException();

    /// <summary>Sends a PUT request whose members fill the URL's placeholders, its query and its body.</summary>
    /// <param name="url">The URL: a path on the page's origin, or an absolute http or https URI, with a placeholder in braces for each member its path takes.</param>
    /// <param name="request">The request's parts.</param>
    /// <typeparam name="TRequest">The type the endpoint binds the request to.</typeparam>
    /// <returns>The request, which takes its routes.</returns>
    public PutRequest<TModel, TRequest> Put<TRequest>(string url, Action<BodyParts<TRequest>> request) => throw new NotImplementedException();

    /// <summary>Sends a DELETE request with no member.</summary>
    /// <param name="url">The URL: a path on the page's origin, or an absolute http or https URI.</param>
    /// <returns>The request, which takes its routes.</returns>
    public DeleteRequest<TModel, NoModel> Delete(string url) => throw new NotImplementedException();

    /// <summary>Sends a DELETE request whose members fill the URL's placeholders, its query and its body.</summary>
    /// <param name="url">The URL: a path on the page's origin, or an absolute http or https URI, with a placeholder in braces for each member its path takes.</param>
    /// <param name="request">The request's parts.</param>
    /// <typeparam name="TRequest">The type the endpoint binds the request to.</typeparam>
    /// <returns>The request, which takes its routes.</returns>
    public DeleteRequest<TModel, TRequest> Delete<TRequest>(string url, Action<BodyParts<TRequest>> request) => throw new NotImplementedException();

    /// <summary>Builds an object from sources when the step runs, member by member.</summary>
    /// <remarks>The object is a source wherever one of its class goes: an app event's payload, an argument, a property.</remarks>
    /// <param name="members">The members the object holds, each a literal or a source.</param>
    /// <typeparam name="T">The class of the object built.</typeparam>
    /// <returns>A source of the object.</returns>
    public TypedSource<T> Object<T>(Action<ObjectMembers<T>> members) => throw new NotImplementedException();

    /// <summary>Reaches the page's own URL, read as the action that rendered the page bound it.</summary>
    /// <typeparam name="TPage">The type the page's action binds its URL to.</typeparam>
    /// <returns>The URL, whose members are sources.</returns>
    public PageUrl<TPage> PageUrl<TPage>() => throw new NotImplementedException();

    /// <summary>Reaches the page's own URL, read by a template that names the members its path holds.</summary>
    /// <param name="template">The page's route, with a placeholder in braces for each member its path holds.</param>
    /// <typeparam name="TPage">The type the page's action binds its URL to.</typeparam>
    /// <returns>The URL, whose members are sources.</returns>
    public PageUrl<TPage> PageUrl<TPage>(string template) => throw new NotImplementedException();

    /// <summary>Names a request container another view rendered, by its element's id.</summary>
    /// <param name="containerId">The id of the container's element.</param>
    /// <typeparam name="TContainer">The model the container's block was rendered for.</typeparam>
    /// <returns>The container, which a request sends and a step validates.</returns>
    public RequestContainer<TContainer> Container<TContainer>(string containerId) => throw new NotImplementedException();

    /// <summary>Unloads what a request loaded into an element: its plans stop, its components leave and the element empties.</summary>
    /// <param name="elementId">The id of the element a request loaded HTML into.</param>
    public void Unload(string elementId) => throw new NotImplementedException();

    /// <summary>Listens to an event of an object the pipeline reaches, such as a component an event's payload hands over, from this step on.</summary>
    /// <remarks>
    /// The listener is added once for each object, when the step runs, and is removed when the plan that wrote it
    /// leaves the page.
    /// </remarks>
    /// <param name="source">The object whose event is listened to.</param>
    /// <param name="raised">The event, as a member of the object's declaration.</param>
    /// <param name="pipeline">The steps to run on each raise, with the event's payload.</param>
    public void Listen<TObject, TArgs>(TObject source, Func<TObject, Event<TArgs>> raised, Action<TArgs, Pipeline<TModel>> pipeline)
        where TObject : EventedObject
        where TArgs : JsObject, new()
        => throw new NotImplementedException();

    /// <summary>Listens to an event of an object the pipeline reaches, from this step on, under a policy for a raise that comes while its last run is in flight.</summary>
    /// <remarks>
    /// The listener is added once for each object, when the step runs, and is removed when the plan that wrote it
    /// leaves the page.
    /// </remarks>
    /// <param name="source">The object whose event is listened to.</param>
    /// <param name="raised">The event, as a member of the object's declaration.</param>
    /// <param name="policy">What a raise does while the run the last raise started is in flight.</param>
    /// <param name="pipeline">The steps to run on each raise, with the event's payload.</param>
    public void Listen<TObject, TArgs>(TObject source, Func<TObject, Event<TArgs>> raised, HandlerPolicy policy, Action<TArgs, Pipeline<TModel>> pipeline)
        where TObject : EventedObject
        where TArgs : JsObject, new()
        => throw new NotImplementedException();
}

/// <summary>
/// The input a member of the view's model binds, typed by the member's value.
/// </summary>
/// <typeparam name="TValue">The type of the member the input binds.</typeparam>
public sealed class BoundInput<TValue>
{
    internal BoundInput()
    {
    }

    /// <summary>Reaches the component that renders the input, by the id the framework gave it.</summary>
    /// <typeparam name="TComponent">The component's declaration.</typeparam>
    /// <returns>The component.</returns>
    public TComponent Component<TComponent>()
        where TComponent : Component, new()
        => throw new NotImplementedException();
}

/// <summary>
/// The model of a pipeline that names no member of a view: a request default's, and a request's that sends no member.
/// </summary>
public sealed class NoModel
{
    private NoModel()
    {
    }
}
