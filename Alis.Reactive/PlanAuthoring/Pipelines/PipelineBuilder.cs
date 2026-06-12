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
    /// <c>t.DomReady(p =&gt; { p.Element("id").AddClass("x"); p.Dispatch("ready"); })</c>.
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

        internal void AddStep(ReactionGraph step)
        {
            _draft.AddReaction(step);
        }

        /// <summary>Queues a <c>CustomEvent</c> reaction without a payload.</summary>
        /// <param name="eventName">Event name matched by <c>t.CustomEvent(...)</c> triggers.</param>
        /// <returns>This pipeline builder; chained reactions run after the dispatch.</returns>
        public PipelineBuilder<TModel> Dispatch(string eventName)
        {
            AddStep(ReactionGraph.Dispatch(eventName));
            return this;
        }

        /// <summary>Queues a <c>CustomEvent</c> reaction with a build-time payload literal.</summary>
        /// <typeparam name="TPayload">Payload contract consumed by matching custom event triggers.</typeparam>
        /// <param name="eventName">Event name matched by <c>t.CustomEvent&lt;TPayload&gt;(...)</c> triggers.</param>
        /// <param name="payload">Payload object serialized into the generated plan.</param>
        /// <returns>This pipeline builder; chained reactions run after the dispatch.</returns>
        public PipelineBuilder<TModel> Dispatch<TPayload>(string eventName, TPayload payload)
        {
            AddStep(ReactionGraph.Dispatch(
                eventName,
                ValueExpression.LiteralRaw(payload, Shape.FromClrType(typeof(TPayload)))));
            return this;
        }

        /// <summary>Dispatches a <c>CustomEvent</c> whose payload fields come from value sources.</summary>
        /// <remarks>
        /// <para>Use this when the payload needs current state from Reactive Plan
        /// value sources, such as component values or URL parameters. The listener consumes the
        /// payload via <c>t.CustomEvent&lt;TPayload&gt;(name, (payload, p) =&gt; ...)</c>.</para>
        /// <para>Use <see cref="Dispatch{TPayload}(string, TPayload)"/> for a fixed build-time
        /// payload object.</para>
        /// </remarks>
        /// <typeparam name="TPayload">Payload contract consumed by matching custom event triggers.</typeparam>
        /// <param name="eventName">Event name matched by <c>t.CustomEvent&lt;TPayload&gt;(...)</c> triggers.</param>
        /// <param name="configure">Maps payload fields to value sources or literals.</param>
        /// <returns>This pipeline builder; chained reactions run after the dispatch.</returns>
        public PipelineBuilder<TModel> DispatchWith<TPayload>(
            string eventName,
            Action<DispatchPayloadBuilder<TPayload, TModel>> configure)
            where TPayload : class
        {
            var builder = new DispatchPayloadBuilder<TPayload, TModel>();
            configure(builder);
            AddStep(ReactionGraph.Dispatch(eventName, builder.Build()));
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
            Expression<Func<TModel, object>> expr)
            where TComponent : IComponent, new()
        {
            var elementId = IdGenerator.For<TModel, object>(expr);
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
            Expression<Func<TOtherModel, object>> expr)
            where TComponent : IComponent, new()
            where TOtherModel : class
        {
            var elementId = IdGenerator.For<TOtherModel, object>(expr);
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

        /// <summary>Reads a string value from the current URL query string at runtime.</summary>
        /// <param name="paramName">Query parameter name read at runtime.</param>
        /// <returns>URL value source for conditions, reactions, or gather.</returns>
        public Conditions.TypedUrlSource<string> FromUrl(string paramName)
        {
            return new Conditions.TypedUrlSource<string>(paramName);
        }

        /// <summary>Reads a typed value from the current URL query string at runtime.</summary>
        /// <typeparam name="T">Value type expected by downstream conditions, reactions, or gather.</typeparam>
        /// <param name="paramName">Query parameter name read at runtime.</param>
        /// <returns>URL value source for conditions, reactions, or gather.</returns>
        public Conditions.TypedUrlSource<T> FromUrl<T>(string paramName)
        {
            return new Conditions.TypedUrlSource<T>(paramName);
        }

        /// <summary>Reads the return value from a named function member on a plan-registered plugin.</summary>
        /// <typeparam name="T">Value type exposed to downstream conditions, reactions, or gather.</typeparam>
        /// <param name="pluginName">Registered plugin key.</param>
        /// <param name="member">Function member invoked on the host-provided plugin object.</param>
        /// <returns>Plugin function call builder.</returns>
        public PluginMemberBuilder<T, TModel> Plugin<T>(string pluginName, string member)
        {
            if (string.IsNullOrWhiteSpace(pluginName)) throw new System.ArgumentException("Plugin name required.", nameof(pluginName));
            if (string.IsNullOrWhiteSpace(member)) throw new System.ArgumentException("Member name required.", nameof(member));
            var operation = PluginOperationId.Of(pluginName, member);
            var signature = Context.DeclarePluginMethod(PluginMethodRequirement.Function(
                operation,
                PlanModel.Shape.FromClrType(typeof(T))));
            return new PluginMemberBuilder<T, TModel>(
                operation,
                signature.Arguments);
        }

        /// <summary>Reads the return value from a plan-registered plugin object that is itself callable.</summary>
        /// <typeparam name="T">Value type exposed to downstream conditions, reactions, or gather.</typeparam>
        /// <param name="pluginName">Registered plugin key.</param>
        /// <returns>Plugin root-function call builder.</returns>
        public PluginMemberBuilder<T, TModel> Plugin<T>(string pluginName)
        {
            if (string.IsNullOrWhiteSpace(pluginName)) throw new System.ArgumentException("Plugin name required.", nameof(pluginName));
            var operation = PluginOperationId.Root(pluginName);
            var signature = Context.DeclarePluginMethod(PluginMethodRequirement.Function(
                operation,
                PlanModel.Shape.FromClrType(typeof(T))));
            return new PluginMemberBuilder<T, TModel>(
                operation,
                signature.Arguments);
        }

        /// <summary>Reads a property value from a plan-registered plugin.</summary>
        /// <typeparam name="T">Value type exposed to downstream conditions, reactions, or gather.</typeparam>
        /// <param name="pluginName">Registered plugin key.</param>
        /// <param name="member">Property member read from the host-provided plugin object.</param>
        /// <returns>Typed plugin property value source.</returns>
        public Conditions.TypedPluginPropertySource<T> PluginProperty<T>(string pluginName, string member)
        {
            if (string.IsNullOrWhiteSpace(pluginName)) throw new System.ArgumentException("Plugin name required.", nameof(pluginName));
            if (string.IsNullOrWhiteSpace(member)) throw new System.ArgumentException("Member name required.", nameof(member));

            var property = PluginPropertyId.Of(pluginName, member);
            Context.DeclarePluginProperty(PluginPropertyRequirement.Read(property, PlanModel.Shape.FromClrType(typeof(T))));
            return new Conditions.TypedPluginPropertySource<T>(property);
        }

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

        /// <summary>Emits a plugin-call reaction against a named member on a plan-registered plugin.</summary>
        /// <param name="pluginName">Registered plugin key.</param>
        /// <param name="member">Command member invoked on the host-provided plugin object.</param>
        /// <returns>Plugin command call builder.</returns>
        public PluginCallBuilder<TModel> Plugin(string pluginName, string member)
        {
            if (string.IsNullOrWhiteSpace(pluginName)) throw new System.ArgumentException("Plugin name required.", nameof(pluginName));
            if (string.IsNullOrWhiteSpace(member)) throw new System.ArgumentException("Member name required.", nameof(member));
            var operation = PluginOperationId.Of(pluginName, member);
            var signature = Context.DeclarePluginMethod(PluginMethodRequirement.Command(operation));
            return new PluginCallBuilder<TModel>(
                operation,
                this,
                signature.Arguments);
        }

        /// <summary>Emits a plugin-call reaction against a plan-registered plugin object that is itself callable.</summary>
        /// <param name="pluginName">Registered plugin key.</param>
        /// <returns>Plugin root-command call builder.</returns>
        public PluginCallBuilder<TModel> Plugin(string pluginName)
        {
            if (string.IsNullOrWhiteSpace(pluginName)) throw new System.ArgumentException("Plugin name required.", nameof(pluginName));
            var operation = PluginOperationId.Root(pluginName);
            var signature = Context.DeclarePluginMethod(PluginMethodRequirement.Command(operation));
            return new PluginCallBuilder<TModel>(
                operation,
                this,
                signature.Arguments);
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
