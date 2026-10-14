using System;
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
    /// <typeparam name="TModel">View model that owns model-bound component IDs.</typeparam>
    public class ParallelBuilder<TModel> where TModel : class
    {
        private readonly PlanBuildContext _context;

        internal ParallelBuilder(PlanBuildContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        internal void AddBranch(Action<HttpRequestBuilder<TModel>> request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var builder = new HttpRequestBuilder<TModel>(_context);
            request(builder);
            Step.Add(builder.Step);
        }

        /// <summary>The requests this builder configures, written where <c>Parallel</c> is.</summary>
        internal ParallelStep Step { get; } = new ParallelStep();

        /// <summary>
        /// Runs a reaction after every parallel request branch has settled.
        /// </summary>
        /// <param name="pipeline">Builds the all-settled reaction graph.</param>
        public ParallelBuilder<TModel> OnAllSettled(Action<PipelineBuilder<TModel>> pipeline)
        {
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));

            Step.OnAllSettled(PipelineBuilder<TModel>.Build(_context, pipeline));
            return this;
        }
    }
}
