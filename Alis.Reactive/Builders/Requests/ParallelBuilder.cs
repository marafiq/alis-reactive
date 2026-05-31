using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    /// <summary>Builds a set of HTTP requests that execute concurrently.</summary>
    public class ParallelBuilder<TModel> where TModel : class
    {
        private readonly PlanBuildContext _context;
        private readonly ParallelDraft _draft = new ParallelDraft();

        internal ParallelBuilder(PlanBuildContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        internal void AddBranch(Action<HttpRequestBuilder<TModel>> request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var builder = new HttpRequestBuilder<TModel>(_context);
            request(builder);
            _draft.AddBranch(builder.BuildRequest());
        }

        /// <summary>Executes a reaction graph after all parallel requests complete.</summary>
        public ParallelBuilder<TModel> OnAllSettled(Action<PipelineBuilder<TModel>> pipeline)
        {
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));

            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            var reaction = pb.BuildReaction();
            _draft.RunWhenAllSettled(reaction);
            return this;
        }

        internal ReactionGraph BuildReaction()
        {
            return _draft.ToReaction();
        }
    }
}
