using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Grammar;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Builds the ordered reactions that execute when a trigger fires: element updates,
    /// event dispatches, HTTP calls, component interactions, and conditional logic.
    /// </summary>
    /// <remarks>
    /// Received as the <c>p</c> parameter inside trigger callbacks:
    /// <c>t.DomReady(p =&gt; { p.Element("id").ClassList.Add("x"); p.Dispatch(CareEvents.Ready); })</c>.
    /// Reactions execute in declaration order.
    /// </remarks>
    /// <typeparam name="TModel">View model that owns model-bound component IDs and validation/gather fields.</typeparam>
    public partial class PipelineBuilder<TModel> where TModel : class
    {
        internal PlanBuildContext Context { get; }

        internal PipelineBuilder(PlanBuildContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Builds a callback's steps. A step lands in the callback being built, whichever pipeline variable
        /// or handle wrote it: a handle declared in a trigger and used inside OnSuccess writes into OnSuccess.
        /// </summary>
        internal static Block Build(PlanBuildContext context, Action<PipelineBuilder<TModel>> reactions)
        {
            if (reactions == null) throw new ArgumentNullException(nameof(reactions));
            var pipeline = new PipelineBuilder<TModel>(context);
            return context.Build(() => reactions(pipeline));
        }

        /// <summary>Raises a declared document event with no payload.</summary>
        /// <param name="appEvent">The event, declared once with <see cref="AppEvent.Declare(string, string)"/>.</param>
        /// <returns>This pipeline builder; chained reactions run after the dispatch.</returns>
        public PipelineBuilder<TModel> Dispatch(AppEvent appEvent)
        {
            if (appEvent == null) throw new ArgumentNullException(nameof(appEvent));
            Context.AddStep(new DispatchStep(appEvent.Name, NoPayload));
            return this;
        }

        /// <summary>Raises a declared document event without its payload: a listener reads every member as missing.</summary>
        /// <typeparam name="TPayload">The payload the event declares.</typeparam>
        /// <param name="appEvent">The event, declared once with <see cref="AppEvent.Declare{TPayload}(string, string)"/>.</param>
        /// <returns>This pipeline builder; chained reactions run after the dispatch.</returns>
        public PipelineBuilder<TModel> Dispatch<TPayload>(AppEvent<TPayload> appEvent)
            where TPayload : class, new()
        {
            if (appEvent == null) throw new ArgumentNullException(nameof(appEvent));
            Context.AddStep(new DispatchStep(appEvent.Name, NoPayload));
            return this;
        }

        // An event raised with nothing: a listener reads every member of the detail as missing.
        private static readonly ValueExpression NoPayload = ValueExpression.Record(Array.Empty<(string Name, ValueExpression Value)>());

        /// <summary>Raises a declared document event with a payload fixed when the view renders.</summary>
        /// <typeparam name="TPayload">The payload the event declares.</typeparam>
        /// <param name="appEvent">The event, declared once with <see cref="AppEvent.Declare{TPayload}(string, string)"/>.</param>
        /// <param name="payload">The payload, written into the plan.</param>
        /// <returns>This pipeline builder; chained reactions run after the dispatch.</returns>
        public PipelineBuilder<TModel> Dispatch<TPayload>(AppEvent<TPayload> appEvent, TPayload payload)
            where TPayload : class, new()
        {
            if (appEvent == null) throw new ArgumentNullException(nameof(appEvent));
            Context.AddStep(new DispatchStep(
                appEvent.Name,
                ValueExpression.LiteralFor(payload, Shape.FromClrType(typeof(TPayload)))));
            return this;
        }

        /// <summary>Raises a declared document event whose payload fields come from value sources.</summary>
        /// <remarks>
        /// Use this when the payload needs the page's current state, such as component values or URL
        /// parameters; use <see cref="Dispatch{TPayload}(AppEvent{TPayload}, TPayload)"/> for a payload
        /// fixed when the view renders.
        /// </remarks>
        /// <typeparam name="TPayload">The payload the event declares.</typeparam>
        /// <param name="appEvent">The event, declared once with <see cref="AppEvent.Declare{TPayload}(string, string)"/>.</param>
        /// <param name="configure">Maps payload fields to value sources or literals.</param>
        /// <returns>This pipeline builder; chained reactions run after the dispatch.</returns>
        public PipelineBuilder<TModel> Dispatch<TPayload>(
            AppEvent<TPayload> appEvent,
            Action<DispatchPayloadBuilder<TPayload, TModel>> configure)
            where TPayload : class, new()
        {
            if (appEvent == null) throw new ArgumentNullException(nameof(appEvent));
            var builder = new DispatchPayloadBuilder<TPayload, TModel>();
            configure(builder);
            Context.AddStep(new DispatchStep(appEvent.Name, builder.Build()));
            return this;
        }

        /// <summary>The element the view rendered with this id: its text, HTML, classes and whether it shows.</summary>
        /// <param name="elementId">The element's id.</param>
        /// <example><c>p.Element("status").Text.Set("Saved")</c></example>
        public HtmlElement Element(string elementId) => new HtmlElement(Context, elementId);

        /// <summary>The component the view rendered for a member of this plan's model.</summary>
        /// <typeparam name="TComponent">The component's declaration, whose members the page uses directly.</typeparam>
        /// <param name="expr">
        /// The model member the component is bound to; its id is derived as the input field derives it, under
        /// the view's field prefix, so a partial rendered for a member of the page's model reaches its own input.
        /// </param>
        /// <returns>The component: <c>rate.Min.Set(0m)</c>, <c>rate.Increment()</c>, <c>p.When(rate.Value).Gt(1m)</c>.</returns>
        public TComponent Component<TComponent>(Expression<Func<TModel, object?>> expr)
            where TComponent : JsObject, IComponent, new() =>
            Live<TComponent>(IdGenerator.For(typeof(TModel), HtmlFieldPrefix.Join(Context.FieldPrefix, ExpressionPathHelper.ToMemberName(expr.Body))));

        /// <summary>
        /// The component a view of another model rendered for one of its members with no field prefix, such as
        /// a partial rendered at the top level. The id is derived from the other model alone, so an input a
        /// partial rendered under a prefix is reached from that partial's own plan, or by an event it raises.
        /// </summary>
        /// <typeparam name="TComponent">The component's declaration.</typeparam>
        /// <typeparam name="TOtherModel">The model the component was rendered for.</typeparam>
        /// <param name="expr">The other model's member the component is bound to.</param>
        /// <returns>The component, whose members the page uses directly.</returns>
        public TComponent Component<TComponent, TOtherModel>(Expression<Func<TOtherModel, object?>> expr)
            where TComponent : JsObject, IComponent, new()
            where TOtherModel : class =>
            Live<TComponent>(IdGenerator.For<TOtherModel, object?>(expr));

        /// <summary>The component the view rendered with an id it chose.</summary>
        /// <typeparam name="TComponent">The component's declaration.</typeparam>
        /// <param name="refId">The component's element id.</param>
        /// <returns>The component: <c>grid.Refresh()</c>, <c>grid.SelectRow(0)</c>.</returns>
        public TComponent Component<TComponent>(string refId)
            where TComponent : JsObject, IComponent, new() =>
            Live<TComponent>(refId);

        /// <summary>An app object the layout renders once, at the id its declaration names: Toast, Drawer, Loader.</summary>
        /// <typeparam name="TComponent">The app object's declaration.</typeparam>
        /// <returns>The app object: <c>toast.Content.Set("Saved")</c>, <c>toast.Show()</c>.</returns>
        public TComponent Component<TComponent>()
            where TComponent : JsObject, IAppLevelComponent, new()
        {
            var component = new TComponent();
            component.BindTo(new ComponentBinding(component.DefaultId, component.Vendor, Context));
            return component;
        }

        private TComponent Live<TComponent>(string componentId)
            where TComponent : JsObject, IComponent, new()
        {
            var component = new TComponent();
            component.BindTo(new ComponentBinding(componentId, component.Vendor, Context));
            return component;
        }

        /// <summary>
        /// The server-sent event stream a trigger listens to, by the declaration and URL the page handed
        /// <c>t.ServerPush(..)</c>: <c>p.Stream(alerts).Close()</c>, <c>p.When(p.Stream(alerts).ReadyState).Eq(ServerPushStream.Closed)</c>.
        /// </summary>
        /// <typeparam name="TStream">The stream's declaration.</typeparam>
        /// <param name="stream">The stream at its URL: <c>ServerPushStream.At&lt;TStream&gt;(url)</c>.</param>
        /// <returns>The stream, whose members the page uses directly.</returns>
        public TStream Stream<TStream>(ServerPushSource<TStream> stream)
            where TStream : ServerPushStream, new()
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            var live = new TStream();
            live.BindTo(new ModuleBinding(Target.Stream(stream.Url), Context));
            return live;
        }

        /// <summary>
        /// A declared hub, for its members and the methods the page calls on the server:
        /// <c>p.Hub(residents).ClearBoard()</c>, <c>p.Element("link").Text.Set(p.Hub(residents).State)</c>.
        /// </summary>
        /// <remarks>
        /// The plan holds the hub's connection, whether or not a trigger listens to it, for as long as its
        /// owner lives: the page, or the slot the plan was loaded into. The connection closes when the last
        /// plan holding it is unloaded.
        /// </remarks>
        /// <typeparam name="THub">The hub's declaration.</typeparam>
        /// <param name="hub">The hub at its URL: <c>SignalRHub.At&lt;THub&gt;(url)</c>.</param>
        /// <returns>The hub, whose members the page uses directly.</returns>
        public THub Hub<THub>(SignalRSource<THub> hub)
            where THub : SignalRHub, new()
        {
            if (hub == null) throw new ArgumentNullException(nameof(hub));
            var live = new THub();
            live.BindTo(new ModuleBinding(Target.Hub(hub.Url), Context));
            return live;
        }

        /// <summary>
        /// Reads the page's URL query string through <typeparamref name="TQuery"/>, the class the page's action
        /// binds with <c>[FromQuery]</c>: <c>p.Url&lt;CensusQuery&gt;().Get(q =&gt; q.FacilityId)</c>.
        /// </summary>
        /// <typeparam name="TQuery">The query class.</typeparam>
        /// <returns>The query, whose members are value sources.</returns>
        public UrlQuery<TQuery> Url<TQuery>() where TQuery : class => new UrlQuery<TQuery>();

        /// <summary>
        /// Shows the response's validation errors on the form's fields and summary. It belongs inside an
        /// error route of the request that received them: <c>.OnError(400, e =&gt; e.ValidationErrors(FormId))</c>.
        /// </summary>
        /// <param name="formId">The element that holds the form's inputs.</param>
        /// <returns>This pipeline builder.</returns>
        public PipelineBuilder<TModel> ValidationErrors(string formId)
        {
            Context.AddStep(new ShowErrorsStep(formId));
            return this;
        }

        /// <summary>Appends a reaction that injects the previous HTTP success body as HTML.</summary>
        /// <remarks>Must follow an HTTP request. The injected HTML comes from the active success response body.</remarks>
        /// <param name="elementId">Target DOM element ID.</param>
        /// <returns>This pipeline builder; chained reactions run after the injection.</returns>
        public PipelineBuilder<TModel> Into(string elementId)
        {
            Context.DeclareElement(elementId);
            var responseBody = ValueExpression.Get(Target.Response(ResponseHandle.Innermost), Shape.None);
            Context.AddStep(new InjectStep(elementId, responseBody));
            return this;
        }

        /// <summary>
        /// Unloads a slot a partial was loaded into: what the partial brought (its components, rules and
        /// behavior) is released, the slot is emptied, and what the page booted with stays.
        /// </summary>
        /// <param name="elementId">The slot's element id, the one <see cref="Into"/> loaded.</param>
        /// <returns>This pipeline builder.</returns>
        public PipelineBuilder<TModel> Unload(string elementId)
        {
            Context.DeclareElement(elementId);
            Context.AddStep(new UnloadStep(elementId));
            return this;
        }
    }
}
