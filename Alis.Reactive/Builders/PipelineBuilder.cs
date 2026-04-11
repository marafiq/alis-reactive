using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Requests;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
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

        internal PipelineBuilder(PlanBuildContext context)
        {
            Context = context;
        }

        /// <inheritdoc />
        void IReactionEmitter.AddStep(Reaction step) => Steps.Add(step);

        /// <inheritdoc />
        PlanBuildContext IReactionEmitter.BuildContext => Context;

        public PipelineBuilder<TModel> Dispatch(string eventName)
        {
            Steps.Add(Reaction.Dispatch(eventName));
            return this;
        }

        public PipelineBuilder<TModel> Dispatch<TPayload>(string eventName, TPayload payload)
        {
            Steps.Add(Reaction.Dispatch(eventName, ValueProducer.LiteralRaw(payload, Shape.FromClrType(typeof(TPayload)))));
            return this;
        }

        public ElementBuilder<TModel> Element(string elementId)
        {
            return new ElementBuilder<TModel>(this, elementId);
        }

        public ComponentRef<TComponent, TModel> Component<TComponent>(
            Expression<Func<TModel, object>> expr)
            where TComponent : IComponent, new()
        {
            var elementId = IdGenerator.For<TModel>(expr);
            return new ComponentRef<TComponent, TModel>(elementId, this);
        }

        public ComponentRef<TComponent, TModel> Component<TComponent, TOtherModel>(
            Expression<Func<TOtherModel, object>> expr)
            where TComponent : IComponent, new()
            where TOtherModel : class
        {
            var elementId = IdGenerator.For<TOtherModel>(expr);
            return new ComponentRef<TComponent, TModel>(elementId, this);
        }

        public ComponentRef<TComponent, TModel> Component<TComponent>(string refId)
            where TComponent : IComponent, new()
        {
            return new ComponentRef<TComponent, TModel>(refId, this);
        }

        public ComponentRef<TComponent, TModel> Component<TComponent>()
            where TComponent : IAppLevelComponent, new()
        {
            var comp = new TComponent();
            return new ComponentRef<TComponent, TModel>(comp.DefaultId, this);
        }

        /// <summary>
        /// Reads a query parameter from the browser's current URL as a string.
        /// </summary>
        public Conditions.TypedUrlSource<string> FromUrl(string paramName)
        {
            return new Conditions.TypedUrlSource<string>(paramName);
        }

        /// <summary>
        /// Reads a query parameter with typed shape conversion.
        /// Use <c>FromUrl&lt;int&gt;("page")</c> for numeric comparison.
        /// </summary>
        public Conditions.TypedUrlSource<T> FromUrl<T>(string paramName)
        {
            return new Conditions.TypedUrlSource<T>(paramName);
        }

        public PipelineBuilder<TModel> ValidationErrors(string formId)
        {
            Steps.Add(Reaction.ShowValidationErrors(formId));
            return this;
        }

        public PipelineBuilder<TModel> Into(string elementId)
        {
            // Register the inject target in the plan — every component reference
            // must be in the plan. No fallbacks in the runtime.
            Context.EnsureElement(elementId);
            Steps.Add(Reaction.Inject(elementId, ValueProducer.Read(PayloadSource.Success(), "responseBody")));
            return this;
        }

        internal void SetConditionalBranches(List<BranchCase> branches)
        {
            ConditionalBranches = branches;
        }

        internal void FlushSegment()
        {
            _segments ??= new List<Reaction>();

            if (_mode == PipelineMode.Http && _httpBuilder != null)
            {
                var request = _httpBuilder.BuildRequest();
                if (Steps.Count > 0)
                    request.Before = new List<Reaction>(Steps);
                _segments.Add(Reaction.Request(request));
                Steps.Clear();
                _httpBuilder = null;
            }
            else if (_mode == PipelineMode.Parallel && _parallelBuilder != null)
            {
                _segments.Add(_parallelBuilder.BuildReaction(
                    Steps.Count > 0 ? new List<Reaction>(Steps) : null));
                Steps.Clear();
                _parallelBuilder = null;
            }
            else
            {
                if (Steps.Count > 0)
                {
                    _segments.Add(Reaction.Sequence(new List<Reaction>(Steps)));
                    Steps.Clear();
                }
            }

            if (ConditionalBranches != null && ConditionalBranches.Count > 0)
            {
                _segments.Add(Reaction.Branch(ConditionalBranches));
                ConditionalBranches = null;
            }

            _mode = PipelineMode.Sequential;
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
            if (_segments == null || _segments.Count == 0)
                return new List<Reaction> { BuildSingleReaction() };

            FlushSegment();
            return _segments;
        }

        private Reaction BuildSingleReaction()
        {
            return _mode switch
            {
                PipelineMode.Parallel => _parallelBuilder!.BuildReaction(
                    Steps.Count > 0 ? Steps : null),
                PipelineMode.Http => BuildHttpReaction(),
                PipelineMode.Conditional => BuildConditionalReaction(),
                _ => Reaction.Sequence(Steps),
            };
        }

        private Reaction BuildConditionalReaction()
        {
            var branch = Reaction.Branch(ConditionalBranches ?? new List<BranchCase>());
            if (Steps.Count > 0)
            {
                var all = new List<Reaction>(Steps) { branch };
                return Reaction.Sequence(all);
            }
            return branch;
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
