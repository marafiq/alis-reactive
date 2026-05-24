using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Builds the sequence of commands that execute when a trigger fires: element mutations,
    /// event dispatches, HTTP calls, component interactions, and conditional logic.
    /// </summary>
    /// <remarks>
    /// Received as the <c>p</c> parameter inside trigger callbacks:
    /// <c>t.DomReady(p =&gt; { p.Element("id").AddClass("x"); p.Dispatch("ready"); })</c>.
    /// Commands execute in declaration order.
    /// </remarks>
    /// <typeparam name="TModel">The view model type.</typeparam>
    public partial class PipelineBuilder<TModel> : IReactionEmitter where TModel : class
    {
        internal PlanBuildContext Context { get; }

        private readonly PipelineDraft<TModel> _draft = new PipelineDraft<TModel>();

        internal PipelineBuilder(PlanBuildContext context)
        {
            Context = context;
        }

        /// <inheritdoc />
        void IReactionEmitter.AddStep(Reaction step) => AddStep(step);

        /// <inheritdoc />
        PlanBuildContext IReactionEmitter.BuildContext => Context;

        internal void AddStep(Reaction step)
        {
            _draft.AddCommand(step);
        }

        /// <summary>Dispatches a custom browser event by name.</summary>
        /// <param name="eventName">The event name. Listeners registered with <c>t.CustomEvent("name", ...)</c> will fire.</param>
        /// <returns>This builder for chaining.</returns>
        public PipelineBuilder<TModel> Dispatch(string eventName)
        {
            AddStep(Reaction.Dispatch(eventName));
            return this;
        }

        /// <summary>Dispatches a custom browser event with a typed payload.</summary>
        /// <typeparam name="TPayload">The payload type.</typeparam>
        /// <param name="eventName">The event name.</param>
        /// <param name="payload">The data to send with the event.</param>
        /// <returns>This builder for chaining.</returns>
        public PipelineBuilder<TModel> Dispatch<TPayload>(string eventName, TPayload payload)
        {
            AddStep(Reaction.Dispatch(
                eventName,
                ValueProducer.LiteralRaw(payload, Shape.FromClrType(typeof(TPayload))),
                PayloadContract.ForPayload(typeof(TPayload))));
            return this;
        }

        /// <summary>Dispatches a custom browser event with a source-backed payload whose fields are resolved at runtime.</summary>
        /// <remarks>
        /// <para>Each field on <typeparamref name="TPayload"/> can come from a live component value,
        /// URL parameter, plugin read, or a compile-time literal. The listener consumes the
        /// payload via <c>t.CustomEvent&lt;TPayload&gt;(name, (payload, p) =&gt; ...)</c>.</para>
        /// <para>Distinct from <see cref="Dispatch{TPayload}(string, TPayload)"/> which takes a
        /// compile-time literal payload object.</para>
        /// </remarks>
        /// <typeparam name="TPayload">The payload type matching the <c>CustomEvent&lt;TPayload&gt;</c> listener.</typeparam>
        /// <param name="eventName">The event name. Listeners registered with <c>t.CustomEvent("name", ...)</c> will fire.</param>
        /// <param name="configure">Populates payload fields via <see cref="DispatchPayloadBuilder{TPayload, TModel}"/>.</param>
        /// <returns>This builder for chaining.</returns>
        public PipelineBuilder<TModel> DispatchWith<TPayload>(
            string eventName,
            Action<DispatchPayloadBuilder<TPayload, TModel>> configure)
            where TPayload : class
        {
            var builder = new DispatchPayloadBuilder<TPayload, TModel>();
            configure(builder);
            AddStep(Reaction.Dispatch(
                eventName,
                builder.Build(),
                PayloadContract.ForPayload(typeof(TPayload))));
            return this;
        }

        /// <summary>Targets a DOM element by ID for mutations (SetText, AddClass, Show, Hide).</summary>
        /// <param name="elementId">The HTML element ID.</param>
        /// <returns>An element builder for chaining mutations.</returns>
        public ElementBuilder<TModel> Element(string elementId)
        {
            return new ElementBuilder<TModel>(this, elementId);
        }

        /// <summary>References a component bound to a model expression for method calls and property mutations.</summary>
        /// <typeparam name="TComponent">The component type.</typeparam>
        /// <param name="expr">The model expression that identifies the component.</param>
        /// <returns>A typed component reference.</returns>
        public ComponentRef<TComponent, TModel> Component<TComponent>(
            Expression<Func<TModel, object>> expr)
            where TComponent : IComponent, new()
        {
            var elementId = IdGenerator.For<TModel, object>(expr);
            return new ComponentRef<TComponent, TModel>(elementId, this);
        }

        /// <summary>References a component bound to a different model (cross-partial scenarios).</summary>
        /// <typeparam name="TComponent">The component type.</typeparam>
        /// <typeparam name="TOtherModel">The other view model type.</typeparam>
        /// <param name="expr">The model expression on the other model.</param>
        /// <returns>A typed component reference.</returns>
        public ComponentRef<TComponent, TModel> Component<TComponent, TOtherModel>(
            Expression<Func<TOtherModel, object>> expr)
            where TComponent : IComponent, new()
            where TOtherModel : class
        {
            var elementId = IdGenerator.For<TOtherModel, object>(expr);
            return new ComponentRef<TComponent, TModel>(elementId, this);
        }

        /// <summary>References a component by explicit ID.</summary>
        /// <typeparam name="TComponent">The component type.</typeparam>
        /// <param name="refId">The component element ID.</param>
        /// <returns>A typed component reference.</returns>
        public ComponentRef<TComponent, TModel> Component<TComponent>(string refId)
            where TComponent : IComponent, new()
        {
            return new ComponentRef<TComponent, TModel>(refId, this);
        }

        /// <summary>References a layout-owned app component (e.g. Toast, Confirm) by its default ID.</summary>
        /// <typeparam name="TComponent">The layout-owned app component type.</typeparam>
        /// <returns>A typed component reference.</returns>
        public ComponentRef<TComponent, TModel> Component<TComponent>()
            where TComponent : IAppLevelComponent, new()
        {
            var comp = new TComponent();
            return new ComponentRef<TComponent, TModel>(
                ComponentObjectTarget.ForLayout<TComponent>(comp.DefaultId),
                this);
        }

        /// <summary>Reads a URL query parameter as a string for use in conditions or as a value source.</summary>
        /// <param name="paramName">The query parameter name.</param>
        /// <returns>A typed source for conditions, SetText, or gather.</returns>
        public Conditions.TypedUrlSource<string> FromUrl(string paramName)
        {
            return new Conditions.TypedUrlSource<string>(paramName);
        }

        /// <summary>Reads a URL query parameter as a typed value: <c>p.FromUrl&lt;int&gt;("page")</c>.</summary>
        /// <param name="paramName">The query parameter name.</param>
        /// <returns>A typed source for conditions, SetText, or gather.</returns>
        public Conditions.TypedUrlSource<T> FromUrl<T>(string paramName)
        {
            return new Conditions.TypedUrlSource<T>(paramName);
        }

        /// <summary>Reads a value from a plugin method. Chain <c>.Arg()</c> to pass arguments.</summary>
        /// <param name="pluginName">The registered plugin name.</param>
        /// <param name="member">The method name on the plugin.</param>
        /// <returns>A builder that implicitly converts to <see cref="Conditions.TypedPluginSource{T}"/>.</returns>
        public PluginReadBuilder<T, TModel> Plugin<T>(string pluginName, string member)
        {
            if (string.IsNullOrWhiteSpace(pluginName)) throw new System.ArgumentException("Plugin name required.", nameof(pluginName));
            if (string.IsNullOrWhiteSpace(member)) throw new System.ArgumentException("Member name required.", nameof(member));
            var operation = PluginOperationId.Of(pluginName, member);
            var signature = Context.EnsurePluginMethod(PluginMethodRequirement.Function(
                operation,
                PlanModel.Shape.FromClrType(typeof(T))));
            return new PluginReadBuilder<T, TModel>(
                operation,
                signature.Arguments);
        }

        /// <summary>Reads a value by calling the registered plugin root function.</summary>
        /// <param name="pluginName">The registered plugin name.</param>
        public PluginReadBuilder<T, TModel> Plugin<T>(string pluginName)
        {
            if (string.IsNullOrWhiteSpace(pluginName)) throw new System.ArgumentException("Plugin name required.", nameof(pluginName));
            var operation = PluginOperationId.Root(pluginName);
            var signature = Context.EnsurePluginMethod(PluginMethodRequirement.Function(
                operation,
                PlanModel.Shape.FromClrType(typeof(T))));
            return new PluginReadBuilder<T, TModel>(
                operation,
                signature.Arguments);
        }

        /// <summary>Reads a property value from a registered plugin object.</summary>
        /// <param name="pluginName">The registered plugin name.</param>
        /// <param name="member">The property name on the plugin object.</param>
        public Conditions.TypedPluginPropertySource<T> PluginProperty<T>(string pluginName, string member)
        {
            if (string.IsNullOrWhiteSpace(pluginName)) throw new System.ArgumentException("Plugin name required.", nameof(pluginName));
            if (string.IsNullOrWhiteSpace(member)) throw new System.ArgumentException("Member name required.", nameof(member));

            var property = PluginPropertyId.Of(pluginName, member);
            Context.EnsurePluginProperty(PluginPropertyRequirement.Read(property, PlanModel.Shape.FromClrType(typeof(T))));
            return new Conditions.TypedPluginPropertySource<T>(property);
        }

        /// <summary>Reads a value from a declared plugin function.</summary>
        /// <param name="function">The plugin function descriptor registered with the plan.</param>
        public PluginReadBuilder<T, TModel> Plugin<T>(PluginFunction<T> function)
        {
            if (function == null) throw new System.ArgumentNullException(nameof(function));
            Context.EnsurePluginMethod(PluginMethodRequirement.Function(function));
            return new PluginReadBuilder<T, TModel>(function);
        }

        /// <summary>Reads a value from a declared plugin property.</summary>
        /// <param name="property">The plugin property descriptor registered with the plan.</param>
        public Conditions.TypedPluginPropertySource<T> Plugin<T>(PluginProperty<T> property)
        {
            if (property == null) throw new System.ArgumentNullException(nameof(property));
            Context.EnsurePluginProperty(PluginPropertyRequirement.Read(property));
            return new Conditions.TypedPluginPropertySource<T>(property.PropertyId);
        }

        /// <summary>Calls a plugin method that does not return a value. Chain <c>.Arg()</c> then <c>.Fire()</c>.</summary>
        /// <param name="pluginName">The registered plugin name.</param>
        /// <param name="member">The method name on the plugin.</param>
        /// <returns>A builder for adding arguments and firing the call.</returns>
        public PluginCallBuilder<TModel> Plugin(string pluginName, string member)
        {
            if (string.IsNullOrWhiteSpace(pluginName)) throw new System.ArgumentException("Plugin name required.", nameof(pluginName));
            if (string.IsNullOrWhiteSpace(member)) throw new System.ArgumentException("Member name required.", nameof(member));
            var operation = PluginOperationId.Of(pluginName, member);
            var signature = Context.EnsurePluginMethod(PluginMethodRequirement.Command(operation));
            return new PluginCallBuilder<TModel>(
                operation,
                this,
                signature.Arguments);
        }

        /// <summary>Calls the registered plugin root function as a command. Chain <c>.Arg()</c> then <c>.Fire()</c>.</summary>
        /// <param name="pluginName">The registered plugin name.</param>
        public PluginCallBuilder<TModel> Plugin(string pluginName)
        {
            if (string.IsNullOrWhiteSpace(pluginName)) throw new System.ArgumentException("Plugin name required.", nameof(pluginName));
            var operation = PluginOperationId.Root(pluginName);
            var signature = Context.EnsurePluginMethod(PluginMethodRequirement.Command(operation));
            return new PluginCallBuilder<TModel>(
                operation,
                this,
                signature.Arguments);
        }

        /// <summary>Calls a declared plugin command. Chain <c>.Arg()</c> then <c>.Fire()</c>.</summary>
        /// <param name="command">The plugin command descriptor registered with the plan.</param>
        public PluginCallBuilder<TModel> Plugin(PluginCommand command)
        {
            if (command == null) throw new System.ArgumentNullException(nameof(command));
            Context.EnsurePluginMethod(PluginMethodRequirement.Command(command));
            return new PluginCallBuilder<TModel>(command, this);
        }

        /// <summary>Displays accumulated validation errors in the specified container.</summary>
        /// <param name="formId">The DOM element ID of the validation error container.</param>
        /// <returns>This builder for chaining.</returns>
        public PipelineBuilder<TModel> ValidationErrors(string formId)
        {
            AddStep(Reaction.ShowValidationErrors(formId));
            return this;
        }

        /// <summary>Injects the HTTP success response body into a DOM element as HTML content.</summary>
        /// <remarks>Must follow an HTTP request (Get/Post). The response body is read from the success payload.</remarks>
        /// <param name="elementId">The target element ID.</param>
        /// <returns>This builder for chaining.</returns>
        public PipelineBuilder<TModel> Into(string elementId)
        {
            Context.EnsureElement(elementId);
            var responseBody = ValueProducer.Read(PayloadSource.Success(), "responseBody");
            AddStep(Reaction.Inject(elementId, responseBody));
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

        internal Reaction BuildReaction()
        {
            return _draft.BuildReaction();
        }

        internal List<Reaction> BuildReactions()
        {
            return _draft.BuildReactions();
        }
    }
}
