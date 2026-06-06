using System;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    /// <summary>
    /// Configures success routes, error routes, and the optional follow-up
    /// request for one HTTP request.
    /// </summary>
    /// <remarks>
    /// Created by <c>request.Response(r =&gt; ...)</c>.
    /// Exact error-status routes are matched before any-status error routes.
    /// </remarks>
    /// <typeparam name="TModel">View model that owns model-bound component IDs.</typeparam>
    public class ResponseBuilder<TModel> where TModel : class
    {
        private readonly PlanBuildContext _context;
        private readonly ResponseRoutingDraft _draft = new ResponseRoutingDraft();

        internal ResponseBuilder(PlanBuildContext context)
        {
            _context = context;
        }

        internal ResponseRoutingDraft Draft => _draft;

        /// <summary>Adds a route for any successful 2xx HTTP response.</summary>
        /// <param name="pipeline">Builds the reaction graph to execute on success.</param>
        public ResponseBuilder<TModel> OnSuccess(Action<PipelineBuilder<TModel>> pipeline)
        {
            var reactionPipeline = new PipelineBuilder<TModel>(_context);
            pipeline(reactionPipeline);
            _draft.AddSuccessRoute(reactionPipeline.BuildReaction());
            return this;
        }

        /// <summary>
        /// Adds a route for any successful 2xx HTTP response with typed
        /// response-body access.
        /// </summary>
        /// <typeparam name="TResponse">Response body type exposed to downstream value sources.</typeparam>
        /// <param name="pipeline">Builds the reaction graph using the success body scope.</param>
        public ResponseBuilder<TModel> OnSuccess<TResponse>(
            Action<ResponseBody<TResponse>, PipelineBuilder<TModel>> pipeline)
            where TResponse : class
        {
            var reactionPipeline = new PipelineBuilder<TModel>(_context);
            pipeline(
                new ResponseBody<TResponse>(PayloadSource.Success(PayloadContract.ForPayload(typeof(TResponse)))),
                reactionPipeline);
            _draft.AddSuccessRoute(reactionPipeline.BuildReaction());
            return this;
        }

        /// <summary>Adds a route for any non-2xx response or response-unavailable failure.</summary>
        /// <param name="pipeline">Builds the reaction graph to execute for the error route.</param>
        public ResponseBuilder<TModel> OnError(Action<PipelineBuilder<TModel>> pipeline)
        {
            var reactionPipeline = new PipelineBuilder<TModel>(_context);
            pipeline(reactionPipeline);
            _draft.AddErrorRoute(reactionPipeline.BuildReaction());
            return this;
        }

        /// <summary>Adds a route for a non-2xx response with a specific status code.</summary>
        /// <param name="statusCode">HTTP status code to match.</param>
        /// <param name="pipeline">Builds the reaction graph to execute for the matching status code.</param>
        public ResponseBuilder<TModel> OnError(int statusCode, Action<PipelineBuilder<TModel>> pipeline)
        {
            var reactionPipeline = new PipelineBuilder<TModel>(_context);
            pipeline(reactionPipeline);
            _draft.AddErrorRoute(statusCode, reactionPipeline.BuildReaction());
            return this;
        }

        /// <summary>
        /// Adds a route for any error outcome with typed error-body access when
        /// a response body is available.
        /// </summary>
        /// <typeparam name="TError">Error body type exposed to downstream value sources.</typeparam>
        /// <param name="pipeline">Builds the reaction graph using the error body scope.</param>
        public ResponseBuilder<TModel> OnError<TError>(
            Action<ResponseBody<TError>, PipelineBuilder<TModel>> pipeline)
            where TError : class
        {
            var reactionPipeline = new PipelineBuilder<TModel>(_context);
            pipeline(
                new ResponseBody<TError>(PayloadSource.Error(PayloadContract.ForPayload(typeof(TError)))),
                reactionPipeline);
            _draft.AddErrorRoute(reactionPipeline.BuildReaction());
            return this;
        }

        /// <summary>
        /// Adds a route for a non-2xx response with a specific status code and
        /// typed error-body access.
        /// </summary>
        /// <typeparam name="TError">Error body type exposed to downstream value sources.</typeparam>
        /// <param name="statusCode">HTTP status code to match.</param>
        /// <param name="pipeline">Builds the reaction graph using the error body scope.</param>
        public ResponseBuilder<TModel> OnError<TError>(int statusCode,
            Action<ResponseBody<TError>, PipelineBuilder<TModel>> pipeline)
            where TError : class
        {
            var reactionPipeline = new PipelineBuilder<TModel>(_context);
            pipeline(
                new ResponseBody<TError>(PayloadSource.Error(PayloadContract.ForPayload(typeof(TError)))),
                reactionPipeline);
            _draft.AddErrorRoute(statusCode, reactionPipeline.BuildReaction());
            return this;
        }

        /// <summary>
        /// Adds one follow-up HTTP request that runs only after the current
        /// request succeeds.
        /// </summary>
        /// <param name="request">Builds the follow-up request.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this response already has a follow-up request.
        /// </exception>
        public ResponseBuilder<TModel> Chained(Action<HttpRequestBuilder<TModel>> request)
        {
            var chainedBuilder = new HttpRequestBuilder<TModel>(_context);
            request(chainedBuilder);
            _draft.ContinueWith(chainedBuilder.BuildRequest());
            return this;
        }
    }
}
