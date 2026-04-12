using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Requests;
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
        private enum PipelineMode { Sequential, Http, Parallel, Conditional }

        internal List<Reaction> Steps { get; } = new List<Reaction>();
        internal List<BranchCase> ConditionalBranches { get; private set; }
        internal PlanBuildContext Context { get; }

        private HttpRequestBuilder<TModel> _httpBuilder;
        private ParallelBuilder<TModel> _parallelBuilder;
        private PipelineMode _mode = PipelineMode.Sequential;
        private List<Reaction> _segments;
        private int _preConditionBoundary;

        internal PipelineBuilder(PlanBuildContext context)
        {
            Context = context;
        }

        /// <inheritdoc />
        void IReactionEmitter.AddStep(Reaction step) => Steps.Add(step);

        /// <inheritdoc />
        PlanBuildContext IReactionEmitter.BuildContext => Context;

        /// <summary>Dispatches a custom browser event by name.</summary>
        /// <param name="eventName">The event name. Listeners registered with <c>t.CustomEvent("name", ...)</c> will fire.</param>
        /// <returns>This builder for chaining.</returns>
        public PipelineBuilder<TModel> Dispatch(string eventName)
        {
            Steps.Add(Reaction.Dispatch(eventName));
            return this;
        }

        /// <summary>Dispatches a custom browser event with a typed payload.</summary>
        /// <typeparam name="TPayload">The payload type.</typeparam>
        /// <param name="eventName">The event name.</param>
        /// <param name="payload">The data to send with the event.</param>
        /// <returns>This builder for chaining.</returns>
        public PipelineBuilder<TModel> Dispatch<TPayload>(string eventName, TPayload payload)
        {
            Steps.Add(Reaction.Dispatch(eventName, ValueProducer.LiteralRaw(payload, Shape.FromClrType(typeof(TPayload)))));
            return this;
        }

        /// <summary>Dispatches a custom browser event with a source-backed payload whose fields are resolved at runtime.</summary>
        /// <remarks>
        /// Each field on <typeparamref name="TPayload"/> can come from a live component value,
        /// URL parameter, plugin read, or a compile-time literal. The listener consumes the
        /// payload via <c>t.CustomEvent&lt;TPayload&gt;(name, (payload, p) =&gt; ...)</c>.
        /// </remarks>
        /// <typeparam name="TPayload">The payload type matching the <c>CustomEvent&lt;TPayload&gt;</c> listener.</typeparam>
        /// <param name="eventName">The event name. Listeners registered with <c>t.CustomEvent("name", ...)</c> will fire.</param>
        /// <param name="configure">Populates payload fields via <see cref="DispatchPayloadBuilder{TPayload, TModel}"/>.</param>
        /// <returns>This builder for chaining.</returns>
        public PipelineBuilder<TModel> Dispatch<TPayload>(
            string eventName,
            Action<DispatchPayloadBuilder<TPayload, TModel>> configure)
            where TPayload : class
        {
            var builder = new DispatchPayloadBuilder<TPayload, TModel>();
            configure(builder);
            Steps.Add(Reaction.Dispatch(eventName, builder.Build()));
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
            var elementId = IdGenerator.For<TModel>(expr);
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
            var elementId = IdGenerator.For<TOtherModel>(expr);
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

        /// <summary>References an app-level component (e.g. Toast, Confirm) by its default ID.</summary>
        /// <typeparam name="TComponent">The app-level component type.</typeparam>
        /// <returns>A typed component reference.</returns>
        public ComponentRef<TComponent, TModel> Component<TComponent>()
            where TComponent : IAppLevelComponent, new()
        {
            var comp = new TComponent();
            return new ComponentRef<TComponent, TModel>(comp.DefaultId, this);
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
            Context.EnsurePluginMethod(pluginName, member, returns: PlanModel.Shape.FromClrType(typeof(T)));
            return new PluginReadBuilder<T, TModel>(pluginName, member);
        }

        /// <summary>Calls a plugin method that does not return a value. Chain <c>.Arg()</c> then <c>.Fire()</c>.</summary>
        /// <param name="pluginName">The registered plugin name.</param>
        /// <param name="member">The method name on the plugin.</param>
        /// <returns>A builder for adding arguments and firing the call.</returns>
        public PluginCallBuilder<TModel> Plugin(string pluginName, string member)
        {
            if (string.IsNullOrWhiteSpace(pluginName)) throw new System.ArgumentException("Plugin name required.", nameof(pluginName));
            if (string.IsNullOrWhiteSpace(member)) throw new System.ArgumentException("Member name required.", nameof(member));
            Context.EnsurePluginMethod(pluginName, member);
            return new PluginCallBuilder<TModel>(pluginName, member, this);
        }

        /// <summary>Displays accumulated validation errors in the specified container.</summary>
        /// <param name="formId">The DOM element ID of the validation error container.</param>
        /// <returns>This builder for chaining.</returns>
        public PipelineBuilder<TModel> ValidationErrors(string formId)
        {
            Steps.Add(Reaction.ShowValidationErrors(formId));
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
            Steps.Add(Reaction.Inject(elementId, responseBody));
            return this;
        }

        internal void SetConditionalBranches(List<BranchCase> branches)
        {
            _preConditionBoundary = Steps.Count;
            ConditionalBranches = branches;
        }

        internal void FlushSegment()
        {
            _segments ??= new List<Reaction>();

            if (_mode == PipelineMode.Http && _httpBuilder != null)
                FlushHttpRequest();
            else if (_mode == PipelineMode.Parallel && _parallelBuilder != null)
                FlushParallelRequest();
            else
                FlushCommandsAroundCondition();

            var hasUnflushedBranch = ConditionalBranches != null && ConditionalBranches.Count > 0;
            if (hasUnflushedBranch)
            {
                _segments.Add(Reaction.Branch(ConditionalBranches));
                ConditionalBranches = null;
            }

            _mode = PipelineMode.Sequential;
        }

        private void FlushHttpRequest()
        {
            var request = _httpBuilder.BuildRequest();
            var hasWhileLoadingCommands = Steps.Count > 0;
            if (hasWhileLoadingCommands)
                request.Before = new List<Reaction>(Steps);
            _segments.Add(Reaction.Request(request));
            Steps.Clear();
            _httpBuilder = null;
        }

        private void FlushParallelRequest()
        {
            var hasPendingCommands = Steps.Count > 0;
            _segments.Add(_parallelBuilder.BuildReaction(
                hasPendingCommands ? new List<Reaction>(Steps) : null));
            Steps.Clear();
            _parallelBuilder = null;
        }

        private void FlushCommandsAroundCondition()
        {
            var hasPreConditionCommands = _preConditionBoundary > 0;
            var hasPostConditionCommands = Steps.Count > _preConditionBoundary;
            var hasPendingBranch = ConditionalBranches != null && ConditionalBranches.Count > 0;

            if (hasPreConditionCommands)
                _segments.Add(Reaction.Sequence(
                    new List<Reaction>(Steps.GetRange(0, _preConditionBoundary))));

            if (hasPendingBranch)
            {
                _segments.Add(Reaction.Branch(ConditionalBranches));
                ConditionalBranches = null;
            }

            if (hasPostConditionCommands)
                _segments.Add(Reaction.Sequence(
                    new List<Reaction>(Steps.GetRange(
                        _preConditionBoundary, Steps.Count - _preConditionBoundary))));

            Steps.Clear();
            _preConditionBoundary = 0;
        }

        internal Reaction BuildReaction()
        {
            var reactions = BuildReactions();
            if (reactions.Count > 1)
                throw new InvalidOperationException(
                    $"BuildReaction() requires exactly one reaction segment but found {reactions.Count}.");
            return reactions[0];
        }

        internal List<Reaction> BuildReactions()
        {
            var isSingleSegmentPipeline = _segments == null || _segments.Count == 0;
            if (isSingleSegmentPipeline)
                return new List<Reaction> { BuildSingleReaction() };

            FlushSegment();
            return _segments;
        }

        private Reaction BuildSingleReaction()
        {
            var pendingCommands = Steps.Count > 0 ? Steps : null;
            return _mode switch
            {
                PipelineMode.Parallel => _parallelBuilder!.BuildReaction(pendingCommands),
                PipelineMode.Http => BuildHttpReaction(),
                PipelineMode.Conditional => BuildConditionalReaction(),
                _ => Reaction.Sequence(Steps),
            };
        }

        private Reaction BuildConditionalReaction()
        {
            var branch = Reaction.Branch(ConditionalBranches ?? new List<BranchCase>());
            if (Steps.Count == 0)
                return branch;

            var hasPreConditionCommands = _preConditionBoundary > 0;
            var hasPostConditionCommands = Steps.Count > _preConditionBoundary;

            var reactions = new List<Reaction>();

            if (hasPreConditionCommands)
                reactions.Add(Reaction.Sequence(
                    new List<Reaction>(Steps.GetRange(0, _preConditionBoundary))));

            reactions.Add(branch);

            if (hasPostConditionCommands)
                reactions.Add(Reaction.Sequence(
                    new List<Reaction>(Steps.GetRange(
                        _preConditionBoundary, Steps.Count - _preConditionBoundary))));

            return Reaction.Sequence(reactions);
        }

        private Reaction BuildHttpReaction()
        {
            var request = _httpBuilder!.BuildRequest();
            var requestReaction = Reaction.Request(request);
            if (Steps.Count > 0)
            {
                var all = new List<Reaction>(Steps) { requestReaction };
                return Reaction.Sequence(all);
            }
            return requestReaction;
        }

        internal void SetHttpMode(HttpRequestBuilder<TModel> builder)
        {
            _mode = PipelineMode.Http;
            _httpBuilder = builder;
        }

        internal void SetParallelMode(ParallelBuilder<TModel> builder)
        {
            _mode = PipelineMode.Parallel;
            _parallelBuilder = builder;
        }

        internal void SetConditionalMode()
        {
            _mode = PipelineMode.Conditional;
        }
    }
}
