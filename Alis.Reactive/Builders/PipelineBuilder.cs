using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Requests;
using Alis.Reactive.Descriptors.Commands;
using Alis.Reactive.Descriptors.Reactions;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Builds the sequence of commands that execute when a trigger fires: element mutations,
    /// event dispatches, HTTP calls, component interactions, and conditional logic.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Received as the <c>p</c> parameter inside trigger callbacks:
    /// <c>t.DomReady(p =&gt; { p.Element("id").AddClass("x"); p.Dispatch("ready"); })</c>.
    /// </para>
    /// <para>
    /// Commands execute in declaration order. Conditions (<c>When</c>/<c>Then</c>/<c>Else</c>)
    /// and HTTP calls (<c>Get</c>/<c>Post</c>) create branching points that produce
    /// separate reaction segments.
    /// </para>
    /// </remarks>
    /// <typeparam name="TModel">The view model type, providing compile-time expression paths.</typeparam>
    public partial class PipelineBuilder<TModel> : ICommandEmitter where TModel : class
    {
        private enum PipelineMode { Sequential, Http, Parallel, Conditional }

        internal List<Command> Commands { get; } = new List<Command>();
        internal List<Branch>? ConditionalBranches { get; private set; }
        private HttpRequestBuilder<TModel>? _httpBuilder;
        private ParallelBuilder<TModel>? _parallelBuilder;
        private PipelineMode _mode = PipelineMode.Sequential;

        /// <summary>
        /// Completed reaction segments. When a new When() is called after a previous
        /// When().Then().Else() block, the current segment (commands + branches) is
        /// flushed here so both conditionals produce independent reactions.
        /// </summary>
        private List<Reaction>? _segments;

        /// <summary>
        /// Adds a command to the pipeline. Vendor extensions accept
        /// <see cref="ICommandEmitter"/>, not <see cref="PipelineBuilder{TModel}"/> directly.
        /// </summary>
        void ICommandEmitter.AddCommand(Command command)
        {
            Commands.Add(command);
        }

        /// <summary>
        /// Fires a custom event in the browser that other triggers can listen for.
        /// </summary>
        /// <param name="eventName">The event name (e.g. <c>"order-submitted"</c>).</param>
        /// <returns>This builder for chaining additional commands.</returns>
        public PipelineBuilder<TModel> Dispatch(string eventName)
        {
            Commands.Add(new DispatchCommand(eventName));
            return this;
        }

        /// <summary>
        /// Fires a custom event with a payload object in the browser.
        /// </summary>
        /// <typeparam name="TPayload">The payload type, serialized as the event's detail data.</typeparam>
        /// <param name="eventName">The event name.</param>
        /// <param name="payload">The data to attach to the event.</param>
        /// <returns>This builder for chaining additional commands.</returns>
        public PipelineBuilder<TModel> Dispatch<TPayload>(string eventName, TPayload payload)
        {
            Commands.Add(new DispatchCommand(eventName, payload));
            return this;
        }

        /// <summary>
        /// Targets a DOM element by its ID for mutations (CSS classes, text, visibility).
        /// </summary>
        /// <remarks>
        /// Use <c>Element()</c> for non-input display elements. For input components bound to
        /// a model property, use <see cref="Component{TComponent}(Expression{Func{TModel, object}})"/> instead.
        /// </remarks>
        /// <param name="elementId">The HTML element ID.</param>
        /// <returns>An element builder for chaining mutations like <c>AddClass</c>, <c>SetText</c>, <c>Show</c>.</returns>
        public ElementBuilder<TModel> Element(string elementId)
        {
            return new ElementBuilder<TModel>(this, elementId);
        }

        // ── Component<T>() — 3 overloads ──

        /// <summary>
        /// Targets a component by model expression (input components bound to a model property).
        /// </summary>
        /// <typeparam name="TComponent">The component type.</typeparam>
        /// <param name="expr">The model property expression (e.g. <c>m =&gt; m.Address.City</c>).</param>
        /// <returns>A component reference for chaining mutations like <c>SetValue</c> or <c>Focus</c>.</returns>
        public ComponentRef<TComponent, TModel> Component<TComponent>(
            Expression<Func<TModel, object?>> expr)
            where TComponent : IComponent, new()
        {
            var elementId = IdGenerator.For<TModel>(expr);
            return new ComponentRef<TComponent, TModel>(elementId, this);
        }

        /// <summary>
        /// Targets a component from a different model (cross-plan component reference).
        /// </summary>
        /// <remarks>
        /// Uses <see cref="IdGenerator.For{TModel}(Expression{Func{TModel, object}})"/> with
        /// <typeparamref name="TOtherModel"/> to produce the correct element ID.
        /// Example: <c>p.Component&lt;NativeHiddenField, Step2Model&gt;(m =&gt; m.Diagnosis).SetValue(...)</c>.
        /// </remarks>
        /// <typeparam name="TComponent">The component type.</typeparam>
        /// <typeparam name="TOtherModel">The other view's model type.</typeparam>
        /// <param name="expr">The model property expression on the other model.</param>
        /// <returns>A component reference for chaining mutations.</returns>
        public ComponentRef<TComponent, TModel> Component<TComponent, TOtherModel>(
            Expression<Func<TOtherModel, object?>> expr)
            where TComponent : IComponent, new()
            where TOtherModel : class
        {
            var elementId = IdGenerator.For<TOtherModel>(expr);
            return new ComponentRef<TComponent, TModel>(elementId, this);
        }

        /// <summary>Targets a component by its string ID (non-input components).</summary>
        /// <typeparam name="TComponent">The component type.</typeparam>
        /// <param name="refId">The HTML element ID of the component.</param>
        /// <returns>A component reference for chaining mutations.</returns>
        public ComponentRef<TComponent, TModel> Component<TComponent>(string refId)
            where TComponent : IComponent, new()
        {
            return new ComponentRef<TComponent, TModel>(refId, this);
        }

        /// <summary>Targets an app-level component by its default ID (e.g. <c>FusionConfirm</c>).</summary>
        /// <typeparam name="TComponent">The app-level component type.</typeparam>
        /// <returns>A component reference for chaining mutations.</returns>
        public ComponentRef<TComponent, TModel> Component<TComponent>()
            where TComponent : IAppLevelComponent, new()
        {
            var comp = new TComponent();
            return new ComponentRef<TComponent, TModel>(comp.DefaultId, this);
        }

        /// <summary>
        /// Displays server-side validation errors returned in the 400 response body
        /// at the correct form fields.
        /// </summary>
        /// <param name="formId">The form element ID to scope error display to.</param>
        /// <returns>This builder for chaining additional commands.</returns>
        public PipelineBuilder<TModel> ValidationErrors(string formId)
        {
            Commands.Add(new ValidationErrorsCommand(formId));
            return this;
        }

        /// <summary>
        /// Injects the HTTP response body as inner HTML of the target element.
        /// </summary>
        /// <remarks>
        /// Used for loading partial views:
        /// <c>p.Get("/url").Response(r =&gt; r.OnSuccess(s =&gt; s.Into("container")))</c>.
        /// </remarks>
        /// <param name="elementId">The HTML element ID to inject content into.</param>
        /// <returns>This builder for chaining additional commands.</returns>
        public PipelineBuilder<TModel> Into(string elementId)
        {
            Commands.Add(new IntoCommand(elementId));
            return this;
        }

        internal void SetConditionalBranches(List<Branch> branches)
        {
            ConditionalBranches = branches;
        }

        /// <summary>
        /// Flushes the current segment (accumulated commands + conditional branches)
        /// into _segments, then resets for the next segment. Called by When() when
        /// a previous conditional block already exists.
        /// </summary>
        internal void FlushSegment()
        {
            _segments ??= new List<Reaction>();

            if (_mode == PipelineMode.Http && _httpBuilder != null)
            {
                // HTTP mode: pre-HTTP commands belong inside the HttpReaction
                _segments.Add(new HttpReaction(
                    Commands.Count > 0 ? new List<Command>(Commands) : null,
                    _httpBuilder.BuildRequestDescriptor()));
                Commands.Clear();
                _httpBuilder = null;
            }
            else if (_mode == PipelineMode.Parallel && _parallelBuilder != null)
            {
                _segments.Add(_parallelBuilder.BuildReaction(
                    Commands.Count > 0 ? new List<Command>(Commands) : null));
                Commands.Clear();
                _parallelBuilder = null;
            }
            else
            {
                // Sequential/Conditional: flush commands as a standalone reaction
                if (Commands.Count > 0)
                {
                    _segments.Add(new SequentialReaction(new List<Command>(Commands)));
                    Commands.Clear();
                }
            }

            // Flush current conditional block
            if (ConditionalBranches != null && ConditionalBranches.Count > 0)
            {
                _segments.Add(new ConditionalReaction(null, ConditionalBranches.ToArray()));
                ConditionalBranches = null;
            }

            _mode = PipelineMode.Sequential;
        }

        /// <summary>
        /// Returns the single reaction for this pipeline.
        /// </summary>
        /// <remarks>
        /// Throws if the pipeline produced multiple segments. Callers that
        /// need multi-segment support must use <see cref="BuildReactions"/> instead.
        /// </remarks>
        /// <returns>The single reaction built from the pipeline commands.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the pipeline contains multiple reaction segments.</exception>
        internal Reaction BuildReaction()
        {
            var reactions = BuildReactions();
            if (reactions.Count > 1)
                throw new InvalidOperationException(
                    $"BuildReaction() requires exactly one reaction segment but found {reactions.Count}. " +
                    "Use BuildReactions() for pipelines with multiple When() blocks.");
            return reactions[0];
        }

        /// <summary>
        /// Builds all reactions from the pipeline. A single When() block produces
        /// one reaction. Multiple When() blocks produce multiple reactions.
        /// Commands between/around conditions produce sequential reactions.
        /// </summary>
        /// <returns>All reaction segments built from the pipeline commands.</returns>
        internal List<Reaction> BuildReactions()
        {
            // If no segments were flushed, build a single reaction (common case)
            if (_segments == null || _segments.Count == 0)
            {
                return new List<Reaction> { BuildSingleReaction() };
            }

            // Flush any trailing content (commands, branches, HTTP, parallel)
            FlushSegment();

            return _segments;
        }

        private Reaction BuildSingleReaction()
        {
            return _mode switch
            {
                PipelineMode.Parallel => _parallelBuilder!.BuildReaction(
                    Commands.Count > 0 ? Commands : null),
                PipelineMode.Http => new HttpReaction(
                    Commands.Count > 0 ? Commands : null,
                    _httpBuilder!.BuildRequestDescriptor()),
                PipelineMode.Conditional => new ConditionalReaction(
                    Commands.Count > 0 ? Commands : null,
                    ConditionalBranches!.ToArray()),
                _ => new SequentialReaction(Commands),
            };
        }
    }
}
