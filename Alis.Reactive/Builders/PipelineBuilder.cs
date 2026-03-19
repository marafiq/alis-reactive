using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Requests;
using Alis.Reactive.Descriptors.Commands;
using Alis.Reactive.Descriptors.Reactions;

namespace Alis.Reactive.Builders
{
    public partial class PipelineBuilder<TModel> where TModel : class
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
        /// Adds a command to the pipeline. Used by vendor-specific projects
        /// (Fusion, Native) to emit their own command descriptors.
        /// </summary>
        public void AddCommand(Command command)
        {
            Commands.Add(command);
        }

        public PipelineBuilder<TModel> Dispatch(string eventName)
        {
            Commands.Add(new DispatchCommand(eventName));
            return this;
        }

        public PipelineBuilder<TModel> Dispatch<TPayload>(string eventName, TPayload payload)
        {
            Commands.Add(new DispatchCommand(eventName, payload));
            return this;
        }

        public ElementBuilder<TModel> Element(string elementId)
        {
            return new ElementBuilder<TModel>(this, elementId);
        }

        // ── Component<T>() — 3 overloads ──

        /// <summary>
        /// Resolve component by model expression (input components bound to model).
        /// Target ID uses underscore separator matching Html.IdFor() convention:
        /// m => m.Address.City → target "Address_City".
        /// </summary>
        public ComponentRef<TComponent, TModel> Component<TComponent>(
            Expression<Func<TModel, object?>> expr)
            where TComponent : IComponent, new()
        {
            var elementId = IdGenerator.For<TModel>(expr);
            return new ComponentRef<TComponent, TModel>(elementId, this);
        }

        /// <summary>Resolve component by string ref (non-input components by ID).</summary>
        public ComponentRef<TComponent, TModel> Component<TComponent>(string refId)
            where TComponent : IComponent, new()
        {
            return new ComponentRef<TComponent, TModel>(refId, this);
        }

        /// <summary>Resolve app-level component by its default ID (e.g., FusionConfirm).</summary>
        public ComponentRef<TComponent, TModel> Component<TComponent>()
            where TComponent : IAppLevelComponent, new()
        {
            var comp = new TComponent();
            return new ComponentRef<TComponent, TModel>(comp.DefaultId, this);
        }

        /// <summary>
        /// Adds a validation-errors command — displays server-side validation errors
        /// returned in the 400 response body at the correct form fields.
        /// </summary>
        public PipelineBuilder<TModel> ValidationErrors(string formId)
        {
            Commands.Add(new ValidationErrorsCommand(formId));
            return this;
        }

        /// <summary>
        /// Injects the HTTP response body as innerHTML of the target element.
        /// Used for loading partial views: p.Get("/url").Response(r => r.OnSuccess(s => s.Into("container")))
        /// </summary>
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

            // Flush any accumulated commands as a sequential segment
            if (Commands.Count > 0)
            {
                _segments.Add(new SequentialReaction(new List<Command>(Commands)));
                Commands.Clear();
            }

            // Flush current conditional block
            if (ConditionalBranches != null && ConditionalBranches.Count > 0)
            {
                _segments.Add(new ConditionalReaction(null, ConditionalBranches.ToArray()));
                ConditionalBranches = null;
            }
        }

        /// <summary>
        /// Returns a single reaction for simple pipelines (one segment),
        /// or the first reaction for backwards compatibility.
        /// Prefer BuildReactions() for multi-segment pipelines.
        /// </summary>
        public Reaction BuildReaction()
        {
            var reactions = BuildReactions();
            return reactions.Count == 1
                ? reactions[0]
                : reactions[0]; // caller should use BuildReactions() for multi-segment
        }

        /// <summary>
        /// Builds all reactions from the pipeline. A single When() block produces
        /// one reaction. Multiple When() blocks produce multiple reactions.
        /// Commands between/around conditions produce sequential reactions.
        /// </summary>
        public List<Reaction> BuildReactions()
        {
            // If no segments were flushed, build a single reaction (common case)
            if (_segments == null || _segments.Count == 0)
            {
                return new List<Reaction> { BuildSingleReaction() };
            }

            // Flush any trailing commands/branches after the last When()
            if (Commands.Count > 0)
            {
                _segments.Add(new SequentialReaction(new List<Command>(Commands)));
                Commands.Clear();
            }
            if (ConditionalBranches != null && ConditionalBranches.Count > 0)
            {
                _segments.Add(new ConditionalReaction(null, ConditionalBranches.ToArray()));
                ConditionalBranches = null;
            }

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
