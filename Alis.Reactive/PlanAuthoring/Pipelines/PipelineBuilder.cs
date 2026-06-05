using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Builds the ordered reactions that execute when a trigger fires: element mutations,
    /// event dispatches, HTTP calls, component interactions, and conditional logic.
    /// </summary>
    /// <remarks>
    /// Received as the <c>p</c> parameter inside trigger callbacks:
    /// <c>t.DomReady(p =&gt; { p.Element("id").AddClass("x"); p.Dispatch("ready"); })</c>.
    /// Reactions execute in declaration order.
    /// </remarks>
    /// <typeparam name="TModel">The view model that owns model-bound component IDs and validation/gather fields.</typeparam>
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
            _draft.AddCommand(step);
        }

        /// <summary>Queues a <c>CustomEvent</c> reaction without a payload.</summary>
        /// <param name="eventName">The event name matched by <c>t.CustomEvent(...)</c> triggers.</param>
        /// <returns>The current builder; chained reactions are appended after the dispatch.</returns>
        public PipelineBuilder<TModel> Dispatch(string eventName)
        {
            AddStep(ReactionGraph.Dispatch(eventName));
            return this;
        }

        /// <summary>Queues a <c>CustomEvent</c> reaction with a build-time payload literal.</summary>
        /// <typeparam name="TPayload">The payload contract consumed by matching custom event triggers.</typeparam>
        /// <param name="eventName">The event name matched by <c>t.CustomEvent&lt;TPayload&gt;(...)</c> triggers.</param>
        /// <param name="payload">The payload object serialized into the generated plan.</param>
        /// <returns>The current builder; chained reactions are appended after the dispatch.</returns>
        public PipelineBuilder<TModel> Dispatch<TPayload>(string eventName, TPayload payload)
        {
            AddStep(ReactionGraph.Dispatch(
                eventName,
                ValueExpression.LiteralRaw(payload, Shape.FromClrType(typeof(TPayload))),
                PayloadContract.ForPayload(typeof(TPayload))));
            return this;
        }

        /// <summary>Dispatches a <c>CustomEvent</c> whose payload fields are resolved from value sources at runtime.</summary>
        /// <remarks>
        /// <para>Use this when the payload needs current state from runtime sources,
        /// such as component values or URL parameters. The listener consumes the
        /// payload via <c>t.CustomEvent&lt;TPayload&gt;(name, (payload, p) =&gt; ...)</c>.</para>
        /// <para>Use <see cref="Dispatch{TPayload}(string, TPayload)"/> for a fixed build-time
        /// payload object.</para>
        /// </remarks>
        /// <typeparam name="TPayload">The payload contract consumed by matching custom event triggers.</typeparam>
        /// <param name="eventName">The event name matched by <c>t.CustomEvent&lt;TPayload&gt;(...)</c> triggers.</param>
        /// <param name="configure">Maps payload fields to runtime value sources or literals.</param>
        /// <returns>The current builder; chained reactions are appended after the dispatch.</returns>
        public PipelineBuilder<TModel> DispatchWith<TPayload>(
            string eventName,
            Action<DispatchPayloadBuilder<TPayload, TModel>> configure)
            where TPayload : class
        {
            var builder = new DispatchPayloadBuilder<TPayload, TModel>();
            configure(builder);
            AddStep(ReactionGraph.Dispatch(
                eventName,
                builder.Build(),
                PayloadContract.ForPayload(typeof(TPayload))));
            return this;
        }

        /// <summary>Targets a controlled DOM element for mutations in the current pipeline.</summary>
        /// <param name="elementId">The markup ID resolved directly by the runtime.</param>
        /// <returns>A builder for appending DOM mutations to this pipeline.</returns>
        public ElementBuilder<TModel> Element(string elementId)
        {
            return new ElementBuilder<TModel>(this, elementId);
        }

        /// <summary>Targets a model-bound component rendered for the current view model.</summary>
        /// <typeparam name="TComponent">The typed component contract that controls available operations.</typeparam>
        /// <param name="expr">The model property expression shared with the markup helper.</param>
        /// <returns>A typed component reference bound to the generated component ID.</returns>
        public ComponentRef<TComponent, TModel> Component<TComponent>(
            Expression<Func<TModel, object>> expr)
            where TComponent : IComponent, new()
        {
            var elementId = IdGenerator.For<TModel, object>(expr);
            return new ComponentRef<TComponent, TModel>(elementId, this);
        }

        /// <summary>Targets a model-bound component rendered from another view model.</summary>
        /// <typeparam name="TComponent">The typed component contract that controls available operations.</typeparam>
        /// <typeparam name="TOtherModel">The model that owns the referenced component ID.</typeparam>
        /// <param name="expr">The other model's property expression shared with the markup helper.</param>
        /// <returns>A typed component reference bound to the other model's generated component ID.</returns>
        public ComponentRef<TComponent, TModel> Component<TComponent, TOtherModel>(
            Expression<Func<TOtherModel, object>> expr)
            where TComponent : IComponent, new()
            where TOtherModel : class
        {
            var elementId = IdGenerator.For<TOtherModel, object>(expr);
            return new ComponentRef<TComponent, TModel>(elementId, this);
        }

        /// <summary>Targets a component whose ID is supplied outside model binding.</summary>
        /// <typeparam name="TComponent">The typed component contract that controls available operations.</typeparam>
        /// <param name="refId">The component ID already rendered in markup.</param>
        /// <returns>A typed component reference bound to the explicit component ID.</returns>
        public ComponentRef<TComponent, TModel> Component<TComponent>(string refId)
            where TComponent : IComponent, new()
        {
            return new ComponentRef<TComponent, TModel>(refId, this);
        }

        /// <summary>Targets a layout-owned app component by its default ID.</summary>
        /// <typeparam name="TComponent">The app-level component contract that controls available operations.</typeparam>
        /// <returns>A typed component reference bound to the component's layout ID.</returns>
        public ComponentRef<TComponent, TModel> Component<TComponent>()
            where TComponent : IAppLevelComponent, new()
        {
            return new ComponentRef<TComponent, TModel>(
                ComponentObjectTarget.ForLayout<TComponent>(),
                this);
        }

        /// <summary>Reads a string value from the current URL query string at runtime.</summary>
        /// <param name="paramName">The query parameter name to read at runtime.</param>
        /// <returns>A URL value source for downstream conditions, mutations, or gather.</returns>
        public Conditions.TypedUrlSource<string> FromUrl(string paramName)
        {
            return new Conditions.TypedUrlSource<string>(paramName);
        }

        /// <summary>Reads a typed value from the current URL query string at runtime.</summary>
        /// <typeparam name="T">The value type expected by downstream conditions, mutations, or gather.</typeparam>
        /// <param name="paramName">The query parameter name to read at runtime.</param>
        /// <returns>A URL value source for downstream conditions, mutations, or gather.</returns>
        public Conditions.TypedUrlSource<T> FromUrl<T>(string paramName)
        {
            return new Conditions.TypedUrlSource<T>(paramName);
        }

        /// <summary>Reads a value returned by a member function on a plan-registered plugin.</summary>
        /// <typeparam name="T">The function return type exposed to downstream value expressions.</typeparam>
        /// <param name="pluginName">The plugin name declared with <c>plan.RegisterPlugin(...)</c>.</param>
        /// <param name="member">The function member to invoke on the host-provided plugin object.</param>
        /// <returns>A builder for supplying arguments before the function result becomes a typed source.</returns>
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

        /// <summary>Reads a value returned by the root function of a plan-registered plugin.</summary>
        /// <typeparam name="T">The function return type exposed to downstream value expressions.</typeparam>
        /// <param name="pluginName">The plugin name declared with <c>plan.RegisterPlugin(...)</c>.</param>
        /// <returns>A builder for supplying arguments before the function result becomes a typed source.</returns>
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
        /// <typeparam name="T">The property value type exposed to downstream value expressions.</typeparam>
        /// <param name="pluginName">The plugin name declared with <c>plan.RegisterPlugin(...)</c>.</param>
        /// <param name="member">The property member to read from the host-provided plugin object.</param>
        /// <returns>A typed plugin property value source.</returns>
        public Conditions.TypedPluginPropertySource<T> PluginProperty<T>(string pluginName, string member)
        {
            if (string.IsNullOrWhiteSpace(pluginName)) throw new System.ArgumentException("Plugin name required.", nameof(pluginName));
            if (string.IsNullOrWhiteSpace(member)) throw new System.ArgumentException("Member name required.", nameof(member));

            var property = PluginPropertyId.Of(pluginName, member);
            Context.DeclarePluginProperty(PluginPropertyRequirement.Read(property, PlanModel.Shape.FromClrType(typeof(T))));
            return new Conditions.TypedPluginPropertySource<T>(property);
        }

        /// <summary>Reads a value returned by a typed plugin function descriptor.</summary>
        /// <typeparam name="T">The function return type exposed to downstream value expressions.</typeparam>
        /// <param name="function">The descriptor carrying the plugin name, target member, argument contract, and return shape.</param>
        /// <returns>A builder for supplying arguments before the function result becomes a typed source.</returns>
        public PluginMemberBuilder<T, TModel> Plugin<T>(PluginFunction<T> function)
        {
            if (function == null) throw new System.ArgumentNullException(nameof(function));
            Context.DeclarePluginMethod(PluginMethodRequirement.Function(function));
            return new PluginMemberBuilder<T, TModel>(function);
        }

        /// <summary>Reads a property value described by a typed plugin descriptor.</summary>
        /// <typeparam name="T">The property value type exposed to downstream value expressions.</typeparam>
        /// <param name="property">The descriptor carrying the plugin name, property member, and value shape.</param>
        /// <returns>A typed plugin property value source.</returns>
        public Conditions.TypedPluginPropertySource<T> Plugin<T>(PluginProperty<T> property)
        {
            if (property == null) throw new System.ArgumentNullException(nameof(property));
            Context.DeclarePluginProperty(PluginPropertyRequirement.Read(property));
            return new Conditions.TypedPluginPropertySource<T>(property.PropertyId);
        }

        /// <summary>Starts a command reaction against a member on a plan-registered plugin.</summary>
        /// <param name="pluginName">The plugin name declared with <c>plan.RegisterPlugin(...)</c>.</param>
        /// <param name="member">The command member to invoke on the host-provided plugin object.</param>
        /// <returns>A command builder for supplying arguments before emitting the plugin call with <c>Fire()</c>.</returns>
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

        /// <summary>Starts a command reaction against the root command of a plan-registered plugin.</summary>
        /// <param name="pluginName">The plugin name declared with <c>plan.RegisterPlugin(...)</c>.</param>
        /// <returns>A command builder for supplying arguments before emitting the plugin call with <c>Fire()</c>.</returns>
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

        /// <summary>Starts a command reaction described by a typed plugin descriptor.</summary>
        /// <param name="command">The descriptor carrying the plugin name, target member, and argument contract.</param>
        /// <returns>A command builder for supplying arguments before emitting the plugin call with <c>Fire()</c>.</returns>
        public PluginCallBuilder<TModel> Plugin(PluginCommand command)
        {
            if (command == null) throw new System.ArgumentNullException(nameof(command));
            Context.DeclarePluginMethod(PluginMethodRequirement.Command(command));
            return new PluginCallBuilder<TModel>(command, this);
        }

        /// <summary>Appends a reaction that renders accumulated validation errors into a container.</summary>
        /// <param name="formId">The element ID of the validation error container.</param>
        /// <returns>The current builder; chained reactions are appended after the validation reaction.</returns>
        public PipelineBuilder<TModel> ValidationErrors(string formId)
        {
            AddStep(ReactionGraph.ShowValidationErrors(formId));
            return this;
        }

        /// <summary>Appends a reaction that injects the previous HTTP success body as HTML.</summary>
        /// <remarks>Must follow an HTTP request. The response body is read from the success payload.</remarks>
        /// <param name="elementId">The target DOM element ID.</param>
        /// <returns>The current builder; chained reactions are appended after the injection.</returns>
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

        internal void FlushSegment()
        {
            _draft.FlushSegment();
        }

        internal ReactionGraph BuildReaction()
        {
            return _draft.BuildReaction();
        }

    }
}
