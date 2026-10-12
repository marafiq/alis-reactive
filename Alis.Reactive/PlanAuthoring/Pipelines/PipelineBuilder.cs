using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Builds the ordered reactions that execute when a trigger fires: element updates,
    /// event dispatches, HTTP calls, component interactions, and conditional logic.
    /// </summary>
    /// <remarks>
    /// Received as the <c>p</c> parameter inside trigger callbacks:
    /// <c>t.DomReady(p =&gt; { p.Element("id").AddClass("x"); p.Dispatch(CareEvents.Ready); })</c>.
    /// Reactions execute in declaration order.
    /// </remarks>
    /// <typeparam name="TModel">View model that owns model-bound component IDs and validation/gather fields.</typeparam>
    public partial class PipelineBuilder<TModel> : IReactionEmitter where TModel : class
    {
        internal PlanBuildContext Context { get; }

        private readonly ReactionPipelineDraft<TModel> _draft = new ReactionPipelineDraft<TModel>();

        internal PipelineBuilder(PlanBuildContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc />
        void IReactionEmitter.AddStep(ReactionGraph step) => AddStep(step);

        /// <inheritdoc />
        PlanBuildContext IReactionEmitter.BuildContext => Context;

        // A step lands in the callback being built, whichever pipeline variable or handle wrote it: a
        // handle declared in a trigger and used inside OnSuccess writes into OnSuccess.
        private PipelineBuilder<TModel> Active => Context.ActiveScope<PipelineBuilder<TModel>>();

        internal void AddStep(ReactionGraph step)
        {
            Active._draft.AddReaction(step);
        }

        /// <summary>Builds a callback's reactions with its pipeline as the active scope.</summary>
        internal static ReactionGraph Build(PlanBuildContext context, Action<PipelineBuilder<TModel>> reactions)
        {
            if (reactions == null) throw new ArgumentNullException(nameof(reactions));
            var pipeline = new PipelineBuilder<TModel>(context);
            context.EnterScope(pipeline);
            try
            {
                reactions(pipeline);
            }
            finally
            {
                context.ExitScope();
            }

            return pipeline.BuildReaction();
        }

        /// <summary>Raises a declared document event with no payload.</summary>
        /// <param name="appEvent">The event, declared once with <see cref="AppEvent.Named(string)"/>.</param>
        /// <returns>This pipeline builder; chained reactions run after the dispatch.</returns>
        public PipelineBuilder<TModel> Dispatch(AppEvent appEvent)
        {
            if (appEvent == null) throw new ArgumentNullException(nameof(appEvent));
            AddStep(ReactionGraph.Dispatch(appEvent.Name));
            return this;
        }

        /// <summary>Raises a declared document event without its payload: a listener reads every member as missing.</summary>
        /// <typeparam name="TPayload">The payload the event declares.</typeparam>
        /// <param name="appEvent">The event, declared once with <see cref="AppEvent.Named{TPayload}(string)"/>.</param>
        /// <returns>This pipeline builder; chained reactions run after the dispatch.</returns>
        public PipelineBuilder<TModel> Dispatch<TPayload>(AppEvent<TPayload> appEvent)
            where TPayload : class, new()
        {
            if (appEvent == null) throw new ArgumentNullException(nameof(appEvent));
            AddStep(ReactionGraph.Dispatch(appEvent.Name));
            return this;
        }

        /// <summary>Raises a declared document event with a payload fixed when the view renders.</summary>
        /// <typeparam name="TPayload">The payload the event declares.</typeparam>
        /// <param name="appEvent">The event, declared once with <see cref="AppEvent.Named{TPayload}(string)"/>.</param>
        /// <param name="payload">The payload, written into the plan.</param>
        /// <returns>This pipeline builder; chained reactions run after the dispatch.</returns>
        public PipelineBuilder<TModel> Dispatch<TPayload>(AppEvent<TPayload> appEvent, TPayload payload)
            where TPayload : class, new()
        {
            if (appEvent == null) throw new ArgumentNullException(nameof(appEvent));
            AddStep(ReactionGraph.Dispatch(
                appEvent.Name,
                ValueExpression.LiteralRaw(payload, Shape.FromClrType(typeof(TPayload)))));
            return this;
        }

        /// <summary>Raises a declared document event whose payload fields come from value sources.</summary>
        /// <remarks>
        /// Use this when the payload needs the page's current state, such as component values or URL
        /// parameters; use <see cref="Dispatch{TPayload}(AppEvent{TPayload}, TPayload)"/> for a payload
        /// fixed when the view renders.
        /// </remarks>
        /// <typeparam name="TPayload">The payload the event declares.</typeparam>
        /// <param name="appEvent">The event, declared once with <see cref="AppEvent.Named{TPayload}(string)"/>.</param>
        /// <param name="configure">Maps payload fields to value sources or literals.</param>
        /// <returns>This pipeline builder; chained reactions run after the dispatch.</returns>
        public PipelineBuilder<TModel> DispatchWith<TPayload>(
            AppEvent<TPayload> appEvent,
            Action<DispatchPayloadBuilder<TPayload, TModel>> configure)
            where TPayload : class, new()
        {
            if (appEvent == null) throw new ArgumentNullException(nameof(appEvent));
            var builder = new DispatchPayloadBuilder<TPayload, TModel>();
            configure(builder);
            AddStep(ReactionGraph.Dispatch(appEvent.Name, builder.Build()));
            return this;
        }

        /// <summary>Targets a controlled DOM element for updates in this Reactive Plan pipeline.</summary>
        /// <param name="elementId">Markup ID resolved directly by the runtime.</param>
        /// <returns>Element update builder.</returns>
        public ElementBuilder<TModel> Element(string elementId)
        {
            return new ElementBuilder<TModel>(this, elementId);
        }

        /// <summary>Targets the component registered for a property on this plan's view model.</summary>
        /// <typeparam name="TComponent">Component contract that determines available reads, writes, and calls.</typeparam>
        /// <param name="expr">
        /// Model property expression used to compute the same controlled ID as the markup helper.
        /// </param>
        /// <returns>An authoring handle joined to the generated component ID.</returns>
        public ComponentRef<TComponent, TModel> Component<TComponent>(
            Expression<Func<TModel, object?>> expr)
            where TComponent : IComponent, new()
        {
            var elementId = IdGenerator.For<TModel, object?>(expr);
            return new ComponentRef<TComponent, TModel>(elementId, this);
        }

        /// <summary>Targets the component registered for a property on another view model type.</summary>
        /// <typeparam name="TComponent">Component contract that determines available reads, writes, and calls.</typeparam>
        /// <typeparam name="TOtherModel">View model type used when the component was rendered.</typeparam>
        /// <param name="expr">
        /// Other model's property expression used to compute the same controlled ID as the markup helper.
        /// </param>
        /// <returns>An authoring handle joined to the other model's generated component ID.</returns>
        public ComponentRef<TComponent, TModel> Component<TComponent, TOtherModel>(
            Expression<Func<TOtherModel, object?>> expr)
            where TComponent : IComponent, new()
            where TOtherModel : class
        {
            var elementId = IdGenerator.For<TOtherModel, object?>(expr);
            return new ComponentRef<TComponent, TModel>(elementId, this);
        }

        /// <summary>Targets a component registered with an explicit markup ID.</summary>
        /// <typeparam name="TComponent">Component contract that determines available reads, writes, and calls.</typeparam>
        /// <param name="refId">Controlled component ID already present in markup.</param>
        /// <returns>An authoring handle joined to the explicit component ID.</returns>
        public ComponentRef<TComponent, TModel> Component<TComponent>(string refId)
            where TComponent : IComponent, new()
        {
            return new ComponentRef<TComponent, TModel>(refId, this);
        }

        /// <summary>Targets a layout-owned app component by the ID declared by its component contract.</summary>
        /// <typeparam name="TComponent">App-level component contract that supplies its default ID.</typeparam>
        /// <returns>An authoring handle joined to the contract's default component ID.</returns>
        public ComponentRef<TComponent, TModel> Component<TComponent>()
            where TComponent : IAppLevelComponent, new()
        {
            return new ComponentRef<TComponent, TModel>(
                ComponentObjectTarget.ForLayout<TComponent>(),
                this);
        }

        /// <summary>Targets the JavaScript object the triggering event handed its callback.</summary>
        /// <typeparam name="TArgs">The event's payload type.</typeparam>
        /// <param name="args">The typed payload the reaction received, for example <c>(args, p) =&gt; p.Payload(args)</c>.</param>
        /// <returns>An authoring handle that writes the payload's fields and calls its methods.</returns>
        public PayloadRef<TArgs, TModel> Payload<TArgs>(TArgs args)
        {
            return new PayloadRef<TArgs, TModel>(args, this);
        }

        /// <summary>
        /// Reads the page's URL query string through <typeparamref name="TQuery"/>, the class the page's action
        /// binds with <c>[FromQuery]</c>: <c>p.Url&lt;CensusQuery&gt;().Get(q =&gt; q.FacilityId)</c>.
        /// </summary>
        /// <typeparam name="TQuery">The query class.</typeparam>
        /// <returns>The query, whose members are value sources.</returns>
        public UrlQuery<TQuery> Url<TQuery>() where TQuery : class => new UrlQuery<TQuery>();

        /// <summary>Reads the return value declared by a reusable plugin function.</summary>
        /// <typeparam name="T">Value type exposed to downstream conditions, reactions, or gather.</typeparam>
        /// <param name="function">Plugin function carrying the plugin key, target member, argument contract, and return shape.</param>
        /// <returns>Plugin function call builder.</returns>
        public PluginMemberBuilder<T, TModel> Plugin<T>(PluginFunction<T> function)
        {
            if (function == null) throw new System.ArgumentNullException(nameof(function));
            Context.DeclarePluginMethod(PluginMethodRequirement.Function(function));
            return new PluginMemberBuilder<T, TModel>(function);
        }

        /// <summary>Reads the value declared by a reusable plugin property.</summary>
        /// <typeparam name="T">Value type exposed to downstream conditions, reactions, or gather.</typeparam>
        /// <param name="property">Plugin property carrying the plugin key, property member, and value shape.</param>
        /// <returns>Typed plugin property value source.</returns>
        public Conditions.TypedPluginPropertySource<T> Plugin<T>(PluginProperty<T> property)
        {
            if (property == null) throw new System.ArgumentNullException(nameof(property));
            Context.DeclarePluginProperty(PluginPropertyRequirement.Read(property));
            return new Conditions.TypedPluginPropertySource<T>(property.PropertyId);
        }

        /// <summary>Emits a plugin-call reaction from a reusable plugin command.</summary>
        /// <param name="command">Plugin command carrying the plugin key, target member, and argument contract.</param>
        /// <returns>Plugin command call builder.</returns>
        public PluginCallBuilder<TModel> Plugin(PluginCommand command)
        {
            if (command == null) throw new System.ArgumentNullException(nameof(command));
            Context.DeclarePluginMethod(PluginMethodRequirement.Command(command));
            return new PluginCallBuilder<TModel>(command, this);
        }

        /// <summary>Appends a reaction that renders accumulated validation errors into a container.</summary>
        /// <param name="formId">Validation error container element ID.</param>
        /// <returns>This pipeline builder.</returns>
        public PipelineBuilder<TModel> ValidationErrors(string formId)
        {
            AddStep(ReactionGraph.ShowValidationErrors(formId));
            return this;
        }

        /// <summary>Appends a reaction that injects the previous HTTP success body as HTML.</summary>
        /// <remarks>Must follow an HTTP request. The injected HTML comes from the active success response body.</remarks>
        /// <param name="elementId">Target DOM element ID.</param>
        /// <returns>This pipeline builder; chained reactions run after the injection.</returns>
        public PipelineBuilder<TModel> Into(string elementId)
        {
            Context.DeclareElement(elementId);
            var responseBody = ValueExpression.ReadWholePayload(PayloadSource.Success());
            AddStep(ReactionGraph.Inject(elementId, responseBody));
            return this;
        }

        internal void SetConditionalBranches(List<BranchCase> branches)
        {
            _draft.SetConditionalBranches(branches);
        }

        internal ReactionGraph BuildReaction()
        {
            return _draft.BuildReaction();
        }

    }
}
