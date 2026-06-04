using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    /// <summary>
    /// Configures request branches that start concurrently in the Reactive Plan.
    /// </summary>
    /// <remarks>
    /// Created by <c>p.Parallel(...)</c>. Each branch is an HTTP request, and
    /// the optional all-settled reaction runs after every branch has completed
    /// or failed.
    /// </remarks>
    /// <typeparam name="TModel">The view model that owns model-bound component IDs.</typeparam>
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

        /// <summary>
        /// Runs a reaction after every parallel request branch has settled.
        /// </summary>
        /// <param name="pipeline">Builds the all-settled reaction graph.</param>
        public ParallelBuilder<TModel> OnAllSettled(Action<PipelineBuilder<TModel>> pipeline)
        {
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));

            var reactionPipeline = new PipelineBuilder<TModel>(_context);
            pipeline(reactionPipeline);
            var reaction = reactionPipeline.BuildReaction();
            _draft.RunWhenAllSettled(reaction);
            return this;
        }

        internal ReactionGraph BuildReaction()
        {
            return _draft.ToReaction();
        }
    }
}
