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

        /// <summary>Raises a <c>CustomEvent</c> without a payload.</summary>
        /// <param name="eventName">The event name matched by <c>t.CustomEvent(...)</c>.</param>
        /// <returns>The current pipeline builder so later reactions execute after the dispatch.</returns>
        public PipelineBuilder<TModel> Dispatch(string eventName)
        {
            AddStep(ReactionGraph.Dispatch(eventName));
            return this;
        }

        /// <summary>Raises a <c>CustomEvent</c> with a literal payload captured at plan build time.</summary>
        /// <typeparam name="TPayload">The payload contract consumed by the matching custom event trigger.</typeparam>
        /// <param name="eventName">The event name matched by <c>t.CustomEvent&lt;TPayload&gt;(...)</c>.</param>
        /// <param name="payload">The payload object serialized into the generated plan.</param>
        /// <returns>The current pipeline builder so later reactions execute after the dispatch.</returns>
        public PipelineBuilder<TModel> Dispatch<TPayload>(string eventName, TPayload payload)
        {
            AddStep(ReactionGraph.Dispatch(
                eventName,
                ValueExpression.LiteralRaw(payload, Shape.FromClrType(typeof(TPayload))),
                PayloadContract.ForPayload(typeof(TPayload))));
            return this;
        }

        /// <summary>Raises a <c>CustomEvent</c> with a source-backed payload whose fields are resolved at runtime.</summary>
        /// <remarks>
        /// <para>Each field on <typeparamref name="TPayload"/> can come from a live component value,
        /// URL parameter, plugin read, or a compile-time literal. The listener consumes the
        /// payload via <c>t.CustomEvent&lt;TPayload&gt;(name, (payload, p) =&gt; ...)</c>.</para>
        /// <para>Distinct from <see cref="Dispatch{TPayload}(string, TPayload)"/> which takes a
        /// compile-time literal payload object.</para>
        /// </remarks>
        /// <typeparam name="TPayload">The payload contract consumed by the matching custom event trigger.</typeparam>
        /// <param name="eventName">The event name matched by <c>t.CustomEvent&lt;TPayload&gt;(...)</c>.</param>
        /// <param name="configure">Configures each payload field from a runtime value source or literal.</param>
        /// <returns>The current pipeline builder so later reactions execute after the dispatch.</returns>
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

        /// <summary>Targets a controlled DOM element for text, class, visibility, and HTML mutations.</summary>
        /// <param name="elementId">The DOM ID declared by markup that the runtime can resolve directly.</param>
        /// <returns>An element builder for configuring DOM mutations.</returns>
        public ElementBuilder<TModel> Element(string elementId)
        {
            return new ElementBuilder<TModel>(this, elementId);
        }

        /// <summary>References a model-bound component using the same generated ID as the markup helper.</summary>
        /// <typeparam name="TComponent">The component contract registered in the Reactive Plan.</typeparam>
        /// <param name="expr">The model property expression used to generate the controlled component ID.</param>
        /// <returns>A typed component reference for configuring component operations.</returns>
        public ComponentRef<TComponent, TModel> Component<TComponent>(
            Expression<Func<TModel, object>> expr)
            where TComponent : IComponent, new()
        {
            var elementId = IdGenerator.For<TModel, object>(expr);
            return new ComponentRef<TComponent, TModel>(elementId, this);
        }

        /// <summary>References a model-bound component from another view model, such as a partial slot model.</summary>
        /// <typeparam name="TComponent">The component contract registered in the Reactive Plan.</typeparam>
        /// <typeparam name="TOtherModel">The model that owns the referenced component, often a partial-slot model.</typeparam>
        /// <param name="expr">The other model's property expression used to generate the controlled component ID.</param>
        /// <returns>A typed component reference for configuring component operations.</returns>
        public ComponentRef<TComponent, TModel> Component<TComponent, TOtherModel>(
            Expression<Func<TOtherModel, object>> expr)
            where TComponent : IComponent, new()
            where TOtherModel : class
        {
            var elementId = IdGenerator.For<TOtherModel, object>(expr);
            return new ComponentRef<TComponent, TModel>(elementId, this);
        }

        /// <summary>References a component by an explicit controlled element ID.</summary>
        /// <typeparam name="TComponent">The component contract registered in the Reactive Plan.</typeparam>
        /// <param name="refId">The explicit controlled component ID rendered in markup.</param>
        /// <returns>A typed component reference for configuring component operations.</returns>
        public ComponentRef<TComponent, TModel> Component<TComponent>(string refId)
            where TComponent : IComponent, new()
        {
            return new ComponentRef<TComponent, TModel>(refId, this);
        }

        /// <summary>References a layout-owned app component, such as Toast or Confirm, by its default ID.</summary>
        /// <typeparam name="TComponent">The layout-owned component contract type.</typeparam>
        /// <returns>A typed component reference for configuring app-level component operations.</returns>
        public ComponentRef<TComponent, TModel> Component<TComponent>()
            where TComponent : IAppLevelComponent, new()
        {
            return new ComponentRef<TComponent, TModel>(
                ComponentObjectTarget.ForLayout<TComponent>(),
                this);
        }

        /// <summary>Reads a URL query parameter as a string value source for conditions, mutations, or gather.</summary>
        /// <param name="paramName">The URL query parameter name.</param>
        /// <returns>A string URL value source.</returns>
        public Conditions.TypedUrlSource<string> FromUrl(string paramName)
        {
            return new Conditions.TypedUrlSource<string>(paramName);
        }

        /// <summary>Reads a URL query parameter as a typed value: <c>p.FromUrl&lt;int&gt;("page")</c>.</summary>
        /// <typeparam name="T">The value type expected by downstream conditions, mutations, or gather.</typeparam>
        /// <param name="paramName">The URL query parameter name.</param>
        /// <returns>A typed URL value source.</returns>
        public Conditions.TypedUrlSource<T> FromUrl<T>(string paramName)
        {
            return new Conditions.TypedUrlSource<T>(paramName);
        }

        /// <summary>Declares and reads a value from a Reactive Plan plugin method.</summary>
        /// <typeparam name="T">The method return type exposed to downstream conditions, mutations, or gather.</typeparam>
        /// <param name="pluginName">The plugin registration name in the plan.</param>
        /// <param name="member">The registered plugin method name.</param>
        /// <returns>A plugin member builder that accepts arguments and can be used as a typed value source.</returns>
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

        /// <summary>Declares and reads a value from a Reactive Plan plugin root function.</summary>
        /// <typeparam name="T">The root function return type exposed to downstream conditions, mutations, or gather.</typeparam>
        /// <param name="pluginName">The plugin registration name in the plan.</param>
        /// <returns>A plugin member builder that accepts arguments and can be used as a typed value source.</returns>
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

        /// <summary>Declares and reads a property value from a Reactive Plan plugin.</summary>
        /// <typeparam name="T">The property value type exposed to downstream conditions, mutations, or gather.</typeparam>
        /// <param name="pluginName">The plugin registration name in the plan.</param>
        /// <param name="member">The registered plugin property name.</param>
        /// <returns>A typed plugin property value source.</returns>
        public Conditions.TypedPluginPropertySource<T> PluginProperty<T>(string pluginName, string member)
        {
            if (string.IsNullOrWhiteSpace(pluginName)) throw new System.ArgumentException("Plugin name required.", nameof(pluginName));
            if (string.IsNullOrWhiteSpace(member)) throw new System.ArgumentException("Member name required.", nameof(member));

            var property = PluginPropertyId.Of(pluginName, member);
            Context.DeclarePluginProperty(PluginPropertyRequirement.Read(property, PlanModel.Shape.FromClrType(typeof(T))));
            return new Conditions.TypedPluginPropertySource<T>(property);
        }

        /// <summary>Reads a value from a typed plugin function descriptor.</summary>
        /// <typeparam name="T">The function return type exposed to downstream conditions, mutations, or gather.</typeparam>
        /// <param name="function">The descriptor that declares the plugin contract in the plan.</param>
        /// <returns>A plugin member builder that accepts arguments and can be used as a typed value source.</returns>
        public PluginMemberBuilder<T, TModel> Plugin<T>(PluginFunction<T> function)
        {
            if (function == null) throw new System.ArgumentNullException(nameof(function));
            Context.DeclarePluginMethod(PluginMethodRequirement.Function(function));
            return new PluginMemberBuilder<T, TModel>(function);
        }

        /// <summary>Reads a value from a typed plugin property descriptor.</summary>
        /// <typeparam name="T">The property value type exposed to downstream conditions, mutations, or gather.</typeparam>
        /// <param name="property">The descriptor that declares the plugin property contract in the plan.</param>
        /// <returns>A typed plugin property value source.</returns>
        public Conditions.TypedPluginPropertySource<T> Plugin<T>(PluginProperty<T> property)
        {
            if (property == null) throw new System.ArgumentNullException(nameof(property));
            Context.DeclarePluginProperty(PluginPropertyRequirement.Read(property));
            return new Conditions.TypedPluginPropertySource<T>(property.PropertyId);
        }

        /// <summary>Declares a Reactive Plan plugin method command. Chain <c>.Arg()</c> then <c>.Fire()</c>.</summary>
        /// <param name="pluginName">The plugin registration name in the plan.</param>
        /// <param name="member">The registered plugin method name.</param>
        /// <returns>A plugin command builder for configuring arguments and emitting the command.</returns>
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

        /// <summary>Declares a Reactive Plan plugin root-function command. Chain <c>.Arg()</c> then <c>.Fire()</c>.</summary>
        /// <param name="pluginName">The plugin registration name in the plan.</param>
        /// <returns>A plugin command builder for configuring arguments and emitting the command.</returns>
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

        /// <summary>Uses a typed plugin command descriptor. Chain <c>.Arg()</c> then <c>.Fire()</c>.</summary>
        /// <param name="command">The descriptor that declares the plugin command contract in the plan.</param>
        /// <returns>A plugin command builder for configuring arguments and emitting the command.</returns>
        public PluginCallBuilder<TModel> Plugin(PluginCommand command)
        {
            if (command == null) throw new System.ArgumentNullException(nameof(command));
            Context.DeclarePluginMethod(PluginMethodRequirement.Command(command));
            return new PluginCallBuilder<TModel>(command, this);
        }

        /// <summary>Displays accumulated validation errors in the specified container.</summary>
        /// <param name="formId">The validation error container element ID.</param>
        public PipelineBuilder<TModel> ValidationErrors(string formId)
        {
            AddStep(ReactionGraph.ShowValidationErrors(formId));
            return this;
        }

        /// <summary>Injects the HTTP success response body into a DOM element as HTML content.</summary>
        /// <remarks>Must follow an HTTP request (Get/Post). The response body is read from the success payload.</remarks>
        /// <param name="elementId">The target DOM element ID.</param>
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
